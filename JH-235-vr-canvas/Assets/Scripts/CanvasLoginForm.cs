using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasLoginForm : SceneObject
{

    public override void Setup()
    {
        HideInstantly();
    }

    public override void Hide()
    {
        sceneContent.SetActive(false);
    }

    public override void Show()
    {
        sceneContent.SetActive(true);
    }

    public override void HideInstantly()
    {
        sceneContent.SetActive(false);
        base.HideInstantly();
    }
}
