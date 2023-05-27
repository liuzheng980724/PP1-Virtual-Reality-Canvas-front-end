using System;
using System.Collections;
using Snobal.CloudSDK;
using UnityEngine;

public abstract class StateBase : MonoBehaviour
{
    [SerializeField]
    private GameObject content;

    protected IEnumerator coroutine;
    protected SceneControl sceneControl;

    private float ShowTime = 0.5f;
    private float HideTime = 0.35f;

    protected Snobal.SnobalCloud SC
    {
        get { return Snobal.CloudSDK.SnobalCloudMonobehaviour.SnobalCloudInstance; }
    }

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
        content.transform.localScale = Vector3.one * 0.8f;
        LeanTween.cancel(content);
        LeanTween.scale(content, Vector3.one, ShowTime)
            .setEase(LeanTweenType.easeOutBack)
            .setOnComplete(ShowFinished);
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

    protected virtual void HideInstantly()
    {
        content.SetActive(false);
    }


    public virtual void Init(SceneControl _sceneControl)
    {
        sceneControl = _sceneControl;
        HideInstantly();
    }

    protected void StopRunningCoroutine()
    {
        if (coroutine != null)
        {
            StopCoroutine(coroutine);
        }
    }

    /// <summary>
    /// Convenience threaded function for running tasks with 1 value passed to a function
    /// </summary>
    /// <param name="threadedAction"></param>
    /// <param name="value"></param>
    /// <param name="finishedCallback"></param>
    /// <typeparam name="T"></typeparam>
    protected void RunThreadedTask<T>(Action<T> threadedAction, T value, Action finishedCallback)
    {
        SnobalCloudMonobehaviour.TaskWatcher.Watch(
            System.Threading.Tasks.Task.Run
            (
                () =>
                {
                    // Seperate thread
                    threadedAction.Invoke(value);
                }
            ),
            () => { finishedCallback.Invoke(); }
        );
    }

    /// <summary>
    /// Convenience threaded function for running tasks with 2 values passed to function
    /// </summary>
    /// <param name="threadedAction"></param>
    /// <param name="value"></param>
    /// <param name="value2"></param>
    /// <param name="finishedCallback"></param>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="U"></typeparam>
    protected void RunThreadedTask<T, U>(Action<T, U> threadedAction, T value, U value2, Action finishedCallback)
    {
        SnobalCloudMonobehaviour.TaskWatcher.Watch(
            System.Threading.Tasks.Task.Run
            (
                () =>
                {
                    // Seperate thread
                    threadedAction.Invoke(value, value2);
                }
            ),
            () => { finishedCallback.Invoke(); }
        );
    }

    /// <summary>
    /// Convenience threaded function for running tasks with 3 values passed to function
    /// </summary>
    /// <param name="threadedAction"></param>
    /// <param name="value"></param>
    /// <param name="value2"></param>
    /// <param name="value3"></param>
    /// <param name="finishedCallback"></param>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="U"></typeparam>
    /// <typeparam name="V"></typeparam>
    protected void RunThreadedTask<T, U, V>(Action<T, U, V> threadedAction, T value, U value2, V value3,
        Action finishedCallback)
    {
        SnobalCloudMonobehaviour.TaskWatcher.Watch(
            System.Threading.Tasks.Task.Run
            (
                () =>
                {
                    // Seperate thread
                    threadedAction.Invoke(value, value2, value3);
                }
            ),
            () => { finishedCallback.Invoke(); }
        );
    }

    /// <summary>
    /// Remove children from target GameObject
    /// </summary>
    /// <param name="parent"></param>
    protected void RemoveChildren(GameObject parent)
    {
        foreach (Transform child in parent.transform)
        {
            Destroy(child.gameObject);
        }
    }
}