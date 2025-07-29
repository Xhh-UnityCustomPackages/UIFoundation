using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEditor.SceneManagement;

namespace Game.UI.Foundation.Editor
{
    public static class Utils
    {
        #region Texutre&Icon

        //生成和缓存 preview图片
        static Dictionary<string, Texture> m_PreviewDict = new Dictionary<string, Texture>();
        static Dictionary<string, Texture2D> m_PreviewDict2D = new Dictionary<string, Texture2D>();

        public static Texture GetAssetsPreviewTexture(string guid, int previewSize = 79)
        {
            if (!File.Exists(AssetDatabase.GUIDToAssetPath(guid))) return null;

            if (!m_PreviewDict.TryGetValue(guid, out var tex))
            {
                tex = GenAssetsPreviewTexture(guid, previewSize);
                if (tex != null)
                {
                    m_PreviewDict[guid] = tex;
                }
            }

            if (tex == null)
            {
                tex = GenAssetsPreviewTexture(guid, previewSize);
                if (tex != null)
                    m_PreviewDict[guid] = tex;
            }

            return tex;
        }

        public static Texture UpdatePreviewTexture(string guid, int previewSize = 79)
        {
            var tex = GenAssetsPreviewTexture(guid, previewSize);
            if (tex != null)
                m_PreviewDict[guid] = tex;

            return tex;
        }

        public static Texture GetAssetsNewPreviewTexture(string guid, int previewSize = 79)
        {
            Texture tex = GenAssetsPreviewTexture(guid, previewSize);
            if (tex != null)
            {
                m_PreviewDict[guid] = tex;
            }

            return tex;
        }

        public static Texture2D GetAssetsPreviewTexture2D(string guid, int previewSize = 79)
        {
            if (!m_PreviewDict2D.TryGetValue(guid, out var tex))
            {
                Texture tex1 = GetAssetsPreviewTexture(guid, previewSize);
                if (tex1 != null)
                {
                    tex = TextureToTexture2D(tex1);
                    m_PreviewDict2D[guid] = tex;
                }
            }

            return tex;
        }

        private static Texture2D TextureToTexture2D(Texture texture)
        {
            Texture2D texture2D = new Texture2D(texture.width, texture.height, TextureFormat.RGBA32, false);
            RenderTexture currentRT = RenderTexture.active;
            RenderTexture renderTexture = RenderTexture.GetTemporary(texture.width, texture.height, 32);
            Graphics.Blit(texture, renderTexture);

            RenderTexture.active = renderTexture;
            texture2D.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
            texture2D.Apply();

            RenderTexture.active = currentRT;
            RenderTexture.ReleaseTemporary(renderTexture);

            return texture2D;
        }

        /// <summary>
        /// 生成prefab的预览图,
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="previewSize"></param>
        /// <returns></returns>
        public static Texture GenAssetsPreviewTexture(string guid, int previewSize = 79)
        {
            // if (EditorApplication.isPlaying)
            // {
            //     return null;
            // }

            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject obj = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            GameObject canvas = new GameObject("UXRenderCanvas", typeof(Canvas));
            GameObject cameraObj = new GameObject("UXRenderCamera", typeof(Camera));
            canvas.transform.position = new Vector3(10000, 10000, 10000);
            canvas.GetComponent<RectTransform>().sizeDelta = new Vector2(Screen.currentResolution.width, Screen.currentResolution.height);

            GameObject go = GameObject.Instantiate(obj, canvas.transform);

            Bounds bound = GetBounds(go);

            cameraObj.transform.position = new Vector3((bound.max.x + bound.min.x) / 2, (bound.max.y + bound.min.y) / 2, (bound.max.z + bound.min.z) / 2 - 100);
            cameraObj.transform.LookAt(cameraObj.transform.position);

            Camera camera = cameraObj.GetComponent<Camera>();
            camera.cameraType = CameraType.SceneView;
            camera.orthographic = true;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0, 0, 0, 0f);

