using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Game.UI.Foundation.Editor
{
    public class DesignImageFunctionWindow : PopupWindowContent
    {
        private DesignImage m_DesignImage;

        public DesignImageFunctionWindow(DesignImage designImage)
        {
            m_DesignImage = designImage;
        }

        public override Vector2 GetWindowSize()
        {
            return new Vector2(200, 90);
        }

        public override void OnGUI(Rect rect)
        {
            GUILayout.BeginVertical();
            GUILayout.Label("设计图工具");
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