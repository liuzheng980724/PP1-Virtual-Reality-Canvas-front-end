using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.Networking;
using Newtonsoft.Json;
using UnityEngine.SceneManagement;

public class getFullCorsesList : MonoBehaviour
{
    public Text myText;
    public Text courseName;
    public string JsonString;
    public string userAccesstoken;
    public string requireCourseURL;
    public string courseNameT;

    [Serializable]
    public class Course
    {
        public int id;
        public string name;
        public string description;
        public string start_at;
        public string end_at;
    }


    // Start is called before the first frame update
    IEnumerator Start()
    {
        //Show button
        
        GameObject buttonObject = new GameObject("Button");
        buttonObject.transform.SetParent(this.transform);
        
        Button buttonComponent = buttonObject.AddComponent<Button>();
        Text buttonText = buttonObject.AddComponent<Text>();
        buttonText.text = "E70012 Sandbox";
        buttonText.font = courseName.font;
        buttonText.color = courseName.color;
        buttonText.fontSize = courseName.fontSize;
        buttonText.rectTransform.sizeDelta = new Vector2(150, 50);
        buttonText.rectTransform.position = new Vector3(0, 5, 15);
        buttonText.transform.localScale = new Vector3(1, 1, 1);
        
        // Add button event
        buttonComponent.onClick.AddListener(LoadClassroom);
        /*
        GameObject textObject = new GameObject("Text");
        textObject.transform.SetParent(this.transform);

        Text textComponent = textObject.AddComponent<Text>();

        textComponent.text = "Hello World!";
        textComponent.font = courseName.font;
        textComponent.color = courseName.color;
        textComponent.fontSize = courseName.fontSize;
        textComponent.alignment = courseName.alignment;
        textComponent.rectTransform.sizeDelta = courseName.rectTransform.sizeDelta;
        textComponent.rectTransform.position = new Vector3(0, 0, 300);
        */



        List<Course> courseList = new List<Course>();
        userAccesstoken = "9595~gNBaPQCzV32DkLkMzxGyCNzA0PjMVbElkcmtUbDi3i7NTaXnsjmu3ESl8abhGvLP";

        requireCourseURL = "https://rmit.instructure.com/api/v1/courses?access_token=" + userAccesstoken;

        courseName.text = "Default Course";

        using (UnityWebRequest www = UnityWebRequest.Get(requireCourseURL))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                courseName.text = www.error;
            }
            else
            {
                JsonString = www.downloadHandler.text;
                Debug.Log(JsonString);
                courseList = JsonConvert.DeserializeObject<List<Course>>(JsonString);

                foreach (Course thisCourse in courseList)
                {
                    courseNameT += thisCourse.name + "\n";
                }
                courseName.text = courseNameT;
            }
        }
    }

    void LoadClassroom()
    {
        SceneManager.LoadScene("Chemistry");
    }

    // Update is called once per frame
    void Update()
    {

    }
}
