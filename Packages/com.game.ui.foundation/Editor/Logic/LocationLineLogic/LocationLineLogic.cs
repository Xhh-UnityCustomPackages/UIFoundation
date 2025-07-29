using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UIElements;

namespace Game.UI.Foundation.Editor
{
    public enum CreateLineType
    {
        Both,
        Vertical,
        Horizon
    }
    
    [Serializable]
    public class LocationLineData
    {
        public int Id;
        public bool Horizontal;
        public float Pos;
    }

    public class LocationLineLogic : UIToolBarLogic<LocationLineLogic>
    {
        public static float sceneviewOffset;

        //所有辅助线对象 应该和上面的数据是一致的
        private List<LocationLine> m_LinesList;
        public int LastLineId => m_LinesList.Count > 0 ? m_LinesList.Max(data => data.id) : 0;

        private GameObject m_SelectedObject;
        public bool EnableSnap = false;
        private bool OnDrag = false;

        public override void Open()
        {
            sceneviewOffset = 0;
            m_LinesList = new();

            SceneView.duringSceneGui += OnSceneGUI;
            EditorApplication.update += UpdateLinesScreenViewPos; //辅助线根据放大缩小更新位置
            EditorApplication.update += SnapToLocationLine;
            Selection.selectionChanged += OnSelectionChanged;
            EditorApplication.update += SnapToFinalPos;

            OnSelectionChanged();
        }

        public override void Close()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
            EditorApplication.update -= UpdateLinesScreenViewPos;
            EditorApplication.update -= SnapToFinalPos;
            EditorApplication.update -= SnapToLocationLine;
            Selection.selectionChanged -= OnSelectionChanged;
            
            Clear();
        }

        public void Clear()
        {
            if (m_LinesList != null)
            {
                foreach (var line in m_LinesList)
                {
                    if (SceneView.lastActiveSceneView.rootVisualElement.Contains(line))
                    {
                        SceneView.lastActiveSceneView.rootVisualElement.Remove(line);
                    }
                }

                m_LinesList.Clear();
            }
        }

        private void OnSceneGUI(SceneView view)
        {
            if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
            {
                OnDrag = true;
            }

            if (Event.current.type == EventType.MouseUp && Event.current.button == 0)
            {
                OnDrag = false;
            }

            if (Event.current.type == EventType.MouseDrag && OnDrag)
            {
                EnableSnap = true;
            }
            else
            {
                EnableSnap = false;
            }
        }

        public void CreateLocationLine(CreateLineType createType)
        {
            SceneView sceneView = SceneView.lastActiveSceneView;
            Vector3 worldPostion =
                    sceneView.camera.ScreenToWorldPoint(new(sceneView.camera.pixelWidth / 2, (sceneView.camera.pixelHeight - 40) / 2, 0));

            int curId = LastLineId + 1;
            LocationLineData horzLineData = null;
            LocationLineData vertLineData = null;
            LocationLine horzLine = null;
            LocationLine vertLine = null;

            if (createType == CreateLineType.Both || createType == CreateLineType.Horizon)
            {
                horzLineData = new LocationLineData()
                {
                    Id = curId,
                    Horizontal = true,
                    Pos = worldPostion.y
                };
                horzLine = new HorizontalLocationLine
                {
                    id = horzLineData.Id,
                    worldPostion = new Vector3(0, horzLineData.Pos, 0)
                };
            }

            if (createType == CreateLineType.Both || createType == CreateLineType.Vertical)
            {
                vertLineData = new LocationLineData()
                {
                    Id = curId + 1,
                    Horizontal = false,
                    Pos = worldPostion.x,
                };
                vertLine = new VerticalLocationLine
                {
                    id = vertLineData.Id,
                    worldPostion = new Vector3(vertLineData.Pos, 0, 0)
                };
            }

            if (createType == CreateLineType.Both)
            {
                PlaceLinesToSceneView(new List<LocationLine> { horzLine, vertLine });
            }
            else if (createType == CreateLineType.Horizon)
            {
                PlaceLinesToSceneView(new List<LocationLine> { horzLine });
            }
            else if (createType == CreateLineType.Vertical)
            {
                PlaceLinesToSceneView(new List<LocationLine> { vertLine });
            }
        }

        private void PlaceLinesToSceneView(List<LocationLine> lines)
        {
            SceneView sceneView = SceneView.lastActiveSceneView;
            if (sceneView == null) return;

            VisualElement firstChild = sceneView.rootVisualElement.Children().First();
            foreach (LocationLine line in lines)
            {
                sceneView.rootVisualElement.Add(line);
                line.PlaceInFront(firstChild);
                m_LinesList.Add(line);
            }
        }

