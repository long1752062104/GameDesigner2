﻿using System.Collections.Generic;
using UnityEngine;

namespace GameDesigner
{
    public abstract class StateMachineView : MonoBehaviour
    {
        public StateMachineCore stateMachine;
#if UNITY_EDITOR
        [HideInInspector] public StateMachineCore editStateMachine;
        internal int editStateMachineId;
        internal List<IStateMachine> stateMachines;
#endif
        public List<string> clipNames;
        public List<string> ClipNames { get => clipNames ??= new List<string>(); set => clipNames = value; }

        public virtual void Init()
        {
        }

        public virtual void Execute()
        {
            stateMachine.Execute();
        }

        public virtual void OnDestroy()
        {
            stateMachine.OnDestroy();
        }

#if UNITY_EDITOR
        public virtual void EditorInit(Transform root) { }

        public void OnValidate()
        {
            OnScriptReload();
        }

        public virtual void OnScriptReload()
        {
            stateMachines ??= new List<IStateMachine>();
            if (stateMachine == null)
                return;
            stateMachine.View = this;
            stateMachines.Clear();
            stateMachine.OnScriptReload(this);
            for (int i = 0; i < stateMachines.Count; i++)
                stateMachines[i].Id = i;
            UpdateEditStateMachine(editStateMachineId);
        }

        public virtual void UpdateEditStateMachine(int editStateMachineId)
        {
            this.editStateMachineId = editStateMachineId;
            if (editStateMachineId >= stateMachines.Count)
                editStateMachineId = 0;
            editStateMachine = (StateMachineCore)stateMachines[editStateMachineId];
        }
#endif
    }
}