using Ghost.Terrain;
using UnityEngine;

namespace SAW
{
    /// <summary>
    /// 寻路图中位置
    /// </summary>
    public interface PosInGraph
    {
        public (Vector2, WeightedGraphNode, WeightedGraphNode) QueryPosAndEdge(
            WeightedUndirectedGraph graph);
    }
}