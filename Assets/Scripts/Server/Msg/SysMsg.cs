using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 系统协议
/// </summary>
public class MsgPing : MsgBase
{
    public MsgPing()
    {
        protoName = "MsgPing";
    }
}
public class MsgPong : MsgBase
{
    public MsgPong()
    {
        protoName = "MsgPong";
    }
}
