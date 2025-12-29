using System;

/// <summary>
/// 通用抽象单例基类（泛型）
/// 所有子类继承此类，即可自动拥有 线程安全+懒加载 的单例能力
/// 调用方式：子类名.Instance  获取唯一实例
/// </summary>
/// <typeparam name="T">子类自身的类型</typeparam>
public abstract class Singleton<T> where T : class, new()
{
    private static T _instance;

    public static T Instance
    {
        get
        {
            if (null == _instance)
            {
                _instance = new T();
                // DebuggerLog.Assert(_instance != null);
            }

            return _instance;
        }
    }
}