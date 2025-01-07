using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;

namespace Game.UI.Foundation.Editor
{
    /// <summary>
    /// Widget: 游戏中会重复使用的Prefab 如: 一级页签|二级页签|确认按钮 等
    /// 提供UI Style 方便统一进行风格迭代和更换
    /// </summary>
    public class WidgetLibraryWindow : EditorWindow
    {
        private static WidgetLibraryWindow m_window;

        [MenuItem("Tools/UI/WidgetLibrary", false, 51)]
        public static void OpenWindow()
        {
            int width = 1272 + 13 + 12;
            int height = 636;
            m_window = GetWindow<WidgetLibraryWindow>();
            m_window.minSize = new Vector2(width, height);
            m_window.titleContent.text = "组件库"; //EditorLocalization.GetLocalization(EditorLocalizationStorage.Def_组件库);
            m_window.titleContent.image = ToolUtils.GetIcon("prefabRepository");
            // UXToolAnalysis.SendUXToolLog(UXToolAnalysisLog.WidgetLibrary);
        }

        static WidgetLibraryWindow()
        {
            EditorApplication.playModeStateChanged += (obj) =>
            {
                if (HasOpenInstances<WidgetLibraryWindow>())
                    m_window = GetWindow<WidgetLibraryWindow>();
                if (!EditorApplication.isPlaying && !EditorApplication.isPlayingOrWillChangePlaymode)
                {
                    // if (m_window)
                    //     m_window.RefreshWindow();
                }
            };
        }


        [UnityEditor.Callbacks.DidReloadScripts(0)]
        private static void OnScriptReload()
        {
            if (HasOpenInstances<WidgetLibraryWindow>())
                m_window = GetWindow<WidgetLibraryWindow>();
        }

        public static WidgetLibraryWindow GetInstance()
        {
            return m_window;
        }

        [SerializeField] private VisualTreeAsset m_VisualTreeAsset = default;

        public static bool clickFlag = false;

        //UI
        private VisualElement leftContainer;
        private VisualElement rightContainer;
        private ScrollView labelScroll;
        private ScrollView widgetScroll;
        private Slider slider;


        //Data
        private GameObject LoadPrefab = null;
        private Texture texture;
        private HashSet<string> labelList = new();
        private List<WidgetItemRepository> asstesItems = new();
        private List<FileInfo> prefabInfoList = new();
        public string filtration = "All";
        private List<WidgetItemRepository> fliterItems = new();
        private bool _isSortByDict;

        //Data
        private bool RightContainerDragIn = false; //判断拖拽操作是否是拖进来
        private bool RightContainerDrag = false; //判断拖拽的起点是否在组件库右容器

        private void OnEnable()
        {
            DragAndDrop.AddDropHandler(OnHierarchyGUI);
        }

        private void OnDisable()
        {
            DragAndDrop.RemoveDropHandler(OnHierarchyGUI);
        }

        void InitWindowData()
        {
            asstesItems.Clear();

            prefabInfoList.Clear();

            List<string> list = new();

            if (UIFoundationSettings.Instance.WidgetLibrary != null)
            {
                list.AddRange(AssetDatabase.FindAssets("t:prefab", new string[] { AssetDatabase.GetAssetPath(UIFoundationSettings.Instance.WidgetLibrary) }));
            }
            else
            {
                void CreateFolder(string path, string name)
                {
                    Undo.RecordObject(this, "Create Folder");
                    AssetDatabase.CreateFolder(path, name);
                }

                if (!AssetDatabase.IsValidFolder("Assets/WidgetLibrary"))
                {
                    CreateFolder("Assets", "WidgetLibrary");
                }

                UIFoundationSettings.Instance.WidgetLibrary = AssetDatabase.LoadAssetAtPath<Object>("Assets/WidgetLibrary");
                UIFoundationSettings.Instance.Save();
            }


            for (int i = 0; i < list.Count; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(list[i]);
                var go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                var labels = AssetDatabase.GetLabels(go);
                foreach (var label in labels)
                {
                    labelList.Add(label);
                }
            }

            for (int i = list.Count - 1; i >= 0; i--)
            {
                //AssetDatabase.Refresh();
                string path = AssetDatabase.GUIDToAssetPath(list[i]);
                if (!File.Exists(path) || path == "")
                {
                    list.Remove(list[i]);
                }
                else
                {
                    prefabInfoList.Add(new FileInfo(path));
                }
            }

            if (!_isSortByDict)
            {
                prefabInfoList.Sort((f1, f2) => f1.Name.CompareTo(f2.Name));
            }

            for (int i = 0; i < prefabInfoList.Count; i++)
            {
                WidgetItemRepository item = new WidgetItemRepository(prefabInfoList[i], slider.value / slider.highValue);
                asstesItems.Add(item);
            }
        }

