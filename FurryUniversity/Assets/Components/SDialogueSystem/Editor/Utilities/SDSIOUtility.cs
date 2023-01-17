using SDS.Data;
using SDS.Data.Save;
using SDS.Elements;
using SDS.ScriptableObjects;
using SDS.Utility;
using SDS.Windows;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace SDS.Utilities
{
    public static class SDSIOUtility
    {
        public static readonly string Assets = "Assets";
        public static readonly string Graphs = "Graphs";
        public static readonly string Components = "Components";
        public static readonly string SDialogueSystem = "SDialogueSystem";
        public static readonly string SDialogueSystemSaveData = "SDialogueSystemSaveData";
        public static readonly string SaveDataRootPath = $"{Components}/{SDialogueSystem}/SDialogueSystemSaveData";//与初版相比修改了下保存位置
        public static readonly string Groups = "Groups";
        public static readonly string Global = "Global";
        public static readonly string Dialogues = "Dialogues";
        public static readonly string Editor = "Editor";

        public static readonly string editorRootPath = $"{Assets}/{SaveDataRootPath}/{Editor}";
        public static readonly string runtimeRootPath = $"{Assets}/{SaveDataRootPath}/{Dialogues}";


        private static SDSGraphView graphView;

        private static string graphFileName;//点击保存时，自定义的graph名称
        private static string containerFolderPath;//runtime下的container

        private static List<SDSGroup> groups;
        private static List<SDSNode> nodes;

        //save object
        private static Dictionary<string, SDSDialogueGroupSO> createdDialogueGroups;//中转一下，创建group时添加，创建node时读取
        private static Dictionary<string, SDSDialogueSO> createdDialogues;//中转，创建dialogueSO时添加，更新其选项时读取

        //load object
        private static Dictionary<string, SDSGroup> loadedGroups;
        private static Dictionary<string, SDSNode> loadedNodes;

        public static void Initialize(SDSGraphView sdsGraphView, string graphName)
        {
            graphView = sdsGraphView;

            graphFileName = graphName;
            containerFolderPath = $"{runtimeRootPath}/{graphFileName}";

            groups = new List<SDSGroup>();
            nodes = new List<SDSNode>();

            createdDialogueGroups = new Dictionary<string, SDSDialogueGroupSO>();
            createdDialogues = new Dictionary<string, SDSDialogueSO>();

            loadedGroups = new Dictionary<string, SDSGroup>();
            loadedNodes = new Dictionary<string, SDSNode>();
        }

        #region Save Methods
        public static void Save()
        {
            CreateStaticFolders();
            GetElementsFromGraphView();

            //editor
            SDSGraphSaveDataSO graphData = CreateAsset<SDSGraphSaveDataSO>($"{editorRootPath}/{Graphs}", graphFileName);
            graphData.Initialize(graphFileName);

            //runtime
            SDSDialogueContainerSO dialogueContainer = CreateAsset<SDSDialogueContainerSO>(containerFolderPath, graphFileName);
            dialogueContainer.Initialize(graphFileName);

            SaveGroups(graphData, dialogueContainer);
            SaveNodes(graphData, dialogueContainer);

            SaveAsset(graphData);
            SaveAsset(dialogueContainer);
        }
        #endregion

        #region Load Methods
        public static void Load()
        {
            SDSGraphSaveDataSO graphData = LoadAsset<SDSGraphSaveDataSO>($"{editorRootPath}/{Graphs}", graphFileName);
            if (graphData == null)
            {
                EditorUtility.DisplayDialog(
                    "无法加载文件",
                    "该路径下的文件无法加载：\n\n" +
                    $"{editorRootPath}/{Graphs}/{graphFileName}\n\n" +
                    "确认选了正确的保存文件，并保存在选择的目录下",
                    "OK"
                    );
                return;
            }

            SDSEditorWindow.UpdateFileName(graphData.FileName);

            LoadGroups(graphData.Groups);
            LoadNodes(graphData.Nodes);
            LoadNodesConnections();
        }

        private static void LoadGroups(List<SDSGroupSaveData> groups)
        {
            foreach (SDSGroupSaveData groupData in groups)
            {
                SDSGroup group = graphView.CreateGroup(groupData.Name, groupData.Position);
                group.ID = groupData.ID;
                loadedGroups.Add(group.ID, group);
            }
        }

        private static void LoadNodes(List<SDSNodeSaveData> nodes)
        {
            foreach (SDSNodeSaveData nodeData in nodes)
            {
                SDSNode node = graphView.CreateNode(nodeData.Name, nodeData.DialogueType, nodeData.Position, false);

                node.ID = nodeData.ID;
                node.Contents = CloneContenSaveDatas(nodeData.Contents);
                node.Choices = CloneNodeChoices(nodeData.Choices);
                node.Events = CloneEventSaveDatas(nodeData.Events);
                node.Draw();

                graphView.AddElement(node);

                loadedNodes.Add(node.ID, node);

                if (!string.IsNullOrEmpty(nodeData.GroupID))
                {
                    //grouped node
                    SDSGroup group = loadedGroups[nodeData.GroupID];
                    node.Group = group;
                    group.AddElement(node);
                }
            }
        }

        private static void LoadNodesConnections()
        {
            foreach (KeyValuePair<string, SDSNode> loadedNode in loadedNodes)
            {
                foreach (Port choicePort in loadedNode.Value.outputContainer.Children())
                {
                    SDSChoiceSaveData choiceData = choicePort.userData as SDSChoiceSaveData;
                    if (string.IsNullOrEmpty(choiceData.NodeID))
                        continue;

                    SDSNode nextNode = loadedNodes[choiceData.NodeID];
                    Port nextNodeInputPort = nextNode.inputContainer.Children().First() as Port;//每个node只有一个input port
                    Edge edge = choicePort.ConnectTo(nextNodeInputPort);
                    graphView.AddElement(edge);

                    loadedNode.Value.RefreshPorts();
                }
            }
        }

        #endregion

        #region Groups
        private static void SaveGroups(SDSGraphSaveDataSO graphData, SDSDialogueContainerSO dialogueContainer)
        {
            List<string> groupNames = new List<string>();//将当前存在的groupName保存下来，用于后面 删除未使用group folder的逻辑
            foreach (SDSGroup group in groups)
            {
                SaveGroupToGraph(group, graphData);
                SaveGroupToScriptableObject(group, dialogueContainer);

                groupNames.Add(group.title);
            }

            UpdateOldGroups(groupNames, graphData);
        }


        /// <summary>
        /// 将图中的group另建为持久化数据，用于editor下次使用
        /// </summary>
        /// <param name="group"></param>
        /// <param name="graphData"></param>
        private static void SaveGroupToGraph(SDSGroup group, SDSGraphSaveDataSO graphData)
        {
            SDSGroupSaveData groupData = new SDSGroupSaveData()
            {
                ID = group.ID,
                Name = group.title,
                Position = group.GetPosition().position
            };

            graphData.Groups.Add(groupData);
        }

        /// <summary>
        /// 将图中的group另建为持久化数据，并保存在container中，用于runtime下使用
        /// </summary>
        /// <param name="group"></param>
        /// <param name="dialogueContainer"></param>
        private static void SaveGroupToScriptableObject(SDSGroup group, SDSDialogueContainerSO dialogueContainer)
        {
            string groupName = group.title;

            CreateFolder($"{containerFolderPath}/{Groups}", groupName);
            CreateFolder($"{containerFolderPath}/{Groups}/{groupName}", Dialogues);//提前建好目录

            SDSDialogueGroupSO dialogueGroup = CreateAsset<SDSDialogueGroupSO>($"{containerFolderPath}/{Groups}/{groupName}", groupName);
            dialogueGroup.Initialize(groupName);

            createdDialogueGroups.Add(group.ID, dialogueGroup);

            dialogueContainer.DialogueGroups.Add(dialogueGroup, new List<SDSDialogueSO>());

            SaveAsset(dialogueGroup);
        }

        /// <summary>
        /// 删除未使用的group folder
        /// </summary>
        /// <param name="groupNames"></param>
        /// <param name="graphData"></param>
        private static void UpdateOldGroups(List<string> currentGroupNames, SDSGraphSaveDataSO graphData)
        {
            if (graphData.OldGroupNames != null && graphData.OldGroupNames.Count != 0)
            {
                List<string> groupsToRemove = graphData.OldGroupNames.Except(currentGroupNames).ToList();//排查出不在currentGroupNames中的groupNames

                foreach (string groupToRemove in groupsToRemove)
                {
                    RemoveFolder($"{containerFolderPath}/{Groups}/{groupToRemove}");
                }
            }

            graphData.OldGroupNames = new List<string>(currentGroupNames);
        }

        #endregion

        #region Nodes
        private static void SaveNodes(SDSGraphSaveDataSO graphData, SDSDialogueContainerSO dialogueContainer)
        {
            SerializableDictionary<string, List<string>> groupedNodeNames = new SerializableDictionary<string, List<string>>();
            List<string> ungroupedNodeNames = new List<string>();
            foreach (SDSNode node in nodes)
            {
                SaveNodeToGraph(node, graphData);
                SaveNodeToScriptableObject(node, dialogueContainer);

                if (node.Group != null)
                {
                    groupedNodeNames.AddItem(node.Group.title, node.DialogueName);
                }
                else
                {
                    ungroupedNodeNames.Add(node.DialogueName);
                }
            }

            UpdateDialoguesChoicesConnections();
            UpdateOldGroupedNodes(groupedNodeNames, graphData);
            UpdateOldUngroupedNodes(ungroupedNodeNames, graphData);
        }

        /// <summary>
        /// for editor
        /// </summary>
        /// <param name="node"></param>
        /// <param name="graphData"></param>
        private static void SaveNodeToGraph(SDSNode node, SDSGraphSaveDataSO graphData)
        {
            List<SDSDialogueContentSaveData> contents = CloneContenSaveDatas(node.Contents);
            List<SDSChoiceSaveData> choices = CloneNodeChoices(node.Choices);
            List<SDSEventSaveData> events = CloneEventSaveDatas(node.Events);

            SDSNodeSaveData nodeData = new SDSNodeSaveData()
            {
                ID = node.ID,
                Name = node.DialogueName,
                Contents = contents,
                Choices = choices,
                GroupID = node.Group?.ID,
                DialogueType = node.DialogueType,
                Position = node.GetPosition().position,
                Events = events,
            };

            graphData.Nodes.Add(nodeData);
        }

        /// <summary>
        /// for runtime
        /// </summary>
        /// <param name="node"></param>
        /// <param name="dialogueContainer"></param>
        /// <exception cref="NotImplementedException"></exception>
        private static void SaveNodeToScriptableObject(SDSNode node, SDSDialogueContainerSO dialogueContainer)
        {
            SDSDialogueSO dialogue;

            if (node.Group != null)
            {
                dialogue = CreateAsset<SDSDialogueSO>($"{containerFolderPath}/{Groups}/{node.Group.title}/{Dialogues}", node.DialogueName);
                dialogueContainer.DialogueGroups.AddItem(createdDialogueGroups[node.Group.ID], dialogue);
            }
            else
            {
                dialogue = CreateAsset<SDSDialogueSO>($"{containerFolderPath}/{Global}/{Dialogues}", node.DialogueName);
                dialogueContainer.UnGroupedDialogues.Add(dialogue);
            }

            dialogue.Initialize(
                node.DialogueName,
                ConvertNodeContentsToDialogueContents(node.Contents),
                ConvertNodeChoicesToDialogueChoices(node.Choices),
                node.DialogueType,
                node.IsStartingNode(),
                ConvertNodeEventsToDialogueEvents(CloneEventSaveDatas(node.Events))
                );

            createdDialogues.Add(node.ID, dialogue);

            SaveAsset(dialogue);
        }

        /// <summary>
        /// 将Editor版本的Choice数据转换为Runtime版本的
        /// </summary>
        /// <param name="nodeChoices"></param>
        /// <returns></returns>
        private static List<SDSDialogueChoiceData> ConvertNodeChoicesToDialogueChoices(List<SDSChoiceSaveData> nodeChoices)
        {
            List<SDSDialogueChoiceData> dialogueChoices = new List<SDSDialogueChoiceData>();

            foreach (SDSChoiceSaveData nodeChoice in nodeChoices)
            {
                SDSDialogueChoiceData choiceData = new SDSDialogueChoiceData()
                {
                    Text = nodeChoice.Text,
                };
                dialogueChoices.Add(choiceData);
            }

            return dialogueChoices;
        }

        /// <summary>
        /// 将Editor版本的Event数据转换为Runtime版本的
        /// </summary>
        /// <param name="nodeEvents"></param>
        /// <returns></returns>
        private static List<SDSDialogueEventData> ConvertNodeEventsToDialogueEvents(List<SDSEventSaveData> nodeEvents)
        {
            List<SDSDialogueEventData> dialogueEvents = new List<SDSDialogueEventData>();

            foreach (SDSEventSaveData nodeEvent in nodeEvents)
            {
                SDSDialogueEventData eventData = new SDSDialogueEventData()
                {
                    EventType = nodeEvent.EventType,
                    AssetName = nodeEvent.AssetObject == null ? null : nodeEvent.AssetObject.name,
                    Parameters = nodeEvent.Parameters,
                    IsEventOnExit = nodeEvent.IsEventOnExit
                };
                dialogueEvents.Add(eventData);
            }

            return dialogueEvents;
        }

        /// <summary>
        /// 将Editor版本的对话数据转换为Runtime版本的
        /// </summary>
        /// <param name="nodeContents"></param>
        /// <returns></returns>
        private static List<SDSDialogueContentData> ConvertNodeContentsToDialogueContents(List<SDSDialogueContentSaveData> nodeContents)
        {
            List<SDSDialogueContentData> dialogueContents = new List<SDSDialogueContentData>();

            foreach (SDSDialogueContentSaveData nodeContent in nodeContents)
            {
                SDSDialogueContentData contentData = new SDSDialogueContentData()
                {
                    Text = nodeContent.Text,
                    Spokesman = nodeContent.Spokesman
                };
                dialogueContents.Add(contentData);
            }

            return dialogueContents;
        }

        private static void UpdateDialoguesChoicesConnections()
        {
            foreach (SDSNode node in nodes)
            {
                SDSDialogueSO dialogue = createdDialogues[node.ID];
                for (int choiceIndex = 0; choiceIndex < node.Choices.Count; ++choiceIndex)
                {
                    SDSChoiceSaveData nodeChoice = node.Choices[choiceIndex];
                    if (string.IsNullOrEmpty(nodeChoice.NodeID))
                    {
                        continue;
                    }
                    dialogue.Choices[choiceIndex].NextDialogue = createdDialogues[nodeChoice.NodeID];

                    SaveAsset(dialogue);
                }
            }
        }

        /// <summary>
        /// 删除未被使用的已分组 dialogue so
        /// </summary>
        /// <param name="currentGroupedNodeNames"></param>
        /// <param name="graphData"></param>
        private static void UpdateOldGroupedNodes(SerializableDictionary<string, List<string>> currentGroupedNodeNames, SDSGraphSaveDataSO graphData)
        {
            if (graphData.OldGroupedNodeNames != null && graphData.OldGroupedNodeNames.Count != 0)
            {
                foreach (KeyValuePair<string, List<string>> oldGroupedNode in graphData.OldGroupedNodeNames)
                {
                    List<string> nodesToRemove = new List<string>();
                    if (currentGroupedNodeNames.ContainsKey(oldGroupedNode.Key))
                    {
                        nodesToRemove = oldGroupedNode.Value.Except(currentGroupedNodeNames[oldGroupedNode.Key]).ToList();
                    }
                    foreach (string nodeToRemove in nodesToRemove)
                    {
                        RemoveAsset($"{containerFolderPath}/{Groups}/{oldGroupedNode.Key}/{Dialogues}",nodeToRemove);
                    }
                }
            }

            graphData.OldGroupedNodeNames = new SerializableDictionary<string, List<string>>(currentGroupedNodeNames);
        }

        /// <summary>
        /// 删除未被使用的未分组 dialogue so
        /// </summary>
        /// <param name="currentUngroupedNodeNames"></param>
        /// <param name="graphData"></param>
        private static void UpdateOldUngroupedNodes(List<string> currentUngroupedNodeNames, SDSGraphSaveDataSO graphData)
        {
            if (graphData.OldUngroupedNodeNames != null && graphData.OldUngroupedNodeNames.Count != 0)
            {
                List<string> nodesToRemove = graphData.OldUngroupedNodeNames.Except(currentUngroupedNodeNames).ToList();
                foreach (string nodeToRemove in nodesToRemove)
                {
                    RemoveAsset($"{containerFolderPath}/{Global}/{Dialogues}", nodeToRemove);
                }
            }
            graphData.OldUngroupedNodeNames = new List<string>(currentUngroupedNodeNames);
        }
        #endregion

        #region Fetch Methods
        private static void GetElementsFromGraphView()
        {
            graphView.graphElements.ForEach(graphElement =>
            {
                if (graphElement is SDSNode node)
                {
                    nodes.Add(node);
                    return;
                }

                if (graphElement is SDSGroup group)
                {
                    groups.Add(group);
                    return;
                }
            });
        }
        #endregion

        #region Creation Methods
        private static void CreateStaticFolders()
        {
            CreateFolder(Assets, Components);
            CreateFolder($"{Assets}/{Components}", SDialogueSystem);//与初版相比，修改了保存位置
            CreateFolder($"{Assets}/{Components}/{SDialogueSystem}", SDialogueSystemSaveData);

            //Editor
            CreateFolder($"{Assets}/{SaveDataRootPath}", Editor);
            CreateFolder(editorRootPath, Graphs);

            //Runtime保存根目录
            CreateFolder($"{Assets}/{SaveDataRootPath}", Dialogues);
            //Runtime根下每个graph一个目录
            CreateFolder(runtimeRootPath, graphFileName);
            //每个graph一个container，container由global保存 未分组节点及其选项 ，groups保存 组 和 已分组节点及其选项
            CreateFolder(containerFolderPath, Global);
            CreateFolder(containerFolderPath, Groups);
            CreateFolder($"{containerFolderPath}/{Global}", Dialogues);
        }
        #endregion

        #region Utility Methods
        public static void CreateFolder(string path,string folderName)
        {
            if (AssetDatabase.IsValidFolder($"{path}/{folderName}"))
            {
                return;
            }
            AssetDatabase.CreateFolder(path,folderName);
        }

        public static void RemoveFolder(string fullPath)
        {
            FileUtil.DeleteFileOrDirectory($"{fullPath}.meta");
            FileUtil.DeleteFileOrDirectory($"{fullPath}/");
        }

        public static T CreateAsset<T>(string path, string assetName) where T : ScriptableObject
        {
            string fullPath = $"{path}/{assetName}.asset";
            T asset = LoadAsset<T>(path, assetName);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<T>();
                AssetDatabase.CreateAsset(asset, fullPath);
            }
            return asset;
        }

        public static T LoadAsset<T>(string path,string assetName) where T : ScriptableObject
        {
            string fullPath = $"{path}/{assetName}.asset";
            return AssetDatabase.LoadAssetAtPath<T>(fullPath);
        }

        public static void RemoveAsset(string path, string assetName)
        {
            AssetDatabase.DeleteAsset($"{path}/{assetName}.asset");
        }

        public static void SaveAsset(UnityEngine.Object asset)
        {
            EditorUtility.SetDirty(asset);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        /// <summary>
        /// 深拷贝一下，避免在graphview中修改时，把so中的数据也一起修改了
        /// </summary>
        /// <param name="nodeChoices"></param>
        /// <returns></returns>
        private static List<SDSChoiceSaveData> CloneNodeChoices(List<SDSChoiceSaveData> nodeChoices)
        {
            List<SDSChoiceSaveData> choices = new List<SDSChoiceSaveData>();
            foreach (SDSChoiceSaveData choice in nodeChoices)
            {
                SDSChoiceSaveData choiceData = new SDSChoiceSaveData()
                {
                    Text = choice.Text,
                    NodeID = choice.NodeID
                };
                choices.Add(choiceData);
            }

            return choices;
        }

        /// <summary>
        /// 深拷贝一下，避免在graphView中修改时，把SO中的数据也一起修改了
        /// </summary>
        /// <param name="eventSaveDatas"></param>
        /// <returns></returns>
        private static List<SDSEventSaveData> CloneEventSaveDatas(List<SDSEventSaveData> eventSaveDatas)
        {
            List<SDSEventSaveData> datas = new List<SDSEventSaveData>();
            foreach (SDSEventSaveData data in eventSaveDatas)
            {
                List<string> parameters = new List<string>();
                data.Parameters.ForEach(p => parameters.Add(p));
                SDSEventSaveData newData = new SDSEventSaveData()
                {
                    EventType = data.EventType,
                    AssetObject = data.AssetObject,
                    Parameters = parameters,
                    Description = data.Description,
                    IsEventOnExit = data.IsEventOnExit
                };
                datas.Add(newData);
            }
            return datas;
        }

        /// <summary>
        /// 深拷贝一下，避免在graphView中修改时，把SO中的数据也一起修改了
        /// </summary>
        /// <param name="contentSaveData"></param>
        /// <returns></returns>
        private static List<SDSDialogueContentSaveData> CloneContenSaveDatas(List<SDSDialogueContentSaveData> contentSaveData)
        {
            List<SDSDialogueContentSaveData> datas = new List<SDSDialogueContentSaveData>();
            foreach (SDSDialogueContentSaveData data in contentSaveData)
            {
                SDSDialogueContentSaveData newData = new SDSDialogueContentSaveData()
                {
                    Text = data.Text,
                    Spokesman = data.Spokesman
                };
                datas.Add(newData);
            }
            return datas;
        }
        #endregion
    }
}