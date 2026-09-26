using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class VipCardShopDetail : IMessage<VipCardShopDetail>, IMessage, IEquatable<VipCardShopDetail>, IDeepCloneable<VipCardShopDetail>, IBufferMessage
{
	private static readonly MessageParser<VipCardShopDetail> _parser = new MessageParser<VipCardShopDetail>((Func<VipCardShopDetail>)(() => new VipCardShopDetail()));

	private UnknownFieldSet _unknownFields;

	public const int GoodsIdFieldNumber = 1;

	private int goodsId_;

	public const int CcyFieldNumber = 2;

	private ECCYType ccy_;

	public const int PriceFieldNumber = 3;

	private float price_;

	public const int DiamondFieldNumber = 4;

	private int diamond_;

	public const int ExpireTmFieldNumber = 5;

	private long expireTm_;

	public const int DailyAwardFieldNumber = 6;

	private EActiveState dailyAward_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<VipCardShopDetail> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeVipCardShopDetailReflection.Descriptor.MessageTypes[0];

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
	public ECCYType Ccy
	{
		get
		{
			return ccy_;
		}
		set
		{
			ccy_ = value;
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
	public int Diamond
	{
		get
		{
			return diamond_;
		}
		set
		{
			diamond_ = value;
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
	public EActiveState DailyAward
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
	public VipCardShopDetail()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public VipCardShopDetail(VipCardShopDetail other)
		: this()
	{
		goodsId_ = other.goodsId_;
		ccy_ = other.ccy_;
		price_ = other.price_;
		diamond_ = other.diamond_;
		expireTm_ = other.expireTm_;
		dailyAward_ = other.dailyAward_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public VipCardShopDetail Clone()
	{
		return new VipCardShopDetail(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as VipCardShopDetail);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(VipCardShopDetail other)
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
		if (Ccy != other.Ccy)
		{
			return false;
		}
		if (!ProtobufEqualityComparers.BitwiseSingleEqualityComparer.Equals(Price, other.Price))
		{
			return false;
		}
		if (Diamond != other.Diamond)
		{
			return false;
		}
		if (ExpireTm != other.ExpireTm)
		{
			return false;
		}
		if (DailyAward != other.DailyAward)
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
		if (Ccy != ECCYType.EmptyValue)
		{
			num ^= Ccy.GetHashCode();
		}
		if (Price != 0f)
		{
			num ^= ProtobufEqualityComparers.BitwiseSingleEqualityComparer.GetHashCode(Price);
		}
		if (Diamond != 0)
		{
			num ^= Diamond.GetHashCode();
		}
		if (ExpireTm != 0L)
		{
			num ^= ExpireTm.GetHashCode();
		}
		if (DailyAward != EActiveState.Pending)
		{
			num ^= DailyAward.GetHashCode();
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
		if (Ccy != ECCYType.EmptyValue)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteEnum((int)Ccy);
		}
		if (Price != 0f)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)29);
			((WriteContext)(ref output)).WriteFloat(Price);
		}
		if (Diamond != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)32);
			((WriteContext)(ref output)).WriteInt32(Diamond);
		}
		if (ExpireTm != 0L)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)40);
			((WriteContext)(ref output)).WriteInt64(ExpireTm);
		}
		if (DailyAward != EActiveState.Pending)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)48);
			((WriteContext)(ref output)).WriteEnum((int)DailyAward);
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
		if (Ccy != ECCYType.EmptyValue)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)Ccy);
		}
		if (Price != 0f)
		{
			num += 5;
		}
		if (Diamond != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(Diamond);
		}
		if (ExpireTm != 0L)
		{
			num += 1 + CodedOutputStream.ComputeInt64Size(ExpireTm);
		}
		if (DailyAward != EActiveState.Pending)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)DailyAward);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(VipCardShopDetail other)
	{
		if (other != null)
		{
			if (other.GoodsId != 0)
			{
				GoodsId = other.GoodsId;
			}
			if (other.Ccy != ECCYType.EmptyValue)
			{
				Ccy = other.Ccy;
			}
			if (other.Price != 0f)
			{
				Price = other.Price;
			}
			if (other.Diamond != 0)
			{
				Diamond = other.Diamond;
			}
			if (other.ExpireTm != 0L)
			{
				ExpireTm = other.ExpireTm;
			}
			if (other.DailyAward != EActiveState.Pending)
			{
				DailyAward = other.DailyAward;
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
				Ccy = (ECCYType)((ParseContext)(ref input)).ReadEnum();
				break;
			case 29u:
				Price = ((ParseContext)(ref input)).ReadFloat();
				break;
			case 32u:
				Diamond = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 40u:
				ExpireTm = ((ParseContext)(ref input)).ReadInt64();
				break;
			case 48u:
				DailyAward = (EActiveState)((ParseContext)(ref input)).ReadEnum();
				break;
			}
		}
	}
}
