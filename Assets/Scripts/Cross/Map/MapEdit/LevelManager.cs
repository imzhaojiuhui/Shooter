using Ghost.Terrain;
using UnityEngine;

namespace Ghost.Edit
{
    public interface LevelManager
    {
        public PathUtils.WeightedUndirectedGraph Graph { get; }

        public bool OnLadderEnterRect(Vector2 pos, out Vector2 enterPos, out bool isUp);

        public bool OnLadderLeaveRect(Vector2 pos, out Vector2 downPos);
    }
}