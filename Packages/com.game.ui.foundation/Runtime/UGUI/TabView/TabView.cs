using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[System.Serializable]
public class TabEvent : UnityEvent<int>
{
}

[AddComponentMenu("UI/Foundation/TabView")]
public class TabView : MonoBehaviour
{
    [System.Serializable]
    public class Tab
    {
        public Button button;
        public GameObject content;
    }

    [Header("Settings")] [SerializeField] private List<Tab> tabs = new List<Tab>();
    [SerializeField] private int defaultTab = 0;

    [Header("Events")] public TabEvent onTabChangedEvent = new TabEvent();
    public Action<Tab, bool> onUpdateTabVisual;
    public Action<int> onTabChanged;
    private int currentTabIndex = -1;

    public int TabCount => tabs.Count;

    // 当前选中索引
    public int CurrentTabIndex => currentTabIndex;

    private void Start()
    {
        ValidateTabs();
        InitializeTabs();
        SelectTab(defaultTab);
    }

    private void ValidateTabs()
    {
        // 移除未正确配置的选项卡
        tabs.RemoveAll(t => t.button == null || t.content == null);
    }

    private void InitializeTabs()
    {
        for (int i = 0; i < tabs.Count; i++)
        {
            int index = i;
            tabs[i].button.onClick.RemoveAllListeners();
            tabs[i].button.onClick.AddListener(() => SelectTab(index));
        }
    }

    public void SelectTab(int index)
    {
        if (index == currentTabIndex || index < 0 || index >= tabs.Count)
            return;

        // 禁用所有内容
        foreach (var tab in tabs)
        {
            tab.content.SetActive(false);
            UpdateTabVisual(tab, false);
        }

        // 启用选中内容
        currentTabIndex = index;
        tabs[index].content.SetActive(true);
        UpdateTabVisual(tabs[index], true);

        // 触发事件
        onTabChangedEvent.Invoke(index);
        onTabChanged?.Invoke(index);
    }

    private void UpdateTabVisual(Tab tab, bool active)
    {
        onUpdateTabVisual?.Invoke(tab, active);
        // // 更新按钮颜色
        // var colors = tab.button.colors;
        // colors.normalColor = active ? activeColor : inactiveColor;
        // colors.selectedColor = activeColor;
        // tab.button.colors = colors;
        //
        // // 更新指示器（如果有）
        // if (tab.activeIndicator != null)
        //    tab.activeIndicator.gameObject.SetActive(active);
    }

    // 编辑器方法：添加新选项卡
    public void AddTab(Button button, GameObject content)
    {
        tabs.Add(new Tab { button = button, content = content });
        InitializeTabs();
    }

    // 编辑器方法：移除选项卡
    public void RemoveTab(int index)
    {
        if (index < 0 || index >= tabs.Count) return;
        tabs.RemoveAt(index);
        InitializeTabs();
    }

    // 通过内容对象查找索引
    public int GetTabIndex(GameObject content)
    {
        return tabs.FindIndex(t => t.content == content);
    }

    public Tab GetTab(int index)
    {
        if (index < 0 || index >= tabs.Count)
            throw new System.IndexOutOfRangeException("index out of tabs count");

        return tabs[index];
    }
}