using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

namespace Game.UI.Foundation.Editor
{
    public class SceneViewUtils
    {
        static private List<Action<SceneView>> list = new List<Action<SceneView>>();
        static private List<Action<SceneView>> Templist = new List<Action<SceneView>>();

        [InitializeOnLoadMethod]
        static void Init()
        {
            list.Clear();
            Templist.Clear();
            SceneView.duringSceneGui += OnSceneGUI;
        }

        static public void AddDelegate(Action<SceneView> method)
        {
            Templist.Add(method);
        }

        static public void RemoveDelegate(Action<SceneView> method)
        {
            var index = Templist.FindIndex(i => i == method);
            if (index >= 0)
            {
                Templist.RemoveAt(index);
            }
        }

        static public void OnSceneGUI(SceneView sceneView)
        {
            foreach (Action<SceneView> method in list)
            {
                method.Invoke(sceneView);
            }

            list.Clear();
            foreach (Action<SceneView> method in Templist)
            {
                list.Add(method);
            }
        }

        static public void ClearDelegate()
        {
            Templist.Clear();
        }
    }
}