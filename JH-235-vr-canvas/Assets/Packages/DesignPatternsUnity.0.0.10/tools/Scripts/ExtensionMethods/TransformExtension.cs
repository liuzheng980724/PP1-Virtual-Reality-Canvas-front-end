using UnityEngine;
using System.Collections.Generic;

namespace Snobal.DesignPatternsUnity_0_0.Extensions
{
    public static class TransformExtensions
    {
        //Breadth-first search
        public static Transform FindDeepChildBreadthFirst(this Transform aParent, string aName)
        {
            Queue<Transform> queue = new Queue<Transform>();
            queue.Enqueue(aParent);
            while (queue.Count > 0)
            {
                var c = queue.Dequeue();
                if (c.name == aName)
                    return c;
                foreach (Transform t in c)
                    queue.Enqueue(t);
            }
            return null;
        }

        //Depth-first search
        public static Transform FindDeepChildDepthFirst(this Transform aParent, string aName)
        {
            foreach (Transform child in aParent)
            {
                if (child.name == aName)
                    return child;
                var result = child.FindDeepChildDepthFirst(aName);
                if (result != null)
                    return result;
            }
            return null;
        }
    }
}
