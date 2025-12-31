using UnityEngine;

namespace Ghost.Terrain
{
    public class CrossMovement : MonoBehaviour
    {
        public float baseSpeed = 3;
        [Header("登梯/下梯 距离小于这个值可切换过去")] public float turnDistance = .4f;
        [Header("登梯子防抖")] public float turnDelaySeconds = 0.2f;
        [Header("小于这个距离时 倾向于转角处")] public float inclineDistance = 1f;

        public Vector2 InputVelocity { get; set; }

        public float Speed
        {
            get { return baseSpeed; }
        }


        private float _turnSeconds;

        // private bool _preFrameMoveHorizontal = true; // 当前是水平移动还是竖直移动
        private (PathUtils.WeightedGraphNode, PathUtils.WeightedGraphNode) _alongEdge;

        private Transform _edgeNodeA;
        private Transform _edgeNodeB;

        private void Start()
        {
            var (edgeFrom, edgeTo, pos) =
                CrossMap.Instance.Graph.GetNearestEdgePos(this.transform.position);
            this.transform.position = pos;
            _alongEdge = (edgeFrom, edgeTo);
            _edgeNodeA = transform.GetChild(1);
            _edgeNodeB = transform.GetChild(2);
        }

        private void Update()
        {
            // if (InputVelocity == Vector2.zero)
            // {
            //     return;
            // }
            var vel = InputVelocity;

            //     #region 转向防抖
            //
            //     if (velocity ==  Vector2.zero)
            //     {
            //         _turnSeconds += Time.deltaTime;
            //         return;
            //     }
            //     else
            //     {
            //         // bool moveHorizontal = Mathf.Abs(Velocity.x) > Mathf.Abs(Velocity.y);
            //         if (moveH && !_preFrameMoveHorizontal)
            //         {
            //             _turnSeconds += Time.deltaTime;
            //         }
            //         else if (moveV && _preFrameMoveHorizontal)
            //         {
            //             _turnSeconds += Time.deltaTime;
            //         }
            //         else // 不转向
            //         {
            //             _turnSeconds = 0;
            //         }
            //         
            //     }
            //     bool turnTimeFull = _turnSeconds > turnDelaySeconds;
            //     if (!turnTimeFull)
            //     {
            //         if (_preFrameMoveHorizontal)
            //         {
            //             velocity.y = 0;
            //         }
            //         else
            //         {
            //             velocity.x = 0;
            //         }
            //     }
            //     if (velocity ==  Vector2.zero)
            //     {
            //         return;
            //     }
            //
            //     #endregion

            var pos = transform.position;
            // var corner = CrossMap.Instance.Graph.GetNearestNodeWithIn(this.transform.position, turnDistance);
            var (edgeFrom, edgeTo) = _alongEdge;
            if (InputVelocity == Vector2.zero)
            {
            }

            bool lerp = false;
            if (Vector2.Distance(edgeFrom.WorldPos, pos) < turnDistance)
            {
                lerp = edgeFrom.AdjacentEdges.Count > 1;
                if (CrossMap.Instance.Graph.CanGoInDirection(edgeFrom, pos, vel, out var nextNode))
                {
                    if (nextNode != edgeFrom && nextNode != edgeTo)
                    {
                        edgeTo = nextNode;
                        _alongEdge = (edgeFrom, edgeTo);
                    }
                }
            }
            else if (Vector2.Distance(edgeTo.WorldPos, pos) < turnDistance)
            {
                lerp = edgeTo.AdjacentEdges.Count > 1;
                if (CrossMap.Instance.Graph.CanGoInDirection(edgeTo, pos, vel, out var nextNode))
                {
                    if (nextNode != edgeFrom && nextNode != edgeTo)
                    {
                        edgeFrom = nextNode;
                        _alongEdge = (edgeFrom, edgeTo);
                    }
                }
            }

            // 移动
            {
                _edgeNodeA.position = edgeFrom.WorldPos;
                _edgeNodeB.position = edgeTo.WorldPos;
                var dir = edgeTo.WorldPos - edgeFrom.WorldPos;
                var dot = Vector2.Dot(dir, vel);
                if (dot < 0)
                {
                    dir = -dir;
                }

                if (Mathf.Abs(dot) < 1e-6f)
                {
                    // return;
                    dir = Vector2.zero;
                }
                else
                {
                    dir = dir.normalized;
                }

                var toPos = transform.position + (Vector3)dir * Speed * Time.deltaTime;

                var xMin = Mathf.Min(edgeFrom.WorldPos.x, edgeTo.WorldPos.x);
                var xMax = Mathf.Max(edgeFrom.WorldPos.x, edgeTo.WorldPos.x);
                var yMin = Mathf.Min(edgeFrom.WorldPos.y, edgeTo.WorldPos.y);
                var yMax = Mathf.Max(edgeFrom.WorldPos.y, edgeTo.WorldPos.y);

                var clampX = Mathf.Clamp(toPos.x, xMin, xMax);
                var clampY = Mathf.Clamp(toPos.y, yMin, yMax);
                if (lerp)
                {
                    toPos.x = Mathf.Lerp(toPos.x, clampX, Mathf.Clamp01(Time.deltaTime));
                    toPos.y = Mathf.Lerp(toPos.y, clampY, Mathf.Clamp01(Time.deltaTime));
                }
                else
                {
                    toPos.x = clampX;
                    toPos.y = clampY;
                }

                transform.position = toPos;
            }
        }

