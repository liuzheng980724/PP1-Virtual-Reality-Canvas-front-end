using UnityEngine;
using UnityEditor;
using UnityEditor.Build.Reporting;
using System;
using UnityEditor.XR.Management;
using UnityEditor.XR.Management.Metadata;
using System.IO;

/// <summary>
/// BUILD NOTES:
/// 
/// Trying to switch platforms, then building without restarting Unity is causing a race condition issues with some machines failing (our AWS build macvhine for one) builds because the plugin is 
/// not changed before the build is started, and the build fails.
/// 
/// By doing this in 2 stages we are avoiding this issue. ie the buildAll.bat calls:
/// 
/// Unity SnobalBuild.SetPlatformPicoAndroid        - This starts Unity, switchs platforms, then closes.
/// Unity SnobalBuild.BuildProductionPicoAndroid    - This builds & creates the APK.
/// 
/// Maybe this will be fixed with updated versions of either Unity/XR Management Plugin or with the Pico SDK. Wave platform didn't have this isssue.
/// 
/// </summary>


public class SnobalBuild : MonoBehaviour
{
    protected static Platform currentPlatform { get; private set; }

    private static SnobalBuildSettings currentSettings = null;

    public enum Platform
    {
        Windows,
        Pico,
        Wave
    }

    static string[] getAllScenes()
    {
        int sceneCount = UnityEngine.SceneManagement.SceneManager.sceneCountInBuildSettings;     
        string[] scenes = new string[sceneCount];
        for( int i = 0; i < sceneCount; i++ )
        {
            scenes[i] = UnityEngine.SceneManagement.SceneUtility.GetScenePathByBuildIndex(i);
        }
        return scenes;
    }

    /// <summary>
    /// Disable the XR plugin for the given platform
    /// </summary>
    /// <param name="newPlatform"></param>
    static private void DisableAndroidXRPlugin(Platform newPlatform)
    {
		var androidGenericSettings = XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(BuildTargetGroup.Android);
		if (androidGenericSettings == null)
			Debug.LogError("androidGenericSettings = null");

        SnobalBuildSettings buildSettings = LoadBuildSettings(newPlatform);
        XRPackageMetadataStore.RemoveLoader(androidGenericSettings.AssignedSettings, buildSettings.XRLoaderHelperName, BuildTargetGroup.Android);
    }

    static private void DisableAllAndroidXRPlugins()
    {
        Debug.Log("Disabling all build setting XR plugins");
        for (int i= 0; i < Enum.GetNames(typeof(Platform)).Length; i++)
        {
            DisableAndroidXRPlugin((Platform)i);
        }
    }


    /// <summary>
    /// This enables the required plugin for the given platform. It disables all others.
    /// </summary>
    private static void SetAndroidXRPlugin(SnobalBuildSettings currentSettings)
    {
		var androidGenericSettings = XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(BuildTargetGroup.Android);
		if (androidGenericSettings == null)
			Debug.LogError("androidGenericSettings = null");

        // Assign new loader
        androidGenericSettings.InitManagerOnStart = true;
        if(!XRPackageMetadataStore.AssignLoader(androidGenericSettings.AssignedSettings, currentSettings.XRLoaderHelperName, BuildTargetGroup.Android))
        {
            // failed to assign
            Debug.LogError("Failed to assign XR plugin management plugin '" + currentSettings.XRLoaderHelperName + "'");
        }
        else
        {
            Debug.Log("Snobal Build: New XR Plugin assigned " + currentSettings.XRLoaderHelperName);
        }

        EditorUtility.SetDirty(androidGenericSettings);
    }


    /// <summary>
    /// This copies the required manifest file into the root "Assets/Plugins/Android" folder.
    /// NOTE: the wave SDK has a custom build pre-build step which will copy it's manifest file into this folder, but only if it doesn't already exist.
    /// </summary>
    /// <param name="newPlatform"></param>
    static void SetAndroidManifestFile(SnobalBuildSettings currentSettings)
    {
        // Create root anrdoid plugin folder
		const string PluginAndroidPath = "Assets/Plugins/Android";
		if (!Directory.Exists(PluginAndroidPath))
			Directory.CreateDirectory(PluginAndroidPath);

		const string AndroidManifestPathDest = "Assets/Plugins/Android/AndroidManifest.xml";
		File.Copy(currentSettings.AndroidManifestPath, AndroidManifestPathDest, true);

        Debug.Log("Snobal Build: New Android manifest copied " + currentSettings.AndroidManifestPath);
    }

