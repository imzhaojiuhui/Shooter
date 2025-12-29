using System;

/// <summary>
/// 通用抽象单例基类（泛型）
/// 所有子类继承此类，即可自动拥有 线程安全+懒加载 的单例能力
/// 调用方式：子类名.Instance  获取唯一实例
/// </summary>
/// <typeparam name="T">子类自身的类型</typeparam>
public abstract class Singleton<T> where T : class, new()
{
    /// <summary>
    /// 全局唯一实例（子类直接通过 子类名.Instance 访问）
    /// </summary>
    public static readonly T Instance = LazyInstance.Value;

    /// <summary>
    /// 静态懒加载对象，.NET原生线程安全，第一次访问Instance时才创建实例
    /// LazyThreadSafetyMode.ExecutionAndPublication 保证实例只被创建一次
    /// </summary>
    private static readonly Lazy<T> LazyInstance = new Lazy<T>(() => new T());
}