using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIButtonLayoutPanel : MonoBehaviour
{
    [SerializeField]
    private Transform root;
    [SerializeField]
    private Transform layoutRect;
    [SerializeField]
    private GameObject buttonPrefab;

    private void Awake()
    {
        root.gameObject.SetActive(false);
    }

    public void SetButtons(ButtonInfo[] buttons, bool autoActivate = true)
    {
        root?.gameObject.SetActive(autoActivate);

        if (layoutRect == null)
            return;

        DeleteAllChildObjects(layoutRect);

        if (buttons == null || buttons.Length <= 0)
            return;

        foreach (var b in buttons)
        {
            if (buttonPrefab == null)
                return;

            GameObject go = Instantiate(buttonPrefab, layoutRect);
            go.SetActive(true);
            UILayoutButton button = go.GetComponent<UILayoutButton>();
            button.Set(b.textCopy, b.action);            
        }
    }

    public void Activate(bool enable)
    {
        root?.gameObject.SetActive(enable);

        if (enable == false)
        {            
            DeleteAllChildObjects(layoutRect);
        }
    }

    void DeleteAllChildObjects(Transform parent)
    {
        if (parent == null)
            return;

        int childs = parent.transform.childCount;

        for (int i = childs - 1; i >= 0; i--)
        {
            GameObject.Destroy(layoutRect.transform.GetChild(i).gameObject);
        }
    }
}

public class ButtonInfo
{
    public string textCopy;
    public System.Action action;

    public ButtonInfo(string textCopy, Action action)
    {
        this.textCopy = textCopy;
        this.action = action;
    }
}
