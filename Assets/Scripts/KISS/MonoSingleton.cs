using UnityEngine;

namespace KISS
{
    public class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T s_instance;
        private static object s_lock = new object();

        public static T Instance
        {
            get
            {
                // 检查是否存在实例，如果不存在则查找现有的实例
                if (s_instance == null)
                {
                    // 在多线程环境下保证只有一个线程可以创建实例
                    lock (s_lock)
                    {
                        // 在锁内再次检查实例是否存在，避免其他线程已经创建实例
                        if (s_instance == null)
                        {
                            // 在场景中查找现有的实例
                            s_instance = FindObjectOfType<T>();

                            // 如果在场景中找不到实例，则创建一个新的实例并将其添加到游戏对象
                            if (s_instance == null)
                            {
                                GameObject singletonObject = new GameObject(typeof(T).Name);
                                s_instance = singletonObject.AddComponent<T>();
                                // DontDestroyOnLoad(singletonObject);
                            }
                        }
                    }
                }

                return s_instance;
            }
        }

        protected virtual void Awake()
        {
            // 检查是否已经存在实例，如果存在则销毁新创建的实例
            if (s_instance != null && s_instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                s_instance = this as T;
            }
        }
    }
}