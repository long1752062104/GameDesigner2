#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GameDesigner
{
    public abstract class StateMachineViewEditor : Editor
    {
        protected static StateMachineView self;
        public static string createScriptName = "NewStateBehaviour";
        public static string stateActionScriptPath = "/Actions/StateActions";
        public static string stateBehaviourScriptPath = "/Actions/StateBehaviours";
        public static string transitionScriptPath = "/Actions/Transitions";
        protected static StateBase addBehaviourState;
        private static bool compiling;
        protected static List<Type> findBehaviourTypes;
        protected static List<Type> findBehaviourTypes1;
        protected static List<Type> findBehaviourTypes2;
        protected static bool animPlay;
        protected static StateAction animAction;

        protected virtual void OnEnable()
        {
            self = target as StateMachineView;
            self.EditorInit(self.transform);
            self.stateMachine.transform = self.transform;
            StateMachineWindow.stateMachine = self.stateMachine;
            if (findBehaviourTypes == null)
            {
                findBehaviourTypes = new List<Type>();
                AddBehaviourTypes(findBehaviourTypes, typeof(StateBehaviour));
            }
            if (findBehaviourTypes1 == null)
            {
                findBehaviourTypes1 = new List<Type>();
                AddBehaviourTypes(findBehaviourTypes1, typeof(ActionBehaviour));
            }
            if (findBehaviourTypes2 == null)
            {
                findBehaviourTypes2 = new List<Type>();
                AddBehaviourTypes(findBehaviourTypes2, typeof(TransitionBehaviour));
            }
        }

        protected void AddBehaviourTypes(List<Type> types, Type type)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                var types1 = assembly.GetTypes().Where(t => t.IsSubclassOf(type)).ToArray();
                types.AddRange(types1);
            }
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();
            serializedObject.Update();
            OnDrawPreField();
            if (GUILayout.Button(BlueprintSetting.Instance.Language["Open the state machine editor"], GUI.skin.GetStyle("LargeButtonMid"), GUILayout.ExpandWidth(true)))
                StateMachineWindow.Init(self ? self.stateMachine : null);
            if (self == null)
                goto J;
            var sm = self.stateMachine;
            if (sm.SelectState != null)
            {
                DrawState(sm.SelectState);
                EditorGUILayout.Space();
                for (int i = 0; i < sm.SelectState.transitions.Length; ++i)
                    DrawTransition(sm.SelectState.transitions[i]);
            }
            else if (StateMachineWindow.selectTransition != null)
            {
                DrawTransition(StateMachineWindow.selectTransition);
            }
            EditorGUILayout.Space();
            if (EditorGUI.EndChangeCheck())
                EditorUtility.SetDirty(self);
            Repaint();
        J: serializedObject.ApplyModifiedProperties();
        }

        protected virtual void OnDrawPreField()
        {
        }

        private SerializedObject _supportObject;
        protected SerializedObject SupportObject
        {
            get
            {
                if (_supportObject == null)
                    _supportObject = new SerializedObject(self);
                return _supportObject;
            }
        }

        private SerializedProperty _stateMachineObject;
        protected SerializedProperty StateMachineObject
        {
            get
            {
                if (_stateMachineObject == null)
                    _stateMachineObject = SupportObject.FindProperty("stateMachine");
                return _stateMachineObject;
            }
        }

        private SerializedProperty _statesProperty;
        protected SerializedProperty StatesProperty
        {
            get
            {
                if (_statesProperty == null)
                    _statesProperty = StateMachineObject.FindPropertyRelative("states").GetArrayElementAtIndex(self.stateMachine.SelectState.ID);
                return _statesProperty;
            }
        }

        private SerializedProperty _nameProperty;
        protected SerializedProperty NameProperty
        {
            get
            {
                if (_nameProperty == null)
                    _nameProperty = StatesProperty.FindPropertyRelative("name");
                return _nameProperty;
            }
        }

        private SerializedProperty _actionSystemProperty;
        protected SerializedProperty ActionSystemProperty
        {
            get
            {
                if (_actionSystemProperty == null)
                    _actionSystemProperty = StatesProperty.FindPropertyRelative("actionSystem");
                return _actionSystemProperty;
            }
        }

        private SerializedProperty _animSpeedProperty;
        protected SerializedProperty animSpeedProperty
        {
            get
            {
                if (_animSpeedProperty == null)
                    _animSpeedProperty = StatesProperty.FindPropertyRelative("animSpeed");
                return _animSpeedProperty;
            }
        }

        private SerializedProperty _animLoopProperty;
        protected SerializedProperty animLoopProperty
        {
            get
            {
                if (_animLoopProperty == null)
                    _animLoopProperty = StatesProperty.FindPropertyRelative("animLoop");
                return _animLoopProperty;
            }
        }

        private SerializedProperty _actionsProperty;
        protected SerializedProperty actionsProperty
        {
            get
            {
                if (_actionsProperty == null)
                    _actionsProperty = StatesProperty.FindPropertyRelative("actions");
                return _actionsProperty;
            }
        }

        private static State CurrentState;

        protected virtual void ResetPropertys()
        {
            _stateMachineObject = null;
            _statesProperty = null;
            _nameProperty = null;
            _actionSystemProperty = null;
            _animSpeedProperty = null;
            _animLoopProperty = null;
            _actionsProperty = null;
        }

        protected virtual void OnDrawAnimationField() { }

        /// <summary>
        /// 绘制状态监视面板属性
        /// </summary>
        public void DrawState(State state)
        {
            if (CurrentState != state)
            {
                CurrentState = state;
                ResetPropertys();
            }
            SupportObject.Update();
            GUILayout.Button(BlueprintGUILayout.Instance.Language["State attribute"], GUI.skin.GetStyle("dragtabdropwindow"));
            EditorGUILayout.BeginVertical("ProgressBarBack");
            EditorGUILayout.PropertyField(NameProperty, new GUIContent(BlueprintGUILayout.Instance.Language["Status name"], "name"));
            EditorGUILayout.IntField(new GUIContent(BlueprintGUILayout.Instance.Language["Status identifier"], "stateID"), state.ID);
            EditorGUILayout.PropertyField(ActionSystemProperty, new GUIContent(BlueprintGUILayout.Instance.Language["Action system"], "actionSystem  专为玩家角色AI其怪物AI所设计的一套AI系统！"));
            if (state.actionSystem)
            {
                OnDrawAnimationField();
                state.animPlayMode = (AnimPlayMode)EditorGUILayout.Popup(new GUIContent(BlueprintGUILayout.Instance.Language["Action execution mode"], "animPlayMode"), (int)state.animPlayMode, new GUIContent[]{
                    new GUIContent(BlueprintGUILayout.Instance.Language["Action randomised"],"Random"),
                    new GUIContent(BlueprintGUILayout.Instance.Language["Action sequence"],"Sequence"),
                    new GUIContent(BlueprintGUILayout.Instance.Language["Action none"],"Code")
                });
                EditorGUILayout.PropertyField(animSpeedProperty, new GUIContent(BlueprintGUILayout.Instance.Language["Animation speed"], "animSpeed"), true);
                EditorGUILayout.PropertyField(animLoopProperty, new GUIContent(BlueprintGUILayout.Instance.Language["Animation cycle?"], "animLoop"), true);
                state.isCrossFade = EditorGUILayout.Toggle(new GUIContent(BlueprintGUILayout.Instance.Language["isCrossFade"], "isCrossFade"), state.isCrossFade);
                if (state.isCrossFade)
                    state.duration = EditorGUILayout.FloatField(new GUIContent(BlueprintGUILayout.Instance.Language["duration"], "duration"), state.duration);
                state.isExitState = EditorGUILayout.Toggle(new GUIContent(BlueprintGUILayout.Instance.Language["Exit status at end of action"], "isExitState"), state.isExitState);
                if (state.isExitState)
                    state.DstStateID = EditorGUILayout.Popup(BlueprintGUILayout.Instance.Language["get into the state"], state.DstStateID, Array.ConvertAll(state.transitions.ToArray(), new Converter<Transition, string>(delegate (Transition t) { return t.CurrState.name + " -> " + t.NextState.name + "   ID:" + t.NextState.ID; })));
                BlueprintGUILayout.BeginStyleVertical(BlueprintGUILayout.Instance.Language["Action tree"], "ProgressBarBack");
                EditorGUI.indentLevel = 1;
                var actRect = EditorGUILayout.GetControlRect();
                state.foldout = EditorGUI.Foldout(new Rect(actRect.position, new Vector2(actRect.size.x - 120f, 15)), state.foldout, BlueprintGUILayout.Instance.Language["Action Tree Set"], true);

                if (GUI.Button(new Rect(new Vector2(actRect.size.x - 40f, actRect.position.y), new Vector2(60, 16)), BlueprintGUILayout.Instance.Language["Add action"]))
                {
                    ArrayExtend.Add(ref state.actions, new StateAction() { ID = state.ID, stateMachine = self.stateMachine, behaviours = new BehaviourBase[0] });
                    return;
                }
                if (GUI.Button(new Rect(new Vector2(actRect.size.x - 100, actRect.position.y), new Vector2(60, 16)), BlueprintGUILayout.Instance.Language["Remove action"]))
                {
                    if (state.actions.Length > 1)
                        ArrayExtend.RemoveAt(ref state.actions, state.actions.Length - 1);
                    return;
                }

                if (state.foldout)
                {
                    EditorGUI.indentLevel = 2;
                    for (int x = 0; x < state.actions.Length; ++x)
                    {
                        var actionProperty = actionsProperty.GetArrayElementAtIndex(x);
                        if (actionProperty == null)
                            continue;
                        var act = state.actions[x];
                        var foldoutRect = EditorGUILayout.GetControlRect();
                        act.foldout = EditorGUI.Foldout(foldoutRect, act.foldout, new GUIContent(BlueprintGUILayout.Instance.Language["Action ->"] + x, "actions[" + x + "]"), true);
                        if (foldoutRect.Contains(Event.current.mousePosition) & Event.current.button == 1)
                        {
                            var menu = new GenericMenu();
                            menu.AddItem(new GUIContent(BlueprintGUILayout.Instance.Language["Remove action"]), false, (obj) =>
                            {
                                ArrayExtend.RemoveAt(ref state.actions, (int)obj);
                            }, x);
                            menu.AddItem(new GUIContent(BlueprintGUILayout.Instance.Language["Copy action"]), false, (obj) =>
                            {
                                StateSystem.Component = state.actions[(int)obj];
                            }, x);
                            menu.AddItem(new GUIContent(BlueprintGUILayout.Instance.Language["Paste new action"]), StateSystem.CopyComponent != null, () =>
                            {
                                if (StateSystem.Component is StateAction stateAction)
                                    ArrayExtend.Add(ref state.actions, Net.CloneHelper.DeepCopy<StateAction>(stateAction, new List<Type>() { typeof(Object), typeof(StateMachineCore) }));
                            });
                            menu.AddItem(new GUIContent(BlueprintGUILayout.Instance.Language["Paste action value"]), StateSystem.CopyComponent != null, (obj) =>
                            {
                                if (StateSystem.Component is StateAction stateAction)
                                {
                                    var index = (int)obj;
                                    if (stateAction == state.actions[index])//如果要黏贴的动作是复制的动作则返回
                                        return;
                                    state.actions[index] = Net.CloneHelper.DeepCopy<StateAction>(stateAction, new List<Type>() { typeof(Object), typeof(StateMachineCore) });
                                }
                            }, x);
                            menu.ShowAsContext();
                        }
                        if (act.foldout)
                        {
                            EditorGUI.indentLevel = 3;
                            act.clipIndex = EditorGUILayout.Popup(new GUIContent(BlueprintGUILayout.Instance.Language["Movie clips"], "clipIndex"), act.clipIndex, Array.ConvertAll(self.stateMachine.ClipNames.ToArray(), input => new GUIContent(input)));
                            if (self.stateMachine.ClipNames.Count > 0 && act.clipIndex < self.stateMachine.ClipNames.Count)
                                act.clipName = self.stateMachine.ClipNames[act.clipIndex];
                            OnDrawActionPropertyField(actionProperty);
                            EditorGUILayout.PropertyField(actionProperty.FindPropertyRelative("animTime"), new GUIContent(BlueprintGUILayout.Instance.Language["Animation time"], "animTime"));
                            EditorGUILayout.PropertyField(actionProperty.FindPropertyRelative("animTimeMax"), new GUIContent(BlueprintGUILayout.Instance.Language["Animation length"], "animTimeMax"));
                            EditorGUILayout.PropertyField(actionProperty.FindPropertyRelative("layer"), new GUIContent(BlueprintGUILayout.Instance.Language["Animation layer"], "layer"));
                            for (int i = 0; i < act.behaviours.Length; ++i)
                            {
                                EditorGUILayout.BeginHorizontal();
                                Rect rect = EditorGUILayout.GetControlRect();
                                act.behaviours[i].show = EditorGUI.Foldout(new Rect(rect.x, rect.y, 50, rect.height), act.behaviours[i].show, GUIContent.none);
                                act.behaviours[i].Active = EditorGUI.ToggleLeft(new Rect(rect.x + 5, rect.y, 70, rect.height), GUIContent.none, act.behaviours[i].Active);
                                EditorGUI.LabelField(new Rect(rect.x + 20, rect.y, rect.width - 15, rect.height), act.behaviours[i].name, GUI.skin.GetStyle("BoldLabel"));
                                if (GUI.Button(new Rect(rect.x + rect.width - 15, rect.y, rect.width, rect.height), GUIContent.none, GUI.skin.GetStyle("ToggleMixed")))
                                {
                                    act.behaviours[i].OnDestroyComponent();
                                    ArrayExtend.RemoveAt(ref act.behaviours, i);
                                    continue;
                                }
                                if (rect.Contains(Event.current.mousePosition) & Event.current.button == 1)
                                {
                                    var menu = new GenericMenu();
                                    menu.AddItem(new GUIContent(BlueprintGUILayout.Instance.Language["Remove action scripts"]), false, (obj) =>
                                    {
                                        var index = (int)obj;
                                        act.behaviours[index].OnDestroyComponent();
                                        ArrayExtend.RemoveAt(ref act.behaviours, index);
                                        return;
                                    }, i);
                                    menu.AddItem(new GUIContent(BlueprintGUILayout.Instance.Language["Copy action scripts"]), false, (obj) =>
                                    {
                                        var index = (int)obj;
                                        StateSystem.CopyComponent = act.behaviours[index];
                                    }, i);
                                    menu.AddItem(new GUIContent(BlueprintGUILayout.Instance.Language["Paste new action scripts"]), StateSystem.CopyComponent != null, () =>
                                    {
                                        if (StateSystem.CopyComponent is ActionBehaviour behaviour)
                                        {
                                            ActionBehaviour ab = (ActionBehaviour)Net.CloneHelper.DeepCopy(behaviour);
                                            ArrayExtend.Add(ref act.behaviours, ab);
                                        }
                                    });
                                    menu.AddItem(new GUIContent(BlueprintGUILayout.Instance.Language["Paste action script values"]), StateSystem.CopyComponent != null, (obj) =>
                                    {
                                        if (StateSystem.CopyComponent is ActionBehaviour behaviour)
                                        {
                                            var index = (int)obj;
                                            if (behaviour.name == act.behaviours[index].name)
                                                act.behaviours[index] = (ActionBehaviour)Net.CloneHelper.DeepCopy(StateSystem.CopyComponent);
                                        }
                                    }, i);
                                    menu.AddItem(new GUIContent(BlueprintGUILayout.Instance.Language["Edit action scripts"]), false, (obj) =>
                                    {
                                        var index = (int)obj;
                                        var scriptName = act.behaviours[index].name;
                                        if (!Net.Helper.ScriptHelper.Cache.TryGetValue(scriptName, out var sequence))
                                            sequence = new Net.Helper.SequencePoint();
                                        InternalEditorUtility.OpenFileAtLineExternal(sequence.FilePath, sequence.StartLine, 0);
                                    }, i);
                                    menu.ShowAsContext();
                                }
                                EditorGUILayout.EndHorizontal();
                                if (act.behaviours[i].show)
                                {
                                    EditorGUI.indentLevel = 4;
                                    if (!act.behaviours[i].OnInspectorGUI(state))
                                        foreach (var metadata in act.behaviours[i].Metadatas)
                                            PropertyField(metadata, 60f, 5, 4);
                                    GUILayout.Space(4);
                                    GUILayout.Box("", BlueprintSetting.Instance.HorSpaceStyle, GUILayout.Height(1), GUILayout.ExpandWidth(true));
                                    GUILayout.Space(4);
                                    EditorGUI.indentLevel = 3;
                                }
                            }
                            OnPlayAnimation(act);
                            var r = EditorGUILayout.GetControlRect();
                            var rr = new Rect(new Vector2(r.x + (r.size.x / 4f), r.y), new Vector2(r.size.x / 2f, 20));
                            if (GUI.Button(rr, BlueprintGUILayout.Instance.Language["Add action scripts"]))
                                addBehaviourState = act;
                            if (addBehaviourState == act)
                            {
                                EditorGUILayout.Space();
                                try
                                {
                                    foreach (var type in findBehaviourTypes1)
                                    {
                                        if (GUILayout.Button(type.Name))
                                        {
                                            var stb = (ActionBehaviour)Activator.CreateInstance(type);
                                            stb.InitMetadatas();
                                            stb.ID = state.ID;
                                            ArrayExtend.Add(ref act.behaviours, stb);
                                            addBehaviourState = null;
                                            EditorUtility.SetDirty(self);
                                        }
                                        if (compiling & type.Name == createScriptName)
                                        {
                                            var stb = (ActionBehaviour)Activator.CreateInstance(type);
                                            stb.InitMetadatas();
                                            stb.ID = state.ID;
                                            ArrayExtend.Add(ref act.behaviours, stb);
                                            addBehaviourState = null;
                                            compiling = false;
                                            EditorUtility.SetDirty(self);
                                        }
                                    }
                                }
                                catch { }
                                EditorGUILayout.Space();
                                EditorGUI.indentLevel = 0;
                                EditorGUILayout.LabelField(BlueprintGUILayout.Instance.Language["Create action script paths:"]);
                                stateActionScriptPath = EditorGUILayout.TextField(stateActionScriptPath);
                                var addRect = EditorGUILayout.GetControlRect();
                                createScriptName = EditorGUI.TextField(new Rect(addRect.position, new Vector2(addRect.size.x - 125f, 18)), createScriptName);
                                if (GUI.Button(new Rect(new Vector2(addRect.size.x - 100f, addRect.position.y), new Vector2(120, 18)), BlueprintGUILayout.Instance.Language["Create action scripts"]))
                                {
                                    var text = Resources.Load<TextAsset>("ActionBehaviourScript");
                                    var scriptCode = text.text.Split(new string[] { "\r\n" }, 0);
                                    scriptCode[7] = scriptCode[7].Replace("ActionBehaviourScript", createScriptName);
                                    ScriptTools.CreateScript(Application.dataPath + stateActionScriptPath, createScriptName, scriptCode);
                                    compiling = true;
                                }
                                if (GUILayout.Button(BlueprintGUILayout.Instance.Language["cancel"]))
                                    addBehaviourState = null;
                            }
                            EditorGUILayout.Space();
                        }
                        EditorGUI.indentLevel = 2;
                    }
                }
                BlueprintGUILayout.EndStyleVertical();
            }
            EditorGUILayout.Space();
            DrawBehaviours(state);
            EditorGUILayout.Space();
            EditorGUILayout.EndVertical();
            SupportObject.ApplyModifiedProperties();
        }

        protected virtual void OnPlayAnimation(StateAction action)
        {
        }

        protected virtual void OnDrawActionPropertyField(SerializedProperty actionProperty)
        {
        }

        /// <summary>
        /// 绘制状态行为
        /// </summary>
        public void DrawBehaviours(State s)
        {
            GUILayout.Space(10);
            GUILayout.Box("", BlueprintSetting.Instance.HorSpaceStyle, GUILayout.Height(1), GUILayout.ExpandWidth(true));
            GUILayout.Space(5);
            for (int i = 0; i < s.behaviours.Length; ++i)
            {
                EditorGUI.indentLevel = 1;
                EditorGUILayout.BeginHorizontal();
                var rect = EditorGUILayout.GetControlRect();
                s.behaviours[i].show = EditorGUI.Foldout(new Rect(rect.x, rect.y, 20, rect.height), s.behaviours[i].show, GUIContent.none);
                s.behaviours[i].Active = EditorGUI.ToggleLeft(new Rect(rect.x + 5, rect.y, 30, rect.height), GUIContent.none, s.behaviours[i].Active);
                EditorGUI.LabelField(new Rect(rect.x + 20, rect.y, rect.width - 15, rect.height), s.behaviours[i].name, GUI.skin.GetStyle("BoldLabel"));
                if (GUI.Button(new Rect(rect.x + rect.width - 15, rect.y, rect.width, rect.height), GUIContent.none, GUI.skin.GetStyle("ToggleMixed")))
                {
                    s.behaviours[i].OnDestroyComponent();
                    ArrayExtend.RemoveAt(ref s.behaviours, i);
                    continue;
                }
                if (rect.Contains(Event.current.mousePosition) & Event.current.button == 1)
                {
                    var menu = new GenericMenu();
                    menu.AddItem(new GUIContent(BlueprintGUILayout.Instance.Language["Remove status scripts"]), false, (obj) =>
                    {
                        var index = (int)obj;
                        s.behaviours[index].OnDestroyComponent();
                        ArrayExtend.RemoveAt(ref s.behaviours, index);
                        return;
                    }, i);
                    menu.AddItem(new GUIContent(BlueprintGUILayout.Instance.Language["Copy status script"]), false, (obj) =>
                    {
                        var index = (int)obj;
                        StateSystem.CopyComponent = s.behaviours[index];
                    }, i);
                    menu.AddItem(new GUIContent(BlueprintGUILayout.Instance.Language["Paste a new status script"]), StateSystem.CopyComponent != null, delegate ()
                    {
                        if (StateSystem.CopyComponent is StateBehaviour behaviour)
                        {
                            var ab = (StateBehaviour)Net.CloneHelper.DeepCopy(behaviour);
                            ArrayExtend.Add(ref s.behaviours, ab);
                        }
                    });
                    menu.AddItem(new GUIContent(BlueprintGUILayout.Instance.Language["Paste status script values"]), StateSystem.CopyComponent != null, (obj) =>
                    {
                        if (StateSystem.CopyComponent is StateBehaviour behaviour)
                        {
                            var index = (int)obj;
                            if (behaviour.name == s.behaviours[index].name)
                                s.behaviours[index] = (StateBehaviour)Net.CloneHelper.DeepCopy(behaviour);
                        }
                    }, i);
                    menu.AddItem(new GUIContent(BlueprintGUILayout.Instance.Language["Edit status script"]), false, (obj) =>
                    {
                        var index = (int)obj;
                        var scriptName = s.behaviours[index].name;
                        if (!Net.Helper.ScriptHelper.Cache.TryGetValue(scriptName, out var sequence))
                            sequence = new Net.Helper.SequencePoint();
                        InternalEditorUtility.OpenFileAtLineExternal(sequence.FilePath, sequence.StartLine, 0);
                    }, i);
                    menu.ShowAsContext();
                }
                EditorGUILayout.EndHorizontal();
                if (s.behaviours[i].show)
                {
                    EditorGUI.indentLevel = 2;
                    if (!s.behaviours[i].OnInspectorGUI(s))
                    {
                        foreach (var metadata in s.behaviours[i].Metadatas)
                        {
                            PropertyField(metadata);
                        }
                    }
                    EditorGUI.indentLevel = 1;
                    GUILayout.Space(4);
                    GUILayout.Box("", BlueprintSetting.Instance.HorSpaceStyle, GUILayout.Height(1), GUILayout.ExpandWidth(true));
                }
            }

            var r = EditorGUILayout.GetControlRect();
            var rr = new Rect(new Vector2(r.x + (r.size.x / 4f), r.y), new Vector2(r.size.x / 2f, 20));
            if (GUI.Button(rr, BlueprintGUILayout.Instance.Language["Adding status scripts"]))
                addBehaviourState = s;
            if (addBehaviourState == s)
            {
                try
                {
                    EditorGUILayout.Space();
                    foreach (var type in findBehaviourTypes)
                    {
                        if (GUILayout.Button(type.Name))
                        {
                            var stb = (StateBehaviour)Activator.CreateInstance(type);
                            stb.InitMetadatas();
                            stb.ID = s.ID;
                            ArrayExtend.Add(ref s.behaviours, stb);
                            addBehaviourState = null;
                            EditorUtility.SetDirty(self);
                        }
                        if (compiling & type.Name == createScriptName)
                        {
                            var stb = (StateBehaviour)Activator.CreateInstance(type);
                            stb.InitMetadatas();
                            stb.ID = s.ID;
                            ArrayExtend.Add(ref s.behaviours, stb);
                            addBehaviourState = null;
                            compiling = false;
                            EditorUtility.SetDirty(self);
                        }
                    }
                }
                catch { }
                EditorGUILayout.Space();
                EditorGUI.indentLevel = 0;
                EditorGUILayout.LabelField(BlueprintGUILayout.Instance.Language["Create a status script path:"]);
                stateBehaviourScriptPath = EditorGUILayout.TextField(stateBehaviourScriptPath);
                var addRect = EditorGUILayout.GetControlRect();
                createScriptName = EditorGUI.TextField(new Rect(addRect.position, new Vector2(addRect.size.x - 125f, 18)), createScriptName);
                if (GUI.Button(new Rect(new Vector2(addRect.size.x - 105f, addRect.position.y), new Vector2(120, 18)), BlueprintGUILayout.Instance.Language["Create status scripts"]))
                {
                    var text = Resources.Load<TextAsset>("StateBehaviourScript");
                    var scriptCode = text.text.Split(new string[] { "\r\n" }, 0);
                    scriptCode[7] = scriptCode[7].Replace("StateBehaviourScript", createScriptName);
                    ScriptTools.CreateScript(Application.dataPath + stateBehaviourScriptPath, createScriptName, scriptCode);
                    compiling = true;
                }
                if (GUILayout.Button(BlueprintGUILayout.Instance.Language["cancel"]))
                    addBehaviourState = null;
            }
        }

        private static void PropertyField(Metadata metadata, float width = 40f, int arrayBeginSpace = 3, int arrayEndSpace = 2)
        {
            if (metadata.type == TypeCode.Byte)
                metadata.value = (byte)EditorGUILayout.IntField(metadata.name, (byte)metadata.value);
            else if (metadata.type == TypeCode.SByte)
                metadata.value = (sbyte)EditorGUILayout.IntField(metadata.name, (sbyte)metadata.value);
            else if (metadata.type == TypeCode.Boolean)
                metadata.value = EditorGUILayout.Toggle(metadata.name, (bool)metadata.value);
            else if (metadata.type == TypeCode.Int16)
                metadata.value = (short)EditorGUILayout.IntField(metadata.name, (short)metadata.value);
            else if (metadata.type == TypeCode.UInt16)
                metadata.value = (ushort)EditorGUILayout.IntField(metadata.name, (ushort)metadata.value);
            else if (metadata.type == TypeCode.Char)
                metadata.value = EditorGUILayout.TextField(metadata.name, metadata.value.ToString()).ToCharArray();
            else if (metadata.type == TypeCode.Int32)
                metadata.value = EditorGUILayout.IntField(metadata.name, (int)metadata.value);
            else if (metadata.type == TypeCode.UInt32)
                metadata.value = (uint)EditorGUILayout.IntField(metadata.name, (int)metadata.value);
            else if (metadata.type == TypeCode.Single)
                metadata.value = EditorGUILayout.FloatField(metadata.name, (float)metadata.value);
            else if (metadata.type == TypeCode.Int64)
                metadata.value = EditorGUILayout.LongField(metadata.name, (long)metadata.value);
            else if (metadata.type == TypeCode.UInt64)
                metadata.value = (ulong)EditorGUILayout.LongField(metadata.name, (long)metadata.value);
            else if (metadata.type == TypeCode.Double)
                metadata.value = EditorGUILayout.DoubleField(metadata.name, (double)metadata.value);
            else if (metadata.type == TypeCode.String)
                metadata.value = EditorGUILayout.TextField(metadata.name, metadata.value.ToString());
            else if (metadata.type == TypeCode.Enum)
                metadata.value = EditorGUILayout.EnumPopup(metadata.name, (Enum)metadata.value);
            else if (metadata.type == TypeCode.Vector2)
                metadata.value = EditorGUILayout.Vector2Field(metadata.name, (Vector2)metadata.value);
            else if (metadata.type == TypeCode.Vector3)
                metadata.value = EditorGUILayout.Vector3Field(metadata.name, (Vector3)metadata.value);
            else if (metadata.type == TypeCode.Vector4)
                metadata.value = EditorGUILayout.Vector4Field(metadata.name, (Vector4)metadata.value);
            else if (metadata.type == TypeCode.Quaternion)
            {
                var q = (Quaternion)metadata.value;
                var value = EditorGUILayout.Vector4Field(metadata.name, new Vector4(q.x, q.y, q.z, q.w));
                var q1 = new Quaternion(value.x, value.y, value.z, value.w);
                metadata.value = q1;
            }
            else if (metadata.type == TypeCode.Rect)
                metadata.value = EditorGUILayout.RectField(metadata.name, (Rect)metadata.value);
            else if (metadata.type == TypeCode.Color)
                metadata.value = EditorGUILayout.ColorField(metadata.name, (Color)metadata.value);
            else if (metadata.type == TypeCode.Color32)
                metadata.value = (Color32)EditorGUILayout.ColorField(metadata.name, (Color32)metadata.value);
            else if (metadata.type == TypeCode.AnimationCurve)
                metadata.value = EditorGUILayout.CurveField(metadata.name, (AnimationCurve)metadata.value);
            else if (metadata.type == TypeCode.Object)
                metadata.value = EditorGUILayout.ObjectField(metadata.name, (Object)metadata.value, metadata.Type, true);
            else if (metadata.type == TypeCode.GenericType | metadata.type == TypeCode.Array)
            {
                var rect = EditorGUILayout.GetControlRect();
                rect.x += width;
                metadata.foldout = EditorGUI.BeginFoldoutHeaderGroup(rect, metadata.foldout, metadata.name);
                if (metadata.foldout)
                {
                    EditorGUI.indentLevel = arrayBeginSpace;
                    EditorGUI.BeginChangeCheck();
                    var arraySize = EditorGUILayout.DelayedIntField("Size", metadata.arraySize);
                    bool flag8 = EditorGUI.EndChangeCheck();
                    IList list = (IList)metadata.value;
                    if (flag8 | list.Count != metadata.arraySize)
                    {
                        metadata.arraySize = arraySize;
                        IList list1 = Array.CreateInstance(metadata.itemType, arraySize);
                        for (int i = 0; i < list1.Count; i++)
                            if (i < list.Count)
                                list1[i] = list[i];
                        if (metadata.type == TypeCode.GenericType)
                        {
                            IList list2 = (IList)Activator.CreateInstance(metadata.Type);
                            for (int i = 0; i < list1.Count; i++)
                                list2.Add(list1[i]);
                            list = list2;
                        }
                        else list = list1;
                    }
                    for (int i = 0; i < list.Count; i++)
                        list[i] = PropertyField("Element " + i, list[i], metadata.itemType);
                    metadata.value = list;
                    EditorGUI.indentLevel = arrayEndSpace;
                }
                EditorGUI.EndFoldoutHeaderGroup();
            }
        }

        private static object PropertyField(string name, object obj, Type type)
        {
            var typeCode = (TypeCode)Type.GetTypeCode(type);
            if (typeCode == TypeCode.Byte)
                obj = (byte)EditorGUILayout.IntField(name, (byte)obj);
            else if (typeCode == TypeCode.SByte)
                obj = (sbyte)EditorGUILayout.IntField(name, (sbyte)obj);
            else if (typeCode == TypeCode.Boolean)
                obj = EditorGUILayout.Toggle(name, (bool)obj);
            else if (typeCode == TypeCode.Int16)
                obj = (short)EditorGUILayout.IntField(name, (short)obj);
            else if (typeCode == TypeCode.UInt16)
                obj = (ushort)EditorGUILayout.IntField(name, (ushort)obj);
            else if (typeCode == TypeCode.Char)
                obj = EditorGUILayout.TextField(name, (string)obj).ToCharArray();
            else if (typeCode == TypeCode.Int32)
                obj = EditorGUILayout.IntField(name, (int)obj);
            else if (typeCode == TypeCode.UInt32)
                obj = (uint)EditorGUILayout.IntField(name, (int)obj);
            else if (typeCode == TypeCode.Single)
                obj = EditorGUILayout.FloatField(name, (float)obj);
            else if (typeCode == TypeCode.Int64)
                obj = EditorGUILayout.LongField(name, (long)obj);
            else if (typeCode == TypeCode.UInt64)
                obj = (ulong)EditorGUILayout.LongField(name, (long)obj);
            else if (typeCode == TypeCode.Double)
                obj = EditorGUILayout.DoubleField(name, (double)obj);
            else if (typeCode == TypeCode.String)
                obj = EditorGUILayout.TextField(name, (string)obj);
            else if (type == typeof(Vector2))
                obj = EditorGUILayout.Vector2Field(name, (Vector2)obj);
            else if (type == typeof(Vector3))
                obj = EditorGUILayout.Vector3Field(name, (Vector3)obj);
            else if (type == typeof(Vector4))
                obj = EditorGUILayout.Vector4Field(name, (Vector4)obj);
            else if (type == typeof(Quaternion))
            {
                var value = EditorGUILayout.Vector4Field(name, (Vector4)obj);
                var quaternion = new Quaternion(value.x, value.y, value.z, value.w);
                obj = quaternion;
            }
            else if (type == typeof(Rect))
                obj = EditorGUILayout.RectField(name, (Rect)obj);
            else if (type == typeof(Color))
                obj = EditorGUILayout.ColorField(name, (Color)obj);
            else if (type == typeof(Color32))
                obj = EditorGUILayout.ColorField(name, (Color32)obj);
            else if (type == typeof(AnimationCurve))
                obj = EditorGUILayout.CurveField(name, (AnimationCurve)obj);
            else if (type.IsSubclassOf(typeof(Object)) | type == typeof(Object))
                obj = EditorGUILayout.ObjectField(name, (Object)obj, type, true);
            return obj;
        }

        /// <summary>
        /// 绘制状态连接行为
        /// </summary>
        public static void DrawTransition(Transition tr)
        {
            EditorGUI.indentLevel = 0;
            var style = GUI.skin.GetStyle("dragtabdropwindow");
            style.fontStyle = FontStyle.Bold;
            style.font = Resources.Load<Font>("Arial");
            style.normal.textColor = Color.red;
            GUILayout.Button(BlueprintGUILayout.Instance.Language["Connection properties"] + tr.CurrState.name + " -> " + tr.NextState.name, style);
            tr.name = tr.CurrState.name + " -> " + tr.NextState.name;
            EditorGUILayout.BeginVertical("ProgressBarBack");

            EditorGUILayout.Space();

            tr.mode = (TransitionMode)EditorGUILayout.Popup(BlueprintGUILayout.Instance.Language["Connection mode"], (int)tr.mode, Enum.GetNames(typeof(TransitionMode)), GUI.skin.GetStyle("PreDropDown"));
            switch (tr.mode)
            {
                case TransitionMode.ExitTime:
                    tr.time = EditorGUILayout.FloatField(BlueprintGUILayout.Instance.Language["current time"], tr.time, GUI.skin.GetStyle("PreDropDown"));
                    tr.exitTime = EditorGUILayout.FloatField(BlueprintGUILayout.Instance.Language["End time"], tr.exitTime, GUI.skin.GetStyle("PreDropDown"));
                    EditorGUILayout.HelpBox(BlueprintGUILayout.Instance.Language["The current time will automatically enter the next state at the end of the time."], MessageType.Info);
                    break;
            }

            GUILayout.Space(10);
            GUILayout.Box("", BlueprintSetting.Instance.HorSpaceStyle, GUILayout.Height(1), GUILayout.ExpandWidth(true));
            GUILayout.Space(10);

            tr.isEnterNextState = EditorGUILayout.Toggle(BlueprintGUILayout.Instance.Language["Enter the next state"], tr.isEnterNextState);

            GUILayout.Space(10);
            GUILayout.Box("", BlueprintSetting.Instance.HorSpaceStyle, GUILayout.Height(1), GUILayout.ExpandWidth(true));

            for (int i = 0; i < tr.behaviours.Length; ++i)
            {
                if (tr.behaviours[i] == null)
                {
                    ArrayExtend.RemoveAt(ref tr.behaviours, i);
                    continue;
                }
                EditorGUI.indentLevel = 1;
                EditorGUILayout.BeginHorizontal();
                var rect = EditorGUILayout.GetControlRect();
                EditorGUI.LabelField(new Rect(rect.x + 20, rect.y, rect.width - 15, 20), tr.behaviours[i].GetType().Name, GUI.skin.GetStyle("BoldLabel"));
                tr.behaviours[i].show = EditorGUI.Foldout(new Rect(rect.x, rect.y, 20, 20), tr.behaviours[i].show, GUIContent.none, true);
                tr.behaviours[i].Active = EditorGUI.ToggleLeft(new Rect(rect.x + 5, rect.y, 30, 20), GUIContent.none, tr.behaviours[i].Active);
                if (GUI.Button(new Rect(rect.x + rect.width - 15, rect.y, rect.width, rect.height), GUIContent.none, GUI.skin.GetStyle("ToggleMixed")))
                {
                    tr.behaviours[i].OnDestroyComponent();
                    ArrayExtend.RemoveAt(ref tr.behaviours, i);
                    continue;
                }
                if (rect.Contains(Event.current.mousePosition) & Event.current.button == 1)
                {
                    var menu = new GenericMenu();
                    menu.AddItem(new GUIContent(BlueprintGUILayout.Instance.Language["Remove connection scripts"]), false, (obj) =>
                    {
                        var index = (int)obj;
                        tr.behaviours[index].OnDestroyComponent();
                        ArrayExtend.RemoveAt(ref tr.behaviours, index);
                        return;
                    }, i);
                    menu.AddItem(new GUIContent(BlueprintGUILayout.Instance.Language["Copy connection scripts"]), false, (obj) =>
                    {
                        var index = (int)obj;
                        StateSystem.CopyComponent = tr.behaviours[index];
                    }, i);
                    menu.AddItem(new GUIContent(BlueprintGUILayout.Instance.Language["Paste a new connection script"]), StateSystem.CopyComponent != null, () =>
                    {
                        if (StateSystem.CopyComponent is TransitionBehaviour behaviour)
                        {
                            TransitionBehaviour ab = (TransitionBehaviour)Net.CloneHelper.DeepCopy(behaviour);
                            ArrayExtend.Add(ref tr.behaviours, ab);
                        }
                    });
                    menu.AddItem(new GUIContent(BlueprintGUILayout.Instance.Language["Paste connection script values"]), StateSystem.CopyComponent != null, (obj) =>
                    {
                        var index = (int)obj;
                        if (StateSystem.CopyComponent is TransitionBehaviour behaviour)
                            if (behaviour.name == tr.behaviours[index].name)
                                tr.behaviours[index] = (TransitionBehaviour)Net.CloneHelper.DeepCopy(behaviour);
                    }, i);
                    menu.AddItem(new GUIContent(BlueprintGUILayout.Instance.Language["Edit connection script"]), false, (obj) =>
                    {
                        var index = (int)obj;
                        var scriptName = tr.behaviours[index].name;
                        if (!Net.Helper.ScriptHelper.Cache.TryGetValue(scriptName, out var sequence))
                            sequence = new Net.Helper.SequencePoint();
                        InternalEditorUtility.OpenFileAtLineExternal(sequence.FilePath, sequence.StartLine, 0);
                    }, i);
                    menu.ShowAsContext();
                }
                EditorGUILayout.EndHorizontal();
                if (tr.behaviours[i].show)
                {
                    EditorGUI.indentLevel = 2;
                    if (!tr.behaviours[i].OnInspectorGUI(tr.CurrState))
                    {
                        foreach (var metadata in tr.behaviours[i].Metadatas)
                        {
                            PropertyField(metadata);
                        }
                    }
                    EditorGUI.indentLevel = 1;
                    GUILayout.Space(10);
                    GUILayout.Box("", BlueprintSetting.Instance.HorSpaceStyle, GUILayout.Height(1), GUILayout.ExpandWidth(true));
                }
            }

            GUILayout.Space(5);

            var r = EditorGUILayout.GetControlRect();
            var rr = new Rect(new Vector2(r.x + (r.size.x / 4f), r.y), new Vector2(r.size.x / 2f, 20));
            if (GUI.Button(rr, BlueprintGUILayout.Instance.Language["Add connection scripts"]))
                addBehaviourState = tr;
            if (addBehaviourState == tr)
            {
                EditorGUILayout.Space();
                foreach (var type in findBehaviourTypes2)
                {
                    if (GUILayout.Button(type.Name))
                    {
                        var stb = (TransitionBehaviour)Activator.CreateInstance(type);
                        stb.InitMetadatas();
                        ArrayExtend.Add(ref tr.behaviours, stb);
                        addBehaviourState = null;
                    }
                    if (compiling & type.Name == createScriptName)
                    {
                        var stb = (TransitionBehaviour)Activator.CreateInstance(type);
                        stb.InitMetadatas();
                        ArrayExtend.Add(ref tr.behaviours, stb);
                        addBehaviourState = null;
                        compiling = false;
                    }
                }

                EditorGUILayout.Space();
                EditorGUILayout.LabelField(BlueprintGUILayout.Instance.Language["Create a connection script path:"]);
                transitionScriptPath = EditorGUILayout.TextField(transitionScriptPath);
                var addRect = EditorGUILayout.GetControlRect();
                createScriptName = EditorGUI.TextField(new Rect(addRect.position, new Vector2(addRect.size.x - 125f, 18)), createScriptName);
                if (GUI.Button(new Rect(new Vector2(addRect.size.x - 105f, addRect.position.y), new Vector2(120, 18)), BlueprintGUILayout.Instance.Language["Create connection scripts"]))
                {
                    var text = Resources.Load<TextAsset>("TransitionBehaviorScript");
                    var scriptCode = text.text.Split(new string[] { "\r\n" }, 0);
                    scriptCode[7] = scriptCode[7].Replace("TransitionBehaviorScript", createScriptName);
                    ScriptTools.CreateScript(Application.dataPath + transitionScriptPath, createScriptName, scriptCode);
                    compiling = true;
                }
                if (GUILayout.Button(BlueprintGUILayout.Instance.Language["cancel"]))
                    addBehaviourState = null;
            }
            GUILayout.Space(10);
            EditorGUILayout.HelpBox(BlueprintGUILayout.Instance.Language["You can create a connection behavior script to control the state to the next state"], MessageType.Info);
            GUILayout.Space(10);
            EditorGUILayout.EndHorizontal();
        }

        [UnityEditor.Callbacks.DidReloadScripts(0)]
        internal static void OnScriptReload()
        {
            if (self == null)
                return;
            self.OnScriptReload();
        }
    }
}
#endif