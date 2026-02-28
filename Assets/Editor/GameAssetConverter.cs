using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;
using VYaml.Serialization;
using VYaml.Emitter;
using VYaml.Parser;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Reflection;
using System.Linq;

public class GameAssetConverter

{
    // 配置 Newtonsoft.Json 只序列化字段（与 Unity JsonUtility 行为一致），
    // 避免 UnityEngine.Color.linear 等属性导致的自引用循环
    private static readonly JsonSerializerSettings JsonSettings = new JsonSerializerSettings
    {
        ContractResolver = new FieldsOnlyContractResolver(),
        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
        Formatting = Formatting.Indented
    };


    [MenuItem("Tools/Convert Json Game to Yaml")]
    public static void ConvertAllJsonGameToYaml()
    {
        string[] files = Directory.GetFiles(Path.Combine(Application.streamingAssetsPath, "ExampleGames"), "*.json", SearchOption.AllDirectories);
        int count = 0;

        var options = YamlSerializerOptions.Standard;
        options.Resolver = CompositeResolver.Create(
            new IYamlFormatterResolver[] {
                StandardResolver.Instance,   // 优先使用内置和生成的格式化器
                ReflectionResolver.Instance  // 对于没有生成的类型，回退到反射
            }
        );


        foreach (string file in files)
        {
            //SaveLoadController.SaveGame saveGame = SaveLoadController.ReadSaveGame(file);
            string jsonContents = File.ReadAllText(file);
            SaveLoadController.SaveGame saveGame = JsonConvert.DeserializeObject<SaveLoadController.SaveGame>(jsonContents, JsonSettings);


            //byte[] jsonBytes = Encoding.UTF8.GetBytes(File.ReadAllText(file));

            //object saveGame = YamlSerializer.Deserialize<object>(jsonBytes);

            var yamlBytes = YamlSerializer.Serialize(saveGame, options);
            string yamlPath = Path.ChangeExtension(file, ".yaml");
            File.WriteAllBytes(yamlPath, yamlBytes.ToArray());
            SaveLoadController.SaveGame yamlGame = YamlSerializer.Deserialize<SaveLoadController.SaveGame>(File.ReadAllBytes(yamlPath), options);

            // 通过 JsonUtility 对比 saveGame 和 yamlGame 是否相等
            string saveGameJson = JsonConvert.SerializeObject(saveGame, JsonSettings);
            string yamlGameJson = JsonConvert.SerializeObject(yamlGame, JsonSettings);
            bool isEqual = saveGameJson == yamlGameJson;
            
            if (!isEqual)
            {
                Debug.LogWarning($"File {Path.GetFileName(file)}: saveGame and yamlGame are NOT equal!");
                Debug.LogWarning($"SaveGame JSON length: {saveGameJson.Length}, YamlGame JSON length: {yamlGameJson.Length}");
                File.WriteAllText(Path.ChangeExtension(file, ".json"), yamlGameJson);
                File.WriteAllText(Path.ChangeExtension(file, ".json2"), saveGameJson);
            }
            else
            {
                Debug.Log($"File {Path.GetFileName(file)}: saveGame and yamlGame are equal.");
                //File.WriteAllText(Path.ChangeExtension(file, ".json"), yamlGameJson);
            }

            ++count;

        }
        
        AssetDatabase.Refresh();
        Debug.Log($"Converted {count} .json files to .yaml");
    }

