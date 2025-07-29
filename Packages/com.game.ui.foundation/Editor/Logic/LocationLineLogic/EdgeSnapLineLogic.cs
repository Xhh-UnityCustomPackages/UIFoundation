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
    public class EdgeSnapLineLogic : UIToolBarLogic<LocationLineLogic>
    {
        private GameObject m_SelectedObject;
        private List<Rect> m_Rects;

        private struct Rect
        {
            /// <summary>
            /// 长度为6，上中下左中右
            /// </summary>
            public float[] pos;

            public Rect(RectTransform trans)
            {
                pos = new float[6];
                pos[0] = (float)Math.Round((double)trans.GetTopWorldPosition(), 1);
                pos[2] = (float)Math.Round((double)trans.GetBottomWorldPosition(), 1);
                pos[3] = (float)Math.Round((double)trans.GetLeftWorldPosition(), 1);
                pos[5] = (float)Math.Round((double)trans.GetRightWorldPosition(), 1);
                pos[4] = (float)Math.Round((double)trans.position.x, 1);
                pos[1] = (float)Math.Round((double)trans.position.y, 1);
            }
        }

        public override void Open()
        {
            m_Rects = new List<Rect>();
            
            Selection.selectionChanged += ResetAll;
            EditorApplication.update += ListenMoving;
        }

        public override void Close()
        {
            Selection.selectionChanged -= ResetAll;
            EditorApplication.update -= ListenMoving;
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
            
            //收集全部RectTransform的边界
            m_Rects.Clear();
            RectTransform[] allObjects;
            var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
            if (prefabStage != null)
            {
                allObjects = prefabStage.prefabContentsRoot.GetComponentsInChildren<RectTransform>();
            }
            else
            {
                allObjects = UnityEngine.Object.FindObjectsOfType<RectTransform>();
            }
            foreach (RectTransform item in allObjects)
            {
                if (ObjectFit(item.gameObject) && item.gameObject != m_SelectedObject && !item.IsChildOf(m_SelectedObject.transform))
                {
                    m_Rects.Add(new Rect(item));
                }
            }
        }
        
        private void ListenMoving()
        {
            if (m_SelectedObject != null && m_SelectedObject.GetComponent<RectTransform>().position != SnapLogic.ObjFinalPos)
            {
                SnapLogic.SnapEdgeDisHoriz = SnapLogic.SnapEdgeDisVert = Mathf.Infinity;
                FindEdges(SnapLogic.SnapWorldDistance);
            }
        }

        /// <summary>
        /// 核心逻辑
        /// </summary>
        /// <param name="eps">eps=0表示吸附最终位置（需要画提示线）</param>
        private void FindEdges(float eps)
        {
             Rect objRect = new Rect(m_SelectedObject.GetComponent<RectTransform>());

            foreach (Rect rect in m_Rects)
            {
                if (eps != 0)
                {
                    float dis = Mathf.Infinity;
                    for (int i = 0; i < 3; i++)
                    {
                        for (int j = 0; j < 3; j++)
                        {
                            if (Mathf.Abs(dis) > Mathf.Abs(rect.pos[i] - objRect.pos[j]))
                            {
                                dis = rect.pos[i] - objRect.pos[j];
                            }
                        }
                    }
                    if (Mathf.Abs(dis) < eps && Mathf.Abs(dis) < Mathf.Abs(SnapLogic.SnapEdgeDisVert))
                    {
                        SnapLogic.SnapEdgeDisVert = dis;
                    }
                    dis = Mathf.Infinity;
                    for (int i = 3; i < 6; i++)
                    {
                        for (int j = 3; j < 6; j++)
                        {
                            if (Mathf.Abs(dis) > Mathf.Abs(rect.pos[i] - objRect.pos[j]))
                            {
                                dis = rect.pos[i] - objRect.pos[j];
                            }
                        }
                    }
                    if (Mathf.Abs(dis) < eps && Mathf.Abs(dis) < Mathf.Abs(SnapLogic.SnapEdgeDisHoriz))
                    {
                        SnapLogic.SnapEdgeDisHoriz = dis;
                    }
                }
                else
                {
                    float minX = Math.Min(rect.pos[3], objRect.pos[3]);
                    float maxX = Math.Max(rect.pos[5], objRect.pos[5]);
                    for (int i = 0; i < 3; i++)
                    {
                        for (int j = 0; j < 3; j++)
                        {
                            if (rect.pos[i] == objRect.pos[j])
                            {
                                // m_VisualManager.AddHorizLine(minX, maxX, rect.pos[i], false);
                            }
                        }
                    }
                    float minY = Math.Min(rect.pos[2], objRect.pos[2]);
                    float maxY = Math.Max(rect.pos[0], objRect.pos[0]);
                    for (int i = 3; i < 6; i++)
                    {
                        for (int j = 3; j < 6; j++)
                        {
                            if (rect.pos[i] == objRect.pos[j])
                            {
                                // m_VisualManager.AddVertLine(rect.pos[i], minY, maxY, false);
                            }
                        }
                    }
                }
            }
        }

        public static bool ObjectFit(GameObject obj)
        {
            if (obj == null) return false;
            //Graphic[] components = obj.GetComponents<Graphic>();
            //if (components == null || components.Length == 0) return false;
            return obj.activeInHierarchy && obj.GetComponent<RectTransform>() != null;
        }
    }
}
