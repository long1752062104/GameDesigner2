#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace GameDesigner
{
    public class StateMachineWindow : GraphEditor
    {
        public static IStateMachineView fsmView;

        private bool dragState = false;
        private State makeTransition;

        [MenuItem("GameDesigner/StateMachine/StateMachine")]
        public static void Init()
        {
            GetWindow<StateMachineWindow>(BlueprintGUILayout.Instance.Language["Game Designer Editor Window"], true);
        }
        public static void Init(IStateMachineView fsmView)
        {
            GetWindow<StateMachineWindow>(BlueprintGUILayout.Instance.Language["Game Designer Editor Window"], true);
            StateMachineWindow.fsmView = fsmView;
        }

        void OnGUI()
        {
            GUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.Button(new GUIContent(fsmView != null ? fsmView.name : "None", BlueprintGUILayout.Instance.stateMachineImage), GUI.skin.GetStyle("GUIEditor.BreadcrumbLeft"), GUILayout.Width(150));
            EditorGUILayout.ToggleLeft("固定", true, GUILayout.Width(50));
            if (fsmView == null)
                fsmView = (IStateMachineView)EditorGUILayout.ObjectField(GUIContent.none, (Object)fsmView, typeof(Object), true, GUILayout.Width(150));
            else
                fsmView = (IStateMachineView)EditorGUILayout.ObjectField(GUIContent.none, (Object)fsmView, fsmView.GetType(), true, GUILayout.Width(150));
            GUILayout.FlexibleSpace();
            GUILayout.Space(10);
            if (GUILayout.Button("刷新脚本", GUILayout.Width(60)))
            {
                StateManagerEditor.OnScriptReload();
                Debug.Log("刷新脚本成功!");
            }
            if (GUILayout.Button(BlueprintGUILayout.Instance.Language["reset"], GUILayout.Width(50)))
            {
                if (fsmView == null)
                    return;
                if (fsmView.States.Length > 0)
                    UpdateScrollPosition(fsmView.States[0].rect.position - new Vector2(position.size.x / 2 - 75, position.size.y / 2 - 15)); //更新滑动矩阵
                else
                    UpdateScrollPosition(Center); //归位到矩形的中心
            }
            GUILayout.Space(10);
            GUILayout.EndHorizontal();
            ZoomableAreaBegin(new Rect(0f, 0f, scaledCanvasSize.width, scaledCanvasSize.height + 21), scale, false);
            BeginWindow();
            if (fsmView != null)
                DrawStates();
            EndWindow();
            ZoomableAreaEnd();
            if (fsmView == null)
                CreateStateMachineMenu();
            else if (openStateMenu)
                OpenStateContextMenu(fsmView.SelectState);
            else
                OpenWindowContextMenu();
            Repaint();
        }

        private void CreateStateMachineMenu()
        {
            if (currentType == EventType.MouseDown & Event.current.button == 1)
            {
                GenericMenu menu = new GenericMenu();
                menu.AddItem(new GUIContent(BlueprintGUILayout.Instance.Language["Create a state machine"]), false, delegate
                {
                    if (Selection.activeGameObject == null)
                    {
                        EditorUtility.DisplayDialog(
                            BlueprintGUILayout.Instance.Language["Tips!"],
                            BlueprintGUILayout.Instance.Language["Please select the object and click to create the state machine!"],
                            BlueprintGUILayout.Instance.Language["yes"],
                            BlueprintGUILayout.Instance.Language["no"]);
                    }
                    else if (Selection.activeGameObject.GetComponent<StateManager>())
                    {
                        Selection.activeGameObject.GetComponent<StateManager>().stateMachine = StateMachine.CreateStateMachineInstance();
                        Selection.activeGameObject.GetComponent<StateManager>().stateMachine.transform.SetParent(Selection.activeGameObject.GetComponent<StateManager>().transform);
                        fsmView = Selection.activeGameObject.GetComponent<StateManager>().stateMachine;
                    }
                    else
                    {
                        Selection.activeGameObject.AddComponent<StateManager>().stateMachine = StateMachine.CreateStateMachineInstance();
                        Selection.activeGameObject.GetComponent<StateManager>().stateMachine.transform.SetParent(Selection.activeGameObject.GetComponent<StateManager>().transform);
                        fsmView = Selection.activeGameObject.GetComponent<StateManager>().stateMachine;
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
            foreach (var state in fsmView.States)
            {
                DrawLineStatePosToMousePosTransition(state);
                foreach (var t in state.transitions)
                {
                    if (selectTransition == t)
                    {
                        DrawConnection(state.rect.center, t.nextState.rect.center, Color.green, 1, true);
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
                        DrawConnection(state.rect.center, t.nextState.rect.center, Color.white, 1, true);
                        ClickTransition(state, t);
                    }
                }
                if (state.rect.Contains(Event.current.mousePosition) & currentType == EventType.MouseDown & Event.current.button == 0)
                {
                    if (Event.current.control)
                        fsmView.SelectState = state;
                    else if (!fsmView.SelectStates.Contains(state.ID))
                    {
                        fsmView.SelectStates = new List<int>
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
                    fsmView.SelectState = state;
                }
                if (currentEvent.keyCode == KeyCode.Delete & currentEvent.type == EventType.KeyUp)
                {
                    DeletedState();
                    return;
                }
            }
            foreach (var state in fsmView.States)
            {
                if (state == fsmView.DefaultState & fsmView.SelectState == fsmView.DefaultState)
                    DragStateBoxPosition(state.rect, state.name, StateMachineSetting.Instance.defaultAndSelectStyle);
                else if (state == fsmView.DefaultState & state.ID == fsmView.StateId)
                    DragStateBoxPosition(state.rect, state.name, StateMachineSetting.Instance.defaultAndRuntimeIndexStyle);
                else if (state == fsmView.DefaultState)
                    DragStateBoxPosition(state.rect, state.name, StateMachineSetting.Instance.stateInDefaultStyle);
                else if (fsmView.StateId == state.ID)
                    DragStateBoxPosition(state.rect, state.name, StateMachineSetting.Instance.indexInRuntimeStyle);
                else if (state == fsmView.SelectState)
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
            for (int i = 0; i < fsmView.SelectStates.Count; i++)
            {
                var state = fsmView.States[fsmView.SelectStates[i]];
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
                    foreach (State state in fsmView.States)
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
                    if (fsmView.SelectState != null)
                        DragStateBoxPosition(fsmView.SelectState.rect, fsmView.SelectState.name, StateMachineSetting.Instance.selectStateStyle);
                    break;
                case SelectMode.selectState:
                    GUI.Box(FromToRect(selectionStartPosition, mousePosition), "", "SelectionRect");
                    SelectStatesInRect(FromToRect(selectionStartPosition, mousePosition));
                    break;
            }
        }

        private void SelectStatesInRect(Rect r)
        {
            for (int i = 0; i < fsmView.States.Length; i++)
            {
                Rect rect = fsmView.States[i].rect;
                if (rect.xMax < r.x || rect.x > r.xMax || rect.yMax < r.y || rect.y > r.yMax)
                {
                    fsmView.SelectStates.Remove(fsmView.States[i].ID);
                    continue;
                }
                if (!fsmView.SelectStates.Contains(fsmView.States[i].ID))
                {
                    fsmView.SelectStates.Add(fsmView.States[i].ID);
                }
                DragStateBoxPosition(fsmView.States[i].rect, fsmView.States[i].name, StateMachineSetting.Instance.selectStateStyle);
            }
        }

        private Rect FromToRect(Vector2 start, Vector2 end)
        {
            Rect rect = new Rect(start.x, start.y, end.x - start.x, end.y - start.y);
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
            if (state.rect.Contains(mousePosition) | t.nextState.rect.Contains(mousePosition))
                return;
            if (currentType == EventType.MouseDown)
            {
                bool offset = state.ID > t.nextState.ID;
                Vector3 start = state.rect.center;
                Vector3 end = t.nextState.rect.center;
                Vector3 cross = Vector3.Cross((start - end).normalized, Vector3.forward);
                if (offset)
                {
                    start += cross * 6;
                    end += cross * 6;
                }
                if (HandleUtility.DistanceToLine(start, end) < 8f)//返回到线的距离
                {
                    selectTransition = t;
                    fsmView.SelectState = state;
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
                    foreach (var s in fsmView.States)
                    {
                        if (state != s & s.rect.Contains(mousePosition))
                        {
                            foreach (var t in state.transitions)
                            {
                                if (t.nextState == s)// 如果拖动的线包含在自身状态盒矩形内,则不添加连接线
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
                    fsmView.DefaultState = state;
                });
                menu.AddSeparator("");
                menu.AddItem(new GUIContent(BlueprintGUILayout.Instance.Language["Replication state"]), false, () =>
                {
                    fsmView.SelectState = state;
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
            foreach (var state in fsmView.States)
            {
                for (int n = 0; n < state.transitions.Length; n++)
                {
                    if (state.transitions[n].nextState == null)
                        continue;
                    if (fsmView.SelectStates.Contains(state.transitions[n].nextState.ID))
                        ArrayExtend.RemoveAt(ref state.transitions, n);
                }
            }
            var ids = new List<int>();
            foreach (var i in fsmView.SelectStates)
                ids.Add(fsmView.States[i].ID);
            while (ids.Count > 0)
            {
                for (int i = 0; i < fsmView.States.Length; i++)
                {
                    if (fsmView.States[i].ID == ids[0])
                    {
                        fsmView.States = ArrayExtend.RemoveAt(fsmView.States, i);
                        EditorUtility.SetDirty((Object)fsmView);
                        break;
                    }
                }
                ids.RemoveAt(0);
            }
            fsmView.UpdateStates();
            fsmView.SelectStates.Clear();
            selectTransition = null;
        }

        /// <summary>
        /// 右键打开窗口菜单
        /// </summary>

        protected void OpenWindowContextMenu()
        {
            if (fsmView == null)
                return;

            if (currentType == EventType.MouseDown)
            {
                if (currentEvent.button == 1)
                {
                    foreach (State state in fsmView.States)
                    {
                        if (state.rect.Contains(currentEvent.mousePosition))
                            return;
                    }
                    var menu = new GenericMenu();
                    menu.AddItem(new GUIContent(BlueprintGUILayout.Instance.Language["Create state"]), false, () =>
                    {
                        State.CreateStateInstance(fsmView as IStateMachine, fsmView, BlueprintGUILayout.Instance.Language["New state"] + fsmView.States.Length, mousePosition);
                    });
                    if (fsmView.SelectState != null)
                    {
                        menu.AddItem(new GUIContent(BlueprintGUILayout.Instance.Language["Paste Selection Status"]), false, (GenericMenu.MenuFunction)(() =>
                        {
                            List<State> states = new List<State>();
                            var seles = fsmView.SelectStates;
                            State s = Net.CloneHelper.DeepCopy<State>(fsmView.States[seles[0]]);
                            s.perID = s.ID;
                            s.ID = fsmView.States.Length;
                            s.rect.center = mousePosition;
                            fsmView.States = ArrayExtend.Add(fsmView.States, s);
                            states.Add(s);
                            Vector2 dis = fsmView.States[seles[0]].rect.center - mousePosition;
                            for (int i = 1; i < fsmView.SelectStates.Count; ++i)
                            {
                                State ss = Net.CloneHelper.DeepCopy<State>(fsmView.States[seles[i]]);
                                ss.perID = ss.ID;
                                ss.ID = fsmView.States.Length;
                                ss.rect.position -= dis;
                                fsmView.States = ArrayExtend.Add(fsmView.States, ss);
                                states.Add(ss);
                            }
                            foreach (var state in states)
                                foreach (var tran in state.transitions)
                                    foreach (var sta in states)
                                        if (tran.nextStateID == sta.perID)
                                            tran.nextStateID = sta.ID;
                            fsmView.UpdateStates();
                            List<int> list = new List<int>();
                            for (int i = 0; i < states.Count; ++i)
                                list.Add(states[i].ID);
                            fsmView.SelectStates = list;
                        }));
                        menu.AddItem(new GUIContent(BlueprintGUILayout.Instance.Language["Delete Selection State"]), false, delegate { DeletedState(); });
                    }
                    menu.AddSeparator("");
                    menu.AddItem(new GUIContent(BlueprintGUILayout.Instance.Language["Create and replace state machines"]), false, delegate
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
                        else if (manager.stateMachine != null)
                            Undo.DestroyObjectImmediate(manager.stateMachine.gameObject);
                        StateMachine machine = StateMachine.CreateStateMachineInstance();
                        Undo.RegisterCreatedObjectUndo(machine.gameObject, machine.name);
                        manager.stateMachine = machine;
                        machine.transform.SetParent(manager.transform);
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
                        StateMachine machine = StateMachine.CreateStateMachineInstance(BlueprintGUILayout.Instance.Language["New state machine"]);
                        Undo.RegisterCreatedObjectUndo(machine.gameObject, machine.name);
                        manager.stateMachine = machine;
                        machine.transform.SetParent(manager.transform);
                    });
                    menu.AddItem(new GUIContent(BlueprintGUILayout.Instance.Language["Delete state machine"]), false, () =>
                    {
                        if (fsmView == null)
                            return;
                        Undo.DestroyObjectImmediate(((MonoBehaviour)fsmView).gameObject);
                    });
                    menu.AddItem(new GUIContent(BlueprintGUILayout.Instance.Language["Delete state manager"]), false, () =>
                    {
                        if (fsmView == null)
                            return;
                        if (fsmView is IStateMachine machine)
                        {
                            if (machine.StateManager == null)
                                return;
                            Undo.DestroyObjectImmediate(((MonoBehaviour)fsmView).gameObject);
                            Undo.DestroyObjectImmediate(((MonoBehaviour)machine.StateManager).gameObject);
                        }
                    });
                    menu.ShowAsContext();
                    Event.current.Use();
                }
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
                        if (dragState & fsmView.SelectState != null)
                        {
                            foreach (var state in fsmView.SelectStates)
                            {
                                fsmView.States[state].rect.position += Event.current.delta;//拖到状态按钮
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