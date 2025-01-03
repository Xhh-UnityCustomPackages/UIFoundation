using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.UI.Foundation
{
    public static class RectTransformExtensions
    {
        public static void SetStretch(this RectTransform rectTransform)
        {
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.anchoredPosition3D = Vector3.zero;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
        }
    }
}