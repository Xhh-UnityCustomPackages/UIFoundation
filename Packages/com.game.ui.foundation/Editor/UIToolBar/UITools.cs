using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Overlays;
using UnityEditor.Toolbars;
using UnityEngine;
using UnityEngine.UIElements;

namespace Game.UI.Foundation.Editor
{
    public class UITools
    {
        public Texture2D IconMore { get; private set; }
        public Texture2D IconDesignImage { get; private set; }
        public Texture2D IconCreateText { get; private set; }
        public Texture2D IconCreateImage { get; private set; }
        public Texture2D IconPrefabWidget { get; private set; }
        public Texture2D IconSettings { get; private set; }
        public Texture2D IconAlignLeft { get; private set; }
        public Texture2D IconAlignMiddle { get; private set; }
        public Texture2D IconAlignRight { get; private set; }
        public Texture2D IconAlignTop { get; private set; }
        public Texture2D IconAlignCenter { get; private set; }
        public Texture2D IconAlignBottom { get; private set; }
        public Texture2D IconReferenceLine { get; private set; }


        private static UITools s_Instance;
        private static GUIStyle s_BtnStyle;

        private const float BTN_WIDTH = 22F;
        private const float BTN_HEIGHT = 22F;

        private static readonly GUILayoutOption[] s_BtnVerticalOptions =
        {
            GUILayout.MinWidth(BTN_WIDTH),
            GUILayout.Height(BTN_HEIGHT),
            GUILayout.ExpandWidth(true)
        };

        private static readonly GUILayoutOption[] s_BtnHorizontalOptions =
        {
            GUILayout.Width(BTN_WIDTH),
            GUILayout.MinHeight(BTN_HEIGHT),
            GUILayout.ExpandHeight(true)
        };

        public static UITools Instance => s_Instance ??= new UITools
        {
            IconMore = ToolUtils.GetIcon("more"),
            IconDesignImage = ToolUtils.GetIcon("createDesignImage"),
            IconCreateText = ToolUtils.GetIcon("createText"),
            IconCreateImage = ToolUtils.GetIcon("createImage"),
            IconPrefabWidget = ToolUtils.GetIcon("prefabRepository"),
            IconSettings = ToolUtils.GetIcon("setting"),
            IconAlignLeft = ToolUtils.GetIcon("align_left"),
            IconAlignMiddle = ToolUtils.GetIcon("align_middle"),
            IconAlignRight = ToolUtils.GetIcon("align_right"),
            IconAlignTop = ToolUtils.GetIcon("align_top"),
            IconAlignCenter = ToolUtils.GetIcon("align_center"),
            IconAlignBottom = ToolUtils.GetIcon("align_bottom"),
            IconReferenceLine = ToolUtils.GetIcon("referenceline"),
        };

        public static bool IsDisplayed => SceneView.lastActiveSceneView.TryGetOverlay("UIToorBar", out Overlay uiTools) && uiTools.displayed;


        [EditorToolbarElement("UITools/Tools", typeof(SceneView))]
        public class UIToolsStrip : IMGUIContainer
        {
            public UIToolsStrip()
            {
                style.marginLeft = 2;
                style.marginTop = -1;
                onGUIHandler = () =>
                {
                    FlexDirection direction = parent.resolvedStyle.flexDirection;
                    bool isHorizontal = direction == FlexDirection.Row || direction == FlexDirection.RowReverse;
                    Instance.DrawButtons(isHorizontal);
                };
            }
        }


        private void DrawButtons(bool isHorizontal)
        {
            if (s_BtnStyle == null)
            {
                s_BtnStyle = new GUIStyle("ButtonMid")
                {
                    margin = new RectOffset(0, 0, 0, 0),
                    padding = new RectOffset(0, 0, 0, 0),
                    border = new RectOffset(0, 0, 0, 0),
                    overflow = new RectOffset(0, 0, 0, 0),
                };
            }


            GUILayoutOption[] btnOptions;
            if (isHorizontal)
            {
                EditorGUILayout.BeginHorizontal(GUILayout.ExpandHeight(true));
                btnOptions = s_BtnHorizontalOptions;
            }
            else
            {
                EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true));
                btnOptions = s_BtnVerticalOptions;
            }

            GUI.contentColor = EditorGUIUtility.isProSkin ? Color.white : Color.gray;


            DrawAlignBtn(IconAlignLeft, "左对齐", 0, 0, s_BtnStyle, btnOptions);
            GUILayout.Space(-1F);
            DrawAlignBtn(IconAlignMiddle, "水平居中", 0, 0.5F, s_BtnStyle, btnOptions);
            GUILayout.Space(-1F);
            DrawAlignBtn(IconAlignRight, "右对齐", 0, 1, s_BtnStyle, btnOptions);
            GUILayout.Space(1F);
            DrawAlignBtn(IconAlignTop, "上对齐", 1, 1, s_BtnStyle, btnOptions);
            GUILayout.Space(-1F);
            DrawAlignBtn(IconAlignCenter, "竖直居中", 1, 0.5F, s_BtnStyle, btnOptions);
            GUILayout.Space(-1F);
            DrawAlignBtn(IconAlignBottom, "下对齐", 1, 0, s_BtnStyle, btnOptions);

