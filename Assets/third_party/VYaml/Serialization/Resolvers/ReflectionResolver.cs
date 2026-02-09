using System;
using System.Collections.Generic;
using System.Reflection;
using VYaml.Annotations;
using VYaml.Emitter;
using VYaml.Parser;

namespace VYaml.Serialization
{
    public class ReflectionResolver : IYamlFormatterResolver
    {
        public static readonly ReflectionResolver Instance = new();

        public IYamlFormatter<T>? GetFormatter<T>()
        {
            return ReflectionFormatter<T>.Instance;
        }
    }

    public class ReflectionFormatter<T> : IYamlFormatter<T>
    {
        public static readonly ReflectionFormatter<T> Instance = new();

        private readonly MemberAccessor<T>[] members;
        private readonly Dictionary<string, MemberAccessor<T>> customNameMembers;

        public ReflectionFormatter()
        {
            var memberList = new List<MemberAccessor<T>>();
            customNameMembers = new Dictionary<string, MemberAccessor<T>>();

            var type = typeof(T);
            
            // Properties
            foreach (var prop in type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                if (prop.GetCustomAttribute<YamlIgnoreAttribute>() != null) continue;
                if (!prop.CanRead || !prop.CanWrite) continue;
                if (prop.GetIndexParameters().Length > 0) continue;

                var accessor = CreateAccessor(prop);
                var attr = prop.GetCustomAttribute<YamlMemberAttribute>();
                if (attr != null && attr.Name != null)
                {
                    customNameMembers[attr.Name] = accessor;
                }
                else
                {
                    memberList.Add(accessor);
                }
            }

            // Fields
            foreach (var field in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (field.GetCustomAttribute<YamlIgnoreAttribute>() != null) continue;
                if (field.IsInitOnly || field.IsLiteral) continue;

                // Check if public or has SerializeField attribute
                bool isPublic = field.IsPublic;
                bool hasSerializeField = field.GetCustomAttribute<UnityEngine.SerializeField>() != null;

                if (!isPublic && !hasSerializeField) continue;

                var accessor = CreateAccessor(field);
                var attr = field.GetCustomAttribute<YamlMemberAttribute>();
                if (attr != null && attr.Name != null)
                {
                    customNameMembers[attr.Name] = accessor;
                }
                else
                {
                    memberList.Add(accessor);
                }
            }


            members = memberList.ToArray();
        }

        private static MemberAccessor<T> CreateAccessor(MemberInfo member)
        {
            Type memberType = member is PropertyInfo p ? p.PropertyType : ((FieldInfo)member).FieldType;
            var accessorType = typeof(MemberAccessor<,>).MakeGenericType(typeof(T), memberType);
            return (MemberAccessor<T>)Activator.CreateInstance(accessorType, member)!;
        }

        public void Serialize(ref Utf8YamlEmitter emitter, T value, YamlSerializationContext context)
        {
            if (value == null)
            {
                emitter.WriteNull();
                return;
            }

            emitter.BeginMapping();

            // Serialize members with custom names
            foreach (var kvp in customNameMembers)
            {
                var accessor = kvp.Value;
                emitter.WriteString(kvp.Key);
                accessor.Serialize(ref emitter, value, context);
            }

            // Serialize standard members
            foreach (var accessor in members)
            {
                var keyName = NamingConventionMutator.Mutate(accessor.Name, context.Options.NamingConvention);
                emitter.WriteString(keyName);
                accessor.Serialize(ref emitter, value, context);
            }

            emitter.EndMapping();
        }

        public T Deserialize(ref YamlParser parser, YamlDeserializationContext context)
        {
            if (parser.IsNullScalar())
            {
                parser.Read();
                return default!;
            }

            parser.ReadWithVerify(ParseEventType.MappingStart);

            var instance = Activator.CreateInstance<T>();

            while (!parser.End && parser.CurrentEventType != ParseEventType.MappingEnd)
            {
                var key = parser.GetScalarAsString();
                parser.Read(); // Consume key

                MemberAccessor<T>? targetMember = null;


                // Check custom names first
                if (customNameMembers.TryGetValue(key, out var member))
                {
                    targetMember = member;
                }
                else
                {
                    // Slow linear search for matching naming convention
                    foreach (var m in members)
                    {
                        var expectedName = NamingConventionMutator.Mutate(m.Name, context.Options.NamingConvention);
                        if (expectedName == key)
                        {
                            targetMember = m;
                            break;
                        }
                    }
                }

                if (targetMember != null)
                {
                    targetMember.Deserialize(ref parser, ref instance, context);
                }
                else
                {
                    parser.SkipCurrentNode();
                }
            }

            parser.ReadWithVerify(ParseEventType.MappingEnd);
            return instance;
        }
    }


    internal abstract class MemberAccessor<TTarget>
    {
        public string Name { get; }

        protected MemberAccessor(string name)
        {
            Name = name;
        }

        public abstract void Serialize(ref Utf8YamlEmitter emitter, TTarget target, YamlSerializationContext context);
        public abstract void Deserialize(ref YamlParser parser, ref TTarget target, YamlDeserializationContext context);
    }

    internal class MemberAccessor<TTarget, TMember> : MemberAccessor<TTarget>
    {
        private readonly PropertyInfo? property;
        private readonly FieldInfo? field;

        public MemberAccessor(MemberInfo member) : base(member.Name)
        {
            property = member as PropertyInfo;
            field = member as FieldInfo;
        }

        public override void Serialize(ref Utf8YamlEmitter emitter, TTarget target, YamlSerializationContext context)
        {
            TMember value;
            if (property != null) value = (TMember)property.GetValue(target)!;
            else value = (TMember)field!.GetValue(target)!;
            
            context.Serialize(ref emitter, value);
        }

        public override void Deserialize(ref YamlParser parser, ref TTarget target, YamlDeserializationContext context)
        {
            var value = context.DeserializeWithAlias<TMember>(ref parser);
            
            if (typeof(TTarget).IsValueType)
            {
                object boxed = target!;
                if (property != null) property.SetValue(boxed, value);
                else field!.SetValue(boxed, value);
                target = (TTarget)boxed;
            }
            else
            {
                if (property != null) property.SetValue(target, value);
                else field!.SetValue(target, value);
            }
        }
    }
}