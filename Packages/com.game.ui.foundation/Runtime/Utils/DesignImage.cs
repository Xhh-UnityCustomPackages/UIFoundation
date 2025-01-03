using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace Game.UI.Foundation
{
    [RequireComponent(typeof(Image))]
    public class DesignImage : MonoBehaviour
    {
        [SerializeField] private Image m_Image;

        private void OnEnable()
        {
        }

        private void Awake()
        {
            gameObject.SetActive(false);
        }

        private void Reset()
        {
            m_Image = GetComponent<Image>();
            gameObject.name = "[设计图]";
        }

#if ODIN_INSPECTOR
#if UNITY_EDITOR

        [Button("显示为顶层")]
        [HorizontalGroup("Sibling")]
        public void SetTop()
        {
            transform.SetAsLastSibling();
        }

        [Button("显示为底层")]
        [HorizontalGroup("Sibling")]
        public void SetBottom()
        {
            transform.SetAsFirstSibling();
        }


        [Button("半透明")]
        [HorizontalGroup("Transparent")]
        public void SetTransparent()
        {
            m_Image.color = new Color(1, 1, 1, 125f / 255.0f);
        }


        [Button("不透明")]
        [HorizontalGroup("Transparent")]
        public void SetOpacity()
        {
            m_Image.color = new Color(1, 1, 1, 1);
        }

        [Button("剧中")]
        [HorizontalGroup("Position")]
        public void SetPositionCenter()
        {
            gameObject.transform.localPosition = Vector3.zero;
        }

        [Button("外面")]
        [HorizontalGroup("Position")]
        public void SetPositionOut()
        {
            gameObject.transform.localPosition = new Vector3(1500, 0, 0);
        }

#endif
#endif
    }
}