            // GUILayout.Space(3F);

            // DrawResizeBtn(m_SameWidth, "相同宽度", 0, s_BtnStyle, btnOptions);
            // GUILayout.Space(-1F);
            // DrawResizeBtn(m_SameHeight, "相同高度", 1, s_BtnStyle, btnOptions);
            //
            // GUILayout.Space(3F);
            //
            // DrawFitBtn(m_FitWidth, "水平贴合", 0, s_BtnStyle, btnOptions);
            // GUILayout.Space(-1F);
            // DrawFitBtn(m_FitHeight, "竖直贴合", 1, s_BtnStyle, btnOptions);
            //
            // GUILayout.Space(3F);
            //
            // DrawGroupPackBtn(m_GroupPack, "成组", s_BtnStyle, btnOptions);
            // GUILayout.Space(-1F);
            // DrawGroupUnpackBtn(m_GroupUnpack, "解组", s_BtnStyle, btnOptions);
            //
            // GUILayout.Space(3F);
            //
            // DrawGapBtn(m_AverageGapH, "平均间距", 0, s_BtnStyle, btnOptions);
            // GUILayout.Space(-1F);
            // DrawGapBtn(m_AverageGapV, "平均行距", 1, s_BtnStyle, btnOptions);

            if (isHorizontal)
            {
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                EditorGUILayout.EndVertical();
            }
        }

        private static void DrawAlignBtn(Texture tex, string name, int axis, float alignPivot, GUIStyle style, params GUILayoutOption[] btnOptions)
        {
            if (GUILayout.Button(new GUIContent(tex, name + "（按住shift同时设置轴点）"), style, btnOptions))
            {
                bool holdShift = (Event.current.modifiers & EventModifiers.Shift) != 0;
                Align(axis, alignPivot, holdShift);
            }
        }

        private static void Align(int axis, float alignPivot, bool alsoSetPivot)
        {
            (RectTransform based, List<RectTransform> list) = GetBasedAndList();
            if (based)
            {
                // 获取对齐点
                (float min, float max) = CalculateRangePart(axis, based);
                float basedPosPart = Mathf.Lerp(min, max, alignPivot);
                // 挨个计算deltaPosition并移动位置
                foreach (var trans in list)
                {
                    Undo.RecordObject(trans, "Align");
                    if (alsoSetPivot)
                    {
                        Vector2 pivot = trans.pivot;
                        pivot[axis] = alignPivot;
                        trans.pivot = pivot;
                    }

                    (float _min, float _max) = CalculateRangePart(axis, trans);
                    float posPart = Mathf.Lerp(_min, _max, alignPivot);
                    Vector3 position = trans.position;
                    position[axis] += basedPosPart - posPart;
                    trans.position = position;
                }
            }
        }

        /// <summary>
        /// 选中多个时，以选中的第一个节点为基准
        /// 只选中一个时，以父节点为基准
        /// </summary>
        /// <returns>基准和列表</returns>
        private static (RectTransform, List<RectTransform>) GetBasedAndList()
        {
            List<RectTransform> list = GetSelectedList();
            if (list.Count > 0)
            {
                RectTransform based = list.Count > 1 ? list[0] : list[0].parent as RectTransform;
                if (based)
                {
                    // 有父子关系的，父节点排前面。设置父节点会影响子节点，所以先设置父节点，再设置子节点。
                    list.Sort((tans1, tans2) => tans1.IsChildOf(tans2) ? 1 : tans2.IsChildOf(tans1) ? -1 : 0);
                    return (based, list);
                }
            }

            return (null, null);
        }

        private static List<RectTransform> GetSelectedList()
        {
            List<RectTransform> list = new List<RectTransform>();
            foreach (var obj in Selection.objects)
            {
                if (obj is GameObject go && go.transform is RectTransform rectTrans)
                {
                    list.Add(rectTrans);
                }
            }

            return list;
        }

        private static (float, float) CalculateRangePart(int axis, RectTransform trans, Transform relativeToTrans = null)
        {
            Rect rect = trans.rect;
            Vector3 globalP0 = trans.TransformPoint(new Vector3(rect.x, rect.y));
            Vector3 globalP1 = trans.TransformPoint(new Vector3(rect.x, rect.y + rect.height));
            Vector3 globalP2 = trans.TransformPoint(new Vector3(rect.x + rect.width, rect.y));
            Vector3 globalP3 = trans.TransformPoint(new Vector3(rect.x + rect.width, rect.y + rect.height));
            Vector3 localP0 = relativeToTrans ? relativeToTrans.InverseTransformPoint(globalP0) : globalP0;
            Vector3 localP1 = relativeToTrans ? relativeToTrans.InverseTransformPoint(globalP1) : globalP1;
            Vector3 localP2 = relativeToTrans ? relativeToTrans.InverseTransformPoint(globalP2) : globalP2;
            Vector3 localP3 = relativeToTrans ? relativeToTrans.InverseTransformPoint(globalP3) : globalP3;
            float min = Mathf.Min(localP0[axis], localP1[axis], localP2[axis], localP3[axis]);
            float max = Mathf.Max(localP0[axis], localP1[axis], localP2[axis], localP3[axis]);
            return (min, max);
        }
    }
}