using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class ArenaInfo : IMessage<ArenaInfo>, IMessage, IEquatable<ArenaInfo>, IDeepCloneable<ArenaInfo>, IBufferMessage
{
	private static readonly MessageParser<ArenaInfo> _parser = new MessageParser<ArenaInfo>((Func<ArenaInfo>)(() => new ArenaInfo()));

	private UnknownFieldSet _unknownFields;

	public const int RankFieldNumber = 1;

	private int rank_;

	public const int BestRankFieldNumber = 2;

	private int bestRank_;

	public const int CombatCountFieldNumber = 3;

	private int combatCount_;

	public const int DailyCountFieldNumber = 4;

	private int dailyCount_;

	public const int BuyCountFieldNumber = 5;

	private int buyCount_;

	public const int GiftListFieldNumber = 6;

	private static readonly FieldCodec<ArenaGiftDetail> _repeated_giftList_codec = FieldCodec.ForMessage<ArenaGiftDetail>(50u, ArenaGiftDetail.Parser);

	private readonly RepeatedField<ArenaGiftDetail> giftList_ = new RepeatedField<ArenaGiftDetail>();

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<ArenaInfo> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeArenaInfoReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int Rank
	{
		get
		{
			return rank_;
		}
		set
		{
			rank_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int BestRank
	{
		get
		{
			return bestRank_;
		}
		set
		{
			bestRank_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int CombatCount
	{
		get
		{
			return combatCount_;
		}
		set
		{
			combatCount_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int DailyCount
	{
		get
		{
			return dailyCount_;
		}
		set
		{
			dailyCount_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int BuyCount
	{
		get
		{
			return buyCount_;
		}
		set
		{
			buyCount_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<ArenaGiftDetail> GiftList => giftList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ArenaInfo()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ArenaInfo(ArenaInfo other)
		: this()
	{
		rank_ = other.rank_;
		bestRank_ = other.bestRank_;
		combatCount_ = other.combatCount_;
		dailyCount_ = other.dailyCount_;
		buyCount_ = other.buyCount_;
		giftList_ = other.giftList_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ArenaInfo Clone()
	{
		return new ArenaInfo(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as ArenaInfo);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(ArenaInfo other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (Rank != other.Rank)
		{
			return false;
		}
		if (BestRank != other.BestRank)
		{
			return false;
		}
		if (CombatCount != other.CombatCount)
		{
			return false;
		}
		if (DailyCount != other.DailyCount)
		{
			return false;
		}
		if (BuyCount != other.BuyCount)
		{
			return false;
		}
		if (!giftList_.Equals(other.giftList_))
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
		if (Rank != 0)
		{
			num ^= Rank.GetHashCode();
		}
		if (BestRank != 0)
		{
			num ^= BestRank.GetHashCode();
		}
		if (CombatCount != 0)
		{
			num ^= CombatCount.GetHashCode();
		}
		if (DailyCount != 0)
		{
			num ^= DailyCount.GetHashCode();
		}
		if (BuyCount != 0)
		{
			num ^= BuyCount.GetHashCode();
		}
		num ^= ((object)giftList_).GetHashCode();
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
		if (Rank != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteInt32(Rank);
		}
		if (BestRank != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteInt32(BestRank);
		}
		if (CombatCount != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)24);
			((WriteContext)(ref output)).WriteInt32(CombatCount);
		}
		if (DailyCount != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)32);
			((WriteContext)(ref output)).WriteInt32(DailyCount);
		}
		if (BuyCount != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)40);
			((WriteContext)(ref output)).WriteInt32(BuyCount);
		}
		giftList_.WriteTo(ref output, _repeated_giftList_codec);
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
		if (Rank != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(Rank);
		}
		if (BestRank != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(BestRank);
		}
		if (CombatCount != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(CombatCount);
		}
		if (DailyCount != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(DailyCount);
		}
		if (BuyCount != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(BuyCount);
		}
		num += giftList_.CalculateSize(_repeated_giftList_codec);
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(ArenaInfo other)
	{
		if (other != null)
		{
			if (other.Rank != 0)
			{
				Rank = other.Rank;
			}
			if (other.BestRank != 0)
			{
				BestRank = other.BestRank;
			}
			if (other.CombatCount != 0)
			{
				CombatCount = other.CombatCount;
			}
			if (other.DailyCount != 0)
			{
				DailyCount = other.DailyCount;
			}
			if (other.BuyCount != 0)
			{
				BuyCount = other.BuyCount;
			}
			giftList_.Add((IEnumerable<ArenaGiftDetail>)other.giftList_);
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
				Rank = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 16u:
				BestRank = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 24u:
				CombatCount = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 32u:
				DailyCount = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 40u:
				BuyCount = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 50u:
				giftList_.AddEntriesFrom(ref input, _repeated_giftList_codec);
				break;
			}
		}
	}
}