    [MenuItem("Tools/Convert Json Prefabs to Yaml")]
    public static void ConvertAllJsonPrefabsToYaml()
    {
        string prefabLibPath = Path.Combine(Application.streamingAssetsPath, "PrefabLibrary");
        if (!Directory.Exists(prefabLibPath))
        {
            Debug.LogError($"PrefabLibrary directory not found at {prefabLibPath}");
            return;
        }

        string[] files = Directory.GetFiles(prefabLibPath, "*.actor-prefab.json", SearchOption.AllDirectories);
        int count = 0;

        var options = YamlSerializerOptions.Standard;
        options.Resolver = CompositeResolver.Create(
            new IYamlFormatterResolver[] {
                StandardResolver.Instance,
                ReflectionResolver.Instance
            }
        );

        foreach (string file in files)
        {
            try
            {
                string jsonContents = File.ReadAllText(file);
                SavedActorPrefab prefab = JsonConvert.DeserializeObject<SavedActorPrefab>(jsonContents, JsonSettings);

                // Perform upgrades using reflection since the method is internal
                System.Reflection.MethodInfo upgradeMethod = typeof(SavedActorPrefab).GetMethod("PerformUpgrades", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
                if (upgradeMethod != null)
                {
                    upgradeMethod.Invoke(prefab, null);
                }

                var yamlBytes = YamlSerializer.Serialize(prefab, options);
                // Replace .json with .yaml
                string yamlPath = file.Substring(0, file.Length - 5) + ".yaml";
                
                File.WriteAllBytes(yamlPath, yamlBytes.ToArray());
                
                // Verify
                SavedActorPrefab yamlPrefab = YamlSerializer.Deserialize<SavedActorPrefab>(File.ReadAllBytes(yamlPath), options);

                string json1 = JsonConvert.SerializeObject(prefab, JsonSettings);
                string json2 = JsonConvert.SerializeObject(yamlPrefab, JsonSettings);

                if (json1 != json2)
                {
                    Debug.LogWarning($"File {Path.GetFileName(file)}: JSON and YAML mismatch!");
                }

                count++;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to convert {file}: {e.Message}");
            }
        }

        AssetDatabase.Refresh();
        Debug.Log($"Converted {count} prefab files to .yaml");
    }

    [MenuItem("Tools/Convert Yaml Game to Json")]
    public static void ConvertAllYamlGameToJson()
    {
        string[] files = Directory.GetFiles(Path.Combine(Application.streamingAssetsPath, "ExampleGames"), "*.yaml", SearchOption.AllDirectories);
        int count = 0;

        var options = YamlSerializerOptions.Standard;
        options.Resolver = CompositeResolver.Create(
            new IYamlFormatterResolver[] {
                StandardResolver.Instance,
                ReflectionResolver.Instance
            }
        );

        foreach (string file in files)
        {
            try
            {
                byte[] yamlBytes = File.ReadAllBytes(file);
                SaveLoadController.SaveGame saveGame = YamlSerializer.Deserialize<SaveLoadController.SaveGame>(yamlBytes, options);

                string jsonPath = Path.ChangeExtension(file, ".json");
                string jsonContents = JsonConvert.SerializeObject(saveGame, JsonSettings);
                File.WriteAllText(jsonPath, jsonContents);

                // 验证：从写入的 json 重新读取并对比
                SaveLoadController.SaveGame jsonGame = JsonConvert.DeserializeObject<SaveLoadController.SaveGame>(File.ReadAllText(jsonPath), JsonSettings);
                string json1 = JsonConvert.SerializeObject(saveGame, JsonSettings);
                string json2 = JsonConvert.SerializeObject(jsonGame, JsonSettings);

                if (json1 != json2)
                {
                    Debug.LogWarning($"File {Path.GetFileName(file)}: YAML and JSON mismatch!");
                }
                else
                {
                    Debug.Log($"File {Path.GetFileName(file)}: conversion verified OK.");
                }

                count++;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to convert {file}: {e.Message}");
            }
        }

        AssetDatabase.Refresh();
        Debug.Log($"Converted {count} .yaml files to .json");
    }

    [MenuItem("Tools/Convert Yaml Prefabs to Json")]
    public static void ConvertAllYamlPrefabsToJson()
    {
        string prefabLibPath = Path.Combine(Application.streamingAssetsPath, "PrefabLibrary");
        if (!Directory.Exists(prefabLibPath))
        {
            Debug.LogError($"PrefabLibrary directory not found at {prefabLibPath}");
            return;
        }

        string[] files = Directory.GetFiles(prefabLibPath, "*.actor-prefab.yaml", SearchOption.AllDirectories);
        int count = 0;

        var options = YamlSerializerOptions.Standard;
        options.Resolver = CompositeResolver.Create(
            new IYamlFormatterResolver[] {
                StandardResolver.Instance,
                ReflectionResolver.Instance
            }
        );

        foreach (string file in files)
        {
            try
            {
                byte[] yamlBytes = File.ReadAllBytes(file);
                SavedActorPrefab prefab = YamlSerializer.Deserialize<SavedActorPrefab>(yamlBytes, options);

                // 将 .yaml 替换为 .json
                string jsonPath = file.Substring(0, file.Length - 5) + ".json";
                string jsonContents = JsonConvert.SerializeObject(prefab, JsonSettings);
                File.WriteAllText(jsonPath, jsonContents);

                // 验证：从写入的 json 重新读取并对比
                SavedActorPrefab jsonPrefab = JsonConvert.DeserializeObject<SavedActorPrefab>(File.ReadAllText(jsonPath), JsonSettings);
                string json1 = JsonConvert.SerializeObject(prefab, JsonSettings);
                string json2 = JsonConvert.SerializeObject(jsonPrefab, JsonSettings);

                if (json1 != json2)
                {
                    Debug.LogWarning($"File {Path.GetFileName(file)}: YAML and JSON mismatch!");
                }
                else
                {
                    Debug.Log($"File {Path.GetFileName(file)}: conversion verified OK.");
                }

                count++;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to convert {file}: {e.Message}");
            }
        }

        AssetDatabase.Refresh();
        Debug.Log($"Converted {count} prefab yaml files to .json");
    }

    // ==================== 动态 content 序列化/反序列化示例 ====================

    /// <summary>
    /// 示例：content 字段为 object 类型，根据 contentType 动态序列化/反序列化。
    /// 不修改原有 Sojo 类，使用新类型来演示。
    /// </summary>
    [System.Serializable]
    public class DynamicContentItem
    {
        public string id;
        public string name;
        public string contentType; // "Color" 或 "Vector3"
        public object content;     // 实际类型由 contentType 决定
    }

    // ---- contentType 到 C# 类型的映射 ----
    static readonly Dictionary<string, Type> ContentTypeMap = new Dictionary<string, Type>
    {
        { "Color",   typeof(Color) },
        { "Vector3", typeof(Vector3) },
    };

    static Type ResolveContentType(string contentType)
    {
        if (ContentTypeMap.TryGetValue(contentType, out var type))
            return type;
        throw new Exception($"未知的 contentType: {contentType}");
    }

    // =======================================================================
    //  Newtonsoft.Json 方案：自定义 JsonConverter
    // =======================================================================

    /// <summary>
    /// 为 DynamicContentItem 编写的 JsonConverter，
    /// 序列化时按 content 的实际类型写入嵌套 JSON 对象；
    /// 反序列化时根据 contentType 字段还原为对应的 C# 类型。
    /// </summary>
    public class DynamicContentItemConverter : JsonConverter<DynamicContentItem>
    {
        // 只序列化字段，与 FieldsOnlyContractResolver 保持一致
        static readonly JsonSerializerSettings InnerSettings = new JsonSerializerSettings
        {
            ContractResolver = new FieldsOnlyContractResolver(),
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
        };

        public override void WriteJson(JsonWriter writer, DynamicContentItem value, JsonSerializer serializer)
        {
            writer.WriteStartObject();

            writer.WritePropertyName("id");
            writer.WriteValue(value.id);

            writer.WritePropertyName("name");
            writer.WriteValue(value.name);

            writer.WritePropertyName("contentType");
            writer.WriteValue(value.contentType);

            // content 直接作为嵌套 JSON 对象写入（不是字符串！）
            writer.WritePropertyName("content");
            var innerSerializer = JsonSerializer.Create(InnerSettings);
            innerSerializer.Serialize(writer, value.content);

            writer.WriteEndObject();
        }

        public override DynamicContentItem ReadJson(JsonReader reader, Type objectType,
            DynamicContentItem existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var jObj = Newtonsoft.Json.Linq.JObject.Load(reader);

            var item = new DynamicContentItem
            {
                id = jObj["id"]?.ToString(),
                name = jObj["name"]?.ToString(),
                contentType = jObj["contentType"]?.ToString(),
            };

            // 根据 contentType 反序列化 content 为对应的 C# 类型
            var targetType = ResolveContentType(item.contentType);
            var innerSerializer = JsonSerializer.Create(InnerSettings);
            item.content = jObj["content"]?.ToObject(targetType, innerSerializer);

            return item;
        }
    }

    // =======================================================================
    //  VYaml 方案：自定义 IYamlFormatter
    // =======================================================================

    /// <summary>
    /// 为 DynamicContentItem 编写的 VYaml Formatter，
    /// 序列化时 content 按实际类型展开为 YAML mapping；
    /// 反序列化时根据 contentType 还原为对应的 C# 类型。
    /// </summary>
    public class DynamicContentItemYamlFormatter : IYamlFormatter<DynamicContentItem>
    {
        public static readonly DynamicContentItemYamlFormatter Instance = new();

        public void Serialize(ref Utf8YamlEmitter emitter, DynamicContentItem value, YamlSerializationContext context)
        {
            emitter.BeginMapping();

            emitter.WriteString("id");
            emitter.WriteString(value.id);

            emitter.WriteString("name");
            emitter.WriteString(value.name);

            emitter.WriteString("contentType");
            emitter.WriteString(value.contentType);

            // content 根据实际类型动态序列化（展开为 YAML 映射，而非字符串）
            emitter.WriteString("content");
            var targetType = ResolveContentType(value.contentType);
            if (targetType == typeof(Color))
                context.Serialize(ref emitter, (Color)value.content);
            else if (targetType == typeof(Vector3))
                context.Serialize(ref emitter, (Vector3)value.content);
            else
                throw new Exception($"VYaml: 不支持的 contentType: {value.contentType}");

            emitter.EndMapping();
        }

        public DynamicContentItem Deserialize(ref YamlParser parser, YamlDeserializationContext context)
        {
            parser.ReadWithVerify(ParseEventType.MappingStart);

            var item = new DynamicContentItem();

            while (!parser.End && parser.CurrentEventType != ParseEventType.MappingEnd)
            {
                var key = parser.ReadScalarAsString();
                switch (key)
                {
                    case "id":
                        item.id = parser.ReadScalarAsString();
                        break;
                    case "name":
                        item.name = parser.ReadScalarAsString();
                        break;
                    case "contentType":
                        item.contentType = parser.ReadScalarAsString();
                        break;
                    case "content":
                        // 根据 contentType 动态反序列化
                        var targetType = ResolveContentType(item.contentType);
                        if (targetType == typeof(Color))
                            item.content = context.DeserializeWithAlias<Color>(ref parser);
                        else if (targetType == typeof(Vector3))
                            item.content = context.DeserializeWithAlias<Vector3>(ref parser);
                        else
                            throw new Exception($"VYaml: 不支持的 contentType: {item.contentType}");
                        break;
                    default:
                        parser.SkipCurrentNode();
                        break;
                }
            }

            parser.ReadWithVerify(ParseEventType.MappingEnd);
            return item;
        }
    }

    /// <summary>
    /// 自定义 Resolver，优先使用 DynamicContentItemYamlFormatter
    /// </summary>
    public class DynamicContentItemResolver : IYamlFormatterResolver
    {
        public static readonly DynamicContentItemResolver Instance = new();

        public IYamlFormatter<T> GetFormatter<T>()
        {
            if (typeof(T) == typeof(DynamicContentItem))
                return (IYamlFormatter<T>)(object)DynamicContentItemYamlFormatter.Instance;
            return null;
        }
    }

    // =======================================================================
    //  菜单入口：演示 Newtonsoft.Json 和 VYaml 的动态序列化/反序列化
    // =======================================================================

    [MenuItem("Tools/Demo Dynamic Content Serialization")]
    public static void DemoDynamicContentSerialization()
    {
        // 构造测试数据：两个 DynamicContentItem，分别用 Color 和 Vector3 作为 content
        var items = new List<DynamicContentItem>
        {
            new DynamicContentItem
            {
                id = "item-001",
                name = "Red Color",
                contentType = "Color",
                content = new Color(1f, 0f, 0f, 1f)
            },
            new DynamicContentItem
            {
                id = "item-002",
                name = "Forward Vector",
                contentType = "Vector3",
                content = new Vector3(0f, 0f, 1f)
            }
        };

        Debug.Log("========== Newtonsoft.Json 示例 ==========");
        DemoNewtonsoftJson(items);

        Debug.Log("========== VYaml 示例 ==========");
        DemoVYaml(items);
    }

    static void DemoNewtonsoftJson(List<DynamicContentItem> items)
    {
        // 配置：注册自定义 Converter + 只序列化字段
        var settings = new JsonSerializerSettings
        {
            ContractResolver = new FieldsOnlyContractResolver(),
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            Formatting = Formatting.Indented,
            Converters = { new DynamicContentItemConverter() }
        };

        // 序列化
        string json = JsonConvert.SerializeObject(items, settings);
        Debug.Log($"[Newtonsoft] 序列化 JSON:\n{json}");

        // 反序列化
        var deserialized = JsonConvert.DeserializeObject<List<DynamicContentItem>>(json, settings);
        foreach (var item in deserialized)
        {
            Debug.Log($"[Newtonsoft] 反序列化: id={item.id}, contentType={item.contentType}, " +
                      $"content类型={item.content.GetType().Name}, content={item.content}");
        }
    }

    static void DemoVYaml(List<DynamicContentItem> items)
    {
        // 配置：注册自定义 Formatter
        var options = YamlSerializerOptions.Standard;
        options.Resolver = CompositeResolver.Create(
            new IYamlFormatterResolver[]
            {
                DynamicContentItemResolver.Instance,  // 优先使用自定义 Formatter
                StandardResolver.Instance,
                ReflectionResolver.Instance
            }
        );

        // 序列化
        var yamlBytes = YamlSerializer.Serialize(items, options);
        string yaml = Encoding.UTF8.GetString(yamlBytes.ToArray());
        Debug.Log($"[VYaml] 序列化 YAML:\n{yaml}");

        // 反序列化
        var deserialized = YamlSerializer.Deserialize<List<DynamicContentItem>>(yamlBytes.ToArray(), options);
        foreach (var item in deserialized)
        {
            Debug.Log($"[VYaml] 反序列化: id={item.id}, contentType={item.contentType}, " +
                      $"content类型={item.content.GetType().Name}, content={item.content}");
        }
    }

    // =======================================================================
    //  数据迁移工具：将 Sojo.Saved.content 从 JSON 字符串迁移为嵌套对象
    // =======================================================================

    /// <summary>
    /// contentType 到 C# 类型的映射（用于 Sojo 数据迁移，复用 Sojo.ContentTypeMap）
    /// </summary>
    static Dictionary<string, Type> SojoContentTypeMap => Sojo.ContentTypeMap;


    /// <summary>
    /// 将 JObject 中的 sojo 数组的 content 字段从 JSON 字符串展开为嵌套 JSON 对象。
    /// </summary>
    /// <param name="sojosArray">sojos JArray</param>
    /// <returns>成功转换的 sojo 数量</returns>
    static int ExpandSojoContentInPlace(Newtonsoft.Json.Linq.JArray sojosArray)
    {
        if (sojosArray == null) return 0;

        int converted = 0;
        var innerSerializer = JsonSerializer.Create(JsonSettings);

        foreach (var sojoToken in sojosArray)
        {
            if (sojoToken is not Newtonsoft.Json.Linq.JObject sojoObj) continue;

            var contentTypeStr = sojoObj["contentType"]?.ToString();
            var contentToken = sojoObj["content"];

            Debug.Log($"[ExpandSojo] id={sojoObj["id"]}, contentType={contentTypeStr}, content JTokenType={contentToken?.Type}");

            // 如果 content 已经是对象（非字符串），说明已经迁移过，跳过
            if (contentToken == null || contentToken.Type != Newtonsoft.Json.Linq.JTokenType.String)
            {
                Debug.Log($"[ExpandSojo] 跳过: content 为 null 或非字符串类型 (Type={contentToken?.Type})");
                continue;
            }


            string contentJsonStr = contentToken.ToString();

            // 空字符串或 "{}" 的情况（Image 类型）
            if (string.IsNullOrWhiteSpace(contentJsonStr) || contentJsonStr == "{}")
            {
                sojoObj["content"] = new Newtonsoft.Json.Linq.JObject();
                converted++;
                continue;
            }

            // 根据 contentType 解析 content JSON 字符串为具体类型，再转为 JToken
            if (contentTypeStr != null && SojoContentTypeMap.TryGetValue(contentTypeStr, out Type targetType))
            {
                try
                {
                    Debug.Log($"[ExpandSojo] 尝试反序列化 content (contentType={contentTypeStr}, targetType={targetType.Name})");
                    // 先反序列化为具体类型（确保字段过滤生效）
                    object contentObj = JsonConvert.DeserializeObject(contentJsonStr, targetType, JsonSettings);
                    // 再转为 JToken（嵌套 JSON 对象）
                    var contentJToken = Newtonsoft.Json.Linq.JToken.FromObject(contentObj, innerSerializer);
                    sojoObj["content"] = contentJToken;
                    converted++;
                    Debug.Log($"[ExpandSojo] 成功转换 sojo (id={sojoObj["id"]})");
                }
                catch (Exception e)
                {
                    Debug.LogError($"迁移 sojo content 失败 (id={sojoObj["id"]}, contentType={contentTypeStr}): {e.Message}\n{e.StackTrace}");
                }

            }
            else
            {
                Debug.LogWarning($"未知的 contentType '{contentTypeStr}'，跳过 sojo (id={sojoObj["id"]})");
            }
        }

        return converted;
    }

    // ---- 迁移 BuiltinSojos.txt ----

    [MenuItem("Tools/Migrate/Migrate BuiltinSojos.txt (content string -> object)")]
    public static void MigrateBuiltinSojos()
    {
        string filePath = Path.Combine(Application.dataPath, "Resources", "BuiltinSojos.txt");
        if (!File.Exists(filePath))
        {
            Debug.LogError($"BuiltinSojos.txt not found at {filePath}");
            return;
        }

        string jsonText = File.ReadAllText(filePath);
        var root = Newtonsoft.Json.Linq.JObject.Parse(jsonText);
        var sojosArray = root["sojos"] as Newtonsoft.Json.Linq.JArray;

        if (sojosArray == null)
        {
            Debug.LogError("BuiltinSojos.txt 中未找到 sojos 数组");
            return;
        }

        Debug.Log($"[BuiltinSojos] 找到 {sojosArray.Count} 个 sojo，开始迁移...");
        int converted = ExpandSojoContentInPlace(sojosArray);

        // 写回文件（格式化输出）

        string newJson = root.ToString(Formatting.Indented);
        File.WriteAllText(filePath, newJson);

        AssetDatabase.Refresh();
        Debug.Log($"[BuiltinSojos.txt] 迁移完成，共转换 {converted}/{sojosArray.Count} 个 sojo 的 content");
    }

    // ---- 迁移 YAML 游戏存档 (ExampleGames/) ----

    [MenuItem("Tools/Migrate/Migrate Yaml Games (sojo content string -> object)")]
    public static void MigrateYamlGameSojoContent()
    {
        string gamesPath = Path.Combine(Application.streamingAssetsPath, "ExampleGames");
        if (!Directory.Exists(gamesPath))
        {
            Debug.LogError($"ExampleGames directory not found at {gamesPath}");
            return;
        }

        string[] files = Directory.GetFiles(gamesPath, "*.yaml", SearchOption.AllDirectories);
        int fileCount = 0;
        int totalConverted = 0;

        // 使用无类型反序列化（object），这样 VYaml 会将 YAML 解析为
        // Dictionary<string, object> / List<object> / string / number 等动态结构，
        // 绕过 Sojo.Saved.content 是 string 类型的限制。
        var options = YamlSerializerOptions.Standard;
        options.Resolver = CompositeResolver.Create(
            new IYamlFormatterResolver[] {
                StandardResolver.Instance,
                ReflectionResolver.Instance
            }
        );

        foreach (string file in files)
        {
            // 跳过 .meta 文件
            if (file.EndsWith(".meta")) continue;

            try
            {
                // 读取 YAML → 反序列化为无类型 object（得到 Dictionary 树）
                byte[] yamlBytes = File.ReadAllBytes(file);
                object dynamicRoot = YamlSerializer.Deserialize<object>(yamlBytes, options);

                // 在动态树中找到 sojoDatabase.sojos 数组，展开 content
                int converted = ExpandSojoContentInDynamicTree(dynamicRoot);

                if (converted > 0)
                {
                    // 直接将修改后的动态树序列化回 YAML（content 已经是 Dictionary 而非 string）
                    var newYamlBytes = YamlSerializer.Serialize(dynamicRoot, options);
                    File.WriteAllBytes(file, newYamlBytes.ToArray());

                    totalConverted += converted;
                    Debug.Log($"[{Path.GetFileName(file)}] 迁移 {converted} 个 sojo content");
                }

                fileCount++;
            }
            catch (Exception e)
            {
                Debug.LogError($"迁移失败 {file}: {e.Message}\n{e.StackTrace}");
            }
        }

        AssetDatabase.Refresh();
        Debug.Log($"[YAML Games] 迁移完成，处理 {fileCount} 个文件，共转换 {totalConverted} 个 sojo content");
    }

    /// <summary>
    /// 在动态字典中按字符串 key 查找值。
    /// VYaml 无类型反序列化返回 Dictionary&lt;object?, object?&gt;，key 是 object（实际为 string），
    /// 不能直接用 Dictionary&lt;string, object&gt; 匹配。
    /// </summary>
    static bool TryGetDictValue(IDictionary<object, object> dict, string key, out object value)
    {
        // 先尝试直接用 string key 查找（因为 VYaml 的 key 实际是 string 类型的 object）
        if (dict.TryGetValue(key, out value)) return true;

        // 如果失败，遍历查找
        foreach (var kvp in dict)
        {
            if (kvp.Key?.ToString() == key)
            {
                value = kvp.Value;
                return true;
            }
        }
        value = null;
        return false;
    }

    /// <summary>
    /// 尝试将 object 转为 IDictionary&lt;object, object&gt;（兼容 VYaml 的动态字典类型）。
    /// </summary>
    static IDictionary<object, object> AsDynamicDict(object obj)
    {
        // VYaml 无类型反序列化返回 Dictionary<object?, object?>，
        // 但 C# 中 Dictionary<object?, object?> 也实现了 IDictionary<object, object>
        if (obj is IDictionary<object, object> dict) return dict;
        return null;
    }

    /// <summary>
    /// 在 VYaml 无类型反序列化得到的动态树（Dictionary/List）中，
    /// 找到 sojoDatabase.sojos 数组，将每个 sojo 的 content 从 JSON 字符串展开为嵌套字典。
    /// 注意：VYaml Deserialize&lt;object&gt; 返回的 mapping 类型是 Dictionary&lt;object?, object?&gt;，
    /// 而非 Dictionary&lt;string, object&gt;。
    /// </summary>
    static int ExpandSojoContentInDynamicTree(object root)
    {
        var rootDict = AsDynamicDict(root);
        if (rootDict == null) return 0;

        // 找到 sojoDatabase
        if (!TryGetDictValue(rootDict, "sojoDatabase", out object sojoDbObj)) return 0;
        var sojoDbDict = AsDynamicDict(sojoDbObj);
        if (sojoDbDict == null) return 0;

        // 找到 sojos 数组
        if (!TryGetDictValue(sojoDbDict, "sojos", out object sojosObj)) return 0;
        if (sojosObj is not IList<object> sojosList) return 0;

        int converted = 0;
        var innerSerializer = JsonSerializer.Create(JsonSettings);

        foreach (var item in sojosList)
        {
            var sojoDict = AsDynamicDict(item);
            if (sojoDict == null) continue;

            // 获取 contentType
            if (!TryGetDictValue(sojoDict, "contentType", out object ctObj)) continue;
            string contentTypeStr = ctObj?.ToString();

            // 获取 content
            if (!TryGetDictValue(sojoDict, "content", out object contentObj)) continue;

            // 如果 content 已经是 Dictionary（说明已经迁移过），跳过
            if (AsDynamicDict(contentObj) != null) continue;

            // content 应该是 string（旧格式的 JSON 字符串）
            if (contentObj is not string contentJsonStr) continue;

            // 空字符串或 "{}" 的情况（Image 类型）
            if (string.IsNullOrWhiteSpace(contentJsonStr) || contentJsonStr == "{}")
            {
                sojoDict["content"] = new Dictionary<object, object>();
                converted++;
                continue;
            }

            // 根据 contentType 将 JSON 字符串解析为具体 C# 类型，再转为 Dictionary
            if (contentTypeStr != null && SojoContentTypeMap.TryGetValue(contentTypeStr, out Type targetType))
            {
                try
                {
                    // 反序列化为具体类型（确保字段过滤生效）
                    object typedObj = JsonConvert.DeserializeObject(contentJsonStr, targetType, JsonSettings);
                    // 转为 JToken 再转为 VYaml 兼容的动态结构
                    var jToken = Newtonsoft.Json.Linq.JToken.FromObject(typedObj, innerSerializer);
                    sojoDict["content"] = JTokenToDynamic(jToken);
                    converted++;
                }
                catch (Exception e)
                {
                    TryGetDictValue(sojoDict, "id", out object idObj);
                    string sojoId = idObj?.ToString() ?? "unknown";
                    Debug.LogWarning($"迁移 sojo content 失败 (id={sojoId}, contentType={contentTypeStr}): {e.Message}");
                }
            }
            else
            {
                TryGetDictValue(sojoDict, "id", out object idObj);
                string sojoId = idObj?.ToString() ?? "unknown";
                Debug.LogWarning($"未知的 contentType '{contentTypeStr}'，跳过 sojo (id={sojoId})");
            }
        }

        return converted;
    }


    /// <summary>
    /// 将 Newtonsoft.Json 的 JToken 递归转换为 VYaml 兼容的动态结构
    /// （Dictionary&lt;object, object&gt; / List / 基本类型），以便 VYaml 无类型序列化能正确输出 YAML 映射。
    /// 注意：必须使用 Dictionary&lt;object, object&gt; 而非 Dictionary&lt;string, object&gt;，
    /// 因为 VYaml 的 PrimitiveObjectFormatter 序列化时期望 Dictionary&lt;object?, object?&gt;。
    /// </summary>
    static object JTokenToDynamic(Newtonsoft.Json.Linq.JToken token)
    {
        switch (token.Type)
        {
            case Newtonsoft.Json.Linq.JTokenType.Object:
                var dict = new Dictionary<object, object>();
                foreach (var prop in ((Newtonsoft.Json.Linq.JObject)token).Properties())
                {
                    dict[prop.Name] = JTokenToDynamic(prop.Value);
                }
                return dict;


            case Newtonsoft.Json.Linq.JTokenType.Array:
                var list = new List<object>();
                foreach (var item in (Newtonsoft.Json.Linq.JArray)token)
                {
                    list.Add(JTokenToDynamic(item));
                }
                return list;

            case Newtonsoft.Json.Linq.JTokenType.Integer:
                return (long)token;


            case Newtonsoft.Json.Linq.JTokenType.Float:
                return (double)token;


            case Newtonsoft.Json.Linq.JTokenType.Boolean:
                return (bool)token;


            case Newtonsoft.Json.Linq.JTokenType.String:
                return (string)token;


            case Newtonsoft.Json.Linq.JTokenType.Null:
                return null;

            default:
                return token.ToString();
        }
    }


    // ---- 一键迁移所有数据 ----

    [MenuItem("Tools/Migrate/Migrate All Sojo Data (BuiltinSojos + Yaml Games)")]
    public static void MigrateAllSojoData()
    {
        Debug.Log("========== 开始迁移所有 Sojo 数据 ==========");
        MigrateBuiltinSojos();
        MigrateYamlGameSojoContent();
        Debug.Log("========== 所有 Sojo 数据迁移完成 ==========");
    }
}


