using System.Collections;
using Snobal.CloudSDK;
using Snobal.DesignPatternsUnity_0_0.Extensions;
using TMPro;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

/// <summary>
/// An editor only state used for testing. This will use config data to pair the device to the server
/// </summary>
public class StateEditorPair : StateBase
{
    [SerializeField]
    private TextMeshProUGUI text;

    private bool pairingSuccess;

    public override void onEnter()
    {
        base.onEnter();

        // Pairing only needs to occur in the editor, or as a standalone app.
        // When used from within Snobal, the pairing will be passed through json via the deviceID
        
        if (SC.IsPaired && SC.Settings.Values.DeviceToken != "")
        {
            // These are for example
            var photonRoomCode = AndroidExternalParameters.GetParameter(ExternalParameterType.PhotonRoomCode);
            var roomRegion = AndroidExternalParameters.GetParameter(ExternalParameterType.RoomRegion);
            var tenantURL = AndroidExternalParameters.GetParameter(ExternalParameterType.TenantURL);
            var userName = AndroidExternalParameters.GetParameter(ExternalParameterType.UserName);
            var avatarID = AndroidExternalParameters.GetParameter(ExternalParameterType.AvatarID);
           
            Snobal.Library.Logger.Log($"photonRoomCode:{photonRoomCode}");
            Snobal.Library.Logger.Log($"roomRegion:{roomRegion}");
            Snobal.Library.Logger.Log($"tenantURL:{tenantURL}");
            Snobal.Library.Logger.Log($"userName:{userName}");
            Snobal.Library.Logger.Log($"userName:{avatarID}");

            
            Debug.Log("Device is already paired".Colorize(Color.magenta));
            sceneControl.TriggerEvent(ExampleAppTriggers.Next);
            return;
        }


#if UNITY_EDITOR
        AttemptPair();
#else
        NotPaired();
#endif
    }

    /// <summary>
    /// Attempt to pair device with server
    /// </summary>
    private void NotPaired()
    {
        text.text =
            $"Device Not Paired.\nPlease ensure you're device is paired with Snobal Cloud before launching this app";
    }


    private void AttemptPair()
    {
        // Attempt to pair using the code stored in ExampleAppSettings
        var pairingCode = sceneControl.exampleAppSettings.editorPairingCode;
        text.text = $"Pairing - Attempting to pair with code {pairingCode}";

        // All of the methods on Snobal Cloud that perform any kind of networking intentionally block the thread calling them. 
        // It is the responsibility of the implementation to prevent this from causing lag spikes
        // Here is an example of a blocking method being inserted into our custom taskWatcher class to solve this exact problem
        SnobalCloudMonobehaviour.TaskWatcher.Watch(
            System.Threading.Tasks.Task.Run
            (
                () =>
                {
                    // NOTE: This is a seperate thread. Unity functions may only be called on the main thread.
                    pairingSuccess = SC.Pair(pairingCode);
                }
            ),
            () =>
            {
                // Called when the thread is done. Back on the main thread.
                PairingTaskComplete();
            }
        );
    }

    /// <summary>
    /// Start a coroutine to display pairing results.
    /// </summary>
    private void PairingTaskComplete()
    {
        StopRunningCoroutine();
        coroutine = PairedCompleteSequence();
        StartCoroutine(coroutine);
    }

    /// <summary>
    /// Pairing call is complete, now evaluate result
    /// </summary>
    /// <returns></returns>
    private IEnumerator PairedCompleteSequence()
    {
        // todo fail conditions need better handling
        yield return new WaitForSeconds(0.5f);
        if (!pairingSuccess)
        {
            text.text = "Pairing Error. Invalid pairing code";
            yield return new WaitForSeconds(0.5f);
            yield break;
        }

        text.text = "Pairing complete";
        yield return new WaitForSeconds(1.0f);
        sceneControl.TriggerEvent(ExampleAppTriggers.Next);

#if UNITY_EDITOR
        // Because of the need for editor development, the deviceId is stored between sessions while developing.
        // In the final app the device ID will be passed through from Snobal Sync
        // Set the scriptable object dirty so that it saves the device ID,
        // otherwise another pairing code needs to be generated.
        EditorUtility.SetDirty(sceneControl.exampleAppSettings);
#endif
    }
}