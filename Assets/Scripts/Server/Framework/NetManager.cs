using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System;
using System.Linq;
public enum NetEvent
{
    ConnectSucc,
    ConnectFail,
    Close
}
public class NetManager
{
    //定义套接字
    static Socket socket;
    //接收缓冲区
    static ByteArray readBuff;
    //网络事件委托类型
    public delegate void EventListener(string str);
    //网络事件监听字典
    public static Dictionary<NetEvent, EventListener> eventListeners = new Dictionary<NetEvent, EventListener>();
    //消息委托类型
    public delegate void MsgListener(MsgBase msg);
    //消息事件监听字典
    public static Dictionary<string, MsgListener> msgListeners = new Dictionary<string, MsgListener>();
    //消息发送队列
    public static Queue<ByteArray> writeQueue = new Queue<ByteArray>();
    //消息接收列表
    public static List<MsgBase> msgList = new List<MsgBase>();
    //接收列表长度
    static int msgCount = 0;
    //每一次Update处理的消息量
    readonly static int MAX_MESSAGE_FIRE = 10;
    //是否正在连接
    static bool isConnecting = false;
    //是否正在关闭
    static bool isClosing = false;
    //是否使用心跳
    public static bool isUsePing = true;
    //心跳间隔
    public static float pingInterval = 30f;
    //上一次发送PING时间
    static float lastPingTime = 0f;
    //上一次收到PONG时间
    static float lastPongTime = 0f;
    /// <summary>
    /// 添加网络事件监听
    /// </summary>
    /// <param name="netEvent"></param>
    /// <param name="listener"></param>
    public static void AddNetEventListener(NetEvent netEvent, EventListener listener)
    {
        if (!eventListeners.ContainsKey(netEvent))
        {
            eventListeners.Add(netEvent, listener);
        }
        else
        {
            eventListeners[netEvent] += listener;
        }
    }
    /// <summary>
    /// 移除网络事件监听
    /// </summary>
    /// <param name="netEvent"></param>
    /// <param name="listener"></param>
    public static void RemoveNetEventListener(NetEvent netEvent, EventListener listener)
    {
        if (eventListeners.ContainsKey(netEvent))
        {
            eventListeners[netEvent] -= listener;
            if (eventListeners[netEvent] == null)
            {
                eventListeners.Remove(netEvent);
            }
        }
    }
    /// <summary>
    /// 网络事件分发
    /// </summary>
    /// <param name="netEvent"></param>
    /// <param name="arg"></param>
    public static void FireNetEvent(NetEvent netEvent, string arg = "")
    {
        if (eventListeners.ContainsKey(netEvent))
        {
            eventListeners[netEvent](arg);
        }
    }
    public static void AddMsgEventListener(string msgName, MsgListener listener)
    {
        if (!msgListeners.ContainsKey(msgName))
        {
            msgListeners.Add(msgName, listener);
        }
        else
        {
            msgListeners[msgName] += listener;
        }
    }
    public static void RemoveMsgEventListener(string msgName, MsgListener listener)
    {
        if (msgListeners.ContainsKey(msgName))
        {
            msgListeners[msgName] -= listener;
            if (msgListeners[msgName] == null)
            {
                msgListeners.Remove(msgName);
            }
        }
    }
    public static void FireMsgEvent(MsgBase msg)
    {
        if (msgListeners.ContainsKey(msg.protoName))
        {
            msgListeners[msg.protoName](msg);
        }
    }
    /// <summary>
    /// 获取描述
    /// </summary>
    /// <returns></returns>
    public static string GetDesc()
    {
        if (socket == null) return "未连接";
        if (!socket.Connected) return "未连接";
        return socket.LocalEndPoint.ToString() + "->" + socket.RemoteEndPoint.ToString();
    }
    //重置
    static void InitState()
    {
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        readBuff = new ByteArray();
        writeQueue = new Queue<ByteArray>();
        msgList = new List<MsgBase>();
        msgCount = 0;
        isConnecting = false;
        isClosing = false;//保证每次发起连接时必然不处于关闭中状态
        lastPingTime = Time.time;
        lastPongTime = Time.time;
        //添加系统消息监听
        //客户端掉线重连可能多次调用InitState,MsgPong无须多次监听
        if (!msgListeners.ContainsKey("MsgPong"))
        {
            AddMsgEventListener("MsgPong", OnMsgPong);
        }
    }
    public static void Connect(string ip, int port)
    {
        if (socket != null && socket.Connected)
        {
            Debug.Log("已经连接服务器");
            return;
        }
        if (isConnecting)
        {
            Debug.Log("正在连接服务器");
            return;
        }
        InitState();
        //设置不延迟发送,不使用Nagle算法
        socket.NoDelay = true;
        //连接服务器
        socket.BeginConnect(new IPEndPoint(IPAddress.Parse(ip), port), ConnectCallback, socket);
        isConnecting = true;
    }
    public static void Close()
    {
        if (socket == null || !socket.Connected)
        {
            Debug.Log("未连接服务器");
            return;
        }
        if (isClosing)
        {
            Debug.Log("正在断开连接");
            return;
        }
        if (writeQueue.Count > 0)
        {
            Debug.Log("有未处理消息，等待处理完再断开连接");
            isClosing = true;
            return;
        }
        socket.Close();
        Debug.Log("断开连接");
        FireNetEvent(NetEvent.Close);
    }
    private static void ConnectCallback(IAsyncResult ar)
    {
        try
        {
            Socket socket = (Socket)ar.AsyncState;
            socket.EndConnect(ar);
            Debug.Log("连接服务器成功");
            FireNetEvent(NetEvent.ConnectSucc);
            //连接成功，开始接收数据
            socket.BeginReceive(readBuff.bytes, readBuff.writeIdx, readBuff.remain, 0, ReceiveCallback, socket);
        }
        catch (SocketException e)
        {
            Debug.Log("连接服务器失败" + e.Message);
            FireNetEvent(NetEvent.ConnectFail, e.Message);
        }
        isConnecting = false;
    }

