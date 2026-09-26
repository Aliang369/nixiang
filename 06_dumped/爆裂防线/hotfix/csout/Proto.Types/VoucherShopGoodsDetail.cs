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
public sealed class VoucherShopGoodsDetail : IMessage<VoucherShopGoodsDetail>, IMessage, IEquatable<VoucherShopGoodsDetail>, IDeepCloneable<VoucherShopGoodsDetail>, IBufferMessage
{
	private static readonly MessageParser<VoucherShopGoodsDetail> _parser = new MessageParser<VoucherShopGoodsDetail>((Func<VoucherShopGoodsDetail>)(() => new VoucherShopGoodsDetail()));

	private UnknownFieldSet _unknownFields;

	public const int GoodsIdFieldNumber = 1;

	private int goodsId_;

	public const int GoodsTypeFieldNumber = 2;

	private EVoucherGoodsType goodsType_;

	public const int GoodsQualityFieldNumber = 3;

	private EQuality goodsQuality_;

	public const int SortIndexFieldNumber = 4;

	private int sortIndex_;

	public const int RefreshTypeFieldNumber = 5;

	private ERefreshType refreshType_;

	public const int ExpireTmFieldNumber = 6;

	private long expireTm_;

	public const int LimitNumFieldNumber = 7;

	private int limitNum_;

	public const int BoughtCountFieldNumber = 8;

	private int boughtCount_;

	public const int ShowPriceFieldNumber = 9;

	private int showPrice_;

	public const int SalePriceFieldNumber = 10;

	private int salePrice_;

	public const int DiscountFieldNumber = 11;

	private int discount_;

	public const int AwardListFieldNumber = 12;

	private static readonly FieldCodec<AwardDetail> _repeated_awardList_codec = FieldCodec.ForMessage<AwardDetail>(98u, AwardDetail.Parser);

