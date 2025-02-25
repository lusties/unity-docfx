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

namespace Lustie.UnityDocfx
{
    public class DocfxSetupTool : EditorWindow
    {
        private string githubRepo = "your-username/your-repo";
        private string unityVersion = "2022.3.27f1";
        private bool includeEditor = false;

        private const string docfxExecutable = "docfx";

        [MenuItem("Lustie/UnityDocfx Generator")]
        public static void ShowWindow()
        {
            GetWindow<DocfxSetupTool>("DocFX Setup Tool");
        }

        void OnGUI()
        {
            GUILayout.Label("Auto Setup DocFX for Unity Project", EditorStyles.boldLabel);


            includeEditor = EditorGUILayout.Toggle("Include Editor Scripts", includeEditor);

            GUILayout.Space(10);


            GUILayout.Space(10);
            if (GUILayout.Button("Copy README.md to index.md"))
            {
                CopyReadmeToIndex();
            }

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
            var txtGithubName = templateContainer.Q<TextField>("txt-git-name");
            var txtUntiyVersion = templateContainer.Q<TextField>("txt-unity-v");


            txtGithubName.value = githubRepo;
            txtGithubName.RegisterValueChangedCallback(evt =>
            {
                githubRepo = evt.newValue;
            });

            txtUntiyVersion.value = unityVersion;
            txtUntiyVersion.RegisterValueChangedCallback(evt =>
            {
                unityVersion = evt.newValue;
            });


            var btnGitAction = templateContainer.Q<Button>("btn-git-action");
            btnGitAction.RegisterCallback<ClickEvent>(evt =>
            {
                GenerateGitHubAction();
            });

            QueryDocfxSettings();
            QueryTOCSettings();
            CreateTOCListView();
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
        }

        void QueryOutputSettings()
        {
            VisualElement outputContainer = rootVisualElement.Q<VisualElement>("output-container");
            var txtDest = outputContainer.Q<TextField>("dest");
            txtDest.BindProperty(docsetSerObj.FindProperty("docfxJson.dest"));

            outputContainer.Q<Button>("btn-output-choose").RegisterCallback<ClickEvent>(evt =>
            {
                string outputDir = EditorUtility.OpenFolderPanel("Choose Output Directory", "", "");
                if(!string.IsNullOrWhiteSpace(outputDir))
                    txtDest.value = outputDir;
            });

            outputContainer.Q<Button>("btn-output-view").RegisterCallback<ClickEvent>(evt =>
            {
                string folderPath = @$"{unityDocset.docfxJson.dest.Replace('/', '\\')}";
                if (Directory.Exists(folderPath))
                {
                    Process.Start("explorer.exe", folderPath);
                }
            });
        }

        #endregion

        #region TABLE OF CONTENTS
        ListView listView;
        int selectedTOCIndex => listView.selectedIndex;

        TextField txtTocName;
        TextField txtTocHref;
        EnumField efSortOption;
        EnumField efSortOrder;

        void QueryTOCSettings()
        {
            VisualElement tocContentContainer = rootVisualElement.Q("toc-content-container");
            txtTocName = tocContentContainer.Q<TextField>("toc-name");
            txtTocHref = tocContentContainer.Q<TextField>("toc-href");
            efSortOption = tocContentContainer.Q<EnumField>("sort-option");
            efSortOrder = tocContentContainer.Q<EnumField>("sort-order");

            txtTocName.RegisterValueChangedCallback(evt =>
            {
                unityDocset.TOC[selectedTOCIndex].name = evt.newValue;
            });
            txtTocHref.RegisterValueChangedCallback(evt =>
            {
                unityDocset.TOC[selectedTOCIndex].href = evt.newValue;
            });
            efSortOption.RegisterValueChangedCallback(evt =>
            {
                unityDocset.TOC[selectedTOCIndex].sortOption = (SortOption)evt.newValue;
            });
            efSortOrder.RegisterValueChangedCallback(evt =>
            {
                unityDocset.TOC[selectedTOCIndex].sortOrder = (SortOrder)evt.newValue;
            });
        }

