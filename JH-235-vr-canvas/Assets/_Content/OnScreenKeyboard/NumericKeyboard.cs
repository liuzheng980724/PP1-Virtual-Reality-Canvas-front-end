using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class NumericKeyboard : MonoBehaviour
{
    public InputField focus;

    public VRInputField oskfocus;

    [Space(10), SerializeField]
    public GameObject[] keys;

    [SerializeField]
    public GameObject[] specialKeys;

    [Space(10), SerializeField]
    public Color32 textColor;

    [SerializeField]
    public Color32 mainColor;

    [SerializeField]
    public Color32 specialColor;

    [SerializeField]
    public Color32 backgroundColor;

    [Space(10), SerializeField]
    public Sprite mainSprite;

    [SerializeField]
    public Sprite specialSprite;


    [HideInInspector]
    public bool isActive = false;

    [HideInInspector]
    public bool capsEnabled = false;

    [SerializeField]
    private KeyboardSFX keyboardSFX;

    void Start()
    {
        SetTextColor(textColor);
        SetMainColor(mainColor);
        SetSpecialColor(specialColor);
        SetBackgroundColor(backgroundColor);
        SetMainSprite(mainSprite);
        SetSpecialSprite(specialSprite);
    }


    public void SetTextColor(Color32 c)
    {
        foreach (GameObject go in keys)
        {
            go.transform.Find("Text").GetComponent<TextMeshProUGUI>().color = c;
        }

        foreach (GameObject go in specialKeys)
        {
            go.transform.Find("Text").GetComponent<TextMeshProUGUI>().color = c;
        }
    }

    public void SetMainColor(Color32 c)
    {
        foreach (GameObject go in keys)
        {
            go.GetComponent<Image>().color = c;
        }
    }

    public void SetSpecialColor(Color32 c)
    {
        foreach (GameObject go in specialKeys)
        {
            go.GetComponent<Image>().color = c;
        }
    }

    public void SetBackgroundColor(Color32 c)
    {
        gameObject.GetComponent<Image>().color = c;
    }

    public void SetMainSprite(Sprite s)
    {
        foreach (GameObject go in keys)
        {
            go.GetComponent<Image>().sprite = s;
        }
    }

    public void SetSpecialSprite(Sprite s)
    {
        foreach (GameObject go in specialKeys)
        {
            go.GetComponent<Image>().sprite = s;
        }
    }

    public void SetFocus(InputField i)
    {
        focus = i;
    }

    public void SetActiveFocus(InputField i)
    {
        focus = i;
        SetActive(true);
        focus.MoveTextEnd(true);
    }

    public void WriteKey(TextMeshProUGUI t)
    {
        keyboardSFX.PlayKeyPressPitchSFX( t.text);
        if (focus)
        {
            focus.text += t.text;
        }

        if (oskfocus)
        {
            oskfocus.text += t.text;
        }
    }

    public void WriteKey(Text t)
    {
        // if the character is 0-9 then it plays a specific type of sound
        keyboardSFX.PlayKeyPressPitchSFX( t.text);
        if (focus)
        {
            focus.text += t.text;
        }

        if (oskfocus)
        {
            oskfocus.text += t.text;
        }
    }

    public void WriteSpecialKey(int n)
    {
        keyboardSFX.PlayKeyPressSFX();
        switch (n)
        {
            case 0:
                if (focus && focus.text.Length > 0)
                    focus.text = focus.text.Substring(0, focus.text.Length - 1);

                if (oskfocus && oskfocus.text.Length > 0)
                    oskfocus.text = oskfocus.text.Substring(0, oskfocus.text.Length - 1);
                break;
            case 1:
                EventSystem system;
                system = EventSystem.current;
                if (!focus)
                {
                    return;
                }

                focus.OnSubmit(new PointerEventData(system));
                break;
            case 2:
                SwitchCaps();
                break;
            case 3:
                SetActive(false);
                break;

            case 6:
                FocusPrevious();
                break;
            case 7:
                FocusNext();
                break;
        }
    }

    public void SetActive(bool b)
    {
        if (b)
        {
            if (!isActive)
            {
                gameObject.GetComponent<Animator>().Rebind();
                gameObject.GetComponent<Animator>().enabled = true;
            }
        }
        else
        {
            if (isActive)
            {
                gameObject.GetComponent<Animator>().SetBool("Hide", true);
            }
        }

        isActive = b;
    }

    public void SetCaps(bool b)
    {
        if (b)
        {
            foreach (GameObject go in keys)
            {
                Text t = go.transform.Find("Text").GetComponent<Text>();
                t.text = t.text.ToUpper();
            }
        }
        else
        {
            foreach (GameObject go in keys)
            {
                Text t = go.transform.Find("Text").GetComponent<Text>();
                t.text = t.text.ToLower();
            }
        }

        capsEnabled = b;
    }

    public void SwitchCaps()
    {
        SetCaps(!capsEnabled);
    }

    public void FocusPrevious()
    {
        EventSystem system;
        system = EventSystem.current;

        if (!focus)
        {
            return;
        }

        Selectable current = focus.GetComponent<Selectable>();
        Selectable next = current.FindSelectableOnLeft();
        if (!next)
        {
            next = current.FindSelectableOnUp();
        }

        if (!next)
        {
            return;
        }

        InputField inputfield = next.GetComponent<InputField>();
        if (inputfield != null)
        {
            inputfield.OnPointerClick(new PointerEventData(system));
            focus = inputfield;
        }

        system.SetSelectedGameObject(next.gameObject);
    }

    public void FocusNext()
    {
        EventSystem system;
        system = EventSystem.current;

        if (!focus)
        {
            return;
        }

        Selectable current = focus.GetComponent<Selectable>();
        Selectable next = current.FindSelectableOnRight();
        if (!next)
        {
            next = current.FindSelectableOnDown();
        }

        if (!next)
        {
            return;
        }

        InputField inputfield = next.GetComponent<InputField>();
        if (inputfield != null)
        {
            inputfield.OnPointerClick(new PointerEventData(system));
            focus = inputfield;
        }

        system.SetSelectedGameObject(next.gameObject);
    }
}