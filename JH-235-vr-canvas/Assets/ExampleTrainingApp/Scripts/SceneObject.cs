using UnityEngine;

public abstract class SceneObject : MonoBehaviour
{
    [SerializeField]
    protected GameObject sceneContent;

    public virtual void Setup()
    {
    }

    public virtual void Hide()
    {
    }

    public virtual void HideInstantly()
    {
        sceneContent.SetActive(false);
    }

    public virtual void Show()
    {
        // if not showing already, then animate showing
        
    }

    public virtual void ShowInstantly()
    {
        sceneContent.SetActive(true);
    }
}