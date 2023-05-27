using UnityEngine;
using UnityEngine.UI;

public class ResultDisplay : MonoBehaviour
{
    [SerializeField]
    protected GameObject content;

    [SerializeField]
    protected Image resultImage;

    [SerializeField]
    protected Sprite correctSprite;

    [SerializeField]
    protected Sprite wrongSprite;

    [SerializeField]
    protected AudioSource audioSource;

    [SerializeField]
    protected AudioClip correctAudio;

    [SerializeField]
    protected AudioClip wrongAudio;

    public void Show(bool correct)
    {
        content.SetActive(true);
        resultImage.sprite = correct ? correctSprite : wrongSprite;

        audioSource.clip = correct ? correctAudio : wrongAudio;
        audioSource.Play();

        LeanTween.cancel(content);
        var timeToTake = 0.3f;
        content.transform.localScale = Vector3.zero * 0.1f;
        LeanTween.scale(content, Vector3.one, timeToTake)
            .setEase(LeanTweenType.easeOutBounce);
    }

    public void Hide()
    {
        if (!content.activeSelf)
        {
            return;
        }

        LeanTween.cancel(content);
        var timeToTake = 0.3f;
        LeanTween.scale(content, Vector3.one * 0.1f, timeToTake)
            .setOnComplete(HideInstantly)
            .setEase(LeanTweenType.easeInBack);
    }

    public void HideInstantly()
    {
        LeanTween.cancel(content);
        content.SetActive(false);
    }
}