using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

/// <summary>
/// 长按按钮
/// </summary>
public class LongPressButton : Button
{
    [SerializeField] private float m_CheckTime = 0.5f; //检测长按的时长
    [SerializeField] private ButtonClickedEvent m_OnLongPress = new ButtonClickedEvent();

    public ButtonClickedEvent onLongPress
    {
        get { return m_OnLongPress; }
        set { m_OnLongPress = value; }
    }

    private bool m_IsDown; //是否按下
    private float m_DownTime; //按下的那一刻时间

    public override void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);
        if (!interactable) return;

        m_DownTime = Time.time;
        m_IsDown = true;
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        base.OnPointerUp(eventData);
        m_DownTime = 0;
        m_IsDown = false;
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        base.OnPointerExit(eventData);
        m_IsDown = false;
    }

    private void Update()
    {
        if (!interactable) return;

        if (m_IsDown)
        {
            float spown = Time.time - m_DownTime;
            if (spown > m_CheckTime)
            {
                m_OnLongPress?.Invoke();
            }
        }
    }
}