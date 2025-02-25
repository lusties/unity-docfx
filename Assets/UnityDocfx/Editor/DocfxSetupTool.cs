using UnityEngine;
using UnityEditor;
using System.IO;
using System.Diagnostics;
using Codice.CM.Common;
using UnityEngine.UIElements;

using Debug = UnityEngine.Debug;
using System.Collections.Generic;
using static UnityEditor.Experimental.GraphView.GraphView;
using System.Linq;
using System.Text;
using Unity.Plastic.Newtonsoft.Json.Linq;
using UnityEditor.UIElements;
using System.Text.RegularExpressions;
using System.Net.Http;
using System;
using System.Threading.Tasks;
using static PlasticPipe.PlasticProtocol.Messages.NegotiationCommand;

namespace Lustie.UnityDocfx
{
    public class DocfxSetupTool : EditorWindow
    {
        private const string Url = "http://localhost:8080";

        private string unityVersion = "2022.3.27f1";
        private bool includeEditor = false;

        private const string docfxExecutable = "docfx";

        [MenuItem("Lustie/UnityDocfx Tool")]
        public static void ShowWindow()
        {
            GetWindow<DocfxSetupTool>("UnityDocfx Tool");
        }

        private void CreateGUI()
        {
            LoadDocsetAsset();

            VisualTreeAsset visualTree = UIPath.LoadUxml("EditorLayout.uxml");
            VisualElement templateContainer = visualTree.Instantiate();
            rootVisualElement.Add(templateContainer);


            var btnBuild = templateContainer.Q<Button>("btn-build");
            btnBuild.RegisterCallback<ClickEvent>(evt =>
            {
                CopyReadmeToIndex();
                GenerateDocfxJson();
                GenerateApiRulesConfig();
                GenerateTocYml();
                //RunDocfxCommand("build");
            });

            var btnServe = templateContainer.Q<Button>("btn-serve");
            btnServe.RegisterCallback<ClickEvent>(evt =>
            {
                RunDocfxCommand("serve");
            });

            templateContainer.Q<TextField>("txt-doc-folder").BindProperty(docsetSerObj.FindProperty("folder"));


            templateContainer.Q<TextField>("txt-git-name").BindProperty(docsetSerObj.FindProperty("docfxJson.baseUrl"));


            var txtUntiyVersion = templateContainer.Q<TextField>("txt-unity-v");


            txtUntiyVersion.value = unityVersion;
            txtUntiyVersion.RegisterValueChangedCallback(evt =>
            {
                unityVersion = evt.newValue;
            });


            templateContainer.Q<Button>("btn-git-action").RegisterCallback<ClickEvent>(evt =>
            {
                GenerateGitHubAction();
            });

            templateContainer.Q<Button>("btn-build-cmd").RegisterCallback<ClickEvent>(evt =>
            {
                BuildAndServeDocfxWithCmd();
            });

            QueryDocfxSettings();
            QueryTOCLSettings();
            QueryOutputSettings();
        }

        UnityDocset unityDocset;
        SerializedObject docsetSerObj;

        void LoadDocsetAsset()
        {
            var docset = Resources.Load<UnityDocfx.UnityDocset>("UnityDocset");
            if (docset == null)
            {
                docset = ScriptableObject.CreateInstance<UnityDocfx.UnityDocset>();
                AssetDatabase.CreateAsset(docset, "");
                AssetDatabase.SaveAssets();
            }


            unityDocset = docset;
            docsetSerObj = new SerializedObject(unityDocset);
            rootVisualElement.Bind(docsetSerObj);
        }

        #region QUERY AND BINDING
        void QueryDocfxSettings()
        {
            VisualElement docfxSettingsContainer = rootVisualElement.Q<VisualElement>("docfx-s-container");
            docfxSettingsContainer.Q<TextField>("_appTitle").BindProperty(docsetSerObj.FindProperty("docfxJson._appTitle"));
            docfxSettingsContainer.Q<TextField>("_appFooter").BindProperty(docsetSerObj.FindProperty("docfxJson._appFooter"));
            docfxSettingsContainer.Q<Toggle>("_enableSearch").BindProperty(docsetSerObj.FindProperty("docfxJson._enableSearch"));

            docfxSettingsContainer.Q<ListView>("list-src").BindProperty(docsetSerObj.FindProperty("docfxJson.src"));
            docfxSettingsContainer.Q<ListView>("list-exclude").BindProperty(docsetSerObj.FindProperty("docfxJson.exclude"));
        }

