using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class BossSubjectRouletteInfo : IMessage<BossSubjectRouletteInfo>, IMessage, IEquatable<BossSubjectRouletteInfo>, IDeepCloneable<BossSubjectRouletteInfo>, IBufferMessage
{
	private static readonly MessageParser<BossSubjectRouletteInfo> _parser = new MessageParser<BossSubjectRouletteInfo>((Func<BossSubjectRouletteInfo>)(() => new BossSubjectRouletteInfo()));

	private UnknownFieldSet _unknownFields;

	public const int CountFieldNumber = 1;

	private int count_;

	public const int SlotListFieldNumber = 2;

	private static readonly FieldCodec<LimitTimeRouletteRandDetail> _repeated_slotList_codec = FieldCodec.ForMessage<LimitTimeRouletteRandDetail>(18u, LimitTimeRouletteRandDetail.Parser);

	private readonly RepeatedField<LimitTimeRouletteRandDetail> slotList_ = new RepeatedField<LimitTimeRouletteRandDetail>();

	public const int TargetListFieldNumber = 3;

	private static readonly FieldCodec<OperateTargetDetail> _repeated_targetList_codec = FieldCodec.ForMessage<OperateTargetDetail>(26u, OperateTargetDetail.Parser);

	private readonly RepeatedField<OperateTargetDetail> targetList_ = new RepeatedField<OperateTargetDetail>();

	public const int OnceLuckyFieldNumber = 4;

	private int onceLucky_;

	public const int MaxLuckyFieldNumber = 5;

	private int maxLucky_;

	public const int LuckyFieldNumber = 6;

	private int lucky_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<BossSubjectRouletteInfo> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeBossSubjectRouletteInfoReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int Count
	{
		get
		{
			return count_;
		}
		set
		{
			count_ = value;
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
	public BossSubjectRouletteInfo()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public BossSubjectRouletteInfo(BossSubjectRouletteInfo other)
		: this()
	{
		count_ = other.count_;
		slotList_ = other.slotList_.Clone();
		targetList_ = other.targetList_.Clone();
		onceLucky_ = other.onceLucky_;
		maxLucky_ = other.maxLucky_;
		lucky_ = other.lucky_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public BossSubjectRouletteInfo Clone()
	{
		return new BossSubjectRouletteInfo(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as BossSubjectRouletteInfo);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(BossSubjectRouletteInfo other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (Count != other.Count)
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
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override int GetHashCode()
	{
		int num = 1;
		if (Count != 0)
		{
			num ^= Count.GetHashCode();
		}
		num ^= ((object)slotList_).GetHashCode();
		num ^= ((object)targetList_).GetHashCode();
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
		if (Count != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteInt32(Count);
		}
		slotList_.WriteTo(ref output, _repeated_slotList_codec);
		targetList_.WriteTo(ref output, _repeated_targetList_codec);
		if (OnceLucky != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)32);
			((WriteContext)(ref output)).WriteInt32(OnceLucky);
		}
		if (MaxLucky != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)40);
			((WriteContext)(ref output)).WriteInt32(MaxLucky);
		}
		if (Lucky != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)48);
			((WriteContext)(ref output)).WriteInt32(Lucky);
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
		if (Count != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(Count);
		}
		num += slotList_.CalculateSize(_repeated_slotList_codec);
		num += targetList_.CalculateSize(_repeated_targetList_codec);
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
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(BossSubjectRouletteInfo other)
	{
		if (other != null)
		{
			if (other.Count != 0)
			{
				Count = other.Count;
			}
			slotList_.Add((IEnumerable<LimitTimeRouletteRandDetail>)other.slotList_);
			targetList_.Add((IEnumerable<OperateTargetDetail>)other.targetList_);
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
				Count = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 18u:
				slotList_.AddEntriesFrom(ref input, _repeated_slotList_codec);
				break;
			case 26u:
				targetList_.AddEntriesFrom(ref input, _repeated_targetList_codec);
				break;
			case 32u:
				OnceLucky = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 40u:
				MaxLucky = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 48u:
				Lucky = ((ParseContext)(ref input)).ReadInt32();
				break;
			}
		}
	}
}
