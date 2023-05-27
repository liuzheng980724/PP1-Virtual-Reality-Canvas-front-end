using UnityEngine;

public class PercentComplete : SceneObject
{
    [SerializeField]
    private Transform growBar;

    private Vector3 scaler = Vector3.one;

    public void DisplayPercent(float percent)
    {
        scaler.y = percent;
        growBar.localScale = scaler;
    }

    public override void Show()
    {
        base.Show();

        sceneContent.SetActive(true);
        sceneContent.transform.localScale = Vector3.one * 0.1f;

        LeanTween.cancel(sceneContent);
        LeanTween.scale(sceneContent, Vector3.one, 0.5f)
            .setEase(LeanTweenType.easeOutBack);
    }

    public override void ShowInstantly()
    {
        base.ShowInstantly();
        sceneContent.SetActive(true);
    }

    public override void Hide()
    {
        base.Hide();

        LeanTween.cancel(sceneContent);
        LeanTween.scale(sceneContent, Vector3.one*0.1f, 0.5f)
            .setEase(LeanTweenType.easeInBack)
            .setOnComplete(HideInstantly);
    }

    public override void HideInstantly()
    {
        base.HideInstantly();
        LeanTween.cancel(sceneContent);
    }
}