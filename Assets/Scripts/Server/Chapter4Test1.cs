using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System.Net;
using System;
using System.Linq;
using UnityEngine.UI;
using Unity.Mathematics;
public class Chapter4Test1 : MonoBehaviour
    {
        //UGUI
        public Text text;
        public InputField input;
        public Button connectBtn;
        public Button sendBtn;
        Socket socket;
        //接收缓冲区
        ByteArray readBuff = new ByteArray();
        string recvStr = "";
        //发送队列
        Queue<ByteArray> writeQueue = new Queue<ByteArray>();
        //是否准备断开连接
        public bool isClosing = false;
        public void Connection(string ip, int port)
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.BeginConnect(new IPEndPoint(IPAddress.Parse(ip), port), ConnectCallback, socket);
        }

        private void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                Socket socket = (Socket)ar.AsyncState;
                socket.EndConnect(ar);
                Debug.Log("连接成功");
                socket.BeginReceive
                (readBuff.bytes,readBuff.writeIdx,readBuff.remain, 0, ReceiveCallback, socket);
            }
            catch (Exception e)
            {
                Debug.Log("连接失败" + e.Message);
            }
        }
        public void OnConnectClick()
        {
            Connection("127.0.0.1",33333);
        }
        public void OnSendClick()
        {
            string str = input.text;
            Send(str);
        }
        public void Send(string str)
        {
            if (socket == null || !socket.Connected)
            {
                Debug.Log("未连接服务器");
                return;
            }
            if(isClosing)
            {
                Debug.Log("正在断开连接,无法发送新数据");
                return;
            }
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(str);
            Int16 length = (short)bytes.Length;
            byte[] lenBytes = BitConverter.GetBytes(length);
            //统一为小端序
            if (!BitConverter.IsLittleEndian)
            {
                lenBytes = lenBytes.Reverse().ToArray();
            }
            //组合协议,消息长度+消息体
            byte[] sendBytes = lenBytes.Concat(bytes).ToArray();
            Debug.Log("[Send] " + BitConverter.ToString(sendBytes));
            //加入发送队列
            ByteArray ba = new ByteArray(bytes);
            int count = 0;
            lock (writeQueue)
            {
                writeQueue.Enqueue(ba);
                count = writeQueue.Count;
            }
            //send
            if (count == 1)
            {
                socket.BeginSend(ba.bytes, ba.readIdx, ba.length, 0, SendCallback, socket);
            }
        }

        private void SendCallback(IAsyncResult ar)
        {
            Socket socket = (Socket)ar.AsyncState;
            int count = socket.EndSend(ar);

            ByteArray ba;
            lock (writeQueue)
            {
                ba = writeQueue.First();
            }

            ba.readIdx += count;
            if (ba.length == 0)
            {
                lock (writeQueue)
                {
                    writeQueue.Dequeue();
                    if (writeQueue.Count > 0)
                    {
                        ba = writeQueue.First();
                        socket.BeginSend(ba.bytes, ba.readIdx, ba.length, 0, SendCallback, socket);
                    }
                    else
                    {
                        //剩余数据发送完毕，断开连接
                        if (isClosing)
                        {
                            socket.Close();
                            socket = null;
                            Debug.Log("断开连接");
                        }
                    }
                }
            }
            else
            {
                socket.BeginSend(ba.bytes, ba.readIdx, ba.length, 0, SendCallback, socket);
            }
        }
        public void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                Socket socket = (Socket)ar.AsyncState;
                //获取接收数据长度
                int count = socket.EndReceive(ar);
                readBuff.writeIdx += count;
                //处理二进制消息
                OnReceiveData();
                //继续接收数据
                if(readBuff.remain<8)
                {
                    readBuff.MoveBytes(); //尝试数据前移，增加剩余容量
                    if(readBuff.remain<8)
                    {
                        readBuff.ReSize(readBuff.length*2);//扩容
                    }
                }
                socket.BeginReceive(readBuff.bytes, readBuff.writeIdx, readBuff.remain, 0, ReceiveCallback, socket);
            }
            catch (SocketException e)
            {
                Debug.Log("服务器断开连接" + e.Message);
            }
        }

        private void OnReceiveData()
        {
            Debug.Log("[Recv 1] buffCount:" + readBuff.length);
            //长度不够，继续接收
            if (readBuff.length < 2) return;
            Debug.Log("[Recv 2] readBuff:" + readBuff.ToString());


            byte[] bytes = readBuff.bytes;
            Int16 bodyLength = (Int16)(readBuff.bytes[readBuff.readIdx+1] << 8 | readBuff.bytes[readBuff.readIdx]);

            if (readBuff.length < 2 + bodyLength) return;//收到的数据长度小于应有长度，继续接收
            //消息体长度
            readBuff.readIdx += 2;
            Debug.Log("[Recv 3] bodyLength:" + bodyLength);
            //消息体
            byte[] stringBytes = new byte[bodyLength];
            readBuff.Read(stringBytes, 0, bodyLength);
            string s = System.Text.Encoding.UTF8.GetString(stringBytes, 0, bodyLength);

            Debug.Log("[Recv 4] s:" + s);
            Debug.Log("[Recv 5] buffCount:" + readBuff.length);
            //消息处理(聊天室)
            recvStr = s;
            //继续读取消息
            OnReceiveData();
        }
        public void Close()
        {
            //还有数据要发送
            if (writeQueue.Count>0)
            {
                isClosing = true;
                return;
            }
            else
            {
                socket.Close();
                socket = null;
                Debug.Log("断开连接");
            }
        }
        void Start()
        {
            connectBtn.onClick.AddListener(OnConnectClick);
            sendBtn.onClick.AddListener(OnSendClick);
        }
        void Update()
        {
            text.text = recvStr;
        }
    }
