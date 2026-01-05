using System;
using UnityEngine;

namespace Ghost.Terrain
{
    public class CrossMovementV2 : Movement
    {
        private bool _onLadder = false;
        private (PathUtils.WeightedGraphNode, PathUtils.WeightedGraphNode) _connectedEdge;

        private void Start()
        {
            var (edgeFrom, edgeTo, pos) =
                CrossMap.Instance.Graph.GetNearestEdgePos(this.transform.position);
            this.transform.position = pos;

            var arrow = edgeTo.WorldPos - edgeFrom.WorldPos;
            _onLadder = Mathf.Abs(arrow.x) < Mathf.Abs(arrow.y);

            _connectedEdge = CrossMap.Instance.Graph.GetConnectedEdge(edgeFrom, edgeTo.WorldPos - edgeFrom.WorldPos);
        }

        private void SwitchOnLadder(bool onLadder, Vector2 conerPos)
        {
            _onLadder = onLadder;
            var coner = CrossMap.Instance.Graph.GetNearestNode(conerPos);
            _connectedEdge = CrossMap.Instance.Graph.GetConnectedEdge(coner, onLadder ? Vector2.up : Vector2.right);
        }

        private void Update()
        {
            var vel = InputVelocity;

            if (vel == Vector2.zero)
            {
                return;
            }


            // if (Mathf.Approximately(Mathf.Abs(vel.x), Mathf.Abs(vel.y)))
            // {
            //     if (_onLadder)
            //     {
            //         vel.x = 0;
            //     }
            //     else
            //     {
            //         vel.y = 0;
            //     }
            // }

            var curPos = this.transform.position;

            if (!_onLadder && CrossMap.Instance.EnterClimbRect(transform.position, out Rect climbRect, out var up))
            {
                var enterPos = climbRect.center;
                var toEnterPos = enterPos - (Vector2)curPos;
                if (enterPos.x < Mathf.Min(_connectedEdge.Item1.WorldPos.x, _connectedEdge.Item2.WorldPos.x))
                {
                }

                if (enterPos.x > Mathf.Max(_connectedEdge.Item1.WorldPos.x, _connectedEdge.Item2.WorldPos.x))
                {
                }

                if (up && vel.y < float.Epsilon) // 楼梯在上却按下
                {
                }
                else if (!up && vel.y > -float.Epsilon) // 楼梯在下 按上
                {
                }
                else if (Mathf.Abs(vel.y) < Mathf.Abs(vel.x)) // 左右运动
                {
                }
                else if (((Vector2)curPos - climbRect.center).sqrMagnitude < .1f) // enter point
                {
                    transform.position = enterPos;
                    // _onLadder = true;
                    SwitchOnLadder(true, enterPos);
                    return;
                }
                else if (Vector2.Dot(toEnterPos, vel) < 0) // 和楼梯不是一个方向
                {
                }
                else // -> enter point
                {
                    var dis = Speed * Time.deltaTime;
                    if (dis < toEnterPos.magnitude)
                    {
                        var dir = toEnterPos.normalized;
                        transform.position += (Vector3)dir * dis;
                    }
                    else
                    {
                        this.transform.position = enterPos;
                        // _onLadder = true;
                        SwitchOnLadder(true, enterPos);
                    }

                    return;
                }
            }

            // todo 优化在enter point时 w a同时按 
            if (_onLadder && CrossMap.Instance.EnterDownRect(transform.position, out Rect downRect, out var downPos))
            {
                var toEnterPos = downPos - (Vector2)curPos;
                // if (enterPos.x < Mathf.Min(_connectedEdge.Item1.WorldPos.y, _connectedEdge.Item2.WorldPos.y))
                // {
                //     
                // }
                // if (enterPos.x > Mathf.Max(_connectedEdge.Item1.WorldPos.y, _connectedEdge.Item2.WorldPos.y))
                // {
                //     
                // }
                if (Mathf.Abs(vel.y) > Mathf.Abs(vel.x)) // 上下
                {
                }
                else if (((Vector2)curPos - downPos).sqrMagnitude < .1f) // enter point
                {
                    transform.position = downPos;
                    // _onLadder = false;
                    SwitchOnLadder(false, downPos);
                    return;
                }
                else if (Vector2.Dot(toEnterPos, vel) < 0) // 和enter pos不是一个方向
                {
                }
                else
                {
                    var dis = Speed * Time.deltaTime;
                    if (dis < toEnterPos.magnitude)
                    {
                        var dir = toEnterPos.normalized;
                        transform.position += (Vector3)dir * dis;
                    }
                    else
                    {
                        this.transform.position = downPos;
                        // _onLadder = false;
                        SwitchOnLadder(false, downPos);
                    }

                    return;
                }
            }

            var forward = Vector2.zero;
            if (_onLadder)
            {
                forward = Speed * Time.deltaTime * Math.Sign(vel.y) * Vector2.up;
                // transform.Translate(Speed * Time.deltaTime * Math.Sign(vel.y) * Vector2.up);
            }
            else
            {
                forward = Speed * Time.deltaTime * Math.Sign(vel.x) * Vector2.right;
                // transform.Translate(Speed * Time.deltaTime * Math.Sign(vel.x) * Vector2.right);
            }

            #region clamp by wall

            var toPos = transform.position + (Vector3)forward;

            var xMin = Mathf.Min(_connectedEdge.Item1.WorldPos.x, _connectedEdge.Item2.WorldPos.x);
            var xMax = Mathf.Max(_connectedEdge.Item1.WorldPos.x, _connectedEdge.Item2.WorldPos.x);
            var yMin = Mathf.Min(_connectedEdge.Item1.WorldPos.y, _connectedEdge.Item2.WorldPos.y);
            var yMax = Mathf.Max(_connectedEdge.Item1.WorldPos.y, _connectedEdge.Item2.WorldPos.y);

            var clampX = Mathf.Clamp(toPos.x, xMin, xMax);
            var clampY = Mathf.Clamp(toPos.y, yMin, yMax);
            {
                toPos.x = clampX;
                toPos.y = clampY;
            }
            transform.position = toPos;

            #endregion
        }
    }
}