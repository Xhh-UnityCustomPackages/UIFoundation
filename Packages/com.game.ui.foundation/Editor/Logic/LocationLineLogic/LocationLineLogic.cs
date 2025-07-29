using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UIElements;

namespace Game.UI.Foundation.Editor
{
    public enum CreateLineType
    {
        Both,
        Vertical,
        Horizon
    }

    public class LocationLineLogic : UIToolBarLogic<LocationLineLogic>
    {
        public static float sceneviewOffset;

        //所有的辅助线数据
        private LocationLinesData m_LinesData;

        //所有辅助线对象 应该和上面的数据是一致的
        private List<LocationLine> m_LinesList;

        public override void Open()
        {
            sceneviewOffset = 0;
            m_LinesData = new();
            m_LinesList = new();

            PrefabStage.prefabStageOpened += PrefabStageChange;
            PrefabStage.prefabStageClosing += PrefabStageChange;
        }

        public override void Close()
        {
            Clear();
            PrefabStage.prefabStageOpened -= PrefabStageChange;
            PrefabStage.prefabStageClosing -= PrefabStageChange;
        }

        public void Clear()
        {
            m_LinesData.Clear();

            if (m_LinesList != null)
            {
                foreach (var line in m_LinesList)
                {
                    if (SceneView.lastActiveSceneView.rootVisualElement.Contains(line))
                    {
                        SceneView.lastActiveSceneView.rootVisualElement.Remove(line);
                    }
                }

                m_LinesList.Clear();
            }
        }

        private void PrefabStageChange(PrefabStage p)
        {
            Clear();
        }

        public void CreateLocationLine(CreateLineType createType)
        {
            SceneView sceneView = SceneView.lastActiveSceneView;
            Vector3 worldPostion =
                    sceneView.camera.ScreenToWorldPoint(new(sceneView.camera.pixelWidth / 2, (sceneView.camera.pixelHeight - 40) / 2, 0));

            int curId = m_LinesData.LastLineId + 1;
            LocationLineData horzLineData = null;
            LocationLineData vertLineData = null;
            LocationLine horzLine = null;
            LocationLine vertLine = null;

            if (createType == CreateLineType.Both || createType == CreateLineType.Horizon)
            {
                horzLineData = new LocationLineData()
                {
                    Id = curId,
                    Horizontal = true,
                    Pos = worldPostion.y
                };
                horzLine = new HorizontalLocationLine
                {
                    id = horzLineData.Id,
                    worldPostion = new Vector3(0, horzLineData.Pos, 0)
                };
            }

            if (createType == CreateLineType.Both || createType == CreateLineType.Vertical)
            {
                vertLineData = new LocationLineData()
                {
                    Id = curId + 1,
                    Horizontal = false,
                    Pos = worldPostion.x,
                };
                vertLine = new VerticalLocationLine
                {
                    id = vertLineData.Id,
                    worldPostion = new Vector3(vertLineData.Pos, 0, 0)
                };
            }

            if (createType == CreateLineType.Both)
            {
                m_LinesData.Add(horzLineData);
                m_LinesData.Add(vertLineData);
                PlaceLinesToSceneView(new List<LocationLine> { horzLine, vertLine });
            }
            else if (createType == CreateLineType.Horizon)
            {
                m_LinesData.Add(horzLineData);
                PlaceLinesToSceneView(new List<LocationLine> { horzLine });
            }
            else if (createType == CreateLineType.Vertical)
            {
                m_LinesData.Add(vertLineData);
                PlaceLinesToSceneView(new List<LocationLine> { vertLine });
            }
        }

        private void PlaceLinesToSceneView(List<LocationLine> lines)
        {
            SceneView sceneView = SceneView.lastActiveSceneView;
            if (sceneView == null) return;

            VisualElement firstChild = sceneView.rootVisualElement.Children().First();
            foreach (LocationLine line in lines)
            {
                sceneView.rootVisualElement.Add(line);
                line.PlaceInFront(firstChild);
                m_LinesList.Add(line);
            }
        }
        
        public void ModifyLine(LocationLine line)
        {
            // LocationLineCommand cmd = new LocationLineCommand(m_LinesData, "Modify LocationLine");
            // cmd.Execute();

            LocationLineData lineData = new LocationLineData()
            {
                Id = line.id,
                Horizontal = line.direction == LocationLineDirection.Horizontal,
                Pos = line.direction == LocationLineDirection.Horizontal ? line.worldPostion.y : line.worldPostion.x,
            };
            m_LinesData.Modify(lineData);
        }
    }
}