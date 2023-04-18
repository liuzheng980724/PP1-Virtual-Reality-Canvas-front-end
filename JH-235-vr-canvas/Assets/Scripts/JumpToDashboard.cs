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
}
