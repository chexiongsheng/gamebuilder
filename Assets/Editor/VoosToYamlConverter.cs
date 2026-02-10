using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;
using VYaml.Serialization;
using VYaml.Emitter;
using VYaml.Parser;
using System;
using System.Collections.Generic;

public class VoosToYamlConverter
{
    [MenuItem("Tools/Convert Voos Game to Yaml")]
    public static void ConvertAllGame()
    {
        string[] files = Directory.GetFiles(Path.Combine(Application.streamingAssetsPath, "ExampleGames"), "*.voos", SearchOption.AllDirectories);
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


            // 升级数据，将 Legacy 字段迁移到 Brain 对象中，避免因私有字段无法序列化导致的数据丢失
            if (saveGame.behaviorDatabase != null)
            {
                HashSet<string> usedBrainIds;
                if (saveGame.voosEngineState.actors != null)
                {
                    usedBrainIds = VoosEngine.GetUsedBrainIds(saveGame.voosEngineState.actors);
                }
                else
                {
                    usedBrainIds = new HashSet<string>();
                    usedBrainIds.Add(VoosEngine.DefaultBrainUid);
                }
                saveGame.behaviorDatabase.PerformUpgrades(usedBrainIds);
            }

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
        Debug.Log($"Converted {count} .voos files to .yaml");
    }

    [MenuItem("Tools/Convert Prefabs to Yaml")]
    public static void ConvertAllPrefabs()
    {
        string prefabLibPath = Path.Combine(Application.streamingAssetsPath, "PrefabLibrary");
        if (!Directory.Exists(prefabLibPath))
        {
            Debug.LogError($"PrefabLibrary directory not found at {prefabLibPath}");
            return;
        }

        string[] files = Directory.GetFiles(prefabLibPath, "*.actor-prefab.voos", SearchOption.AllDirectories);
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
                // Replace .voos with .yaml
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

    [MenuItem("Tools/Upgrade Yaml Save Format")]
    public static void UpgradeYamlSaveFormat()
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

                if (saveGame.behaviorDatabase != null)
                {
                    var db = saveGame.behaviorDatabase;
                    bool changed = false;

                    // Upgrade Behaviors
                    if (db.behaviorIds != null && db.behaviors != null && db.behaviorIds.Length == db.behaviors.Length)
                    {
                        for (int i = 0; i < db.behaviors.Length; i++)
                        {
                            db.behaviors[i].id = db.behaviorIds[i];
                        }
                        db.behaviorIds = null;
                        changed = true;
                    }

                    // Upgrade Brains
                    if (db.brainIds != null && db.brains != null && db.brainIds.Length == db.brains.Length)
                    {
                        for (int i = 0; i < db.brains.Length; i++)
                        {
                            if (db.brains[i] != null)
                            {
                                db.brains[i].id = db.brainIds[i];
                            }
                        }
                        db.brainIds = null;
                        changed = true;
                    }

                    if (changed)
                    {
                        var newYamlBytes = YamlSerializer.Serialize(saveGame, options);
                        File.WriteAllBytes(file, newYamlBytes.ToArray());
                        count++;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to upgrade {file}: {e.Message}");
            }
        }

        AssetDatabase.Refresh();
        Debug.Log($"Upgraded {count} .yaml files");
    }
}

