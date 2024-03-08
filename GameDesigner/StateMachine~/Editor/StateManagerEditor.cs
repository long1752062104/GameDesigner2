#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using Object = UnityEngine.Object;
#if SHADER_ANIMATED
using FSG.MeshAnimator.ShaderAnimated;
#endif

namespace GameDesigner
{
    [CustomEditor(typeof(StateManager))]
    [CanEditMultipleObjects]
    public class StateManagerEditor : Editor
    {
        private static StateManager stateManager = null;
        public static string createScriptName = "NewStateBehaviour";
        public static string stateActionScriptPath = "/Actions/StateActions";
        public static string stateBehaviourScriptPath = "/Actions/StateBehaviours";
        public static string transitionScriptPath = "/Actions/Transitions";
        private static StateBase addBehaviourState;
        private static bool compiling;
        private static List<Type> findBehaviourTypes;
        private static List<Type> findBehaviourTypes1;
        private static List<Type> findBehaviourTypes2;
        private static bool animPlay;
        private static StateAction animAction;

        void OnEnable()
        {
            stateManager = target as StateManager;
            var stateMachine = stateManager.stateMachine;
            if (stateMachine != null)
            {
                if (stateMachine.animation == null)
                    stateMachine.animation = stateManager.GetComponentInChildren<Animation>();
                if (stateMachine.animation != null)
                {
                    var clips = AnimationUtility.GetAnimationClips(stateMachine.animation.gameObject);
                    stateMachine.clipNames.Clear();
                    foreach (var clip in clips)
                        stateMachine.clipNames.Add(clip.name);
                }
                if (stateMachine.animator == null)
                    stateMachine.animator = stateManager.GetComponentInChildren<Animator>();
                if (stateMachine.animator != null)
                {
                    if (stateMachine.animator.runtimeAnimatorController is UnityEditor.Animations.AnimatorController controller)
                    {
                        if (controller.layers.Length > 0) //打AB包后选择这里是0
                        {
                            var layer = controller.layers[0];
                            var states = layer.stateMachine.states;
                            stateMachine.clipNames.Clear();
                            foreach (var state in states)
                                stateMachine.clipNames.Add(state.state.name);
                        }
                    }
                }
#if SHADER_ANIMATED
                if (stateMachine.meshAnimator == null)
                    stateMachine.meshAnimator = stateManager.GetComponentInChildren<ShaderMeshAnimator>();
                if (stateMachine.meshAnimator != null)
                {
                    var clips = stateMachine.meshAnimator.animations;
                    if (stateMachine.clipNames.Count != clips.Length)
                    {
                        stateMachine.clipNames.Clear();
                        foreach (var clip in clips)
                        {
                            stateMachine.clipNames.Add(clip.AnimationName);
                        }
                    }
                }
#endif
            }
            StateMachineWindow.stateMachine = stateManager.stateMachine;
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

        private void AddBehaviourTypes(List<Type> types, Type type)
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
            EditorGUILayout.PropertyField(serializedObject.FindProperty("stateMachine"), new GUIContent(BlueprintGUILayout.Instance.Language["State Machine Controller"]));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("initMode"), new GUIContent(BlueprintGUILayout.Instance.Language["initMode"]));
            if (GUILayout.Button(BlueprintSetting.Instance.Language["Open the state machine editor"], GUI.skin.GetStyle("LargeButtonMid"), GUILayout.ExpandWidth(true)))
                StateMachineWindow.Init(stateManager.stateMachine);
            if (stateManager.stateMachine == null)
                goto J;
            if (stateManager.stateMachine.selectState != null)
            {
                DrawState(stateManager.stateMachine.selectState, stateManager);
                EditorGUILayout.Space();
                for (int i = 0; i < stateManager.stateMachine.selectState.transitions.Length; ++i)
                    DrawTransition(stateManager.stateMachine.selectState.transitions[i]);
            }
            else if (StateMachineWindow.selectTransition != null)
            {
                DrawTransition(StateMachineWindow.selectTransition);
            }
            EditorGUILayout.Space();
            if (EditorGUI.EndChangeCheck())
                EditorUtility.SetDirty(stateManager.stateMachine);
            Repaint();
        J: serializedObject.ApplyModifiedProperties();
        }

        private static SerializedObject _stateMachineObject;
        private static SerializedObject StateMachineObject
        {
            get
            {
                if (_stateMachineObject == null)
                    _stateMachineObject = new SerializedObject(stateManager.stateMachine);
                return _stateMachineObject;
            }
        }

        private static SerializedProperty _statesProperty;
        private static SerializedProperty StatesProperty
        {
            get
            {
                if (_statesProperty == null)
                    _statesProperty = StateMachineObject.FindProperty("states").GetArrayElementAtIndex(stateManager.stateMachine.selectState.ID);
                return _statesProperty;
            }
        }

