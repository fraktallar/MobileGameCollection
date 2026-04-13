using UnityEditor;
using UnityEngine;

public static class BuildSettingsSetup
{
    private static readonly string[] ScenePaths = new[]
    {
        "Assets/Scenes/SampleScene.unity",
        "Assets/Games/01_Snake/Scenes/SnakeGame.unity",
        "Assets/Games/02_BrickBreaker/Scenes/BrickBreaker.unity",
        "Assets/Games/03_FlappyBird/Scenes/FlappyBird.unity",
        "Assets/Games/04_ColorSwitch/Scenes/ColorSwitch.unity",
        "Assets/Games/05_BubbleShooter/Scenes/BubbleShooter.unity",
    };

    [MenuItem("Tools/Add All Scenes to Build Settings")]
    public static void AddScenes()
    {
        var existing = new System.Collections.Generic.List<EditorBuildSettingsScene>(
            EditorBuildSettings.scenes);

        int added = 0;
        foreach (string path in ScenePaths)
        {
            bool alreadyIn = false;
            foreach (var s in existing)
                if (s.path == path) { alreadyIn = true; break; }

            if (!alreadyIn)
            {
                existing.Add(new EditorBuildSettingsScene(path, true));
                added++;
                Debug.Log("Build Settings'e eklendi: " + path);
            }
            else
            {
                // Var olanı enabled yap
                for (int i = 0; i < existing.Count; i++)
                    if (existing[i].path == path)
                        existing[i] = new EditorBuildSettingsScene(path, true);
            }
        }

        EditorBuildSettings.scenes = existing.ToArray();

        if (added > 0)
            Debug.Log($"{added} sahne Build Settings'e eklendi.");
        else
            Debug.Log("Tüm sahneler zaten Build Settings'te mevcut.");

        EditorWindow.GetWindow(System.Type.GetType("UnityEditor.BuildPlayerWindow,UnityEditor"));
    }
}
