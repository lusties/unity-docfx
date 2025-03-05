using System;
using System.Collections.Generic;
using System.IO;
using Unity.Plastic.Newtonsoft.Json;
using UnityEngine;

namespace Lustie.UnityDocfx
{
    /// <summary>
    /// Unity Docset data
    /// </summary>
    [CreateAssetMenu(fileName = "UnityDocset", menuName = "UnityDocfx/UnityDocset")]
    public class UnityDocset : ScriptableObject
    {
        /// <summary>
        /// Folder
        /// </summary>
        public string folder = "Documentation";

        /// <summary>
        /// Get current documentaion directory where contains docfx.json
        /// </summary>
        public string currentDocfxFolderPath => Path.Combine(Directory.GetCurrentDirectory(), folder);

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
                    _appName = "Unity Documentaion",
                    _appFooter = "Made by Unity-Docfx: https://github.com/lusties/unity-docfx",
                    _enableSearch = true,
                    _disableContribution = false,
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

                resource = new List<Resource>()
                {
                    new Resource()
                    {
                        files = new List<string>()
                        {
                            "icons/**",
                            "resources/**"
                        }
                    }
                },

                dest = "../docs",

                template = new List<string>()
                {
                    "default",
                }
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

        public string trueHref
        {
            get
            {
                if (hrefOption == HREF_README)
                {
                    return "index.md";
                }
                else if (hrefOption == HREF_CUSTOM)
                {
                    return href;
                }
                return href[^1] != '/' ? $"{href}/" : href;
            }
        }

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
        public List<Resource> resource;
        public string dest;
        public List<string> template;

        [JsonIgnore]
        /// <summary>
        /// Gets the full destination path.
        /// </summary>
        public string fullDest
            => dest.StartsWith("../") ? string.Join('\\', Directory.GetCurrentDirectory(), dest.Replace("../", "")) : dest;
    }

    /// <summary>
    /// template metadata: https://dotnet.github.io/docfx/docs/template.html?tabs=modern#template-metadata
    /// </summary>
    [Serializable]
    public class GlobalMetadata
    {
        [Tooltip("A string append to every page title")]
        public string _appTitle;

        [Tooltip("The footer HTML")]
        public string _appFooter;

        [Tooltip("The name of the site displayed after logo")]
        public string _appName;

        [Tooltip("Path to the app logo")]
        public string _appLogoPath;

        [Tooltip("URL for the app logo")]
        public string _appLogoUrl;

        [Tooltip("Favicon URL path")]
        public string _appFaviconPath;

        [Tooltip("Whether to show the search box")]
        public bool _enableSearch;

        [Tooltip("Whether to show the \"Edit this page\" button")]
        public bool _disableContribution;
    }

    [Serializable]
    public class Content
    {
        public string src;
        public List<string> files;
        public string dest;
    }

    [Serializable]
    public class Resource
    {
        public List<string> files;
    }

    #endregion
}
