using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class LimitTimeShopItemDetail : IMessage<LimitTimeShopItemDetail>, IMessage, IEquatable<LimitTimeShopItemDetail>, IDeepCloneable<LimitTimeShopItemDetail>, IBufferMessage
{
	private static readonly MessageParser<LimitTimeShopItemDetail> _parser = new MessageParser<LimitTimeShopItemDetail>((Func<LimitTimeShopItemDetail>)(() => new LimitTimeShopItemDetail()));

	private UnknownFieldSet _unknownFields;

	public const int IndexFieldNumber = 1;

	private int index_;

	public const int DiscountFieldNumber = 2;

	private int discount_;

	public const int SalePriceFieldNumber = 3;

	private int salePrice_;

	public const int LimitNumFieldNumber = 4;

	private int limitNum_;

	public const int CountFieldNumber = 5;

	private int count_;

	public const int AwardDetailFieldNumber = 6;

	private AwardDetail awardDetail_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<LimitTimeShopItemDetail> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeLimitTimeShopItemDetailReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int Index
	{
		get
		{
			return index_;
		}
		set
		{
			index_ = value;
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
	public int Count
	{
		get
		{
			return count_;
		}
		set
		{
			count_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public AwardDetail AwardDetail
	{
		get
		{
			return awardDetail_;
		}
		set
		{
			awardDetail_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public LimitTimeShopItemDetail()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public LimitTimeShopItemDetail(LimitTimeShopItemDetail other)
		: this()
	{
		index_ = other.index_;
		discount_ = other.discount_;
		salePrice_ = other.salePrice_;
		limitNum_ = other.limitNum_;
		count_ = other.count_;
		awardDetail_ = ((other.awardDetail_ != null) ? other.awardDetail_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public LimitTimeShopItemDetail Clone()
	{
		return new LimitTimeShopItemDetail(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as LimitTimeShopItemDetail);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(LimitTimeShopItemDetail other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (Index != other.Index)
		{
			return false;
		}
		if (Discount != other.Discount)
		{
			return false;
		}
		if (SalePrice != other.SalePrice)
		{
			return false;
		}
		if (LimitNum != other.LimitNum)
		{
			return false;
		}
		if (Count != other.Count)
		{
			return false;
		}
		if (!object.Equals(AwardDetail, other.AwardDetail))
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
		if (Index != 0)
		{
			num ^= Index.GetHashCode();
		}
		if (Discount != 0)
		{
			num ^= Discount.GetHashCode();
		}
		if (SalePrice != 0)
		{
			num ^= SalePrice.GetHashCode();
		}
		if (LimitNum != 0)
		{
			num ^= LimitNum.GetHashCode();
		}
		if (Count != 0)
		{
			num ^= Count.GetHashCode();
		}
		if (awardDetail_ != null)
		{
			num ^= AwardDetail.GetHashCode();
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
		if (Index != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteInt32(Index);
		}
		if (Discount != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteInt32(Discount);
		}
		if (SalePrice != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)24);
			((WriteContext)(ref output)).WriteInt32(SalePrice);
		}
		if (LimitNum != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)32);
			((WriteContext)(ref output)).WriteInt32(LimitNum);
		}
		if (Count != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)40);
			((WriteContext)(ref output)).WriteInt32(Count);
		}
		if (awardDetail_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)50);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)AwardDetail);
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
		if (Index != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(Index);
		}
		if (Discount != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(Discount);
		}
		if (SalePrice != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(SalePrice);
		}
		if (LimitNum != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(LimitNum);
		}
		if (Count != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(Count);
		}
		if (awardDetail_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)AwardDetail);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(LimitTimeShopItemDetail other)
	{
		if (other == null)
		{
			return;
		}
		if (other.Index != 0)
		{
			Index = other.Index;
		}
		if (other.Discount != 0)
		{
			Discount = other.Discount;
		}
		if (other.SalePrice != 0)
		{
			SalePrice = other.SalePrice;
		}
		if (other.LimitNum != 0)
		{
			LimitNum = other.LimitNum;
		}
		if (other.Count != 0)
		{
			Count = other.Count;
		}
		if (other.awardDetail_ != null)
		{
			if (awardDetail_ == null)
			{
				AwardDetail = new AwardDetail();
			}
			AwardDetail.MergeFrom(other.AwardDetail);
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
			case 8u:
				Index = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 16u:
				Discount = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 24u:
				SalePrice = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 32u:
				LimitNum = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 40u:
				Count = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 50u:
				if (awardDetail_ == null)
				{
					AwardDetail = new AwardDetail();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)AwardDetail);
				break;
			}
		}
	}
}
