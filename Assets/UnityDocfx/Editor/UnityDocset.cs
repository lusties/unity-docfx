using System;
using System.Collections.Generic;
using System.IO;
using Unity.Plastic.Newtonsoft.Json;
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
        public DocfxData docfxJson = new DocfxData()
        {
            metadata = new List<Metadata>()
            {
                new Metadata()
                {
                    src = new List<Src>()
                    {
                        new Src()
                        {
                            src = "..",
                            files = new List<string>()
                            {
                                "Assets/**/*.cs"
                            },
                            exclude = new List<string>()
                            {
                                "Assets/Plugins/**",
                            }
                        }
                    },
                    globalNamespaceId = "Global",
                    dest = "api"
                }
            },

            build = new Build()
            {
                globalMetadata = new GlobalMetadata()
                {
                    _appTitle = "Unity Docfx",
                    _appFooter = "Made by Unity-Docfx: https://github.com/lusties/unity-docfx",
                    _enableSearch = true
                },

                content = new List<Content>()
                {
                    new Content()
                    {
                        src = "api",
                        files = new List<string>()
                        {
                            "*.yml"
                        },
                        dest = "api"
                    }
                },

                dest = "../docs"
            }
        };

        /// <summary>
        /// Table of contents list
        /// </summary>
        public List<TOC> TOC = new List<TOC>()
        {
            new TOC("Home", "index.md", "README.md to index.md"),
            new TOC("Manual", "manual/", "Custom"),
            new TOC("API", "api/", "api"),
        };

        /// <summary>
        /// Server's port
        /// </summary>
        public int port = 18080;
    }

    #region Table of Contents (toc.yml)

    [Serializable]
    public class TOC
    {
        public string name;
        public string href;
        public SortOption sortOption;
        public SortOrder sortOrder;

        public const string HREF_CUSTOM = "Custom";
        public const string HREF_README = "README.md to index.md";
        public string hrefOption;

        public TOC(string name, string href, string hrefOption)
        {
            this.name = name;
            this.href = href;
            this.hrefOption = hrefOption;
        }
    }

    #endregion

    #region DOCSET (docfx.json)

    [Serializable]
    public class DocfxData
    {
        public List<Metadata> metadata;

        public Build build;
    }

    [Serializable]
    public class Metadata
    {
        public List<Src> src;
        public string globalNamespaceId;
        public string dest;
    }

    [Serializable]
    public class Src
    {
        public string src;
        public List<string> files;
        public List<string> exclude;
    }

    [Serializable]
    public class Build
    {
        public GlobalMetadata globalMetadata;
        public List<Content> content;
        public string dest;


        [JsonIgnore]
        /// <summary>
        /// Gets the full destination path.
        /// </summary>
        public string fullDest
            => dest.StartsWith("../") ? string.Join('\\', Directory.GetCurrentDirectory(), dest.Replace("../", "")) : dest;
    }

    [Serializable]
    public class GlobalMetadata
    {
        public string _appTitle;
        public string _appFooter;
        public bool _enableSearch;
    }

    [Serializable]
    public class Content
    {
        public string src;
        public List<string> files;
        public string dest;

    }

    #endregion
}
