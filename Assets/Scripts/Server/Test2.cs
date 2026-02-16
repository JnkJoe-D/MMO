using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using UnityEngine.UI;
using System;
using System.Threading;
/// <summary>
/// 异步，非阻塞
/// </summary>
public class Test2 : MonoBehaviour
{
    Socket socket;

    public InputField InputField;
    public Text text;

    //接收缓冲区
    byte[] readBuff = new byte[1024];
    string recvStr = "";

    public void Connection()
    {
        socket = new Socket
            (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        socket.BeginConnect("127.0.0.1", 33333,ConnectCallback,socket);
    }

    public void ConnectCallback(IAsyncResult ar)
    {
        try
        {
            Socket socket = (Socket)ar.AsyncState;
            socket.EndConnect(ar);
            Debug.Log("Socket Connect Succeed");
            socket.BeginReceive(readBuff, 0, 1024, 0, ReceiveCallback, socket);
        }
        catch(SocketException ex)
        {
            Debug.Log("Socket Connect fail" + ex.ToString());
        }
    }

    private void ReceiveCallback(IAsyncResult ar)
    {
        try
        {
            Socket socket = (Socket)ar.AsyncState;
            int count = socket.EndReceive(ar);
            recvStr = System.Text.Encoding.Default.GetString(readBuff, 0, count);

            socket.BeginReceive(readBuff, 0, 1024, 0, ReceiveCallback, socket);
        }
        catch(SocketException ex)
        {
            Debug.Log("Socket Receive fail" + ex.ToString());
        }
    }

    public void Send()
    {
        if (socket == null) Connection();
        //send
        string sendStr = InputField.text;
        byte[] sendBytes = System.Text.Encoding.Default.GetBytes(sendStr);

        socket.BeginSend(sendBytes, 0, sendBytes.Length, 0, SendCallback, socket);
    }

    private void SendCallback(IAsyncResult ar)
    {
        try
        {
            Socket socket = (Socket)ar.AsyncState;
            int count = socket.EndSend(ar);
            Debug.Log("Socket Send succeed"+count);
        }
        catch (SocketException ex)
        {
            Debug.Log("Socket Send fail" + ex.ToString());
        }
    }

    private void Update()
    {
        text.text = recvStr;
    }
}
