using Snobal.Library;
using System.Collections.Generic;
using UnityEngine;

public static class AndroidUtils
{
    /// <summary>
    /// Is this package installed on the current device.
    /// </summary>
    public static bool IsAndroidPackageInstalled(string packageName)
    {
        bool isInstalled = false;

        Snobal.Library.Logger.Log("Checking if package " + packageName + " is installed");

#if !UNITY_EDITOR && UNITY_ANDROID
        using AndroidJavaClass up = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        using AndroidJavaObject ca = up.GetStatic<AndroidJavaObject>("currentActivity");
        using AndroidJavaObject packageManager = ca.Call<AndroidJavaObject>("getPackageManager");
        using AndroidJavaObject launchIntent =
 packageManager.Call<AndroidJavaObject>("getLaunchIntentForPackage",packageName);
        isInstalled = launchIntent != null;
#endif

        Snobal.Library.Logger.Log("Package " + packageName + " installed - " + isInstalled.ToString());

        return isInstalled;
    }

    public static bool IsMDMInstalled()
    {
        return IsAndroidPackageInstalled("com.snobal.clouddeploy") ||
               IsAndroidPackageInstalled("net.snobal.sync");
    }

    /// <summary>
    /// Launch an external app.
    /// 
    /// Note: This doesn't kill the current appilcation, when you exit the launchec app you will return to this one. If you dont want to return, then launch and external app
    /// and quit appilcation.
    /// Note: Launching an appilcation which is currently running doesn't seem to work.
    /// </summary>
    public static bool LaunchAndroidApp(string packageName, Dictionary<string, string> extraArguments = null)
    {
#if !UNITY_EDITOR && UNITY_ANDROID

        Snobal.Library.Logger.Log("LaunchAndroidApp: Launching external app - " + packageName);

        bool fail = false;

        using AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        using AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        using AndroidJavaObject packageManager = currentActivity.Call<AndroidJavaObject>("getPackageManager");
        using AndroidJavaObject launchIntent = packageManager.Call<AndroidJavaObject>("getLaunchIntentForPackage", packageName);

        try
        {
            if (extraArguments != null)
            {
                Snobal.Library.Logger.Log("LaunchAndroidApp: Passing extra arguments into external app");

                foreach (KeyValuePair<string, string> kvp in extraArguments)
                {
                    launchIntent.Call<AndroidJavaObject>("putExtra", kvp.Key, kvp.Value);
                    Snobal.Library.Logger.Log("Key = " + kvp.Key + ", Value = " + kvp.Value);
                }
            }
        }
        catch (System.Exception e)
        {
            fail = true;
        }

        if (fail)
            Snobal.Library.Logger.Log("MenuLaunchApplication, method LaunchApp: Failed");
        else
            currentActivity.Call("startActivity", launchIntent);

        return true;
#else
        return false;
#endif
    }


    /// <summary>
    /// Returns the intent data passed in from sphere to the external app.
    /// External apps can use this to get the data passed in.
    /// Usage:
    ///  AndroidUtils.GetExtraArgumentValue("RoomCode")
    /// </summary>
    public static string GetExtraArgumentValue(string argumentKey)
    {
#if !UNITY_EDITOR && UNITY_ANDROID

        AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        AndroidJavaObject intent = currentActivity.Call<AndroidJavaObject>("getIntent");
        AndroidJavaObject extras = GetExtras(intent);

        if (extras == null)
            return string.Empty;

        return GetProperty(extras, argumentKey);
#endif
        return null;
    }

    public static bool InstallAPK(string apkPath)
    {
        bool success = true;
        try
        {
            using AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            using AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            using AndroidJavaObject unityContext = currentActivity.Call<AndroidJavaObject>("getApplicationContext");
            
            string packageName = unityContext.Call<string>("getPackageName");
            string authority = packageName + ".provider";

            Debug.Log($"Attempting to install {packageName}");

            AndroidJavaClass intentClass = new AndroidJavaClass ("android.content.Intent");
            string ACTION_VIEW = intentClass.GetStatic<string>("ACTION_VIEW");
            AndroidJavaObject intentObject = new AndroidJavaObject("android.content.Intent", ACTION_VIEW);
            
            int FLAG_GRANT_READ_URI_PERMISSION = intentClass.GetStatic<int>("FLAG_GRANT_READ_URI_PERMISSION");
            int FLAG_GRANT_WRITE_URI_PERMISSION = intentClass.GetStatic<int>("FLAG_GRANT_WRITE_URI_PERMISSION");
            string EXTRA_RETURN_RESULT = intentClass.GetStatic<string>("EXTRA_RETURN_RESULT");
            int FLAG_ACTIVITY_NO_HISTORY = intentClass.GetStatic<int>("FLAG_ACTIVITY_NO_HISTORY");
            int FLAG_ACTIVITY_NEW_DOCUMENT = intentClass.GetStatic<int>("FLAG_ACTIVITY_NEW_DOCUMENT");

            using AndroidJavaObject fileObj = new AndroidJavaObject("java.io.File", apkPath);
            using AndroidJavaClass fileProvider = new AndroidJavaClass("androidx.core.content.FileProvider");

            object[] providerParams = new object[3];
            providerParams[0] = unityContext;
            providerParams[1] = authority;
            providerParams[2] = fileObj;
            
            using AndroidJavaObject uri = fileProvider.CallStatic<AndroidJavaObject>("getUriForFile", providerParams);
            
            intentObject.Call<AndroidJavaObject>("setDataAndType", uri, "application/vnd.android.package-archive");
            intentObject.Call<AndroidJavaObject>("addFlags", FLAG_ACTIVITY_NEW_DOCUMENT);
            intentObject.Call<AndroidJavaObject>("addFlags", FLAG_ACTIVITY_NO_HISTORY); 
            intentObject.Call<AndroidJavaObject>("addFlags", FLAG_GRANT_READ_URI_PERMISSION | FLAG_GRANT_WRITE_URI_PERMISSION);
            intentObject.Call<AndroidJavaObject>("putExtra", EXTRA_RETURN_RESULT, true);
            currentActivity.Call("startActivityForResult", intentObject, 100);
        }
        catch (System.Exception e)
        {
            success = false;
        }

        return success;
    }