        private static SerializedProperty _nameProperty;
        private static SerializedProperty NameProperty
        {
            get
            {
                if (_nameProperty == null)
                    _nameProperty = StatesProperty.FindPropertyRelative("name");
                return _nameProperty;
            }
        }

        private static SerializedProperty _actionSystemProperty;
        private static SerializedProperty ActionSystemProperty
        {
            get
            {
                if (_actionSystemProperty == null)
                    _actionSystemProperty = StatesProperty.FindPropertyRelative("actionSystem");
                return _actionSystemProperty;
            }
        }

        private static SerializedProperty _animSpeedProperty;
        private static SerializedProperty animSpeedProperty
        {
            get
            {
                if (_animSpeedProperty == null)
                    _animSpeedProperty = StatesProperty.FindPropertyRelative("animSpeed");
                return _animSpeedProperty;
            }
        }

        private static SerializedProperty _animLoopProperty;
        private static SerializedProperty animLoopProperty
        {
            get
            {
                if (_animLoopProperty == null)
                    _animLoopProperty = StatesProperty.FindPropertyRelative("animLoop");
                return _animLoopProperty;
            }
        }

        private static SerializedProperty _actionsProperty;
        private static SerializedProperty actionsProperty
        {
            get
            {
                if (_actionsProperty == null)
                    _actionsProperty = StatesProperty.FindPropertyRelative("actions");
                return _actionsProperty;
            }
        }

        private static SerializedProperty _animationProperty;
        private static SerializedProperty animationProperty
        {
            get
            {
                if (_animationProperty == null)
                    _animationProperty = StateMachineObject.FindProperty("animation");
                return _animationProperty;
            }
        }

        private static SerializedProperty _animatorProperty;
        private static SerializedProperty animatorProperty
        {
            get
            {
                if (_animatorProperty == null)
                    _animatorProperty = StateMachineObject.FindProperty("animator");
                return _animatorProperty;
            }
        }

        private static SerializedProperty _meshAnimatorProperty;
        private static SerializedProperty meshAnimatorProperty
        {
            get
            {
                if (_meshAnimatorProperty == null)
                    _meshAnimatorProperty = StateMachineObject.FindProperty("meshAnimator");
                return _meshAnimatorProperty;
            }
        }

        private static void ResetPropertys()
        {
            _stateMachineObject = null;
            _statesProperty = null;
            _nameProperty = null;
            _actionSystemProperty = null;
            _animSpeedProperty = null;
            _animLoopProperty = null;
            _actionsProperty = null;
            _animationProperty = null;
            _animatorProperty = null;
            _meshAnimatorProperty = null;
        }

        private static State CurrentState;

