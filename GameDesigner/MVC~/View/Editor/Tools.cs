#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace MVC.View
{
    public class Tools
    {
        [MenuItem("CONTEXT/FieldCollection/SetFieldValueInChilds")]
        private static void SetFieldValueInChilds(MenuCommand menuCommand)
        {
            var fc = menuCommand.context as FieldCollection;
            for (int i = 0; i < fc.fields.Count; i++)
            {
                var fieldTarget = fc.fields[i].target;
                Transform transform = null;
                if (fieldTarget is GameObject gameObject)
                    transform = gameObject.transform;
                else if (fieldTarget is Component component)
                    transform = component.transform;
                if (transform == null)
                    continue;
                var path = transform.name;
                while (transform.parent != null)
                {
                    transform = transform.parent;
                    if (transform.GetComponent<FieldCollection>() != null)
                        break;
                    path = transform.name + "/" + path;
                }
                transform = fc.transform.Find(path);
                fc.fields[i].target = transform.GetComponent(fieldTarget.GetType());
            }
            EditorUtility.SetDirty(fc);
        }
    }
}
#endif