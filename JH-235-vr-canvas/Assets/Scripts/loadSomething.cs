using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.Networking;
using Newtonsoft.Json;
using UnityEngine.SceneManagement;
using System.IO;
using System.IO.Compression;

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
        //Show button
        /*
        GameObject buttonObject = new GameObject("Button");
        buttonObject.transform.SetParent(this.transform);

        Button buttonComponent = buttonObject.AddComponent<Button>();
        Text buttonText = buttonObject.AddComponent<Text>();
        buttonText.text = "E70012 Sandbox";
        buttonText.font = courseName.font;
        buttonText.color = courseName.color;
        buttonText.fontSize = courseName.fontSize;
        buttonText.rectTransform.sizeDelta = new Vector2(150, 50);
        buttonText.rectTransform.position = new Vector3(0, 10, 1);
        buttonText.transform.localScale = new Vector3(1, 1, 1);
        */


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
                    /*
                    string localPath = "Assets/Models/" + thisFile.filename;
                    UnityWebRequest wwwObj = UnityWebRequest.Get(thisFile.url);

                    DownloadHandlerFile downloadHandler = new DownloadHandlerFile(localPath);
                    wwwObj.downloadHandler = downloadHandler;

                    yield return wwwObj.SendWebRequest();

                    if (wwwObj.result != UnityWebRequest.Result.Success)
                    {
                        Debug.Log(wwwObj.error);
                    } else
                    {
                        Debug.Log("FBX file downloaded to " + localPath);


                            AssetBundle bundle = AssetBundle.LoadFromFile(localPath);

                            if (bundle == null)
                            {
                                Debug.Log("Failed to load AssetBundle!");
                            }
                            else
                            {
                                GameObject prefab = bundle.LoadAsset<GameObject>("");
                                loadedObject = Instantiate(prefab);
                            }

                    }
                    */
                    ///*
                    using (UnityWebRequest wwwObj = UnityWebRequest.Get(thisFile.url))
                    {
                        yield return wwwObj;

                        if (wwwObj.error != null)
                        {
                            Debug.Log(wwwObj.error);
                        } else
                        {
                            Debug.Log(thisFile.url);
                            Debug.Log(thisFile.filename);

                            AssetBundle bundle = AssetBundle.LoadFromMemory(wwwObj.downloadHandler.data);

                            if (bundle == null) // check if bundle is null
                            {
                                Debug.Log("Failed to load AssetBundle!");
                            } else
                            {
                                string[] assetNames = bundle.GetAllAssetNames();
                                //Check null error
                                Debug.Log("TEST");
                                foreach (string assetName in assetNames)
                                {
                                    Debug.Log("TEST123");
                                    Debug.Log(assetName);
                                }

                                if (assetNames.Length == 0)
                                {
                                    Debug.Log("Bundle is empty");
                                }
                                else
                                {
                                    GameObject prefab = bundle.LoadAsset<GameObject>("");
                                    loadedObject = Instantiate(prefab);
                                }
                            }

                        }

                    }
                //*/
                }
                objectName.text = objNameT;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
