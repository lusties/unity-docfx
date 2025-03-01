using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Unity.Plastic.Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using static PlasticPipe.PlasticProtocol.Messages.NegotiationCommand;
using Debug = UnityEngine.Debug;

namespace Lustie.UnityDocfx
{
    public class DocfxSetupTool : EditorWindow
    {
        private string unityVersion = "2022.3.27f1";
        private bool includeEditor = false;

        static UnityDocset unityDocset;
        static SerializedObject uniDocsetSerObj;

        #region Initialize
        private void CreateGUI()
        {
            VisualTreeAsset visualTree = UIPath.LoadUxml("EditorLayout.uxml");
            VisualElement templateContainer = visualTree.Instantiate();
            rootVisualElement.Add(templateContainer);

            if (unityDocset == null)
            {
                LoadDocsetAsset();
                SetupUIElements();
            }
        }

        void SetupUIElements()
        {
            VisualElement templateContainer = rootVisualElement.contentContainer;
            var btnBuild = templateContainer.Q<Button>("btn-build");
            btnBuild.RegisterCallback<ClickEvent>(evt =>
            {
                CopyReadmeToIndex();
                GenerateDocfxJson();
                GenerateApiRulesConfig();
                GenerateTocYml();
            });

            var btnServe = templateContainer.Q<Button>("btn-serve");
            btnServe.RegisterCallback<ClickEvent>(evt =>
            {
                ServeDocfxCommand();
            });

            templateContainer.Q<TextField>("txt-doc-folder").BindProperty(FindProp("folder"));
            templateContainer.Q<TextField>("txt-git-name").BindProperty(FindProp("docfxJson.baseUrl"));

            //templateContainer.Q<TextField>("txt-doc-folder").bindingPath = "folder";
            //templateContainer.Q<TextField>("txt-git-name").bindingPath = "docfxJson.baseUrl";


            templateContainer.Q<TextField>("txt-unity-v");


            templateContainer.Q<Button>("btn-git-action").RegisterCallback<ClickEvent>(evt =>
            {
                GenerateGitHubAction();
            });

            templateContainer.Q<Button>("btn-build-cmd").RegisterCallback<ClickEvent>(evt =>
            {
                BuildAndServeDocfxWithCmd();
            });

            InstallationCheck();

            QueryDocfxSettings();
            QueryTOCLSettings();
            QueryOutputSettings();
            QueryServerSettings();
        }

        void LoadDocsetAsset()
        {
            var docsets = Resources.LoadAll<UnityDocfx.UnityDocset>("UnityDocfx");
            var docset = docsets.FirstOrDefault();
            //Debug.Log(docset);
            if (docset == null)
            {
                docset = ScriptableObject.CreateInstance<UnityDocfx.UnityDocset>();
                Directory.CreateDirectory("Assets/Editor/Resources/UnityDocfx");
                string path = AssetDatabase.GenerateUniqueAssetPath("Assets/Editor/Resources/UnityDocfx/unity-docset.asset");
                AssetDatabase.CreateAsset(docset, $"{path}");
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                EditorUtility.FocusProjectWindow();
                Selection.activeObject = docset;
            }
            Bind(docset);
        }

        void Bind(UnityDocset docset)
        {
            unityDocset = docset;
            uniDocsetSerObj = new SerializedObject(unityDocset);
            rootVisualElement.Bind(uniDocsetSerObj);
        }

        #endregion

