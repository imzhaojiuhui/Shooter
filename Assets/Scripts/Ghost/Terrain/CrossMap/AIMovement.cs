using System.Collections;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Ghost.Terrain
{
    public class AIMovement : MonoBehaviour
    {
        private IEnumerator Start()
        {
            var targetMark = transform.GetChild(0);
            var fromMark = transform.GetChild(1);
            targetMark.SetParent(this.transform.parent);
            fromMark.SetParent(this.transform.parent);
            yield return new WaitForSeconds(1);
            var graph = CrossMap.Instance.Graph;
            var allNodes = CrossMap.Instance.Graph.GetAllNodes();
            while (true)
            {
                var from = graph.GetNearestNode(this.transform.position);
                var to = allNodes[Random.Range(0, allNodes.Count)];
                var path = graph.FindShortestPath(from, to).ToList();
                targetMark.position = to.WorldPos;
                fromMark.position = from.WorldPos;

                foreach (var nodeId in path)
                {
                    var node = graph.GetNodeById(nodeId);
                    var dir = node.WorldPos - (Vector2)this.transform.position;
                    if (dir.sqrMagnitude < 1e-6)
                    {
                        // 已到达node
                        continue;
                    }

                    dir = dir.normalized;

                    while (true)
                    {
                        var toPos = this.transform.position + (Vector3)dir * 3 * Time.deltaTime;
                        var relativePos = (Vector2)toPos - node.WorldPos;
                        var dot = Vector2.Dot(dir, relativePos);
                        if (dot > 0) // toPos超过node
                        {
                            break;
                        }

                        this.transform.position = toPos;
                        yield return null;
                    }
                }
                // if (MathTool.IsPointOnLine(this.transform.position, new MathTool.Line2D(from.WorldPos, to.WorldPos)))
                // {
                //     
                // }
            }
        }

        // private UniTask Movement()
        // {
        //     
        // }
    }
}