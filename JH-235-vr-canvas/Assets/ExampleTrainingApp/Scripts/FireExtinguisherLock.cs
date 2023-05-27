using System;
using UnityEngine;

public class FireExtinguisherLock : MonoBehaviour
{
    [SerializeField]
    private MeshRenderer meshRenderer;

    [SerializeField]
    private Material locked;

    [SerializeField]
    private Material unlocked;

    [SerializeField]
    private GameObject particleAttractor;

    private Action unlockedCallback;

    public void Init()
    {
        particleAttractor.SetActive(false);
    }

    public void ShowLocked()
    {
        //meshRenderer.sharedMaterial = locked;
        meshRenderer.enabled = true;
    }

    public void ShowUnLocked()
    {
        // meshRenderer.sharedMaterial = unlocked; 
        meshRenderer.enabled = false;
    }


    public void SetupLock(Action callback)
    {
        ShowLocked();
        unlockedCallback = callback;
        particleAttractor.SetActive(true);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (unlockedCallback == null)
        {
            return;
        }

        Debug.Log($"OnTriggerEnter:{other.name} tag:{other.tag}", other.gameObject);

        if (other.CompareTag("HandCollider"))
        {
            ShowUnLocked();
            particleAttractor.SetActive(false);
            unlockedCallback?.Invoke();
            unlockedCallback = null;
        }
    }
}