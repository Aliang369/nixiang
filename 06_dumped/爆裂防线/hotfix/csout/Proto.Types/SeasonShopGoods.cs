using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class SeasonShopGoods : IMessage<SeasonShopGoods>, IMessage, IEquatable<SeasonShopGoods>, IDeepCloneable<SeasonShopGoods>, IBufferMessage
{
	private static readonly MessageParser<SeasonShopGoods> _parser = new MessageParser<SeasonShopGoods>((Func<SeasonShopGoods>)(() => new SeasonShopGoods()));

	private UnknownFieldSet _unknownFields;

	public const int ShopIdFieldNumber = 1;

	private int shopId_;

	public const int ResTypeFieldNumber = 2;

	private int resType_;

	public const int ResIdFieldNumber = 3;

	private int resId_;

	public const int ResCountFieldNumber = 4;

	private int resCount_;

	public const int MoneyTypeFieldNumber = 5;

	private int moneyType_;

	public const int PriceFieldNumber = 6;

	private int price_;

	public const int LimitNumFieldNumber = 7;

	private int limitNum_;

	public const int BoughtFieldNumber = 8;

	private int bought_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<SeasonShopGoods> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeSeasonShopGoodsReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int ShopId
	{
		get
		{
			return shopId_;
		}
		set
		{
			shopId_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int ResType
	{
		get
		{
			return resType_;
		}
		set
		{
			resType_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int ResId
	{
		get
		{
			return resId_;
		}
		set
		{
			resId_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int ResCount
	{
		get
		{
			return resCount_;
		}
		set
		{
			resCount_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int MoneyType
	{
		get
		{
			return moneyType_;
		}
		set
		{
			moneyType_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int Price
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
	public int Bought
	{
		get
		{
			return bought_;
		}
		set
		{
			bought_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SeasonShopGoods()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SeasonShopGoods(SeasonShopGoods other)
		: this()
	{
		shopId_ = other.shopId_;
		resType_ = other.resType_;
		resId_ = other.resId_;
		resCount_ = other.resCount_;
		moneyType_ = other.moneyType_;
		price_ = other.price_;
		limitNum_ = other.limitNum_;
		bought_ = other.bought_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SeasonShopGoods Clone()
	{
		return new SeasonShopGoods(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as SeasonShopGoods);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(SeasonShopGoods other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (ShopId != other.ShopId)
		{
			return false;
		}
		if (ResType != other.ResType)
		{
			return false;
		}
		if (ResId != other.ResId)
		{
			return false;
		}
		if (ResCount != other.ResCount)
		{
			return false;
		}
		if (MoneyType != other.MoneyType)
		{
			return false;
		}
		if (Price != other.Price)
		{
			return false;
		}
		if (LimitNum != other.LimitNum)
		{
			return false;
		}
		if (Bought != other.Bought)
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
		if (ShopId != 0)
		{
			num ^= ShopId.GetHashCode();
		}
		if (ResType != 0)
		{
			num ^= ResType.GetHashCode();
		}
		if (ResId != 0)
		{
			num ^= ResId.GetHashCode();
		}
		if (ResCount != 0)
		{
			num ^= ResCount.GetHashCode();
		}
		if (MoneyType != 0)
		{
			num ^= MoneyType.GetHashCode();
		}
		if (Price != 0)
		{
			num ^= Price.GetHashCode();
		}
		if (LimitNum != 0)
		{
			num ^= LimitNum.GetHashCode();
		}
		if (Bought != 0)
		{
			num ^= Bought.GetHashCode();
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
		if (ShopId != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteInt32(ShopId);
		}
		if (ResType != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteInt32(ResType);
		}
		if (ResId != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)24);
			((WriteContext)(ref output)).WriteInt32(ResId);
		}
		if (ResCount != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)32);
			((WriteContext)(ref output)).WriteInt32(ResCount);
		}
		if (MoneyType != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)40);
			((WriteContext)(ref output)).WriteInt32(MoneyType);
		}
		if (Price != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)48);
			((WriteContext)(ref output)).WriteInt32(Price);
		}
		if (LimitNum != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)56);
			((WriteContext)(ref output)).WriteInt32(LimitNum);
		}
		if (Bought != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)64);
			((WriteContext)(ref output)).WriteInt32(Bought);
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
		if (ShopId != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(ShopId);
		}
		if (ResType != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(ResType);
		}
		if (ResId != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(ResId);
		}
		if (ResCount != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(ResCount);
		}
		if (MoneyType != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(MoneyType);
		}
		if (Price != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(Price);
		}
		if (LimitNum != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(LimitNum);
		}
		if (Bought != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(Bought);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(SeasonShopGoods other)
	{
		if (other != null)
		{
			if (other.ShopId != 0)
			{
				ShopId = other.ShopId;
			}
			if (other.ResType != 0)
			{
				ResType = other.ResType;
			}
			if (other.ResId != 0)
			{
				ResId = other.ResId;
			}
			if (other.ResCount != 0)
			{
				ResCount = other.ResCount;
			}
			if (other.MoneyType != 0)
			{
				MoneyType = other.MoneyType;
			}
			if (other.Price != 0)
			{
				Price = other.Price;
			}
			if (other.LimitNum != 0)
			{
				LimitNum = other.LimitNum;
			}
			if (other.Bought != 0)
			{
				Bought = other.Bought;
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
				ShopId = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 16u:
				ResType = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 24u:
				ResId = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 32u:
				ResCount = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 40u:
				MoneyType = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 48u:
				Price = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 56u:
				LimitNum = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 64u:
				Bought = ((ParseContext)(ref input)).ReadInt32();
				break;
			}
		}
	}
}
