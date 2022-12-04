using SDS.Utilities;
using System;
using System.IO;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SDS.Windows
{
    public class SDSEditorWindow : EditorWindow
    {
        private SDSGraphView graphView;
        private const string defaultFileName = "DialoguesFileName";
        private static TextField fileNameTextField;
        private Button saveButton;
        private Button miniMapButton;

        [MenuItem("Window/SDS/Dialogue Graph")]
        public static void Open()
        {
            GetWindow<SDSEditorWindow>("Dialogue Graph");
        }

        private void CreateGUI()
        {
            this.AddGraphView();
            this.AddStyles();
            this.AddToolBar();
        }

        #region Element Addtion
        private void AddGraphView()
        {
            this.graphView = new SDSGraphView(this);

            this.graphView.StretchToParentSize();

            this.rootVisualElement.Add(this.graphView);
        }

        private void AddStyles()
        {
            this.rootVisualElement.AddStyleSheets("SDialogueSystem/SDSVariables.uss");
        }

        private void AddToolBar()
        {
            Toolbar toolbar = new Toolbar();

            fileNameTextField = SDSElementUtility.CreateTextField(defaultFileName, "File Name", callback =>
            {
                fileNameTextField.value = callback.newValue.RemoveWhitespaces().RemoveSpecialCharacters();
            });
            this.saveButton = SDSElementUtility.CreateButton("Save",this.Save);


            Button loadButton = SDSElementUtility.CreateButton("Load", this.Load);
            Button clearButton = SDSElementUtility.CreateButton("Clear", this.Clear);
            Button resetButton = SDSElementUtility.CreateButton("Reset", this.ResetGraph);
            this.miniMapButton = SDSElementUtility.CreateButton("Minimap", this.ToggleMiniMap);

            toolbar.Add(fileNameTextField);
            toolbar.Add(this.saveButton);
            toolbar.Add(loadButton);
            toolbar.Add(clearButton);
            toolbar.Add(resetButton);
            toolbar.Add(this.miniMapButton);

            toolbar.AddStyleSheets("SDialogueSystem/SDSToolbarStyles.uss");

            this.rootVisualElement.Add(toolbar);
        }
        #endregion


        #region Toolbar Action
        private void Save()
        {
            if (string.IsNullOrEmpty(fileNameTextField.value))
            {
                EditorUtility.DisplayDialog("无效的文件名", "文件名不能为空", "OK");
                return;
            }

            SDSIOUtility.Initialize(this.graphView, fileNameTextField.value);
            SDSIOUtility.Save();
        }

        private void Load()
        {
            string saveDataPath = $"{SDSIOUtility.editorRootPath}/{SDSIOUtility.Graphs}";
            string filePath = EditorUtility.OpenFilePanel("Dialogue Graphs", saveDataPath, "asset");

            if (string.IsNullOrEmpty(filePath))
            {
                return;
            }

            this.Clear();

            string graphName = Path.GetFileNameWithoutExtension(filePath);
            SDSIOUtility.Initialize(this.graphView, graphName);
            SDSIOUtility.Load();
            UpdateFileName(graphName);
        }

        /// <summary>
        /// 清除当前graph中的节点、组
        /// </summary>
        private void Clear()
        {
            this.graphView.ClearGraph();
        }

        /// <summary>
        /// 清除当前graph中的node和group，并重置文件名
        /// </summary>
        private void ResetGraph()
        {
            this.Clear();
            UpdateFileName(defaultFileName);
        }

        private void ToggleMiniMap()
        {
            this.graphView.ToggleMiniMap();
            this.miniMapButton.ToggleInClassList("sds-toolbar__button__selected");
        }
        #endregion

        #region Utility Methods
        public static void UpdateFileName(string newFileName)
        {
            fileNameTextField.value = newFileName;
        }

        public void EnableSaving()
        {
            this.saveButton.SetEnabled(true);
        }

        public void DisableSaving()
        {
            this.saveButton.SetEnabled(false);
        }
        #endregion
    }
}