        void QueryOutputSettings()
        {
            VisualElement outputContainer = rootVisualElement.Q<VisualElement>("output-container");
            var txtDest = outputContainer.Q<TextField>("dest");
            txtDest.BindProperty(docsetSerObj.FindProperty("docfxJson.dest"));

            outputContainer.Q<Button>("btn-output-choose").RegisterCallback<ClickEvent>(evt =>
            {
                string outputDir = EditorUtility.OpenFolderPanel("Choose Output Directory", "", "");
                if (!string.IsNullOrWhiteSpace(outputDir))
                    txtDest.value = outputDir;
            });

            outputContainer.Q<Button>("btn-output-view").RegisterCallback<ClickEvent>(evt =>
            {
                string folderPath = @$"{unityDocset.docfxJson.dest.Replace('/', '\\')}";
                if (Directory.Exists(folderPath))
                {
                    Process.Start("explorer.exe", folderPath);
                }
                else
                {
                    EditorUtility.DisplayDialog("Path doesn't exist", $"Path: {folderPath} DOES NOT EXIST", "OK");
                }
            });

            // Go live
            outputContainer.Q<Button>("btn-go-live").RegisterCallback<ClickEvent>(evt =>
            {
                GoLive();
            });
        }

        #endregion

        #region TABLE OF CONTENTS
        ListView listViewTOC;
        int selectedTOCIndex => listViewTOC.selectedIndex;

        void OnTOCItemSelected()
        {
            SerializedProperty selectedElementSP = docsetSerObj.FindProperty("TOC").GetArrayElementAtIndex(selectedTOCIndex);

            VisualElement tocContentContainer = rootVisualElement.Q("toc-content-container");
            tocContentContainer.Q<TextField>("toc-name").BindProperty(selectedElementSP.FindPropertyRelative("name"));
            tocContentContainer.Q<TextField>("toc-href").BindProperty(selectedElementSP.FindPropertyRelative("href"));
            tocContentContainer.Q<EnumField>("sort-option").BindProperty(selectedElementSP.FindPropertyRelative("sortOption"));
            tocContentContainer.Q<EnumField>("sort-order").BindProperty(selectedElementSP.FindPropertyRelative("sortOrder"));
        }

        void QueryTOCLSettings()
        {
            listViewTOC = rootVisualElement.Q<ListView>("list-toc");
            listViewTOC.selectionType = SelectionType.Single;
            listViewTOC.fixedItemHeight = 25;
            listViewTOC.showBorder = true;
            listViewTOC.showAddRemoveFooter = true;
            listViewTOC.reorderable = true;
            //listView.horizontalScrollingEnabled = true;
            listViewTOC.reorderMode = ListViewReorderMode.Animated;
            listViewTOC.itemsSource = unityDocset.TOC;
            listViewTOC.makeItem = MakeItem;
            listViewTOC.bindItem = BindItem;
            listViewTOC.itemsChosen += (items) => { OnTOCItemSelected(); };
            listViewTOC.selectedIndex = 0;
            OnTOCItemSelected();
        }

        VisualElement MakeItem()
        {
            VisualElement tocElement = new VisualElement();
            Label lbName = new Label();
            lbName.name = "txt-toc-name";

            tocElement.Add(lbName);
            return tocElement;
        }

        void BindItem(VisualElement e, int i)
        {
            e.Q<Label>("txt-toc-name").text = unityDocset.TOC[i].name;
        }
        #endregion

        void SaveDocset()
        {
            EditorUtility.SetDirty(unityDocset);
            AssetDatabase.SaveAssetIfDirty(unityDocset);
        }

        private void OnDisable()
        {
            SaveDocset();
        }

        #region FILES GENERATOR

