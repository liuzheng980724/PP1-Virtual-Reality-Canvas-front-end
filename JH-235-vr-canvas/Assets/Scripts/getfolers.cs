using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.Networking;
using Newtonsoft.Json;
using UnityEngine.SceneManagement;

public class getfolers : MonoBehaviour
{
    public Text myTitle;
    public string JsonString;
    public string userAccesstoken;
    public string requireFoldersURL;

    [Serializable]
    public class folders
    {
        public int id;
        public string name;
        public string full_name;
        public string files_url;
    }

    // Start is called before the first frame update
    IEnumerator Start()
    {
        //Font myDefaultFont = Resources.Load("Fonts/myFont") as Font;    //Load my font
        myTitle.text = "E70012 Sandbox Files";
        myTitle.fontSize = 16;
        //myTitle.font = myDefaultFont;

        List<folders> foldersList = new List<folders>();
        
        userAccesstoken = "9595~gNBaPQCzV32DkLkMzxGyCNzA0PjMVbElkcmtUbDi3i7NTaXnsjmu3ESl8abhGvLP";
        requireFoldersURL = "https://rmit.instructure.com/api/v1/courses/70814/folders?access_token=" + userAccesstoken;

        using (UnityWebRequest www = UnityWebRequest.Get(requireFoldersURL))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
            }
            else
            {
                JsonString = www.downloadHandler.text;
                Debug.Log(JsonString);
                foldersList = JsonConvert.DeserializeObject<List<folders>>(JsonString);

                int posX, posY, posZ;
                posX = -10;
                posY = 15;
                posZ = 15;

                foreach (folders thisFolder in foldersList) //Generate each folder button
                {
                    GameObject buttonObject = new GameObject("Button_"+ thisFolder.id);
                    buttonObject.transform.SetParent(this.transform);

                    Button buttonComponent = buttonObject.AddComponent<Button>();
                    Text buttonText = buttonObject.AddComponent<Text>();
                    buttonText.text = thisFolder.full_name;
                    buttonText.font = myTitle.font;
                    buttonText.color = Color.white;
                    buttonText.fontSize = 14;
                    buttonText.rectTransform.sizeDelta = new Vector2(150, 20);
                    buttonText.rectTransform.position = new Vector3(posX, posY, posZ);
                    buttonText.transform.localScale = new Vector3(1, 1, 1);
                    posY = posY - 2;    //DO NOT TOUCH. 1:10    --ZHENG LIU

                    // Add button event
                    buttonComponent.onClick.AddListener(() => Loadfolder(thisFolder.files_url, thisFolder.full_name));
                }
            }
        }



    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void Loadfolder(String thisUrl, String thisName)
    {
        Debug.Log(thisUrl);
        PlayerPrefs.SetString("thisFolderName", thisName);
        PlayerPrefs.SetString("thisFolderUrl", thisUrl);
        SceneManager.LoadScene("Chemistry");
        //SceneManager.LoadScene("Chemistry");
    }
}
