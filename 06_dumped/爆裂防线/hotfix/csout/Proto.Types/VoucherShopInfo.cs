using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class VoucherShopInfo : IMessage<VoucherShopInfo>, IMessage, IEquatable<VoucherShopInfo>, IDeepCloneable<VoucherShopInfo>, IBufferMessage
{
	private static readonly MessageParser<VoucherShopInfo> _parser = new MessageParser<VoucherShopInfo>((Func<VoucherShopInfo>)(() => new VoucherShopInfo()));

	private UnknownFieldSet _unknownFields;

	public const int CouponListFieldNumber = 1;

	private static readonly FieldCodec<VoucherCouponDetail> _repeated_couponList_codec = FieldCodec.ForMessage<VoucherCouponDetail>(10u, VoucherCouponDetail.Parser);

	private readonly RepeatedField<VoucherCouponDetail> couponList_ = new RepeatedField<VoucherCouponDetail>();

	public const int RecentBuyFieldNumber = 2;

	private static readonly FieldCodec<VoucherRecentBuyDetail> _repeated_recentBuy_codec = FieldCodec.ForMessage<VoucherRecentBuyDetail>(18u, VoucherRecentBuyDetail.Parser);

	private readonly RepeatedField<VoucherRecentBuyDetail> recentBuy_ = new RepeatedField<VoucherRecentBuyDetail>();

	public const int DiscountListFieldNumber = 3;

	private static readonly FieldCodec<int> _repeated_discountList_codec = FieldCodec.ForInt32(26u);

	private readonly RepeatedField<int> discountList_ = new RepeatedField<int>();

	public const int LimitTimeFieldNumber = 4;

	private static readonly FieldCodec<VoucherShopGoodsDetail> _repeated_limitTime_codec = FieldCodec.ForMessage<VoucherShopGoodsDetail>(34u, VoucherShopGoodsDetail.Parser);

	private readonly RepeatedField<VoucherShopGoodsDetail> limitTime_ = new RepeatedField<VoucherShopGoodsDetail>();

	public const int HotSaleFieldNumber = 5;

	private static readonly FieldCodec<VoucherShopGoodsDetail> _repeated_hotSale_codec = FieldCodec.ForMessage<VoucherShopGoodsDetail>(42u, VoucherShopGoodsDetail.Parser);

	private readonly RepeatedField<VoucherShopGoodsDetail> hotSale_ = new RepeatedField<VoucherShopGoodsDetail>();

	public const int ShopCartFieldNumber = 6;

	private VoucherShopCartInfo shopCart_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<VoucherShopInfo> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeVoucherShopInfoReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<VoucherCouponDetail> CouponList => couponList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<VoucherRecentBuyDetail> RecentBuy => recentBuy_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<int> DiscountList => discountList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<VoucherShopGoodsDetail> LimitTime => limitTime_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<VoucherShopGoodsDetail> HotSale => hotSale_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public VoucherShopCartInfo ShopCart
	{
		get
		{
			return shopCart_;
		}
		set
		{
			shopCart_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public VoucherShopInfo()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public VoucherShopInfo(VoucherShopInfo other)
		: this()
	{
		couponList_ = other.couponList_.Clone();
		recentBuy_ = other.recentBuy_.Clone();
		discountList_ = other.discountList_.Clone();
		limitTime_ = other.limitTime_.Clone();
		hotSale_ = other.hotSale_.Clone();
		shopCart_ = ((other.shopCart_ != null) ? other.shopCart_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public VoucherShopInfo Clone()
	{
		return new VoucherShopInfo(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as VoucherShopInfo);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(VoucherShopInfo other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!couponList_.Equals(other.couponList_))
		{
			return false;
		}
		if (!recentBuy_.Equals(other.recentBuy_))
		{
			return false;
		}
		if (!discountList_.Equals(other.discountList_))
		{
			return false;
		}
		if (!limitTime_.Equals(other.limitTime_))
		{
			return false;
		}
		if (!hotSale_.Equals(other.hotSale_))
		{
			return false;
		}
		if (!object.Equals(ShopCart, other.ShopCart))
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
		num ^= ((object)couponList_).GetHashCode();
		num ^= ((object)recentBuy_).GetHashCode();
		num ^= ((object)discountList_).GetHashCode();
		num ^= ((object)limitTime_).GetHashCode();
		num ^= ((object)hotSale_).GetHashCode();
		if (shopCart_ != null)
		{
			num ^= ShopCart.GetHashCode();
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
		couponList_.WriteTo(ref output, _repeated_couponList_codec);
		recentBuy_.WriteTo(ref output, _repeated_recentBuy_codec);
		discountList_.WriteTo(ref output, _repeated_discountList_codec);
		limitTime_.WriteTo(ref output, _repeated_limitTime_codec);
		hotSale_.WriteTo(ref output, _repeated_hotSale_codec);
		if (shopCart_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)50);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)ShopCart);
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
		num += couponList_.CalculateSize(_repeated_couponList_codec);
		num += recentBuy_.CalculateSize(_repeated_recentBuy_codec);
		num += discountList_.CalculateSize(_repeated_discountList_codec);
		num += limitTime_.CalculateSize(_repeated_limitTime_codec);
		num += hotSale_.CalculateSize(_repeated_hotSale_codec);
		if (shopCart_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)ShopCart);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(VoucherShopInfo other)
	{
		if (other == null)
		{
			return;
		}
		couponList_.Add((IEnumerable<VoucherCouponDetail>)other.couponList_);
		recentBuy_.Add((IEnumerable<VoucherRecentBuyDetail>)other.recentBuy_);
		discountList_.Add((IEnumerable<int>)other.discountList_);
		limitTime_.Add((IEnumerable<VoucherShopGoodsDetail>)other.limitTime_);
		hotSale_.Add((IEnumerable<VoucherShopGoodsDetail>)other.hotSale_);
		if (other.shopCart_ != null)
		{
			if (shopCart_ == null)
			{
				ShopCart = new VoucherShopCartInfo();
			}
			ShopCart.MergeFrom(other.ShopCart);
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
				couponList_.AddEntriesFrom(ref input, _repeated_couponList_codec);
				break;
			case 18u:
				recentBuy_.AddEntriesFrom(ref input, _repeated_recentBuy_codec);
				break;
			case 24u:
			case 26u:
				discountList_.AddEntriesFrom(ref input, _repeated_discountList_codec);
				break;
			case 34u:
				limitTime_.AddEntriesFrom(ref input, _repeated_limitTime_codec);
				break;
			case 42u:
				hotSale_.AddEntriesFrom(ref input, _repeated_hotSale_codec);
				break;
			case 50u:
				if (shopCart_ == null)
				{
					ShopCart = new VoucherShopCartInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)ShopCart);
				break;
			}
		}
	}
}
