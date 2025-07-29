using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.IO;
using UnityEngine.Serialization;
using UnityEngine.UIElements;

namespace Game.UI.Foundation.Editor
{
    public class UIFoundationSettings : ScriptableObject
    {
        public GameObject DefaultTextPrefab;
        public Object WidgetLibrary;

        internal const string k_SettingsPath = "ProjectSettings/UIFoundationSettings.asset";

        #region Singleton

        private static UIFoundationSettings s_instance;

        public static UIFoundationSettings Instance
        {
            get
            {
                if (!s_instance) CreateAndLoad();
                return s_instance;
            }
        }

        private static void CreateAndLoad()
        {
            // Assert.IsTrue(!s_instance);

            // Load
            var files = InternalEditorUtility.LoadSerializedFileAndForget(k_SettingsPath);
            if (files.Length != 0)
            {
                s_instance = (UIFoundationSettings)files[0];
            }

            // Create
            if (!s_instance)
            {
                s_instance = CreateInstance<UIFoundationSettings>();
                EditorUtility.SetDirty(s_instance);
                s_instance.Save();
            }
        }

        #endregion


        public void Save()
        {
            if (!s_instance)
            {
                Debug.LogError($"Cannot save {nameof(UIFoundationSettings)}: no instance!");
                return;
            }

            InternalEditorUtility.SaveToSerializedFileAndForget(new[] { s_instance }, k_SettingsPath, true);
        }


        internal static SerializedObject GetSerializedSettings()
        {
            return new SerializedObject(Instance);
        }
    }


    public class UIFoundationSettingsProvider : SettingsProvider
    {
        private SerializedObject m_Settings;

        public UIFoundationSettingsProvider(string path, SettingsScope scopes) : base(path, scopes)
        {
        }

        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            m_Settings = UIFoundationSettings.GetSerializedSettings();
        }

        public override void OnGUI(string searchContext)
        {
            EditorGUILayout.LabelField("按住「Alt」键将图片拖放到 Hierarchy窗口 内的 RectTransform 下可直接创建 Image ");
            EditorGUILayout.LabelField("组件库中的预制体,如果AssetLabel包含Prefab的话,拖出就会使用预制体");
            EditorGUILayout.LabelField("对齐按钮： 分为「上」、「中」、「下」、「左」、「中」、「右」共6个按钮。");
            EditorGUILayout.LabelField("\t当选中单个UI节点，则将该节点与父节点对齐。当选中多个UI节点，则将其他节点与第一个节点对齐。当按住Shift键再点击时，会同时设置节点的轴点。");
                 
            EditorGUILayout.Space(5);
            // Use IMGUI to display UI:
            EditorGUILayout.PropertyField(m_Settings.FindProperty("DefaultTextPrefab"));
            EditorGUILayout.PropertyField(m_Settings.FindProperty("WidgetLibrary"));

            if (m_Settings.hasModifiedProperties)
            {
                m_Settings.ApplyModifiedProperties();
                UIFoundationSettings.Instance.Save();
            }
        }

        public static bool IsSettingsAvailable()
        {
            return File.Exists(UIFoundationSettings.k_SettingsPath);
        }

        [SettingsProvider]
        public static SettingsProvider GetProjectSettingsProvider()
        {
            var provider = new UIFoundationSettingsProvider("Project/UI Foundation", SettingsScope.Project);
            return provider;
        }
    }
}