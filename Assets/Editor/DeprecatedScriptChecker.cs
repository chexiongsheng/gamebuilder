using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Behaviors;

public class DeprecatedScriptChecker : EditorWindow
{
    [MenuItem("Voos/Check Deprecated Scripts (Cards & Panels)")]
    public static void CheckDeprecatedScripts()
    {
        CheckScriptsInternal(new string[] { "DeprecatedCards", "DeprecatedPanels" }, "[Deprecated Script]");
    }

    [MenuItem("Voos/Check Legacy Scripts")]
    public static void CheckLegacyScripts()
    {
        CheckScriptsInternal(new string[] { "LegacyBehaviors" }, "[Legacy Script]");
    }

    private static void CheckScriptsInternal(string[] targetDirs, string logTag)
    {
        string projectPath = Path.GetDirectoryName(Application.dataPath);
        string behaviorLibraryPath = Path.Combine(Application.dataPath, "Scripts/Behaviors/Resources/BehaviorLibrary");
        
        Dictionary<string, string> deprecatedScripts = new Dictionary<string, string>(); // ScriptName -> RelativePath

        foreach (string dir in targetDirs)
        {
            string fullPath = Path.Combine(behaviorLibraryPath, dir);
            if (Directory.Exists(fullPath))
            {
                // Check for .mjs files
                string[] files = Directory.GetFiles(fullPath, "*.mjs");
                foreach (string file in files)
                {
                    string fileName = Path.GetFileNameWithoutExtension(file);
                    deprecatedScripts[fileName] = Path.Combine(dir, Path.GetFileName(file));
                }
                
                // Check for .js files as well, just in case
                string[] jsFiles = Directory.GetFiles(fullPath, "*.js");
                foreach (string file in jsFiles)
                {
                    string fileName = Path.GetFileNameWithoutExtension(file);
                    if (!deprecatedScripts.ContainsKey(fileName))
                    {
                        deprecatedScripts[fileName] = Path.Combine(dir, Path.GetFileName(file));
                    }
                }
            }
        }

        string exampleGamesPath = Path.Combine(Application.streamingAssetsPath, "ExampleGames/Public");
        if (!Directory.Exists(exampleGamesPath))
        {
            Debug.LogError($"Example games directory not found: {exampleGamesPath}");
            return;
        }

        string[] voosFiles = Directory.GetFiles(exampleGamesPath, "*.yaml");

        
        foreach (string voosFile in voosFiles)
        {
            try
            {
                SaveLoadController.SaveGame saveGame = SaveLoadController.ReadSaveGame(voosFile);
                
                // Map brainId to Brain
                Dictionary<string, Behaviors.Brain> brainMap = new Dictionary<string, Behaviors.Brain>();
                
                if (saveGame.behaviorDatabase.brains != null)
                {
                    // Try to use embedded IDs first
                    foreach (var brain in saveGame.behaviorDatabase.brains)
                    {
                        if (brain != null && !string.IsNullOrEmpty(brain.id))
                        {
                            brainMap[brain.id] = brain;
                        }
                    }

                    // If no IDs found in brains, try to use the separate ID array (legacy support)
                    if (brainMap.Count == 0 && saveGame.behaviorDatabase.brainIds != null)
                    {
                        for (int i = 0; i < saveGame.behaviorDatabase.brainIds.Length; i++)
                        {
                            if (i < saveGame.behaviorDatabase.brains.Length)
                            {
                                brainMap[saveGame.behaviorDatabase.brainIds[i]] = saveGame.behaviorDatabase.brains[i];
                            }
                        }
                    }
                }

                if (saveGame.voosEngineState.actors != null)
                {
                    foreach (var actor in saveGame.voosEngineState.actors)
                    {
                        if (string.IsNullOrEmpty(actor.brainName)) continue;

                        if (brainMap.TryGetValue(actor.brainName, out Behaviors.Brain brain))
                        {
                            if (brain.behaviorUses != null)
                            {
                                foreach (var use in brain.behaviorUses)
                                {
                                    if (string.IsNullOrEmpty(use.behaviorUri)) continue;

                                    if (use.behaviorUri.StartsWith("builtin:"))
                                    {
                                        string scriptName = use.behaviorUri.Substring("builtin:".Length);
                                        
                                        // Try to parse URI to get LocalPath, which handles encoding and other URI specifics
                                        try 
                                        {
                                            System.Uri uri = new System.Uri(use.behaviorUri);
                                            scriptName = uri.LocalPath;
                                        }
                                        catch 
                                        {
                                            // Fallback to simple substring if Uri parsing fails
                                        }

                                        if (deprecatedScripts.ContainsKey(scriptName))
                                        {
                                            // Use forward slashes for path display to be consistent with Unity/URI style
                                            string deprecatedPath = deprecatedScripts[scriptName].Replace("\\", "/");
                                            Debug.LogWarning($"{logTag} Game: {Path.GetFileName(voosFile)}, Actor: {actor.displayName}, Script: {deprecatedPath}");
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to process {voosFile}: {e.Message}");
            }
        }
        
        Debug.Log($"{logTag} Check complete.");
    }
}
