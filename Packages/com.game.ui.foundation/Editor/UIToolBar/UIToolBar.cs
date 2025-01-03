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
            MoreDropdown.id
            // DropdownToggleExample.id
        )
        {
        }
    }


    [EditorToolbarElement(id, typeof(SceneView))]
    class CreateDesignImage : EditorToolbarButton //, IAccessContainerWindow
    {
        public const string id = "UIToorBar/CreateDesignImage";

        // IAccessContainerWindow provides a way for toolbar elements to access the `EditorWindow` in which they exist.
        // Here we use `containerWindow` to focus the camera on our newly instantiated objects after creation.
        //public EditorWindow containerWindow { get; set; }

        // Because this is a VisualElement, it is appropriate to place initialization logic in the constructor.
        // In this method you can also register to any additional events as required. In this example there is a tooltip, an icon, and an action.

        public CreateDesignImage()
        {
            // A toolbar element can be either text, icon, or a combination of the two. Keep in mind that if a toolbar is
            // docked horizontally the text will be clipped, so usually it's a good idea to specify an icon.

            // text = "设计图";
            icon = AssetDatabase.LoadAssetAtPath<Texture2D>("Packages/com.game.ui.foundation/Editor/Res/Icon/createDesignImage.png");
            tooltip = "创建设计图";
            clicked += OnClick;
        }

        // This method will be invoked when the `Create Cube` button is clicked.

        void OnClick()
        {
            var canvas = CreateImage.GetCanvas();
            if (canvas == null)
                return;

            var go = new GameObject();
            go.AddComponent<DesignImage>();
            go.layer = LayerMask.NameToLayer("UI");
            go.transform.SetParent(canvas.transform);
            var rt = (go.transform as RectTransform);
            rt.SetStretch();
            //TODO 还需要添加默认图片

            Undo.RegisterCreatedObjectUndo(go, "Create DesignImage");

            //if (containerWindow is SceneView view)
            //    view.FrameSelected();
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
            // 下一次点胶机到游戏物体得时候 就会创建子物体
            var canvas = CreateImage.GetCanvas();
            if (canvas == null)
                return;

            GameObject go;
            if (UIFoundationSettings.Instance.DefaultTextPrefab == null)
            {
                go = new GameObject("Text");
                var text = go.AddComponent<UnityEngine.UI.Text>();
            }
            else
            {
                go = GameObject.Instantiate(UIFoundationSettings.Instance.DefaultTextPrefab);
            }


            go.layer = LayerMask.NameToLayer("UI");
            go.transform.SetParent(canvas.transform);

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
            var canvas = CreateImage.GetCanvas();
            if (canvas == null)
                return;

            var go = new GameObject("Image");
            var img = go.AddComponent<UnityEngine.UI.Image>();
            img.raycastTarget = false;
            go.layer = LayerMask.NameToLayer("UI");
            go.transform.SetParent(canvas.transform);

            Undo.RegisterCreatedObjectUndo(go, "Create Image");
        }

        void Create(ChangeEvent<bool> evt)
        {
            if (evt.newValue)
            {
                Debug.Log("ON");
            }
            else
            {
                Debug.Log("OFF");
            }
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
    class MoreDropdown : EditorToolbarDropdown
    {
        public const string id = "UIToorBar/MoreDropdown";

        static string dropChoice = null;

        public MoreDropdown()
        {
            // text = "Axis";
            icon = AssetDatabase.LoadAssetAtPath<Texture2D>("Packages/com.game.ui.foundation/Editor/Res/Icon/more.png");
            clicked += ShowDropdown;
        }

        void ShowDropdown()
        {
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("1"), dropChoice == "X", () => { dropChoice = "X"; });
            menu.AddItem(new GUIContent("2"), dropChoice == "Y", () => { dropChoice = "Y"; });
            menu.AddItem(new GUIContent("3"), dropChoice == "Z", () => { dropChoice = "Z"; });
            menu.ShowAsContext();
        }
    }
}