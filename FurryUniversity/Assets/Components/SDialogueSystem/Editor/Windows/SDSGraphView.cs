using SDS.Elements;
using SDS.Enumerations;
using SDS.Utilities;
using SDS.Data.Error;
using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using SDS.Data.Save;
using static UnityEngine.GraphicsBuffer;
using SDS.Utility;

namespace SDS.Windows
{
    public class SDSGraphView : GraphView
    {
        private SDSEditorWindow editorWindow;
        private SDSSearchWindow searchWindow;
        private MiniMap miniMap;

        
        private SerializableDictionary<string, SDSNodeErrorData> ungroupedNodes;//key：node name
        private SerializableDictionary<string, SDSGroupErrorData> groups;//key：group name
        private SerializableDictionary<Group, SerializableDictionary<string, SDSNodeErrorData>> groupedNodes;

        private int nameErrorsAmount;
        /// <summary> 当有重复命名的节点时，禁用保存按钮；包括组内组外节点，组 </summary>
        public int NameErrorsAmount 
        { 
            get => this.nameErrorsAmount; 
            set
            {
                this.nameErrorsAmount = value;
                if (this.nameErrorsAmount == 0)
                {
                    //启用保存按钮
                    this.editorWindow.EnableSaving();
                }
                if (this.nameErrorsAmount == 1)
                {
                    //禁用保存按钮
                    this.editorWindow.DisableSaving();
                }
            } 
        }


        public SDSGraphView(SDSEditorWindow editorWindow)
        {
            this.editorWindow = editorWindow;

            this.ungroupedNodes = new SerializableDictionary<string, SDSNodeErrorData>();
            this.groups = new SerializableDictionary<string, SDSGroupErrorData>();
            this.groupedNodes = new SerializableDictionary<Group, SerializableDictionary<string, SDSNodeErrorData>>();

            this.AddManipulators();
            this.AddGridBackground();
            this.AddSearchWindow();
            this.AddMiniMap();

            this.AddStyles();
            this.AddMiniMapStyles();

            //注册回调
            this.OnElementDeleted();
            this.OnGroupElementsAdded();
            this.OnGroupElementsRemoved();
            this.OnGroupRenamed();
            this.OnGraphViewChanged();
        }


        #region overrided method

        /// <summary>
        /// 连接检查
        /// </summary>
        /// <param name="startPort"></param>
        /// <param name="nodeAdapter"></param>
        /// <returns></returns>
        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            List<Port> compatiblePorts = new List<Port>();

            this.ports.ForEach(port =>
            {
                if (startPort == port)
                    return;

                if (startPort.node == port.node)
                    return;

                if (startPort.direction == port.direction)
                    return;

                compatiblePorts.Add(port);
            });

            return compatiblePorts;
        }
        #endregion

        #region Manipulators

        private void AddManipulators()
        {
            this.SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);//缩放

            this.AddManipulator(new ContentDragger());//拖拽
            this.AddManipulator(new SelectionDragger());//多选拖拽，放框选选择器前才能生效
            this.AddManipulator(new RectangleSelector());//框选

            this.AddManipulator(this.CreateNodeContextualMenu(SDSDialogueType.SingleChoice, "Add Node (Single Choice)"));//右键创建sdsNode
            this.AddManipulator(this.CreateNodeContextualMenu(SDSDialogueType.MultipleChoice, "Add Node (Multiple Choice)"));

