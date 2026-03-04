/*
 * Copyright 2019 Google LLC
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     https://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using UnityEngine;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

/// <summary>
/// 只序列化公有字段，不序列化属性（模拟 Unity JsonUtility 的行为），
/// 避免 UnityEngine.Color.linear 等属性导致的自引用循环。
/// </summary>
public class FieldsOnlyContractResolver : DefaultContractResolver
{
  protected override List<MemberInfo> GetSerializableMembers(System.Type objectType)
  {
    // 只返回公有实例字段（与 JsonUtility 行为一致）
    return objectType.GetFields(BindingFlags.Public | BindingFlags.Instance)
        .Cast<MemberInfo>()
        .ToList();
  }
}

// Database of SOJOs (Small JSON Objects).
// This class is responsible for maintaining the database locally,
// but NOT responsible for networking it (see SojoSystem for that).
public class SojoDatabase
{
  private const string BUILTIN_SOJOS_RESOURCE_FILE = "BuiltinSojos";

  // SOJOs by ID.
  private Dictionary<string, Sojo> sojosById = new Dictionary<string, Sojo>();
  // Lazy cache of SOJOs by name. More than one SOJO can have the same name, so this
  // stores an arbitrary one.
  private Dictionary<string, Sojo> cacheByName = new Dictionary<string, Sojo>();
  // Names known NOT to exist (performance optimization in case some script code wants to
  // query for a name repeatedly even though it's not in the DB).
  private HashSet<string> namesKnownNotToExist = new HashSet<string>();

  public SojoDatabase() { }

  public void Reset()
  {
    sojosById.Clear();
    cacheByName.Clear();
    namesKnownNotToExist.Clear();
  }

  public void PutSojo(Sojo sojo)
  {
    // We must first delete it to keep our cache consistent.
    DeleteSojo(sojo.id);
    // Now add it.
    sojosById[sojo.id] = sojo;
    cacheByName[sojo.name] = sojo;
    namesKnownNotToExist.Remove(sojo.name);
  }

  public Sojo GetSojoById(string id)
  {
    Sojo sojo;
    return sojosById.TryGetValue(id, out sojo) ? sojo : null;
  }

  // Gets a SOJO with the given name. If there is more than one, returns an arbitrary one.
  public Sojo GetSojoByName(string name)
  {
    Sojo sojo;
    if (namesKnownNotToExist.Contains(name))
    {
      return null;
    }
    else if (cacheByName.TryGetValue(name, out sojo))
    {
      return sojo;
    }
    // Linear search.
    foreach (KeyValuePair<string, Sojo> pair in sojosById)
    {
      if (pair.Value.name == name)
      {
        // Found it. Cache it.
        cacheByName[name] = pair.Value;
        return pair.Value;
      }
    }
    // We now know that a Sojo by this name does not exist, so cache this
    // knowledge for performance in case we get asked again.
    namesKnownNotToExist.Add(name);
    return null;
  }

  public void DeleteSojo(string sojoId)
  {
    Sojo sojo;
    if (sojosById.TryGetValue(sojoId, out sojo))
    {
      sojosById.Remove(sojoId);
      Sojo cachedSojo;
      if (cacheByName.TryGetValue(sojo.name, out cachedSojo) && sojo == cachedSojo)
      {
        cacheByName.Remove(sojo.name);
      }
    }
  }

  // Returns a list of Sojo's of the given type. This is a linear-time operation that shouldn't
  // be called too often. Maybe once when populating a list is OK, but don't call every frame.
  public List<Sojo> GetAllSojosOfType(SojoType type)
  {
    List<Sojo> sojos = new List<Sojo>();
    foreach (Sojo sojo in sojosById.Values)
    {
      if (sojo.contentType == type)
      {
        sojos.Add(sojo);
      }
    }
    return sojos;
  }

  public List<Sojo> GetAllSojos()
  {
    return new List<Sojo>(sojosById.Values);
  }

  public Saved Save()
  {
    Saved saved = new Saved();
    saved.sojos = new Sojo.Saved[sojosById.Count];
    int i = 0;
    foreach (Sojo sojo in sojosById.Values)
    {
      saved.sojos[i++] = sojo.Save();
    }
    return saved;
  }

  public void Load(Saved database)
  {
    Reset();
    string builtinSojoJson = Resources.Load<TextAsset>(BUILTIN_SOJOS_RESOURCE_FILE).text;
    SojoDatabase.Saved builtIn = JsonConvert.DeserializeObject<SojoDatabase.Saved>(builtinSojoJson, Sojo.JsonSettings);
    foreach (Sojo.Saved saved in builtIn.sojos)
    {
      PutSojo(Sojo.Load(saved));
    }
    foreach (Sojo.Saved saved in database.sojos)
    {
      PutSojo(Sojo.Load(saved));
    }
  }


  [System.Serializable]
  public struct Saved
  {
    public Sojo.Saved[] sojos;
  }
}

public enum SojoType
{
  // DO NOT CHANGE the names of these enum values. They are used in serialized data.
  SoundEffect, // content存放的是SoundEffectContent序列化后的JSON
  ParticleEffect, // ParticleEffectContent序列化后的JSON
  Image, //{}
  ActorPrefab // SavedActorPrefab序列化后的JSON
}

public class Sojo
{
  // Unique ID (GUID)
  public readonly string id;
  // User-facing (display) name
  public readonly string name;
  // Sojo content type
  public readonly SojoType contentType;
  // Content（实际类型由 contentType 决定：SoundEffectContent / ParticleEffectContent / SavedActorPrefab / object）
  public readonly object content;

  /// <summary>
  /// contentType → C# 类型的映射，用于反序列化 content
  /// </summary>
  public static readonly Dictionary<string, System.Type> ContentTypeMap = new Dictionary<string, System.Type>
  {
    { "SoundEffect",    typeof(SoundEffectContent) },
    { "ParticleEffect", typeof(ParticleEffectContent) },
    { "ActorPrefab",    typeof(SavedActorPrefab) },
    // Image 的 content 为空对象
  };

  /// <summary>
  /// Newtonsoft.Json 序列化设置：只序列化字段（模拟 JsonUtility），
  /// 注册 SojoSavedConverter 处理 content 的动态类型。
  /// </summary>
  public static readonly JsonSerializerSettings JsonSettings = new JsonSerializerSettings
  {
    ContractResolver = new FieldsOnlyContractResolver(),
    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
    Converters = { new SojoSavedConverter() }
  };

  [System.Serializable]
  public struct Saved
  {
    public string id;
    public string name;
    public string contentType;
    public object content;
  }

  public Sojo(string id, string name, SojoType contentType, object content)
  {
    this.id = id;
    this.name = name;
    this.contentType = contentType;
    this.content = content;
  }

  public static Sojo Load(Saved saved)
  {
    return new Sojo(saved.id, saved.name, Util.ParseEnum<SojoType>(saved.contentType), saved.content);
  }

  public Saved Save()
  {
    Saved json = new Saved();
    json.id = id;
    json.name = name;
    json.content = content;
    json.contentType = contentType.ToString();
    return json;
  }

  public override string ToString()
  {
    return string.Format("SOJO id:{0}, name:{1}, type:{2}, content:{3}", id, name, contentType, content);
  }
}

/// <summary>
/// Sojo.Saved 的自定义 JsonConverter：
/// 序列化时将 content 作为嵌套 JSON 对象写入；
/// 反序列化时根据 contentType 字段将 content 还原为对应的 C# 类型。
/// </summary>
public class SojoSavedConverter : JsonConverter<Sojo.Saved>
{
  static readonly JsonSerializerSettings InnerSettings = new JsonSerializerSettings
  {
    ContractResolver = new FieldsOnlyContractResolver(),
    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
  };

  public override void WriteJson(JsonWriter writer, Sojo.Saved value, JsonSerializer serializer)
  {
    writer.WriteStartObject();

    writer.WritePropertyName("id");
    writer.WriteValue(value.id);

    writer.WritePropertyName("name");
    writer.WriteValue(value.name);

    writer.WritePropertyName("contentType");
    writer.WriteValue(value.contentType);

    // content 直接作为嵌套 JSON 对象写入
    writer.WritePropertyName("content");
    var innerSerializer = JsonSerializer.Create(InnerSettings);
    innerSerializer.Serialize(writer, value.content);

    writer.WriteEndObject();
  }

  public override Sojo.Saved ReadJson(JsonReader reader, System.Type objectType,
    Sojo.Saved existingValue, bool hasExistingValue, JsonSerializer serializer)
  {
    var jObj = JObject.Load(reader);

    var saved = new Sojo.Saved
    {
      id = jObj["id"]?.ToString(),
      name = jObj["name"]?.ToString(),
      contentType = jObj["contentType"]?.ToString(),
    };

    // 根据 contentType 反序列化 content
    var contentToken = jObj["content"];
    if (contentToken != null && !string.IsNullOrEmpty(saved.contentType))
    {
      var innerSerializer = JsonSerializer.Create(InnerSettings);

      // 兼容旧数据：如果 content 仍然是 JSON 字符串（未迁移的数据），先解析字符串
      if (contentToken.Type == JTokenType.String)
      {
        string contentJsonStr = contentToken.ToString();
        if (!string.IsNullOrWhiteSpace(contentJsonStr) && contentJsonStr != "{}")
        {
          if (Sojo.ContentTypeMap.TryGetValue(saved.contentType, out System.Type targetType))
          {
            saved.content = JsonConvert.DeserializeObject(contentJsonStr, targetType, InnerSettings);
          }
          else
          {
            saved.content = contentJsonStr;
          }
        }
        else
        {
          saved.content = new object();
        }
      }
      else if (contentToken.Type == JTokenType.Object)
      {
        // 新格式：content 是嵌套 JSON 对象
        if (Sojo.ContentTypeMap.TryGetValue(saved.contentType, out System.Type targetType))
        {
          saved.content = contentToken.ToObject(targetType, innerSerializer);
        }
        else
        {
          saved.content = new object();
        }
      }
      else
      {
        saved.content = new object();
      }
    }
    else
    {
      saved.content = new object();
    }

    return saved;
  }
}

/// <summary>
/// VYaml 的自定义 Formatter，处理 Sojo.Saved 的 content 字段的动态类型。
/// 序列化时根据 content 的实际类型写入 YAML mapping；
/// 反序列化时根据 contentType 字段将 YAML mapping 还原为对应的 C# 类型。
/// </summary>
public class SojoSavedYamlFormatter : VYaml.Serialization.IYamlFormatter<Sojo.Saved>
{
  public static readonly SojoSavedYamlFormatter Instance = new SojoSavedYamlFormatter();

  // 预分配 UTF-8 字节数组，替代 C# 11 的 u8 字符串字面量
  static readonly byte[] Key_id = System.Text.Encoding.UTF8.GetBytes("id");
  static readonly byte[] Key_name = System.Text.Encoding.UTF8.GetBytes("name");
  static readonly byte[] Key_contentType = System.Text.Encoding.UTF8.GetBytes("contentType");
  static readonly byte[] Key_content = System.Text.Encoding.UTF8.GetBytes("content");


  public void Serialize(ref VYaml.Emitter.Utf8YamlEmitter emitter, Sojo.Saved value,
    VYaml.Serialization.YamlSerializationContext context)
  {
    emitter.BeginMapping();

    // id
    emitter.WriteScalar(Key_id);
    context.Serialize(ref emitter, value.id);

    // name
    emitter.WriteScalar(Key_name);
    context.Serialize(ref emitter, value.name);

    // contentType
    emitter.WriteScalar(Key_contentType);
    context.Serialize(ref emitter, value.contentType);

    // content — 根据 contentType 序列化
    emitter.WriteScalar(Key_content);

    SerializeContent(ref emitter, value.contentType, value.content, context);

    emitter.EndMapping();
  }

  /// <summary>
  /// 根据 contentType 字符串显式调用对应的泛型 Serialize 方法。
  /// </summary>
  static void SerializeContent(ref VYaml.Emitter.Utf8YamlEmitter emitter, string contentType,
    object content, VYaml.Serialization.YamlSerializationContext context)
  {
    if (content == null)
    {
      emitter.WriteNull();
      return;
    }

    switch (contentType)
    {
      case "SoundEffect":
        context.Serialize(ref emitter, (SoundEffectContent)content);
        break;
      case "ParticleEffect":
        context.Serialize(ref emitter, (ParticleEffectContent)content);
        break;
      case "ActorPrefab":
        context.Serialize(ref emitter, (SavedActorPrefab)content);
        break;
      default:
        // Image 或未知类型，写空 mapping
        emitter.BeginMapping();
        emitter.EndMapping();
        break;
    }
  }


  public Sojo.Saved Deserialize(ref VYaml.Parser.YamlParser parser,
    VYaml.Serialization.YamlDeserializationContext context)
  {
    var saved = new Sojo.Saved();
    // 标记 content 是否已被解析为具体类型
    bool contentResolved = false;
    // 如果 content 出现在 contentType 之前，先读为 object（动态结构），后续再转换
    object rawContent = null;

    parser.ReadWithVerify(VYaml.Parser.ParseEventType.MappingStart);

    while (!parser.End && parser.CurrentEventType != VYaml.Parser.ParseEventType.MappingEnd)
    {
      var key = parser.ReadScalarAsString();

      switch (key)
      {
        case "id":
          saved.id = parser.ReadScalarAsString();
          break;
        case "name":
          saved.name = parser.ReadScalarAsString();
          break;
        case "contentType":
          saved.contentType = parser.ReadScalarAsString();
          break;
        case "content":
          if (!string.IsNullOrEmpty(saved.contentType))
          {
            // contentType 已知，直接反序列化为具体类型
            saved.content = DeserializeContent(saved.contentType, ref parser, context);
            contentResolved = true;
          }
          else
          {
            // contentType 未知（字段顺序问题），先用 PrimitiveObjectFormatter 读为动态结构
            rawContent = context.DeserializeWithAlias<object>(ref parser);
          }
          break;
        default:
          // 跳过未知字段
          parser.SkipCurrentNode();
          break;
      }
    }

    parser.ReadWithVerify(VYaml.Parser.ParseEventType.MappingEnd);

    // 如果 content 在 contentType 之前出现（rawContent 不为 null），需要后处理
    if (!contentResolved && rawContent != null)
    {
      if (!string.IsNullOrEmpty(saved.contentType) &&
          Sojo.ContentTypeMap.TryGetValue(saved.contentType, out System.Type targetType))
      {
        // 将动态结构（Dictionary<object?,object?>）通过 Newtonsoft.Json 转为具体类型
        // 先序列化为 JSON，再反序列化为目标类型
        string tempJson = JsonConvert.SerializeObject(rawContent);
        saved.content = JsonConvert.DeserializeObject(tempJson, targetType,
          new JsonSerializerSettings
          {
            ContractResolver = new FieldsOnlyContractResolver(),
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
          });
      }
      else
      {
        saved.content = new object();
      }
    }
    else if (!contentResolved)
    {
      saved.content = new object();
    }

    return saved;
  }


  /// <summary>
  /// 根据 contentType 字符串显式调用对应的泛型 DeserializeWithAlias 方法。
  /// 不使用反射，直接 switch-case。
  /// </summary>
  static object DeserializeContent(string contentType, ref VYaml.Parser.YamlParser parser,
    VYaml.Serialization.YamlDeserializationContext context)
  {
    switch (contentType)
    {
      case "SoundEffect":
        return context.DeserializeWithAlias<SoundEffectContent>(ref parser);
      case "ParticleEffect":
        return context.DeserializeWithAlias<ParticleEffectContent>(ref parser);
      case "ActorPrefab":
        return context.DeserializeWithAlias<SavedActorPrefab>(ref parser);
      default:
        // Image 或未知类型，跳过 content 的值
        parser.SkipCurrentNode();
        return new object();
    }
  }
}


/// <summary>
/// 注册 SojoSavedYamlFormatter 的 VYaml FormatterResolver。
/// 需要在 SaveLoadController 的 YamlOptions 中使用。
/// </summary>
public class SojoSavedYamlFormatterResolver : VYaml.Serialization.IYamlFormatterResolver
{
  public static readonly SojoSavedYamlFormatterResolver Instance = new SojoSavedYamlFormatterResolver();

  public VYaml.Serialization.IYamlFormatter<T> GetFormatter<T>()
  {
    if (typeof(T) == typeof(Sojo.Saved))
    {
      return (VYaml.Serialization.IYamlFormatter<T>)(object)SojoSavedYamlFormatter.Instance;
    }
    return null;
  }
}


