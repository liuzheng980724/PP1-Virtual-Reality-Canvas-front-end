using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UILayoutButton : MonoBehaviour
{
    [SerializeField]
    private TMPro.TMP_Text textBox;
    [SerializeField]
    private Button button;

    private void OnValidate()
    {
        if (textBox == null)
            textBox = this.GetComponentInChildren<TMPro.TMP_Text>();
        if (button == null)
            button = this.GetComponentInChildren<Button>();
    }

    public void Set(string text, Action onClickAction)
    {
        if (textBox!=null)
            textBox.text = text;
        if (button!=null)
            button.onClick.AddListener(() => { onClickAction?.Invoke(); });
    }
}
