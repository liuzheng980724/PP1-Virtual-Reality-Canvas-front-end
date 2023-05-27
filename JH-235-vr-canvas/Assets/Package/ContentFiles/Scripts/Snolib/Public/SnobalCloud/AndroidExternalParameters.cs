// todo namespace

using System;
using Snobal.Library;

/// <summary>
/// Evaluate the android 'intents' (etc) being passed through to the app
/// </summary>
public class AndroidExternalParameters
{
    /// <summary>
    /// Intents can 'cleared' or disabled so that 
    /// </summary>
    private static bool _androidIntentsConsumed = false;

    public static void SetAndroidIntentsConsumed()
    {
        Logger.Log("Set Android Intents to consumed");
        _androidIntentsConsumed = true;
    }
    
    public static string GetParameter(ExternalParameterType value)
    {
        if (_androidIntentsConsumed)
        {
            return null;
        }
        var parameterTypeToString = ParameterTypeToString(value);
        var returnString = AndroidUtils.GetExtraArgumentValue(parameterTypeToString);
        return returnString;
    }

    public static string ParameterTypeToString(ExternalParameterType type)
    {
        return Enum.GetName(typeof(ExternalParameterType), type);
    }
    
}

public class RequiredExternalParamerter
{
    public string photonRoomCode;
    public string networkRegion;
    public string userDisplayName;
    public string userAvatarId;
    public string userAvatarUrl;
    public string returnToRoom;
}

public enum ExternalParameterType
{
    Participant,
    Settings,
    TenantURL,
    PhotonRoomCode,
    RoomRegion,
    UserName,
    AvatarID,
    ReturnToRoomName,
    AvatarUrl
}