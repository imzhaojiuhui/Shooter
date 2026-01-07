using KISS;
using UnityEngine;

namespace Ghost.Terrain
{
    [DisallowMultipleComponent]
    public abstract class Movement : MonoBehaviour
    {
        public float baseSpeed = 3;
        public Vector2 InputVelocity { get; set; }

        public float Speed
        {
            get { return baseSpeed; }
        }

        public virtual (PathUtils.WeightedGraphNode, PathUtils.WeightedGraphNode) AlongLine { get; }

        public bool OnGround
        {
            get
            {
                var alongLine = AlongLine;
                var line = new MathTool.Line2D(alongLine.Item1.WorldPos, alongLine.Item2.WorldPos);
                return line.Horizontal;
            }
        }
    }
}