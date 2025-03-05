using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using Unity.Plastic.Newtonsoft.Json;
using Unity.Plastic.Newtonsoft.Json.Serialization;
using UnityEditor;
using UnityEngine;

namespace Lustie.UnityDocfx
{
    /// <summary>
    /// Provides services for interacting with docfx, including checking installation,
    /// installing docfx, opening command prompts, and running docfx commands.
    /// </summary>
    public static class DocfxService
    {
        public static readonly Version maxSupportVersion = new Version("2.61.0");
        public static readonly string uninstallCommand = "dotnet tool uninstall -g docfx";
        public static readonly string installCommand = "dotnet tool install -g docfx --version 2.61.0";

        private static readonly HashSet<string> warnings = new HashSet<string>()
        {
            "No files",
            "Unable to",
            "Invalid",
            "404 (Not Found)",
            "will be ignored",
            "No detected",
        };

        /// <summary>
        /// Checks if docfx is installed and retrieves its version.
        /// </summary>
        /// <param name="version">The version of docfx if installed, otherwise an error message.</param>
        /// <returns>True if docfx is installed, otherwise false.</returns>
        public static bool CheckInstallation(out Version version)
        {
            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = "docfx",
                    Arguments = "--version",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using Process process = Process.Start(startInfo);
                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();

                if (process.ExitCode == 0 && !string.IsNullOrEmpty(output))
                {
                    string v = output[5..output.IndexOf('+')];
                    version = new Version(v);
                    return true;
                }
            }
            catch (Exception)
            {
            }

            version = new Version();
            return false;
        }

