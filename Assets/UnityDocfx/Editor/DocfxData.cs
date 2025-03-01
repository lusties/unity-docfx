using System;
using System.IO;
using System.Collections.Generic;

namespace Lustie.UnityDocfx
{
    [Serializable]
    public class DocfxData
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

        public string fullDest
        {
            get
            {
                string folderPath = @$"{dest}";
                if (folderPath.StartsWith("../"))
                {
                    return folderPath = string.Join('\\', Directory.GetCurrentDirectory(), folderPath.Replace("../", ""));
                }

                return dest;
            }
        }
    }
}
