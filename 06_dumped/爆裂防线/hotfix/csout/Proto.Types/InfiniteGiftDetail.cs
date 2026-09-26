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
public sealed class InfiniteGiftDetail : IMessage<InfiniteGiftDetail>, IMessage, IEquatable<InfiniteGiftDetail>, IDeepCloneable<InfiniteGiftDetail>, IBufferMessage
{
	private static readonly MessageParser<InfiniteGiftDetail> _parser = new MessageParser<InfiniteGiftDetail>((Func<InfiniteGiftDetail>)(() => new InfiniteGiftDetail()));

	private UnknownFieldSet _unknownFields;

	public const int BaseInfoFieldNumber = 1;

	private OperateBaseInfo baseInfo_;

	public const int GoodsListFieldNumber = 2;

	private static readonly FieldCodec<InfiniteGiftGoodsDetail> _repeated_goodsList_codec = FieldCodec.ForMessage<InfiniteGiftGoodsDetail>(18u, InfiniteGiftGoodsDetail.Parser);

	private readonly RepeatedField<InfiniteGiftGoodsDetail> goodsList_ = new RepeatedField<InfiniteGiftGoodsDetail>();

	public const int TargetListFieldNumber = 3;

	private static readonly FieldCodec<OperateTargetDetail> _repeated_targetList_codec = FieldCodec.ForMessage<OperateTargetDetail>(26u, OperateTargetDetail.Parser);

	private readonly RepeatedField<OperateTargetDetail> targetList_ = new RepeatedField<OperateTargetDetail>();

	public const int BuyCountFieldNumber = 4;

	private int buyCount_;

	public const int TotalScoreFieldNumber = 5;

	private int totalScore_;

	public const int DailyFreeAwardFieldNumber = 6;

	private static readonly FieldCodec<AwardDetail> _repeated_dailyFreeAward_codec = FieldCodec.ForMessage<AwardDetail>(50u, AwardDetail.Parser);

	private readonly RepeatedField<AwardDetail> dailyFreeAward_ = new RepeatedField<AwardDetail>();

	public const int DailyFreeStateFieldNumber = 7;

	private EBoolState dailyFreeState_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<InfiniteGiftDetail> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeInfiniteGiftDetailReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public OperateBaseInfo BaseInfo
	{
		get
		{
			return baseInfo_;
		}
		set
		{
			baseInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<InfiniteGiftGoodsDetail> GoodsList => goodsList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<OperateTargetDetail> TargetList => targetList_;

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
	public int TotalScore
	{
		get
		{
			return totalScore_;
		}
		set
		{
			totalScore_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<AwardDetail> DailyFreeAward => dailyFreeAward_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public EBoolState DailyFreeState
	{
		get
		{
			return dailyFreeState_;
		}
		set
		{
			dailyFreeState_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public InfiniteGiftDetail()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public InfiniteGiftDetail(InfiniteGiftDetail other)
		: this()
	{
		baseInfo_ = ((other.baseInfo_ != null) ? other.baseInfo_.Clone() : null);
		goodsList_ = other.goodsList_.Clone();
		targetList_ = other.targetList_.Clone();
		buyCount_ = other.buyCount_;
		totalScore_ = other.totalScore_;
		dailyFreeAward_ = other.dailyFreeAward_.Clone();
		dailyFreeState_ = other.dailyFreeState_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public InfiniteGiftDetail Clone()
	{
		return new InfiniteGiftDetail(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as InfiniteGiftDetail);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(InfiniteGiftDetail other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!object.Equals(BaseInfo, other.BaseInfo))
		{
			return false;
		}
		if (!goodsList_.Equals(other.goodsList_))
		{
			return false;
		}
		if (!targetList_.Equals(other.targetList_))
		{
			return false;
		}
		if (BuyCount != other.BuyCount)
		{
			return false;
		}
		if (TotalScore != other.TotalScore)
		{
			return false;
		}
		if (!dailyFreeAward_.Equals(other.dailyFreeAward_))
		{
			return false;
		}
		if (DailyFreeState != other.DailyFreeState)
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
		if (baseInfo_ != null)
		{
			num ^= BaseInfo.GetHashCode();
		}
		num ^= ((object)goodsList_).GetHashCode();
		num ^= ((object)targetList_).GetHashCode();
		if (BuyCount != 0)
		{
			num ^= BuyCount.GetHashCode();
		}
		if (TotalScore != 0)
		{
			num ^= TotalScore.GetHashCode();
		}
		num ^= ((object)dailyFreeAward_).GetHashCode();
		if (DailyFreeState != EBoolState.False)
		{
			num ^= DailyFreeState.GetHashCode();
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
		if (baseInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)10);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)BaseInfo);
		}
		goodsList_.WriteTo(ref output, _repeated_goodsList_codec);
		targetList_.WriteTo(ref output, _repeated_targetList_codec);
		if (BuyCount != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)32);
			((WriteContext)(ref output)).WriteInt32(BuyCount);
		}
		if (TotalScore != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)40);
			((WriteContext)(ref output)).WriteInt32(TotalScore);
		}
		dailyFreeAward_.WriteTo(ref output, _repeated_dailyFreeAward_codec);
		if (DailyFreeState != EBoolState.False)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)56);
			((WriteContext)(ref output)).WriteEnum((int)DailyFreeState);
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
		if (baseInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)BaseInfo);
		}
		num += goodsList_.CalculateSize(_repeated_goodsList_codec);
		num += targetList_.CalculateSize(_repeated_targetList_codec);
		if (BuyCount != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(BuyCount);
		}
		if (TotalScore != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(TotalScore);
		}
		num += dailyFreeAward_.CalculateSize(_repeated_dailyFreeAward_codec);
		if (DailyFreeState != EBoolState.False)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)DailyFreeState);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(InfiniteGiftDetail other)
	{
		if (other == null)
		{
			return;
		}
		if (other.baseInfo_ != null)
		{
			if (baseInfo_ == null)
			{
				BaseInfo = new OperateBaseInfo();
			}
			BaseInfo.MergeFrom(other.BaseInfo);
		}
		goodsList_.Add((IEnumerable<InfiniteGiftGoodsDetail>)other.goodsList_);
		targetList_.Add((IEnumerable<OperateTargetDetail>)other.targetList_);
		if (other.BuyCount != 0)
		{
			BuyCount = other.BuyCount;
		}
		if (other.TotalScore != 0)
		{
			TotalScore = other.TotalScore;
		}
		dailyFreeAward_.Add((IEnumerable<AwardDetail>)other.dailyFreeAward_);
		if (other.DailyFreeState != EBoolState.False)
		{
			DailyFreeState = other.DailyFreeState;
		}
		_unknownFields = UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
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
				if (baseInfo_ == null)
				{
					BaseInfo = new OperateBaseInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)BaseInfo);
				break;
			case 18u:
				goodsList_.AddEntriesFrom(ref input, _repeated_goodsList_codec);
				break;
			case 26u:
				targetList_.AddEntriesFrom(ref input, _repeated_targetList_codec);
				break;
			case 32u:
				BuyCount = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 40u:
				TotalScore = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 50u:
				dailyFreeAward_.AddEntriesFrom(ref input, _repeated_dailyFreeAward_codec);
				break;
			case 56u:
				DailyFreeState = (EBoolState)((ParseContext)(ref input)).ReadEnum();
				break;
			}
		}
	}
}
