using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class CoinCatcherWebBuild
{
    private const string ScenePath = "Assets/Scenes/GameScene.unity";
    public const string DefaultOutputDir = "Builds/WebGL";

    [MenuItem("Coin Catcher/Build WebGL")]
    public static void BuildWebGLMenu()
    {
        ExecuteBuild(DefaultOutputDir);
    }

    public static void BuildForWeb()
    {
        ExecuteBuild(DefaultOutputDir);
    }

    public static void BuildForDocs()
    {
        ExecuteBuild("docs");
    }

    public static void ExecuteBuild(string outputPath)
    {
        Debug.Log("[CoinCatcherWebBuild] Starting WebGL Build...");

        if (!File.Exists(ScenePath))
        {
            Debug.LogError("[CoinCatcherWebBuild] Scene missing: " + ScenePath);
            EditorApplication.Exit(1);
            return;
        }

        // Configure WebGL player settings for optimal web and GitHub Pages compatibility
        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled; // Disabled avoids gzip/brotli MIME issues on GitHub Pages
        PlayerSettings.WebGL.decompressionFallback = true;
        PlayerSettings.defaultWebScreenWidth = 960;
        PlayerSettings.defaultWebScreenHeight = 600;
        PlayerSettings.runInBackground = false;
        PlayerSettings.productName = "Coin Catcher";
        PlayerSettings.companyName = "Sachit Yadav";

        if (Directory.Exists(outputPath))
        {
            try { Directory.Delete(outputPath, true); } catch { }
        }
        Directory.CreateDirectory(outputPath);

        var buildOptions = new BuildPlayerOptions
        {
            scenes = new[] { ScenePath },
            locationPathName = outputPath,
            target = BuildTarget.WebGL,
            options = BuildOptions.None
        };

        BuildReport report = BuildPipeline.BuildPlayer(buildOptions);
        BuildSummary summary = report.summary;

        if (summary.result == BuildResult.Succeeded)
        {
            // Add .nojekyll so GitHub Pages serves all WebGL files directly
            File.WriteAllText(Path.Combine(outputPath, ".nojekyll"), "");

            Debug.Log($"[CoinCatcherWebBuild] WebGL Build SUCCEEDED: {summary.totalSize} bytes in {summary.totalTime.TotalSeconds:F1}s at {outputPath}");
            EditorApplication.Exit(0);
        }
        else
        {
            Debug.LogError($"[CoinCatcherWebBuild] WebGL Build FAILED with {summary.totalErrors} errors.");
            EditorApplication.Exit(1);
        }
    }
}
