using System;
using System.Collections.Generic;

namespace Lustie.UnityDocfx
{
    [Serializable]
    public class Docfx
    {
        /// <summary>
        /// metadata.src.files
        /// </summary>
        public List<string> src;

        /// <summary>
        /// metadata.src.exclude
        /// </summary>
        public List<string> exclude;

        /// <summary>
        /// build.globalMetadata._appTitle
        /// </summary>
        public string _appTitle;

        /// <summary>
        ///  build.globalMetadata._appFooter
        /// </summary>
        public string _appFooter;

        /// <summary>
        /// build.globalMetadata._enableSearch
        /// </summary>
        public bool _enableSearch;

        /// <summary>
        /// sitemap.baseUrl
        /// </summary>
        public string baseUrl;

        /// <summary>
        /// build.dest
        /// </summary>
        public string dest;

    }
}
