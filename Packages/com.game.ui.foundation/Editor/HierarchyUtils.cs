using UnityEditor;
using UnityEngine;

namespace Game.UI.Foundation.Editor
{
    //如果组件库的物体拖到了Hierarchy上,就解组UI
    public class HierarchyUtils
    {
        [InitializeOnLoadMethod]
        static void Init()
        {
            // list.Clear();
            // Templist.Clear();
            EditorApplication.hierarchyChanged += OnHierarchyChanged;
        }

        private static void OnHierarchyChanged()
        {
            foreach (GameObject go in Selection.gameObjects)
            {
                if (PrefabUtility.IsPartOfAnyPrefab(go))
                {
                }
            }

        }
    }
}