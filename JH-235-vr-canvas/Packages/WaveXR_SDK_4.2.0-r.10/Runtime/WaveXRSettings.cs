using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Management;
using static Wave.XR.Constants;

namespace Wave.XR.Settings
{
    [XRConfigurationData("WaveXRSettings", k_SettingsKey)]
    [System.Serializable]
    public class WaveXRSettings : ScriptableObject
    {
        static WaveXRSettings s_RuntimeInstance = null;

        public enum StereoRenderingPath
        {
            MultiPass,
            SinglePass
        }

        public enum AdaptiveQualityMode
        {
            Disabled,
            QualityOrientedMode,
            PerformanceOrientedMode,
            CustomizationMode
        }
        public enum FoveationMode
        {
            Disable,
            Enable,
            Default
        }

        public enum FoveatedRenderingPeripheralQuality
        {
            Low,
            Middle,
            High,
        }

        public enum AMCMode
        {
            Off,
            Force_UMC,
            Auto,
            Force_PMC
        }

        public enum DisplayGamut
        {
            Native,
            sRGB,
            DisplayP3
        }

		public enum SupportedFPS
		{
			HMD_Default,
			_120
		}

        [SerializeField, Tooltip("In URP, only SinglePass can be used.")]
        public StereoRenderingPath preferedStereoRenderingPath = StereoRenderingPath.SinglePass;

        [SerializeField, Tooltip("Use Double Width texture.  On Android, only MultiPass can have double width texture.")]
        public bool useDoubleWidth = false;

        [SerializeField, Tooltip("Enable RenderMask (Occlusion Mesh).")]
        public bool useRenderMask = true;

        public enum TimeWarpStabilizedMode
        {
            Off,
            On,
            Auto,
        }

        [SerializeField, Tooltip("If you select On, it will reduce jitter in some case. Please refer developer guide.")]
        public TimeWarpStabilizedMode enableTimeWarpStabilizedMode = TimeWarpStabilizedMode.Auto;

        [SerializeField, Tooltip("Adaptive Quality Mode.")]
        public AdaptiveQualityMode adaptiveQualityMode = AdaptiveQualityMode.PerformanceOrientedMode;

        [SerializeField, Tooltip("Allow set quality strategy is send quality event. SendQualityEvent = false if quality strategy use default.")]
        public bool AQ_SendQualityEvent = true;

        [SerializeField, Tooltip("Allow set auto foveation quality strategy. AutoFoveation = false if quality strategy disable foveation.")]
        public bool AQ_AutoFoveation = false;

        [SerializeField, Tooltip("Enable Dynamic Resolution based on Adaptive Quality event.")]
        public bool useAQDynamicResolution = true;

        [Tooltip("You can choose one of resolution scale from this list as a default resolution scale by setting the default index.")]
        [SerializeField]
        public int DR_DefaultIndex = 0;

        [Tooltip("The unit used for measuring text size here is dmm (Distance-Independent Millimeter). The method of conversion from Unity text size into dmm can be found in the documentation of the SDK.")]
        [SerializeField]
        [Range(20, 40)]
        public int DR_TextSize = 20;

        [SerializeField]
        public List<float> DR_ResolutionScaleList = new List<float>();

        [SerializeField, Tooltip("Set foveationMode.  Choose default mode will apply device's config.")]
        public FoveationMode foveationMode = FoveationMode.Default;

        [SerializeField, Tooltip("Set LeftClearVisionFOV")]
        [Range(1, 179)]
        public float leftClearVisionFOV = 38;

        [SerializeField, Tooltip("Set RightClearVisionFOV")]
        [Range(1, 179)]
        public float rightClearVisionFOV = 38;

        [SerializeField, Tooltip("Set LeftPeripheralQuality")]
        public FoveatedRenderingPeripheralQuality leftPeripheralQuality = FoveatedRenderingPeripheralQuality.High;

        [SerializeField, Tooltip("Set RightPeripheralQuality")]
        public FoveatedRenderingPeripheralQuality rightPeripheralQuality = FoveatedRenderingPeripheralQuality.High;


        [SerializeField, Tooltip("Enabled to override system's PixelDensity.  Disabled to use system's PixelDensity.")]
        public bool overridePixelDensity = false;

