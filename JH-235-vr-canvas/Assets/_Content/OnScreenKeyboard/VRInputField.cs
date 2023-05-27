using UnityEngine;
using TMPro;
using System;
using UnityEngine.EventSystems;


// MK: unity's input field and tmp input field have a few short comings when in pico vr, hence this class
// issue 1: you cant disable the sort of busted native keyboard appearing when interacting with the input field
// issue 2: there is no on click event to allow you to manually force it focused when trying to type with an OSK
public class VRInputField : MonoBehaviour, IPointerClickHandler
{
    [SerializeField]
    private TextMeshProUGUI placeholderTextbox;

    [SerializeField]
    private TextMeshProUGUI inputFieldTextbox;

    public event Action onClick;

    public string text
    {
        get { return inputFieldTextbox.text; }
        set
        {
            inputFieldTextbox.text = value;
            OnValueChanged();
        }
    }

    private void Awake()
    {
        Debug.Assert(placeholderTextbox != null);
        Debug.Assert(inputFieldTextbox != null);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        onClick?.Invoke();
    }

    void OnValueChanged()
    {
        placeholderTextbox.gameObject.SetActive(string.IsNullOrEmpty(inputFieldTextbox.text));
    }
}