        public void CreateGUI()
        {
            // Each editor window contains a root VisualElement object
            VisualElement root = rootVisualElement;

            // Instantiate UXML
            VisualElement labelFromUXML = m_VisualTreeAsset.Instantiate();
            root.Add(labelFromUXML);

            leftContainer = root.Q("leftContainer");
            rightContainer = root.Q("rightContainer");
            rightContainer.RegisterCallback((DragEnterEvent e) => { RightContainerDragIn = true; });
            widgetScroll = root.Q<ScrollView>("widgetScroll");
            slider = root.Q<Slider>("Slider");
            slider.value = slider.highValue;
            slider.RegisterValueChangedCallback(evt => OnSliderValueChanged(evt.newValue));

            InitWindowData();
            RefreshLeftTypeList();
            ChangeScrollView();
            RefreshRightPrefabContainer();
        }

        public void RefreshWindow()
        {
            //Debug.Log("RefreshWindow");
            RefreshLeftTypeList();
            RefreshRightPrefabContainer();
        }

        void RefreshLeftTypeList()
        {
            leftContainer.Clear();
            Button allBtn = new Button();
            allBtn.clicked += () =>
            {
                _isSortByDict = false;
                RefreshRightPrefabContainer("All");
                EditorApplication.delayCall += RefreshWindow;
            };
            allBtn.text = "All";
            leftContainer.Add(allBtn);

            foreach (var label in labelList)
            {
                Button btn = new Button();
                btn.text = label;
                btn.clicked += () =>
                {
                    _isSortByDict = false;
                    RefreshRightPrefabContainer(label);
                    EditorApplication.delayCall += RefreshWindow;
                };
                leftContainer.Add(btn);
            }
        }

        private void ChangeScrollView()
        {
            widgetScroll.style.whiteSpace = WhiteSpace.NoWrap;
            var ve = widgetScroll.contentContainer;
            ve.style.flexDirection = FlexDirection.Row;
            ve.style.flexWrap = Wrap.Wrap;
            ve.style.overflow = Overflow.Visible;
            var viewport = widgetScroll.contentViewport;
            viewport.style.marginLeft = 10;
            viewport.style.marginTop = 10;
            viewport.style.marginRight = 10;
            widgetScroll.RegisterCallback<MouseDownEvent>(evt =>
            {
                if (clickFlag)
                {
                    clickFlag = false;
                }
                else
                {
                    foreach (var t in fliterItems)
                    {
                        // t.SetSelected(false);
                    }
                }
            });
            widgetScroll.RegisterCallback<DragPerformEvent>(evt =>
            {
                // Debug.Log("yeah!!!");
                DoPrefabDrag();
                RightContainerDrag = false;
            });
        }


        private void OnSliderValueChanged(float x)
        {
            if (x / slider.highValue < 0.1f)
            {
                slider.value = 0;
            }

            RefreshRightPrefabContainer();
        }

        void RefreshRightPrefabContainer(string type = null)
        {
            InitWindowData();

            if (type != null)
            {
                filtration = type;
            }

            widgetScroll.Clear();

            if (filtration == "All")
            {
                fliterItems = asstesItems;
            }
            else
            {
                fliterItems = asstesItems.Where(item => item.Labels.Contains(filtration)).ToList();
            }


            if (fliterItems.Count == 0) return;

            for (int i = 0; i < fliterItems.Count; i++)
            {
                int tmp = i;
                fliterItems[i].RegisterCallback((MouseDownEvent e) =>
                {
                    for (int j = 0; j < fliterItems.Count; j++)
                        fliterItems[j].SetSelected(false);
                    fliterItems[tmp].SetSelected(true);
                    InitDragState();
                    RightContainerDrag = true; //拖拽起始点在右容器
                    DragAndDrop.PrepareStartDrag();
                    DragAndDrop.StartDrag("prefab");
                    LoadPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(fliterItems[tmp].FilePath);
                    string guid = AssetDatabase.AssetPathToGUID(fliterItems[tmp].FilePath);
                    texture = Utils.GetAssetsPreviewTexture(guid);
                    Object[] obj = { LoadPrefab };
                    DragAndDrop.objectReferences = obj;
                    SceneViewUtils.ClearDelegate();
                    SceneViewUtils.AddDelegate(CustomScene);
                });
                widgetScroll.Add(fliterItems[i]);
            }

            Repaint();
        }