        void OnTOCItemSelected(TOC toc)
        {
            txtTocName.SetValueWithoutNotify(toc.name);
            txtTocHref.SetValueWithoutNotify(toc.href);
            efSortOption.SetValueWithoutNotify(toc.sortOption);
            efSortOrder.SetValueWithoutNotify(toc.sortOrder);
        }

        void CreateTOCListView()
        {
            listView = rootVisualElement.Q<ListView>();
            listView.selectionType = SelectionType.Single;
            listView.fixedItemHeight = 25;


            listView.showAddRemoveFooter = true;
            listView.reorderable = true;
            //listView.horizontalScrollingEnabled = true;
            listView.reorderMode = ListViewReorderMode.Animated;
            listView.itemsSource = unityDocset.TOC;
            listView.makeItem = MakeItem;
            listView.bindItem = BindItem;
            listView.itemsChosen += (items) =>
            {
                OnTOCItemSelected(items.FirstOrDefault() as TOC);
            };
            OnTOCItemSelected(unityDocset.TOC[listView.selectedIndex = 0]);
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


        /// <summary>
        /// Generate docfx.json
        /// </summary>
        void GenerateDocfxJson()
        {
            if (!DocfxTemplatesPath.LoadTemplates("docfx.json", out string templateFilePath))
            {
                Debug.LogError($"template not found {templateFilePath}");
                return;
            }

            string docfxJson = File.ReadAllText(templateFilePath);
            JObject metadata = JObject.Parse(docfxJson);
            JObject build = metadata.GetJObject("build");

            //build >> global metadata
            JObject globalMetadata = build.GetJObject("globalMetadata");
            globalMetadata.GetJValue("_appTitle").Replace(unityDocset.docfxJson._appTitle);
            globalMetadata.GetJValue("_appFooter").Replace(unityDocset.docfxJson._appFooter);
            globalMetadata.GetJValue("_enableSearch").Replace(unityDocset.docfxJson._enableSearch);

            //dest
            metadata.GetJValue("dest").Replace(unityDocset.docfxJson.dest);


            string docfxFolderPath = Path.Combine(Directory.GetCurrentDirectory(), unityDocset.folder);

            if (!Directory.Exists(docfxFolderPath))
            {
                Directory.CreateDirectory(docfxFolderPath);
            }

            string tocFilePath = Path.Combine(docfxFolderPath, "docfx.json");

            //File.Copy(templateFilePath, tocFilePath, true);
            File.WriteAllText(tocFilePath, metadata.ToString());
            Debug.Log("docfx.json generated at " + tocFilePath);
        }

        /// <summary>
        /// Generate Github workflow
        /// </summary>
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

        /// <summary>
        /// Create or Copy README.md to index.md
        /// </summary>
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
                EditorUtility.DisplayDialog("Success", $"Copied README.md to {indexPath}", "OK");
            }
            else
            {
               
                string defaultIndexContent = @"# Welcome to My Docs

This is an auto-generated **index.md** file because no README.md was found.

Feel free to edit this file to customize your documentation's home page.
";
                File.WriteAllText(indexPath, defaultIndexContent);
                EditorUtility.DisplayDialog("Info", $"No README.md found.\nCreated a default index.md at {indexPath}", "OK");
            }
        }

        /// <summary>
        /// 
        /// </summary>
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

        /// <summary>
        /// RUn DocFX cmd (build or serve)
        /// </summary>
        void RunDocfxCommand(string argument)
        {
            string docfxJsonFullPath = Path.Combine(Directory.GetCurrentDirectory(), unityDocset.folder, "docfx.json");

            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/C {docfxExecutable} \"{docfxJsonFullPath}\" {argument}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            Process process = new Process { StartInfo = psi };

            process.OutputDataReceived += (sender, e) => { if (!string.IsNullOrEmpty(e.Data)) UnityEngine.Debug.Log(e.Data); };
            process.ErrorDataReceived += (sender, e) => { if (!string.IsNullOrEmpty(e.Data)) UnityEngine.Debug.LogError(e.Data); };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit();

            EditorUtility.DisplayDialog("DocFX", $"DocFX command '{argument}' finished with exit code {process.ExitCode}", "OK");
        }
    }
}