using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class ChatActionDetail : IMessage<ChatActionDetail>, IMessage, IEquatable<ChatActionDetail>, IDeepCloneable<ChatActionDetail>, IBufferMessage
{
	private static readonly MessageParser<ChatActionDetail> _parser = new MessageParser<ChatActionDetail>((Func<ChatActionDetail>)(() => new ChatActionDetail()));

	private UnknownFieldSet _unknownFields;

	public const int ActionTypeFieldNumber = 1;

	private EChatActionType actionType_;

	public const int UUIDFieldNumber = 2;

	private string uUID_ = "";

	public const int BasicDataFieldNumber = 3;

	private ChatActionBasicData basicData_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<ChatActionDetail> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeChatActionDetailReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

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
	public string UUID
	{
		get
		{
			return uUID_;
		}
		set
		{
			uUID_ = ProtoPreconditions.CheckNotNull<string>(value, "value");
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ChatActionBasicData BasicData
	{
		get
		{
			return basicData_;
		}
		set
		{
			basicData_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ChatActionDetail()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ChatActionDetail(ChatActionDetail other)
		: this()
	{
		actionType_ = other.actionType_;
		uUID_ = other.uUID_;
		basicData_ = ((other.basicData_ != null) ? other.basicData_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ChatActionDetail Clone()
	{
		return new ChatActionDetail(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as ChatActionDetail);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(ChatActionDetail other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (ActionType != other.ActionType)
		{
			return false;
		}
		if (UUID != other.UUID)
		{
			return false;
		}
		if (!object.Equals(BasicData, other.BasicData))
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
		if (ActionType != EChatActionType.EmptyValue)
		{
			num ^= ActionType.GetHashCode();
		}
		if (UUID.Length != 0)
		{
			num ^= UUID.GetHashCode();
		}
		if (basicData_ != null)
		{
			num ^= BasicData.GetHashCode();
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
		if (ActionType != EChatActionType.EmptyValue)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteEnum((int)ActionType);
		}
		if (UUID.Length != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)18);
			((WriteContext)(ref output)).WriteString(UUID);
		}
		if (basicData_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)26);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)BasicData);
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
		if (ActionType != EChatActionType.EmptyValue)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)ActionType);
		}
		if (UUID.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(UUID);
		}
		if (basicData_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)BasicData);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(ChatActionDetail other)
	{
		if (other == null)
		{
			return;
		}
		if (other.ActionType != EChatActionType.EmptyValue)
		{
			ActionType = other.ActionType;
		}
		if (other.UUID.Length != 0)
		{
			UUID = other.UUID;
		}
		if (other.basicData_ != null)
		{
			if (basicData_ == null)
			{
				BasicData = new ChatActionBasicData();
			}
			BasicData.MergeFrom(other.BasicData);
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
				ActionType = (EChatActionType)((ParseContext)(ref input)).ReadEnum();
				break;
			case 18u:
				UUID = ((ParseContext)(ref input)).ReadString();
				break;
			case 26u:
				if (basicData_ == null)
				{
					BasicData = new ChatActionBasicData();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)BasicData);
				break;
			}
		}
	}
}
