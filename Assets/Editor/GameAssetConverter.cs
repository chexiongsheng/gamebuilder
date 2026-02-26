using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;
using VYaml.Serialization;
using VYaml.Emitter;
using VYaml.Parser;
using System;
using System.Collections.Generic;

public class GameAssetConverter
{
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
            SaveLoadController.SaveGame saveGame = JsonUtility.FromJson<SaveLoadController.SaveGame>(jsonContents);


            //byte[] jsonBytes = Encoding.UTF8.GetBytes(File.ReadAllText(file));

            //object saveGame = YamlSerializer.Deserialize<object>(jsonBytes);

            var yamlBytes = YamlSerializer.Serialize(saveGame, options);
            string yamlPath = Path.ChangeExtension(file, ".yaml");
            File.WriteAllBytes(yamlPath, yamlBytes.ToArray());
            SaveLoadController.SaveGame yamlGame = YamlSerializer.Deserialize<SaveLoadController.SaveGame>(File.ReadAllBytes(yamlPath), options);

            // 通过 JsonUtility 对比 saveGame 和 yamlGame 是否相等
            string saveGameJson = JsonUtility.ToJson(saveGame, true);
            string yamlGameJson = JsonUtility.ToJson(yamlGame, true);
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
                SavedActorPrefab prefab = JsonUtility.FromJson<SavedActorPrefab>(jsonContents);

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

                string json1 = JsonUtility.ToJson(prefab, true);
                string json2 = JsonUtility.ToJson(yamlPrefab, true);

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
                string jsonContents = JsonUtility.ToJson(saveGame, true);
                File.WriteAllText(jsonPath, jsonContents);

                // 验证：从写入的 json 重新读取并对比
                SaveLoadController.SaveGame jsonGame = JsonUtility.FromJson<SaveLoadController.SaveGame>(File.ReadAllText(jsonPath));
                string json1 = JsonUtility.ToJson(saveGame, true);
                string json2 = JsonUtility.ToJson(jsonGame, true);

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
                string jsonContents = JsonUtility.ToJson(prefab, true);
                File.WriteAllText(jsonPath, jsonContents);

                // 验证：从写入的 json 重新读取并对比
                SavedActorPrefab jsonPrefab = JsonUtility.FromJson<SavedActorPrefab>(File.ReadAllText(jsonPath));
                string json1 = JsonUtility.ToJson(prefab, true);
                string json2 = JsonUtility.ToJson(jsonPrefab, true);

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


