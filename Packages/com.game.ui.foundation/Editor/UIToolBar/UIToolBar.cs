using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEditor.EditorTools;
using UnityEditor.Toolbars;
using UnityEditor.Overlays;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.Graphs;
using UnityEditor.SceneManagement;
using UnityEditor.Overlays;

namespace Game.UI.Foundation.Editor
{
    [Overlay(typeof(SceneView), "UIToorBar", true)]
    public class UIToolsOverlay : ToolbarOverlay
    {
        public UIToolsOverlay() : base(CreateDesignImage.id,
            CreateText.id,
            CreateImage.id,
            PrefabWidget.id,
            "UITools/Tools",
            LocationLineToolbar.id,
            Settings.id)
        {
        }

        public override void OnCreated()
        {
            this.displayedChanged += OnDisplayChanged;
            
            SceneViewCursorLogic.S.Init();
        }

       
        public override void OnWillBeDestroyed()
        {
            this.displayedChanged -= OnDisplayChanged;
        }

        void OnDisplayChanged(bool visible)
        {
            if (visible)
            {
                LocationLineLogic.S.Open();
                EdgeSnapLineLogic.S.Open();
                SceneViewCursorLogic.S.Open();
            }
            else
            {
                LocationLineLogic.S.Close();
                EdgeSnapLineLogic.S.Close();
                SceneViewCursorLogic.S.Close();
            }
        }
    }

    [EditorToolbarElement(id, typeof(SceneView))]
    class CreateDesignImage : EditorToolbarDropdownToggle, IAccessContainerWindow
    {
        public const string id = "UIToorBar/CreateDesignImage";
        public EditorWindow containerWindow { get; set; }

        private DesignImage m_DesignImage;

        public CreateDesignImage()
        {
            // text = "设计图";

            icon = UITools.Instance.IconDesignImage;
            tooltip = "创建设计图";

            FindDesignImage();
            value = m_DesignImage != null && m_DesignImage.gameObject.activeSelf;

            this.RegisterValueChangedCallback(delegate(ChangeEvent<bool> evt)
            {
                FindDesignImage();
                if (m_DesignImage == null)
                    CreateDesignImageGO();

                if (m_DesignImage != null)
                    m_DesignImage.gameObject.SetActive(evt.newValue);
            });

            RegisterCallback<AttachToPanelEvent>(OnAttachedToPanel);
            RegisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);
        }

        void OnAttachedToPanel(AttachToPanelEvent evt)
        {
            dropdownClicked += OnDropdownClicked;
        }

        void OnDetachFromPanel(DetachFromPanelEvent evt)
        {
            dropdownClicked -= OnDropdownClicked;
        }

        void OnDropdownClicked()
        {
            if (Application.isPlaying) return;
            FindDesignImage();
            if (m_DesignImage == null)
            {
                CreateDesignImageGO();
            }

            ShowWindow();
        }

        void ShowWindow()
        {
            if (m_DesignImage == null) return;

            if (!(containerWindow is SceneView view))
                return;

            var activatorRect = GUIUtility.GUIToScreenRect(this.worldBound);
            UnityEditor.PopupWindow.Show(worldBound, new DesignImageFunctionWindow(m_DesignImage));
        }

        void CreateDesignImageGO()
        {
            if (m_DesignImage != null) return;

            var stage = PrefabStageUtility.GetCurrentPrefabStage();
            if (stage == null)
                return;

            var root = CreateImage.GetCanvas();
            if (root == null)
                return;

            var go = new GameObject();
            m_DesignImage = go.AddComponent<DesignImage>();
            go.layer = LayerMask.NameToLayer("UI");
            go.transform.SetParent(root.transform);
            var rt = (go.transform as RectTransform);
            rt.SetStretch();
            EditorGUIUtility.PingObject(go);

            Undo.RegisterCreatedObjectUndo(go, "Create DesignImage");
        }

