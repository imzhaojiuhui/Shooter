using System.Collections.Generic;
using System.Linq;
using KISS;
using UnityEngine;

namespace Ghost.Terrain
{
    [DisallowMultipleComponent]
    public class CrossMap : MonoSingleton<CrossMap>
    {
        // [Header("画线开始颜色")]
        // public Color lineColorFrom = Color.black;
        // [Header("画线结束颜色")] public Color lineColor = Color.red;
        public float lineWidth = 0.1f;

        // [Header("忽略roadLine毛刺")] public float burrTrimLen = .5f; //修剪roadLine毛刺

        protected override void Awake()
        {
            base.Awake();
            var lines = GhostLocalSave.Instance.GetRoadLines().Select(l => l.ToLine2D()).ToList();
            // _lines = lines;
            // foreach (var line in lines)
            // {
            //     DrawNewLine(line.start, line.end);
            // }

            WeightedUndirectedGraph graph = new();

            for (int i = 0; i < lines.Count; i++)
            {
                var lineA = lines[i];
                List<Vector2> nodesPosOnLine = new List<Vector2>();
                nodesPosOnLine.Add(lineA.start);
                nodesPosOnLine.Add(lineA.end);
                for (int j = 0; j < lines.Count; j++)
                {
                    if (i == j)
                    {
                        continue;
                    }

                    var lineB = lines[j];

                    if (MathTool.IsTwoLinesIntersect(lineA, lineB, out Vector2 point))
                    {
                        nodesPosOnLine.Add(point); // intersection

                        float enterWidth = 2f;
                        var vLine = lineA.Horizontal ? lineB : lineA;
                        Debug.Assert(!vLine.Horizontal);

                        var yMax = Mathf.Max(vLine.start.y, vLine.end.y);
                        var yMin = Mathf.Min(vLine.start.y, vLine.end.y);
                        // var stairDir = Vector2.down;
                        bool up = false;
                        var downRectYMin = point.y - enterWidth + .01f;
                        if (yMax - point.y > point.y - yMin)
                        {
                            up = true;
                            // stairDir = Vector2.up;
                            downRectYMin = point.y - .01f;
                        }

                        var climbRect = new Rect(point.x - enterWidth / 2, point.y - .1f, enterWidth, .2f);
                        var downRect = new Rect(point.x - .1f, downRectYMin, .2f, enterWidth);

                        _climbRects.Add((climbRect, up));
                        _downRects.Add((downRect, point));
                    }
                }

                WeightedGraphNode preNode = null;
                // List<PathUtils.WeightedGraphNode> nodesOnLine = new();
                foreach (var p in nodesPosOnLine.OrderBy(n => Vector2.Distance(lineA.start, n)))
                {
                    var node = graph.GetNearestNodeWithIn(p, .5f); // epsilon = .5 视为一个点
                    if (node == null)
                    {
                        node = graph.AddNode(p);
                    }

                    // nodesOnLine.Add(node);
                    if (preNode != null)
                    {
                        graph.AddEdge(preNode, node);
                        DrawNewLine(preNode.WorldPos, node.WorldPos);
                    }

                    preNode = node;
                }
            }

            Graph = graph;
            // allNodes = MathUtils.RemoveRepeatPoints(allNodes, 1f);
        }

        public WeightedUndirectedGraph Graph { get; private set; }

        private readonly List<(Rect, bool)> _climbRects = new(); // rect, up
        private readonly List<(Rect, Vector2)> _downRects = new(); // rect, point


        public bool EnterClimbRect(Vector2 pos, out Rect climbRect, out bool isUp)
        {
            foreach (var (rect, up) in _climbRects)
            {
                if (rect.Contains(pos))
                {
                    climbRect = rect;
                    isUp = up;
                    return true;
                }
            }

            climbRect = Rect.zero;
            isUp = false;
            return false;
        }

        public bool EnterDownRect(Vector2 pos, out Rect downRect, out Vector2 downPos)
        {
            foreach (var (rect, p) in _downRects)
            {
                if (rect.Contains(pos))
                {
                    downRect = rect;
                    downPos = p;
                    return true;
                }
            }

            downRect = Rect.zero;
            downPos = Vector2.zero;
            return false;
        }

        // private List<MathUtils.Line2D> _lines;
        //
        // public Vector2 GetNearestCrossPos(Vector2 point)
        // {
        //     float minDistance = float.MaxValue;
        //     Vector2 closest = Vector2.zero;
        //     foreach (var line in _lines)
        //     {
        //         var dis = MathUtils.PointToLineSegmentDistance(point, 
        //             line.start,  line.end,  out var curClosest);
        //         if (dis < float.Epsilon)
        //         {
        //             return curClosest;
        //         }
        //             
        //         if (dis < minDistance)
        //         {
        //             minDistance = dis;
        //             closest = curClosest;
        //         }
        //     }
        //     return closest;
        // }

        // private void FillGroup(PathUtils.WeightedUndirectedGraph graph, )

        private int _lineId;

        private GameObject DrawNewLine(Vector2 start, Vector2 realEndPos)
        {
            var lineId = _lineId++;
            GameObject lineObj = new GameObject($"2D_Line_{lineId}");
            lineObj.transform.SetParent(this.transform);
            LineRenderer lr = lineObj.AddComponent<LineRenderer>();
            lr.material = new Material(Shader.Find("Sprites/Default"));

            Color color = new Color(Random.Range(0, 1.0f), Random.Range(0, 1.0f), Random.Range(0, 1.0f));
            lr.startColor = color;
            lr.endColor = color;
            lr.startWidth = lineWidth;
            lr.endWidth = lineWidth;
            lr.positionCount = 2;
            lr.SetPosition(0, start);
            lr.SetPosition(1, realEndPos);

            return lineObj;
        }
    }
}