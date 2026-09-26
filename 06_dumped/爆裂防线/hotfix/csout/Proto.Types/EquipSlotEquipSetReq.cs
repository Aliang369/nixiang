using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class EquipSlotEquipSetReq : IMessage<EquipSlotEquipSetReq>, IMessage, IEquatable<EquipSlotEquipSetReq>, IDeepCloneable<EquipSlotEquipSetReq>, IBufferMessage
{
	private static readonly MessageParser<EquipSlotEquipSetReq> _parser = new MessageParser<EquipSlotEquipSetReq>((Func<EquipSlotEquipSetReq>)(() => new EquipSlotEquipSetReq()));

	private UnknownFieldSet _unknownFields;

	public const int EquipTypeFieldNumber = 1;

	private EEquipType equipType_;

	public const int EquipIdFieldNumber = 2;

	private string equipId_ = "";

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<EquipSlotEquipSetReq> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeEquipSlotEquipSetReqReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public EEquipType EquipType
	{
		get
		{
			return equipType_;
		}
		set
		{
			equipType_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public string EquipId
	{
		get
		{
			return equipId_;
		}
		set
		{
			equipId_ = ProtoPreconditions.CheckNotNull<string>(value, "value");
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public EquipSlotEquipSetReq()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public EquipSlotEquipSetReq(EquipSlotEquipSetReq other)
		: this()
	{
		equipType_ = other.equipType_;
		equipId_ = other.equipId_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public EquipSlotEquipSetReq Clone()
	{
		return new EquipSlotEquipSetReq(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as EquipSlotEquipSetReq);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(EquipSlotEquipSetReq other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (EquipType != other.EquipType)
		{
			return false;
		}
		if (EquipId != other.EquipId)
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
		if (EquipType != EEquipType.EmptyValue)
		{
			num ^= EquipType.GetHashCode();
		}
		if (EquipId.Length != 0)
		{
			num ^= EquipId.GetHashCode();
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
		if (EquipType != EEquipType.EmptyValue)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteEnum((int)EquipType);
		}
		if (EquipId.Length != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)18);
			((WriteContext)(ref output)).WriteString(EquipId);
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
		if (EquipType != EEquipType.EmptyValue)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)EquipType);
		}
		if (EquipId.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(EquipId);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(EquipSlotEquipSetReq other)
	{
		if (other != null)
		{
			if (other.EquipType != EEquipType.EmptyValue)
			{
				EquipType = other.EquipType;
			}
			if (other.EquipId.Length != 0)
			{
				EquipId = other.EquipId;
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
				EquipType = (EEquipType)((ParseContext)(ref input)).ReadEnum();
				break;
			case 18u:
				EquipId = ((ParseContext)(ref input)).ReadString();
				break;
			}
		}
	}
}