    static private SnobalBuildSettings LoadBuildSettings(Platform newPlatform)
    {
        // Find the build assets based on the enum
        string platformStr = Enum.GetName(typeof(Platform), newPlatform);

        string buildSettingName = "BuildSettings" + platformStr;
        string[] guids = AssetDatabase.FindAssets(buildSettingName);

        if(guids.Length != 1)
        {
            Debug.LogError("Looking for build settings " + buildSettingName + ", found none or multiple");
        }
        else
        {
            string buildSettingsAsset = AssetDatabase.GUIDToAssetPath(guids[0]);

            // load this buildsettings scriptable object
            SnobalBuildSettings settings = AssetDatabase.LoadAssetAtPath(buildSettingsAsset, typeof(SnobalBuildSettings)) as SnobalBuildSettings;
            Debug.Assert(settings != null, "Can't find build settings for platform '" + newPlatform + "', path = " + buildSettingsAsset);

            return settings;
        }
        return null;
    }

    static void SetPlatform(Platform newPlatform)
    {
        // Disable all platforms
        DisableAllAndroidXRPlugins();

        currentSettings = LoadBuildSettings(newPlatform);

        if(currentSettings != null)
        {
            if(!string.IsNullOrEmpty(currentSettings.XRLoaderHelperName))
            {
                SetAndroidXRPlugin(currentSettings);
            }

            if (!string.IsNullOrEmpty(currentSettings.AndroidManifestPath))
            {
                SetAndroidManifestFile(currentSettings);
            }

            currentPlatform = newPlatform;
        }
    }

    [MenuItem("Snobal/Build/SetPlatform - Pico Android")]
    public static void SetPlatformPicoAndroid()
    {
        SetPlatform(Platform.Pico);
    }

    [MenuItem("Snobal/Build/SetPlatform - Wave Android")]
    public static void SetPlatformWaveAndroid()
    {
        SetPlatform(Platform.Wave);
    }

    [MenuItem("Snobal/Build/SetAndBuildProduction - Pico Android")]
    static void SetAndBuildProductionPicoAndroid()
    {
        SetPlatform(Platform.Pico);
        BuildProductionAndroid();
    }

    [MenuItem("Snobal/Build/SetAndBuildProduction - Wave Android")]
    static void SetAndBuildProductionWaveAndroid()
    {
        SetPlatform(Platform.Wave);
        BuildProductionAndroid();
    }

    static void BuildProductionPicoAndroid()
    {
        currentSettings = LoadBuildSettings(Platform.Pico);
        BuildProductionAndroid();
    }

    static void BuildProductionWaveAndroid()
    {
        currentSettings = LoadBuildSettings(Platform.Wave);
        BuildProductionAndroid();
    }

    static void BuildProductionWindows()
    {
        SetPlatform(Platform.Windows);

        // Disable the stack trace on logs (not errors or warnings), makes the log file output bloated.
        Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);

        Debug.Log("---------------------------------------------------------------------------");
        Debug.Log("- BuildProductionWindows");
        Debug.Log("---------------------------------------------------------------------------");
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = new[] { "Assets/Scene/Main.unity" };
        buildPlayerOptions.locationPathName = currentSettings.outputPath;
        buildPlayerOptions.target = BuildTarget.StandaloneWindows64;
        buildPlayerOptions.options = BuildOptions.None;
        BuildPipeline.BuildPlayer(buildPlayerOptions);
    }

    public static void BuildProductionAndroid()
    {
        // Doesn't display well on output
        //Debug.Log("---------------------------------------------------------------------------");
        //Debug.Log("- BuildProductionAndroid");
        //Debug.Log("---------------------------------------------------------------------------");

        string versionNumber = Environment.GetEnvironmentVariable("FULLVERSION");
        string query = "%AndroidKeystorePassword%";
        string keystorePassword = Environment.ExpandEnvironmentVariables(query);

        if (string.IsNullOrEmpty(versionNumber))
            versionNumber = "1.0.0.0";
        PlayerSettings.bundleVersion = versionNumber;

        string keystorePath = System.IO.Directory.GetCurrentDirectory() + "/../" + "snobal.keystore"; // It ain't pretty but it works
        Debug.Log(keystorePath);
        PlayerSettings.Android.keystoreName = keystorePath;
        PlayerSettings.Android.keystorePass = keystorePassword;
        PlayerSettings.Android.keyaliasPass = keystorePassword;

        // Disable the stack trace on logs (not errors or warnings), makes the log file output bloated.
        Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);

        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = getAllScenes();
        buildPlayerOptions.locationPathName = currentSettings.outputPath;
        buildPlayerOptions.target = BuildTarget.Android;
        buildPlayerOptions.options = BuildOptions.None;
        BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
        BuildSummary summary = report.summary;
        if (summary.result == BuildResult.Succeeded) {
            Debug.Log("Build succeeded: " + summary.totalSize + " bytes");
        } else {
            Debug.Log("Build failed");
        }
    }

}
