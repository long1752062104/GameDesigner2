#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace GameDesigner
{
    internal static class Styles
    {
        public static readonly GUIStyle breadCrumbLeft = (GUIStyle)"GUIEditor.BreadcrumbLeft";
        public static readonly GUIStyle breadCrumbMid = (GUIStyle)"GUIEditor.BreadcrumbMid";
        public static readonly GUIStyle breadCrumbLeftBg = (GUIStyle)"GUIEditor.BreadcrumbLeftBackground";
        public static readonly GUIStyle breadCrumbMidBg = (GUIStyle)"GUIEditor.BreadcrumbMidBackground";
    }

    public class StateMachineWindow : GraphEditor
    {
        public static IStateMachine stateMachine;
        private bool dragState = false;
        private State makeTransition;

        [MenuItem("GameDesigner/StateMachine/StateMachine")]
        public static void Init()
        {
            GetWindow<StateMachineWindow>(BlueprintGUILayout.Instance.Language["Game Designer Editor Window"], true);
        }
        public static void Init(IStateMachine stateMachine)
        {
            GetWindow<StateMachineWindow>(BlueprintGUILayout.Instance.Language["Game Designer Editor Window"], true);
            StateMachineWindow.stateMachine = stateMachine;
        }

        private void BreadCrumb(int index, int maxCount, string name)
        {
            var style = index == 0 ? Styles.breadCrumbLeft : Styles.breadCrumbMid;
            var guiStyle = index == 0 ? Styles.breadCrumbLeftBg : Styles.breadCrumbMidBg;
            var content = new GUIContent(name);
            var vector2 = style.CalcSize(content);
            var rect = GUILayoutUtility.GetRect(content, style, GUILayout.Height(20), GUILayout.MaxWidth(vector2.x));
            if (Event.current.type == EventType.Repaint)
                guiStyle.Draw(rect, GUIContent.none, 0);
            GUI.Toggle(rect, index == maxCount - 1, content, style);
        }

        void OnGUI()
        {
            GUILayout.BeginHorizontal();//(EditorStyles.toolbar);

            BreadCrumb(0, 3, "Base Layer");
            BreadCrumb(1, 3, "Base Layer 1");
            BreadCrumb(2, 3, "Base Layer 2");

            GUILayout.FlexibleSpace();
            GUILayout.Space(10);
            if (GUILayout.Button("刷新脚本", EditorStyles.toolbarButton, GUILayout.Width(60)))
            {
                StateMachineViewEditor.OnScriptReload();
                Debug.Log("刷新脚本成功!");
            }
            if (GUILayout.Button(BlueprintGUILayout.Instance.Language["reset"], EditorStyles.toolbarButton, GUILayout.Width(50)))
            {
                if (stateMachine == null)
                    return;
                if (stateMachine.States.Length > 0)
                    UpdateScrollPosition(stateMachine.States[0].rect.position - new Vector2(position.size.x / 2 - 75, position.size.y / 2 - 15)); //更新滑动矩阵
                else
                    UpdateScrollPosition(Center); //归位到矩形的中心
            }
            GUILayout.EndHorizontal();
            ZoomableAreaBegin(new Rect(0f, 0f, scaledCanvasSize.width, scaledCanvasSize.height + 21), scale, false);
            BeginWindow();
            if (stateMachine != null)
                DrawStates();
            EndWindow();
            ZoomableAreaEnd();
            if (stateMachine == null)
                CreateStateMachineMenu();
            else if (openStateMenu)
                OpenStateContextMenu(stateMachine.SelectState);
            else
                OpenWindowContextMenu();
            Repaint();
        }

        private void CreateStateMachineMenu()
        {
            if (currentType == EventType.MouseDown & Event.current.button == 1)
            {
                var menu = new GenericMenu();
                menu.AddItem(new GUIContent(BlueprintGUILayout.Instance.Language["Create a state machine"]), false, delegate
                {
                    var go = Selection.activeGameObject;
                    if (go == null)
                    {
                        EditorUtility.DisplayDialog(
                            BlueprintGUILayout.Instance.Language["Tips!"],
                            BlueprintGUILayout.Instance.Language["Please select the object and click to create the state machine!"],
                            BlueprintGUILayout.Instance.Language["yes"],
                            BlueprintGUILayout.Instance.Language["no"]);
                    }
                    else if (go.GetComponent<StateManager>())
                    {
                        go.GetComponent<StateManager>().support = StateMachineMono.CreateSupport();
                        go.GetComponent<StateManager>().support.transform.SetParent(go.GetComponent<StateManager>().transform);
                        stateMachine = go.GetComponent<StateManager>().support.stateMachine;
                    }
                    else
                    {
                        go.AddComponent<StateManager>().support = StateMachineMono.CreateSupport();
                        go.GetComponent<StateManager>().support.transform.SetParent(go.GetComponent<StateManager>().transform);
                        stateMachine = go.GetComponent<StateManager>().support.stateMachine;
                    }
                });
                menu.ShowAsContext();
                Event.current.Use();
            }
        }

        internal static Transition selectTransition;

        /// <summary>
        /// 绘制状态(状态的层,状态窗口举行)
        /// </summary>
        protected void DrawStates()
        {
            foreach (var state in stateMachine.States)
            {
                DrawLineStatePosToMousePosTransition(state);
                foreach (var t in state.transitions)
                {
                    if (selectTransition == t)
                    {
                        DrawConnection(state.rect.center, t.NextState.rect.center, Color.green, 1, true);
                        if (Event.current.keyCode == KeyCode.Delete)
                        {
                            ArrayExtend.Remove(ref state.transitions, t);
                            for (int i = 0; i < state.transitions.Length; i++)
                                state.transitions[i].ID = i;
                            return;
                        }
                        ClickTransition(state, t);
                    }
                    else
                    {
                        DrawConnection(state.rect.center, t.NextState.rect.center, Color.white, 1, true);
                        ClickTransition(state, t);
                    }
                }
                if (state.rect.Contains(Event.current.mousePosition) & currentType == EventType.MouseDown & Event.current.button == 0)
                {
                    if (Event.current.control)
                        stateMachine.SelectState = state;
                    else if (!stateMachine.SelectStates.Contains(state.ID))
                    {
                        stateMachine.SelectStates = new List<int>
                        {
                            state.ID
                        };
                    }
                    if (state.transitions.Length == 0)
                        selectTransition = null;
                    else
                        selectTransition = state.transitions[0];
                }
                else if (state.rect.Contains(mousePosition) & currentType == EventType.MouseDown & currentEvent.button == 1)
                {
                    openStateMenu = true;
                    stateMachine.SelectState = state;
                }
                if (currentEvent.keyCode == KeyCode.Delete & currentEvent.type == EventType.KeyUp)
                {
                    DeletedState();
                    return;
                }
            }
            foreach (var state in stateMachine.States)
            {
                if (state == stateMachine.DefaultState & stateMachine.SelectState == stateMachine.DefaultState)
                    DragStateBoxPosition(state.rect, state.name, StateMachineSetting.Instance.defaultAndSelectStyle);
                else if (state == stateMachine.DefaultState & state.ID == stateMachine.StateId)
                    DragStateBoxPosition(state.rect, state.name, StateMachineSetting.Instance.defaultAndRuntimeIndexStyle);
                else if (state == stateMachine.DefaultState)
                    DragStateBoxPosition(state.rect, state.name, StateMachineSetting.Instance.stateInDefaultStyle);
                else if (stateMachine.StateId == state.ID)
                    DragStateBoxPosition(state.rect, state.name, StateMachineSetting.Instance.indexInRuntimeStyle);
                else if (state == stateMachine.SelectState)
                    DragStateBoxPosition(state.rect, state.name, StateMachineSetting.Instance.selectStateStyle);
                else
                    DragStateBoxPosition(state.rect, state.name, StateMachineSetting.Instance.defaultStyle);
            }
            DragSelectStates();
        }

        /// <summary>
        /// 绘制选择状态
        /// </summary>
        private void DragSelectStates()
        {
            for (int i = 0; i < stateMachine.SelectStates.Count; i++)
            {
                var state = stateMachine.States[stateMachine.SelectStates[i]];
                DragStateBoxPosition(state.rect, state.name, StateMachineSetting.Instance.selectStateStyle);
            }

            switch (currentType)
            {
                case EventType.MouseDown:
                    selectionStartPosition = mousePosition;
                    if (currentEvent.button == 2 | currentEvent.button == 1)
                    {
                        mode = SelectMode.none;
                        return;
                    }
                    foreach (State state in stateMachine.States)
                    {
                        if (state.rect.Contains(currentEvent.mousePosition))
                        {
                            mode = SelectMode.dragState;
                            return;
                        }
                    }
                    mode = SelectMode.selectState;
                    break;
                case EventType.MouseUp:
                    mode = SelectMode.none;
                    break;
            }

            switch (mode)
            {
                case SelectMode.dragState:
                    if (stateMachine.SelectState != null)
                        DragStateBoxPosition(stateMachine.SelectState.rect, stateMachine.SelectState.name, StateMachineSetting.Instance.selectStateStyle);
                    break;
                case SelectMode.selectState:
                    GUI.Box(FromToRect(selectionStartPosition, mousePosition), "", "SelectionRect");
                    SelectStatesInRect(FromToRect(selectionStartPosition, mousePosition));
                    break;
            }
        }

        private void SelectStatesInRect(Rect r)
        {
            for (int i = 0; i < stateMachine.States.Length; i++)
            {
                var rect = stateMachine.States[i].rect;
                if (rect.xMax < r.x || rect.x > r.xMax || rect.yMax < r.y || rect.y > r.yMax)
                {
                    stateMachine.SelectStates.Remove(stateMachine.States[i].ID);
                    continue;
                }
                if (!stateMachine.SelectStates.Contains(stateMachine.States[i].ID))
                {
                    stateMachine.SelectStates.Add(stateMachine.States[i].ID);
                }
                DragStateBoxPosition(stateMachine.States[i].rect, stateMachine.States[i].name, StateMachineSetting.Instance.selectStateStyle);
            }
        }

        private Rect FromToRect(Vector2 start, Vector2 end)
        {
            var rect = new Rect(start.x, start.y, end.x - start.x, end.y - start.y);
            if (rect.width < 0f)
            {
                rect.x += rect.width;
                rect.width = -rect.width;
            }
            if (rect.height < 0f)
            {
                rect.y += rect.height;
                rect.height = -rect.height;
            }
            return rect;
        }

        /// <summary>
        /// 点击连接线条
        /// </summary>

        protected void ClickTransition(State state, Transition t)
        {
            if (state.rect.Contains(mousePosition) | t.NextState.rect.Contains(mousePosition))
                return;
            if (currentType == EventType.MouseDown)
            {
                bool offset = state.ID > t.NextState.ID;
                Vector3 start = state.rect.center;
                Vector3 end = t.NextState.rect.center;
                Vector3 cross = Vector3.Cross((start - end).normalized, Vector3.forward);
                if (offset)
                {
                    start += cross * 6;
                    end += cross * 6;
                }
                if (HandleUtility.DistanceToLine(start, end) < 8f)//返回到线的距离
                {
                    selectTransition = t;
                    stateMachine.SelectState = state;
                }
            }
        }

        /// <summary>
        /// 绘制一条从状态点到鼠标位置的线条
        /// </summary>

        protected void DrawLineStatePosToMousePosTransition(State state)
        {
            if (state == null)
                return;
            if (makeTransition == state)
            {
                var startpos = new Vector2(state.rect.x + 80, state.rect.y + 15);
                var endpos = currentEvent.mousePosition;
                DrawConnection(startpos, endpos, Color.white, 1, true);
                if (currentEvent.button == 0 & currentType == EventType.MouseDown)
                {
                    foreach (var s in stateMachine.States)
                    {
                        if (state != s & s.rect.Contains(mousePosition))
                        {
                            foreach (var t in state.transitions)
                            {
                                if (t.NextState == s)// 如果拖动的线包含在自身状态盒矩形内,则不添加连接线
                                {
                                    makeTransition = null;
                                    return;
                                }
                            }
                            Transition.CreateTransitionInstance(state, s);
                            break;
                        }
                    }
                    makeTransition = null;
                }
            }
        }

        /// <summary>
        /// 右键打开状态菜单
        /// </summary>
        protected void OpenStateContextMenu(State state)
        {
            if (state == null)
            {
                openStateMenu = false;
                return;
            }

            if (currentType == EventType.MouseDown & currentEvent.button == 0)
            {
                openStateMenu = false;
            }
            else if (currentType == EventType.MouseDown & currentEvent.button == 1)
            {
                var menu = new GenericMenu();
                menu.AddItem(new GUIContent(BlueprintGUILayout.Instance.Language["Create transition"]), false, () =>
                {
                    makeTransition = state;
                });
                menu.AddSeparator("");
                menu.AddItem(new GUIContent(BlueprintGUILayout.Instance.Language["Default state"]), false, () =>
                {
                    stateMachine.DefaultState = state;
                });
                menu.AddSeparator("");
                menu.AddItem(new GUIContent(BlueprintGUILayout.Instance.Language["Replication state"]), false, () =>
                {
                    stateMachine.SelectState = state;
                });
                menu.AddItem(new GUIContent(BlueprintGUILayout.Instance.Language["deleted state"]), false, () => { DeletedState(); });
                menu.ShowAsContext();
                Event.current.Use();
            }
        }

        /// <summary>
        /// 删除状态节点
        /// </summary>
        private void DeletedState()
        {
            foreach (var state in stateMachine.States)
            {
                for (int n = 0; n < state.transitions.Length; n++)
                {
                    if (state.transitions[n].NextState == null)
                        continue;
                    if (stateMachine.SelectStates.Contains(state.transitions[n].NextState.ID))
                        ArrayExtend.RemoveAt(ref state.transitions, n);
                }
            }
            var ids = new List<int>();
            foreach (var i in stateMachine.SelectStates)
                ids.Add(stateMachine.States[i].ID);
            while (ids.Count > 0)
            {
                for (int i = 0; i < stateMachine.States.Length; i++)
                {
                    if (stateMachine.States[i].ID == ids[0])
                    {
                        stateMachine.States = ArrayExtend.RemoveAt(stateMachine.States, i);
                        EditorUtility.SetDirty(stateMachine.transform.gameObject);
                        break;
                    }
                }
                ids.RemoveAt(0);
            }
            stateMachine.UpdateStates();
            stateMachine.SelectStates.Clear();
            selectTransition = null;
        }

        /// <summary>
        /// 右键打开窗口菜单
        /// </summary>

        protected void OpenWindowContextMenu()
        {
            if (stateMachine == null)
                return;
            if (currentType == EventType.MouseDown && currentEvent.button == 1)
            {
                foreach (State state in stateMachine.States)
                {
                    if (state.rect.Contains(currentEvent.mousePosition))
                        return;
                }
                var menu = new GenericMenu();
                menu.AddItem(new GUIContent(BlueprintGUILayout.Instance.Language["Create state"]), false, () =>
                {
                    State.AddNode(stateMachine, BlueprintGUILayout.Instance.Language["New state"] + stateMachine.States.Length, mousePosition);
                });
                menu.AddItem(new GUIContent("创建子状态机"), false, () =>
                {
                    State.AddNode(stateMachine, BlueprintGUILayout.Instance.Language["New state"] + stateMachine.States.Length, mousePosition);
                });
                if (stateMachine.SelectState != null)
                {
                    menu.AddItem(new GUIContent(BlueprintGUILayout.Instance.Language["Paste Selection Status"]), false, () =>
                    {
                        var states = new List<State>();
                        var seles = stateMachine.SelectStates;
                        var s = Net.CloneHelper.DeepCopy<State>(stateMachine.States[seles[0]], new List<System.Type>() { typeof(Object), typeof(StateMachineCore) });
                        s.perID = s.ID;
                        s.ID = stateMachine.States.Length;
                        s.rect.center = mousePosition;
                        stateMachine.States = ArrayExtend.Add(stateMachine.States, s);
                        states.Add(s);
                        var dis = stateMachine.States[seles[0]].rect.center - mousePosition;
                        for (int i = 1; i < stateMachine.SelectStates.Count; ++i)
                        {
                            var ss = Net.CloneHelper.DeepCopy<State>(stateMachine.States[seles[i]], new List<System.Type>() { typeof(Object), typeof(StateMachineCore) });
                            ss.perID = ss.ID;
                            ss.ID = stateMachine.States.Length;
                            ss.rect.position -= dis;
                            stateMachine.States = ArrayExtend.Add(stateMachine.States, ss);
                            states.Add(ss);
                        }
                        foreach (var state in states)
                            foreach (var tran in state.transitions)
                                foreach (var sta in states)
                                    if (tran.nextStateID == sta.perID)
                                        tran.nextStateID = sta.ID;
                        stateMachine.UpdateStates();
                        var list = new List<int>();
                        for (int i = 0; i < states.Count; ++i)
                            list.Add(states[i].ID);
                        stateMachine.SelectStates = list;
                    });
                    menu.AddItem(new GUIContent(BlueprintGUILayout.Instance.Language["Delete Selection State"]), false, DeletedState);
                }
                menu.AddSeparator("");
                menu.AddItem(new GUIContent(BlueprintGUILayout.Instance.Language["Create and replace state machines"]), false, () =>
                {
                    if (Selection.activeGameObject == null)
                    {
                        EditorUtility.DisplayDialog(
                            BlueprintGUILayout.Instance.Language["Tips!"],
                            BlueprintGUILayout.Instance.Language["Please select the object and click to create the state machine!"],
                            BlueprintGUILayout.Instance.Language["yes"],
                            BlueprintGUILayout.Instance.Language["no"]);
                        return;
                    }
                    if (!Selection.activeGameObject.TryGetComponent<StateManager>(out var manager))
                        manager = Selection.activeGameObject.AddComponent<StateManager>();
                    else if (manager.support != null)
                        DestroyImmediate(manager.support.gameObject, true);
                    var support = StateMachineMono.CreateSupport();
                    manager.support = support;
                    support.transform.SetParent(manager.transform);
                });
                menu.AddItem(new GUIContent(BlueprintGUILayout.Instance.Language["Create and replace state machines"]), false, () =>
                {
                    if (Selection.activeGameObject == null)
                    {
                        EditorUtility.DisplayDialog(
                            BlueprintGUILayout.Instance.Language["Tips!"],
                            BlueprintGUILayout.Instance.Language["Please select the object and click to create the state machine!"],
                            BlueprintGUILayout.Instance.Language["yes"],
                            BlueprintGUILayout.Instance.Language["no"]);
                        return;
                    }
                    if (!Selection.activeGameObject.TryGetComponent<StateManager>(out var manager))
                        manager = Selection.activeGameObject.AddComponent<StateManager>();
                    var support = StateMachineMono.CreateSupport(BlueprintGUILayout.Instance.Language["New state machine"]);
                    manager.support = support;
                    support.transform.SetParent(manager.transform);
                });
                menu.AddItem(new GUIContent(BlueprintGUILayout.Instance.Language["Delete state machine"]), false, () =>
                {
                    if (stateMachine == null)
                        return;
                    Undo.DestroyObjectImmediate(stateMachine.transform.gameObject);
                });
                menu.AddItem(new GUIContent(BlueprintGUILayout.Instance.Language["Delete state manager"]), false, () =>
                {
                    if (stateMachine == null)
                        return;
                    Undo.DestroyObjectImmediate(stateMachine.transform.gameObject);
                });
                menu.ShowAsContext();
                Event.current.Use();
            }
        }

        protected Rect DragStateBoxPosition(Rect dragRect, string name, GUIStyle style = null, int eventButton = 0)
        {
            GUI.Box(dragRect, name, style);
            if (Event.current.button == eventButton)
            {
                switch (Event.current.rawType)
                {
                    case EventType.MouseDown:
                        if (dragRect.Contains(Event.current.mousePosition))
                            dragState = true;
                        break;
                    case EventType.MouseDrag:
                        if (dragState & stateMachine.SelectState != null)
                        {
                            foreach (var state in stateMachine.SelectStates)
                            {
                                stateMachine.States[state].rect.position += Event.current.delta;//拖到状态按钮
                            }
                        }
                        Event.current.Use();
                        break;
                    case EventType.MouseUp:
                        dragState = false;
                        break;
                }
            }
            return dragRect;
        }
    }
}
#endif