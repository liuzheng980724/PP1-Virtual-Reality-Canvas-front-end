using UnityEngine;

/// <summary>
/// Scene Content that is specific to a task, like visual guides, or static prompts only used during a task
/// </summary>
public class SceneTaskHelper : MonoBehaviour
{
    [SerializeField]
    protected GameObject content;

    public virtual void Show()
    {
        content.SetActive(true);
        LeanTween.cancel(content);
        var timeToTake = 1.0f;
        LeanTween.value(content, 0.0f, 1.0f, timeToTake)
            .setOnUpdate(ShowProgress);
    }

    protected virtual void ShowProgress(float value)
    {
        //Debug.Log($"Show progress:{value}");
        // todo this allows for using alpha to fade out things
    }

    public virtual void Hide()
    {
        // fade ?
        // here sub items will need to be told to hide
        LeanTween.cancel(content);
        var timeToTake = 1.0f;
        LeanTween.value(content, 0.0f, 1.0f, timeToTake)
            .setOnUpdate(HideProgress)
            .setOnComplete(HideInstantly);
    }

    protected virtual void HideProgress(float value)
    {
        //Debug.Log($"Hide progress:{value}");
        // todo this allows for using alpha to fade out things
    }

    public virtual void HideInstantly()
    {
        LeanTween.cancel(content);
        content.SetActive(false);
    }

    public virtual void Setup()
    {
        Debug.Log($"Setup {gameObject.name}");
        HideInstantly();
    }
}