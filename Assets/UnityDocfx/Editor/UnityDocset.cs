using System.Collections.Generic;
using UnityEngine;

namespace Lustie.UnityDocfx
{
    [CreateAssetMenu(fileName = "UnityDocset", menuName = "UnityDocfx/UnityDocset")]
    public class UnityDocset : ScriptableObject
    {
        /// <summary>
        /// Folder
        /// </summary>
        public string folder = "Documentation";

        /// <summary>
        /// docfx.json data
        /// </summary>
        public Docfx docfxJson = new Docfx()
        {
            src = new List<string>()
            {
                "Assets/**/*.cs"
            },

            exclude = new List<string>()
            {
                "Assets/Plugins/**",
            },

            _appTitle = "Unity Docfx title",
            _appFooter = "Unity Docfx footer",
            _enableSearch = true,

            baseUrl = "",

            dest = "../docs",

        };

        /// <summary>
        /// Table of contents list
        /// </summary>
        public List<TOC> TOC = new List<TOC>()
        {
            new TOC("Home", "index.md"),
            new TOC("Manual", "manual/"),
            new TOC("API", "api/"),
        };

        /// <summary>
        /// Server's port
        /// </summary>
        public int port = 18080;
    }
}
