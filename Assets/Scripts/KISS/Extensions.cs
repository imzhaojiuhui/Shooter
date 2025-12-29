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
            return new []{v.x, v.y};
        }
    }
}