    private static void ReceiveCallback(IAsyncResult ar)
    {
        try
        {
            Socket socket = (Socket)ar.AsyncState;
            int count = socket.EndReceive(ar);
            if (count == 0)
            {
                Debug.Log("收到消息长度为0,服务器断开连接Final");
                Close();
                return;
            }
            readBuff.writeIdx += count;
            //处理二进制消息
            OnReceiveData();
            //继续接收数据
            if(readBuff.remain<8)
            {
                readBuff.MoveBytes();
                readBuff.ReSize(readBuff.length*2);
            }
            socket.BeginReceive(readBuff.bytes, readBuff.writeIdx, readBuff.remain, 0, ReceiveCallback, socket);
        }
        catch (SocketException e)
        {
            Debug.Log("服务器断开连接" + e.Message);
        }
    }

    private static void OnReceiveData()
    {
        //消息长度
        if (readBuff.length < 2)
        {
            //不完整继续接收
            return;
        }
        //获取消息体长度
        int readIdx = readBuff.readIdx;
        byte[] bytes = readBuff.bytes;
        Int16 bodyLength = (Int16)((readBuff.bytes[readIdx + 1] << 8) | readBuff.bytes[readIdx]);
        if (readBuff.length < 2 + bodyLength)
        {
            //不完整继续接收
            return;
        }
        readBuff.readIdx += 2;//跳过长度字段
                              //解析协议名
        int nameCount = 0;
        string protoName = MsgBase.DecodeName(bytes, readBuff.readIdx, out nameCount);
        if (protoName == "")
        {
            Debug.Log("解析协议名失败");
            // readBuff.readIdx += bodyLength;
            // OnReceiveData();
            return;
        }
        readBuff.readIdx += nameCount;//跳过协议名
        //解析消息体
        int bodyCount = bodyLength - nameCount;
        MsgBase msg = MsgBase.Decode(protoName, bytes, readBuff.readIdx, bodyCount);
        if (msg == null)
        {
            Debug.Log("解析消息体失败");
            // readBuff.readIdx += bodyCount;
            // OnReceiveData();
            return;
        }
        readBuff.readIdx += bodyCount;//跳过消息体
        readBuff.CheckAndMoveBytes(); //整理接收缓冲区
        //添加到消息列表
        lock (msgList)
        {
            msgList.Add(msg);
            msgCount++;
        }
        //继续处理下一条消息
        if(readBuff.length >= 2)
        {
            OnReceiveData();
        }
    }

