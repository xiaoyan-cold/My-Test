using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class RegsterTest : MessageHandler<MessageType.Test>
{
    public override async Task HandleMessage(MessageType.Test arg)
    {
        Debug.Log(arg.data);
        GameManager.Message.Subscribe<MessageType.InTest2>(CallBackTest);
        GameManager.Message.Subscribe<MessageType.IntTest>(CallBackTest2);

        await Task.Delay(2000);
        await GameManager.Message.Post<MessageType.InTest2>(new MessageType.InTest2 { Value = 42 });

        await Task.Delay(2000);
        await GameManager.Message.Post<MessageType.IntTest>(new MessageType.IntTest { Value = 52 });
        await Task.Yield();
    }

    private async Task CallBackTest2(MessageType.IntTest arg)
    {
        Debug.Log("这是局部消息传递过来的值 + CallBackTest2  " + arg.Value);
        await Task.Yield();
    }

    private async Task CallBackTest(MessageType.InTest2 arg)
    {
        Debug.Log("这是局部消息传递过来的值 + CallBackTest  " + arg.Value);
        await Task.Yield();
    }


}
