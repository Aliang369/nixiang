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
public sealed class RechargeGoodsDetail : IMessage<RechargeGoodsDetail>, IMessage, IEquatable<RechargeGoodsDetail>, IDeepCloneable<RechargeGoodsDetail>, IBufferMessage
{
	private static readonly MessageParser<RechargeGoodsDetail> _parser = new MessageParser<RechargeGoodsDetail>((Func<RechargeGoodsDetail>)(() => new RechargeGoodsDetail()));

	private UnknownFieldSet _unknownFields;

	public const int GoodsIdFieldNumber = 1;

	private int goodsId_;

	public const int CCYFieldNumber = 2;

	private ECCYType cCY_;

	public const int PriceFieldNumber = 3;

	private float price_;

	public const int LimitNumFieldNumber = 4;

	private int limitNum_;

	public const int LimitVipFieldNumber = 5;

	private int limitVip_;

	public const int BoughtCountFieldNumber = 6;

	private int boughtCount_;

	public const int AwardListFieldNumber = 7;

	private static readonly FieldCodec<AwardDetail> _repeated_awardList_codec = FieldCodec.ForMessage<AwardDetail>(58u, AwardDetail.Parser);

	private readonly RepeatedField<AwardDetail> awardList_ = new RepeatedField<AwardDetail>();

	public const int BonusFieldNumber = 8;

	private string bonus_ = "";

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<RechargeGoodsDetail> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeRechargeGoodsDetailReflection.Descriptor.MessageTypes[0];

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
	public ECCYType CCY
	{
		get
		{
			return cCY_;
		}
		set
		{
			cCY_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public float Price
	{
		get
		{
			return price_;
		}
		set
		{
			price_ = value;
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
	public int LimitVip
	{
		get
		{
			return limitVip_;
		}
		set
		{
			limitVip_ = value;
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
	public RepeatedField<AwardDetail> AwardList => awardList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public string Bonus
	{
		get
		{
			return bonus_;
		}
		set
		{
			bonus_ = ProtoPreconditions.CheckNotNull<string>(value, "value");
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RechargeGoodsDetail()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RechargeGoodsDetail(RechargeGoodsDetail other)
		: this()
	{
		goodsId_ = other.goodsId_;
		cCY_ = other.cCY_;
		price_ = other.price_;
		limitNum_ = other.limitNum_;
		limitVip_ = other.limitVip_;
		boughtCount_ = other.boughtCount_;
		awardList_ = other.awardList_.Clone();
		bonus_ = other.bonus_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RechargeGoodsDetail Clone()
	{
		return new RechargeGoodsDetail(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as RechargeGoodsDetail);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(RechargeGoodsDetail other)
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
		if (CCY != other.CCY)
		{
			return false;
		}
		if (!ProtobufEqualityComparers.BitwiseSingleEqualityComparer.Equals(Price, other.Price))
		{
			return false;
		}
		if (LimitNum != other.LimitNum)
		{
			return false;
		}
		if (LimitVip != other.LimitVip)
		{
			return false;
		}
		if (BoughtCount != other.BoughtCount)
		{
			return false;
		}
		if (!awardList_.Equals(other.awardList_))
		{
			return false;
		}
		if (Bonus != other.Bonus)
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
		if (CCY != ECCYType.EmptyValue)
		{
			num ^= CCY.GetHashCode();
		}
		if (Price != 0f)
		{
			num ^= ProtobufEqualityComparers.BitwiseSingleEqualityComparer.GetHashCode(Price);
		}
		if (LimitNum != 0)
		{
			num ^= LimitNum.GetHashCode();
		}
		if (LimitVip != 0)
		{
			num ^= LimitVip.GetHashCode();
		}
		if (BoughtCount != 0)
		{
			num ^= BoughtCount.GetHashCode();
		}
		num ^= ((object)awardList_).GetHashCode();
		if (Bonus.Length != 0)
		{
			num ^= Bonus.GetHashCode();
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
		if (GoodsId != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteInt32(GoodsId);
		}
		if (CCY != ECCYType.EmptyValue)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteEnum((int)CCY);
		}
		if (Price != 0f)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)29);
			((WriteContext)(ref output)).WriteFloat(Price);
		}
		if (LimitNum != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)32);
			((WriteContext)(ref output)).WriteInt32(LimitNum);
		}
		if (LimitVip != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)40);
			((WriteContext)(ref output)).WriteInt32(LimitVip);
		}
		if (BoughtCount != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)48);
			((WriteContext)(ref output)).WriteInt32(BoughtCount);
		}
		awardList_.WriteTo(ref output, _repeated_awardList_codec);
		if (Bonus.Length != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)66);
			((WriteContext)(ref output)).WriteString(Bonus);
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
		if (GoodsId != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(GoodsId);
		}
		if (CCY != ECCYType.EmptyValue)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)CCY);
		}
		if (Price != 0f)
		{
			num += 5;
		}
		if (LimitNum != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(LimitNum);
		}
		if (LimitVip != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(LimitVip);
		}
		if (BoughtCount != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(BoughtCount);
		}
		num += awardList_.CalculateSize(_repeated_awardList_codec);
		if (Bonus.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(Bonus);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(RechargeGoodsDetail other)
	{
		if (other != null)
		{
			if (other.GoodsId != 0)
			{
				GoodsId = other.GoodsId;
			}
			if (other.CCY != ECCYType.EmptyValue)
			{
				CCY = other.CCY;
			}
			if (other.Price != 0f)
			{
				Price = other.Price;
			}
			if (other.LimitNum != 0)
			{
				LimitNum = other.LimitNum;
			}
			if (other.LimitVip != 0)
			{
				LimitVip = other.LimitVip;
			}
			if (other.BoughtCount != 0)
			{
				BoughtCount = other.BoughtCount;
			}
			awardList_.Add((IEnumerable<AwardDetail>)other.awardList_);
			if (other.Bonus.Length != 0)
			{
				Bonus = other.Bonus;
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
				GoodsId = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 16u:
				CCY = (ECCYType)((ParseContext)(ref input)).ReadEnum();
				break;
			case 29u:
				Price = ((ParseContext)(ref input)).ReadFloat();
				break;
			case 32u:
				LimitNum = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 40u:
				LimitVip = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 48u:
				BoughtCount = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 58u:
				awardList_.AddEntriesFrom(ref input, _repeated_awardList_codec);
				break;
			case 66u:
				Bonus = ((ParseContext)(ref input)).ReadString();
				break;
			}
		}
	}
}
