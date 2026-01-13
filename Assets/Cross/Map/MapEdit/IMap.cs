using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ghost.Edit
{
    public interface IMap
    {
        public WeightedUndirectedGraph Graph { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="enterPos"></param>
        /// <param name="isUp"></param>
        /// <returns>(enterPos, isUp)</returns>
        public IEnumerable<(Vector2, bool)> OnLadderEnterRect(Vector2 pos);

        public IEnumerable<Vector2> OnLadderLeaveRect(Vector2 pos);

        public void UnlockDoor(int doorId);

        public event Action AfterGraphChanged;
    }
}