        void InstallationCheck()
        {
            // Check if docfx is installed
            bool isDocfxInstalled = DocfxService.CheckInstallation(out Version currentVersion);
            //isDocfxInstalled = true;
            (HelpBoxMessageType helpBoxType, string helpBoxMessage) = isDocfxInstalled ?
                (HelpBoxMessageType.Info, $"docfx version: {currentVersion}") :
                (HelpBoxMessageType.Error, "docfx is not installed. Please install docfx first.");

            // Check if the installed version is supported
            //Version maxSupportVersion = new Version("2.61.0");
            Version maxSupportVersion = DocfxService.maxSupportVersion;
            bool isVersionSupported = currentVersion <= maxSupportVersion;
            //isVersionSupported = false;

            if (!isVersionSupported && isDocfxInstalled)
            {
                helpBoxType = HelpBoxMessageType.Warning;
                helpBoxMessage = $"docfx version: {currentVersion} is not supported. " +
                    $"Versions higher than {maxSupportVersion} may not work correctly.";
            }

            //Help box message
            HelpBox helpBox = new HelpBox(helpBoxMessage, helpBoxType);
            helpBox.style.flexShrink = helpBox.style.flexGrow = 1;

            VisualElement installInfoContainer = rootVisualElement.Q<VisualElement>("docfx").Q<VisualElement>("install-info");
            installInfoContainer.Insert(0, helpBox);

            // Install buttons
            Button btn_i1 = installInfoContainer.Q<Button>("btn-i-1");
            Button btn_i2 = installInfoContainer.Q<Button>("btn-i-2");

            const string newLine = " & echo ";
            string uninstallCommand = DocfxService.uninstallCommand;
            string installCommand = DocfxService.installCommand;

            if (!isDocfxInstalled)
            {
                btn_i1.text = "Install docfx";
                btn_i2.text = "Install docfx (Cmd)";

                btn_i1.RegisterCallback<ClickEvent>(evt =>
                {
                    DocfxService.InstallDocfx(static (e, i) => { Debug.Log("Installed"); });
                });

                btn_i2.RegisterCallback<ClickEvent>(evt =>
                {
                    string command = "echo" +
                                     $"{installCommand} {newLine}" +
                                     $"press right mouse or Ctrl + v to paste command";

                    $"{installCommand}".CopyToClipboard();
                    DocfxService.OpenCmd(command, static (e, i) => { Debug.Log($"install with cmd ended"); });

                });
            }
            else if (!isVersionSupported)
            {
                btn_i1.text = "Install another version";
                btn_i2.text = "Install another version (Cmd)";

                btn_i1.RegisterCallback<ClickEvent>(evt =>
                {
                    DocfxService.OpenCmd($"{uninstallCommand} & {installCommand}", static (e, i) => { Debug.Log($"install with cmd ended"); }, "/C");
                });

                btn_i2.RegisterCallback<ClickEvent>(evt =>
                {
                    string command = "echo" +
                                     $" {uninstallCommand} {newLine}" +
                                     $"{installCommand} {newLine}" +
                                     $"press right mouse or Ctrl + v to paste these commands";

                    $"{uninstallCommand}\n{installCommand}".CopyToClipboard();
                    DocfxService.OpenCmd(command, static (e, i) => { Debug.Log($"install with cmd ended"); });
                });
            }

            if(isDocfxInstalled && isVersionSupported)
            {
                btn_i1.style.display = DisplayStyle.None;
                btn_i2.style.display = DisplayStyle.None;
            }
        }

        #region QUERY AND BINDING
        void QueryDocfxSettings()
        {
            VisualElement docfxSettingsContainer = rootVisualElement.Q<VisualElement>("docfx-s-container");
            docfxSettingsContainer.Q<TextField>("_appTitle").BindProperty(FindProp("docfxJson._appTitle"));
            docfxSettingsContainer.Q<TextField>("_appFooter").BindProperty(FindProp("docfxJson._appFooter"));
            docfxSettingsContainer.Q<Toggle>("_enableSearch").BindProperty(FindProp("docfxJson._enableSearch"));

            docfxSettingsContainer.Q<ListView>("list-src").BindProperty(FindProp("docfxJson.src"));
            docfxSettingsContainer.Q<ListView>("list-exclude").BindProperty(FindProp("docfxJson.exclude"));
        }

        void QueryOutputSettings()
        {
            VisualElement outputContainer = rootVisualElement.Q<VisualElement>("output-container");
            var txtDest = outputContainer.Q<TextField>("dest");
            txtDest.BindProperty(FindProp("docfxJson.dest"));

            outputContainer.Q<Button>("btn-output-browse").RegisterCallback<ClickEvent>(evt =>
            {
                string outputDir = EditorUtility.OpenFolderPanel("Browse Output Directory", "", "");
                if (!string.IsNullOrWhiteSpace(outputDir))
                    txtDest.value = outputDir;
            });

            outputContainer.Q<Button>("btn-output-view").RegisterCallback<ClickEvent>(evt =>
            {
                string folderPath = unityDocset.docfxJson.fullDest;
                if (Directory.Exists(folderPath))
                {
                    Process.Start("explorer.exe", folderPath);
                }
                else
                {
                    EditorUtility.DisplayDialog("Path doesn't exist", $"Path: {folderPath} DOES NOT EXIST", "OK");
                }
            });
        }

