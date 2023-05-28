using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class JumpToDashboard : MonoBehaviour
{
    public void gotoDashboard()
    {
        SceneManager.LoadScene("Dashboard");
    }

    public void gotoLandPage()
    {
        SceneManager.LoadScene("MainPage");
    }

    public void gotoFolders()
    {
        SceneManager.LoadScene("folderslist");
    }

    public void gotoAssignment()
    {
        SceneManager.LoadScene("Assignment");
    }
}
