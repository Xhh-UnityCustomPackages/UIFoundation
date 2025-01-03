using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Game.UI.Foundation.Editor
{
    public class DesignImageFunctionWindow : EditorWindow
    {
        private static DesignImageFunctionWindow m_Window;

        public static void Show(Rect activatorRect, Vector2 size, DesignImage designImage)
        {
            var popup = EditorWindow.GetWindow<DesignImageFunctionWindow>();
            popup.hideFlags = HideFlags.DontSave;
            popup.ShowAsDropDown(activatorRect, size);
            popup.Init(designImage);
            m_Window = popup;
        }

        public static void CloseWindow()
        {
            if (m_Window == null) return;
            m_Window.Close();
        }


        private DesignImage m_DesignImage;

        public void Init(DesignImage designImage)
        {
            m_DesignImage = designImage;
        }

        private void OnGUI()
        {
            GUILayout.BeginVertical();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("顶层")) m_DesignImage.SetTop();
            if (GUILayout.Button("底层")) m_DesignImage.SetBottom();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("半透明")) m_DesignImage.SetTransparent();
            if (GUILayout.Button("不透明")) m_DesignImage.SetOpacity();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("剧中")) m_DesignImage.SetPositionCenter();
            if (GUILayout.Button("外面")) m_DesignImage.SetPositionOut();
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
        }
    }
}