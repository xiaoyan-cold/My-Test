using System;
using System.Collections.Generic;

/// <summary>
/// ObjectPool<T>: 这是一个泛型类，用于管理类型为T的对象。
/// IDisposable: 实现了IDisposable接口，表示该类有资源需要释放。
/// where T : new(): 这是一个泛型约束，表示类型T必须有一个无参数的构造函数，这样对象池可以创建新对象。
/// </summary>
/// <typeparam name="T"></typeparam>
public class ObjectPool<T> : IDisposable where T : new()
{
    // MaxCacheCount: 对象池中缓存的最大对象数，默认为32。
    public int MaxCacheCount = 32;

    // cache: 一个LinkedList<T>链表，用于存储可重用的对象。
    private static LinkedList<T> cache;
    // onRelease: 一个Action<T>委托，在对象被释放回对象池时调用。
    private Action<T> onRelease;
    /// <summary>
    /// 构造函数: 初始化对象池，创建一个空的链表，并设置onRelease委托。
    /// </summary>
    /// <param name="onRelease"></param>
    public ObjectPool(Action<T> onRelease)
    {
        cache = new LinkedList<T>();
        this.onRelease = onRelease;
    }
    /// <summary>
    /// 从链表集合中获取对象
    /// </summary>
    /// <returns></returns>
    public T Obtain()
    {
        // Obtain(): 从对象池获取一个对象。
        T value;
        // 如果对象池为空，则创建一个新对象。
        if (cache.Count == 0)
        {
            value = new T();
        }
        else
        {
            // 如果对象池不为空，则从池中获取第一个对象，并将其从池中移除。
            value = cache.First.Value;
            cache.RemoveFirst();
        }
        return value;
    }
    // Release(T value): 将对象释放回对象池。
    public void Release(T value)
    {
        // 如果池中的对象数量已达到最大缓存数量，则不将对象放回池中。
        if (cache.Count >= MaxCacheCount)
            return;

        // 调用onRelease委托（如果已设置），对对象进行处理。
        onRelease?.Invoke(value);
        // 将对象添加到池的末尾。
        cache.AddLast(value);
    }

    /// <summary>
    ///  清空对象池
    /// </summary>
    public void Clear()
    {
        cache.Clear();
    }
    /// <summary>
    /// 释放资源
    /// </summary>
    public void Dispose()
    {
        cache = null;
        onRelease = null;
    }
}


public class QueuePool<T>
{
    private static ObjectPool<Queue<T>> pool = new ObjectPool<Queue<T>>((value) => value.Clear());
    public static Queue<T> Obtain() => pool.Obtain();
    public static void Release(Queue<T> value) => pool.Release(value);
    public static void Clear() => pool.Clear();
}

public class ListPool<T>
{
    private static ObjectPool<List<T>> pool = new ObjectPool<List<T>>((value) => value.Clear());
    // 获取对象
    public static List<T> Obtain() => pool.Obtain();
    /// <summary>
    /// 释放集合
    /// </summary>
    /// <param name="value"></param>
    public static void Release(List<T> value) => pool.Release(value);
    /// <summary>
    /// 清除集合
    /// </summary>
    public static void Clear() => pool.Clear();
}
public class HashSetPool<T>
{
    private static ObjectPool<HashSet<T>> pool = new ObjectPool<HashSet<T>>((value) => value.Clear());
    public static HashSet<T> Obtain() => pool.Obtain();
    public static void Release(HashSet<T> value) => pool.Release(value);
    public static void Clear() => pool.Clear();
}
public class DictionaryPool<K, V>
{
    private static ObjectPool<Dictionary<K, V>> pool = new ObjectPool<Dictionary<K, V>>((value) => value.Clear());
    public static Dictionary<K, V> Obtain() => pool.Obtain();
    public static void Release(Dictionary<K, V> value) => pool.Release(value);
    public static void Clear() => pool.Clear();
}
