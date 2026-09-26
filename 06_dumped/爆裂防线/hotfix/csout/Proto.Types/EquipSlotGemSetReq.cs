using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class EquipSlotGemSetReq : IMessage<EquipSlotGemSetReq>, IMessage, IEquatable<EquipSlotGemSetReq>, IDeepCloneable<EquipSlotGemSetReq>, IBufferMessage
{
	private static readonly MessageParser<EquipSlotGemSetReq> _parser = new MessageParser<EquipSlotGemSetReq>((Func<EquipSlotGemSetReq>)(() => new EquipSlotGemSetReq()));

	private UnknownFieldSet _unknownFields;

	public const int GemPlanIdFieldNumber = 1;

	private int gemPlanId_;

	public const int EquipTypeFieldNumber = 2;

	private EEquipType equipType_;

	public const int GemTidFieldNumber = 3;

	private int gemTid_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<EquipSlotGemSetReq> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeEquipSlotGemSetReqReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int GemPlanId
	{
		get
		{
			return gemPlanId_;
		}
		set
		{
			gemPlanId_ = value;
		}
	}

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
	public int GemTid
	{
		get
		{
			return gemTid_;
		}
		set
		{
			gemTid_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public EquipSlotGemSetReq()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public EquipSlotGemSetReq(EquipSlotGemSetReq other)
		: this()
	{
		gemPlanId_ = other.gemPlanId_;
		equipType_ = other.equipType_;
		gemTid_ = other.gemTid_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public EquipSlotGemSetReq Clone()
	{
		return new EquipSlotGemSetReq(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as EquipSlotGemSetReq);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(EquipSlotGemSetReq other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (GemPlanId != other.GemPlanId)
		{
			return false;
		}
		if (EquipType != other.EquipType)
		{
			return false;
		}
		if (GemTid != other.GemTid)
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
		if (GemPlanId != 0)
		{
			num ^= GemPlanId.GetHashCode();
		}
		if (EquipType != EEquipType.EmptyValue)
		{
			num ^= EquipType.GetHashCode();
		}
		if (GemTid != 0)
		{
			num ^= GemTid.GetHashCode();
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
		if (GemPlanId != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteInt32(GemPlanId);
		}
		if (EquipType != EEquipType.EmptyValue)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteEnum((int)EquipType);
		}
		if (GemTid != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)24);
			((WriteContext)(ref output)).WriteInt32(GemTid);
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
		if (GemPlanId != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(GemPlanId);
		}
		if (EquipType != EEquipType.EmptyValue)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)EquipType);
		}
		if (GemTid != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(GemTid);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(EquipSlotGemSetReq other)
	{
		if (other != null)
		{
			if (other.GemPlanId != 0)
			{
				GemPlanId = other.GemPlanId;
			}
			if (other.EquipType != EEquipType.EmptyValue)
			{
				EquipType = other.EquipType;
			}
			if (other.GemTid != 0)
			{
				GemTid = other.GemTid;
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
				GemPlanId = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 16u:
				EquipType = (EEquipType)((ParseContext)(ref input)).ReadEnum();
				break;
			case 24u:
				GemTid = ((ParseContext)(ref input)).ReadInt32();
				break;
			}
		}
	}
}
