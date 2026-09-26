using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class RechargeShopGrowthGiftAck : IMessage<RechargeShopGrowthGiftAck>, IMessage, IEquatable<RechargeShopGrowthGiftAck>, IDeepCloneable<RechargeShopGrowthGiftAck>, IBufferMessage
{
	private static readonly MessageParser<RechargeShopGrowthGiftAck> _parser = new MessageParser<RechargeShopGrowthGiftAck>((Func<RechargeShopGrowthGiftAck>)(() => new RechargeShopGrowthGiftAck()));

	private UnknownFieldSet _unknownFields;

	public const int GoodsIdFieldNumber = 1;

	private int goodsId_;

	public const int GrowthGiftShopFieldNumber = 2;

	private static readonly FieldCodec<GrowthGiftShopDetail> _repeated_growthGiftShop_codec = FieldCodec.ForMessage<GrowthGiftShopDetail>(18u, GrowthGiftShopDetail.Parser);

	private readonly RepeatedField<GrowthGiftShopDetail> growthGiftShop_ = new RepeatedField<GrowthGiftShopDetail>();

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<RechargeShopGrowthGiftAck> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeRechargeShopGrowthGiftAckReflection.Descriptor.MessageTypes[0];

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
	public RepeatedField<GrowthGiftShopDetail> GrowthGiftShop => growthGiftShop_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RechargeShopGrowthGiftAck()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RechargeShopGrowthGiftAck(RechargeShopGrowthGiftAck other)
		: this()
	{
		goodsId_ = other.goodsId_;
		growthGiftShop_ = other.growthGiftShop_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RechargeShopGrowthGiftAck Clone()
	{
		return new RechargeShopGrowthGiftAck(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as RechargeShopGrowthGiftAck);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(RechargeShopGrowthGiftAck other)
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
		if (!growthGiftShop_.Equals(other.growthGiftShop_))
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
		num ^= ((object)growthGiftShop_).GetHashCode();
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
		growthGiftShop_.WriteTo(ref output, _repeated_growthGiftShop_codec);
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
		num += growthGiftShop_.CalculateSize(_repeated_growthGiftShop_codec);
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(RechargeShopGrowthGiftAck other)
	{
		if (other != null)
		{
			if (other.GoodsId != 0)
			{
				GoodsId = other.GoodsId;
			}
			growthGiftShop_.Add((IEnumerable<GrowthGiftShopDetail>)other.growthGiftShop_);
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
			case 18u:
				growthGiftShop_.AddEntriesFrom(ref input, _repeated_growthGiftShop_codec);
				break;
			}
		}
	}
}
