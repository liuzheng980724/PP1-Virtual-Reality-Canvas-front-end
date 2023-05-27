using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Snobal.DesignPatternsUnity_0_0
{

    /**
     * Class SingletonMonoBehaviour
     * 
     * A Singleton pattern for Unity.
     * Designed to be derived from
     * 
     * type T The deriving class type
     */
    public class SingletonMonoBehaviour<T> : MonoBehaviour where T : SingletonMonoBehaviour<T>
    {
        // PUBLIC

        [SerializeField]
        private bool dontDestroyOnLoad;

        /**
         * Global instance access
         */
        public static T Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<T>();

                    if (instance.dontDestroyOnLoad)
                    {
                        DontDestroyOnLoad(instance.gameObject);
                    }
                }

                Debug.Assert(instance != null);

                return instance;
            }
        }

        // PRIVATE

        private static T instance;

        // PRIVATE

        protected virtual void Awake()
        {
            instance = (T)this;

            if (dontDestroyOnLoad)
            {
                DontDestroyOnLoad(gameObject);
            }
        }
    }


}