// ============================================================
// 由 proto 定义手动编写的 Protobuf 消息类
// 仅包含网络框架核心依赖的消息
// 后续安装 protoc 后可用自动生成代码替换本文件
// ============================================================

using Google.Protobuf;
using Google.Protobuf.Reflection;
using System;

namespace Game.Network.Protocol
{
    // ── 错误码枚举 ──────────────────────────────
    public enum ErrorCode
    {
        Success          = 0,
        UnknownError     = 1,
        NotAuthenticated = 2,
        InvalidParams    = 3,
        PlayerNotFound   = 4,
        AlreadyLoggedIn  = 5,
        LoginFailed      = 6,
        RegisterFailed   = 7,
        UsernameExists   = 8,
        InvalidToken     = 9,
        SessionExpired   = 10,
    }

    // ── 通用响应 ────────────────────────────────

    public sealed class CommonResponse : IMessage<CommonResponse>
    {
        public int    Code    { get; set; }
        public string Message { get; set; } = "";

        // ── IMessage 实现 ───────────────────────
        public MessageDescriptor Descriptor => null;
        public void MergeFrom(CommonResponse other) { Code = other.Code; Message = other.Message; }
        public void MergeFrom(CodedInputStream input)
        {
            uint tag;
            while ((tag = input.ReadTag()) != 0)
            {
                switch (tag)
                {
                    case 8:  Code = input.ReadInt32(); break;
                    case 18: Message = input.ReadString(); break;
                    default: input.SkipLastField(); break;
                }
            }
        }
        public void WriteTo(CodedOutputStream output)
        {
            if (Code != 0)    { output.WriteTag(1, WireFormat.WireType.Varint);         output.WriteInt32(Code); }
            if (Message != "") { output.WriteTag(2, WireFormat.WireType.LengthDelimited); output.WriteString(Message); }
        }
        public int CalculateSize()
        {
            int size = 0;
            if (Code != 0)    size += 1 + CodedOutputStream.ComputeInt32Size(Code);
            if (Message != "") size += 1 + CodedOutputStream.ComputeStringSize(Message);
            return size;
        }
        public CommonResponse Clone() => new CommonResponse { Code = Code, Message = Message };
        public bool Equals(CommonResponse other) => other != null && Code == other.Code && Message == other.Message;
        public override bool Equals(object obj) => Equals(obj as CommonResponse);
        public override int GetHashCode() => Code.GetHashCode() ^ Message.GetHashCode();
        public override string ToString() => $"CommonResponse {{ Code={Code}, Message={Message} }}";

        // ── Parser（MessageDispatcher 需要）────
        private static readonly MessageParser<CommonResponse> _parser = new(() => new CommonResponse());
        public static MessageParser<CommonResponse> Parser => _parser;
    }

    // ── 心跳请求 ────────────────────────────────

    public sealed class C2S_Heartbeat : IMessage<C2S_Heartbeat>
    {
        public long ClientTime { get; set; }

        public MessageDescriptor Descriptor => null;
        public void MergeFrom(C2S_Heartbeat other) { ClientTime = other.ClientTime; }
        public void MergeFrom(CodedInputStream input)
        {
            uint tag;
            while ((tag = input.ReadTag()) != 0)
            {
                switch (tag)
                {
                    case 8: ClientTime = input.ReadInt64(); break;
                    default: input.SkipLastField(); break;
                }
            }
        }
        public void WriteTo(CodedOutputStream output)
        {
            if (ClientTime != 0) { output.WriteTag(1, WireFormat.WireType.Varint); output.WriteInt64(ClientTime); }
        }
        public int CalculateSize()
        {
            int size = 0;
            if (ClientTime != 0) size += 1 + CodedOutputStream.ComputeInt64Size(ClientTime);
            return size;
        }
        public C2S_Heartbeat Clone() => new C2S_Heartbeat { ClientTime = ClientTime };
        public bool Equals(C2S_Heartbeat other) => other != null && ClientTime == other.ClientTime;
        public override bool Equals(object obj) => Equals(obj as C2S_Heartbeat);
        public override int GetHashCode() => ClientTime.GetHashCode();
        public override string ToString() => $"C2S_Heartbeat {{ ClientTime={ClientTime} }}";

        private static readonly MessageParser<C2S_Heartbeat> _parser = new(() => new C2S_Heartbeat());
        public static MessageParser<C2S_Heartbeat> Parser => _parser;
    }

    // ── 心跳响应 ────────────────────────────────

    public sealed class S2C_Heartbeat : IMessage<S2C_Heartbeat>
    {
        public long ServerTime { get; set; }

        public MessageDescriptor Descriptor => null;
        public void MergeFrom(S2C_Heartbeat other) { ServerTime = other.ServerTime; }
        public void MergeFrom(CodedInputStream input)
        {
            uint tag;
            while ((tag = input.ReadTag()) != 0)
            {
                switch (tag)
                {
                    case 8: ServerTime = input.ReadInt64(); break;
                    default: input.SkipLastField(); break;
                }
            }
        }
        public void WriteTo(CodedOutputStream output)
        {
            if (ServerTime != 0) { output.WriteTag(1, WireFormat.WireType.Varint); output.WriteInt64(ServerTime); }
        }
        public int CalculateSize()
        {
            int size = 0;
            if (ServerTime != 0) size += 1 + CodedOutputStream.ComputeInt64Size(ServerTime);
            return size;
        }
        public S2C_Heartbeat Clone() => new S2C_Heartbeat { ServerTime = ServerTime };
        public bool Equals(S2C_Heartbeat other) => other != null && ServerTime == other.ServerTime;
        public override bool Equals(object obj) => Equals(obj as S2C_Heartbeat);
        public override int GetHashCode() => ServerTime.GetHashCode();
        public override string ToString() => $"S2C_Heartbeat {{ ServerTime={ServerTime} }}";

        private static readonly MessageParser<S2C_Heartbeat> _parser = new(() => new S2C_Heartbeat());
        public static MessageParser<S2C_Heartbeat> Parser => _parser;
    }

    // ── 重连请求 ────────────────────────────────

    public sealed class C2S_Reconnect : IMessage<C2S_Reconnect>
    {
        public string Token { get; set; } = "";

        public MessageDescriptor Descriptor => null;
        public void MergeFrom(C2S_Reconnect other) { Token = other.Token; }
        public void MergeFrom(CodedInputStream input)
        {
            uint tag;
            while ((tag = input.ReadTag()) != 0)
            {
                switch (tag)
                {
                    case 10: Token = input.ReadString(); break;
                    default: input.SkipLastField(); break;
                }
            }
        }
        public void WriteTo(CodedOutputStream output)
        {
            if (Token != "") { output.WriteTag(1, WireFormat.WireType.LengthDelimited); output.WriteString(Token); }
        }
        public int CalculateSize()
        {
            int size = 0;
            if (Token != "") size += 1 + CodedOutputStream.ComputeStringSize(Token);
            return size;
        }
        public C2S_Reconnect Clone() => new C2S_Reconnect { Token = Token };
        public bool Equals(C2S_Reconnect other) => other != null && Token == other.Token;
        public override bool Equals(object obj) => Equals(obj as C2S_Reconnect);
        public override int GetHashCode() => Token.GetHashCode();
        public override string ToString() => $"C2S_Reconnect {{ Token={Token} }}";

        private static readonly MessageParser<C2S_Reconnect> _parser = new(() => new C2S_Reconnect());
        public static MessageParser<C2S_Reconnect> Parser => _parser;
    }
}
