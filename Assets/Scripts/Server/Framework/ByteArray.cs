using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ByteArray
{
    //默认大小
    const int DEFAULT_SIZE = 1024;
    //初始大小
    int initSize = 0;
    //缓冲区
    public byte[] bytes;
    //读写索引
    public int readIdx; //可读位置，缓冲区有效数据起始位置
    public int writeIdx; //可写位置，缓冲区有效数据结束位置
    //容量
    public int capacity = 0;
    //剩余空间
    public int remain => capacity - writeIdx;
    //数据长度
    public int length => writeIdx - readIdx;
    public ByteArray(byte[] defaultBytes)
    {
        bytes = defaultBytes;
        capacity = defaultBytes.Length;
        initSize = defaultBytes.Length;
        readIdx = 0;
        writeIdx = defaultBytes.Length;
    }
    public ByteArray(int size = DEFAULT_SIZE)
    {
        if (size <= 0) size = DEFAULT_SIZE;
        bytes = new byte[size];
        capacity = size;
        initSize = size;
        readIdx = 0;
        writeIdx = 0;
    }
    //重设尺寸
    public void ReSize(int minSize)
    {
        if (minSize < length) return;
        if (minSize < initSize) return;
        int n = 1;
        while (n < minSize)
        {
            n *= 2;
        }
        capacity = n;
        byte[] newBytes = new byte[capacity];
        Array.Copy(bytes, readIdx, newBytes, 0, writeIdx - readIdx);
        bytes = newBytes;
        writeIdx = length;
        readIdx = 0;
    }
    //检查有效数据是否过小并移动数据
    public void CheckAndMoveBytes()
    {
        if (length < 8)
        {
            MoveBytes();
        }
    }
    //移动数据
    public void MoveBytes()
    {
        if (length > 0)
        {
            Array.Copy(bytes, readIdx, bytes, 0, length);
        }
        writeIdx = length;
        readIdx = 0;
    }
    //写入数据
    public int Write(byte[] bs, int offset, int count)
    {
        if (remain < count)
        {
            ReSize(length + count);
        }
        Array.Copy(bs, offset, bytes, writeIdx, count);
        writeIdx += count;
        return count;
    }
    //读取数据
    public int Read(byte[] bs, int offset, int count)
    {
        count = Math.Min(count, length);
        Array.Copy(bytes, readIdx, bs, offset, count);
        readIdx += count;
        CheckAndMoveBytes();
        return count;
    }
    //读取Int16,小端序
    public Int16 ReadInt16()
    {
        if (length < 2) return 0;
        Int16 ret = (Int16)(bytes[readIdx] << 8 | bytes[readIdx + 1]);
        readIdx += 2;
        CheckAndMoveBytes();
        return ret;
    }
    //读取Int32,小端序
    public Int32 ReadInt32()
    {
        if (length < 4) return 0;
        Int32 ret = (Int32)(bytes[readIdx] << 24 | bytes[readIdx + 1] << 16 | bytes[readIdx + 2] << 8 | bytes[readIdx + 3]);
        readIdx += 4;
        CheckAndMoveBytes();
        return ret;
    }
    public override string ToString()
    {
        return BitConverter.ToString(bytes, readIdx, length);
    }
    public string Debug()
    {
        return string.Format("readIdx:{0} writeIdx:{1} length:{2} capacity:{3} remain:{4}",
        readIdx, writeIdx, length, capacity, remain);
    }
}
