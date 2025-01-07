using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using System.IO;

namespace Game.UI.Foundation.Editor
{
    public class AssetItemBase : VisualElement
    {
        private const int BorderWidth = 1;
        private const int SizeW = 140; //资产块初始大小
        private const int LabelHeight = 20; // 名称高度
        private readonly Image _thumbnail; // 缩略图
        private readonly Image _itemIcon;
        private readonly VisualElement _upContainer;
        protected readonly VisualElement Row;

        public readonly string FilePath = string.Empty;
        public readonly string FileName = string.Empty;
        public bool Selected;

        protected AssetItemBase(FileInfo fileInfo, float scale = 1)
        {
            if (fileInfo.DirectoryName == null) return;
            var tmp = fileInfo.DirectoryName.Replace("\\", "/");
            FilePath = FileUtil.GetProjectRelativePath(tmp) + "/" + fileInfo.Name;
            FileName = Path.GetFileNameWithoutExtension(fileInfo.Name);

            style.width = scale == 0 ? Length.Percent(100) : SizeW * scale;
            style.marginLeft = 12;
            style.marginTop = 12;
            style.marginRight = 12;
            style.marginBottom = scale == 0 ? 0 : 12;

            Row = new VisualElement();
            Row.style.width = Length.Percent(100);
            Row.style.borderTopWidth = BorderWidth;
            Row.style.borderBottomWidth = BorderWidth;
            Row.style.borderLeftWidth = BorderWidth;
            Row.style.borderRightWidth = BorderWidth;
            Row.style.paddingBottom = 1;
            Row.style.paddingLeft = 1;
            Row.style.paddingRight = 1;
            Row.style.paddingTop = 1;
            Row.style.flexDirection = scale == 0 ? FlexDirection.Row : FlexDirection.Column;
            Row.tooltip = FileName;
            this.Add(Row);

            // 列表模式下不显示缩略图
            if (scale != 0)
            {
                _upContainer = new VisualElement();
                _upContainer.style.alignItems = Align.Center;
                _upContainer.style.backgroundColor = new Color(63f / 255f, 63f / 255f, 63f / 255f);
                _upContainer.style.height = scale == 0 ? 20 : 154 * scale;
                _upContainer.name = "UpContainer";
                Row.Add(_upContainer);

                // 缩略图设置
                _thumbnail = new Image();
                _upContainer.Add(_thumbnail);
                _thumbnail.style.height = Length.Percent(100);
                _thumbnail.style.width = Length.Percent(100);
            }

            // label区域
            VisualElement downContainer = new VisualElement();
            downContainer.style.width = Length.Percent(100);
            downContainer.style.height = LabelHeight;
            downContainer.style.flexDirection = FlexDirection.Row;
            downContainer.name = "DownContainer";
            Row.Add(downContainer);

            _itemIcon = new Image()
            {
                style =
                {
                    width = LabelHeight,
                    height = LabelHeight,
                }
            };
            downContainer.Add(_itemIcon);

            var label = new Label();
            label.style.flexGrow = 1;
            label.style.flexShrink = 1;
            label.style.fontSize = 14;
            label.style.color = Color.white;
            label.style.unityTextAlign = scale == 0 ? TextAnchor.MiddleLeft : TextAnchor.MiddleCenter;
            label.style.overflow = Overflow.Hidden;
            label.style.textOverflow = TextOverflow.Ellipsis;
            label.style.whiteSpace = WhiteSpace.NoWrap;
            label.text = FileName;
            downContainer.Add(label);
        }


        protected void SetThumbnail(Texture texture)
        {
            if (_thumbnail != null)
            {
                _thumbnail.image = texture;
            }
        }

        protected void SetIcon(Texture iconTexture)
        {
            if (_itemIcon != null)
            {
                _itemIcon.image = iconTexture;
            }
        }

        /// <summary>
        /// 选择界面资源后，更改其选中状态，且在info区域显示信息
        /// </summary>
        /// <param name="selected">是否选择</param>
        /// <param name="icon">资源图标</param>
        /// <param name="filePath">资源路径</param>
        public void SetSelected(bool selected, out Texture icon, out string filePath)
        {
            SetSelectedUI(selected);
            icon = _itemIcon.image;
            filePath = FilePath;
        }

        /// <summary>
        /// 选择界面资源，更改其选中状态
        /// </summary>
        /// <param name="selected"></param>
        public void SetSelected(bool selected)
        {
            SetSelectedUI(selected);
        }

        private void SetSelectedUI(bool selected)
        {
            Selected = selected;
            if (Selected)
            {
                Color myColor = new Color(27f / 255f, 150f / 255f, 233f / 255f, 1f);
                Row.style.borderTopColor = myColor;
                Row.style.borderBottomColor = myColor;
                Row.style.borderLeftColor = myColor;
                Row.style.borderRightColor = myColor;
            }
            else
            {
                Color myColor = Color.clear;
                Row.style.borderTopColor = myColor;
                Row.style.borderBottomColor = myColor;
                Row.style.borderLeftColor = myColor;
                Row.style.borderRightColor = myColor;
            }
        }
    }
}