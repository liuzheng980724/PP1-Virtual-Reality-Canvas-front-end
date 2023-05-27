using UnityEngine;

/// <summary>
/// Build settings for this platform.
/// 
/// Want to add a new platform?
/// 1) Add the new enum to SnobalBuild.Platform
/// 2) Create a new scriptable object of this class, name it the same as the enum (uses the enum name to find the scriptable to load it)
/// 3) Fill it in with the required data
/// 4) Add menu items like SnobalBuild.SetPlatformPicoAndroid & SnobalBuild.BuildProductionPicoAndroid. 
/// 5) Switch to the new platform using the menu item. Check the manifest file is correct (if Android) & the XR Plugin's look good.
/// 6) Add the new menu item to build into the buildall.bat script so the build machine will build it.
/// </summary>

[CreateAssetMenu(fileName = "BuildSettings", menuName = "Snobal/Build/Create build settings", order = 1)]
public class SnobalBuildSettings : ScriptableObject
{
    [Tooltip ("File & path name to copy into the Assets/Plugins/Android folder when using this platform. Blank to skip.")]
    [SerializeField] public string AndroidManifestPath;

    [Tooltip ("File & path name for the build output for this platform")]
    [SerializeField] public string outputPath;

    [Tooltip ("The name of the XRLoaderHelper used in the XR Management plugin. Enabled when using this platform. Blank to skip.")]
    [SerializeField] public string XRLoaderHelperName;
}
