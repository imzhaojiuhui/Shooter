using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace KISS
{
    public static class MathTool
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

        // 线段结构体：两点确定一条线段
        public struct Line2D
        {
            public Vector2 start;
            public Vector2 end;

            public Line2D(Vector2 s, Vector2 e)
            {
                start = s;
                end = e;
            }

            public bool Horizontal
            {
                get
                {
                    var dir = end - start;
                    return Mathf.Abs(dir.x) > Mathf.Abs(dir.y);
                }
            }
        }

        #region 核心修复：手动实现2D向量叉积（重中之重）

        /// <summary>
        /// 2D向量叉积的标准实现，替代Unity缺失的Vector2.Cross
        /// </summary>
        public static float Cross(Vector2 a, Vector2 b)
        {
            return a.x * b.y - a.y * b.x;
        }

        #endregion

        /// <summary>
        /// 核心方法：判断两条2D线段是否相交，并输出交点坐标
        /// </summary>
        public static bool IsTwoLinesIntersect(Line2D line1, Line2D line2, out Vector2 intersection)
        {
            intersection = Vector2.zero;
            Vector2 A = line1.start;
            Vector2 B = line1.end;
            Vector2 C = line2.start;
            Vector2 D = line2.end;

            // 定义所有需要的向量
            Vector2 AB = B - A;
            Vector2 AC = C - A;
            Vector2 AD = D - A;
            Vector2 CD = D - C;
            Vector2 CA = A - C;
            Vector2 CB = B - C;

            // ✅ 修正：调用自定义的Cross方法，替代Vector2.Cross
            float cross1 = Cross(AB, AC);
            float cross2 = Cross(AB, AD);
            float cross3 = Cross(CD, CA);
            float cross4 = Cross(CD, CB);

            // 跨立实验：判断线段是否互相跨立 → 相交
            bool isCross = (cross1 * cross2 < 0) && (cross3 * cross4 < 0);

            // 【可选开启】如果需要判断 端点接触/线段共线重叠 也算相交，取消下面注释
            /*
            if (!isCross)
            {
                isCross = IsPointOnLine(C, line1) || IsPointOnLine(D, line1) ||
                          IsPointOnLine(A, line2) || IsPointOnLine(B, line2);
            }
            */

            if (isCross)
            {
                // ✅ 修正：这里也替换成自定义的Cross方法，就是你问的核心公式！
                float t = Cross(AC, CD) / Cross(AB, CD);
                intersection = A + t * AB; // 代入参数方程求交点，不变
                return true;
            }

            return false;
        }

        /// <summary>
        /// 辅助方法：判断点是否在线段上（含端点）
        /// </summary>
        public static bool IsPointOnLine(Vector2 point, Line2D line)
        {
            // ✅ 修正：叉积判断点是否在线上
            if (Mathf.Abs(Cross(line.end - line.start, point - line.start)) > 1e-6) return false;
            float minX = Mathf.Min(line.start.x, line.end.x);
            float maxX = Mathf.Max(line.start.x, line.end.x);
            float minY = Mathf.Min(line.start.y, line.end.y);
            float maxY = Mathf.Max(line.start.y, line.end.y);
            return point.x >= minX - 1e-6 && point.x <= maxX + 1e-6 && point.y >= minY - 1e-6 && point.y <= maxY + 1e-6;
        }

        /// <summary>
        /// 批量计算多条线段的所有无重复交点（核心业务方法）
        /// </summary>
        public static List<Vector2> GetAllIntersections(List<Line2D> allLines, bool isRemoveRepeat = true)
        {
            List<Vector2> allIntersections = new List<Vector2>();
            for (int i = 0; i < allLines.Count; i++)
            {
                for (int j = i + 1; j < allLines.Count; j++)
                {
                    if (IsTwoLinesIntersect(allLines[i], allLines[j], out Vector2 point))
                    {
                        allIntersections.Add(point);
                    }
                }
            }

            if (isRemoveRepeat && allIntersections.Count > 0)
            {
                allIntersections = RemoveRepeatPoints(allIntersections);
            }

            return allIntersections;
        }

        /// <summary>
        /// 去除重复点（解决浮点精度误差）
        /// </summary>
        public static List<Vector2> RemoveRepeatPoints(List<Vector2> points, float epsilon = 1e-5f)
        {
            List<Vector2> uniquePoints = new List<Vector2>();
            foreach (var p in points)
            {
                bool isRepeat = false;
                foreach (var up in uniquePoints)
                {
                    if (Vector2.Distance(p, up) < epsilon)
                    {
                        isRepeat = true;
                        break;
                    }
                }

                if (!isRepeat) uniquePoints.Add(p);
            }

            return uniquePoints;
        }

        /// <summary>
        /// 对交点排序（X轴升序，X相同则Y轴升序）
        /// </summary>
        public static List<Vector2> SortPoints(List<Vector2> points)
        {
            return points.OrderBy(p => p.x).ThenBy(p => p.y).ToList();
        }

        /// <summary>
        /// 判断两个向量的夹角是否在deg度以内（含deg度）
        /// </summary>
        public static bool IsAngleWithinDeg(Vector3 vecA, Vector3 vecB, float deg)
        {
            // 1. 归一化向量 → 转为单位向量（长度=1，消除向量长度对判断的影响）
            Vector3 dirA = vecA.normalized;
            Vector3 dirB = vecB.normalized;
            // 2. 计算点积 + 核心判断：点积 ≥ cos(deg) → 夹角 ≤deg
            float dot = Vector3.Dot(dirA, dirB);
            return dot >= Mathf.Cos(Mathf.Deg2Rad * deg);
        }

        /// <summary>
        /// 判断两个向量的夹角是否在45度以内（含45度）
        /// </summary>
        public static bool IsAngleWithin45(Vector3 vecA, Vector3 vecB)
        {
            // 1. 归一化向量 → 转为单位向量（长度=1，消除向量长度对判断的影响）
            Vector3 dirA = vecA.normalized;
            Vector3 dirB = vecB.normalized;
            // 2. 计算点积 + 核心判断：点积 ≥ cos(45°) → 夹角 ≤45°
            float dot = Vector3.Dot(dirA, dirB);
            return dot >= Mathf.Cos(Mathf.Deg2Rad * 45);
        }
    }
}