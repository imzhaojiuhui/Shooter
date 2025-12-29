using UnityEngine;

namespace KISS
{
    public static class MathUtils
    {
        /// <summary>
        /// 计算【点】到【有限线段】的最短距离 (Unity2D 最优写法，99%场景用这个)
        /// </summary>
        /// <param name="point">目标点 P</param>
        /// <param name="linePointA">线段起点 A</param>
        /// <param name="linePointB">线段终点 B</param>
        /// <returns>点到线段的最短距离（浮点值）</returns>
        public static float PointToLineSegmentDistance(Vector2 point, Vector2 linePointA, Vector2 linePointB, 
            out Vector2 closestPoint)
        {
            // 计算线段AB的向量
            Vector2 lineVec = linePointB - linePointA;
            // 计算线段长度的平方（用平方避免开根号，提升性能）
            float lineLengthSqr = lineVec.sqrMagnitude;

            // 特殊情况：线段的起点和终点重合（A=B），距离 = 点到该点的直线距离
            if (lineLengthSqr < Mathf.Epsilon)
            {
                closestPoint = linePointA;
                return Vector2.Distance(point, linePointA);
            }

            // 计算 点A到点P的向量 在 线段AB向量上的投影系数 t
            float t = Vector2.Dot(point - linePointA, lineVec) / lineLengthSqr;
            // 约束t的范围【0,1】，核心：判断垂足是否在线段上
            t = Mathf.Clamp01(t);

            // 计算线段上的最近点
            closestPoint = linePointA + t * lineVec;
            // 计算点到最近点的距离
            return Vector2.Distance(point, closestPoint);
        }

        /// <summary>
        /// 计算【点】到【无限直线】的垂直距离 (游戏开发极少用，纯数学计算)
        /// </summary>
        /// <param name="point">目标点 P</param>
        /// <param name="linePointA">直线上任意一点 A</param>
        /// <param name="linePointB">直线上任意一点 B</param>
        /// <returns>点到直线的绝对垂直距离</returns>
        // public static float PointToInfiniteLineDistance(Vector2 point, Vector2 linePointA, Vector2 linePointB)
        // {
        //     // 向量叉乘的绝对值 / 线段长度 = 点到直线的垂直距离 (2D叉乘就是向量的z轴值)
        //     return Mathf.Abs(Vector2.Cross(linePointB - linePointA, point - linePointA)) / Vector2.Distance(linePointA, linePointB);
        // }
    }
}