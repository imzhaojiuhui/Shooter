using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Ghost.Edit;
using KISS;
using UnityEngine;

namespace Ghost.Terrain
{
    public class AIMovementFollower : MonoBehaviour, GroundMovement
    {
        public float baseSpeed = 1f;

        public float Speed => baseSpeed;
        // private IEnumerator Start()
        // {
        //     var graph = _map.Graph;
        //     while (true)
        //     {
        //         var from = transform.position;
        //         var to = Character.Instance.transform.position;
        //         
        //         var fromNode = graph.GetNearestNode(transform.position);
        //         var toNode = graph.GetNearestNode(to);
        //         
        //         if (fromNode == toNode)
        //         {
        //             transform.position = (to-from).normalized*Time.deltaTime;
        //             yield return null;
        //             break;
        //         }
        //         
        //         {
        //             var path = graph.FindShortestPath(fromNode, toNode);
        //             foreach (var nodeId in path)
        //             {
        //                 var node = graph.GetNodeById(nodeId);
        //                 var dir = node.WorldPos - (Vector2)this.transform.position;
        //                 if (dir.sqrMagnitude < 1e-6)
        //                 {
        //                     // 已到达node
        //                     continue;
        //                 }
        //
        //                 dir = dir.normalized;
        //
        //                 while (true)
        //                 {
        //                     var toPos = this.transform.position + (Vector3)dir * 3 * Time.deltaTime;
        //                     var relativePos = (Vector2)toPos - node.WorldPos;
        //                     var dot = Vector2.Dot(dir, relativePos);
        //                     if (dot > 0) // toPos超过node
        //                     {
        //                         break;
        //                     }
        //
        //                     this.transform.position = toPos;
        //                     yield return null;
        //                 }
        //             }
        //         }
        //     }
        // }

        private bool _onGround;

        // private PathUtils.WeightedGraphNode _edgeFrom;
        // private PathUtils.WeightedGraphNode _edgeTo;
        private List<Vector2> _path = new List<Vector2>();

        private LevelManager _map;

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
                yield return new WaitForSeconds(2f);
            }
        }

        private int _pathIndex = 0;

        private void Update()
        {
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

        // private void Update()
        // {
        //     var forward = 1f * Time.fixedDeltaTime;
        //     // var preNode = fromEdgeA;
        //     foreach (var p in _path)
        //     {
        //         var toNext = p - (Vector2)this.transform.position;
        //         // if (toNext.magnitude < 0.1)
        //         // {
        //         //     // if (node != fromEdgeA &&  node != fromEdgeB)
        //         //     // {
        //         //     //     preNode = node;
        //         //     // }
        //         //     // 已到达node
        //         //     // this.transform.position = p;
        //         //     continue;
        //         // }
        //
        //         if (toNext.magnitude < forward)
        //         {
        //             this.transform.position = p;
        //             forward -= toNext.magnitude;
        //             continue;
        //         }
        //
        //         var dir = toNext.normalized;
        //         _onGround = Mathf.Abs(dir.x) > Mathf.Abs(dir.y);
        //         this.transform.position += forward * (Vector3)dir;
        //         // if (node != _edgeFrom &&  node != _edgeTo)
        //         // {
        //         //     if (node == null) // end
        //         //     {
        //         //         _edgeFrom = toEdgeA;
        //         //         _edgeTo = toEdgeB;
        //         //     }
        //         //     else if (preNode == _edgeFrom)
        //         //     {
        //         //         _edgeTo =  node;
        //         //     }
        //         //     else if (preNode == _edgeTo)
        //         //     {
        //         //         _edgeFrom = node;
        //         //     }
        //         //     else
        //         //     {
        //         //         _edgeFrom = preNode;
        //         //         _edgeTo = node;
        //         //     }
        //         // }
        //         return;
        //     }
        // }

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
    }
}