	private readonly RepeatedField<AwardDetail> awardList_ = new RepeatedField<AwardDetail>();

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<VoucherShopGoodsDetail> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeVoucherShopGoodsDetailReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int GoodsId
	{
		get
		{
			return goodsId_;
		}
		set
		{
			goodsId_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public EVoucherGoodsType GoodsType
	{
		get
		{
			return goodsType_;
		}
		set
		{
			goodsType_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public EQuality GoodsQuality
	{
		get
		{
			return goodsQuality_;
		}
		set
		{
			goodsQuality_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int SortIndex
	{
		get
		{
			return sortIndex_;
		}
		set
		{
			sortIndex_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ERefreshType RefreshType
	{
		get
		{
			return refreshType_;
		}
		set
		{
			refreshType_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public long ExpireTm
	{
		get
		{
			return expireTm_;
		}
		set
		{
			expireTm_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int LimitNum
	{
		get
		{
			return limitNum_;
		}
		set
		{
			limitNum_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int BoughtCount
	{
		get
		{
			return boughtCount_;
		}
		set
		{
			boughtCount_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int ShowPrice
	{
		get
		{
			return showPrice_;
		}
		set
		{
			showPrice_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int SalePrice
	{
		get
		{
			return salePrice_;
		}
		set
		{
			salePrice_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int Discount
	{
		get
		{
			return discount_;
		}
		set
		{
			discount_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<AwardDetail> AwardList => awardList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public VoucherShopGoodsDetail()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public VoucherShopGoodsDetail(VoucherShopGoodsDetail other)
		: this()
	{
		goodsId_ = other.goodsId_;
		goodsType_ = other.goodsType_;
		goodsQuality_ = other.goodsQuality_;
		sortIndex_ = other.sortIndex_;
		refreshType_ = other.refreshType_;
		expireTm_ = other.expireTm_;
		limitNum_ = other.limitNum_;
		boughtCount_ = other.boughtCount_;
		showPrice_ = other.showPrice_;
		salePrice_ = other.salePrice_;
		discount_ = other.discount_;
		awardList_ = other.awardList_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public VoucherShopGoodsDetail Clone()
	{
		return new VoucherShopGoodsDetail(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as VoucherShopGoodsDetail);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(VoucherShopGoodsDetail other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (GoodsId != other.GoodsId)
		{
			return false;
		}
		if (GoodsType != other.GoodsType)
		{
			return false;
		}
		if (GoodsQuality != other.GoodsQuality)
		{
			return false;
		}
		if (SortIndex != other.SortIndex)
		{
			return false;
		}
		if (RefreshType != other.RefreshType)
		{
			return false;
		}
		if (ExpireTm != other.ExpireTm)
		{
			return false;
		}
		if (LimitNum != other.LimitNum)
		{
			return false;
		}
		if (BoughtCount != other.BoughtCount)
		{
			return false;
		}
		if (ShowPrice != other.ShowPrice)
		{
			return false;
		}
		if (SalePrice != other.SalePrice)
		{
			return false;
		}
		if (Discount != other.Discount)
		{
			return false;
		}
		if (!awardList_.Equals(other.awardList_))
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
		if (GoodsId != 0)
		{
			num ^= GoodsId.GetHashCode();
		}
		if (GoodsType != EVoucherGoodsType.EmptyValue)
		{
			num ^= GoodsType.GetHashCode();
		}
		if (GoodsQuality != EQuality.EmptyValue)
		{
			num ^= GoodsQuality.GetHashCode();
		}
		if (SortIndex != 0)
		{
			num ^= SortIndex.GetHashCode();
		}
		if (RefreshType != ERefreshType.EmptyValue)
		{
			num ^= RefreshType.GetHashCode();
		}
		if (ExpireTm != 0L)
		{
			num ^= ExpireTm.GetHashCode();
		}
		if (LimitNum != 0)
		{
			num ^= LimitNum.GetHashCode();
		}
		if (BoughtCount != 0)
		{
			num ^= BoughtCount.GetHashCode();
		}
		if (ShowPrice != 0)
		{
			num ^= ShowPrice.GetHashCode();
		}
		if (SalePrice != 0)
		{
			num ^= SalePrice.GetHashCode();
		}
		if (Discount != 0)
		{
			num ^= Discount.GetHashCode();
		}
		num ^= ((object)awardList_).GetHashCode();
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
		if (GoodsId != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteInt32(GoodsId);
		}
		if (GoodsType != EVoucherGoodsType.EmptyValue)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteEnum((int)GoodsType);
		}
		if (GoodsQuality != EQuality.EmptyValue)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)24);
			((WriteContext)(ref output)).WriteEnum((int)GoodsQuality);
		}
		if (SortIndex != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)32);
			((WriteContext)(ref output)).WriteInt32(SortIndex);
		}
		if (RefreshType != ERefreshType.EmptyValue)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)40);
			((WriteContext)(ref output)).WriteEnum((int)RefreshType);
		}
		if (ExpireTm != 0L)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)48);
			((WriteContext)(ref output)).WriteInt64(ExpireTm);
		}
		if (LimitNum != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)56);
			((WriteContext)(ref output)).WriteInt32(LimitNum);
		}
		if (BoughtCount != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)64);
			((WriteContext)(ref output)).WriteInt32(BoughtCount);
		}
		if (ShowPrice != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)72);
			((WriteContext)(ref output)).WriteInt32(ShowPrice);
		}
		if (SalePrice != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)80);
			((WriteContext)(ref output)).WriteInt32(SalePrice);
		}
		if (Discount != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)88);
			((WriteContext)(ref output)).WriteInt32(Discount);
		}
		awardList_.WriteTo(ref output, _repeated_awardList_codec);
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
		if (GoodsId != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(GoodsId);
		}
		if (GoodsType != EVoucherGoodsType.EmptyValue)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)GoodsType);
		}
		if (GoodsQuality != EQuality.EmptyValue)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)GoodsQuality);
		}
		if (SortIndex != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(SortIndex);
		}
		if (RefreshType != ERefreshType.EmptyValue)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)RefreshType);
		}
		if (ExpireTm != 0L)
		{
			num += 1 + CodedOutputStream.ComputeInt64Size(ExpireTm);
		}
		if (LimitNum != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(LimitNum);
		}
		if (BoughtCount != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(BoughtCount);
		}
		if (ShowPrice != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(ShowPrice);
		}
		if (SalePrice != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(SalePrice);
		}
		if (Discount != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(Discount);
		}
		num += awardList_.CalculateSize(_repeated_awardList_codec);
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(VoucherShopGoodsDetail other)
	{
		if (other != null)
		{
			if (other.GoodsId != 0)
			{
				GoodsId = other.GoodsId;
			}
			if (other.GoodsType != EVoucherGoodsType.EmptyValue)
			{
				GoodsType = other.GoodsType;
			}
			if (other.GoodsQuality != EQuality.EmptyValue)
			{
				GoodsQuality = other.GoodsQuality;
			}
			if (other.SortIndex != 0)
			{
				SortIndex = other.SortIndex;
			}
			if (other.RefreshType != ERefreshType.EmptyValue)
			{
				RefreshType = other.RefreshType;
			}
			if (other.ExpireTm != 0L)
			{
				ExpireTm = other.ExpireTm;
			}
			if (other.LimitNum != 0)
			{
				LimitNum = other.LimitNum;
			}
			if (other.BoughtCount != 0)
			{
				BoughtCount = other.BoughtCount;
			}
			if (other.ShowPrice != 0)
			{
				ShowPrice = other.ShowPrice;
			}
			if (other.SalePrice != 0)
			{
				SalePrice = other.SalePrice;
			}
			if (other.Discount != 0)
			{
				Discount = other.Discount;
			}
			awardList_.Add((IEnumerable<AwardDetail>)other.awardList_);
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
				GoodsId = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 16u:
				GoodsType = (EVoucherGoodsType)((ParseContext)(ref input)).ReadEnum();
				break;
			case 24u:
				GoodsQuality = (EQuality)((ParseContext)(ref input)).ReadEnum();
				break;
			case 32u:
				SortIndex = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 40u:
				RefreshType = (ERefreshType)((ParseContext)(ref input)).ReadEnum();
				break;
			case 48u:
				ExpireTm = ((ParseContext)(ref input)).ReadInt64();
				break;
			case 56u:
				LimitNum = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 64u:
				BoughtCount = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 72u:
				ShowPrice = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 80u:
				SalePrice = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 88u:
				Discount = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 98u:
				awardList_.AddEntriesFrom(ref input, _repeated_awardList_codec);
				break;
			}
		}
	}
}
