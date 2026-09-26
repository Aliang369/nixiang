using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class VoucherShopCartInfo : IMessage<VoucherShopCartInfo>, IMessage, IEquatable<VoucherShopCartInfo>, IDeepCloneable<VoucherShopCartInfo>, IBufferMessage
{
	private static readonly MessageParser<VoucherShopCartInfo> _parser = new MessageParser<VoucherShopCartInfo>((Func<VoucherShopCartInfo>)(() => new VoucherShopCartInfo()));

	private UnknownFieldSet _unknownFields;

	public const int GoodsListFieldNumber = 1;

	private static readonly FieldCodec<VoucherShopCartGoodsDetail> _repeated_goodsList_codec = FieldCodec.ForMessage<VoucherShopCartGoodsDetail>(10u, VoucherShopCartGoodsDetail.Parser);

	private readonly RepeatedField<VoucherShopCartGoodsDetail> goodsList_ = new RepeatedField<VoucherShopCartGoodsDetail>();

	public const int AvailableCouponListFieldNumber = 2;

	private static readonly FieldCodec<VoucherShopCartCouponDetail> _repeated_availableCouponList_codec = FieldCodec.ForMessage<VoucherShopCartCouponDetail>(18u, VoucherShopCartCouponDetail.Parser);

	private readonly RepeatedField<VoucherShopCartCouponDetail> availableCouponList_ = new RepeatedField<VoucherShopCartCouponDetail>();

	public const int RecommendResultFieldNumber = 3;

	private VoucherShopCartResult recommendResult_;

	public const int SelectedResultFieldNumber = 4;

	private VoucherShopCartResult selectedResult_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<VoucherShopCartInfo> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeVoucherShopCartInfoReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<VoucherShopCartGoodsDetail> GoodsList => goodsList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<VoucherShopCartCouponDetail> AvailableCouponList => availableCouponList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public VoucherShopCartResult RecommendResult
	{
		get
		{
			return recommendResult_;
		}
		set
		{
			recommendResult_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public VoucherShopCartResult SelectedResult
	{
		get
		{
			return selectedResult_;
		}
		set
		{
			selectedResult_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public VoucherShopCartInfo()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public VoucherShopCartInfo(VoucherShopCartInfo other)
		: this()
	{
		goodsList_ = other.goodsList_.Clone();
		availableCouponList_ = other.availableCouponList_.Clone();
		recommendResult_ = ((other.recommendResult_ != null) ? other.recommendResult_.Clone() : null);
		selectedResult_ = ((other.selectedResult_ != null) ? other.selectedResult_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public VoucherShopCartInfo Clone()
	{
		return new VoucherShopCartInfo(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as VoucherShopCartInfo);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(VoucherShopCartInfo other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!goodsList_.Equals(other.goodsList_))
		{
			return false;
		}
		if (!availableCouponList_.Equals(other.availableCouponList_))
		{
			return false;
		}
		if (!object.Equals(RecommendResult, other.RecommendResult))
		{
			return false;
		}
		if (!object.Equals(SelectedResult, other.SelectedResult))
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
		num ^= ((object)goodsList_).GetHashCode();
		num ^= ((object)availableCouponList_).GetHashCode();
		if (recommendResult_ != null)
		{
			num ^= RecommendResult.GetHashCode();
		}
		if (selectedResult_ != null)
		{
			num ^= SelectedResult.GetHashCode();
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
		goodsList_.WriteTo(ref output, _repeated_goodsList_codec);
		availableCouponList_.WriteTo(ref output, _repeated_availableCouponList_codec);
		if (recommendResult_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)26);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)RecommendResult);
		}
		if (selectedResult_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)34);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)SelectedResult);
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
		num += goodsList_.CalculateSize(_repeated_goodsList_codec);
		num += availableCouponList_.CalculateSize(_repeated_availableCouponList_codec);
		if (recommendResult_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)RecommendResult);
		}
		if (selectedResult_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)SelectedResult);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(VoucherShopCartInfo other)
	{
		if (other == null)
		{
			return;
		}
		goodsList_.Add((IEnumerable<VoucherShopCartGoodsDetail>)other.goodsList_);
		availableCouponList_.Add((IEnumerable<VoucherShopCartCouponDetail>)other.availableCouponList_);
		if (other.recommendResult_ != null)
		{
			if (recommendResult_ == null)
			{
				RecommendResult = new VoucherShopCartResult();
			}
			RecommendResult.MergeFrom(other.RecommendResult);
		}
		if (other.selectedResult_ != null)
		{
			if (selectedResult_ == null)
			{
				SelectedResult = new VoucherShopCartResult();
			}
			SelectedResult.MergeFrom(other.SelectedResult);
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
				goodsList_.AddEntriesFrom(ref input, _repeated_goodsList_codec);
				break;
			case 18u:
				availableCouponList_.AddEntriesFrom(ref input, _repeated_availableCouponList_codec);
				break;
			case 26u:
				if (recommendResult_ == null)
				{
					RecommendResult = new VoucherShopCartResult();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)RecommendResult);
				break;
			case 34u:
				if (selectedResult_ == null)
				{
					SelectedResult = new VoucherShopCartResult();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)SelectedResult);
				break;
			}
		}
	}
}
