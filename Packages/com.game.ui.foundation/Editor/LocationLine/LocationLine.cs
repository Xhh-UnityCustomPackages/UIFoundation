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

        
        // protected static List<RectEx> m_Rects;
        //
        // protected struct RectEx
        // {
        //     /// <summary>
        //     /// 长度为4，上下左右
        //     /// </summary>
        //     public float[] pos;
        //
        //     public RectEx(RectTransform trans)
        //     {
        //         pos = new float[4];
        //         pos[0] = (float)Math.Round((double)trans.GetTopWorldPosition(), 1);
        //         pos[1] = (float)Math.Round((double)trans.GetBottomWorldPosition(), 1);
        //         pos[2] = (float)Math.Round((double)trans.GetLeftWorldPosition(), 1);
        //         pos[3] = (float)Math.Round((double)trans.GetRightWorldPosition(), 1);
        //     }
        // }
        
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
            worldPostion = sceneView.camera.ScreenToWorldPoint(new Vector3(sceneView.camera.pixelWidth / 2, (sceneView.camera.pixelHeight - 40) / 2,
                0));

            Action action = null;

            clickable = new CustomClickable(action);
            clickable.OnPointerEnterAction += OnMouseOver;
            clickable.OnPointerDownAction += OnMouseDown;
            clickable.OnPointerMoveAction += OnMouseMove;
            clickable.OnPointerUpAction += OnMouseUp;
            clickable.OnPointerLeaveAction += OnMouseOut;

            style.left = sceneView.camera.pixelWidth / 2;
            style.bottom = sceneView.camera.pixelHeight / 2;
        }

        private void UpdateLineState()
        {
            m_Line.style.backgroundColor = m_Selected ? m_MyRed : m_MyBlue;
        }

        public void OnMouseOver(PointerEnterEvent evt)
        {
        }

        public void OnMouseDown(PointerDownEvent evt)
        {
            if (evt.button == 0)
            {
                m_Selected = true;
            }
        }

        public void OnMouseUp(PointerUpEvent evt)
        {
            if (evt.button == 0 && m_Selected)
            {
                m_Selected = false;
                LocationLineLogic.S.ModifyLine(this);
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
            }
        }
        
        protected virtual void OnDrag(Vector2 mousePosition)
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
            mousePosition.y = mousePosition.y - LocationLineLogic.sceneviewOffset;
            
            //SceneView的(0,0)在左上角, 所以用SceneView的高减去mousePosition.y 才是距离底部的高度
            float y = SceneView.lastActiveSceneView.camera.pixelHeight - mousePosition.y;

            style.bottom = y - style.height.value.value / 2;
            Vector3 mousePos = SceneView.lastActiveSceneView.camera.ScreenToWorldPoint(new Vector3(0, y, 0));
            float minDis = Mathf.Infinity;
            // foreach (var rect in m_Rects)
            // {
            //     if (Mathf.Abs(minDis) > Mathf.Abs(rect.pos[0] - mousePos.y))
            //     {
            //         minDis = rect.pos[0] - mousePos.y;
            //     }
            //     if (Mathf.Abs(minDis) > Mathf.Abs(rect.pos[1] - mousePos.y))
            //     {
            //         minDis = rect.pos[1] - mousePos.y;
            //     }
            // }
            if (Mathf.Abs(minDis) < SnapLogic.SnapWorldDistance)
            {
                worldPostion = mousePos + new Vector3(0, minDis, 0);
                style.bottom = SceneView.lastActiveSceneView.camera.WorldToScreenPoint(worldPostion).y - style.height.value.value / 2;
            }
            else
            {
                worldPostion = mousePos;
            }
            SceneView.lastActiveSceneView.Repaint();
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
            style.left = mousePosition.x - style.width.value.value / 2;

            float screenLeft = style.left.value.value + style.width.value.value / 2;
            Vector3 mousePos = SceneView.lastActiveSceneView.camera.ScreenToWorldPoint(new Vector3(mousePosition.x, 0, 0));

            float minDis = Mathf.Infinity;
            // foreach (var rect in m_Rects)
            // {
            //     if (Mathf.Abs(minDis) > Mathf.Abs(rect.pos[2] - mousePos.x))
            //     {
            //         minDis = rect.pos[2] - mousePos.x;
            //     }
            //     if (Mathf.Abs(minDis) > Mathf.Abs(rect.pos[3] - mousePos.x))
            //     {
            //         minDis = rect.pos[3] - mousePos.x;
            //     }
            // }
            if (Mathf.Abs(minDis) < SnapLogic.SnapWorldDistance)
            {
                worldPostion = mousePos + new Vector3(minDis, 0, 0);
                style.left = SceneView.lastActiveSceneView.camera.WorldToScreenPoint(worldPostion).x - style.width.value.value / 2;
            }
            else
            {
                worldPostion = mousePos;
            }
            SceneView.lastActiveSceneView.Repaint();
        }
    }
}