using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class OperateSubjectRouletteInfo : IMessage<OperateSubjectRouletteInfo>, IMessage, IEquatable<OperateSubjectRouletteInfo>, IDeepCloneable<OperateSubjectRouletteInfo>, IBufferMessage
{
	private static readonly MessageParser<OperateSubjectRouletteInfo> _parser = new MessageParser<OperateSubjectRouletteInfo>((Func<OperateSubjectRouletteInfo>)(() => new OperateSubjectRouletteInfo()));

	private UnknownFieldSet _unknownFields;

	public const int OnceLuckyFieldNumber = 1;

	private int onceLucky_;

	public const int MaxLuckyFieldNumber = 2;

	private int maxLucky_;

	public const int LuckyFieldNumber = 3;

	private int lucky_;

	public const int TotalCountFieldNumber = 4;

	private int totalCount_;

	public const int SlotListFieldNumber = 5;

	private static readonly FieldCodec<LimitTimeRouletteRandDetail> _repeated_slotList_codec = FieldCodec.ForMessage<LimitTimeRouletteRandDetail>(42u, LimitTimeRouletteRandDetail.Parser);

	private readonly RepeatedField<LimitTimeRouletteRandDetail> slotList_ = new RepeatedField<LimitTimeRouletteRandDetail>();

	public const int TargetListFieldNumber = 6;

	private static readonly FieldCodec<OperateTargetDetail> _repeated_targetList_codec = FieldCodec.ForMessage<OperateTargetDetail>(50u, OperateTargetDetail.Parser);

	private readonly RepeatedField<OperateTargetDetail> targetList_ = new RepeatedField<OperateTargetDetail>();

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<OperateSubjectRouletteInfo> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeOperateSubjectRouletteInfoReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int OnceLucky
	{
		get
		{
			return onceLucky_;
		}
		set
		{
			onceLucky_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int MaxLucky
	{
		get
		{
			return maxLucky_;
		}
		set
		{
			maxLucky_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int Lucky
	{
		get
		{
			return lucky_;
		}
		set
		{
			lucky_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int TotalCount
	{
		get
		{
			return totalCount_;
		}
		set
		{
			totalCount_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<LimitTimeRouletteRandDetail> SlotList => slotList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<OperateTargetDetail> TargetList => targetList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public OperateSubjectRouletteInfo()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public OperateSubjectRouletteInfo(OperateSubjectRouletteInfo other)
		: this()
	{
		onceLucky_ = other.onceLucky_;
		maxLucky_ = other.maxLucky_;
		lucky_ = other.lucky_;
		totalCount_ = other.totalCount_;
		slotList_ = other.slotList_.Clone();
		targetList_ = other.targetList_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public OperateSubjectRouletteInfo Clone()
	{
		return new OperateSubjectRouletteInfo(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as OperateSubjectRouletteInfo);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(OperateSubjectRouletteInfo other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (OnceLucky != other.OnceLucky)
		{
			return false;
		}
		if (MaxLucky != other.MaxLucky)
		{
			return false;
		}
		if (Lucky != other.Lucky)
		{
			return false;
		}
		if (TotalCount != other.TotalCount)
		{
			return false;
		}
		if (!slotList_.Equals(other.slotList_))
		{
			return false;
		}
		if (!targetList_.Equals(other.targetList_))
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
		if (OnceLucky != 0)
		{
			num ^= OnceLucky.GetHashCode();
		}
		if (MaxLucky != 0)
		{
			num ^= MaxLucky.GetHashCode();
		}
		if (Lucky != 0)
		{
			num ^= Lucky.GetHashCode();
		}
		if (TotalCount != 0)
		{
			num ^= TotalCount.GetHashCode();
		}
		num ^= ((object)slotList_).GetHashCode();
		num ^= ((object)targetList_).GetHashCode();
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
		if (OnceLucky != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteInt32(OnceLucky);
		}
		if (MaxLucky != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteInt32(MaxLucky);
		}
		if (Lucky != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)24);
			((WriteContext)(ref output)).WriteInt32(Lucky);
		}
		if (TotalCount != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)32);
			((WriteContext)(ref output)).WriteInt32(TotalCount);
		}
		slotList_.WriteTo(ref output, _repeated_slotList_codec);
		targetList_.WriteTo(ref output, _repeated_targetList_codec);
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
		if (OnceLucky != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(OnceLucky);
		}
		if (MaxLucky != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(MaxLucky);
		}
		if (Lucky != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(Lucky);
		}
		if (TotalCount != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(TotalCount);
		}
		num += slotList_.CalculateSize(_repeated_slotList_codec);
		num += targetList_.CalculateSize(_repeated_targetList_codec);
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(OperateSubjectRouletteInfo other)
	{
		if (other != null)
		{
			if (other.OnceLucky != 0)
			{
				OnceLucky = other.OnceLucky;
			}
			if (other.MaxLucky != 0)
			{
				MaxLucky = other.MaxLucky;
			}
			if (other.Lucky != 0)
			{
				Lucky = other.Lucky;
			}
			if (other.TotalCount != 0)
			{
				TotalCount = other.TotalCount;
			}
			slotList_.Add((IEnumerable<LimitTimeRouletteRandDetail>)other.slotList_);
			targetList_.Add((IEnumerable<OperateTargetDetail>)other.targetList_);
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
				OnceLucky = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 16u:
				MaxLucky = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 24u:
				Lucky = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 32u:
				TotalCount = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 42u:
				slotList_.AddEntriesFrom(ref input, _repeated_slotList_codec);
				break;
			case 50u:
				targetList_.AddEntriesFrom(ref input, _repeated_targetList_codec);
				break;
			}
		}
	}
}
