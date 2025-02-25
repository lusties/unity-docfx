using System.Collections.Generic;
using UnityEngine;

namespace Lustie.UnityDocfx
{
    [CreateAssetMenu(fileName = "UnityDocset", menuName = "UnityDocfx/Docset")]
    public class UnityDocset : ScriptableObject
    {
        public string folder = "Documentation";

        public Docfx docfxJson = new Docfx()
        {
            _appTitle = "Unity Docfx title",
            _appFooter = "Unity Docfx footer",
            _enableSearch = true,
            dest = "../_site",

        };

        public List<TOC> TOC = new List<TOC>()        
        {
            new TOC("Home", "index.md"),
            new TOC("Manual", "manual/"),
            new TOC("API", "api/"),
        };
    }
}
