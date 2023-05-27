using System.Collections.Generic;
using UnityEngine;

namespace Snobal.CameraRigSystems_0_0
{
    // MK: this is for editor/local build testing of different camera rigs, hand controllers etc
    public class CameraRigInstantiation : MonoBehaviour
    {
        public enum LoadRigBehaviour { Managed, LoadedInScene }

        [SerializeField]
        private LoadRigBehaviour loadCameraRig = LoadRigBehaviour.Managed;

        [Header("Properties")]
        [SerializeField]
        bool setRigDontDestroyOnLoad = true;

        [Header("Camera Rig Prefabs")]
        [SerializeField]
        private CameraRig editorEmulatedCameraRigPrefab;

        [SerializeField]
        private List<CameraRigInfo> cameraRigPrefabRegister = new List<CameraRigInfo>()
        {   
            new CameraRigInfo("Pico Pico Neo 3"),
            new CameraRigInfo("HTC VIVE Focus 3")
        };

        void Awake()
        {
            // camera rig must be manually placed in the scene
            if (loadCameraRig == LoadRigBehaviour.LoadedInScene)
                return;

            switch (Application.platform)
            {
                case RuntimePlatform.OSXEditor:
                case RuntimePlatform.OSXPlayer:
                case RuntimePlatform.WindowsPlayer:
                case RuntimePlatform.WindowsEditor:
                    SpawnCameraRig(editorEmulatedCameraRigPrefab);
                    break;
                case RuntimePlatform.Android:
                    ValidateAndroidCameraRig();
                    break;                                    
                default:
                    throw new UnityException("Platform not supported");
            }

            Destroy(this.gameObject);
        }

        void ValidateAndroidCameraRig()
        {
            var rigInfo = cameraRigPrefabRegister.Find(i => i.DeviceModelName == SystemInfo.deviceModel);

            if (rigInfo == null)
                throw new UnityException("CameraRigInstantiation: registered device model not found");

            if (rigInfo.CameraRigPrefab == null)
                throw new UnityException("CameraRigInstantiation: registered device model found but camera rig prefab is null");

            SpawnCameraRig(rigInfo.CameraRigPrefab);            
        }

        void SpawnCameraRig(CameraRig prefab)
        {
            var cameraRig = GameObject.FindObjectOfType<CameraRig>();

            if (cameraRig != null)
            {
                Debug.Log("Found camera rig in scene during camera rig instantiation, destroying scene camera rig");
                Destroy(cameraRig.gameObject);
            }

            if (prefab == null)
                throw new UnityException("CameraRigInstantiation SpawnCameraRig: Prefab is null");

            cameraRig = Instantiate(prefab);

            cameraRig.name = prefab.name;

            if (setRigDontDestroyOnLoad)
                DontDestroyOnLoad(cameraRig.gameObject);
        }

        [System.Serializable]
        public class CameraRigInfo
        {
            public CameraRigInfo(string _deviceName)
            {
                deviceModelName = _deviceName;
            }

            public string DeviceModelName => deviceModelName;
            public CameraRig CameraRigPrefab => cameraRigPrefab;

            [SerializeField]
            string deviceModelName;
            [SerializeField]
            CameraRig cameraRigPrefab;
        }
    }
}

