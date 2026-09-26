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
public sealed class RestaurantDetail : IMessage<RestaurantDetail>, IMessage, IEquatable<RestaurantDetail>, IDeepCloneable<RestaurantDetail>, IBufferMessage
{
	private static readonly MessageParser<RestaurantDetail> _parser = new MessageParser<RestaurantDetail>((Func<RestaurantDetail>)(() => new RestaurantDetail()));

	private UnknownFieldSet _unknownFields;

	public const int BaseInfoFieldNumber = 1;

	private OperateBaseInfo baseInfo_;

	public const int BuyCountFieldNumber = 2;

	private int buyCount_;

	public const int ActivateAwardFieldNumber = 3;

	private EBoolState activateAward_;

	public const int DailyAwardFieldNumber = 4;

	private OperateRestaurantDailyAwardDetail dailyAward_;

	public const int SingleCostFieldNumber = 5;

	private static readonly FieldCodec<AwardDetail> _repeated_singleCost_codec = FieldCodec.ForMessage<AwardDetail>(42u, AwardDetail.Parser);

	private readonly RepeatedField<AwardDetail> singleCost_ = new RepeatedField<AwardDetail>();

	public const int PriceOffFieldNumber = 6;

	private int priceOff_;

	public const int SlotListFieldNumber = 7;

	private static readonly FieldCodec<int> _repeated_slotList_codec = FieldCodec.ForInt32(58u);

	private readonly RepeatedField<int> slotList_ = new RepeatedField<int>();

	public const int SlotSyncTmFieldNumber = 8;

	private long slotSyncTm_;

	public const int SlotCountMaxFieldNumber = 9;

	private int slotCountMax_;

	public const int TargetDetailFieldNumber = 10;

	private OperateRestaurantTargetDetail targetDetail_;

	public const int TargetNextGrandFieldNumber = 11;

	private OperateTargetNextGrandDetail targetNextGrand_;

	public const int OfferListFieldNumber = 12;

	private static readonly FieldCodec<int> _repeated_offerList_codec = FieldCodec.ForInt32(98u);

