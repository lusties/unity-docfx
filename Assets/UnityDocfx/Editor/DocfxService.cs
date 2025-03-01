using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text.RegularExpressions;
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
            if(cmdOption == "/K")
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
            string docfxJsonFullPath = Path.Combine(Directory.GetCurrentDirectory(), folder, "docfx.json");
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/C docfx \"{docfxJsonFullPath}\" {argument}",
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
    }
}
