using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Game.UI.Foundation.Editor
{
    public enum LocationLineDirection
    {
        Vertical,
        Horizontal
    }

    public class LocationLine : VisualElement
    {
        public int id;
        public Vector3 worldPostion;
        public LocationLineDirection direction;
        protected VisualElement m_Line;
        protected bool m_Selected;
        private CustomClickable m_Clickable;

        protected readonly Color m_MyBlue = new(0.4f, 0.8f, 1f);
        protected readonly Color m_MyRed = new(1f, 0.3f, 0.3f);
        
        
        public CustomClickable clickable
        {
            get
            {
                return m_Clickable;
            }
            set
            {
                if (m_Clickable != null && m_Clickable.target == this)
                {
                    this.RemoveManipulator(m_Clickable);
                }

                m_Clickable = value;

                if (m_Clickable != null)
                {
                    this.AddManipulator(m_Clickable);
                }
            }
        }

        public LocationLine()
        {
            SceneView sceneView = SceneView.lastActiveSceneView;
   
            
            // 使用动态偏移计算，而不是硬编码的40
            float dynamicOffset = LocationLineLogic.sceneviewOffset;
            
            // 计算屏幕中心点，考虑工具栏偏移
            float centerX = sceneView.camera.pixelWidth / 2f;
            float centerY = (sceneView.camera.pixelHeight - dynamicOffset) / 2f;

            worldPostion = sceneView.camera.ScreenToWorldPoint(new Vector3(centerX, centerY, 0));


            Action action = null;

            clickable = new CustomClickable(action);
            clickable.OnPointerEnterAction += OnMouseOver;
            clickable.OnPointerDownAction += OnMouseDown;
            clickable.OnPointerMoveAction += OnMouseMove;
            clickable.OnPointerUpAction += OnMouseUp;
            clickable.OnPointerLeaveAction += OnMouseOut;

            // 考虑DPI缩放因子
            float dpiScale = EditorGUIUtility.pixelsPerPoint;
            style.left = centerX / dpiScale;
            style.bottom = sceneView.camera.pixelHeight / (2 * dpiScale);
        }

        private void UpdateLineState()
        {
            m_Line.style.backgroundColor = m_Selected ? m_MyRed : m_MyBlue;
        }

        public void OnMouseOver(PointerEnterEvent evt)
        {
            RecoverCursor();
            SetCursor();
        }

        public void OnMouseDown(PointerDownEvent evt)
        {
            if (evt.button == 0)
            {
                m_Selected = true;
            }
            UpdateLineState();
        }

        public void OnMouseUp(PointerUpEvent evt)
        {
            if (evt.button == 0 && m_Selected)
            {
                m_Selected = false;
                RecoverCursor();
            }
            UpdateLineState();
        }

        public void OnMouseMove(PointerMoveEvent evt)
        {
            if (m_Selected)
            {
                OnDrag(Event.current.mousePosition);
                Event.current.Use();
            }
        }

        public void OnMouseOut(PointerLeaveEvent evt)
        {
            if (m_Selected)
            {
                OnDrag(Event.current.mousePosition);
            }else
            {
                RecoverCursor();
            }
        }

        #region Cursor

        private UXCursorType GetCursorType()
        {
            if (direction == LocationLineDirection.Horizontal)
            {
                return UXCursorType.Updown;
            }
            else
            {
                return UXCursorType.Leftright;
            }
        }
        
        private void SetCursor()
        {
            SceneViewCursorLogic.S.SetCursor(GetCursorType());
        }
        private void RecoverCursor()
        {
            SceneViewCursorLogic.S.SetCursor(UXCursorType.None);
        }

        #endregion
        
        protected virtual void OnDrag(Vector2 mousePosition)
        {

        }
        
        public virtual void UpdateLineScreenViewPos(SceneView sceneView)
        {

        }

        public class CustomClickable : Clickable
        {
            public CustomClickable(Action handler, long delay, long interval) : base(handler, delay, interval) { }
            public CustomClickable(Action<EventBase> handler) : base(handler) { }
            public CustomClickable(Action handler) : base(handler) { }

            public event Action<PointerEnterEvent> OnPointerEnterAction;
            public event Action<PointerDownEvent> OnPointerDownAction;
            public event Action<PointerMoveEvent> OnPointerMoveAction;
            public event Action<PointerUpEvent> OnPointerUpAction;
            public event Action<PointerLeaveEvent> OnPointerLeaveAction;

            protected override void RegisterCallbacksOnTarget()
            {
                target.RegisterCallback<PointerEnterEvent>(OnPointerEnter);
                target.RegisterCallback<PointerDownEvent>(OnPointerDown);
                target.RegisterCallback<PointerMoveEvent>(OnPointerMove);
                target.RegisterCallback<PointerUpEvent>(OnPointerUp);
                target.RegisterCallback<PointerLeaveEvent>(OnPointerLeave);
                base.RegisterCallbacksOnTarget();
            }

            protected override void UnregisterCallbacksFromTarget()
            {
                target.UnregisterCallback<PointerEnterEvent>(OnPointerEnter);
                target.UnregisterCallback<PointerDownEvent>(OnPointerDown);
                target.UnregisterCallback<PointerMoveEvent>(OnPointerMove);
                target.UnregisterCallback<PointerUpEvent>(OnPointerUp);
                target.UnregisterCallback<PointerLeaveEvent>(OnPointerLeave);
                base.UnregisterCallbacksFromTarget();
            }

            private void OnPointerEnter(PointerEnterEvent e)
            {
                OnPointerEnterAction?.Invoke(e);
            }

            private void OnPointerDown(PointerDownEvent e)
            {
                OnPointerDownAction?.Invoke(e);
            }

            private void OnPointerMove(PointerMoveEvent e)
            {
                OnPointerMoveAction?.Invoke(e);
            }

            private void OnPointerUp(PointerUpEvent e)
            {
                OnPointerUpAction?.Invoke(e);
            }

            private void OnPointerLeave(PointerLeaveEvent e)
            {
                OnPointerLeaveAction?.Invoke(e);
            }
        }
    }

    public class HorizontalLocationLine : LocationLine
    {
        public HorizontalLocationLine()
        {
            direction = LocationLineDirection.Horizontal;
            m_Line = new VisualElement();
            m_Line.style.position = Position.Absolute;
            m_Line.style.height = 2;
            m_Line.style.left = 0;
            m_Line.style.right = 0;
            m_Line.style.alignSelf = Align.Center;
            m_Line.style.backgroundColor = m_MyBlue;
            style.flexDirection = FlexDirection.Row;

            style.position = Position.Absolute;
            style.flexDirection = FlexDirection.Row;
            style.height = 10;
            style.right = 0;
            style.left = 0;
            Add(m_Line);
        }

        protected override void OnDrag(Vector2 mousePosition)
        {
            // 考虑DPI缩放因子
            float dpiScale = EditorGUIUtility.pixelsPerPoint;
            float adjustedMouseY = mousePosition.y * dpiScale - LocationLineLogic.sceneviewOffset;

            //SceneView的(0,0)在左上角, 所以用SceneView的高减去mousePosition.y 才是距离底部的高度
            float y = SceneView.lastActiveSceneView.camera.pixelHeight - adjustedMouseY;

            style.bottom = y / dpiScale - style.height.value.value / 2;
            Vector3 mousePos = SceneView.lastActiveSceneView.camera.ScreenToWorldPoint(new Vector3(0, y, 0));
            float minDis = Mathf.Infinity;

            if (Mathf.Abs(minDis) < SnapLogic.SnapWorldDistance)
            {
                worldPostion = mousePos + new Vector3(0, minDis, 0);
                Vector3 worldScreenPos = SceneView.lastActiveSceneView.camera.WorldToScreenPoint(worldPostion);
                style.bottom = worldScreenPos.y / dpiScale - style.height.value.value / 2;
            }
            else
            {
                worldPostion = mousePos;
            }
            SceneView.lastActiveSceneView.Repaint();
        }
        
        public override void UpdateLineScreenViewPos(SceneView sceneView)
        {
            if (!m_Selected)
            {
                // 将世界坐标转换为屏幕坐标
                Vector3 screenPos = sceneView.camera.WorldToScreenPoint(worldPostion);

                // 考虑DPI缩放因子
                float dpiScale = EditorGUIUtility.pixelsPerPoint;
                float adjustedY = screenPos.y / dpiScale - style.height.value.value / 2f;

                // 确保坐标在有效范围内
                adjustedY = Mathf.Clamp(adjustedY, 0, sceneView.camera.pixelHeight / dpiScale);

                style.bottom = adjustedY;
            }
        }
    }

    public class VerticalLocationLine : LocationLine
    {
        public VerticalLocationLine()
        {
            direction = LocationLineDirection.Vertical;
            m_Line = new VisualElement();
            m_Line.style.position = Position.Absolute;
            m_Line.style.alignSelf = Align.Center;
            m_Line.style.top = 0;
            m_Line.style.bottom = 0;
            m_Line.style.width = 2;
            m_Line.style.backgroundColor = m_MyBlue;
            style.position = Position.Absolute;
            style.width = 10;
            style.bottom = 0;
            style.top = 0;
            Add(m_Line);
        }
        
        protected override void OnDrag(Vector2 mousePosition)
        {
            // 考虑DPI缩放因子
            float dpiScale = EditorGUIUtility.pixelsPerPoint;
            float adjustedMouseX = mousePosition.x * dpiScale;

            style.left = adjustedMouseX - style.width.value.value / 2;

            Vector3 mousePos = SceneView.lastActiveSceneView.camera.ScreenToWorldPoint(new Vector3(adjustedMouseX, 0, 0));

            float minDis = Mathf.Infinity;

            if (Mathf.Abs(minDis) < SnapLogic.SnapWorldDistance)
            {
                worldPostion = mousePos + new Vector3(minDis, 0, 0);
                Vector3 worldScreenPos = SceneView.lastActiveSceneView.camera.WorldToScreenPoint(worldPostion);
                style.left = worldScreenPos.x / dpiScale - style.width.value.value / 2;
            }
            else
            {
                worldPostion = mousePos;
            }
            SceneView.lastActiveSceneView.Repaint();
        }
        
        //用来更新 SceneView的拖动或者滚轮缩放后 辅助线在SceneView的位置
        public override void UpdateLineScreenViewPos(SceneView sceneView)
        {
            if (!m_Selected)
            {
                // 将世界坐标转换为屏幕坐标
                Vector3 screenPos = sceneView.camera.WorldToScreenPoint(worldPostion);

                // 考虑DPI缩放因子
                float dpiScale = EditorGUIUtility.pixelsPerPoint;
                float adjustedX = screenPos.x / dpiScale - style.width.value.value / 2f;

                // 确保坐标在有效范围内
                adjustedX = Mathf.Clamp(adjustedX, 0, sceneView.camera.pixelWidth / dpiScale);

                style.left = adjustedX;
            }
        }
    }
}