        void UpdateLinesScreenViewPos()
        {
            SceneView sceneView = SceneView.lastActiveSceneView;
            if (sceneView == null) return;

            var h = sceneView.camera.pixelHeight;
            foreach (LocationLine line in m_LinesList)
            {
                line.UpdateLineScreenViewPos(sceneView);
                if (line.style.bottom.value.value > h - 26)
                {
                    line.style.visibility = Visibility.Hidden;
                }
                else
                {
                    line.style.visibility = Visibility.Visible;
                }
            }
        }

        private void OnSelectionChanged()
        {
            if (Selection.gameObjects.Length == 1 && EdgeSnapLineLogic.ObjectFit(Selection.activeGameObject))
            {
                m_SelectedObject = Selection.activeGameObject;
            }
            else
            {
                m_SelectedObject = null;
            }
        }

        /// <summary>
        /// 将物体吸附到最终位置
        /// </summary>
        void SnapToFinalPos()
        {
            if (m_SelectedObject != null && m_SelectedObject.GetComponent<RectTransform>().position != SnapLogic.ObjFinalPos && EnableSnap)
            {
                RectTransform rectTransform = m_SelectedObject.GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    Vector3 vec = rectTransform.transform.position;
                    if (!float.IsPositiveInfinity(SnapLogic.SnapLineDisHoriz))
                    {
                        if (Math.Abs(SnapLogic.SnapLineDisHoriz) <= Math.Abs(SnapLogic.SnapEdgeDisHoriz) &&
                            Math.Abs(SnapLogic.SnapLineDisHoriz) <= Math.Abs(SnapLogic.SnapIntervalDisHoriz))
                        {
                            vec.x += SnapLogic.SnapLineDisHoriz;
                        }
                    }

                    if (!float.IsPositiveInfinity(SnapLogic.SnapLineDisVert))
                    {
                        if (Math.Abs(SnapLogic.SnapLineDisVert) <= Math.Abs(SnapLogic.SnapEdgeDisVert) &&
                            Math.Abs(SnapLogic.SnapLineDisVert) <= Math.Abs(SnapLogic.SnapIntervalDisVert))
                        {
                            vec.y += SnapLogic.SnapLineDisVert;
                        }
                    }

                    rectTransform.transform.position = vec;
                }
            }
        }

        /// <summary>
        ///辅助线吸附逻辑
        /// </summary>
        private void SnapToLocationLine()
        {
            if (m_SelectedObject != null && m_SelectedObject.GetComponent<RectTransform>().position != SnapLogic.ObjFinalPos && EnableSnap)
            {
                SnapLogic.SnapLineDisVert = SnapLogic.SnapLineDisHoriz = Mathf.Infinity;
                RectTransform rectTransform = m_SelectedObject.GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    float leftEdgePos = rectTransform.GetLeftWorldPosition();
                    float rightEdgePos = rectTransform.GetRightWorldPosition();
                    float bottomEdgePos = rectTransform.GetBottomWorldPosition();
                    float topEdgePos = rectTransform.GetTopWorldPosition();

                    Vector2 distance = new Vector2(Mathf.Infinity, Mathf.Infinity);

                    foreach (var line in m_LinesList)
                    {
                        //查找竖直方向最近的辅助线距离
                        if (line.direction == LocationLineDirection.Vertical)
                        {
                            float dis1 = line.worldPostion.x - leftEdgePos;
                            float dis2 = line.worldPostion.x - rightEdgePos;
                            float min = Mathf.Abs(dis1) < Mathf.Abs(dis2) ? dis1 : dis2;

                            distance.x = Mathf.Abs(distance.x) < Mathf.Abs(min) ? distance.x : min;
                        }

                        //查找水平方向最近的辅助线距离
                        if (line.direction == LocationLineDirection.Horizontal)
                        {
                            float dis1 = line.worldPostion.y - bottomEdgePos;
                            float dis2 = line.worldPostion.y - topEdgePos;
                            float min = Mathf.Abs(dis1) < Mathf.Abs(dis2) ? dis1 : dis2;

                            distance.y = Mathf.Abs(distance.y) < Mathf.Abs(min) ? distance.y : min;
                        }
                    }

                    if (Mathf.Abs(distance.x) < SnapLogic.SnapWorldDistance)
                    {
                        SnapLogic.SnapLineDisHoriz = distance.x;
                    }

                    if (Mathf.Abs(distance.y) < SnapLogic.SnapWorldDistance)
                    {
                        SnapLogic.SnapLineDisVert = distance.y;
                    }
                }
            }
        }
    }
}