        void GenerateDocfxJson()
        {
            if (!DocfxTemplatesPath.LoadTemplates("docfx.json", out string templateFilePath))
            {
                Debug.LogError($"template not found {templateFilePath}");
                return;
            }

            string docfxJson = File.ReadAllText(templateFilePath);
            JObject docfxRoot = JObject.Parse(docfxJson);

            //metadata.src files:
            JArray metadata = docfxRoot.GetValue<JArray>("metadata");
            JObject srcObj = metadata[0] as JObject;
            JArray srcArr = srcObj.GetValue<JArray>("src");
            JObject srcArrObj = srcArr[0] as JObject;
            srcArrObj.GetValue<JArray>("files").ReplaceAll(unityDocset.docfxJson.src);
            srcArrObj.GetValue<JArray>("exclude").ReplaceAll(unityDocset.docfxJson.exclude);

            JObject build = docfxRoot.GetJObject("build");

            //build.global metadata
            JObject globalMetadata = build.GetJObject("globalMetadata");
            globalMetadata.GetJValue("_appTitle").Replace(unityDocset.docfxJson._appTitle);
            globalMetadata.GetJValue("_appFooter").Replace(unityDocset.docfxJson._appFooter);
            globalMetadata.GetJValue("_enableSearch").Replace(unityDocset.docfxJson._enableSearch);

            //dest
            build.GetJValue("dest").Replace(unityDocset.docfxJson.dest);

            string docfxFolderPath = Path.Combine(Directory.GetCurrentDirectory(), unityDocset.folder);

            if (!Directory.Exists(docfxFolderPath))
            {
                Directory.CreateDirectory(docfxFolderPath);
            }

            string tocFilePath = Path.Combine(docfxFolderPath, "docfx.json");

            //File.Copy(templateFilePath, tocFilePath, true);
            File.WriteAllText(tocFilePath, docfxRoot.ToString());
            Debug.Log("docfx.json generated at " + tocFilePath);
        }

        void GenerateGitHubAction()
        {
            string workflowContent = @"name: Build and Deploy DocFX
on:
  push:
    branches:
      - main

jobs:
  build:
    runs-on: ubuntu-latest
    steps:
      - name: Checkout Repository
        uses: actions/checkout@v3

      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '8.0.x'

      - name: Install DocFX
        run: dotnet tool install -g docfx

      - name: Build Documentation
        run: docfx ""docfx.json""

      - name: Deploy to GitHub Pages
        uses: JamesIves/github-pages-deploy-action@v4
        with:
          branch: gh-pages
          folder: _site";

            string workflowsFolder = Path.Combine(Application.dataPath, "../.github/workflows");
            if (!Directory.Exists(workflowsFolder))
                Directory.CreateDirectory(workflowsFolder);

            string workflowPath = Path.Combine(workflowsFolder, "docfx.yml");
            File.WriteAllText(workflowPath, workflowContent);
            EditorUtility.DisplayDialog("Success", "GitHub Action workflow generated at " + workflowPath, "OK");
        }

