using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ExpiryDateObject : MonoBehaviour
{
    [SerializeField]
    private TextMeshPro expiryDateText;

    [SerializeField]
    private ExampleAppSettings settings;

    [SerializeField]
    private GameObject content;

    public void Setup()
    {
        expiryDateText.text = settings.extinguisherExpiryDate;
    }

    public void Show()
    {
        content.SetActive(true);
    }

    public void Hide()
    {
        content.SetActive(false);
    }
}