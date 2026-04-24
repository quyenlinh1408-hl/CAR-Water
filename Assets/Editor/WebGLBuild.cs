using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class WebGLBuild
{
    private const string DefaultScenePath = "Assets/Scenes/Main.unity";
    private const string OutputDirectory = "docs";

    [MenuItem("Tools/DeepSeaDriver/Build WebGL Docs")]
    public static void BuildWebGLDocsFromMenu()
    {
        BuildForGitHubPages(false);
    }

    [MenuItem("Tools/DeepSeaDriver/Build WebGL Docs (Safe Mode)")]
    public static void BuildWebGLDocsSafeModeFromMenu()
    {
        BuildForGitHubPages(true);
    }

    public static void BuildForGitHubPages(bool safeMode = false)
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

        if (safeMode)
        {
            try
            {
                PatchIndexHtmlForGitHubPages();
            }
            catch (Exception ex)
            {
                Debug.LogWarning("Safe Mode: skipped index patch. " + ex.Message);
            }
        }
        else
        {
            PatchIndexHtmlForGitHubPages();
        }

        File.WriteAllText(Path.Combine(OutputDirectory, ".nojekyll"), string.Empty);
        Debug.Log($"WebGL build completed at: {Path.GetFullPath(OutputDirectory)}");
    }

    private static void PatchIndexHtmlForGitHubPages()
    {
        var projectSettingsPath = Path.GetFullPath(Path.Combine("ProjectSettings", "ProjectSettings.asset"));
        if (File.Exists(projectSettingsPath))
        {
            var projectSettings = File.ReadAllText(projectSettingsPath);
            if (projectSettings.Contains("webGLCompressionFormat: 0"))
            {
                return;
            }
        }

        var indexPath = Path.Combine(OutputDirectory, "index.html");
        if (!File.Exists(indexPath))
        {
            throw new BuildFailedException($"Generated WebGL index not found: {indexPath}");
        }

        var html = File.ReadAllText(indexPath);
        if (html.Contains("async function decompressGzipFile"))
        {
            return;
        }

        const string marker = "      document.querySelector(\"#unity-loading-bar\").style.display = \"block\";";
        var markerIndex = html.IndexOf(marker, StringComparison.Ordinal);
        if (markerIndex < 0)
        {
            throw new BuildFailedException("Unable to locate Unity loading block in generated index.html.");
        }

        var scriptEndIndex = html.IndexOf("\n    </script>", markerIndex, StringComparison.Ordinal);
        if (scriptEndIndex < 0)
        {
            throw new BuildFailedException("Unable to locate Unity script terminator in generated index.html.");
        }

        var replacement = @"      document.querySelector('#unity-loading-bar').style.display = 'block';

      async function decompressGzipFile(url, mimeType) {
        const response = await fetch(url);
        if (!response.ok) {
          throw new Error('Unable to load ' + url + ' (HTTP ' + response.status + ')');
        }

        const compressedData = await response.arrayBuffer();
        if (!('DecompressionStream' in window)) {
          throw new Error('This browser does not support gzip decompression for Unity WebGL assets.');
        }

        const decompressedStream = new Blob([compressedData]).stream().pipeThrough(new DecompressionStream('gzip'));
        const decompressedData = await new Response(decompressedStream).arrayBuffer();
        return URL.createObjectURL(new Blob([decompressedData], { type: mimeType }));
      }

      (async () => {
        const dataUrl = await decompressGzipFile(buildUrl + '/docs.data.gz', 'application/octet-stream');
        const frameworkUrl = await decompressGzipFile(buildUrl + '/docs.framework.js.gz', 'application/javascript');
        const codeUrl = await decompressGzipFile(buildUrl + '/docs.wasm.gz', 'application/wasm');

        config.dataUrl = dataUrl;
        config.frameworkUrl = frameworkUrl;
        config.codeUrl = codeUrl;

        var script = document.createElement('script');
        script.src = loaderUrl;
        script.onload = () => {
          createUnityInstance(canvas, config, (progress) => {
            document.querySelector('#unity-progress-bar-full').style.width = 100 * progress + '%';
          }).then((unityInstance) => {
            document.querySelector('#unity-loading-bar').style.display = 'none';
            document.querySelector('#unity-fullscreen-button').onclick = () => {
              unityInstance.SetFullscreen(1);
            };
          }).catch((message) => {
            alert(message);
          });
        };

        document.body.appendChild(script);
      })().catch((message) => {
        alert(message);
      });
";

        html = html.Substring(0, markerIndex) + replacement + html.Substring(scriptEndIndex);
        File.WriteAllText(indexPath, html);
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

        SceneManager.SetActiveScene(scene);
    }
}
