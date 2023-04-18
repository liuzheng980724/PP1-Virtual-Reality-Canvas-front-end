using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class getUsernameFromCanvas : MonoBehaviour
{   
    public Text textComponent;
    public string JsonString;
    public string userAccesstoken;
    public string requireURL;

    [Serializable]
    public class Person {
        public string name;
    }

    IEnumerator Start() {
        userAccesstoken = "x5f4kbQGzeME7eetYVsFIAp06AeZUhwV8F3u4IFFsvctbdySKFLaRjic17qEzVtW";
        requireURL = "http://168.138.23.126/api/v1/users/self/profile?access_token=" + userAccesstoken;

        //textComponent.text = userAccesstoken;

        using (WWW www = new WWW(requireURL)) {
            yield return www;
            if (www.error != null) {
                Debug.Log(www.error);
            } else {
                JsonString = www.text;

                textComponent.text = JsonString;
              
                Person data = JsonUtility.FromJson<Person>(JsonString);
                textComponent.text = data.name;
            }

            

        }
    }



}
