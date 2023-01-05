using SDS;
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

#if UNITY_EDITOR
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
                        this.showInfos.AppendLine(this.dialogue.CurrentDialogue.Text);
                        this.line++;
                    }

                    switch (this.dialogue.CurrentDialogue.DialogueType)
                    {
                        case SDS.Enumerations.SDSDialogueType.SingleChoice:
                            if (GUILayout.Button("切换到下一句"))
                            {

                            }
                            break;
                        case SDS.Enumerations.SDSDialogueType.MultipleChoice:
                            foreach (var choice in this.dialogue.CurrentDialogue.Choices)
                            {

                            }
                            break;
                    }
                }

            }
            GUILayout.EndHorizontal();
        }
#endif
    }
}