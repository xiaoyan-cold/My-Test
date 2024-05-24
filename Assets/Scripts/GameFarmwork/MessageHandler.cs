using System;
using System.Threading.Tasks;

[MessageHandler]
public abstract class MessageHandler<T> : IMessageHandler where T : struct
{
    public Type GetHandlerType()
    {
        return typeof(T);
    }
    // 异步消息处理
    public abstract Task HandleMessage(T arg);
}
// 特性约束，只能用于类，可以被子类继承，不允许同一程序元素上应用多个实例
[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
sealed class MessageHandlerAttribute : Attribute { }