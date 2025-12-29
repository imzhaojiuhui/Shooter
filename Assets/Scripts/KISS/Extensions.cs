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
    }
}