        void FindDesignImage()
        {
            var stage = PrefabStageUtility.GetCurrentPrefabStage();
            if (stage == null)
                return;

            var root = CreateImage.GetCanvas();
            if (root == null)
                return;

            m_DesignImage = null;
            m_DesignImage = root.gameObject.GetComponentInChildren<DesignImage>(true);
        }
    }

    [EditorToolbarElement(id, typeof(SceneView))]
    class CreateText : EditorToolbarButton //, IAccessContainerWindow
    {
        public const string id = "UIToorBar/CreateText";

        public CreateText()
        {
            // text = "Text";
            icon = UITools.Instance.IconCreateText;
            tooltip = "创建 Text";
            clicked += OnClick;
        }

        void OnClick()
        {
            if (Application.isPlaying) return;
            // 下一次点胶机到游戏物体得时候 就会创建子物体
            var root = CreateImage.GetCanvas();
            if (root == null)
                return;

            GameObject go;
            if (UIFoundationSettings.Instance.DefaultTextPrefab == null)
            {
                go = new GameObject("Text");
                var text = go.AddComponent<UnityEngine.UI.Text>();
                text.text = "Text";
                text.raycastTarget = false;
            }
            else
            {
                go = GameObject.Instantiate(UIFoundationSettings.Instance.DefaultTextPrefab);
                go.name = "Text";
            }

            go.layer = LayerMask.NameToLayer("UI");
            if (Selection.activeTransform != null)
                go.transform.SetParent(Selection.activeTransform);
            else
                go.transform.SetParent(root.transform);
            go.transform.localScale = Vector3.one;
            (go.transform as RectTransform).anchoredPosition3D = Vector3.zero;
            Selection.activeTransform = go.transform;
            EditorGUIUtility.PingObject(go);

            Undo.RegisterCreatedObjectUndo(go, "Create Text");
        }
    }

    [EditorToolbarElement(id, typeof(SceneView))]
    class CreateImage : EditorToolbarButton
    {
        public const string id = "UIToorBar/CreateImage";

        public CreateImage()
        {
            // text = "Image";
            icon = UITools.Instance.IconCreateImage;
            tooltip = "创建 Image";
            clicked += OnClick;
        }

        public static Transform GetCanvas()
        {
            var stage = PrefabStageUtility.GetCurrentPrefabStage();
            if (stage != null)
            {
                if (stage.prefabContentsRoot.transform.parent == null) return null;
                var canvas = stage.prefabContentsRoot.transform.parent.gameObject.GetComponentInChildren<Canvas>();
                if (canvas != null)
                    return canvas.transform.GetChild(0);
            }
            else
            {
                var canvas = GameObject.FindObjectOfType<Canvas>();
                if (canvas != null)
                    return canvas.transform;
            }

            return null;
        }

        void OnClick()
        {
            if (Application.isPlaying) return;

            var root = CreateImage.GetCanvas();
            if (root == null)
                return;

            var go = new GameObject("Image");
            var img = go.AddComponent<UnityEngine.UI.Image>();
            img.raycastTarget = false;
            go.layer = LayerMask.NameToLayer("UI");
            if (Selection.activeTransform != null)
                go.transform.SetParent(Selection.activeTransform);
            else
                go.transform.SetParent(root.transform);
            go.transform.localScale = Vector3.one;
            (go.transform as RectTransform).anchoredPosition3D = Vector3.zero;
            Selection.activeTransform = go.transform;
            EditorGUIUtility.PingObject(go);

            Undo.RegisterCreatedObjectUndo(go, "Create Image");
        }
    }

    [EditorToolbarElement(id, typeof(SceneView))]
    class PrefabWidget : EditorToolbarToggle
    {
        public const string id = "UIToorBar/PrefabWidget";

        public PrefabWidget()
        {
            // text = "Image";
            icon = UITools.Instance.IconPrefabWidget;
            tooltip = "组件库";
            // clicked += OnClick;
            this.RegisterValueChangedCallback(Create);
        }

        void OnClick()
        {
            WidgetLibraryWindow.OpenWindow();
        }

        void Create(ChangeEvent<bool> evt)
        {
            if (evt.newValue)
            {
                WidgetLibraryWindow.OpenWindow();
            }
            else
            {
                WidgetLibraryWindow.GetInstance().Close();
            }
        }
    }

    [EditorToolbarElement(id, typeof(SceneView))]
    class Settings : EditorToolbarButton
    {
        public const string id = "UIToorBar/Settings";

        public Settings()
        {
            // text = "Image";
            icon = UITools.Instance.IconSettings;
            tooltip = "设置";
            clicked += OnClick;
        }

        void OnClick()
        {
            SettingsService.OpenProjectSettings("Project/UI Foundation");
        }
    }

    [EditorToolbarElement(id, typeof(SceneView))]
    class LocationLineToolbar : EditorToolbarDropdown
    {
        public const string id = "UIToorBar/LocationLine";

        public LocationLineToolbar()
        {
            icon = UITools.Instance.IconReferenceLine;
            tooltip = "辅助线";
            ToolUtils.GetIcon("more");
            clicked += ShowDropdown;
        }

        void ShowDropdown()
        {
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("X&Y-水平&垂直"), false, () => { AddLocationLine(CreateLineType.Both); });
            menu.AddItem(new GUIContent("X-水平"), false, () => { AddLocationLine(CreateLineType.Horizon); });
            menu.AddItem(new GUIContent("Y-垂直"), false, () => { AddLocationLine(CreateLineType.Vertical); });
            menu.AddItem(new GUIContent("清空所有辅助线"), false, () => { ClearLocationLine(); });
            menu.ShowAsContext();
        }

        void AddLocationLine(CreateLineType type)
        {
            LocationLineLogic.S.CreateLocationLine(type);
        }

        void ClearLocationLine()
        {
            LocationLineLogic.S.Clear();
        }
    }

    [EditorToolbarElement(id, typeof(SceneView))]
    class MoreDropdown : EditorToolbarDropdown
    {
        public const string id = "UIToorBar/MoreDropdown";

        public MoreDropdown()
        {
            icon = UITools.Instance.IconMore;
            ToolUtils.GetIcon("more");
            clicked += ShowDropdown;
        }

        void ShowDropdown()
        {
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("Settings"), false, () => { OpenSetting(); });
            menu.ShowAsContext();
        }

        void OpenSetting()
        {
            SettingsService.OpenProjectSettings("Project/UI Foundation");
        }
    }
}