using UnityEngine;
using UnityEngine.UIElements;

namespace Lustie.UnityDocfx
{
    public static class SomeExtensions
    {
        public static void CopyToClipboard(this string text)
        {
            GUIUtility.systemCopyBuffer = text;
        }

        public static void MakeReorderable(this ListView listView)
        {
            listView.showBorder = true;
            listView.showAddRemoveFooter = true;
            listView.reorderable = true;
            listView.reorderMode = ListViewReorderMode.Animated;
            //listView.horizontalScrollingEnabled = true;
        }
    }
}
