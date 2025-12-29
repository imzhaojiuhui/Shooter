using System.Collections.Generic;
using KISS;
using UnityEngine;

namespace Ghost.Terrain
{
    [DisallowMultipleComponent]
    public class RoadMap: MonoSingleton<RoadMap>
    {
        [Header("2D画线配置")]
        public Color lineColor = Color.red;
        public float lineWidth = 0.1f;
        
        protected override void Awake()
        {
            base.Awake();
            var lines = GhostLocalSave.Instance.GetRoadLines();
            foreach (var line in lines)
            {
                DrawNewLine(line.start.ToVector2(), line.end.ToVector2());
            }
            RoadLines = lines;
        }

        public List<RoadLine> RoadLines { get; private set; }

        private int _lineId;
        private GameObject DrawNewLine(Vector2 start, Vector2 realEndPos)
        {
            GameObject lineObj = new GameObject($"2D_Line_{_lineId++}");
            lineObj.transform.SetParent(this.transform);
            LineRenderer lr = lineObj.AddComponent<LineRenderer>();
            lr.material = new Material(Shader.Find("Sprites/Default"));
            lr.startColor = lineColor;
            lr.endColor = lineColor;
            lr.startWidth = lineWidth;
            lr.endWidth = lineWidth;
            lr.positionCount = 2;
            lr.SetPosition(0, start);
            lr.SetPosition(1, realEndPos);
        
            return lineObj;
        }
    }
}