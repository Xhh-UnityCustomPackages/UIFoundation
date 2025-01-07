using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

namespace Game.UI.Foundation.Editor
{
    public static class ToolUtils
    {
        public static readonly string RootPath = "Packages/com.game.ui.foundation/";
        public static readonly string IconPath = RootPath + "Editor/Res/Icon/";

        #region Texutre&Icon

        //读取和缓存Icon图片
        static Dictionary<string, Texture2D> m_IconDict = new();

        public static Texture2D GetIcon(string name)
        {
            if (!m_IconDict.TryGetValue(name, out var tex))
            {
                tex = AssetDatabase.LoadAssetAtPath<Texture2D>($"{IconPath}{name}.png");

                m_IconDict[name] = tex;
            }

            return tex;
        }

        #endregion
    }
}