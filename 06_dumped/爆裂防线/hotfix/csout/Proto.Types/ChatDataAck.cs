using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class ChatDataAck : IMessage<ChatDataAck>, IMessage, IEquatable<ChatDataAck>, IDeepCloneable<ChatDataAck>, IBufferMessage
{
	private static readonly MessageParser<ChatDataAck> _parser = new MessageParser<ChatDataAck>((Func<ChatDataAck>)(() => new ChatDataAck()));

	private UnknownFieldSet _unknownFields;

	public const int ChatTypeFieldNumber = 1;

	private EChatType chatType_;

	public const int ChatListFieldNumber = 2;

	private static readonly FieldCodec<ChatDetail> _repeated_chatList_codec = FieldCodec.ForMessage<ChatDetail>(18u, ChatDetail.Parser);

	private readonly RepeatedField<ChatDetail> chatList_ = new RepeatedField<ChatDetail>();

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<ChatDataAck> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeChatDataAckReflection.Descriptor.MessageTypes[0];

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
	public RepeatedField<ChatDetail> ChatList => chatList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ChatDataAck()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ChatDataAck(ChatDataAck other)
		: this()
	{
		chatType_ = other.chatType_;
		chatList_ = other.chatList_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ChatDataAck Clone()
	{
		return new ChatDataAck(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as ChatDataAck);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(ChatDataAck other)
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
		if (!chatList_.Equals(other.chatList_))
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
		num ^= ((object)chatList_).GetHashCode();
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
		chatList_.WriteTo(ref output, _repeated_chatList_codec);
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
		num += chatList_.CalculateSize(_repeated_chatList_codec);
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(ChatDataAck other)
	{
		if (other != null)
		{
			if (other.ChatType != EChatType.EmptyValue)
			{
				ChatType = other.ChatType;
			}
			chatList_.Add((IEnumerable<ChatDetail>)other.chatList_);
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
			case 18u:
				chatList_.AddEntriesFrom(ref input, _repeated_chatList_codec);
				break;
			}
		}
	}
}
