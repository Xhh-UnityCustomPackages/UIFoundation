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


namespace Game.UI.Foundation.Editor
{
// Use [EditorToolbarElement(Identifier, EditorWindowType)] to register toolbar elements for use in ToolbarOverlay implementation.


    [EditorToolbarElement(id, typeof(SceneView))]
    class ToggleExample : EditorToolbarToggle
    {
        public const string id = "ExampleToolbar/Toggle";

        public ToggleExample()
        {
            text = "Toggle OFF";
            this.RegisterValueChangedCallback(Test);
        }

        void Test(ChangeEvent<bool> evt)
        {
            if (evt.newValue)
            {
                Debug.Log("ON");
                text = "Toggle ON";
            }
            else
            {
                Debug.Log("OFF");
                text = "Toggle OFF";
            }
        }
    }

    [EditorToolbarElement(id, typeof(SceneView))]
    class DropdownToggleExample : EditorToolbarDropdownToggle, IAccessContainerWindow
    {
        public const string id = "ExampleToolbar/DropdownToggle";

        // This property is specified by IAccessContainerWindow and is used to access the Overlay's EditorWindow.

        public EditorWindow containerWindow { get; set; }
        static int colorIndex = 0;
        static readonly Color[] colors = new Color[] { Color.red, Color.green, Color.cyan };

        public DropdownToggleExample()
        {
            text = "Color Bar";
            tooltip = "Display a color rectangle in the top left of the Scene view. Toggle on or off, and open the dropdown" +
                      "to change the color.";

            // When the dropdown is opened, ShowColorMenu is invoked and we can create a popup menu.

            dropdownClicked += ShowColorMenu;

            // Subscribe to the Scene view OnGUI callback so that we can draw our color swatch.

            SceneView.duringSceneGui += DrawColorSwatch;
        }

        void DrawColorSwatch(SceneView view)
        {
            // Test that this callback is for the Scene View that we're interested in, and also check if the toggle is on
            // or off (value).

            if (view != containerWindow || !value)
            {
                return;
            }

            Handles.BeginGUI();
            GUI.color = colors[colorIndex];
            GUI.DrawTexture(new Rect(8, 8, 120, 24), Texture2D.whiteTexture);
            GUI.color = Color.white;
            Handles.EndGUI();
        }

        // When the dropdown button is clicked, this method will create a popup menu at the mouse cursor position.

        void ShowColorMenu()
        {
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("Red"), colorIndex == 0, () => colorIndex = 0);
            menu.AddItem(new GUIContent("Green"), colorIndex == 1, () => colorIndex = 1);
            menu.AddItem(new GUIContent("Blue"), colorIndex == 2, () => colorIndex = 2);
            menu.ShowAsContext();
        }
    }
}