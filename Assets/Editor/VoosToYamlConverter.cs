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
    public static void ConvertAll()
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
}