	private readonly RepeatedField<int> offerList_ = new RepeatedField<int>();

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<RestaurantDetail> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeRestaurantDetailReflection.Descriptor.MessageTypes[0];

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
	public EBoolState ActivateAward
	{
		get
		{
			return activateAward_;
		}
		set
		{
			activateAward_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public OperateRestaurantDailyAwardDetail DailyAward
	{
		get
		{
			return dailyAward_;
		}
		set
		{
			dailyAward_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<AwardDetail> SingleCost => singleCost_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int PriceOff
	{
		get
		{
			return priceOff_;
		}
		set
		{
			priceOff_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<int> SlotList => slotList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public long SlotSyncTm
	{
		get
		{
			return slotSyncTm_;
		}
		set
		{
			slotSyncTm_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int SlotCountMax
	{
		get
		{
			return slotCountMax_;
		}
		set
		{
			slotCountMax_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public OperateRestaurantTargetDetail TargetDetail
	{
		get
		{
			return targetDetail_;
		}
		set
		{
			targetDetail_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public OperateTargetNextGrandDetail TargetNextGrand
	{
		get
		{
			return targetNextGrand_;
		}
		set
		{
			targetNextGrand_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<int> OfferList => offerList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RestaurantDetail()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RestaurantDetail(RestaurantDetail other)
		: this()
	{
		baseInfo_ = ((other.baseInfo_ != null) ? other.baseInfo_.Clone() : null);
		buyCount_ = other.buyCount_;
		activateAward_ = other.activateAward_;
		dailyAward_ = ((other.dailyAward_ != null) ? other.dailyAward_.Clone() : null);
		singleCost_ = other.singleCost_.Clone();
		priceOff_ = other.priceOff_;
		slotList_ = other.slotList_.Clone();
		slotSyncTm_ = other.slotSyncTm_;
		slotCountMax_ = other.slotCountMax_;
		targetDetail_ = ((other.targetDetail_ != null) ? other.targetDetail_.Clone() : null);
		targetNextGrand_ = ((other.targetNextGrand_ != null) ? other.targetNextGrand_.Clone() : null);
		offerList_ = other.offerList_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RestaurantDetail Clone()
	{
		return new RestaurantDetail(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as RestaurantDetail);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(RestaurantDetail other)
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
		if (BuyCount != other.BuyCount)
		{
			return false;
		}
		if (ActivateAward != other.ActivateAward)
		{
			return false;
		}
		if (!object.Equals(DailyAward, other.DailyAward))
		{
			return false;
		}
		if (!singleCost_.Equals(other.singleCost_))
		{
			return false;
		}
		if (PriceOff != other.PriceOff)
		{
			return false;
		}
		if (!slotList_.Equals(other.slotList_))
		{
			return false;
		}
		if (SlotSyncTm != other.SlotSyncTm)
		{
			return false;
		}
		if (SlotCountMax != other.SlotCountMax)
		{
			return false;
		}
		if (!object.Equals(TargetDetail, other.TargetDetail))
		{
			return false;
		}
		if (!object.Equals(TargetNextGrand, other.TargetNextGrand))
		{
			return false;
		}
		if (!offerList_.Equals(other.offerList_))
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
		if (BuyCount != 0)
		{
			num ^= BuyCount.GetHashCode();
		}
		if (ActivateAward != EBoolState.False)
		{
			num ^= ActivateAward.GetHashCode();
		}
		if (dailyAward_ != null)
		{
			num ^= DailyAward.GetHashCode();
		}
		num ^= ((object)singleCost_).GetHashCode();
		if (PriceOff != 0)
		{
			num ^= PriceOff.GetHashCode();
		}
		num ^= ((object)slotList_).GetHashCode();
		if (SlotSyncTm != 0L)
		{
			num ^= SlotSyncTm.GetHashCode();
		}
		if (SlotCountMax != 0)
		{
			num ^= SlotCountMax.GetHashCode();
		}
		if (targetDetail_ != null)
		{
			num ^= TargetDetail.GetHashCode();
		}
		if (targetNextGrand_ != null)
		{
			num ^= TargetNextGrand.GetHashCode();
		}
		num ^= ((object)offerList_).GetHashCode();
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
		if (BuyCount != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteInt32(BuyCount);
		}
		if (ActivateAward != EBoolState.False)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)24);
			((WriteContext)(ref output)).WriteEnum((int)ActivateAward);
		}
		if (dailyAward_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)34);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)DailyAward);
		}
		singleCost_.WriteTo(ref output, _repeated_singleCost_codec);
		if (PriceOff != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)48);
			((WriteContext)(ref output)).WriteInt32(PriceOff);
		}
		slotList_.WriteTo(ref output, _repeated_slotList_codec);
		if (SlotSyncTm != 0L)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)64);
			((WriteContext)(ref output)).WriteInt64(SlotSyncTm);
		}
		if (SlotCountMax != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)72);
			((WriteContext)(ref output)).WriteInt32(SlotCountMax);
		}
		if (targetDetail_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)82);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)TargetDetail);
		}
		if (targetNextGrand_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)90);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)TargetNextGrand);
		}
		offerList_.WriteTo(ref output, _repeated_offerList_codec);
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
		if (BuyCount != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(BuyCount);
		}
		if (ActivateAward != EBoolState.False)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)ActivateAward);
		}
		if (dailyAward_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)DailyAward);
		}
		num += singleCost_.CalculateSize(_repeated_singleCost_codec);
		if (PriceOff != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(PriceOff);
		}
		num += slotList_.CalculateSize(_repeated_slotList_codec);
		if (SlotSyncTm != 0L)
		{
			num += 1 + CodedOutputStream.ComputeInt64Size(SlotSyncTm);
		}
		if (SlotCountMax != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(SlotCountMax);
		}
		if (targetDetail_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)TargetDetail);
		}
		if (targetNextGrand_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)TargetNextGrand);
		}
		num += offerList_.CalculateSize(_repeated_offerList_codec);
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(RestaurantDetail other)
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
		if (other.BuyCount != 0)
		{
			BuyCount = other.BuyCount;
		}
		if (other.ActivateAward != EBoolState.False)
		{
			ActivateAward = other.ActivateAward;
		}
		if (other.dailyAward_ != null)
		{
			if (dailyAward_ == null)
			{
				DailyAward = new OperateRestaurantDailyAwardDetail();
			}
			DailyAward.MergeFrom(other.DailyAward);
		}
		singleCost_.Add((IEnumerable<AwardDetail>)other.singleCost_);
		if (other.PriceOff != 0)
		{
			PriceOff = other.PriceOff;
		}
		slotList_.Add((IEnumerable<int>)other.slotList_);
		if (other.SlotSyncTm != 0L)
		{
			SlotSyncTm = other.SlotSyncTm;
		}
		if (other.SlotCountMax != 0)
		{
			SlotCountMax = other.SlotCountMax;
		}
		if (other.targetDetail_ != null)
		{
			if (targetDetail_ == null)
			{
				TargetDetail = new OperateRestaurantTargetDetail();
			}
			TargetDetail.MergeFrom(other.TargetDetail);
		}
		if (other.targetNextGrand_ != null)
		{
			if (targetNextGrand_ == null)
			{
				TargetNextGrand = new OperateTargetNextGrandDetail();
			}
			TargetNextGrand.MergeFrom(other.TargetNextGrand);
		}
		offerList_.Add((IEnumerable<int>)other.offerList_);
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
			case 16u:
				BuyCount = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 24u:
				ActivateAward = (EBoolState)((ParseContext)(ref input)).ReadEnum();
				break;
			case 34u:
				if (dailyAward_ == null)
				{
					DailyAward = new OperateRestaurantDailyAwardDetail();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)DailyAward);
				break;
			case 42u:
				singleCost_.AddEntriesFrom(ref input, _repeated_singleCost_codec);
				break;
			case 48u:
				PriceOff = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 56u:
			case 58u:
				slotList_.AddEntriesFrom(ref input, _repeated_slotList_codec);
				break;
			case 64u:
				SlotSyncTm = ((ParseContext)(ref input)).ReadInt64();
				break;
			case 72u:
				SlotCountMax = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 82u:
				if (targetDetail_ == null)
				{
					TargetDetail = new OperateRestaurantTargetDetail();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)TargetDetail);
				break;
			case 90u:
				if (targetNextGrand_ == null)
				{
					TargetNextGrand = new OperateTargetNextGrandDetail();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)TargetNextGrand);
				break;
			case 96u:
			case 98u:
				offerList_.AddEntriesFrom(ref input, _repeated_offerList_codec);
				break;
			}
		}
	}
}
