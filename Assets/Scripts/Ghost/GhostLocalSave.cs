using System;
using System.Collections.Generic;
using KISS;
using UnityEngine;

namespace Ghost
{
    [Serializable]
    public class RoadLine
    {
        public float[] start;
        public float[] end;
    }

    [DisallowMultipleComponent]
    public class GhostLocalSave: MonoSingleton<GhostLocalSave>
    {
        protected override void Awake()
        {
            base.Awake();
            LocalSave.Instance.Load(0);
        }

        public List<RoadLine> GetRoadLines()
        {
            return LocalSave.Instance.GetList<RoadLine>("RoadLines");
        }

        public void SaveRoadLines(List<RoadLine> roadLines)
        {
            LocalSave.Instance.Save("RoadLines", roadLines);
        }
    }
}