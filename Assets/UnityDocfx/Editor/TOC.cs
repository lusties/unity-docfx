using System;

namespace Lustie.UnityDocfx
{
    [Serializable]
    public class TOC
    {
        public string name;
        public string href;
        public SortOption sortOption;
        public SortOrder sortOrder;

        public TOC(string name, string href)
        {
            this.name = name;
            this.href = href;
        }
    }
}
