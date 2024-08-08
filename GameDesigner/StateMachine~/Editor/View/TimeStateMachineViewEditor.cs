#if UNITY_EDITOR
using UnityEditor;

namespace GameDesigner
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(TimeStateMachineView))]
    public class TimeStateMachineViewEditor : StateMachineViewEditor
    {
    }
}
#endif