using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class EquipSlotInfo : IMessage<EquipSlotInfo>, IMessage, IEquatable<EquipSlotInfo>, IDeepCloneable<EquipSlotInfo>, IBufferMessage
{
	private static readonly MessageParser<EquipSlotInfo> _parser = new MessageParser<EquipSlotInfo>((Func<EquipSlotInfo>)(() => new EquipSlotInfo()));

	private UnknownFieldSet _unknownFields;

	public const int EquipSlotListFieldNumber = 1;

	private static readonly FieldCodec<EquipSlotDetail> _repeated_equipSlotList_codec = FieldCodec.ForMessage<EquipSlotDetail>(10u, EquipSlotDetail.Parser);

	private readonly RepeatedField<EquipSlotDetail> equipSlotList_ = new RepeatedField<EquipSlotDetail>();

	public const int GemPlanListFieldNumber = 2;

	private static readonly FieldCodec<GemPlanDetail> _repeated_gemPlanList_codec = FieldCodec.ForMessage<GemPlanDetail>(18u, GemPlanDetail.Parser);

	private readonly RepeatedField<GemPlanDetail> gemPlanList_ = new RepeatedField<GemPlanDetail>();

	public const int GemPlanIdFieldNumber = 3;

	private int gemPlanId_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<EquipSlotInfo> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeEquipSlotInfoReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<EquipSlotDetail> EquipSlotList => equipSlotList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<GemPlanDetail> GemPlanList => gemPlanList_;

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
	public EquipSlotInfo()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public EquipSlotInfo(EquipSlotInfo other)
		: this()
	{
		equipSlotList_ = other.equipSlotList_.Clone();
		gemPlanList_ = other.gemPlanList_.Clone();
		gemPlanId_ = other.gemPlanId_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public EquipSlotInfo Clone()
	{
		return new EquipSlotInfo(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as EquipSlotInfo);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(EquipSlotInfo other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!equipSlotList_.Equals(other.equipSlotList_))
		{
			return false;
		}
		if (!gemPlanList_.Equals(other.gemPlanList_))
		{
			return false;
		}
		if (GemPlanId != other.GemPlanId)
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
		num ^= ((object)equipSlotList_).GetHashCode();
		num ^= ((object)gemPlanList_).GetHashCode();
		if (GemPlanId != 0)
		{
			num ^= GemPlanId.GetHashCode();
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
		equipSlotList_.WriteTo(ref output, _repeated_equipSlotList_codec);
		gemPlanList_.WriteTo(ref output, _repeated_gemPlanList_codec);
		if (GemPlanId != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)24);
			((WriteContext)(ref output)).WriteInt32(GemPlanId);
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
		num += equipSlotList_.CalculateSize(_repeated_equipSlotList_codec);
		num += gemPlanList_.CalculateSize(_repeated_gemPlanList_codec);
		if (GemPlanId != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(GemPlanId);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(EquipSlotInfo other)
	{
		if (other != null)
		{
			equipSlotList_.Add((IEnumerable<EquipSlotDetail>)other.equipSlotList_);
			gemPlanList_.Add((IEnumerable<GemPlanDetail>)other.gemPlanList_);
			if (other.GemPlanId != 0)
			{
				GemPlanId = other.GemPlanId;
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
			case 10u:
				equipSlotList_.AddEntriesFrom(ref input, _repeated_equipSlotList_codec);
				break;
			case 18u:
				gemPlanList_.AddEntriesFrom(ref input, _repeated_gemPlanList_codec);
				break;
			case 24u:
				GemPlanId = ((ParseContext)(ref input)).ReadInt32();
				break;
			}
		}
	}
}
