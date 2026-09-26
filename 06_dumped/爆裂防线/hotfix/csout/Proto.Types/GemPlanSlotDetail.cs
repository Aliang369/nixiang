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
public sealed class GemPlanSlotDetail : IMessage<GemPlanSlotDetail>, IMessage, IEquatable<GemPlanSlotDetail>, IDeepCloneable<GemPlanSlotDetail>, IBufferMessage
{
	private static readonly MessageParser<GemPlanSlotDetail> _parser = new MessageParser<GemPlanSlotDetail>((Func<GemPlanSlotDetail>)(() => new GemPlanSlotDetail()));

	private UnknownFieldSet _unknownFields;

	public const int EquipTypeFieldNumber = 1;

	private EEquipType equipType_;

	public const int GemSlotListFieldNumber = 2;

	private static readonly FieldCodec<GemSlotDetail> _repeated_gemSlotList_codec = FieldCodec.ForMessage<GemSlotDetail>(18u, GemSlotDetail.Parser);

	private readonly RepeatedField<GemSlotDetail> gemSlotList_ = new RepeatedField<GemSlotDetail>();

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<GemPlanSlotDetail> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeGemPlanSlotDetailReflection.Descriptor.MessageTypes[0];

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
	public RepeatedField<GemSlotDetail> GemSlotList => gemSlotList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public GemPlanSlotDetail()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public GemPlanSlotDetail(GemPlanSlotDetail other)
		: this()
	{
		equipType_ = other.equipType_;
		gemSlotList_ = other.gemSlotList_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public GemPlanSlotDetail Clone()
	{
		return new GemPlanSlotDetail(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as GemPlanSlotDetail);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(GemPlanSlotDetail other)
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
		if (!gemSlotList_.Equals(other.gemSlotList_))
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
		num ^= ((object)gemSlotList_).GetHashCode();
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
		gemSlotList_.WriteTo(ref output, _repeated_gemSlotList_codec);
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
		num += gemSlotList_.CalculateSize(_repeated_gemSlotList_codec);
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(GemPlanSlotDetail other)
	{
		if (other != null)
		{
			if (other.EquipType != EEquipType.EmptyValue)
			{
				EquipType = other.EquipType;
			}
			gemSlotList_.Add((IEnumerable<GemSlotDetail>)other.gemSlotList_);
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
				gemSlotList_.AddEntriesFrom(ref input, _repeated_gemSlotList_codec);
				break;
			}
		}
	}
}
