using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class RoleTipsEquipInfo : IMessage<RoleTipsEquipInfo>, IMessage, IEquatable<RoleTipsEquipInfo>, IDeepCloneable<RoleTipsEquipInfo>, IBufferMessage
{
	private static readonly MessageParser<RoleTipsEquipInfo> _parser = new MessageParser<RoleTipsEquipInfo>((Func<RoleTipsEquipInfo>)(() => new RoleTipsEquipInfo()));

	private UnknownFieldSet _unknownFields;

	public const int EquipSlotListFieldNumber = 1;

	private static readonly FieldCodec<EquipSlotDetail> _repeated_equipSlotList_codec = FieldCodec.ForMessage<EquipSlotDetail>(10u, EquipSlotDetail.Parser);

	private readonly RepeatedField<EquipSlotDetail> equipSlotList_ = new RepeatedField<EquipSlotDetail>();

	public const int GemSlotListFieldNumber = 2;

	private static readonly FieldCodec<GemPlanSlotDetail> _repeated_gemSlotList_codec = FieldCodec.ForMessage<GemPlanSlotDetail>(18u, GemPlanSlotDetail.Parser);

	private readonly RepeatedField<GemPlanSlotDetail> gemSlotList_ = new RepeatedField<GemPlanSlotDetail>();

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<RoleTipsEquipInfo> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeRoleTipsEquipInfoReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<EquipSlotDetail> EquipSlotList => equipSlotList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<GemPlanSlotDetail> GemSlotList => gemSlotList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RoleTipsEquipInfo()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RoleTipsEquipInfo(RoleTipsEquipInfo other)
		: this()
	{
		equipSlotList_ = other.equipSlotList_.Clone();
		gemSlotList_ = other.gemSlotList_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RoleTipsEquipInfo Clone()
	{
		return new RoleTipsEquipInfo(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as RoleTipsEquipInfo);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(RoleTipsEquipInfo other)
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
		num ^= ((object)equipSlotList_).GetHashCode();
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
		equipSlotList_.WriteTo(ref output, _repeated_equipSlotList_codec);
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
		num += equipSlotList_.CalculateSize(_repeated_equipSlotList_codec);
		num += gemSlotList_.CalculateSize(_repeated_gemSlotList_codec);
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(RoleTipsEquipInfo other)
	{
		if (other != null)
		{
			equipSlotList_.Add((IEnumerable<EquipSlotDetail>)other.equipSlotList_);
			gemSlotList_.Add((IEnumerable<GemPlanSlotDetail>)other.gemSlotList_);
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
				gemSlotList_.AddEntriesFrom(ref input, _repeated_gemSlotList_codec);
				break;
			}
		}
	}
}