            this.AddManipulator(this.CreateGroupContextualMenu());
        }

        private IManipulator CreateNodeContextualMenu(SDSDialogueType dialogueType, string actionTitle)
        {
            ContextualMenuManipulator contextualMenuManipulator = new ContextualMenuManipulator(
                menuEvent => menuEvent.menu.AppendAction(actionTitle,
                actionEvent => this.AddElement(this.CreateNode(null, dialogueType, this.GetLocalMousePosition(actionEvent.eventInfo.localMousePosition))))
            );
            return contextualMenuManipulator;
        }



        private IManipulator CreateGroupContextualMenu()
        {
            ContextualMenuManipulator contextualMenuManipulator = new ContextualMenuManipulator(
                menuEvent => menuEvent.menu.AppendAction("Add Group",
                actionEvent => this.CreateGroup(null, this.GetLocalMousePosition(actionEvent.eventInfo.localMousePosition)))
            );
            return contextualMenuManipulator;
        }

        #endregion

        #region Elements Creation

        public SDSGroup CreateGroup(string title, Vector2 position)
        {
            SDSGroup group = new SDSGroup(title, position);
            this.AddGroup(group);

            //将group添加到graphView中；之所以在这里添加而不是在Manipulator中自动添加，是为了解决选中node后创建group，group还未被添加到graphView，导致回调elementsAddedToGroup未调用，
            //node未被添加到group中的问题，修改后Manipulator和SearchWindow中的创建逻辑就不再需要调用 this.AddElement() 方法了
            //执行 this.AddElement() 将触发 this.elementsAddedToGroup 回调
            this.AddElement(group);

            foreach (GraphElement selectedElement in this.selection)
            {
                if (selectedElement is SDSNode node)
                {
                    group.AddElement(node);
                }
            }

            return group;
        }

        private void AddSearchWindow()
        {
            if (this.searchWindow == null)
            {
                this.searchWindow = ScriptableObject.CreateInstance<SDSSearchWindow>();

                this.searchWindow.Initialize(this);
            }

            //请求显示 node creation window 的回调，快捷键空格；打开 SDSSearchWindow
            this.nodeCreationRequest = context => SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), this.searchWindow);
        }

        /// <summary>
        /// 创建节点，并放入未分组集合中
        /// </summary>
        /// <param name="dialogueType"></param>
        /// <param name="position"></param>
        /// <returns></returns>
        public SDSNode CreateNode(string nodeName, SDSDialogueType dialogueType, Vector2 position, bool shouldDraw = true)
        {
            Type nodeType = Type.GetType($"SDS.Elements.SDS{dialogueType}Node");

            SDSNode node = Activator.CreateInstance(nodeType) as SDSNode;

            node.Initialize(nodeName, this, position);
            if (shouldDraw)
            {
                //只有在新创建node的时候才调用Draw；从so中加载node时，由于这时候还没有还原node的数据（ID、Text等；Initialize方法只是初始化数据，不是我们想要的），因此先不调用Draw，
                //在设置完数据后调用Draw
                node.Draw();
            }

            this.AddUngroupedNode(node);

            return node;
        }
#endregion

#region Callbacks
        /// <summary>
        /// 删除节点时的回调
        /// </summary>
        private void OnElementDeleted()
        {
            this.deleteSelection = (operationName, askUser) =>
            {
                List<SDSNode> nodesToDelete = new List<SDSNode>();
                List<SDSGroup> groupsToDelete = new List<SDSGroup>();
                List<Edge> edgesToDelete = new List<Edge>();
                foreach (GraphElement element in this.selection)
                {
                    if(element is SDSNode node)
                    {
                        nodesToDelete.Add(node);
                        continue;
                    }

                    if (element is SDSGroup group)
                    {
                        groupsToDelete.Add(group);
                        continue;
                    }

                    if (element is Edge edge)
                    {
                        edgesToDelete.Add(edge);
                        continue;
                    }
                }

                //删除group
                foreach (SDSGroup group in groupsToDelete)
                {
                    //当删除group时其拥有node，先把里面的node放回未分组节点中
                    List<SDSNode> groupNodes = new List<SDSNode>();
                    foreach (GraphElement groupElement in group.containedElements)
                    {
                        if (groupElement is SDSNode groupNode)
                        {
                            groupNodes.Add(groupNode);
                        }
                    }
                    group.RemoveElements(groupNodes);//触发 this.elementsRemovedFromGroup 回调，处理从分组节点放到未分组节点的逻辑

                    this.RemoveGroup(group);
                    this.RemoveElement(group);
                }

                //删除edge
                this.DeleteElements(edgesToDelete);//graph没有RemoveElements方法，怎会如此，只能用DeleteElements

                //删除node
                foreach (SDSNode node in nodesToDelete)
                {
                    if (node.Group != null)
                    {
                        //自动调用 this.elementsRemovedFromGroup 回调；即先从组内删除，节点回到未分组的状态，再从未分组中删除
                        node.Group.RemoveElement(node);
                    }
                    node.DisconnectAllPorts();
                    this.RemoveUngroupedNode(node);//修改未分组节点的数据
                    this.RemoveElement(node);//从graph中删除元素，即节点
                }

            };
        }

        /// <summary>
        /// 向组内添加元素时的回调
        /// </summary>
        private void OnGroupElementsAdded()
        {
            this.elementsAddedToGroup = (group, elements) =>
            {
                foreach (GraphElement element in elements)
                {
                    if(element is SDSNode node)
                    {
                        SDSGroup nodeGroup = group as SDSGroup;
                        this.RemoveUngroupedNode(node);
                        this.AddGroupNode(node, nodeGroup);
                    }
                }
            };
        }

        /// <summary>
        /// 删除group中节点的回调，先从dic中移除，再加入未分组节点；不处理从graphView中删除node的逻辑
        /// </summary>
        private void OnGroupElementsRemoved()
        {
            this.elementsRemovedFromGroup = (group, elements) =>
            {
                foreach (GraphElement element in elements)
                {
                    if (element is SDSNode node)
                    {
                        this.RemoveGroupedNode(node, group);
                        this.AddUngroupedNode(node);
                    }
                }
            };
        }

        /// <summary>
        /// group重命名的回调
        /// </summary>
        private void OnGroupRenamed()
        {
            this.groupTitleChanged += (group, newTitle) =>
            {
                SDSGroup sdsGroup = group as SDSGroup;
                sdsGroup.title = newTitle.RemoveWhitespaces().RemoveSpecialCharacters();

                if (string.IsNullOrEmpty(sdsGroup.title))
                {
                    if (!string.IsNullOrEmpty(sdsGroup.OldTitle))//当dialogue name 从非空字符串改为空字符串，禁用保存按钮
                    {
                        ++this.NameErrorsAmount;
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(sdsGroup.OldTitle))//dialogue name 从空字符串改为非空字符串，错误减一
                    {
                        --this.NameErrorsAmount;
                    }
                }

                this.RemoveGroup(sdsGroup);
                sdsGroup.OldTitle = sdsGroup.title;
                this.AddGroup(sdsGroup);
            };
        }

        private void OnGraphViewChanged()
        {
            this.graphViewChanged = (changes) =>
            {
                //连线后，设置nextNodeID
                if (changes.edgesToCreate != null)
                {
                    foreach (Edge edge in changes.edgesToCreate)
                    {
                        SDSNode nextNode = edge.input.node as SDSNode;
                        SDSChoiceSaveData choiceData = edge.output.userData as SDSChoiceSaveData;
                        choiceData.NodeID = nextNode.ID;
                    }
                }

                //删除连线后，重置nextNodeID
                if (changes.elementsToRemove != null)
                {
                    foreach (GraphElement element in changes.elementsToRemove)
                    {
                        if (element is Edge edge)
                        {
                            SDSChoiceSaveData choiceData = edge.output.userData as SDSChoiceSaveData;
                            choiceData.NodeID = "";
                        }
                    }
                }

                return changes;
            };
        }
