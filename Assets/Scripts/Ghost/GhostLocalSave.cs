using System;
using System.Collections.Generic;
using KISS;
using UnityEngine;

namespace Ghost
{
    [Serializable]
    public class LineSave
    {
        public float[] start;
        public float[] end;

        // public bool Horizontal => Mathf.Abs(start[0] - end[0]) < float.Epsilon;
        public bool Horizontal
        {
            get
            {
                var dir = end.ToVector2() - start.ToVector2();
                return Mathf.Abs(dir.x) > Mathf.Abs(dir.y);
            }
        }

        // public CrossLine ToCrossLine()
        // {
        //     return new CrossLine()
        //     {
        //
        //     };
        // }

        public MathTool.Line2D ToLine2D()
        {
            return new MathTool.Line2D(start.ToVector2(), end.ToVector2());
        }
    }

    [DisallowMultipleComponent]
    public class GhostLocalSave : MonoSingleton<GhostLocalSave>
    {
        protected override void Awake()
        {
            base.Awake();
            LocalSave.Instance.Load(0);
        }

        public List<LineSave> GetRoadLines()
        {
            return LocalSave.Instance.GetList<LineSave>("RoadLines");
        }

        public void SaveRoadLines(List<LineSave> roadLines)
        {
            LocalSave.Instance.Save("RoadLines", roadLines);
        }
    }
}