    public static void Send(MsgBase msg)
    {
        if (socket == null || !socket.Connected)
        {
            Debug.Log("未连接服务器");
            return;
        }
        if (isConnecting)
        {
            Debug.Log("正在连接服务器，稍后再发送");
            return;
        }
        if (isClosing)
        {
            Debug.Log("正在断开连接，无法发送");
            return;
        }
        //数据编码
        byte[] nameBytes = MsgBase.EncodeName(msg);
        byte[] bodyBytes = MsgBase.Encode(msg);
        int len = nameBytes.Length + bodyBytes.Length;
        byte[] sendBytes = new byte[2 + len];
        //组装长度
        sendBytes[0] = (byte)(len % 256);//取余,小端序低八位
        sendBytes[1] = (byte)(len / 256);//取商,小端序高八位
        //组装名字
        Array.Copy(nameBytes, 0, sendBytes, 2, nameBytes.Length);
        //组装消息体
        Array.Copy(bodyBytes, 0, sendBytes, 2 + nameBytes.Length, bodyBytes.Length);
        //写入队列
        ByteArray ba = new ByteArray(sendBytes);
        int count = 0;
        lock (writeQueue)
        {
            writeQueue.Enqueue(ba);
            count = writeQueue.Count;
        }
        if(count == 1)
        {
            //队列中只有当前消息，直接发送
            socket.BeginSend(sendBytes, 0, sendBytes.Length, 0, SendCallback, socket);
        }
        else
        {
            Debug.Log("消息已入队，等待发送，当前队列长度：" + count);
        }
    }

    private static void SendCallback(IAsyncResult ar)
    {
        try
        {
            Socket socket = (Socket)ar.AsyncState;
            if (socket == null || !socket.Connected)
            {
                Debug.Log("发送数据失败，连接已断开");
                return;
            }
            //EndSend
            int count = socket.EndSend(ar);
            //获取队列头
            ByteArray ba;
            lock (writeQueue)
            {
                ba = writeQueue.First();
            }
            ba.readIdx += count;
            //如果当前消息发送完毕，出队并发送下一条
            if (ba.length == 0)
            {
                lock (writeQueue)
                {
                    writeQueue.Dequeue();
                    //获取下一条消息
                    ba = writeQueue.First();
                }
            }
            if (ba != null) //下一条消息不为空
            {
                //继续发送消息
                socket.BeginSend(ba.bytes, ba.readIdx, ba.length, 0, SendCallback, socket);
            }
            else
            {
                //没有消息了，检查是否正在关闭连接
                if (isClosing)
                {
                    socket.Close();
                    Debug.Log("断开连接");
                    FireNetEvent(NetEvent.Close);
                }
            }
        }
        catch (SocketException e)
        {
            Debug.Log("发送数据失败" + e.Message);
        }
    }
    void OnConnectSucc(string str)
    {
        Debug.Log("连接服务器成功回调");
    }
    void OnConnectFail(string str)
    {
        Debug.Log("连接服务器失败回调" + str);
    }
    void OnClose(string str)
    {
        Debug.Log("断开服务器连接回调" + str);
    }
    static void OnMsgPong(MsgBase msg)
    {
        Debug.Log("收到PONG消息");
        lastPongTime = Time.time;
    }
    public static void MsgUpdate()
    {
        //初步判断，提升效率
        if (msgCount == 0) return;
        //重复处理消息
        for (int i = 0; i < MAX_MESSAGE_FIRE; i++)
        {
            MsgBase msg = null;
            lock (msgList)
            {
                if (msgCount > 0)
                {
                    msg = msgList[0];
                    msgList.RemoveAt(0);
                    msgCount--;
                }
            }
            if (msg == null) break;
            //分发消息
            FireMsgEvent(msg);
        }
    }
    public static void PingUpdate()
    {
        if (!isUsePing) return;
        float now = Time.time;
        //发送PING
        if (now - lastPingTime > pingInterval)
        {
            MsgPing ping = new MsgPing();
            Send(ping);
            lastPingTime = now;
            Debug.Log("发送PING");
        }
        //检测PONG
        if (now - lastPongTime > pingInterval * 4)
        {
            Debug.Log("超过4倍心跳间隔未收到PONG,断开连接");
            Close();
        }
    }
    public static void Update()
    {
        MsgUpdate();
        PingUpdate();
    }
}
