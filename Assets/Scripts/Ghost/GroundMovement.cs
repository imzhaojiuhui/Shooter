using System;
using Ghost.Terrain;
using KISS;
using UnityEngine;

namespace Ghost
{
    public class GroundMovement:MonoBehaviour
    {
        public Vector2 Velocity {get; set; }
        public float Speed {get; set; }

        private void Start()
        {
            Speed = 1;
        }

        private void Update()
        {
            transform.Translate(Velocity*Speed*Time.deltaTime);
            if (Velocity != Vector2.zero)
            {
                float minDistance = float.MaxValue;
                Vector2 closest = transform.position;
                foreach (var line in RoadMap.Instance.RoadLines)
                {
                    var dis = MathUtils.PointToLineSegmentDistance(transform.position, 
                        line.start.ToVector2(),  line.end.ToVector2(),  out closest);
                    if (dis < float.Epsilon)
                    {
                        break;
                    }

                    if (dis < minDistance)
                    {
                        minDistance = dis;
                    }
                }
                transform.position = closest;
            }
        }
    }
}