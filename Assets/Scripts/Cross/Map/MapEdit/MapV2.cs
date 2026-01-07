using System;
using System.Collections.Generic;
using System.Linq;
using Ghost.Terrain;
using KISS;
using UnityEngine;

namespace Ghost.Edit
{
    [DisallowMultipleComponent]
    public class MapV2 : MonoSingleton<MapV2>, IMap
    {
        public float doorWidth = .2f;
        public float nodeEpsilon = 1e-6f;

        // private enum NodeType
        // {
        //     Normal,
        //     Door,
        // }

        private void Awake()
        {
            var floors = GetComponentsInChildren<EditFloor>();
            var ladders = GetComponentsInChildren<EditLadder>();

            WeightedUndirectedGraph graph = new();
            foreach (var floor in floors)
            {
                var nodesOnFloor = new List<(Vector2, EditDoor)>();
                nodesOnFloor.Add((floor.Start, null));
                nodesOnFloor.Add((floor.End, null));

                var doors = floor.GetComponentsInChildren<EditDoor>();
                foreach (var door in doors)
                {
                    nodesOnFloor.Add((door.DoorPos, door));
                }

                var adjLadders = ladders.Where(l => l.floorA == floor.transform || l.floorB == floor.transform)
                    .ToList();
                foreach (var adjLadder in adjLadders)
                {
                    nodesOnFloor.Add((new Vector2(adjLadder.transform.position.x, floor.transform.position.y),
                        null));
                }

                WeightedGraphNode preNode = null;
                var nodesL2R = nodesOnFloor.OrderBy(n => Vector2.Distance(floor.Start, n.Item1));
                foreach (var (pos, door) in nodesL2R)
                {
                    if (door != null)
                    {
                        var nodeL = graph.EnsureNode(pos + Vector2.left * doorWidth / 2, nodeEpsilon);
                        var nodeR = graph.EnsureNode(pos + Vector2.right * doorWidth / 2, nodeEpsilon);

                        var doorId = _doorIdCnt++;
                        _doorNodeLR[doorId] = (nodeL, nodeR);
                        door.doorId = doorId;

                        if (preNode != null)
                        {
                            graph.AddEdge(preNode, nodeL);
                        }

                        preNode = nodeR;
                        continue;
                    }

                    var node = graph.EnsureNode(pos, nodeEpsilon);
                    if (preNode != null)
                    {
                        graph.AddEdge(preNode, node);
                    }

                    preNode = node;
                }
            }

            foreach (var ladder in ladders)
            {
                var nodeA = graph.EnsureNode(ladder.PosHigh, nodeEpsilon);
                var nodeB = graph.EnsureNode(ladder.PosLow, nodeEpsilon);
                // nodeA ??= graph.AddNode(ladder.PosHigh);
                // nodeB ??= graph.AddNode(ladder.PosLow);

                graph.AddEdge(nodeA, nodeB);
            }

            _enterLadderRects.Clear();
            _leaveLadderRects.Clear();
            foreach (var ladder in ladders)
            {
                _enterLadderRects.Add((ladder.EnterUpRect, ladder.PosLow, true));
                _enterLadderRects.Add((ladder.EnterDownRect, ladder.PosHigh, false));

                _leaveLadderRects.Add((ladder.LeaveUpRect, ladder.PosHigh));
                _leaveLadderRects.Add((ladder.LeaveDownRect, ladder.PosLow));
            }

            Graph = graph;
        }

        private readonly List<(Rect, Vector2, bool)> _enterLadderRects = new(); // rect, up
        private readonly List<(Rect, Vector2)> _leaveLadderRects = new(); // rect, point
        private int _doorIdCnt;
        private Dictionary<int, (WeightedGraphNode, WeightedGraphNode)> _doorNodeLR = new();

        public WeightedUndirectedGraph Graph { get; private set; }

        public IEnumerable<(Vector2, bool)> OnLadderEnterRect(Vector2 pos)
        {
            foreach (var (rect, enterPos, up) in _enterLadderRects)
            {
                if (rect.Contains(pos))
                {
                    yield return (enterPos, up);
                }
            }

            yield break;
        }

        public IEnumerable<Vector2> OnLadderLeaveRect(Vector2 pos)
        {
            foreach (var (rect, leavePos) in _leaveLadderRects)
            {
                if (rect.Contains(pos))
                {
                    yield return leavePos;
                }
            }

            yield break;
        }

        public void UnlockDoor(int doorId)
        {
            var (l, r) = _doorNodeLR[doorId];
            Graph.EnsureEdge(l, r);
            _graphChanged = true;
        }

        public event Action AfterGraphChanged;

        private bool _graphChanged;

        private void LateUpdate()
        {
            if (_graphChanged)
            {
                AfterGraphChanged?.Invoke();
                _graphChanged = false;
            }
        }
    }
}