        private void DoPrefabDrag()
        {
            if (RightContainerDragIn && !RightContainerDrag)
            {
                // foreach(var t in DragAndDrop.paths)
                List<GameObject> dragObjList = new List<GameObject>();
                foreach (var t in DragAndDrop.objectReferences)
                {
                    // Debug.LogWarning(t.name);
                    GameObject obj = t as GameObject;
                    if (obj != null)
                    {
                        dragObjList.Add(obj);
                    }
                }

                if (dragObjList.Count == 1)
                {
                    string path = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(DragAndDrop.objectReferences[0]);
                    if (!string.IsNullOrEmpty(path))
                    {
                        //从外面拖进来了一个prefab,设置为组件
                        // PrefabCreateWindow.OpenWindowFromPrefab(dragObjList[0], path);
                    }
                    else
                    {
                        //从hierarchy拖进来了一个节点,要新建prefab并设置组件
                        //         PrefabCreateWindow.OpenWindowFromObjList(dragObjList.ToArray());
                    }
                }
                else
                {
                    //从hierarchy拖进来了多个节点,要新建prefab并设置组件
                    //     if (CombineWidgetLogic.CanCombine(dragObjList.ToArray()))
                    //     {
                    //         PrefabCreateWindow.OpenWindowFromObjList(dragObjList.ToArray());
                    //     }
                    //     else
                    //     {
                    //         EditorUtility.DisplayDialog("messageBox",
                    //             EditorLocalization.GetLocalization(EditorLocalizationStorage.Def_Prefab创建失败Tip),
                    //             EditorLocalization.GetLocalization(EditorLocalizationStorage.Def_确定),
                    //             EditorLocalization.GetLocalization(EditorLocalizationStorage.Def_取消));
                    //     }
                }

                InitDragState();
            }
        }

        /// <summary>
        /// 重置拖拽状态，设置为：拖拽起始点是不在组件库，并且没有拖拽进入组件库的事件发生。
        /// </summary>
        private void InitDragState()
        {
            //RightContainerDragOut = false;
            RightContainerDragIn = false;
            RightContainerDrag = false;
            LoadPrefab = null;
        }

        private void CustomScene(SceneView sceneView)
        {
            if (Event.current.type == EventType.DragUpdated || Event.current.type == EventType.DragPerform)
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                SceneViewUtils.RemoveDelegate(DrawTexture);
                SceneViewUtils.AddDelegate(DrawTexture);

                if (Event.current.type == EventType.DragPerform)
                {
                    Vector2 mousePos = Event.current.mousePosition;
                    Transform container = FindContainerLogic.GetObjectParent(Selection.gameObjects);

                    if (container != null)
                    {
                        mousePos.y = sceneView.camera.pixelHeight - mousePos.y;
                        Vector3 WorldPos = sceneView.camera.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, 0));
                        Vector3 localPos = container.InverseTransformPoint(new Vector3(WorldPos.x, WorldPos.y, 0));
                        bool unpack = AssetDatabase.GetLabels(LoadPrefab).Contains("Unpack");
                        if (unpack)
                        {
                            DragPerformAsUnPack(container, localPos);
                        }
                        else
                        {
                            DragPerformAsPrefab(container, localPos);
                        }

                        EditorUtility.SetDirty(container);
                    }

                    SceneViewUtils.RemoveDelegate(DrawTexture);
                    SceneViewUtils.RemoveDelegate(CustomScene);
                    // InitDragState();
                }

                Event.current.Use();
            }
        }

        private void DragPerformAsPrefab(Transform container, Vector3 localPos)
        {
            GameObject currentPrefab = PrefabUtility.InstantiatePrefab(LoadPrefab) as GameObject;
            currentPrefab.transform.SetParent(container);
            currentPrefab.transform.localPosition = localPos;
            Selection.activeObject = currentPrefab;
        }

        private void DragPerformAsUnPack(Transform container, Vector3 localPos)
        {
            GameObject currentPrefab = PrefabUtility.InstantiatePrefab(LoadPrefab) as GameObject;
            currentPrefab.transform.SetParent(container);
            currentPrefab.transform.localPosition = localPos;
            Selection.activeObject = currentPrefab;
            PrefabUtility.UnpackPrefabInstance(currentPrefab, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
        }

        private void DrawTexture(SceneView sceneView)
        {
            if (OutBounds(sceneView))
            {
                SceneViewUtils.RemoveDelegate(DrawTexture);
            }

            Handles.BeginGUI();
            GUI.DrawTexture(new Rect(Event.current.mousePosition.x - texture.width / 2, Event.current.mousePosition.y - texture.height / 2, texture.width, texture.height), texture);
            Handles.EndGUI();
            sceneView.Repaint();
        }

        private bool OutBounds(SceneView sceneView, float offset = 0f)
        {
            if (Event.current.mousePosition.y < sceneView.camera.pixelHeight + offset && Event.current.mousePosition.y > 0 && Event.current.mousePosition.x < sceneView.camera.pixelWidth &&
                Event.current.mousePosition.x > 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }


        private DragAndDropVisualMode OnHierarchyGUI(int dropTargetInstanceID, HierarchyDropFlags dropMode, Transform parentForDraggedObjects, bool perform)
        {
            return DragAndDrop.visualMode;
        }
    }
}