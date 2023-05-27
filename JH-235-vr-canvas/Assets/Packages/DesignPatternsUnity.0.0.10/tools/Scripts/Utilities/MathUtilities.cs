using UnityEngine;

namespace Snobal.DesignPatternsUnity_0_0.Utilities
{
    public static class Math
    {
        /// <summary>
        /// Allows you to modulo negative numbers returning absolute values
        /// </summary>
        /// <param name="x"></param>
        /// <param name="m"></param>
        /// <returns></returns>
        public static int Mod(int x, int m)
        {
            return (x % m + m) % m;
        }

        /// <summary>
        /// allows you to ping pong integers, similar to Mathf.PingPong for floats, will inclusively return t when t = length
        /// </summary>
        /// <param name="t"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static int PingPong(int t, int length)
        {
            int q = t / length;
            int r = t % length;

            if ((q % 2) == 0)
                return r;
            else
                return length - r;
        }
    }
}