        void QueryServerSettings()
        {
            VisualElement footer = rootVisualElement.Q<VisualElement>("footer");
            // Go live
            Button btnServer = footer.Q<Button>("btn-go-live");
            btnServer.RegisterCallback<ClickEvent>(evt =>
            {
                if (isServerLiving)
                {
                    DisposeServer();
                    btnServer.text = "Go live";
                    return;
                }

                GoLiveServer();
                btnServer.text = "Dispose";
            });

            footer.Q<IntegerField>("txt-port").bindingPath = "port";
        }

        #endregion

        #region TABLE OF CONTENTS
        ListView listViewTOC;
        int selectedTOCIndex => listViewTOC.selectedIndex;

        void OnTOCItemSelected()
        {
            SerializedProperty selectedElementSP = FindProp("TOC").GetArrayElementAtIndex(selectedTOCIndex);

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
            string command = $"docfx {unityDocset.folder}/docfx.json --serve";
            command.CopyToClipboard();

            string cmd = $"echo copy this command to build: {command} & echo " +
                         $"or press right mouse / Ctrl + v to paste the command"; ;
            DocfxService.OpenCmd(cmd, static (e, i) => { Debug.Log($"build with cmd ended"); });
        }

        void ServeDocfxCommand()
        {
            using Process docfxProcess = DocfxService.GetDocfxCommand("serve", unityDocset.folder);
            docfxProcess.Start();
            docfxProcess.BeginOutputReadLine();
            docfxProcess.BeginErrorReadLine();
            docfxProcess.WaitForExit();
            docfxProcess.Exited += (sender, e) => { Debug.Log("finished"); };
            docfxProcess.Disposed += (sender, e) => { Debug.Log("finished"); };

            int port = 18080;
            string folderPath = unityDocset.docfxJson.dest;

            if (server)
            {
                server.Dispose();
            }
            server = new LiveServer(folderPath, port);
            server.Run();
            Application.OpenURL(server.GetUrl());
        }

        #endregion

        #region LIVE SERVER

        static LiveServer server;
        static bool isServerLiving => server != null && server.IsRunning;

        void GoLiveServer()
        {
            int port = unityDocset.port;
            string folderPath = unityDocset.docfxJson.fullDest;

            server = new LiveServer(folderPath, port);
            server.Run();
            //Application.OpenURL(server.GetUrl());
        }

        void DisposeServer()
        {
            if (server) server.Dispose();
        }

        #endregion

        SerializedProperty FindProp(string propName)
        {
            return uniDocsetSerObj.FindProperty(propName);
        }

        void SaveDocset()
        {
            if (unityDocset == null)
                return;
            EditorUtility.SetDirty(unityDocset);
            AssetDatabase.SaveAssetIfDirty(unityDocset);
        }

        private void OnDisable()
        {
            SaveDocset();
            uniDocsetSerObj.ApplyModifiedProperties();
            unityDocset = null;
            uniDocsetSerObj = null;
            if (isServerLiving) DisposeServer();
        }

        #region OPEN EDITOR
        [MenuItem("Window/UnityDocfx Tool")]
        public static void Open()
        {
            var window = GetWindow();
            window.LoadDocsetAsset();
        }

        public static void ShowWindow(UnityDocset unityDocset)
        {
            var window = GetWindow();
            window.Bind(unityDocset);
        }

        public static DocfxSetupTool GetWindow()
        {
            return GetWindow<DocfxSetupTool>("UnityDocfx Tool");
        }

        [OnOpenAsset(0)]
        public static bool OpenAsset(int instanceID, int line)
        {
            var unityDocsetAsset = EditorUtility.InstanceIDToObject(instanceID) as UnityDocset;

            if (unityDocsetAsset == null)
                return false;

            ShowWindow(unityDocsetAsset);

            return true;
        }
        #endregion
    }
}