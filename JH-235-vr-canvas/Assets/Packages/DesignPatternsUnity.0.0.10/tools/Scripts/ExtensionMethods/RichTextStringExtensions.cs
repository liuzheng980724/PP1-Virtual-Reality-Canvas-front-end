using UnityEngine;

namespace Snobal.DesignPatternsUnity_0_0.Extensions
{
    public static class RichTextStringExtensions
    {
        public static string Colorize(this string value, Color color)
        {
            return $"<color=#{ColorUtility.ToHtmlStringRGBA(color)}>{value}</color>";
        }
        public static string Bolden(this string value)
        {
            return $"<b>{value}</b>";
        }
        public static string Italics(this string value)
        {
            return $"<i>{value}</i>";
        }

        private static string example = "exampleString".Colorize(Color.blue);
    }
}