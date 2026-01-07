using System.Collections.Generic;
using Ghost.Terrain;
using UnityEngine;

namespace Ghost.Edit
{
    public interface LevelManager
    {
        public PathUtils.WeightedUndirectedGraph Graph { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="enterPos"></param>
        /// <param name="isUp"></param>
        /// <returns>(enterPos, isUp)</returns>
        public IEnumerable<(Vector2, bool)> OnLadderEnterRect(Vector2 pos);

        public IEnumerable<Vector2> OnLadderLeaveRect(Vector2 pos);
    }
}