        private void CopyReadmeToIndex()
        {
            string readmePath = Path.Combine(Directory.GetCurrentDirectory(), "README.md");
            string indexPath = Path.Combine(Directory.GetCurrentDirectory(), unityDocset.folder, "index.md");


            if (!Directory.Exists(Path.Combine(Directory.GetCurrentDirectory(), unityDocset.folder)))
            {
                Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), unityDocset.folder));
            }


            if (File.Exists(readmePath))
            {
                File.Copy(readmePath, indexPath, true);
                Debug.Log($"Copied README.md to {indexPath}");
            }
            else
            {

                string defaultIndexContent = @"# Welcome to My Docs

This is an auto-generated **index.md** file because no README.md was found.

Feel free to edit this file to customize your documentation's home page.
";
                File.WriteAllText(indexPath, defaultIndexContent);
                Debug.Log($"No README.md found.\nCreated a default index.md at {indexPath}");
            }
        }

        void GenerateApiRulesConfig()
        {
            if (!DocfxTemplatesPath.LoadTemplates("apiRules.yml", out string templateFilePath))
            {
                Debug.LogError($"template not found {templateFilePath}");
                return;
            }

            string docfxFolderPath = Path.Combine(Directory.GetCurrentDirectory(), unityDocset.folder);

            if (!Directory.Exists(docfxFolderPath))
            {
                Directory.CreateDirectory(docfxFolderPath);
            }

            string tocFilePath = Path.Combine(docfxFolderPath, "apiRules.yml");

            File.Copy(templateFilePath, tocFilePath, true);
            Debug.Log("apiRules.yml generated at " + tocFilePath);
        }

        private void GenerateTocYml()
        {
            /*            string docfxFolderPath = Path.Combine(Directory.GetCurrentDirectory(), docfxFolder);

                        if (!Directory.Exists(docfxFolderPath))
                        {
                            Directory.CreateDirectory(docfxFolderPath);
                        }

                        string tocFilePath = Path.Combine(docfxFolderPath, "toc.yml");
                        if (DocfxTemplatesPath.LoadTemplates("toc.yml", out string packagePath))
                        {
                            File.Copy(packagePath, tocFilePath, true);
                            EditorUtility.DisplayDialog("Success", $"toc.yml copied from template at {packagePath}", "OK");
                        }
                        else
                        {
                            EditorUtility.DisplayDialog("Error", $"Template toc.yml not found at {packagePath}", "OK");
                        }*/

            string docfxFolderPath = Path.Combine(Directory.GetCurrentDirectory(), unityDocset.folder);

            List<TOC> TOC = unityDocset.TOC;
            StringBuilder docTOCContent = new StringBuilder();
            foreach (TOC toc in TOC)
            {
                // Main TOC
                docTOCContent.AppendLine($"- name: {toc.name}");
                docTOCContent.AppendLine($"  href: {toc.href}");

                // TOC Directory
                if (toc.href == "api/")
                {
                    continue;
                }

                if (toc.href.Contains(".md") || !toc.href.Contains("/"))
                {
                    continue;
                }

                // Create Directory
                string tocFolderPath = Path.Combine(docfxFolderPath, toc.href);
                var directoryInfo = Directory.CreateDirectory(tocFolderPath);
                StringBuilder tocContent = new StringBuilder();

                var fileInfos = directoryInfo.GetFiles();
                foreach (var fileInfo in fileInfos)
                {
                    // Content must be markdown file
                    if (Path.GetExtension(fileInfo.FullName) != ".md")
                        continue;
                    tocContent.AppendLine($"- name: {Path.GetFileNameWithoutExtension(fileInfo.FullName)}");
                    tocContent.AppendLine($"  href: {fileInfo.Name}");
                }

                //  Create toc.yml
                string currentTOCFilePath = Path.Combine(tocFolderPath, $"toc.yml");
                File.WriteAllText(currentTOCFilePath, tocContent.ToString());
            }

            string tocFilePath = Path.Combine(docfxFolderPath, "toc.yml");
            File.WriteAllText(tocFilePath, docTOCContent.ToString());
        }

        #endregion

        #region BUILD / SERVE DOCFX

        void BuildAndServeDocfxWithCmd()
        {
            string path = Directory.GetParent(Application.dataPath).FullName;
            string command = $"copy this command to build: docfx {unityDocset.folder}/docfx.json --serve";
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                WorkingDirectory = path,
                Arguments = $"/K echo {command}",
                UseShellExecute = true,
                CreateNoWindow = false,
            };

            using Process process = new Process { StartInfo = startInfo };
            process.Disposed += (e, i) => { Debug.Log($"build with cmd ended"); };
            process.Start();
            EditorUtility.DisplayDialog("Being build with cmd", "See cmd window", "OK");
            process.WaitForExit();
        }

        void RunDocfxCommand(string argument)
        {
            string docfxJsonFullPath = Path.Combine(Directory.GetCurrentDirectory(), unityDocset.folder, "docfx.json");
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/C {docfxExecutable} \"{docfxJsonFullPath}\" {argument}",
                //WorkingDirectory = Directory.GetParent(Application.dataPath).FullName,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using Process process = new Process { StartInfo = psi };

            HashSet<string> warnings = new HashSet<string>()
            {
                "No files",
                "Unable to",
                "Invalid",
                "404 (Not Found)",
                "will be ignored",
                "No detected",
            };

            string warningPattern = string.Join("|", warnings);
            Regex warningRegex = new Regex(warningPattern, RegexOptions.IgnoreCase);

            process.OutputDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    if (warningRegex.IsMatch(e.Data))
                        UnityEngine.Debug.LogWarning($"[Warning] {e.Data}");
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
                        UnityEngine.Debug.LogError($"[Error] {e.Data}");
                }
            };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit();
            process.Exited += (sender, e) => { Debug.Log("finished"); };
            process.Disposed += (sender, e) => { Debug.Log("finished"); };

            int port = 18080;
            string folderPath = unityDocset.docfxJson.dest;

            if(server)
            {
                server.Dispose();
            }
            server = new LiveServer(folderPath, port);
            server.Run();
            Application.OpenURL(server.GetUrl());
        }

        #endregion

        #region

        static LiveServer server;

        void GoLive()
        {
            int port = 18080;
            string folderPath = unityDocset.docfxJson.dest;

            if (server)
            {
                server.Dispose();
            }
            server = new LiveServer(folderPath, port);
            server.Run();
            //Application.OpenURL(server.GetUrl());
        }
        
        #endregion
    }
}