using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class RechargeShopInfo : IMessage<RechargeShopInfo>, IMessage, IEquatable<RechargeShopInfo>, IDeepCloneable<RechargeShopInfo>, IBufferMessage
{
	private static readonly MessageParser<RechargeShopInfo> _parser = new MessageParser<RechargeShopInfo>((Func<RechargeShopInfo>)(() => new RechargeShopInfo()));

	private UnknownFieldSet _unknownFields;

	public const int DiamondShopFieldNumber = 1;

	private static readonly FieldCodec<DiamondShopDetail> _repeated_diamondShop_codec = FieldCodec.ForMessage<DiamondShopDetail>(10u, DiamondShopDetail.Parser);

	private readonly RepeatedField<DiamondShopDetail> diamondShop_ = new RepeatedField<DiamondShopDetail>();

	public const int VipCardShopFieldNumber = 2;

	private static readonly FieldCodec<VipCardShopDetail> _repeated_vipCardShop_codec = FieldCodec.ForMessage<VipCardShopDetail>(18u, VipCardShopDetail.Parser);

	private readonly RepeatedField<VipCardShopDetail> vipCardShop_ = new RepeatedField<VipCardShopDetail>();

	public const int VoucherShopFieldNumber = 3;

	private static readonly FieldCodec<VoucherShopDetail> _repeated_voucherShop_codec = FieldCodec.ForMessage<VoucherShopDetail>(26u, VoucherShopDetail.Parser);

	private readonly RepeatedField<VoucherShopDetail> voucherShop_ = new RepeatedField<VoucherShopDetail>();

	public const int FuncShopFieldNumber = 4;

	private static readonly FieldCodec<FuncShopDetail> _repeated_funcShop_codec = FieldCodec.ForMessage<FuncShopDetail>(34u, FuncShopDetail.Parser);

	private readonly RepeatedField<FuncShopDetail> funcShop_ = new RepeatedField<FuncShopDetail>();

	public const int GrowthGiftShopFieldNumber = 5;

	private static readonly FieldCodec<GrowthGiftShopDetail> _repeated_growthGiftShop_codec = FieldCodec.ForMessage<GrowthGiftShopDetail>(42u, GrowthGiftShopDetail.Parser);

	private readonly RepeatedField<GrowthGiftShopDetail> growthGiftShop_ = new RepeatedField<GrowthGiftShopDetail>();

	public const int PathwayShopFieldNumber = 6;

	private static readonly FieldCodec<RechargeGoodsBaseInfo> _repeated_pathwayShop_codec = FieldCodec.ForMessage<RechargeGoodsBaseInfo>(50u, RechargeGoodsBaseInfo.Parser);

	private readonly RepeatedField<RechargeGoodsBaseInfo> pathwayShop_ = new RepeatedField<RechargeGoodsBaseInfo>();

	public const int RepaymentShopFieldNumber = 7;

	private static readonly FieldCodec<RechargeGoodsBaseInfo> _repeated_repaymentShop_codec = FieldCodec.ForMessage<RechargeGoodsBaseInfo>(58u, RechargeGoodsBaseInfo.Parser);

	private readonly RepeatedField<RechargeGoodsBaseInfo> repaymentShop_ = new RepeatedField<RechargeGoodsBaseInfo>();

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<RechargeShopInfo> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeRechargeShopInfoReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<DiamondShopDetail> DiamondShop => diamondShop_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<VipCardShopDetail> VipCardShop => vipCardShop_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<VoucherShopDetail> VoucherShop => voucherShop_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<FuncShopDetail> FuncShop => funcShop_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<GrowthGiftShopDetail> GrowthGiftShop => growthGiftShop_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<RechargeGoodsBaseInfo> PathwayShop => pathwayShop_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<RechargeGoodsBaseInfo> RepaymentShop => repaymentShop_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RechargeShopInfo()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RechargeShopInfo(RechargeShopInfo other)
		: this()
	{
		diamondShop_ = other.diamondShop_.Clone();
		vipCardShop_ = other.vipCardShop_.Clone();
		voucherShop_ = other.voucherShop_.Clone();
		funcShop_ = other.funcShop_.Clone();
		growthGiftShop_ = other.growthGiftShop_.Clone();
		pathwayShop_ = other.pathwayShop_.Clone();
		repaymentShop_ = other.repaymentShop_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RechargeShopInfo Clone()
	{
		return new RechargeShopInfo(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as RechargeShopInfo);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(RechargeShopInfo other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!diamondShop_.Equals(other.diamondShop_))
		{
			return false;
		}
		if (!vipCardShop_.Equals(other.vipCardShop_))
		{
			return false;
		}
		if (!voucherShop_.Equals(other.voucherShop_))
		{
			return false;
		}
		if (!funcShop_.Equals(other.funcShop_))
		{
			return false;
		}
		if (!growthGiftShop_.Equals(other.growthGiftShop_))
		{
			return false;
		}
		if (!pathwayShop_.Equals(other.pathwayShop_))
		{
			return false;
		}
		if (!repaymentShop_.Equals(other.repaymentShop_))
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
		num ^= ((object)diamondShop_).GetHashCode();
		num ^= ((object)vipCardShop_).GetHashCode();
		num ^= ((object)voucherShop_).GetHashCode();
		num ^= ((object)funcShop_).GetHashCode();
		num ^= ((object)growthGiftShop_).GetHashCode();
		num ^= ((object)pathwayShop_).GetHashCode();
		num ^= ((object)repaymentShop_).GetHashCode();
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
		diamondShop_.WriteTo(ref output, _repeated_diamondShop_codec);
		vipCardShop_.WriteTo(ref output, _repeated_vipCardShop_codec);
		voucherShop_.WriteTo(ref output, _repeated_voucherShop_codec);
		funcShop_.WriteTo(ref output, _repeated_funcShop_codec);
		growthGiftShop_.WriteTo(ref output, _repeated_growthGiftShop_codec);
		pathwayShop_.WriteTo(ref output, _repeated_pathwayShop_codec);
		repaymentShop_.WriteTo(ref output, _repeated_repaymentShop_codec);
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
		num += diamondShop_.CalculateSize(_repeated_diamondShop_codec);
		num += vipCardShop_.CalculateSize(_repeated_vipCardShop_codec);
		num += voucherShop_.CalculateSize(_repeated_voucherShop_codec);
		num += funcShop_.CalculateSize(_repeated_funcShop_codec);
		num += growthGiftShop_.CalculateSize(_repeated_growthGiftShop_codec);
		num += pathwayShop_.CalculateSize(_repeated_pathwayShop_codec);
		num += repaymentShop_.CalculateSize(_repeated_repaymentShop_codec);
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(RechargeShopInfo other)
	{
		if (other != null)
		{
			diamondShop_.Add((IEnumerable<DiamondShopDetail>)other.diamondShop_);
			vipCardShop_.Add((IEnumerable<VipCardShopDetail>)other.vipCardShop_);
			voucherShop_.Add((IEnumerable<VoucherShopDetail>)other.voucherShop_);
			funcShop_.Add((IEnumerable<FuncShopDetail>)other.funcShop_);
			growthGiftShop_.Add((IEnumerable<GrowthGiftShopDetail>)other.growthGiftShop_);
			pathwayShop_.Add((IEnumerable<RechargeGoodsBaseInfo>)other.pathwayShop_);
			repaymentShop_.Add((IEnumerable<RechargeGoodsBaseInfo>)other.repaymentShop_);
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
				diamondShop_.AddEntriesFrom(ref input, _repeated_diamondShop_codec);
				break;
			case 18u:
				vipCardShop_.AddEntriesFrom(ref input, _repeated_vipCardShop_codec);
				break;
			case 26u:
				voucherShop_.AddEntriesFrom(ref input, _repeated_voucherShop_codec);
				break;
			case 34u:
				funcShop_.AddEntriesFrom(ref input, _repeated_funcShop_codec);
				break;
			case 42u:
				growthGiftShop_.AddEntriesFrom(ref input, _repeated_growthGiftShop_codec);
				break;
			case 50u:
				pathwayShop_.AddEntriesFrom(ref input, _repeated_pathwayShop_codec);
				break;
			case 58u:
				repaymentShop_.AddEntriesFrom(ref input, _repeated_repaymentShop_codec);
				break;
			}
		}
	}
}