        /// <summary>
        /// Installs docfx using the dotnet tool command.
        /// </summary>
        public static void InstallDocfx(EventHandler onDisposed)
        {
            string path = Directory.GetParent(Application.dataPath).FullName;
            string command = "dotnet tool install -g docfx --version 2.61.0";
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                WorkingDirectory = path,
                Arguments = $"/C {command}",
                UseShellExecute = true,
                CreateNoWindow = false,
            };
            using Process process = new Process { StartInfo = startInfo };
            process.Disposed += onDisposed;
            process.Start();
            process.WaitForExit();
        }

        /// <summary>
        /// Opens a command prompt and executes a command.
        /// </summary>
        /// <param name="command">The command to execute.</param>
        /// <param name="onDisposed">Event handler to call when the process is disposed.</param>
        public static void OpenCmd(string command, EventHandler onDisposed, string cmdOption = "/K")
        {
            string path = Directory.GetParent(Application.dataPath).FullName;
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                WorkingDirectory = path,
                Arguments = $"{cmdOption} {command}",
                UseShellExecute = true,
                CreateNoWindow = false,
            };

            using Process process = new Process { StartInfo = startInfo };
            process.Disposed += onDisposed;
            process.Start();
            if (cmdOption == "/K")
                EditorUtility.DisplayDialog("Run Cmd", "See cmd window", "OK");
            process.WaitForExit();
        }

        /// <summary>
        /// Gets a process to run a docfx command with specified arguments and folder.
        /// </summary>
        /// <param name="argument">The arguments to pass to the docfx command.</param>
        /// <param name="folder">The folder containing the docfx.json file.</param>
        /// <returns>A process configured to run the docfx command.</returns>
        public static Process GetDocfxCommand(string argument, string folder)
        {
            string docfxJsonFullPath = string.Join("/", folder, "docfx.json");
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/C docfx {docfxJsonFullPath} {argument}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            Process process = new Process { StartInfo = psi };

            string warningPattern = string.Join("|", warnings);
            Regex warningRegex = new Regex(warningPattern, RegexOptions.IgnoreCase);

            process.OutputDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    if (warningRegex.IsMatch(e.Data))
                        UnityEngine.Debug.LogWarning($"{e.Data}");
                    else
                        UnityEngine.Debug.Log(e.Data);
                }
            };

            process.ErrorDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    if (Regex.IsMatch(e.Data, @"\bCS\d{4}:\b"))
                        UnityEngine.Debug.LogWarning($"[Warning] {e.Data}");
                    else
                        UnityEngine.Debug.LogError($"{e.Data}");
                }
            };

            return process;
        }

        /// <summary>
        /// Generates a docfx.json file for the UnityDocset.
        /// </summary>
        /// <param name="unityDocset">The UnityDocset object containing the docfx.json data.</param>
        public static void GenDocfxJson(UnityDocset unityDocset)
        {
            // Gen buid.content
            GenBuildContent(unityDocset);
            // Gen build.resource
            GenResource(unityDocset);

            var settings = new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new CamelCaseNamingStrategy(),
                    IgnoreSerializableAttribute = true
                },
                Formatting = Formatting.Indented
            };

            string docsetJson = JsonConvert.SerializeObject(unityDocset.docfxJson, settings);

            string docfxFolderPath = unityDocset.currentDocfxFolderPath;

            if (!Directory.Exists(docfxFolderPath))
            {
                Directory.CreateDirectory(docfxFolderPath);
            }

            string tocFilePath = Path.Combine(docfxFolderPath, "docfx.json");

            File.WriteAllText(tocFilePath, docsetJson);
            UnityEngine.Debug.Log("docfx.json generated at " + tocFilePath);
        }

        private static void GenBuildContent(UnityDocset unityDocset)
        {
            // Gen buid.content
            var content = unityDocset.docfxJson.build.content;
            content.Clear();

            bool copyReadme = false;

            foreach (var toc in unityDocset.TOC)
            {
                var hrefOption = toc.hrefOption;
                string href = toc.href[^1] == '/' ? toc.href[..^1] : toc.href;
                if (hrefOption == TOC.HREF_README)
                {
                    content.Add(new Content()
                    {
                        src = "",
                        files = new List<string>() { "toc.yml", "index.md" },
                        dest = ""
                    });

                    if (!copyReadme)
                    {
                        CopyReadmeToIndex(unityDocset.folder);
                        copyReadme = true;
                    }
                    continue;
                }
                if (hrefOption == TOC.HREF_CUSTOM)
                {
                    content.Add(new Content()
                    {
                        src = href,
                        files = new List<string>() { "toc.yml", "*.md" },
                        dest = href
                    });
                    continue;
                }
                content.Add(new Content()
                {
                    src = href,
                    files = new List<string>() { "*.yml" },
                    dest = href
                });
            }
        }

        private static void GenResource(UnityDocset unityDocset)
        {
            var resource = unityDocset.docfxJson.build.resource;
            if (resource.Count == 0)
                resource.Add(new Resource());
            resource[0].files.Clear();
            string _appFaviconPath = unityDocset.docfxJson.build.globalMetadata._appFaviconPath;
            if(!_appFaviconPath.StartsWith("http"))
                resource[0].files.Add(_appFaviconPath);
            string _appLogoPath = unityDocset.docfxJson.build.globalMetadata._appLogoPath;
            if (!_appFaviconPath.StartsWith("http"))
                resource[0].files.Add(_appLogoPath);
        }

        private static void CopyReadmeToIndex(string folderPath)
        {
            string readmePath = Path.Combine(Directory.GetCurrentDirectory(), "README.md");
            string indexPath = Path.Combine(Directory.GetCurrentDirectory(), folderPath, "index.md");


            if (!Directory.Exists(Path.Combine(Directory.GetCurrentDirectory(), folderPath)))
            {
                Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), folderPath));
            }


            if (File.Exists(readmePath))
            {
                File.Copy(readmePath, indexPath, true);
                UnityEngine.Debug.Log($"Copied README.md to {indexPath}");
            }
            else
            {

                if (!DocfxTemplatesPath.LoadTemplates("README_Template.md", out string template))
                {
                    UnityEngine.Debug.LogWarning("No template README.md found.\nNo README_Template.md found.");
                    File.WriteAllText(indexPath, "# Welcome to UnityDocfx\n\nThis is a default index.md file. You can replace this with your own README.md file.");
                    return;
                }
                File.Copy(template, indexPath);
                UnityEngine.Debug.Log($"No README.md found.\nCreated a default index.md at {indexPath}");
            }
        }
    }
}
