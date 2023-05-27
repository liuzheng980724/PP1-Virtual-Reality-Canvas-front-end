using System;
using System.Collections.Generic;
using UnityEngine;

namespace Snobal.DesignPatternsUnity_0_0
{
    // A simple pooling class for gameobjects with single components such as audio sources
    // will create the pool of gameobjecs with component T
    // pool can be accessed in oldest used first using the Get() method
    // will add more overloads to Get() for more pool selection criteria later
    public class SimplePool<T> where T : Component
    {
        // public field for the pool, useful for setting properties on each pool item
        public List<PoolItem<T>> Pool { get; } = new List<PoolItem<T>>();

        public SimplePool(string nameSuffix, int poolSize, Transform parent = null)
        {
            for (int i = 0; i < poolSize; i++)
            {
                Pool.Add(new PoolItem<T>(nameSuffix, i, parent));
            }
        }

        private PoolItem<T> GetOldest()
        {
            var last = Pool[0];
            foreach (var p in Pool)        
                if (p.lastUsed < last.lastUsed)
                    last = p;

            last?.UpdateLastUsed();
            return last;
        }

        public T Get(Func<T, bool> predicate)
        {
            var last = GetOldest();
            foreach (var p in Pool)
                if (predicate.Invoke(p.Component) == true)
                    last = p;

            last?.UpdateLastUsed();
            return last?.Component ?? default(T);
        }

        public T Get()
        {
            return GetOldest().Component;
        }
    }

    public class PoolItem<U> where U : Component
    {
        public U Component { get; private set; }
        public float lastUsed { get; private set; }

        public void UpdateLastUsed()
        {
            lastUsed = Time.time;
        }

        public PoolItem(string nameSuffix, int index, Transform parent)
        {
            GameObject go = new GameObject($"{nameSuffix}{index}");
            Component = go.AddComponent<U>();

            if (parent !=null) 
                go.transform.SetParent(parent);

            lastUsed = Time.time;
        }
    }
}