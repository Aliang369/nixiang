using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class EquipSubAttrDetail : IMessage<EquipSubAttrDetail>, IMessage, IEquatable<EquipSubAttrDetail>, IDeepCloneable<EquipSubAttrDetail>, IBufferMessage
{
	private static readonly MessageParser<EquipSubAttrDetail> _parser = new MessageParser<EquipSubAttrDetail>((Func<EquipSubAttrDetail>)(() => new EquipSubAttrDetail()));

	private UnknownFieldSet _unknownFields;

	public const int AttrTypeFieldNumber = 1;

	private EAttrType attrType_;

	public const int FuncIdFieldNumber = 2;

	private int funcId_;

	public const int AttrIdFieldNumber = 3;

	private int attrId_;

	public const int ValueFieldNumber = 4;

	private float value_;

	public const int ExceFieldNumber = 5;

	private EBoolState exce_;

	public const int ShowPctFieldNumber = 6;

	private EBoolState showPct_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<EquipSubAttrDetail> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeEquipSubAttrDetailReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public EAttrType AttrType
	{
		get
		{
			return attrType_;
		}
		set
		{
			attrType_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int FuncId
	{
		get
		{
			return funcId_;
		}
		set
		{
			funcId_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int AttrId
	{
		get
		{
			return attrId_;
		}
		set
		{
			attrId_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public float Value
	{
		get
		{
			return value_;
		}
		set
		{
			value_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public EBoolState Exce
	{
		get
		{
			return exce_;
		}
		set
		{
			exce_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public EBoolState ShowPct
	{
		get
		{
			return showPct_;
		}
		set
		{
			showPct_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public EquipSubAttrDetail()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public EquipSubAttrDetail(EquipSubAttrDetail other)
		: this()
	{
		attrType_ = other.attrType_;
		funcId_ = other.funcId_;
		attrId_ = other.attrId_;
		value_ = other.value_;
		exce_ = other.exce_;
		showPct_ = other.showPct_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public EquipSubAttrDetail Clone()
	{
		return new EquipSubAttrDetail(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as EquipSubAttrDetail);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(EquipSubAttrDetail other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (AttrType != other.AttrType)
		{
			return false;
		}
		if (FuncId != other.FuncId)
		{
			return false;
		}
		if (AttrId != other.AttrId)
		{
			return false;
		}
		if (!ProtobufEqualityComparers.BitwiseSingleEqualityComparer.Equals(Value, other.Value))
		{
			return false;
		}
		if (Exce != other.Exce)
		{
			return false;
		}
		if (ShowPct != other.ShowPct)
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
		if (AttrType != EAttrType.EmptyValue)
		{
			num ^= AttrType.GetHashCode();
		}
		if (FuncId != 0)
		{
			num ^= FuncId.GetHashCode();
		}
		if (AttrId != 0)
		{
			num ^= AttrId.GetHashCode();
		}
		if (Value != 0f)
		{
			num ^= ProtobufEqualityComparers.BitwiseSingleEqualityComparer.GetHashCode(Value);
		}
		if (Exce != EBoolState.False)
		{
			num ^= Exce.GetHashCode();
		}
		if (ShowPct != EBoolState.False)
		{
			num ^= ShowPct.GetHashCode();
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
		if (AttrType != EAttrType.EmptyValue)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteEnum((int)AttrType);
		}
		if (FuncId != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteInt32(FuncId);
		}
		if (AttrId != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)24);
			((WriteContext)(ref output)).WriteInt32(AttrId);
		}
		if (Value != 0f)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)37);
			((WriteContext)(ref output)).WriteFloat(Value);
		}
		if (Exce != EBoolState.False)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)40);
			((WriteContext)(ref output)).WriteEnum((int)Exce);
		}
		if (ShowPct != EBoolState.False)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)48);
			((WriteContext)(ref output)).WriteEnum((int)ShowPct);
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
		if (AttrType != EAttrType.EmptyValue)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)AttrType);
		}
		if (FuncId != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(FuncId);
		}
		if (AttrId != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(AttrId);
		}
		if (Value != 0f)
		{
			num += 5;
		}
		if (Exce != EBoolState.False)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)Exce);
		}
		if (ShowPct != EBoolState.False)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)ShowPct);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(EquipSubAttrDetail other)
	{
		if (other != null)
		{
			if (other.AttrType != EAttrType.EmptyValue)
			{
				AttrType = other.AttrType;
			}
			if (other.FuncId != 0)
			{
				FuncId = other.FuncId;
			}
			if (other.AttrId != 0)
			{
				AttrId = other.AttrId;
			}
			if (other.Value != 0f)
			{
				Value = other.Value;
			}
			if (other.Exce != EBoolState.False)
			{
				Exce = other.Exce;
			}
			if (other.ShowPct != EBoolState.False)
			{
				ShowPct = other.ShowPct;
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
				AttrType = (EAttrType)((ParseContext)(ref input)).ReadEnum();
				break;
			case 16u:
				FuncId = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 24u:
				AttrId = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 37u:
				Value = ((ParseContext)(ref input)).ReadFloat();
				break;
			case 40u:
				Exce = (EBoolState)((ParseContext)(ref input)).ReadEnum();
				break;
			case 48u:
				ShowPct = (EBoolState)((ParseContext)(ref input)).ReadEnum();
				break;
			}
		}
	}
}
