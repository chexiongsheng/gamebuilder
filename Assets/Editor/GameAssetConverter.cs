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
        Formatting = Formatting.Indented,
        Converters = { new SojoSavedConverter() }
    };


    [MenuItem("Tools/Convert Json Game to Yaml")]
    public static void ConvertAllJsonGameToYaml()
    {
        string[] files = Directory.GetFiles(Path.Combine(Application.streamingAssetsPath, "ExampleGames"), "*.json", SearchOption.AllDirectories);
        int count = 0;

        var options = YamlSerializerOptions.Standard;
        options.Resolver = CompositeResolver.Create(
            new IYamlFormatterResolver[] {
                SojoSavedYamlFormatterResolver.Instance, // 处理 Sojo.Saved 的 content 动态类型
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
                SojoSavedYamlFormatterResolver.Instance,
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
                SojoSavedYamlFormatterResolver.Instance,
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
                SojoSavedYamlFormatterResolver.Instance,
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

    /// <summary>
    /// 升级旧格式的 YAML Prefab 文件：
    /// 旧格式中 Sojo.Saved.content 是一个 JSON 字符串（标量），
    /// 新格式中 content 是根据 contentType 序列化的嵌套 YAML mapping。
    /// 
    /// 升级思路：
    /// 1. 先用 VYaml 通用 object 反序列化读取旧 YAML（字符串 content 会被读为普通字符串）
    /// 2. 将通用 object 序列化为 JSON 字符串
    /// 3. 通过 Newtonsoft.Json 反序列化为 SavedActorPrefab（SojoSavedConverter 兼容旧字符串格式）
    /// 4. 再用新格式 YAML 重新序列化写回文件
    /// </summary>
    [MenuItem("Tools/Upgrade Old Yaml Prefabs (Sojo content string -> object)")]
    public static void UpgradeOldYamlPrefabs()
    {
        string prefabLibPath = Path.Combine(Application.streamingAssetsPath, "PrefabLibrary");
        if (!Directory.Exists(prefabLibPath))
        {
            Debug.LogError($"PrefabLibrary directory not found at {prefabLibPath}");
            return;
        }

        string[] files = Directory.GetFiles(prefabLibPath, "*.actor-prefab.yaml", SearchOption.AllDirectories);
        int upgraded = 0;
        int skipped = 0;
        int failed = 0;

        // 新格式的 YAML 序列化选项（带 SojoSavedYamlFormatter）
        var yamlOptions = YamlSerializerOptions.Standard;
        yamlOptions.Resolver = CompositeResolver.Create(
            new IYamlFormatterResolver[] {
                SojoSavedYamlFormatterResolver.Instance,
                StandardResolver.Instance,
                ReflectionResolver.Instance
            }
        );

        // 用于通用读取的 YAML 选项（不注册 SojoSavedYamlFormatter，避免旧格式报错）
        var genericYamlOptions = YamlSerializerOptions.Standard;
        genericYamlOptions.Resolver = CompositeResolver.Create(
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

                // 先尝试用新格式直接反序列化，如果成功说明不需要升级
                bool needsUpgrade = false;
                try
                {
                    YamlSerializer.Deserialize<SavedActorPrefab>(yamlBytes, yamlOptions);
                }
                catch
                {
                    needsUpgrade = true;
                }

                if (!needsUpgrade)
                {
                    skipped++;
                    continue;
                }

                // 步骤 1：用通用方式读取旧 YAML（content 会被读为字符串）
                object genericObj = YamlSerializer.Deserialize<object>(yamlBytes, genericYamlOptions);

                // 步骤 2：转为 JSON 字符串
                string intermediateJson = JsonConvert.SerializeObject(genericObj);

                // 步骤 3：用 Newtonsoft.Json 反序列化为 SavedActorPrefab
                // SojoSavedConverter.ReadJson 会兼容旧格式（content 为 JSON 字符串）
                SavedActorPrefab prefab = JsonConvert.DeserializeObject<SavedActorPrefab>(intermediateJson, JsonSettings);

                // 执行数据升级（如果有 PerformUpgrades 方法）
                MethodInfo upgradeMethod = typeof(SavedActorPrefab).GetMethod("PerformUpgrades",
                    BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                if (upgradeMethod != null)
                {
                    upgradeMethod.Invoke(prefab, null);
                }

                // 步骤 4：用新格式 YAML 重新序列化并写回
                var newYamlBytes = YamlSerializer.Serialize(prefab, yamlOptions);
                File.WriteAllBytes(file, newYamlBytes.ToArray());

                // 验证：重新读取并对比
                SavedActorPrefab verifyPrefab = YamlSerializer.Deserialize<SavedActorPrefab>(
                    File.ReadAllBytes(file), yamlOptions);
                string json1 = JsonConvert.SerializeObject(prefab, JsonSettings);
                string json2 = JsonConvert.SerializeObject(verifyPrefab, JsonSettings);

                if (json1 != json2)
                {
                    Debug.LogWarning($"[升级] {Path.GetFileName(file)}: 升级后验证不一致！");
                }
                else
                {
                    Debug.Log($"[升级] {Path.GetFileName(file)}: 升级成功并验证通过。");
                }

                upgraded++;
            }
            catch (Exception e)
            {
                Debug.LogError($"[升级失败] {file}: {e.Message}\n{e.StackTrace}");
                failed++;
            }
        }

        AssetDatabase.Refresh();
        Debug.Log($"YAML Prefab 升级完成：升级 {upgraded} 个，跳过 {skipped} 个（已是新格式），失败 {failed} 个。");
    }

    /// <summary>
    /// 升级旧格式的 YAML Game 文件（ExampleGames）：
    /// 与 Prefab 升级逻辑类似，处理 Sojo.Saved.content 从字符串到对象的迁移。
    /// </summary>
    [MenuItem("Tools/Upgrade Old Yaml Games (Sojo content string -> object)")]
    public static void UpgradeOldYamlGames()
    {
        string gamesPath = Path.Combine(Application.streamingAssetsPath, "ExampleGames");
        if (!Directory.Exists(gamesPath))
        {
            Debug.LogError($"ExampleGames directory not found at {gamesPath}");
            return;
        }

        string[] files = Directory.GetFiles(gamesPath, "*.yaml", SearchOption.AllDirectories);
        int upgraded = 0;
        int skipped = 0;
        int failed = 0;

        var yamlOptions = YamlSerializerOptions.Standard;
        yamlOptions.Resolver = CompositeResolver.Create(
            new IYamlFormatterResolver[] {
                SojoSavedYamlFormatterResolver.Instance,
                StandardResolver.Instance,
                ReflectionResolver.Instance
            }
        );

        var genericYamlOptions = YamlSerializerOptions.Standard;
        genericYamlOptions.Resolver = CompositeResolver.Create(
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

                bool needsUpgrade = false;
                try
                {
                    YamlSerializer.Deserialize<SaveLoadController.SaveGame>(yamlBytes, yamlOptions);
                }
                catch
                {
                    needsUpgrade = true;
                }

                if (!needsUpgrade)
                {
                    skipped++;
                    continue;
                }

                object genericObj = YamlSerializer.Deserialize<object>(yamlBytes, genericYamlOptions);
                string intermediateJson = JsonConvert.SerializeObject(genericObj);
                SaveLoadController.SaveGame saveGame = JsonConvert.DeserializeObject<SaveLoadController.SaveGame>(
                    intermediateJson, JsonSettings);

                var newYamlBytes = YamlSerializer.Serialize(saveGame, yamlOptions);
                File.WriteAllBytes(file, newYamlBytes.ToArray());

                // 验证
                SaveLoadController.SaveGame verifyGame = YamlSerializer.Deserialize<SaveLoadController.SaveGame>(
                    File.ReadAllBytes(file), yamlOptions);
                string json1 = JsonConvert.SerializeObject(saveGame, JsonSettings);
                string json2 = JsonConvert.SerializeObject(verifyGame, JsonSettings);

                if (json1 != json2)
                {
                    Debug.LogWarning($"[升级] {Path.GetFileName(file)}: 升级后验证不一致！");
                }
                else
                {
                    Debug.Log($"[升级] {Path.GetFileName(file)}: 升级成功并验证通过。");
                }

                upgraded++;
            }
            catch (Exception e)
            {
                Debug.LogError($"[升级失败] {file}: {e.Message}\n{e.StackTrace}");
                failed++;
            }
        }

        AssetDatabase.Refresh();
        Debug.Log($"YAML Game 升级完成：升级 {upgraded} 个，跳过 {skipped} 个（已是新格式），失败 {failed} 个。");
    }
}



