using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.Networking;
using Newtonsoft.Json;
using UnityEngine.SceneManagement;
using System.IO;
using TriLibCore;
using TriLibCore.General;



public class loadSomething : MonoBehaviour
{
    public Text objectName;
    public GameObject loadedObject;
    public string JsonString;
    public string userAccesstoken;
    public string requireCourseURL;
    public string objNameT;

    [Serializable]
    public class File
    {
        public int id;
        public string display_name;
        public string filename;
        public string url;
        public string updated_at;
    }

    // Start is called before the first frame update
    IEnumerator Start()
    {
        List<File> filesList = new List<File>();
        userAccesstoken = "9595~gNBaPQCzV32DkLkMzxGyCNzA0PjMVbElkcmtUbDi3i7NTaXnsjmu3ESl8abhGvLP";

        requireCourseURL = "https://rmit.instructure.com/api/v1/courses/70814/files?access_token=" + userAccesstoken;

        objectName.text = "Default List";

        using (UnityWebRequest www = UnityWebRequest.Get(requireCourseURL))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                objectName.text = www.error;
            }
            else
            {
                JsonString = www.downloadHandler.text;
                Debug.Log(JsonString);
                filesList = JsonConvert.DeserializeObject<List<File>>(JsonString);

                foreach (File thisFile in filesList)
                {
                    objNameT += thisFile.display_name + "\n";

                    //Load model
                    var assetLoaderOptions = AssetLoader.CreateDefaultLoaderOptions();
                    var webRequest = AssetDownloader.CreateWebRequest(thisFile.url);
                    AssetDownloader.LoadModelFromUri(webRequest, OnLoad, OnMaterialsLoad, OnProgress, OnError, null, assetLoaderOptions, null, null);
                }

                objectName.text = objNameT;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnProgress(AssetLoaderContext assetLoaderContext, float progress)
    {

    }

    // This event is called when there is any critical error loading your model.
    // You can use this to show a message to the user.
    private void OnError(IContextualizedError contextualizedError)
    {

    }

    // This event is called when all model GameObjects and Meshes have been loaded.
    // There may still Materials and Textures processing at this stage.
    private void OnLoad(AssetLoaderContext assetLoaderContext)
    {
        // The root loaded GameObject is assigned to the "assetLoaderContext.RootGameObject" field.
        // If you want to make sure the GameObject will be visible only when all Materials and Textures have been loaded, you can disable it at this step.
        var myLoadedGameObject = assetLoaderContext.RootGameObject;
        myLoadedGameObject.SetActive(false);
    }

    // This event is called after OnLoad when all Materials and Textures have been loaded.
    // This event is also called after a critical loading error, so you can clean up any resource you want to.
    private void OnMaterialsLoad(AssetLoaderContext assetLoaderContext)
    {
        // The root loaded GameObject is assigned to the "assetLoaderContext.RootGameObject" field.
        // You can make the GameObject visible again at this step if you prefer to.
        var myLoadedGameObject = assetLoaderContext.RootGameObject;
        myLoadedGameObject.SetActive(true);
    }
}
