using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using System.IO;

namespace Game.UI.Foundation.Editor
{
    public class WidgetItemRepository : AssetItemBase
    {
        protected readonly GameObject AssetObj;

        public readonly string[] Labels;

        public WidgetItemRepository(FileInfo fileInfo, float scale = 1) : base(fileInfo, scale)
        {
            var obj = AssetDatabase.LoadAssetAtPath<GameObject>(FilePath);
            AssetObj = obj;


            // 获取缩略图
            if (string.IsNullOrEmpty(FilePath)) return;
            var guid = AssetDatabase.AssetPathToGUID(FilePath);
            var previewTex = Utils.GetAssetsPreviewTexture(guid, 144);
            SetThumbnail(previewTex);

            // 获取图标
            var icon = ToolUtils.GetIcon("PrefabIcon");
            SetIcon(icon);

            // Row.RegisterCallback<MouseDownEvent, bool>(OnClick, isPrefabRecent);
            Labels = AssetDatabase.GetLabels(AssetObj);
        }
    }
}