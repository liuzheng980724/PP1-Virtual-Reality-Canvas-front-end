using System;
using System.Collections;
using UnityEngine;

namespace Snobal.DesignPatternsUnity_0_0
{
    public static class Coroutiner
    {
        public static Coroutine StartCoroutine(IEnumerator iterationResult, string customName = "default")
        {
            // Create GameObject with MonoBehaviour to handle task. This could be a shared gameobject, 
            // but having multiple instances makes it clear what coroutines are underway
            GameObject routineHandlerGo = new GameObject("Coroutiner: " + customName);
            CoroutinerInstance routineHandler = routineHandlerGo.AddComponent(typeof(CoroutinerInstance)) as CoroutinerInstance;
            return routineHandler.ProcessWork(iterationResult);
        }

        private class CoroutinerInstance : MonoBehaviour
        {
            void Awake()
            {
                DontDestroyOnLoad(this);
            }

            public Coroutine ProcessWork(IEnumerator iterationResult)
            {
                return StartCoroutine(DestroyWhenComplete(iterationResult));
            }

            private IEnumerator DestroyWhenComplete(IEnumerator iterationResult)
            {
                yield return StartCoroutine(iterationResult);
                Destroy(gameObject);
            }
        }
    }
}