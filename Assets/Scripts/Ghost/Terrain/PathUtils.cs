using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Ghost.Terrain
{
    public static class PathUtils
    {
        public class HLine
        {
            public Vector2 Start;
            private float _end;
            public bool Horizontal;
            public Vector2 End => Horizontal ? new Vector2(_end, Start.y) : new Vector2(Start.x, _end);
        }

        /// <summary>
        /// 带权重的边 - 无向图专用
        /// 存储：目标节点 + 当前节点到目标节点的权重
        /// </summary>
        public struct WeightedEdge
        {
            // 这条边连接的「目标节点」
            public int targetNodeId;

            // 这条边的权重 (你的场景=两点间线段距离，可自定义为步数/代价)
            public float weight;

            public WeightedEdge(int nodeId, float w)
            {
                targetNodeId = nodeId;
                weight = w;
            }
        }

        /// <summary>
        /// 带权重无向图的节点（顶点）
        /// 核心存储：节点ID + 世界坐标 + 邻接边集合（目标节点+权重）
        /// 附带：Dijkstra/A星寻路所需的全部辅助数据，无需额外定义类
        /// </summary>
        public class WeightedGraphNode
        {
            #region 一、图结构核心数据（必选）

            public int NodeId { get; private set; } // 节点唯一ID (必须唯一，用于索引)
            public Vector2 WorldPos { get; private set; } // 节点的世界坐标 (你的场景=线段交点/起点/终点坐标)
            public List<WeightedEdge> AdjacentEdges { get; } // 邻接边集合：当前节点能直接连通的所有节点+权重

            #endregion

            #region 二、寻路算法辅助数据（Dijkstra/A星通用，可选，初始化即可用）

            public float GCost { get; set; } // 起点到当前节点的累计权重（真实代价，Dijkstra核心）
            public float HCost { get; set; } // 当前节点到终点的预估权重（A星核心，Dijkstra用不到，设0即可）
            public float FCost => GCost + HCost; // A星总代价 F=G+H (只读)
            public WeightedGraphNode ParentNode { get; set; } // 父节点，用于回溯路径
            public bool IsVisited { get; set; } // 是否已访问，Dijkstra/A星通用标记

            #endregion

            #region 三、拓展字段（你的场景专属，按需开启）

            public bool IsObstacle { get; set; } = false; // 是否是障碍物节点（你的场景：不可走的线段交点，比如墙体）

            #endregion

            /// <summary>
            /// 构造函数：初始化带权重无向图节点
            /// </summary>
            /// <param name="id">节点唯一ID（从0开始自增即可）</param>
            /// <param name="pos">节点世界坐标</param>
            public WeightedGraphNode(int id, Vector2 pos)
            {
                NodeId = id;
                WorldPos = pos;
                AdjacentEdges = new List<WeightedEdge>();
                // 寻路数据初始化
                GCost = float.MaxValue;
                HCost = 0;
                ParentNode = null;
                IsVisited = false;
            }

            /// <summary>
            /// 无向图核心方法：给当前节点 添加一条「带权重的邻接边」
            /// 无向图特性：A加B的边，B必须加A的边，权重相同！
            /// </summary>
            public void AddAdjacentEdge(int targetNodeId, float weight)
            {
                AdjacentEdges.Add(new WeightedEdge(targetNodeId, weight));
            }
        }

        /// <summary>
        /// 带权重的无向图 完整数据结构核心类
        /// ✅ 核心特性：1.自动维护双向边 2.节点唯一ID索引 3.封装所有图操作 4.无缝适配Dijkstra/A星
        /// ✅ 你的场景：线段交点=节点，线段=带权边，权重=线段距离
        /// </summary>
        public class WeightedUndirectedGraph
        {
            // 存储图中所有节点：用字典<节点ID, 节点对象>，O(1)快速查找，效率拉满
            private Dictionary<int, WeightedGraphNode> _nodesDict;

            // 节点ID自增器：保证每个节点ID唯一
            private int _nodeIdCounter;

            public WeightedUndirectedGraph()
            {
                _nodesDict = new Dictionary<int, WeightedGraphNode>();
                _nodeIdCounter = 0;
            }

            /// <summary>
            /// 1. 添加节点到图中，自动分配唯一ID，返回创建的节点
            /// </summary>
            public WeightedGraphNode AddNode(Vector2 nodeWorldPos)
            {
                ClearPathFindCache();
                WeightedGraphNode newNode = new WeightedGraphNode(_nodeIdCounter, nodeWorldPos);
                _nodesDict.Add(_nodeIdCounter, newNode);
                _nodeIdCounter++;
                return newNode;
            }

            /// <summary>
            /// 2. 无向图核心：添加「带权重的无向边」
            /// ✅ 核心：自动给两个节点互相添加邻接边，权重完全相等，无需手动添加双向边
            /// </summary>
            /// <param name="nodeA">节点A</param>
            /// <param name="nodeB">节点B</param>
            /// <param name="weight">边的权重（你的场景=两点间线段距离）</param>
            public void AddWeightedEdge(WeightedGraphNode nodeA, WeightedGraphNode nodeB, float weight)
            {
                ClearPathFindCache();
                if (nodeA == null || nodeB == null) return;
                if (weight < 0) weight = 0; // 权重不能为负（距离/代价不可能为负）
                nodeA.AddAdjacentEdge(nodeB.NodeId, weight);
                nodeB.AddAdjacentEdge(nodeA.NodeId, weight);
            }

            public void AddWeightedEdge(WeightedGraphNode nodeA, WeightedGraphNode nodeB)
            {
                AddWeightedEdge(nodeA, nodeB, Vector2.Distance(nodeA.WorldPos, nodeB.WorldPos));
            }

            /// <summary>
            /// 3. 根据节点ID获取节点（O(1)查找）
            /// </summary>
            public WeightedGraphNode GetNodeById(int nodeId)
            {
                if (_nodesDict.ContainsKey(nodeId))
                {
                    return _nodesDict[nodeId];
                }

                Debug.LogWarning($"图中不存在ID为{nodeId}的节点！");
                return null;
            }

            /// <summary>
            /// 4. 获取图中所有节点
            /// </summary>
            public List<WeightedGraphNode> GetAllNodes()
            {
                return new List<WeightedGraphNode>(_nodesDict.Values);
            }

            /// <summary>
            /// 5. 重置所有节点的寻路辅助数据（关键！每次寻路前必须调用）
            /// 重置后可重复使用图结构，多次寻路无残留数据
            /// </summary>
            public void ResetPathfindingData()
            {
                foreach (var node in _nodesDict.Values)
                {
                    node.GCost = float.MaxValue;
                    node.HCost = 0;
                    node.ParentNode = null;
                    node.IsVisited = false;
                }
            }

            /// <summary>
            /// 6. 清空整个图
            /// </summary>
            public void ClearGraph()
            {
                ClearPathFindCache();
                _nodesDict.Clear();
                _nodeIdCounter = 0;
            }

            #region 寻路cache

            /// <summary>
            /// 单条路径的缓存项 - 存储：路径点集合 + 路径总权重
            /// </summary>
            private class PathCacheItem
            {
                // 从起点到该终点的最短路径点（WorldPos，顺序：起点→终点）
                public List<int> PathNodeIds { get; set; }

                // 该路径的总权重（所有边的权重之和）
                public float TotalWeight { get; set; }

                public PathCacheItem(List<int> nodeIds, float weight)
                {
                    PathNodeIds = nodeIds;
                    TotalWeight = weight;
                }

                public IEnumerable<int> ReversePathNodeIds()
                {
                    for (int i = PathNodeIds.Count - 1; i >= 0; i--)
                    {
                        yield return PathNodeIds[i];
                    }
                }
            }

            private Dictionary<(int, int), PathCacheItem> _pathFindCache = new();

            private PathCacheItem GetPathFindCache(int startNodeId, int endNodeId)
            {
                if (_pathFindCache.ContainsKey((startNodeId, endNodeId)))
                {
                    return _pathFindCache[(startNodeId, endNodeId)];
                }

                return _pathFindCache.GetValueOrDefault((endNodeId, startNodeId));
            }

            private void ClearPathFindCache()
            {
                _pathFindCache.Clear();
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="startNode"></param>
            /// <param name="endNode"></param>
            /// <returns>最短路径的集合（起点→拐点→终点），无路径返回空列表</returns>
            public IEnumerable<int> FindShortestPath(WeightedGraphNode startNode,
                WeightedGraphNode endNode)
            {
                var cache = GetPathFindCache(startNode.NodeId, endNode.NodeId);
                if (cache == null)
                {
                    // 没找到缓存
                }
                else if (cache.PathNodeIds[0] == endNode.NodeId)
                {
                    return cache.ReversePathNodeIds();
                }
                else
                {
                    return cache.PathNodeIds;
                }

                var (path, gCost) = FindShortestPathDijkstra(startNode, endNode);
                // FindShortestPathDijkstra计算留下的GCost收集到缓存
                foreach (var node in _nodesDict.Values)
                {
                    if (node.GCost == float.MaxValue) continue;
                    List<int> nodePathIds = new List<int>();
                    WeightedGraphNode tempNode = node;
                    while (tempNode != null)
                    {
                        nodePathIds.Add(tempNode.NodeId); // 收集ID
                        tempNode = tempNode.ParentNode;
                    }

                    nodePathIds.Reverse(); // 反转：终点ID→起点ID → 起点ID→终点ID
                    _pathFindCache.Add((startNode.NodeId, node.NodeId),
                        new PathCacheItem(nodePathIds, node.GCost)); //是否会重复add？
                }

                if (path.Count == 0)
                {
                    _pathFindCache.Add((startNode.NodeId, endNode.NodeId), new PathCacheItem(path, gCost));
                }

                return path;
            }

            #endregion

            #region 寻路

            /// <summary>
            /// Dijkstra核心寻路方法
            /// </summary>
            /// <param name="graph">带权重无向图</param>
            /// <param name="startNode">起点节点</param>
            /// <param name="endNode">终点节点</param>
            /// <returns>最短路径的集合（起点→拐点→终点），无路径返回空列表 最短距离</returns>
            private (List<int>, float) FindShortestPathDijkstra(WeightedGraphNode startNode,
                WeightedGraphNode endNode)
            {
                List<int> path = new();
                if (startNode == null || endNode == null) return (path, float.MaxValue);
                if (startNode == endNode)
                {
                    path.Add(startNode.NodeId);
                    return (path, float.MaxValue);
                }

                // 每次寻路前重置所有节点的寻路数据，关键！
                ResetPathfindingData();
                // 单源起点初始化：起点到自己的权重为0
                startNode.GCost = 0;

                // 获取图中所有节点，用于循环筛选
                List<WeightedGraphNode> allNodes = GetAllNodes();

                while (true)
                {
                    // 步骤1：找到【未访问】且【GCost最小】的节点（Dijkstra核心，单源的体现）
                    WeightedGraphNode currentNode = allNodes
                        .Where(n => !n.IsVisited && n.GCost < float.MaxValue)
                        .OrderBy(n => n.GCost)
                        .FirstOrDefault();

                    if (currentNode == null) break; // 无路径，退出循环
                    if (currentNode == endNode) break; // 找到终点，退出循环

                    currentNode.IsVisited = true;

                    // 步骤2：遍历当前节点的所有邻接边，更新邻居节点的GCost
                    foreach (var edge in currentNode.AdjacentEdges)
                    {
                        WeightedGraphNode neighborNode = GetNodeById(edge.targetNodeId);
                        if (neighborNode == null || neighborNode.IsVisited || neighborNode.IsObstacle) continue;

                        // 新权重 = 起点到当前节点的权重 + 当前节点到邻居的权重
                        float newGCost = currentNode.GCost + edge.weight;
                        // 如果新权重更小，更新邻居的权重和父节点
                        if (newGCost < neighborNode.GCost)
                        {
                            neighborNode.GCost = newGCost;
                            neighborNode.ParentNode = currentNode;
                        }
                    }
                }

                // 步骤3：回溯父节点，生成路径
                if (endNode.GCost == float.MaxValue)
                {
                    Debug.LogWarning("Dijkstra寻路失败：无可行路径！");
                    return (path, float.MaxValue);
                }

                WeightedGraphNode tempNode = endNode;
                while (tempNode != null)
                {
                    path.Add(tempNode.NodeId);
                    tempNode = tempNode.ParentNode;
                }

                path.Reverse(); // 反转：终点→起点 → 起点→终点
                return (path, endNode.GCost);
            }

            #endregion
        }
    }
}