        /// <summary>
        /// 绘制状态监视面板属性
        /// </summary>
        public static void DrawState(State state, StateManager sm)
        {
            if (CurrentState != state)
            {
                CurrentState = state;
                ResetPropertys();
            }
            StateMachineObject.Update();
            GUILayout.Button(BlueprintGUILayout.Instance.Language["State attribute"], GUI.skin.GetStyle("dragtabdropwindow"));
            EditorGUILayout.BeginVertical("ProgressBarBack");
            EditorGUILayout.PropertyField(NameProperty, new GUIContent(BlueprintGUILayout.Instance.Language["Status name"], "name"));
            EditorGUILayout.IntField(new GUIContent(BlueprintGUILayout.Instance.Language["Status identifier"], "stateID"), state.ID);
            EditorGUILayout.PropertyField(ActionSystemProperty, new GUIContent(BlueprintGUILayout.Instance.Language["Action system"], "actionSystem  专为玩家角色AI其怪物AI所设计的一套AI系统！"));
            if (state.actionSystem)
            {
                sm.stateMachine.animMode = (AnimationMode)EditorGUILayout.EnumPopup(new GUIContent(BlueprintGUILayout.Instance.Language["Animation mode"], "animMode"), sm.stateMachine.animMode);
                switch (sm.stateMachine.animMode)
                {
                    case AnimationMode.Animation:
                        EditorGUILayout.PropertyField(animationProperty, new GUIContent(BlueprintGUILayout.Instance.Language["Old animation"], "animation"));
                        break;
                    case AnimationMode.Animator:
                        EditorGUILayout.PropertyField(animatorProperty, new GUIContent(BlueprintGUILayout.Instance.Language["New animation"], "animator"));
                        break;
#if SHADER_ANIMATED
                    case AnimationMode.MeshAnimator:
                        EditorGUILayout.PropertyField(meshAnimatorProperty, new GUIContent(BlueprintGUILayout.Instance.Language["mesh Animated"], "meshAnimator"));
                        break;
#endif
                }
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
                    state.DstStateID = EditorGUILayout.Popup(BlueprintGUILayout.Instance.Language["get into the state"], state.DstStateID, Array.ConvertAll(state.transitions.ToArray(), new Converter<Transition, string>(delegate (Transition t) { return t.currState.name + " -> " + t.nextState.name + "   ID:" + t.nextState.ID; })));
                BlueprintGUILayout.BeginStyleVertical(BlueprintGUILayout.Instance.Language["Action tree"], "ProgressBarBack");
                EditorGUI.indentLevel = 1;
                Rect actRect = EditorGUILayout.GetControlRect();
                state.foldout = EditorGUI.Foldout(new Rect(actRect.position, new Vector2(actRect.size.x - 120f, 15)), state.foldout, BlueprintGUILayout.Instance.Language["Action Tree Set"], true);

                if (GUI.Button(new Rect(new Vector2(actRect.size.x - 40f, actRect.position.y), new Vector2(60, 16)), BlueprintGUILayout.Instance.Language["Add action"]))
                {
                    ArrayExtend.Add(ref state.actions, new StateAction() { ID = state.ID, stateMachine = state.stateMachine, behaviours = new BehaviourBase[0] });
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
                                    ArrayExtend.Add(ref state.actions, Net.CloneHelper.DeepCopy<StateAction>(stateAction));
                            });
                            menu.AddItem(new GUIContent(BlueprintGUILayout.Instance.Language["Paste action value"]), StateSystem.CopyComponent != null, (obj) =>
                            {
                                if (StateSystem.Component is StateAction stateAction)
                                {
                                    var index = (int)obj;
                                    if (stateAction == state.actions[index])//如果要黏贴的动作是复制的动作则返回
                                        return;
                                    state.actions[index] = Net.CloneHelper.DeepCopy<StateAction>(stateAction);
                                }
                            }, x);
                            menu.ShowAsContext();
                        }
                        if (act.foldout)
                        {
                            EditorGUI.indentLevel = 3;
                            try
                            {
                                act.clipIndex = EditorGUILayout.Popup(new GUIContent(BlueprintGUILayout.Instance.Language["Movie clips"], "clipIndex"), act.clipIndex, Array.ConvertAll(state.stateMachine.clipNames.ToArray(), input => new GUIContent(input)));
                                act.clipName = state.stateMachine.clipNames[act.clipIndex];
                            }
                            catch { }
                            EditorGUILayout.PropertyField(actionProperty.FindPropertyRelative("animTime"), new GUIContent(BlueprintGUILayout.Instance.Language["Animation time"], "animTime"));
                            EditorGUILayout.PropertyField(actionProperty.FindPropertyRelative("animTimeMax"), new GUIContent(BlueprintGUILayout.Instance.Language["Animation length"], "animTimeMax"));
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
                            switch (sm.stateMachine.animMode)
                            {
                                case AnimationMode.Animation:
                                    break;
                                case AnimationMode.Animator:
                                    AnimatorPlay(sm, act);
                                    break;
                                case AnimationMode.Time:
                                    break;
                                case AnimationMode.None:
                                    break;
                            }
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
                                            stb.InitMetadatas(act.stateMachine);
                                            stb.ID = state.ID;
                                            ArrayExtend.Add(ref act.behaviours, stb);
                                            addBehaviourState = null;
                                            EditorUtility.SetDirty(act.stateMachine);
                                        }
                                        if (compiling & type.Name == createScriptName)
                                        {
                                            var stb = (ActionBehaviour)Activator.CreateInstance(type);
                                            stb.InitMetadatas(sm.stateMachine);
                                            stb.ID = state.ID;
                                            ArrayExtend.Add(ref act.behaviours, stb);
                                            addBehaviourState = null;
                                            compiling = false;
                                            EditorUtility.SetDirty(act.stateMachine);
                                        }
                                    }
                                }
                                catch { }
                                EditorGUILayout.Space();
                                EditorGUI.indentLevel = 0;
                                EditorGUILayout.LabelField(BlueprintGUILayout.Instance.Language["Create action script paths:"]);
                                stateActionScriptPath = EditorGUILayout.TextField(stateActionScriptPath);
                                Rect addRect = EditorGUILayout.GetControlRect();
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
            StateMachineObject.ApplyModifiedProperties();
        }

        private static void AnimatorPlay(StateManager sm, StateAction act)
        {
            var animator = sm.stateMachine.animator;
            EditorGUILayout.BeginHorizontal();
            var rect = EditorGUILayout.GetControlRect();
            if (GUI.Button(new Rect(rect.x + 45, rect.y, 30, rect.height), EditorGUIUtility.IconContent(animPlay ? "PauseButton" : "PlayButton")))
            {
                animPlay = !animPlay;
                animAction = act;
            }
            EditorGUI.BeginChangeCheck();
            act.animTime = GUI.HorizontalSlider(new Rect(rect.x + 75, rect.y, rect.width - 75, rect.height), act.animTime, 0f, act.animTimeMax);
            var normalizedTime = act.animTime / act.animTimeMax;
            EditorGUI.ProgressBar(new Rect(rect.x + 75, rect.y, rect.width - 75, rect.height), normalizedTime, $"动画进度:{act.animTime.ToString("f0")}");
            if (EditorGUI.EndChangeCheck())
            {
                animPlay = false;
                animAction = act;
                if (!EditorApplication.isPlaying)
                {
                    animator.Play(act.clipName, 0, normalizedTime);
                    animator.Update(0f);
                }
            }
            EditorGUILayout.EndHorizontal();
            if (animPlay && animAction == act && !EditorApplication.isPlaying)
            {
                act.animTime += 20f * Time.deltaTime;
                if (act.animTime >= act.animTimeMax)
                    act.animTime = 0f;
                animator.Play(act.clipName, 0, normalizedTime);
                animator.Update(0f);
            }
        }

        /// <summary>
        /// 绘制状态行为
        /// </summary>
        public static void DrawBehaviours(State s)
        {
            GUILayout.Space(10);
            GUILayout.Box("", BlueprintSetting.Instance.HorSpaceStyle, GUILayout.Height(1), GUILayout.ExpandWidth(true));
            GUILayout.Space(5);
            for (int i = 0; i < s.behaviours.Length; ++i)
            {
                EditorGUI.indentLevel = 1;
                EditorGUILayout.BeginHorizontal();
                Rect rect = EditorGUILayout.GetControlRect();
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

            Rect r = EditorGUILayout.GetControlRect();
            Rect rr = new Rect(new Vector2(r.x + (r.size.x / 4f), r.y), new Vector2(r.size.x / 2f, 20));
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
                            stb.InitMetadatas(s.stateMachine);
                            stb.ID = s.ID;
                            ArrayExtend.Add(ref s.behaviours, stb);
                            addBehaviourState = null;
                            EditorUtility.SetDirty(s.stateMachine);
                        }
                        if (compiling & type.Name == createScriptName)
                        {
                            var stb = (StateBehaviour)Activator.CreateInstance(type);
                            stb.InitMetadatas(s.stateMachine);
                            stb.ID = s.ID;
                            ArrayExtend.Add(ref s.behaviours, stb);
                            addBehaviourState = null;
                            compiling = false;
                            EditorUtility.SetDirty(s.stateMachine);
                        }
                    }
                }
                catch { }
                EditorGUILayout.Space();
                EditorGUI.indentLevel = 0;
                EditorGUILayout.LabelField(BlueprintGUILayout.Instance.Language["Create a status script path:"]);
                stateBehaviourScriptPath = EditorGUILayout.TextField(stateBehaviourScriptPath);
                Rect addRect = EditorGUILayout.GetControlRect();
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
                Quaternion q = (Quaternion)metadata.value;
                var value = EditorGUILayout.Vector4Field(metadata.name, new Vector4(q.x, q.y, q.z, q.w));
                Quaternion q1 = new Quaternion(value.x, value.y, value.z, value.w);
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
                Quaternion quaternion = new Quaternion(value.x, value.y, value.z, value.w);
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
            GUIStyle style = GUI.skin.GetStyle("dragtabdropwindow");
            style.fontStyle = FontStyle.Bold;
            style.font = Resources.Load<Font>("Arial");
            style.normal.textColor = Color.red;
            GUILayout.Button(BlueprintGUILayout.Instance.Language["Connection properties"] + tr.currState.name + " -> " + tr.nextState.name, style);
            tr.name = tr.currState.name + " -> " + tr.nextState.name;
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
                Rect rect = EditorGUILayout.GetControlRect();
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
                    if (!tr.behaviours[i].OnInspectorGUI(tr.currState))
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

            Rect r = EditorGUILayout.GetControlRect();
            Rect rr = new Rect(new Vector2(r.x + (r.size.x / 4f), r.y), new Vector2(r.size.x / 2f, 20));
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
                        stb.InitMetadatas(tr.stateMachine);
                        ArrayExtend.Add(ref tr.behaviours, stb);
                        addBehaviourState = null;
                    }
                    if (compiling & type.Name == createScriptName)
                    {
                        var stb = (TransitionBehaviour)Activator.CreateInstance(type);
                        stb.InitMetadatas(tr.stateMachine);
                        ArrayExtend.Add(ref tr.behaviours, stb);
                        addBehaviourState = null;
                        compiling = false;
                    }
                }

                EditorGUILayout.Space();
                EditorGUILayout.LabelField(BlueprintGUILayout.Instance.Language["Create a connection script path:"]);
                transitionScriptPath = EditorGUILayout.TextField(transitionScriptPath);
                Rect addRect = EditorGUILayout.GetControlRect();
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
            if (stateManager == null)
                return;
            stateManager.OnValidate();
        }
    }

}
#endif