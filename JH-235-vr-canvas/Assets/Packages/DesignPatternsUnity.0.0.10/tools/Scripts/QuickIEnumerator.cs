using System;
using System.Collections;
using UnityEngine;

namespace Snobal.DesignPatternsUnity_0_0
{
    public static class QuickIEnumerator
    {
        public static IEnumerator DelayFrames(int frames, System.Action callback)
        {
            for (int i = 0; i < frames; i++)
                yield return null;

            callback?.Invoke();
        }

        public static IEnumerator DelayTime(float duration, System.Action callback)
        {
            yield return new WaitForSeconds(duration);
            callback?.Invoke();
        }

        /// <summary>
        /// Run an iterator function that might throw an exception. Call the callback with the exception
        /// if it does or null if it finishes without throwing an exception.
        /// </summary>
        /// <param name="enumerator">Iterator function to run</param>
        /// <param name="onException">Callback to call when the iterator has thrown an exception or finished.
        /// The thrown exception or null is passed as the parameter.</param>
        /// <returns>An enumerator that runs the given enumerator</returns>
        public static IEnumerator ExceptionIEnumerator<T>(IEnumerator enumerator, Action<T> onException)
            where T : Exception
		{
			while (true)
			{
				object current;
				try
				{
					if (enumerator.MoveNext() == false)
						break;
                    
					current = enumerator.Current;
				}
				catch (T ex)
				{
					onException(ex);
					yield break;
				}
				yield return current;
			}			
		}
	}
}