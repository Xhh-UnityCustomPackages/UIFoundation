using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Game.UI.Foundation.Editor
{
    /// <summary>
    /// Widget: 游戏中会重复使用的Prefab 如: 一级页签|二级页签|确认按钮 等
    /// 提供UI Style 方便统一进行风格迭代和更换
    /// </summary>
    public class WidgetRepositoryWindow : EditorWindow
    {
        private static WidgetRepositoryWindow m_window;

        [MenuItem("Tools/UI/WidgetLibrary", false, 51)]
        public static void OpenWindow()
        {
            int width = 1272 + 13 + 12;
            int height = 636;
            m_window = GetWindow<WidgetRepositoryWindow>();
            m_window.minSize = new Vector2(width, height);
            // m_window.titleContent.text = EditorLocalization.GetLocalization(EditorLocalizationStorage.Def_组件库);
            // m_window.titleContent.image = ToolUtils.GetIcon("component_w");
            // UXToolAnalysis.SendUXToolLog(UXToolAnalysisLog.WidgetLibrary);
        }

        public static void CloseWindow()
        {
            if (m_window == null) return;
            m_window.Close();
        }

        static WidgetRepositoryWindow()
        {
            EditorApplication.playModeStateChanged += (obj) =>
            {
                if (HasOpenInstances<WidgetRepositoryWindow>())
                    m_window = GetWindow<WidgetRepositoryWindow>();
                if (!EditorApplication.isPlaying && !EditorApplication.isPlayingOrWillChangePlaymode)
                {
                    // if (m_window)
                    //     m_window.RefreshWindow();
                }
            };
        }


        [UnityEditor.Callbacks.DidReloadScripts(0)]
        private static void OnScriptReload()
        {
            if (HasOpenInstances<WidgetRepositoryWindow>())
                m_window = GetWindow<WidgetRepositoryWindow>();
        }

        public static WidgetRepositoryWindow GetInstance()
        {
            return m_window;
        }

        private void OnEnable()
        {
            DragAndDrop.AddDropHandler(OnHierarchyGUI);
        }

        private void OnDisable()
        {
            DragAndDrop.RemoveDropHandler(OnHierarchyGUI);
        }


        private DragAndDropVisualMode OnHierarchyGUI(int dropTargetInstanceID, HierarchyDropFlags dropMode, Transform parentForDraggedObjects, bool perform)
        {
            return DragAndDrop.visualMode;
        }
    }
}