using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
/// <summary>
/// 监视器类
/// </summary>
public class Monitor
{
    // 声明只读字段，表示字段的值一旦被初始化后就不能更改
    private readonly Dictionary<Type, object> waitObjects = new Dictionary<Type, object>();

    public WaitObject<T> Wait<T>() where T : struct
    {
        // 创建一个等待对象的实例
        WaitObject<T> waitObject = new WaitObject<T>();
        // 添加进只读的字典中，类型做键，对象作值
        waitObjects.Add(typeof(T), waitObject);
        return waitObject;
    }

    public void SetResult<T>(T result) where T : struct
    {
        Type type = typeof(T);
        // 尝试从字典中获取此类型的对象，如果字典中没有，结束方法
        if (!waitObjects.TryGetValue(type, out object obj))
            return;
        
        waitObjects.Remove(type);

    }

    /// <summary>
    /// 内嵌类，继承通报完成的接口
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class WaitObject<T> : INotifyCompletion where T : struct
    {
        // 是否完成属性
        public bool IsCompleted { get; private set; }
        // 结果
        public T Result { get; private set; }

        // 回调事件
        private Action callback;

        /// <summary>
        /// 设置委托
        /// </summary>
        /// <param name="result"></param>
        public void SetResult(T result)
        {
            Result = result;
            IsCompleted = true;
            Action c = callback;
            callback = null;
            c?.Invoke();
        }
        /// <summary>
        /// 获取一个等待的对象
        /// </summary>
        /// <returns></returns>
        public WaitObject<T> GetAwaiter()
        {
            return this;
        }

        /// <summary>
        /// 方法用于注册一个回调，当异步操作完成时调用该回调，callback代表需要执行的后续操作
        /// </summary>
        /// <param name="callback"></param>
        public void OnCompleted(Action callback)
        {
            this.callback = callback;
        }
        /// <summary>
        /// 获取结果
        /// </summary>
        /// <returns></returns>
        public T GetResult()
        {
            return Result;
        }
    }

}


