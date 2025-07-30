using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Game.UI.Foundation.Editor
{
    //统一控制鼠标样式
    public enum UXCursorType
    {
        None,
        Updown,
        Leftright
    }

    public class SceneViewCursorLogic : UIToolBarLogic<SceneViewCursorLogic>
    {
        Dictionary<UXCursorType, Texture2D> CursorTexturesDic;
        Texture2D CurCursorTexture;

        public void Init()
        {
            InitCursorTexture();
        }

        public override void Open()
        {
            SceneView.duringSceneGui += OnSceneGUI;
        }

        public override void Close()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
        }

        private void InitCursorTexture()
        {
            CursorTexturesDic = new Dictionary<UXCursorType, Texture2D>();
            CursorTexturesDic.Add(UXCursorType.Updown, ToolUtils.GetIcon("SplitResizeUpDown") as Texture2D);
            CursorTexturesDic.Add(UXCursorType.Leftright, ToolUtils.GetIcon("SplitResizeLeftRight") as Texture2D);
        }

        private Texture2D GetCursorTexture(UXCursorType cursorType)
        {
            if (CursorTexturesDic != null && CursorTexturesDic.ContainsKey(cursorType))
            {
                return CursorTexturesDic[cursorType];
            }
            else
            {
                return null;
            }
        }

        public void SetCursor(UXCursorType cursorType)
        {
            if (cursorType == UXCursorType.None)
            {
                CurCursorTexture = null;
                Utils.ClearCurrentViewCursor();
            }
            else
            {
                CurCursorTexture = GetCursorTexture(cursorType);
            }
        }
        
        private void OnSceneGUI(SceneView sceneView)
        {
            if (CurCursorTexture != null)
            {
                Utils.SetCursor(CurCursorTexture);
                HandleUtility.AddDefaultControl(0);
            }
        }
    }
}