        // private void Update() // 距离转角进时 倾向于转角处
        // {
        //     #region 处理输入方向
        //
        //     var input = InputVelocity;
        //     var absX = Mathf.Abs(input.x);
        //     var absY = Mathf.Abs(input.y);
        //     bool moveV = absY > float.Epsilon && 
        //                  (absY > absX || !_preFrameMoveHorizontal); //防止抖动登上梯子
        //     bool moveH = absX > float.Epsilon && 
        //                  (absY < absX || _preFrameMoveHorizontal); 
        //     var velocity = Vector2.zero;
        //     if (moveH)
        //     {
        //         velocity += Math.Sign(input.x) * Vector2.right;
        //     }
        //     
        //     if (moveV)
        //     {
        //         velocity += Math.Sign(input.y) * Vector2.up;
        //     }
        //
        //     #endregion
        //
        //     #region 转向防抖
        //
        //     if (velocity ==  Vector2.zero)
        //     {
        //         _turnSeconds += Time.deltaTime;
        //         return;
        //     }
        //     else
        //     {
        //         // bool moveHorizontal = Mathf.Abs(Velocity.x) > Mathf.Abs(Velocity.y);
        //         if (moveH && !_preFrameMoveHorizontal)
        //         {
        //             _turnSeconds += Time.deltaTime;
        //         }
        //         else if (moveV && _preFrameMoveHorizontal)
        //         {
        //             _turnSeconds += Time.deltaTime;
        //         }
        //         else // 不转向
        //         {
        //             _turnSeconds = 0;
        //         }
        //         
        //     }
        //     bool turnTimeFull = _turnSeconds > turnDelaySeconds;
        //     if (!turnTimeFull)
        //     {
        //         if (_preFrameMoveHorizontal)
        //         {
        //             velocity.y = 0;
        //         }
        //         else
        //         {
        //             velocity.x = 0;
        //         }
        //     }
        //     if (velocity ==  Vector2.zero)
        //     {
        //         return;
        //     }
        //
        //     #endregion
        //     
        //     // bool tryTurn = turnTimeFull && ((_preFrameMoveHorizontal && velocity.y > float.Epsilon))
        //     
        //     var corner = CrossMap.Instance.Graph.GetNearestNodeWithIn(this.transform.position, turnDistance);
        //     if (corner == null)
        //     {
        //         var trans = (_preFrameMoveHorizontal?new Vector2(velocity.x, 0):new Vector2(0, velocity.y)) * Speed*Time.deltaTime;
        //         transform.Translate(trans);
        //     }
        //     else if (CrossMap.Instance.Graph.CanGoInDirection(corner, velocity, out var realDir))
        //     {
        //         var toPos = transform.position + (Vector3)realDir*Speed*Time.deltaTime;
        //         if (realDir.x > float.Epsilon)
        //         {
        //             toPos.y = corner.WorldPos.y;
        //         }
        //
        //         if (realDir.y > float.Epsilon)
        //         {
        //             toPos.x = corner.WorldPos.x;
        //         }
        //
        //         transform.position = toPos;
        //     }
        // }
    }
}