#endregion

#region Repeated Elements
        /// <summary>
        /// 将未分组的，拥有相同名字的节点分为一类，它们共用errorData
        /// </summary>
        /// <param name="node"></param>
        public void AddUngroupedNode(SDSNode node)
        {
            string nodeName = node.DialogueName.ToLower();

            //第一个节点，仅记录
            if (!this.ungroupedNodes.ContainsKey(nodeName))
            {
                SDSNodeErrorData nodeErrorData = new SDSNodeErrorData();

                nodeErrorData.Nodes.Add(node);

                this.ungroupedNodes.Add(nodeName, nodeErrorData);

                return;
            }

            List<SDSNode> ungroupedNodesList = this.ungroupedNodes[nodeName].Nodes;

            //多余一个节点，记录并设置error color
            ungroupedNodesList.Add(node);

            //设置errorColor
            Color errorColor = this.ungroupedNodes[nodeName].ErrorData.Color;
            node.SetErrorStyle(errorColor);

            if (ungroupedNodesList.Count == 2)//更新第一个节点
            {
                ++this.NameErrorsAmount;
                ungroupedNodesList[0].SetErrorStyle(errorColor);
            }
        }

        /// <summary>
        /// 当节点名字修改后，修改未分组节点的数据
        /// </summary>
        /// <param name="node"></param>
        public void RemoveUngroupedNode(SDSNode node)
        {
            string nodeName = node.DialogueName.ToLower();

            List<SDSNode> ungroupedNodesList = this.ungroupedNodes[nodeName].Nodes;

            //删除并重置style
            ungroupedNodesList.Remove(node);
            node.ResetStyle();

            if (ungroupedNodesList.Count == 1)
            {
                --this.NameErrorsAmount;
                ungroupedNodesList[0].ResetStyle();
                return;
            }

            if (ungroupedNodesList.Count == 0)
            {
                this.ungroupedNodes.Remove(nodeName);
            }
        }

        /// <summary>
        /// 向组内添加节点，不处理将节点从未分组中移除的逻辑
        /// </summary>
        /// <param name="node"></param>
        /// <param name="group"></param>
        public void AddGroupNode(SDSNode node, SDSGroup group)
        {
            node.Group = group;

            string nodeName = node.DialogueName.ToLower();

            if (!this.groupedNodes.ContainsKey(group))//没有group，添加
            {
                this.groupedNodes.Add(group, new SerializableDictionary<string, SDSNodeErrorData>());
            }

            if (!this.groupedNodes[group].ContainsKey(nodeName))//该组第一个名为nodeName的节点加入，放入后即可
            {
                SDSNodeErrorData nodeErrorData = new SDSNodeErrorData();

                nodeErrorData.Nodes.Add(node);

                this.groupedNodes[group].Add(nodeName, nodeErrorData);

                return;
            }

            List<SDSNode> groupedNodeList = this.groupedNodes[group][nodeName].Nodes;

            groupedNodeList.Add(node);

            Color errorColor = this.groupedNodes[group][nodeName].ErrorData.Color;

            node.SetErrorStyle(errorColor);

            if (groupedNodeList.Count == 2)//第二个同名节点加入，需同时设置第一个节点的error color
            {
                ++this.NameErrorsAmount;
                groupedNodeList[0].SetErrorStyle(errorColor);
            }
        }

        /// <summary>
        /// 从组内移除节点，不处理将节点加入未分组的逻辑
        /// </summary>
        /// <param name="node"></param>
        /// <param name="group"></param>
        public void RemoveGroupedNode(SDSNode node, Group group)
        {
            node.Group = null;

            string nodeName = node.DialogueName.ToLower();

            List<SDSNode> groupedNodesList = this.groupedNodes[group][nodeName].Nodes;

            groupedNodesList.Remove(node);

            node.ResetStyle();

            if (groupedNodesList.Count == 1)
            {
                --this.NameErrorsAmount;
                groupedNodesList[0].ResetStyle();
                return;
            }

            if (groupedNodesList.Count == 0)
            {
                this.groupedNodes[group].Remove(nodeName);

                if (this.groupedNodes[group].Count == 0)
                {
                    this.groupedNodes.Remove(group);
                }
            }
        }

        /// <summary>
        /// 添加组，并且同名的组拥有相同的error color
        /// </summary>
        /// <param name="group"></param>
        private void AddGroup(SDSGroup group)
        {
            string groupName = group.title.ToLower();

            if (!this.groups.ContainsKey(groupName))//新创建的组，只需加入dic即可
            {
                SDSGroupErrorData groupErrorData = new SDSGroupErrorData();
                groupErrorData.Groups.Add(group);
                this.groups.Add(groupName, groupErrorData);
                return;
            }

            List<SDSGroup> groupsList = this.groups[groupName].Groups;
            groupsList.Add(group);

            Color errorColor = groups[groupName].ErrorData.Color;
            group.SetErrorStyle(errorColor);

            if (groupsList.Count == 2)//第二个同名组出现，同样要设置第一个同名组error color
            {
                ++this.NameErrorsAmount;
                groupsList[0].SetErrorStyle(errorColor);
            }
        }

        /// <summary>
        /// 从dic删除SDSGroup
        /// </summary>
        /// <param name="group"></param>
        private void RemoveGroup(SDSGroup group)
        {
            string oldGroupName = group.OldTitle.ToLower();

            List<SDSGroup> groupsList = this.groups[oldGroupName].Groups;
            groupsList.Remove(group);
            group.ResetStyle();

            if (groupsList.Count == 1)
            {
                --this.NameErrorsAmount;
                groupsList[0].ResetStyle();
                return;
            }

            if (groupsList.Count == 0)
            {
                groups.Remove(oldGroupName);
            }
        }
