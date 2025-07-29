using System;
using System.Collections.Generic;
using System.Linq;

namespace Game.UI.Foundation.Editor
{
    [Serializable]
    public class LocationLineData
    {
        public int Id;
        public bool Horizontal;
        public float Pos;
    }

    [Serializable]
    public class LocationLinesData
    {
        public List<LocationLineData> List = new();

        public int LastLineId => List.Count > 0 ? List.Max(data => data.Id) : 0;

        public void Add(LocationLineData line)
        {
            List.Add(line);
        }

        public void Remove(int id)
        {
            var index = List.FindIndex(l => id == l.Id);
            if (index >= 0)
            {
                List.RemoveAt(index);
            }
        }

        public void Clear()
        {
            List.Clear();
        }

        public void Modify(LocationLineData line)
        {
            var index = List.FindIndex(l => line.Id == l.Id);
            if (index >= 0)
            {
                List[index].Horizontal = line.Horizontal;
                List[index].Pos = line.Pos;
            }
        }
    }
}