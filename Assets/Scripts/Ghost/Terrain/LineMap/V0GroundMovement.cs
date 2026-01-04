using System;
using Ghost.Terrain;
using KISS;
using UnityEngine;

namespace Ghost
{
    public class V0GroundMovement : MonoBehaviour
    {
        public float baseSpeed = 3;
        [Header("登梯/下梯 距离小于这个值直接跳过去")] public float turnDistance = .4f;
        [Header("登梯子防抖")] public float turnDelaySeconds = 0.2f;
        [Header("忽略roadLine毛刺")] public float burrTrimLen = .5f; //修剪roadLine毛刺
        [Header("小于这个距离时 倾向于转角处")] public float inclineDistance = 1f;

        public Vector2 InputVelocity { get; set; }

        public float Speed
        {
            get { return baseSpeed; }
        }


        private float _turnSeconds;
        private bool _preFrameMoveHorizontal; // 当前是水平移动还是竖直移动

        private void Update() // 距离转角进时 倾向于转角处
        {
            #region 处理输入方向

            var input = InputVelocity;
            var absX = Mathf.Abs(input.x);
            var absY = Mathf.Abs(input.y);
            bool moveV = absY > float.Epsilon &&
                         (absY > absX || !_preFrameMoveHorizontal); //防止抖动登上梯子
            bool moveH = absX > float.Epsilon &&
                         (absY < absX || _preFrameMoveHorizontal);
            var velocity = Vector2.zero;
            if (moveH)
            {
                velocity += Math.Sign(input.x) * Vector2.right;
            }

            if (moveV)
            {
                velocity += Math.Sign(input.y) * Vector2.up;
            }

            #endregion

            bool canTurn = false;
            foreach (var line in RoadMap.Instance.RoadLines)
            {
                if (line.Horizontal == _preFrameMoveHorizontal) // 和行进在一个方向
                {
                    continue;
                }

                var (canTurn_, _, dis) = CanTurn(transform.position,
                    velocity,
                    line.start.ToVector2(), line.end.ToVector2());
                if (canTurn_)
                {
                    canTurn = true;
                    break;
                }
            }

            // transform.Translate(Velocity*Speed*Time.deltaTime);
            var toPos = transform.position + (Vector3)velocity * Speed * Time.deltaTime;
            if (velocity == Vector2.zero)
            {
                _turnSeconds += Time.deltaTime;
            }
            else
            {
                // bool moveHorizontal = Mathf.Abs(Velocity.x) > Mathf.Abs(Velocity.y);
                if (moveH && !_preFrameMoveHorizontal)
                {
                    _turnSeconds += Time.deltaTime;
                }
                else if (moveV && _preFrameMoveHorizontal)
                {
                    _turnSeconds += Time.deltaTime;
                }
                else // 不转向
                {
                    _turnSeconds = 0;
                }

                bool turnTimeFull = _turnSeconds > turnDelaySeconds;

                float minDistance = float.MaxValue;
                Vector2 closest = toPos;
                foreach (var line in RoadMap.Instance.RoadLines)
                {
                    var dis = MathTool.PointToLineSegmentDistance(toPos,
                        line.start.ToVector2(), line.end.ToVector2(), out var curClosest);
                    // if (dis < float.Epsilon)
                    // {
                    //     break;
                    // }

                    #region 登梯/下楼梯

                    if (!turnTimeFull || !canTurn)
                    {
                    }
                    else if (moveH && !_preFrameMoveHorizontal && line.Horizontal)
                    {
                        dis -= turnDistance;
                    }
                    else if (moveV && _preFrameMoveHorizontal && !line.Horizontal)
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="point"></param>
        /// <param name="moveDir"></param>
        /// <param name="linePointA"></param>
        /// <param name="linePointB"></param>
        /// <param name="closestPoint"></param>
        /// <returns>canTurn, closest, distance</returns>
        public (bool, Vector2, float) CanTurn(Vector2 point, Vector2 moveDir, Vector2 linePointA, Vector2 linePointB)
        {
            // 计算线段AB的向量
            Vector2 lineVec = linePointB - linePointA;
            // 计算线段长度的平方（用平方避免开根号，提升性能）
            float lineLengthSqr = lineVec.sqrMagnitude;

            bool canTurn = false;
            // 特殊情况：线段的起点和终点重合（A=B），距离 = 点到该点的直线距离
            if (lineLengthSqr < Mathf.Epsilon)
            {
                // closestPoint = linePointA;
                return (false, linePointA, Vector2.Distance(point, linePointA));
            }

            // 计算 点A到点P的向量 在 线段AB向量上的投影系数 t
            float t = Vector2.Dot(point - linePointA, lineVec) / lineLengthSqr;
            // 约束t的范围【0,1】，核心：判断垂足是否在线段上
            t = Mathf.Clamp01(t);

            // 计算线段上的最近点
            var closestPoint = linePointA + t * lineVec;
            // 计算点到最近点的距离
            var dis = Vector2.Distance(point, closestPoint);

            var dot = Vector2.Dot(moveDir, lineVec);
            if (dot < float.Epsilon)
            {
                // 与line垂直
                canTurn = false;
            }
            // 还要要求移动到toPos方向上roadLine超过一定长度
            else if (dot > 0) // 同向
            {
                var forwardDis = (1 - t) * lineVec.magnitude;
                canTurn = forwardDis > burrTrimLen;
            }
            else // 反向
            {
                var forwardDis = t * lineVec.magnitude;
                canTurn = forwardDis > burrTrimLen;
            }

            return (canTurn, closestPoint, dis);
        }
    }
}