#endregion

#region Elements Addition
        private void AddGridBackground()
        {
            GridBackground gridBackground = new GridBackground();

            gridBackground.StretchToParentWidth();

            this.Insert(0, gridBackground);//网格塞到最底层
        }

        private void AddStyles()
        {
            this.AddStyleSheets(
                "SDialogueSystem/SDSGraphViewStyles.uss",
                "SDialogueSystem/SDSNodeStyles.uss"
                );
        }

        private void AddMiniMap()
        {
            this.miniMap = new MiniMap()
            {
                anchored = true//固定不可拖动
            };

            this.miniMap.SetPosition(new Rect(15, 50, 200, 180));

            this.Add(this.miniMap);
            this.miniMap.visible = false;//默认隐藏
        }

        private void AddMiniMapStyles()
        {
            StyleColor backgroundColor = new StyleColor(new Color32(29, 29, 30, 255));
            StyleColor borderColor = new StyleColor(new Color32(51, 51, 51, 255));

            this.miniMap.style.backgroundColor = backgroundColor;
            this.miniMap.style.borderTopColor = borderColor;
            this.miniMap.style.borderBottomColor = borderColor;
            this.miniMap.style.borderRightColor = borderColor;
            this.miniMap.style.borderLeftColor = borderColor;
        }

#endregion

#region Utilities
        public Vector2 GetLocalMousePosition(Vector2 mousePosition, bool isSearchWindow = false)
        {
            Vector2 worldMousePosition = mousePosition;

            if (isSearchWindow)
            {
                worldMousePosition -= this.editorWindow.position.position;
            }

            Vector2 localMousePosition = this.contentViewContainer.WorldToLocal(worldMousePosition);
            return localMousePosition;
        }

        public void ClearGraph()
        {
            this.graphElements.ForEach(graphElement => this.RemoveElement(graphElement));

            this.groups.Clear();
            this.groupedNodes.Clear();
            this.ungroupedNodes.Clear();
            this.nameErrorsAmount = 0;
        }

        public void ToggleMiniMap()
        {
            this.miniMap.visible = !this.miniMap.visible;
        }
#endregion
    }
}