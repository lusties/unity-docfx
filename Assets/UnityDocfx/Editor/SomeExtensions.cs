using UnityEngine;

namespace Lustie.UnityDocfx
{
    public static class SomeExtensions
    {
        public static void CopyToClipboard(this string text)
        {
            GUIUtility.systemCopyBuffer = text;
        }
    }
}
