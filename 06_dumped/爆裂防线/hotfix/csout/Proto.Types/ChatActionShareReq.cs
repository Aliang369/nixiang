using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class ChatActionShareReq : IMessage<ChatActionShareReq>, IMessage, IEquatable<ChatActionShareReq>, IDeepCloneable<ChatActionShareReq>, IBufferMessage
{
	private static readonly MessageParser<ChatActionShareReq> _parser = new MessageParser<ChatActionShareReq>((Func<ChatActionShareReq>)(() => new ChatActionShareReq()));

	private UnknownFieldSet _unknownFields;

	public const int ChatTypeFieldNumber = 1;

	private EChatType chatType_;

	public const int ActionTypeFieldNumber = 2;

	private EChatActionType actionType_;

	public const int ActionParamsFieldNumber = 3;

	private ChatActionParams actionParams_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<ChatActionShareReq> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeChatActionShareReqReflection.Descriptor.MessageTypes[0];

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
	public EChatActionType ActionType
	{
		get
		{
			return actionType_;
		}
		set
		{
			actionType_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ChatActionParams ActionParams
	{
		get
		{
			return actionParams_;
		}
		set
		{
			actionParams_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ChatActionShareReq()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ChatActionShareReq(ChatActionShareReq other)
		: this()
	{
		chatType_ = other.chatType_;
		actionType_ = other.actionType_;
		actionParams_ = ((other.actionParams_ != null) ? other.actionParams_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ChatActionShareReq Clone()
	{
		return new ChatActionShareReq(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as ChatActionShareReq);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(ChatActionShareReq other)
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
		if (ActionType != other.ActionType)
		{
			return false;
		}
		if (!object.Equals(ActionParams, other.ActionParams))
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
		if (ActionType != EChatActionType.EmptyValue)
		{
			num ^= ActionType.GetHashCode();
		}
		if (actionParams_ != null)
		{
			num ^= ActionParams.GetHashCode();
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
		if (ActionType != EChatActionType.EmptyValue)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteEnum((int)ActionType);
		}
		if (actionParams_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)26);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)ActionParams);
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
		if (ActionType != EChatActionType.EmptyValue)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)ActionType);
		}
		if (actionParams_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)ActionParams);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(ChatActionShareReq other)
	{
		if (other == null)
		{
			return;
		}
		if (other.ChatType != EChatType.EmptyValue)
		{
			ChatType = other.ChatType;
		}
		if (other.ActionType != EChatActionType.EmptyValue)
		{
			ActionType = other.ActionType;
		}
		if (other.actionParams_ != null)
		{
			if (actionParams_ == null)
			{
				ActionParams = new ChatActionParams();
			}
			ActionParams.MergeFrom(other.ActionParams);
		}
		_unknownFields = UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
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
				ActionType = (EChatActionType)((ParseContext)(ref input)).ReadEnum();
				break;
			case 26u:
				if (actionParams_ == null)
				{
					ActionParams = new ChatActionParams();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)ActionParams);
				break;
			}
		}
	}
}
