using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class BuildAutomator
{
    [MenuItem("Build/Android")]
    public static void Build()
    {
        PlayerSettings.Android.useCustomKeystore = true;
        PlayerSettings.Android.keystoreName = "user.keystore";
        PlayerSettings.Android.keystorePass = "123456";
        PlayerSettings.Android.keyaliasName = "user";
        PlayerSettings.Android.keyaliasPass = "123456";
        
        BuildPlayerOptions options = new BuildPlayerOptions();
        /*options.scenes = new[]
        {
            "Assets/04.Scenes/LoginScene",
            "Assets/04.Scenes/LoadingScene",
            "Assets/04.Scenes/MainScene",
            "Assets/04.Scenes/InGameScene/GameScene_Map1",
            "Assets/04.Scenes/InGameScene/GameScene_Map2",
            "Assets/04.Scenes/InGameScene/GameScene_Map3",
            "Assets/04.Scenes/InGameScene/GameScene_Map4"
        };*/
        EditorUserBuildSettings.buildAppBundle = true;
        options.scenes = GetEnabledScenes();
        options.locationPathName = "D:\\Build\\ChickenRun\\ChickenRun.aab";
        options.target = BuildTarget.Android;
        options.options = BuildOptions.None;
        
        BuildReport report = BuildPipeline.BuildPlayer(options);
        
        if (report.summary.result == BuildResult.Succeeded)
        {
            Debug.Log("build succeeded");
        }
        else
        {
            Debug.LogError("build failed: " + report.summary.result);
        }
    }

    private static string[] GetEnabledScenes()
    {
        System.Collections.Generic.List<string> scenes = new System.Collections.Generic.List<string>();
        foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
        {
            if (scene.enabled)
            {
                scenes.Add(scene.path);
            }
        }
        return scenes.ToArray();
    }
}