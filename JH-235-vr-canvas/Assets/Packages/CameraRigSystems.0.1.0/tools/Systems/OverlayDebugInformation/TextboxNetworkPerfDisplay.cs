using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using TMPro;

public class TextboxNetworkPerfDisplay : MonoBehaviour
{
    [SerializeField] 
    TMP_Text displayText = null;

    StringBuilder stringBuilder = new StringBuilder();

    private void OnValidate()
    {
        if (displayText == null)
            displayText = this.GetComponentInChildren<TMP_Text>();
    }

    private void Awake()
    {
        Debug.Assert(displayText != null);
    }

    private void Start()
    {
        displayText.text = string.Empty;
    }

    void OnEnable()
    {
        UpdateTextMesh();
    }

    void Update()
    {
        UpdateTextMesh();        
    }

    void UpdateTextMesh()
    {
        stringBuilder.Clear();
        stringBuilder.Append($"<color=cyan>Network bytes: {string.Empty} </color>\n");
        displayText.text = stringBuilder.ToString();
    }
}