        [SerializeField, Tooltip("PixelDensity is a scale to the real display's width and height.  It is use to set a default eye buffer size.  The default eye buffer size will be calculated by (display w, display h) * pixelDensity.")]
        [Range(0.1f, 2)]
        public float pixelDensity = 1;

        [SerializeField, Tooltip("ResolutionScale is a scale to the default eye buffer size which is decided by PixelDensity.  Default is 1.  You can also use XRSettings.eyeTextureResolutionScale to configure it in the runtime.  The final eye buffer size will be calculated by (eye buffer w, eye buffer h) * resolutionScale.")]
        [Range(0.1f, 1)]
        public float resolutionScale = 1;

        [SerializeField, Tooltip("Debug log flag which native XR plugin should follow.")]
        public uint debugLogFlagForNative = (uint)(DebugLogFlag.BasicMask | DebugLogFlag.LifecycleMask | DebugLogFlag.RenderMask | DebugLogFlag.InputMask);

        [SerializeField, Tooltip("Debug log flag which Unity Player should follow.")]
        public uint debugLogFlagForUnity = (uint)(DebugLogFlag.BasicMask | DebugLogFlag.LifecycleMask | DebugLogFlag.RenderMask | DebugLogFlag.InputMask);

        [SerializeField, Tooltip("Override the LogFlag for native.")]
        public bool overrideLogFlagForNative = false;

        [SerializeField, Tooltip("Set the mode of Adaptive Motion Compensator (AMC).  You can choose to force enable one of Universal Motion Compensator (UMC) and Positional Motion Compensator (PMC).  Or use auto UMC.  Default is off")]
        public AMCMode amcMode = AMCMode.Off;

        [SerializeField, Tooltip("0. not accept, 1. accept, 2. accept and don't ask again")]
        public int amcModeConfirm = 0;

		[SerializeField, Tooltip("Set the App supported FPS. (Experimental function)\nHMD Default:\nApp will run with HMD default FPS.\n120 FPS:\nDeclare the App can support up to 120 FPS and the HMD display will switch to 120 FPS to run your App if it can support.\nNotice : You must to tune your App to make it suitable for running at 120FPS, or App might get worse rendering performance and cause jitter phenomenon.")]
		public SupportedFPS supportedFPS = SupportedFPS.HMD_Default;

		[SerializeField, Tooltip("Wave XR Feature Package folder location")]
		public string waveXRFolder = "Assets/Wave/XR";

		[SerializeField, Tooltip("Wave Essence Feature Package folder location")]
		public string waveEssenceFolder = "Assets/Wave/Essence";

		[SerializeField, Tooltip("Display gamut preferences.  Wave will try to apply gamut from the top of the list.  When the earier gamut is accepted, Wave will not try the next.  Not all gamut preference here can be accepted.  It's depended on device.  The sRGB is default acceptable for all device.")]
        public List<DisplayGamut> displayGamutPreferences = new List<DisplayGamut>() { DisplayGamut.Native, DisplayGamut.sRGB, DisplayGamut.DisplayP3 };

        [SerializeField, Tooltip("Select to enable auto fallback when using Multi-Layer (i.e. Layers that exceed the maximum layer count will be rendered in-game).")]
        private bool enableAutoFallbackForMultiLayer = true;

		#region Tracker
		[SerializeField, Tooltip("Select to enable the Tracker feature when AP starts.")]
		public bool EnableTracker = false;
		public const string EnableTrackerText = "EnableTracker";
		#endregion

		public bool enableAutoFallbackForMultiLayerProperty
        {
            get
            {
                return enableAutoFallbackForMultiLayer;
            }
        }

        void Awake()
        {
            Debug.Log("WaveXRSettings.Awake()");
#if UNITY_EDITOR
            if (Application.isEditor)
                return;
#endif
            s_RuntimeInstance = this;
        }

        public static WaveXRSettings GetInstance()
        {
#if UNITY_EDITOR
            if (Application.isEditor)
            {
                Object obj = null;
                UnityEditor.EditorBuildSettings.TryGetConfigObject(k_SettingsKey, out obj);
                if (obj == null || !(obj is WaveXRSettings))
                    return null;
                return (WaveXRSettings)obj;
            }
#endif
            if (s_RuntimeInstance == null)
                s_RuntimeInstance = new WaveXRSettings();
            return s_RuntimeInstance;
        }
    }
}
