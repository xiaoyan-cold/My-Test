using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrodCast : MonoBehaviour
{
    // Start is called before the first frame update
    async void Start()
    {
        await GameManager.Message.Post<MessageType.Test>(new MessageType.Test { data = "����ȫ����Ϣ�ĵ�һ�γ���" });
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
