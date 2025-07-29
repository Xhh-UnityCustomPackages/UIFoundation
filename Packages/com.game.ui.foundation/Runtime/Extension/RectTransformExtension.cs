
using UnityEngine;

namespace Game.UI.Foundation
{
    public static class RectTransformExtension
    {
        //获取一个对象的边缘的世界坐标(去掉缩放和pivot的影响)
        public static float GetLeftWorldPosition(this RectTransform rect)
        {
            return rect.TransformPoint(new Vector3(rect.rect.xMin, 0, 0)).x;
        }
        
        //右边x
        public static float GetRightWorldPosition(this RectTransform rect)
        {
            return rect.TransformPoint(new Vector3(rect.rect.xMax, 0, 0)).x;
        }
        
        //上边y
        public static float GetTopWorldPosition(this RectTransform rect)
        {
            return rect.TransformPoint(new Vector3(0, rect.rect.yMax, 0)).y;
        }
        
        //下边y
        public static float GetBottomWorldPosition(this RectTransform rect)
        {
            return rect.TransformPoint(new Vector3(0, rect.rect.yMin, 0)).y;
        }
    }
}