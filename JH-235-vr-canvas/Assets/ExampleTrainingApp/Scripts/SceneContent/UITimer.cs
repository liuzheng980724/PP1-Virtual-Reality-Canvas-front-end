using System;
using UnityEngine;
using UnityEngine.UI;

public class UITimer : MonoBehaviour
{
    [SerializeField]
    private RectTransform rectTransform;

    [SerializeField]
    private Image barImage;

    [SerializeField]
    private Color[] barColors;

    [SerializeField]
    private GameObject content;

    private bool clockRunning;
    private float timePassed = 0.0f;
    private float timeTotal = 10.0f;
    private Vector3 scale = Vector3.one;
    private Action timeUpCallBack;

    public void StartTimer(float secondsToTake, Action _timeUpCallBack)
    {
        timeTotal = secondsToTake;
        timeUpCallBack = _timeUpCallBack;
        clockRunning = true;
        Show();
    }

    private void Update()
    {
        if (!clockRunning)
        {
            return;
        }

        timePassed += Time.deltaTime;
        var percent = timePassed / timeTotal;
        percent = Mathf.Clamp(percent, 0.0f, 1.0f);

        var inverseTime = 1.0f - percent;
        scale.x = inverseTime;

        var colorIndex = Mathf.RoundToInt(inverseTime * (barColors.Length - 1));
        barImage.color = barColors[colorIndex];

        rectTransform.localScale = scale;
        if (percent >= 1.0f)
        {
            timeUpCallBack.Invoke();
            clockRunning = false;
        }
    }

    public void StopTimer()
    {
        clockRunning = false;
    }

    public void Show()
    {
        content.SetActive(true);
        LeanTween.cancel(content);
        content.transform.localScale = Vector3.one * 0.1f;
        LeanTween.scale(content, Vector3.one, 0.5f)
            .setEase(LeanTweenType.easeOutCirc);
    }

    public void Hide()
    {
        LeanTween.cancel(content);
        content.transform.localScale = Vector3.one * 0.1f;
        LeanTween.scale(content, Vector3.one, 0.5f)
            .setEase(LeanTweenType.easeInCirc);
    }

    public void HideInstantly()
    {
        LeanTween.cancel(content);

        content.SetActive(false);
    }
}