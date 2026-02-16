using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[Serializable]
public class MsgBase
{
    public string protoName = "";
    /// <summary>
    /// 编码,json序列化,返回字节数组
    /// </summary>
    /// <param name="msg"></param>
    /// <returns></returns>
    public static byte[] Encode(MsgBase msg)
    {
        return System.Text.Encoding.UTF8.GetBytes(JsonUtility.ToJson(msg));
    }
    /// <summary>
    /// 解码,json反序列化
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="bytes"></param>
    /// <param name="offset"></param>
    /// <param name="count"></param>
    /// <returns></returns>
    public static MsgBase Decode(string protoName,byte[] bytes, int offset, int count)
    {
        string json = System.Text.Encoding.UTF8.GetString(bytes, offset, count);
        return JsonUtility.FromJson(json,Type.GetType(protoName)) as MsgBase;
    }
    public static byte[] EncodeName(MsgBase msg)
    {
        byte[] nameBytes = System.Text.Encoding.UTF8.GetBytes(msg.protoName);
        Int16 nameLen = (Int16)nameBytes.Length;

        byte[] bytes = new byte[2 + nameLen];
        bytes[0] = (byte)(nameLen % 256);//取余,小端序低八位
        bytes[1] = (byte)(nameLen / 256);//取商,小端序高八位
        Array.Copy(nameBytes, 0, bytes, 2, nameLen);
        return bytes;
    }
    public static string DecodeName(byte[] bytes, int offset, out int count)
    {
        count = 0;
        //有效长度需大于2子节
        if (offset + 2 > bytes.Length) return "";

        //读取长度
        Int16 len = (Int16)((bytes[offset + 1] << 8) | bytes[offset]);
        if (len < 0) return "";
        //长度必须足够
        if (offset + 2 + len > bytes.Length) return "";

        //解析
        count = 2 + len;
        string msgName = System.Text.Encoding.UTF8.GetString(bytes, offset+2, len);
        return msgName; 
    }
}
