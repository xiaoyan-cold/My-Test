using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrodCast : MonoBehaviour
{
    // Start is called before the first frame update
    async void Start()
    {
        await GameManager.Message.Post<MessageType.Test>(new MessageType.Test { data = "这是全局消息的第一次尝试" });
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