    public static bool UninstallAPK(string packageName)
    {
        bool success = true;
        try
        {
            using AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            using AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            using AndroidJavaObject unityContext = currentActivity.Call<AndroidJavaObject>("getApplicationContext");

            using AndroidJavaClass uriClass = new AndroidJavaClass("android.net.Uri");
            using AndroidJavaObject uriObject =
                uriClass.CallStatic<AndroidJavaObject>("fromParts", "package", packageName, null);

            AndroidJavaClass intentClass = new AndroidJavaClass ("android.content.Intent");
            string ACTION_DELETE = intentClass.GetStatic<string>("ACTION_DELETE");
            AndroidJavaObject intentObject = new AndroidJavaObject("android.content.Intent", ACTION_DELETE, uriObject);
            int FLAG_ACTIVITY_NEW_DOCUMENT = intentClass.GetStatic<int>("FLAG_ACTIVITY_NEW_DOCUMENT");
            int FLAG_ACTIVITY_NO_HISTORY = intentClass.GetStatic<int>("FLAG_ACTIVITY_NO_HISTORY");
            intentObject.Call<AndroidJavaObject>("addFlags", FLAG_ACTIVITY_NEW_DOCUMENT);
            intentObject.Call<AndroidJavaObject>("addFlags", FLAG_ACTIVITY_NO_HISTORY); 

            currentActivity.Call("startActivityForResult", intentObject, 101);
        }
        catch (System.Exception e)
        {
            success = false;
        }

        return success;
    }
    public static bool CheckForInstallPermissions()
    {
        try
        {
            using AndroidJavaClass unityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            using AndroidJavaObject currentActivity = unityClass.GetStatic<AndroidJavaObject>("currentActivity");
            using AndroidJavaObject packageManager = currentActivity.Call<AndroidJavaObject>("getPackageManager");
            return packageManager.Call<bool>("canRequestPackageInstalls");
        }
        catch (System.Exception e)
        {
            Snobal.Library.Logger.Log(e.Message);
            return false;
        }
    }

    public static void PromptInstallPermissions()
    {
        try
        {
            using AndroidJavaClass unityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            using AndroidJavaObject currentActivity = unityClass.GetStatic<AndroidJavaObject>("currentActivity");
            string packageName = currentActivity.Call<string>("getPackageName");
            using AndroidJavaClass uriClass = new AndroidJavaClass("android.net.Uri");
            using AndroidJavaObject uriObject =
                uriClass.CallStatic<AndroidJavaObject>("fromParts", "package", packageName, null);
            using AndroidJavaObject intentObj = new AndroidJavaObject("android.content.Intent",
                "android.settings.MANAGE_UNKNOWN_APP_SOURCES", uriObject);

            currentActivity.Call("startActivityForResult", intentObj, 102);
        }
        catch (System.Exception e)
        {
            Snobal.Library.Logger.Log(e.Message);
        }
    }

    private static AndroidJavaObject GetExtras(AndroidJavaObject intent)
    {
        AndroidJavaObject extras = null;

        try
        {
            extras = intent.Call<AndroidJavaObject>("getExtras");
        }
        catch (System.Exception e)
        {
            Snobal.Library.Logger.Log(e.Message);
        }

        return extras;
    }

    private static string GetProperty(AndroidJavaObject extras, string name)
    {
        string s = string.Empty;

        try
        {
            s = extras.Call<string>("getString", name);
        }
        catch (System.Exception e)
        {
            Snobal.Library.Logger.Log(e.Message);
        }

        return s;
    }
}