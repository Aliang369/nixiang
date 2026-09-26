using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class BattleMonsterRoundDetail : IMessage<BattleMonsterRoundDetail>, IMessage, IEquatable<BattleMonsterRoundDetail>, IDeepCloneable<BattleMonsterRoundDetail>, IBufferMessage
{
	private static readonly MessageParser<BattleMonsterRoundDetail> _parser = new MessageParser<BattleMonsterRoundDetail>((Func<BattleMonsterRoundDetail>)(() => new BattleMonsterRoundDetail()));

	private UnknownFieldSet _unknownFields;

	public const int RoundFieldNumber = 1;

	private int round_;

	public const int BornStartFieldNumber = 2;

	private int bornStart_;

	public const int BornEndFieldNumber = 3;

	private int bornEnd_;

	public const int BornCountFieldNumber = 4;

	private int bornCount_;

	public const int BornListFieldNumber = 5;

	private static readonly FieldCodec<BattleBornMonsterDetail> _repeated_bornList_codec = FieldCodec.ForMessage<BattleBornMonsterDetail>(42u, BattleBornMonsterDetail.Parser);

	private readonly RepeatedField<BattleBornMonsterDetail> bornList_ = new RepeatedField<BattleBornMonsterDetail>();

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<BattleMonsterRoundDetail> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeBattleMonsterRoundDetailReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int Round
	{
		get
		{
			return round_;
		}
		set
		{
			round_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int BornStart
	{
		get
		{
			return bornStart_;
		}
		set
		{
			bornStart_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int BornEnd
	{
		get
		{
			return bornEnd_;
		}
		set
		{
			bornEnd_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int BornCount
	{
		get
		{
			return bornCount_;
		}
		set
		{
			bornCount_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<BattleBornMonsterDetail> BornList => bornList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public BattleMonsterRoundDetail()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public BattleMonsterRoundDetail(BattleMonsterRoundDetail other)
		: this()
	{
		round_ = other.round_;
		bornStart_ = other.bornStart_;
		bornEnd_ = other.bornEnd_;
		bornCount_ = other.bornCount_;
		bornList_ = other.bornList_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public BattleMonsterRoundDetail Clone()
	{
		return new BattleMonsterRoundDetail(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as BattleMonsterRoundDetail);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(BattleMonsterRoundDetail other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (Round != other.Round)
		{
			return false;
		}
		if (BornStart != other.BornStart)
		{
			return false;
		}
		if (BornEnd != other.BornEnd)
		{
			return false;
		}
		if (BornCount != other.BornCount)
		{
			return false;
		}
		if (!bornList_.Equals(other.bornList_))
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
		if (Round != 0)
		{
			num ^= Round.GetHashCode();
		}
		if (BornStart != 0)
		{
			num ^= BornStart.GetHashCode();
		}
		if (BornEnd != 0)
		{
			num ^= BornEnd.GetHashCode();
		}
		if (BornCount != 0)
		{
			num ^= BornCount.GetHashCode();
		}
		num ^= ((object)bornList_).GetHashCode();
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
		if (Round != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteInt32(Round);
		}
		if (BornStart != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteInt32(BornStart);
		}
		if (BornEnd != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)24);
			((WriteContext)(ref output)).WriteInt32(BornEnd);
		}
		if (BornCount != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)32);
			((WriteContext)(ref output)).WriteInt32(BornCount);
		}
		bornList_.WriteTo(ref output, _repeated_bornList_codec);
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
		if (Round != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(Round);
		}
		if (BornStart != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(BornStart);
		}
		if (BornEnd != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(BornEnd);
		}
		if (BornCount != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(BornCount);
		}
		num += bornList_.CalculateSize(_repeated_bornList_codec);
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(BattleMonsterRoundDetail other)
	{
		if (other != null)
		{
			if (other.Round != 0)
			{
				Round = other.Round;
			}
			if (other.BornStart != 0)
			{
				BornStart = other.BornStart;
			}
			if (other.BornEnd != 0)
			{
				BornEnd = other.BornEnd;
			}
			if (other.BornCount != 0)
			{
				BornCount = other.BornCount;
			}
			bornList_.Add((IEnumerable<BattleBornMonsterDetail>)other.bornList_);
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
				Round = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 16u:
				BornStart = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 24u:
				BornEnd = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 32u:
				BornCount = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 42u:
				bornList_.AddEntriesFrom(ref input, _repeated_bornList_codec);
				break;
			}
		}
	}
}