            float width = bound.max.x - bound.min.x;
            float height = bound.max.y - bound.min.y;
            float max_camera_size = (width > height ? width : height) + 10;
            camera.orthographicSize = max_camera_size / 2;

            RenderTexture rt = RenderTexture.GetTemporary(previewSize, previewSize, 24);
            camera.targetTexture = rt;
            camera.RenderDontRestore();

            RenderTexture tex = new RenderTexture(previewSize, previewSize, 0, RenderTextureFormat.Default);
            Graphics.Blit(rt, tex);

            //Texture2D tex = new Texture2D(previewSize, previewSize, TextureFormat.ARGB32, false);
            //tex.ReadPixels(new Rect(0, 0, previewSize, previewSize), 0, 0);
            //tex.Apply();

            RenderTexture.active = null;
            camera.targetTexture = null;
            rt.Release();
            RenderTexture.ReleaseTemporary(rt);

            Object.DestroyImmediate(canvas);
            Object.DestroyImmediate(cameraObj);

            return tex;
        }

        public static Bounds GetBounds(GameObject obj)
        {
            Vector3 Min = new Vector3(99999, 99999, 99999);
            Vector3 Max = new Vector3(-99999, -99999, -99999);
            MeshRenderer[] renders = obj.GetComponentsInChildren<MeshRenderer>();
            if (renders.Length > 0)
            {
                for (int i = 0; i < renders.Length; i++)
                {
                    if (renders[i].bounds.min.x < Min.x)
                        Min.x = renders[i].bounds.min.x;
                    if (renders[i].bounds.min.y < Min.y)
                        Min.y = renders[i].bounds.min.y;
                    if (renders[i].bounds.min.z < Min.z)
                        Min.z = renders[i].bounds.min.z;

                    if (renders[i].bounds.max.x > Max.x)
                        Max.x = renders[i].bounds.max.x;
                    if (renders[i].bounds.max.y > Max.y)
                        Max.y = renders[i].bounds.max.y;
                    if (renders[i].bounds.max.z > Max.z)
                        Max.z = renders[i].bounds.max.z;
                }
            }
            else
            {
                RectTransform[] rectTrans = obj.GetComponentsInChildren<RectTransform>();
                Vector3[] corner = new Vector3[4];
                for (int i = 0; i < rectTrans.Length; i++)
                {
                    //获取节点的四个角的世界坐标，分别按顺序为左下左上，右上右下
                    rectTrans[i].GetWorldCorners(corner);
                    if (corner[0].x < Min.x)
                        Min.x = corner[0].x;
                    if (corner[0].y < Min.y)
                        Min.y = corner[0].y;
                    if (corner[0].z < Min.z)
                        Min.z = corner[0].z;

                    if (corner[2].x > Max.x)
                        Max.x = corner[2].x;
                    if (corner[2].y > Max.y)
                        Max.y = corner[2].y;
                    if (corner[2].z > Max.z)
                        Max.z = corner[2].z;
                }
            }

            Vector3 center = (Min + Max) / 2;
            Vector3 size = new Vector3(Max.x - Min.x, Max.y - Min.y, Max.z - Min.z);
            return new Bounds(center, size);
        }

        public static void DrawGreenRect(int instanceID, Rect selectionRect, string text)
        {
            GameObject go = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
            Rect rect = new Rect(selectionRect)
            {
                width = selectionRect.width + (PrefabUtility.IsAnyPrefabInstanceRoot(go) ? 0 : 20)
            };
            EditorGUI.DrawRect(rect, new Color(0.157f, 0.157f, 0.157f, 1f));
            GUI.Label(selectionRect, PrefabUtility.GetIconForGameObject(go));
            GUI.Label(new Rect(selectionRect) { x = selectionRect.x + 20 },
                text, new GUIStyle() { normal = { textColor = Color.green } });
        }

        #endregion

    }
}