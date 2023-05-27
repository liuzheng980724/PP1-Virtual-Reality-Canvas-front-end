using Snobal.Library;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.XR.PXR;
using UnityEngine;

public class PicoAppControlService : MonoBehaviour
{
    //Initializing and bind ToBService, the objectname refers to name of the object which is used to receive callback.
    private void Awake()
    {
        PXR_System.InitSystemService(name);
        PXR_System.BindSystemService();
    }

    private void OnDestroy()
    {
        PXR_System.UnBindSystemService();
    }

    public void toBServiceBind(string s) 
    {
        Snobal.Library.Logger.Log($"PicoAppControlService: binding success");
        Debug.Log("Bind success."); 
    }

    public static void Install(string apkfilePath)
    {
        Snobal.Library.Logger.Log($"Install apk from path: {apkfilePath} starting");

        PXR_System.ControlAPPManager(PackageControlEnum.PACKAGE_SILENCE_INSTALL, apkfilePath, (success) =>
            {
                Snobal.Library.Logger.Log($"Install apk from path: {apkfilePath}, status: {(success == 0 ? "Failed" : "Succeeded")}");
            });
    }

    public static void Uninstall(string packageName)
    {
        Snobal.Library.Logger.Log($"Uninstall application with package name: {packageName} starting");

        PXR_System.ControlAPPManager(PackageControlEnum.PACKAGE_SILENCE_UNINSTALL, packageName, (success) => {
                Snobal.Library.Logger.Log($"Uninstall application: {packageName}, status: {(success == 0 ? "Failed" : "Succeeded")}");
            });
    }
}