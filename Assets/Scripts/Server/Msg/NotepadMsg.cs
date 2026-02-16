using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MsgGetText : MsgBase
{
    public MsgGetText() { protoName = "MsgGetText"; }
    public string text = "";
}
public class MsgSaveText : MsgBase
{
    public MsgSaveText() { protoName = "MsgSaveText"; }
    public string text = "";
    public int result = 0;
}