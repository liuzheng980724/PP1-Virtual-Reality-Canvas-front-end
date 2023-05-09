using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class getUsernameFromCanvas : MonoBehaviour
{   
    public Text Username;
    public string JsonString;
    public string userAccesstoken;
    public string requireURL;

    [Serializable]
    public class Person {
        public string name;
    }

    IEnumerator Start() {
        userAccesstoken = "9595~gNBaPQCzV32DkLkMzxGyCNzA0PjMVbElkcmtUbDi3i7NTaXnsjmu3ESl8abhGvLP";
        requireURL = "https://rmit.instructure.com/api/v1/users/self/profile?access_token=" + userAccesstoken;
        
        //Username.text = userAccesstoken;

        using (WWW www = new WWW(requireURL)) {
            yield return www;
            if (www.error != null) {
                Debug.Log(www.error);
            } else {
                JsonString = www.text;
              
                Person data = JsonUtility.FromJson<Person>(JsonString);
                Username.text = data.name;
            }
        }
    }



}
