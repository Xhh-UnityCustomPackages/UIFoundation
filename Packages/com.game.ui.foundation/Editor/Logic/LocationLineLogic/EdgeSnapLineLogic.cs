using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Game.UI.Foundation.Editor
{
    /// <summary>
    /// 处理边缘吸附
    /// </summary>
    public class EdgeSnapLineLogic : UIToolBarLogic<EdgeSnapLineLogic>
    {
        private GameObject m_SelectedObject;
        
        public override void Open()
        {

            EditorApplication.hierarchyChanged += ResetAll;
            Selection.selectionChanged += ResetAll;
            EditorApplication.update += ListenMoving;
        }

        public override void Close()
        {
            Selection.selectionChanged -= ResetAll;
            EditorApplication.update -= ListenMoving;
            EditorApplication.hierarchyChanged -= ResetAll;
        }

        private void ResetAll()
        {
            if (Selection.gameObjects.Length == 1 && ObjectFit(Selection.activeGameObject))
            {
                m_SelectedObject = Selection.activeGameObject;
                m_SelectedObject.transform.hasChanged = false;
            }
            else
            {
                m_SelectedObject = null;
                return;
            }
            
            SnapLogic.ObjFinalPos = m_SelectedObject.GetComponent<RectTransform>().position;
        }

        private void ListenMoving()
        {
            if (m_SelectedObject != null && m_SelectedObject.GetComponent<RectTransform>().position != SnapLogic.ObjFinalPos&& LocationLineLogic.S.EnableSnap)
            {
                SnapLogic.SnapEdgeDisHoriz = SnapLogic.SnapEdgeDisVert = Mathf.Infinity;
                SnapLogic.SnapIntervalDisHoriz = SnapLogic.SnapIntervalDisVert = Mathf.Infinity;
            }
        }
        

        public static bool ObjectFit(GameObject obj)
        {
            if (obj == null) return false;
            return obj.activeInHierarchy && obj.GetComponent<RectTransform>() != null;
        }
    }
}