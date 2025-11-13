using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

public static class FolderCreator
{
    private static readonly string[] _rootFolders =
    {
        "Art",
        "Art/Materials",
        "Art/Models",
        "Art/Textures",
        "Art/Sprites",
        "Art/UI",
        "Art/Shaders",
        "Art/Animations",

        "Audio",
        "Audio/Music",
        "Audio/SFX",
        "Audio/Voice",

        "Scripts",
        "Scripts/Core",
        "Scripts/Core/GameManager",
        "Scripts/Core/SaveSystem",
        "Scripts/Core/Input",
        "Scripts/Core/Events",
        "Scripts/Gameplay",
        "Scripts/Gameplay/Characters",
        "Scripts/Gameplay/Items",
        "Scripts/Gameplay/Environment",
        "Scripts/Gameplay/Systems",
        "Scripts/UI",
        "Scripts/Utilities",

        "Scenes",
        "Scenes/Core",
        "Scenes/Core/Boot",
        "Scenes/Core/MainMenu",
        "Scenes/Core/Loading",
        "Scenes/Levels",
        "Scenes/Sandbox",

        "Prefabs",
        "Prefabs/Characters",
        "Prefabs/Environment",
        "Prefabs/UI",
        "Prefabs/Systems",

        "Settings",
        "Settings/Input",
        "Settings/Graphics",
        "Settings/Gameplay",

        "Resources",
        "StreamingAssets",
        "Plugins",
        "ThirdParty",

        "Editor",
        "Editor/Tools"
    };

    [MenuItem("Tools/Project Setup/Create Folder Structure", false, 1)]
    public static void CreateFolderStructure()
    {
        try
        {
            EditorApplication.LockReloadAssemblies();

            List<string> createdFolders = new();
            List<string> skippedFolders = new();

            foreach (string folderPath in _rootFolders)
            {
                string fullPath = Path.Combine("Assets", folderPath);

                if (Directory.Exists(fullPath))
                {
                    skippedFolders.Add(folderPath);
                    continue;
                }

                _ = Directory.CreateDirectory(fullPath);
                createdFolders.Add(folderPath);
            }

            AssetDatabase.Refresh();

            ShowCreationReport(createdFolders, skippedFolders);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to create folder structure: {e.Message}");
            _ = EditorUtility.DisplayDialog("Error", $"Failed to create folders: {e.Message}", "OK");
        }
        finally
        {
            EditorApplication.UnlockReloadAssemblies();
        }
    }

    private static void ShowCreationReport(List<string> created, List<string> skipped)
    {
        string message = $"Folder creation completed!\n\nCreated: {created.Count} folders\nSkipped (already existed): {skipped.Count} folders";

        if (created.Count > 0)
        {
            message += "\n\nCreated folders:\n• " + string.Join("\n• ", created);
        }

        if (skipped.Count > 0)
        {
            message += "\n\nSkipped folders:\n• " + string.Join("\n• ", skipped);
        }

        Debug.Log(message);
        _ = EditorUtility.DisplayDialog("Folder Structure Created", message, "OK");
    }
}
