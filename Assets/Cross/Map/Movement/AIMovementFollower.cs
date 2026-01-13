using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Ghost.Terrain
{
    public class AIMovementFollower : MonoBehaviour, GroundMovement, PosInGraph
    {
        public float baseSpeed = 1f;
        public float Speed => baseSpeed;

        public float stayDistance = 1f;

        private bool _onGround;

        // private PathUtils.WeightedGraphNode _edgeFrom;
        // private PathUtils.WeightedGraphNode _edgeTo;
        private List<Vector2> _path = new List<Vector2>();

        private IMap _map;

        private IEnumerator Start()
        {
            _map = MapV2.Instance;
            var (edgeFrom, edgeTo, pos) =
                _map.Graph.GetNearestEdgePos(this.transform.position);
            this.transform.position = pos;
            _onGround = new MathTool.Line2D(edgeFrom.WorldPos, edgeTo.WorldPos).Horizontal;
            // _edgeFrom = edgeFrom;
            // _edgeTo = edgeTo;
            yield return new WaitForSeconds(1f);
            while (true)
            {
                var path = GenPath();
                _path = path.Select(p => p.Item1).ToList();
                _pathIndex = 0;
                yield return new WaitForSeconds(.2f);
            }
        }

        private int _pathIndex = 0;

        private void Update()
        {
            if (_path.Count <= 0)
            {
                return;
            }

            var des = _path[^1];
            if (Vector2.Distance(des, transform.position) <= stayDistance)
            {
                return;
            }

            var forward = Speed * Time.deltaTime;
            for (; _pathIndex < _path.Count; _pathIndex++)
            {
                var next = _path[_pathIndex];
                var toNext = next - (Vector2)this.transform.position;

                if (toNext.magnitude < forward)
                {
                    this.transform.position = next;
                    forward -= toNext.magnitude;
                    continue;
                }

                var dir = toNext.normalized;
                _onGround = Mathf.Abs(dir.x) > Mathf.Abs(dir.y);
                this.transform.position += forward * (Vector3)dir;
                return;
            }
        }

        private IEnumerable<(Vector2, WeightedGraphNode)> GenPath()
        {
            var graph = _map.Graph;
            var from = transform.position;
            // var to = Character.Instance.transform.position;
            var (to, toEdgeA, toEdgeB) = Character.Instance.GroundMovement.QueryPosAndEdge(graph);
            // var (toEdgeA, toEdgeB, pos) = graph.GetNearestEdgePos(to);
            var curEdge = _map.Graph.GetEdgeByPoint(this.transform.position, _onGround);
            WeightedGraphNode fromEdgeA, fromEdgeB;
            if (curEdge == null)
            {
                (fromEdgeA, fromEdgeB, _) = _map.Graph.GetNearestEdgePos(this.transform.position);
            }
            else
            {
                (fromEdgeA, fromEdgeB) = curEdge.Value;
            }


            var path = graph.FindShortestPath(from, to,
                (fromEdgeA, fromEdgeB), (toEdgeA, toEdgeB));
            return path;
        }

        public (Vector2, WeightedGraphNode, WeightedGraphNode) QueryPosAndEdge(WeightedUndirectedGraph graph)
        {
            var curEdge = graph.GetEdgeByPoint(this.transform.position, _onGround);
            if (curEdge != null)
            {
                return (transform.position, curEdge.Value.Item1, curEdge.Value.Item2);
            }

            var (from, to, pos) = graph.GetNearestEdgePos(this.transform.position);
            return (pos, from, to);
        }
    }
}