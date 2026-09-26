using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class ChatSendReq : IMessage<ChatSendReq>, IMessage, IEquatable<ChatSendReq>, IDeepCloneable<ChatSendReq>, IBufferMessage
{
	private static readonly MessageParser<ChatSendReq> _parser = new MessageParser<ChatSendReq>((Func<ChatSendReq>)(() => new ChatSendReq()));

	private UnknownFieldSet _unknownFields;

	public const int ChatTypeFieldNumber = 1;

	private EChatType chatType_;

	public const int ChatTargetFieldNumber = 2;

	private long chatTarget_;

	public const int ContentFieldNumber = 3;

	private string content_ = "";

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<ChatSendReq> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeChatSendReqReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public EChatType ChatType
	{
		get
		{
			return chatType_;
		}
		set
		{
			chatType_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public long ChatTarget
	{
		get
		{
			return chatTarget_;
		}
		set
		{
			chatTarget_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public string Content
	{
		get
		{
			return content_;
		}
		set
		{
			content_ = ProtoPreconditions.CheckNotNull<string>(value, "value");
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ChatSendReq()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ChatSendReq(ChatSendReq other)
		: this()
	{
		chatType_ = other.chatType_;
		chatTarget_ = other.chatTarget_;
		content_ = other.content_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ChatSendReq Clone()
	{
		return new ChatSendReq(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as ChatSendReq);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(ChatSendReq other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (ChatType != other.ChatType)
		{
			return false;
		}
		if (ChatTarget != other.ChatTarget)
		{
			return false;
		}
		if (Content != other.Content)
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override int GetHashCode()
	{
		int num = 1;
		if (ChatType != EChatType.EmptyValue)
		{
			num ^= ChatType.GetHashCode();
		}
		if (ChatTarget != 0L)
		{
			num ^= ChatTarget.GetHashCode();
		}
		if (Content.Length != 0)
		{
			num ^= Content.GetHashCode();
		}
		if (_unknownFields != null)
		{
			num ^= ((object)_unknownFields).GetHashCode();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override string ToString()
	{
		return JsonFormatter.ToDiagnosticString((IMessage)(object)this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void WriteTo(CodedOutputStream output)
	{
		output.WriteRawMessage((IMessage)(object)this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	void IBufferMessage.InternalWriteTo(ref WriteContext output)
	{
		if (ChatType != EChatType.EmptyValue)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteEnum((int)ChatType);
		}
		if (ChatTarget != 0L)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteInt64(ChatTarget);
		}
		if (Content.Length != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)26);
			((WriteContext)(ref output)).WriteString(Content);
		}
		if (_unknownFields != null)
		{
			_unknownFields.WriteTo(ref output);
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int CalculateSize()
	{
		int num = 0;
		if (ChatType != EChatType.EmptyValue)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)ChatType);
		}
		if (ChatTarget != 0L)
		{
			num += 1 + CodedOutputStream.ComputeInt64Size(ChatTarget);
		}
		if (Content.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(Content);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(ChatSendReq other)
	{
		if (other != null)
		{
			if (other.ChatType != EChatType.EmptyValue)
			{
				ChatType = other.ChatType;
			}
			if (other.ChatTarget != 0L)
			{
				ChatTarget = other.ChatTarget;
			}
			if (other.Content.Length != 0)
			{
				Content = other.Content;
			}
			_unknownFields = UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(CodedInputStream input)
	{
		input.ReadRawMessage((IMessage)(object)this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	void IBufferMessage.InternalMergeFrom(ref ParseContext input)
	{
		uint num;
		while ((num = ((ParseContext)(ref input)).ReadTag()) != 0)
		{
			switch (num)
			{
			default:
				_unknownFields = UnknownFieldSet.MergeFieldFrom(_unknownFields, ref input);
				break;
			case 8u:
				ChatType = (EChatType)((ParseContext)(ref input)).ReadEnum();
				break;
			case 16u:
				ChatTarget = ((ParseContext)(ref input)).ReadInt64();
				break;
			case 26u:
				Content = ((ParseContext)(ref input)).ReadString();
				break;
			}
		}
	}
}
