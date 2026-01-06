using System;
using UnityEngine;

namespace KISS
{
    public static class Extensions
    {
        public static T EnsureComponent<T>(this GameObject go) where T : Component
        {
            if (!go.TryGetComponent<T>(out var component))
            {
                component = go.AddComponent<T>();
            }

            return component;
        }

        public static Vector2 ToVector2(this (float, float) v)
        {
            return new Vector2(v.Item1, v.Item2);
        }

        public static (float, float) ToTuple(this Vector2 v)
        {
            return (v.x, v.y);
        }

        public static Vector2 ToVector2(this float[] v)
        {
            return new Vector2(v[0], v[1]);
        }

        public static float[] ToArray(this Vector2 v)
        {
            return new[] { v.x, v.y };
        }


        /// <summary>
        /// 为 Action 委托添加“去重订阅”逻辑
        /// </summary>
        /// <param name="action">要操作的 Action 委托（ref 确保修改原委托）</param>
        /// <param name="handler">要订阅的方法</param>
        public static void SubscribeDeDup(ref Action action, Action handler)
        {
            if (handler == null) return;
            action -= handler; // 先移除，避免重复
            action += handler;
        }

        // 重载：支持带单个参数的 Action<T>
        public static void SubscribeDeDup<T>(ref Action<T> action, Action<T> handler)
        {
            if (handler == null) return;
            action -= handler;
            action += handler;
        }
    }
}