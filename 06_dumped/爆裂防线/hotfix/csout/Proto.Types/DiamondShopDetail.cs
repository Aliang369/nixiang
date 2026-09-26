using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class DiamondShopDetail : IMessage<DiamondShopDetail>, IMessage, IEquatable<DiamondShopDetail>, IDeepCloneable<DiamondShopDetail>, IBufferMessage
{
	private static readonly MessageParser<DiamondShopDetail> _parser = new MessageParser<DiamondShopDetail>((Func<DiamondShopDetail>)(() => new DiamondShopDetail()));

	private UnknownFieldSet _unknownFields;

	public const int GoodsIdFieldNumber = 1;

	private int goodsId_;

	public const int CcyFieldNumber = 2;

	private ECCYType ccy_;

	public const int PriceFieldNumber = 3;

	private float price_;

	public const int DiamondFieldNumber = 4;

	private int diamond_;

	public const int GiveDiamondFieldNumber = 5;

	private int giveDiamond_;

	public const int BoughtCountFieldNumber = 6;

	private int boughtCount_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<DiamondShopDetail> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeDiamondShopDetailReflection.Descriptor.MessageTypes[0];

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
	public int GiveDiamond
	{
		get
		{
			return giveDiamond_;
		}
		set
		{
			giveDiamond_ = value;
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
	public DiamondShopDetail()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public DiamondShopDetail(DiamondShopDetail other)
		: this()
	{
		goodsId_ = other.goodsId_;
		ccy_ = other.ccy_;
		price_ = other.price_;
		diamond_ = other.diamond_;
		giveDiamond_ = other.giveDiamond_;
		boughtCount_ = other.boughtCount_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public DiamondShopDetail Clone()
	{
		return new DiamondShopDetail(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as DiamondShopDetail);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(DiamondShopDetail other)
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
		if (GiveDiamond != other.GiveDiamond)
		{
			return false;
		}
		if (BoughtCount != other.BoughtCount)
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
		if (GiveDiamond != 0)
		{
			num ^= GiveDiamond.GetHashCode();
		}
		if (BoughtCount != 0)
		{
			num ^= BoughtCount.GetHashCode();
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
		if (GiveDiamond != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)40);
			((WriteContext)(ref output)).WriteInt32(GiveDiamond);
		}
		if (BoughtCount != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)48);
			((WriteContext)(ref output)).WriteInt32(BoughtCount);
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
		if (GiveDiamond != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(GiveDiamond);
		}
		if (BoughtCount != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(BoughtCount);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(DiamondShopDetail other)
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
			if (other.GiveDiamond != 0)
			{
				GiveDiamond = other.GiveDiamond;
			}
			if (other.BoughtCount != 0)
			{
				BoughtCount = other.BoughtCount;
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
				GiveDiamond = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 48u:
				BoughtCount = ((ParseContext)(ref input)).ReadInt32();
				break;
			}
		}
	}
}
