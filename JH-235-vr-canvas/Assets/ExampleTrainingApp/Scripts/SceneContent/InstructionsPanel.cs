using UnityEngine;

public class InstructionsPanel : MonoBehaviour
{
    [SerializeField]
    private GameObject content;

    public void Show()
    {
        content.SetActive(true);
        content.transform.localScale = Vector3.one * 0.1f;

        LeanTween.cancel(content);
        LeanTween.scale(content, Vector3.one, 0.5f)
            .setEase(LeanTweenType.easeOutBack);
    }

    public void ShowInstantly()
    {
        content.SetActive(true);
    }

    public void Hide()
    {
        LeanTween.cancel(content);
        LeanTween.scale(content, Vector3.one * 0.1f, 0.5f)
            .setEase(LeanTweenType.easeInBack)
            .setOnComplete(HideInstantly);
    }

    public void HideInstantly()
    {
        LeanTween.cancel(content);
    }
}