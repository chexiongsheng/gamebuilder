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

/// <summary>
/// 只序列化公有字段，不序列化属性（模拟 Unity JsonUtility 的行为），
/// 避免 UnityEngine.Color.linear 等属性导致的自引用循环。
/// </summary>
public class FieldsOnlyContractResolver : DefaultContractResolver
{
    protected override List<MemberInfo> GetSerializableMembers(Type objectType)
    {
        // 只返回公有实例字段（与 JsonUtility 行为一致）
        return objectType.GetFields(BindingFlags.Public | BindingFlags.Instance)
            .Cast<MemberInfo>()
            .ToList();
    }
}

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
}


