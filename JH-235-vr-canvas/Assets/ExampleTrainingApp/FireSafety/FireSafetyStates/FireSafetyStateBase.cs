using UnityEngine;

public class FireSafetyStateBase : MonoBehaviour
{
    [SerializeField]
    private GameObject content;

    private FireSafetyControl fireSafetyControl;

    public virtual void onEnter()
    {
        Show();
    }

    public virtual void onExit()
    {
        Hide();
    }

    protected virtual void Show()
    {
        content.SetActive(true);
    }

    protected virtual void ShowFinished()
    {
        // empty
    }

    protected virtual void Hide()
    {
        LeanTween.cancel(content);
        //LeanTween.scale(content, Vector3.one * 0.01f, HideTime)
        //    .setEase(LeanTweenType.easeOutQuad)
        //    .setOnComplete(HideInstantly);
        HideInstantly();
    }

    private void HideInstantly()
    {
        content.SetActive(false);
    }

    public void Init(FireSafetyControl _fireSafetyControl)
    {
        fireSafetyControl = _fireSafetyControl;
    }
}