using SDS;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SFramework.Core.GameManagers
{
    public class Init : MonoBehaviour
    {
        void Awake()
        {
            AssetBundleManager.LoadManifest();//load manifest before GameManagers' Initialization
            GameManager.InitializeGameManager();

            DontDestroyOnLoad(this.gameObject);
        }

//#if UNITY_EDITOR
        private SDSDialogue dialogue;
        private StringBuilder showInfos = new StringBuilder();
        private Vector2 scrollPos;
        private int line = 0;
        private void OnGUI()
        {
            this.scrollPos = GUILayout.BeginScrollView(this.scrollPos, GUILayout.Width(1000), GUILayout.Height(300));
            GUILayout.TextArea(this.showInfos.ToString(), new GUIStyle("textArea"), GUILayout.Width(1000), GUILayout.MinHeight(300), GUILayout.Height(20 * this.line));
            GUILayout.EndScrollView();

            GUILayout.Space(20);

            GUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("清除信息"))
                {
                    this.showInfos.Clear();
                }

                if (GUILayout.Button("加载对话文件"))
                {
                    this.dialogue = GameObject.Find("DialoguesContainer").GetComponent<SDSDialogue>();
                    if (this.dialogue != null)
                    {
                        this.showInfos.AppendLine($"加载 {this.dialogue} 成功");
                    }
                    else
                    {
                        this.showInfos.AppendLine($"加载失败");
                    }
                    this.line++;
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            {
                if (this.dialogue != null)
                {
                    if (GUILayout.Button("显示当前对话"))
                    {
                        this.showInfos.AppendLine(this.dialogue.CurrentDialogue.Contents[this.dialogue.CurrentIndexOfSentenceInDialogue].Text);
                        this.line++;
                    }
                    if (GUILayout.Button("显示对话事件参数"))
                    {
                        if (this.dialogue.CurrentDialogue.Events != null && this.dialogue.CurrentDialogue.Events.Count > 0)
                        {
                            foreach (var e in this.dialogue.CurrentDialogue.Events)
                            {
                                foreach (var p in e.Parameters)
                                {
                                    this.showInfos.AppendLine(p.ToString());
                                    this.line++;
                                }
                            }
                        }
                    }

                    switch (this.dialogue.CurrentDialogue.DialogueType)
                    {
                        case SDS.Enumerations.SDSDialogueType.SingleChoice:
                            if (this.dialogue.CheckCurrentDialogueHasNextNode())
                            {
                                if (GUILayout.Button("切换到下一句"))
                                {
                                    this.dialogue.SwitchToNextDialogue();
                                }
                            }
                            break;
                        case SDS.Enumerations.SDSDialogueType.MultipleChoice:
                            if (this.dialogue.CheckCurrentDialogueHasNextNode())
                            {
                                if (this.dialogue.CurrentDialogue.Contents.Count > this.dialogue.CurrentIndexOfSentenceInDialogue + 1)
                                {
                                    if (GUILayout.Button("切换到下一句"))
                                    {
                                        this.dialogue.SwitchToNextDialogue();
                                    }
                                }
                                else
                                {
                                    for (int i = 0; i < this.dialogue.CurrentDialogue.Choices.Count; ++i)
                                    {
                                        var curChoice = this.dialogue.CurrentDialogue.Choices[i];
                                        if (GUILayout.Button($"选择分支 {curChoice.Text}"))
                                        {
                                            this.dialogue.ChooseDialogueBranch(i);
                                        }
                                    }
                                }
                            }
                            break;
                    }
                }

            }
            GUILayout.EndHorizontal();
        }
//#endif
    }
}