using System.IO;
using System.Linq;
using UnityEditor.Build;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class WebGLBuild
{
    private const string DefaultScenePath = "Assets/Scenes/Main.unity";
    private const string OutputDirectory = "docs";

    public static void BuildForGitHubPages()
    {
        EnsureAtLeastOneScene();

        var scenes = EditorBuildSettings.scenes
            .Where(scene => scene.enabled)
            .Select(scene => scene.path)
            .ToArray();

        if (scenes.Length == 0)
        {
            throw new BuildFailedException("No enabled scenes found for WebGL build.");
        }

        Directory.CreateDirectory(OutputDirectory);

        var options = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = OutputDirectory,
            target = BuildTarget.WebGL,
            options = BuildOptions.None
        };

        var report = BuildPipeline.BuildPlayer(options);

        if (report.summary.result != BuildResult.Succeeded)
        {
            throw new BuildFailedException($"WebGL build failed: {report.summary.result}");
        }

        File.WriteAllText(Path.Combine(OutputDirectory, ".nojekyll"), string.Empty);
        Debug.Log($"WebGL build completed at: {Path.GetFullPath(OutputDirectory)}");
    }

    private static void EnsureAtLeastOneScene()
    {
        var enabledScenes = EditorBuildSettings.scenes.Where(scene => scene.enabled).ToArray();
        if (enabledScenes.Length > 0)
        {
            return;
        }

        Directory.CreateDirectory("Assets/Scenes");

        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        var floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
        floor.name = "SandFloor";
        floor.transform.localScale = new Vector3(6f, 1f, 6f);

        var manager = new GameObject("WaterproofManager");
        manager.AddComponent<WaterproofManager>();

        RenderSettings.fog = true;
        RenderSettings.fogColor = new Color(0f, 0.2f, 0.4f);
        RenderSettings.fogDensity = 0.02f;

        EditorSceneManager.SaveScene(scene, DefaultScenePath);
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(DefaultScenePath, true) };
        AssetDatabase.SaveAssets();

        // Keep this scene as active after generation so subsequent manual edits are straightforward.
        SceneManager.SetActiveScene(scene);
    }
}
