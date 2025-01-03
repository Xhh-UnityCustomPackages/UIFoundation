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
// All Overlays must be tagged with the OverlayAttribute
    [Overlay(typeof(SceneView), "UI Toolbar")]
// IconAttribute provides a way to define an icon for when an Overlay is in collapsed form. If not provided, the name initials are used.
    [Icon("Assets/unity.png")]
// Toolbar Overlays must inherit `ToolbarOverlay` and implement a parameter-less constructor. The contents of a toolbar are populated with string IDs, which are passed to the base constructor. IDs are defined by EditorToolbarElementAttribute.
    public class UIToorBar : ToolbarOverlay
    {
        // ToolbarOverlay implements a parameterless constructor, passing the EditorToolbarElementAttribute ID.
        // This is the only code required to implement a toolbar Overlay. Unlike panel Overlays, the contents are defined
        // as standalone pieces that will be collected to form a strip of elements.

        UIToorBar() : base(
            CreateDesignImage.id,
            CreateText.id,
            CreateImage.id,
            PrefabWidget.id,
            Settings.id
            // MoreDropdown.id
            // DropdownToggleExample.id
        )
        {
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
            icon = AssetDatabase.LoadAssetAtPath<Texture2D>("Packages/com.game.ui.foundation/Editor/Res/Icon/createDesignImage.png");
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
            icon = AssetDatabase.LoadAssetAtPath<Texture2D>("Packages/com.game.ui.foundation/Editor/Res/Icon/createText.png");
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
            }

            go.layer = LayerMask.NameToLayer("UI");
            go.transform.SetParent(root.transform);
            (go.transform as RectTransform).anchoredPosition3D = Vector3.zero;
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
            icon = AssetDatabase.LoadAssetAtPath<Texture2D>("Packages/com.game.ui.foundation/Editor/Res/Icon/createImage.png");
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
            go.transform.SetParent(root.transform);
            (go.transform as RectTransform).anchoredPosition3D = Vector3.zero;
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
            icon = AssetDatabase.LoadAssetAtPath<Texture2D>("Packages/com.game.ui.foundation/Editor/Res/Icon/prefabRepository.png");
            tooltip = "组件库";
            // clicked += OnClick;
            this.RegisterValueChangedCallback(Create);
        }

        void OnClick()
        {
            WidgetRepositoryWindow.OpenWindow();
        }

        void Create(ChangeEvent<bool> evt)
        {
            if (evt.newValue)
            {
                WidgetRepositoryWindow.OpenWindow();
            }
            else
            {
                WidgetRepositoryWindow.CloseWindow();
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
            icon = AssetDatabase.LoadAssetAtPath<Texture2D>("Packages/com.game.ui.foundation/Editor/Res/Icon/setting.png");
            tooltip = "设置";
            clicked += OnClick;
        }

        void OnClick()
        {
            SettingsService.OpenProjectSettings("Project/UI Foundation");
        }
    }

    [EditorToolbarElement(id, typeof(SceneView))]
    class MoreDropdown : EditorToolbarDropdown
    {
        public const string id = "UIToorBar/MoreDropdown";

        public MoreDropdown()
        {
            icon = AssetDatabase.LoadAssetAtPath<Texture2D>("Packages/com.game.ui.foundation/Editor/Res/Icon/more.png");
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