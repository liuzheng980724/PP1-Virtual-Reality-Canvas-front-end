using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class OnScreenKeyboard : MonoBehaviour
{
    public enum keyBoardType
    {
        alphanumeric,
        symbols,
        numeric
    };

    public InputField focus;
    public VRInputField oskfocus;
    public bool showNumeric = false;
    public bool showSymbols = false;
    public Color32 textColor;
    public Color32 mainColor;
    public Color32 specialColor;
    public Color32 backgroundColor;
    public Sprite mainSprite;
    public Sprite specialSprite;
    public GameObject[] panels;
    public GameObject[] keys;
    public GameObject[] specialKeys;

    [HideInInspector]
    public bool isActive = false;

    [HideInInspector]
    public bool capsEnabled = false;

    void Start()
    {
        ShowNumeric(showNumeric);
        ShowSymbols(showSymbols);
        SetTextColor(textColor);
        SetMainColor(mainColor);
        SetSpecialColor(specialColor);
        SetBackgroundColor(backgroundColor);
        SetMainSprite(mainSprite);
        SetSpecialSprite(specialSprite);
    }

    public void ShowSymbols(bool b)
    {
        panels[2].SetActive(b);
        showSymbols = b;
    }

    public void ShowNumeric(bool b)
    {
        panels[3].SetActive(b);
        showNumeric = b;
    }

    public void SetTextColor(Color32 c)
    {
        foreach (GameObject go in keys)
        {
            go.transform.Find("Text").GetComponent<Text>().color = c;
        }

        foreach (GameObject go in specialKeys)
        {
            go.transform.Find("Text").GetComponent<Text>().color = c;
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

    public void WriteKey(Text t)
    {
        keyboardSFX.PlayKeyPressSFX();
        if (focus)
        {
            focus.text += t.text;
        }

        if (oskfocus)
        {
            oskfocus.text += t.text;
        }
    }

    [SerializeField]
    private KeyboardSFX keyboardSFX;

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
            case 4:
                SetKeyboardType(keyBoardType.symbols);
                break;
            case 5:
                SetKeyboardType(keyBoardType.numeric);
                break;
            case 6:
                FocusPrevious();
                break;
            case 7:
                FocusNext();
                break;
            case 8:
                SetKeyboardType(0);
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

    public void SetKeyboardType(keyBoardType n)
    {
        switch (n)
        {
            case keyBoardType.alphanumeric:
                panels[0].SetActive(true);
                panels[1].SetActive(false);
                panels[2].SetActive(false);
                panels[3].SetActive(true);
                break;
            case keyBoardType.symbols:
                panels[0].SetActive(false);
                panels[1].SetActive(true);
                panels[2].SetActive(false);
                panels[3].SetActive(false);
                break;
            case keyBoardType.numeric:
                panels[0].SetActive(false);
                panels[1].SetActive(false);
                panels[2].SetActive(false);
                panels[3].SetActive(true);
                break;
        }
    }
}