using SDS.ScriptableObjects;
using SDS.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SDS.Inspectors
{
    [CustomEditor(typeof(SDSDialogue))]
    public class SDSDialogueEditor : Editor
    {
        //Dialogue Scriptable Objects
        private SerializedProperty dialogueContainerProperty;
        private SerializedProperty dialogueGroupProperty;
        private SerializedProperty dialogueProperty;

        //Filters
        private SerializedProperty groupedDialoguesProperty;
        private SerializedProperty startingDialoguesOnlyProperty;

        //Indexes
        private SerializedProperty selectedDialogueGroupIndexProperty;
        private SerializedProperty selectedDialogueIndexProperty;

        private SerializedProperty includedContainersProperty;

        private void OnEnable()
        {
            this.dialogueContainerProperty = this.serializedObject.FindProperty("dialogueContainer");
            this.dialogueGroupProperty = this.serializedObject.FindProperty("dialogueGroup");
            this.dialogueProperty = this.serializedObject.FindProperty("dialogue");

            this.groupedDialoguesProperty = this.serializedObject.FindProperty("groupedDialogues");
            this.startingDialoguesOnlyProperty = this.serializedObject.FindProperty("startingDialoguesOnly");

            this.selectedDialogueGroupIndexProperty = this.serializedObject.FindProperty("selectedDialogueGroupIndex");
            this.selectedDialogueIndexProperty = this.serializedObject.FindProperty("selectedDialogueIndex");

            this.includedContainersProperty = this.serializedObject.FindProperty("includedContainers");
        }

        public override void OnInspectorGUI()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                SDSInspectorUtility.DrawHelpBox("运行模式下不支持编辑此组件");
                SDSInspectorUtility.DrawDisabledFields(this.DrawDialogueContainerArea);
                return;
            }

            this.serializedObject.Update();

            this.DrawDialogueContainerArea();

            SDSDialogueContainerSO dialogueContainer = this.dialogueContainerProperty.objectReferenceValue as SDSDialogueContainerSO;

            if (dialogueContainer == null)
            {
                //没有选择container，显示提示并返回
                this.StopDrawing("选择一个 Dialogue Container 以继续");
                return;
            }

            this.DrawFilterArea();

            bool currentStartingDialoguesOnlyFilter = this.startingDialoguesOnlyProperty.boolValue;

            List<string> dialogueNames;
            string dialogueFolderPath = $"{SDSIOUtility.runtimeRootPath}/{dialogueContainer.FileName}";
            string dialogueInfoMessage;
            if (this.groupedDialoguesProperty.boolValue)               
            {
                //勾选显示分组对话
                List<string> dialogueGroupNames = dialogueContainer.GetDialogueGroupNames();
                if (dialogueGroupNames.Count == 0)
                {
                    this.StopDrawing("Dialogue Container has no group");
                    return;
                }
                else
                {
                    this.DrawDialogueGroupArea(dialogueContainer, dialogueGroupNames);//绘制分组区域

                    //设置分组对话需要的变量
                    SDSDialogueGroupSO dialogueGroup = this.dialogueGroupProperty.objectReferenceValue as SDSDialogueGroupSO;
                    dialogueNames = dialogueContainer.GetGroupedDialogueNames(dialogueGroup, currentStartingDialoguesOnlyFilter);
                    dialogueFolderPath += $"/{SDSIOUtility.Groups}/{dialogueGroup.GroupName}/{SDSIOUtility.Dialogues}";
                    dialogueInfoMessage = "There are no" + (currentStartingDialoguesOnlyFilter ? "Starting" : "") + " Dialogues in this Dialogue Group";
                }
            }
            else
            {
                //设置显示未分组对话需要的变量
                dialogueNames = dialogueContainer.GetUngroupedDialogueNames(currentStartingDialoguesOnlyFilter);
                dialogueFolderPath += $"/{SDSIOUtility.Global}/{SDSIOUtility.Dialogues}";
                dialogueInfoMessage = "There are no" + (currentStartingDialoguesOnlyFilter ? "Starting" : "") + " Ungrouped Dialogues in this Dialogue Container";
            }

            if (dialogueNames.Count == 0)
            {
                this.StopDrawing(dialogueInfoMessage);
                return;
            }
            else
            {
                this.DrawDialogueArea(dialogueNames, dialogueFolderPath);
            }

            SDSInspectorUtility.DrawSpace();
            this.DrawIncludedContainersArea();
            this.serializedObject.ApplyModifiedProperties();
        }

        #region Draw Methods
        private void DrawDialogueContainerArea()
        {
            SDSInspectorUtility.DrawHeader("Dialogue Container");

            this.dialogueContainerProperty.DrawPropertyField();
            SDSInspectorUtility.DrawSpace();
        }

        private void DrawFilterArea()
        {
            SDSInspectorUtility.DrawHeader("Filters");

            this.groupedDialoguesProperty.DrawPropertyField();
            this.startingDialoguesOnlyProperty.DrawPropertyField();
            SDSInspectorUtility.DrawSpace();
        }

        private void DrawDialogueGroupArea(SDSDialogueContainerSO dialogueContainer, List<string> dialogueGroupNames)
        {
            SDSInspectorUtility.DrawHeader("Dialogue Group");

            //更新Popup索引，避免删除group后报错
            int oldSelectedDialogueGroupIndex = this.selectedDialogueGroupIndexProperty.intValue;
            SDSDialogueGroupSO oldDialogueGroup = this.dialogueGroupProperty.objectReferenceValue as SDSDialogueGroupSO;
            bool isOldDialogueGroupNull = oldDialogueGroup == null;
            string oldDialogueGroupName = isOldDialogueGroupNull ? "" : oldDialogueGroup.GroupName;
            this.UpdateIndexOnNamesListUpdate(dialogueGroupNames, this.selectedDialogueGroupIndexProperty, oldSelectedDialogueGroupIndex, oldDialogueGroupName, isOldDialogueGroupNull);

            this.selectedDialogueGroupIndexProperty.intValue = SDSInspectorUtility.DrawPopup("Dialogue Group", this.selectedDialogueGroupIndexProperty.intValue, dialogueGroupNames.ToArray());
            //绘制选中的group对象
            string selectedDialogueGroupName = dialogueGroupNames[this.selectedDialogueGroupIndexProperty.intValue];
            SDSDialogueGroupSO selectedDialogueGroup = SDSIOUtility.LoadAsset<SDSDialogueGroupSO>($"{SDSIOUtility.runtimeRootPath}/{dialogueContainer.FileName}/{SDSIOUtility.Groups}/{selectedDialogueGroupName}", selectedDialogueGroupName);
            this.dialogueGroupProperty.objectReferenceValue = selectedDialogueGroup;


            SDSInspectorUtility.DrawDisabledFields(this.dialogueGroupProperty.DrawPropertyField);
            SDSInspectorUtility.DrawSpace();
        }

        private void DrawDialogueArea(List<string> dialogueNames, string dialogueFolderPath)
        {
            SDSInspectorUtility.DrawHeader("Dialogue");

            //更新Popup索引，更新graph的元素后索引将重置
            int oldSelectedDialogueIndex = this.selectedDialogueIndexProperty.intValue;
            SDSDialogueSO oldDialogue = this.dialogueProperty.objectReferenceValue as SDSDialogueSO;
            bool isOldDialogueNull = oldDialogue == null;
            string oldDialogueName = isOldDialogueNull ? "" : oldDialogue.DialogueName;
            this.UpdateIndexOnNamesListUpdate(dialogueNames, this.selectedDialogueIndexProperty, oldSelectedDialogueIndex, oldDialogueName, isOldDialogueNull);

            this.selectedDialogueIndexProperty.intValue = SDSInspectorUtility.DrawPopup("Dialogue", this.selectedDialogueIndexProperty.intValue, dialogueNames.ToArray());
            //绘制选中的dialogue对象
            string selectedDialogueName = dialogueNames[this.selectedDialogueIndexProperty.intValue];
            SDSDialogueSO selectedDialogue = SDSIOUtility.LoadAsset<SDSDialogueSO>(dialogueFolderPath, selectedDialogueName);
            this.dialogueProperty.objectReferenceValue = selectedDialogue;

            SDSInspectorUtility.DrawDisabledFields(this.dialogueProperty.DrawPropertyField);
        }

        private void StopDrawing(string reason, MessageType messageType = MessageType.Info)
        {
            SDSInspectorUtility.DrawHelpBox(reason, messageType);
            SDSInspectorUtility.DrawSpace();
            SDSInspectorUtility.DrawHelpBox("You need to select a Dialogue for this componet to work properly at Runtime", MessageType.Warning);

            this.serializedObject.ApplyModifiedProperties();
        }

        private void DrawIncludedContainersArea()
        {
            SDSInspectorUtility.DrawHeader("Pick Included Containers");

            SDSInspectorUtility.DrawPropertyField(this.includedContainersProperty);
        }

        #endregion

        private void UpdateIndexOnNamesListUpdate(List<string> optionNames, SerializedProperty indexProperty, int oldSelectedPropertyIndex, 
            string oldPropertyName, bool isOldPropertyNull)
        {
            if (isOldPropertyNull)
            {
                indexProperty.intValue = 0;
                return;
            }

            bool oldIndexIsOutOfBoundsOfNamesListCount = oldSelectedPropertyIndex > optionNames.Count - 1;
            bool oldNameIsDifferentThanSelectedName = oldIndexIsOutOfBoundsOfNamesListCount || oldPropertyName != optionNames[oldSelectedPropertyIndex];

            if (oldNameIsDifferentThanSelectedName)
            {
                if (optionNames.Contains(oldPropertyName))
                {
                    indexProperty.intValue = optionNames.IndexOf(oldPropertyName);
                }
                else
                {
                    indexProperty.intValue = 0;
                }
            }
        }
    }
}