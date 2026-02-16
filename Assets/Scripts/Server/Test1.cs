using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using UnityEngine.UI;
/// <summary>
/// 同步，阻塞
/// </summary>
public class Test1 : MonoBehaviour
{
    Socket socket;

    public InputField InputField;
    public Text text;
    
    public void Connection()
    {
        socket = new Socket
            (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        socket.Connect("127.0.0.1",33333);
    }

    public void Send()
    {
        if (socket == null) Connection();
        //send
        string sendStr = InputField.text;
        byte[] sendBytes = System.Text.Encoding.Default.GetBytes(sendStr);
        //阻塞方法Send
        socket.Send(sendBytes); 

        //receive
        byte[] readBuff = new byte[1024];
        //阻塞方法Receive,接受到服务器消息后才往下执行
        int count = socket.Receive(readBuff);
        string recvStr = System.Text.Encoding.Default.GetString(readBuff, 0, count);
        text.text = recvStr;

        //close
        socket.Close();
    }
}
