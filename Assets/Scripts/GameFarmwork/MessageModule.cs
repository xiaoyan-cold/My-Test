using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using UnityEngine;

public class MessageModule : BaseGameModule
{
    // 定义了一个泛型委托（delegate），具体来说，它是一个能够处理异步方法（返回类型为Task）的委托
    public delegate Task MessageHandlerEventArgs<T>(T arg);
    // 定义全局的消息处理
    private Dictionary<Type, List<object>> globalMessageHandlers;
    // 本地的消息处理
    private Dictionary<Type, List<object>> localMessageHandlers;
    // 监视器
    public Monitor Monitor { get; private set; }

    // 重写初始化方法
    protected internal override void OnModuleInit()
    {
        base.OnModuleInit();
        // 初始化localMessageHandlers对象
        localMessageHandlers = new Dictionary<Type, List<object>>();
        // 初始化Monitor对象
        Monitor = new Monitor();
        LoadAllMessageHandlers();
    }

    protected internal override void OnModuleStop()
    {
        base.OnModuleStop();
        // 清除两个字典
        globalMessageHandlers = null;
        localMessageHandlers = null;

    }
    // 加载全局的消息处理
    private void LoadAllMessageHandlers()
    {
        globalMessageHandlers = new Dictionary<Type, List<object>>();
        // etTypes()` 方法返回一个 `Type[]` 数组，其中包含程序集中定义的所有类型的实例
        // `GetCallingAssembly()` 方法返回调用方的程序集。
        foreach (var type in Assembly.GetCallingAssembly().GetTypes())
        {
            if (type.IsAbstract)
                continue;

            MessageHandlerAttribute messageHandlerAttribute = type.GetCustomAttribute<MessageHandlerAttribute>(true);
            if (messageHandlerAttribute != null)
            {
                // 创建实例对象，转为IMessageHandler类型
                IMessageHandler messageHandler = Activator.CreateInstance(type) as IMessageHandler;
                // 全局消息处理中不包含本类型
                if (!globalMessageHandlers.ContainsKey(messageHandler.GetHandlerType()))
                {
                    // 将次添加进去
                    globalMessageHandlers.Add(messageHandler.GetHandlerType(), new List<object>());
                }

                globalMessageHandlers[messageHandler.GetHandlerType()].Add(messageHandler);
            }

        }
    }
    /// <summary>
    /// 注册本地消息处理
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="handler"></param>
    public void Subscribe<T>(MessageHandlerEventArgs<T> handler)
    {
        Type argType = typeof(T);
        Debug.Log("本地消息类型" + argType);
        if (!localMessageHandlers.TryGetValue(argType, out var handlerList))
        {
            handlerList = new List<object>();
            localMessageHandlers.Add(argType, handlerList);
        }

        handlerList.Add(handler);
    }
    /// <summary>
    /// 移除监听
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="handler"></param>
    public void Unsubscribe<T>(MessageHandlerEventArgs<T> handler)
    {
        // 如果集合中不包含此事件，结束方法
        if (!localMessageHandlers.TryGetValue(typeof(T), out var handlerList))
            return;

        handlerList.Remove(handler);
    }
    /// <summary>
    /// 广播消息
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="arg"></param>
    /// <returns></returns>
    public async Task Post<T>(T arg) where T : struct
    {
        // 此类型的消息是否在全局消息中
        if (globalMessageHandlers.TryGetValue(typeof(T), out List<object> globalHandlerList))
        {
            // 如果在全局消息中，取出它的全局消息集合
            foreach (var handler in globalHandlerList)
            {
                // 是一个类型模式匹配（type pattern matching）的语法，用于判断一个对象是否属于指定类型，并将其转换为该类型的实例。
                // 在给定的代码中，handler 是一个对象，而 MessageHandler<T> 是一个泛型类型。该判断语句可以解读为：
                // handler is MessageHandler < T >：判断 handler 对象是否是类型 MessageHandler<T> 的实例。
                // messageHandler：如果判断为真，将 handler 对象转换为 MessageHandler< T > 类型，并将其赋值给变量 messageHandler。
                if (!(handler is MessageHandler<T> messageHandler))
                    // 不属于指定类型时，跳出本次循环
                    continue;
                // 等待消息处理结果
                await messageHandler.HandleMessage(arg);
            }
        }
        // 此类型在本地消息中
        if (localMessageHandlers.TryGetValue(typeof(T), out List<object> localHandlerList))
        {
            // 从集合对象池中获取集合对象
            List<object> list = ListPool<object>.Obtain();
            // 无GC版本的AddRange
            list.AddRangeNonAlloc(localHandlerList);

            foreach (var handler in list)
            {
                // 是一个类型模式匹配（type pattern matching）的语法，用于判断一个对象是否属于指定类型，并将其转换为该类型的实例。
                if (!(handler is MessageHandlerEventArgs<T> messageHandler))
                    // 不属于指定类型时，跳出本次循环
                    continue;
                // 等待消息处理结果
                await messageHandler(arg);
            }
            // 将对象放回对象池
            ListPool<object>.Release(list);
        }
    }


}

