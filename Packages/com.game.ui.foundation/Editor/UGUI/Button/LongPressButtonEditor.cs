using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEditor.UI;

namespace Game.UI.Foundation.Editor
{
    [CustomEditor(typeof(LongPressButton), true)]
    [CanEditMultipleObjects]
    public class LongPressButtonEditor : ButtonEditor
    {
        SerializedProperty m_OnLongPressProperty;
        SerializedProperty m_CheckTimeProperty;

        protected override void OnEnable()
        {
            base.OnEnable();
            m_OnLongPressProperty = serializedObject.FindProperty("m_OnLongPress");
            m_CheckTimeProperty = serializedObject.FindProperty("m_CheckTime");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            EditorGUILayout.PropertyField(m_CheckTimeProperty);
            EditorGUILayout.PropertyField(m_OnLongPressProperty);
            serializedObject.ApplyModifiedProperties();
        }
    }
}