using UnityEngine.UIElements;

namespace Lustie.UnityDocfx
{
    public class HelpBox : UnityEngine.UIElements.HelpBox
    {
        public HelpBox() : base()
        {
            
        }

        new class UxmlFactory : UxmlFactory<HelpBox, UxmlTraits> { }

        new class UxmlTraits : UnityEngine.UIElements.HelpBox.UxmlTraits { }
    }
}
