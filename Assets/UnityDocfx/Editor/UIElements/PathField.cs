using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Lustie.UnityDocfx
{
    public class PathField : VisualElement, IBindable, INotifyValueChanged<string>
    {
        public string value
        {
            get => txtPath.value;
            set
            {
                if (txtPath.value == value)
                    return;
                using var evt = ChangeEvent<string>.GetPooled(txtPath.value, value);
                evt.target = this;
                SendEvent(evt);
                txtPath.value = value;
            }
        }

        public string label
        {
            get => txtPath.label;
            set => txtPath.label = value;
        }

        public string buttonBrowseLabel
        {
            get => btnBrowse.text;
            set => btnBrowse.text = value;
        }

        public string buttonViewLabel
        {
            get => btnView.text;
            set => btnView.text = value;
        }

        public PathType pathType { get; set; }

        public string extensions { get; set; }

        public string FullPath
            => value.StartsWith("../") ? string.Join('\\', Directory.GetCurrentDirectory(), value.Replace("../", "")) : value;

        public IBinding binding { get => txtPath.binding; set => txtPath.binding = value; }
        public string bindingPath { get => txtPath.bindingPath; set => txtPath.bindingPath = value; }

        #region Child Elements
        private TextField txtPath;
        private Button btnBrowse;
        private Button btnView;
        #endregion


        public PathField()
        {
            // Container styles
            this.style.flexDirection = FlexDirection.Row;

            txtPath = new TextField();
            txtPath.name = "txt-path";
            txtPath.style.flexGrow = 1;
            txtPath.style.flexShrink = 1;

            btnBrowse = new Button();
            btnBrowse.name = "btn-browse";
            btnBrowse.style.width = new StyleLength(new Length(9.5f, LengthUnit.Percent));

            btnView = new Button();
            btnView.name = "btn-view";
            btnView.style.width = new StyleLength(new Length(9.5f, LengthUnit.Percent));

            // button events
            btnBrowse.RegisterCallback<ClickEvent>(evt => { Browse(); });

            btnView.RegisterCallback<ClickEvent>(evt => { ViewInPanel(); });

            hierarchy.Add(txtPath);
            hierarchy.Add(btnBrowse);
            hierarchy.Add(btnView);
        }

        void Browse()
        {
            string outputDir = pathType == PathType.Directory ? EditorUtility.OpenFolderPanel("Browse Folder", "", "")
                : EditorUtility.OpenFilePanel("Browse File", "", ".jpg");
            if (!string.IsNullOrWhiteSpace(outputDir))
                this.value = outputDir;
        }

        void ViewInPanel()
        {
            string folderPath = FullPath;
            if (Uri.IsWellFormedUriString(folderPath, UriKind.Absolute) && (folderPath.StartsWith("http://") || folderPath.StartsWith("https://")))
            {
                Application.OpenURL(folderPath);
            }
            else if (Directory.Exists(folderPath))
            {
                Process.Start("explorer.exe", folderPath);
            }
            else
            {
                EditorUtility.DisplayDialog("Path doesn't exist", $"Path: {folderPath} DOES NOT EXIST", "OK");
            }
        }

        public void SetValueWithoutNotify(string newValue)
        {
            txtPath.SetValueWithoutNotify(newValue);
        }

        public enum PathType
        {
            File,
            Directory
        }

        new class UxmlFactory : UxmlFactory<PathField, UxmlTraits> { }

        new class UxmlTraits : BindableElement.UxmlTraits
        {
            UxmlStringAttributeDescription m_labelAttr = new UxmlStringAttributeDescription { name = "Label", defaultValue = "Path" };
            UxmlStringAttributeDescription m_PathAttr = new UxmlStringAttributeDescription { name = "value", defaultValue = "C:/" };
            UxmlStringAttributeDescription m_BtnBrowseLabelAttr = new UxmlStringAttributeDescription { name = "button-Browse-Label", defaultValue = "Browse" };
            UxmlStringAttributeDescription m_BtnViewLabelAttr = new UxmlStringAttributeDescription { name = "button-View-Label", defaultValue = "View" };
            UxmlEnumAttributeDescription<PathType> m_PathTypeAttr = new UxmlEnumAttributeDescription<PathType> { name = "path-type", defaultValue = PathType.Directory };

            public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
            {
                get { yield return new UxmlChildElementDescription(typeof(VisualElement)); }
            }

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);

                PathField pathField = (PathField)ve;

                pathField.label = m_labelAttr.GetValueFromBag(bag, cc);
                pathField.value = m_PathAttr.GetValueFromBag(bag, cc);
                pathField.buttonBrowseLabel = m_BtnBrowseLabelAttr.GetValueFromBag(bag, cc);
                pathField.buttonViewLabel = m_BtnViewLabelAttr.GetValueFromBag(bag, cc);
                pathField.pathType = m_PathTypeAttr.GetValueFromBag(bag, cc);
            }
        }
    }
}