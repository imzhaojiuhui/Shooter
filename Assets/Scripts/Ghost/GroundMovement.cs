using System;
using Ghost.Terrain;
using KISS;
using UnityEngine;

namespace Ghost
{
    public class GroundMovement:MonoBehaviour
    {
        public float baseSpeed = 3;
        [Header("登梯/下梯 距离")]
        public  float turnDistance = 10f;
        public  float turnDelaySeconds = 0.2f;
        
        public Vector2 InputVelocity {get; set; }
        public float Speed {get; set; }

        private void Start()
        {
            Speed = baseSpeed;
        }

        private float _turnSeconds;
        private bool _preFrameMoveHorizontal;
        private void Update()
        {
            var input = InputVelocity;
            var absX = Mathf.Abs(input.x);
            var absY = Mathf.Abs(input.y);
            bool moveV = absY > .1f && 
                            (absY > absX || 
                            !_preFrameMoveHorizontal); 
            bool moveH = absX > .1f && 
                              (absY < absX || 
                               _preFrameMoveHorizontal); 
            var velocity = Vector2.zero;
            if (moveH)
            {
                velocity += Math.Sign(input.x) * Vector2.right;
            }
            
            if (moveV)
            {
                velocity += Math.Sign(input.y) * Vector2.up;
            }
            
            // transform.Translate(Velocity*Speed*Time.deltaTime);
            var toPos = transform.position + (Vector3)velocity*Speed*Time.deltaTime;
            if (velocity != Vector2.zero)
            {
                // bool moveHorizontal = Mathf.Abs(Velocity.x) > Mathf.Abs(Velocity.y);
                // if (moveHorizontal != _preFrameMoveHorizontal)
                // {
                //     _turnSeconds += Time.deltaTime;
                // }
                // _preFrameMoveHorizontal = moveHorizontal;
                // bool turn = _turnSeconds > turnDelaySeconds;
                // bool turn = true;

                float minDistance = float.MaxValue;
                Vector2 closest = toPos;
                foreach (var line in RoadMap.Instance.RoadLines)
                {
                    var dis = MathUtils.PointToLineSegmentDistance(toPos, 
                        line.start.ToVector2(),  line.end.ToVector2(),  out var curClosest);
                    // if (dis < float.Epsilon)
                    // {
                    //     break;
                    // }
                    
                    #region 登梯/下楼梯
                    
                    if (moveH && line.Horizontal)
                    {
                        dis -= turnDistance;
                    }
                    if (moveV && !line.Horizontal)
                    {
                        dis -= turnDistance;
                    }
                    
                    #endregion
                    
                    if (dis < minDistance)
                    {
                        minDistance = dis;
                        closest = curClosest;
                        _preFrameMoveHorizontal = line.Horizontal;
                    }
                }
                transform.position = closest;
            }
        }
    }
}