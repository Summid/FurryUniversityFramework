using SDS;
using SFramework.Utilities;
using SFramework.Utilities.Archive;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SFramework.Core.GameManagers
{
    public partial class GameManager
    {
        public static GameManager Instance { get; private set; }
        public Init GameSettings;
        private GameManager() { }
        public static void InitializeGameManager()
        {
            try
            {
                Instance = new GameManager();
                Instance.GameSettings = GameObject.Find("Global").GetComponent<Init>();
                Instance.InitializeGameManagers();
                Instance.InitializeComponents();
            }
            catch(Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        private Dictionary<Type, GameManagerBase> gameManagers = new Dictionary<Type, GameManagerBase>();
        private void InitializeGameManagers()
        {
            var managerTypes = typeof(GameManagerBase).GetSubTypesInAssemblies();
            foreach (var managerType in managerTypes)
            {
                if (!this.gameManagers.ContainsKey(managerType))
                {
                    this.gameManagers.Add(managerType, Activator.CreateInstance(managerType) as GameManagerBase);
                }
                this.gameManagers[managerType].Initialize();
            }
        }

        //TODO 放到显示LoginView之后再加载，并显示加载进度
        private void InitializeComponents()
        {
            SaveMaster.Init();
        }

        public T GetManager<T>(Type type) where T : GameManagerBase
        {
            if (this.gameManagers.TryGetValue(type, out GameManagerBase manager))
            {
                return manager as T;
            }
            else
            {
                return null;
            }
        }

        public GameManagerBase GetManager(Type type)
        {
            return this.gameManagers.TryGetValue(type, out GameManagerBase manager) ? manager : null;
        }

        private SDSDialogue dialogueSystem;
        public SDSDialogue DialogueSystem
        {
            get
            {
                if (this.dialogueSystem == null)
                    this.dialogueSystem = GameObject.Find("DialoguesContainer").GetComponent<SDSDialogue>();
                return this.dialogueSystem;
            }
        }
    }
}