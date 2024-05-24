using System;
/// <summary>
/// 消息处理的接口
/// </summary>
public interface IMessageHandler
{
    Type GetHandlerType();
}

