using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class ShopSlotDetail : IMessage<ShopSlotDetail>, IMessage, IEquatable<ShopSlotDetail>, IDeepCloneable<ShopSlotDetail>, IBufferMessage
{
	private static readonly MessageParser<ShopSlotDetail> _parser = new MessageParser<ShopSlotDetail>((Func<ShopSlotDetail>)(() => new ShopSlotDetail()));

	private UnknownFieldSet _unknownFields;

	public const int IndexFieldNumber = 1;

	private int index_;

	public const int SlotItemFieldNumber = 2;

	private AwardDetail slotItem_;

	public const int MoneyTypeFieldNumber = 3;

	private EMoneyType moneyType_;

	public const int PriceFieldNumber = 4;

	private int price_;

	public const int RefreshTypeFieldNumber = 5;

	private ERefreshType refreshType_;

	public const int LimitFieldNumber = 6;

	private int limit_;

	public const int CountFieldNumber = 7;

	private int count_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<ShopSlotDetail> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeShopSlotDetailReflection.Descriptor.MessageTypes[0];

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
	public AwardDetail SlotItem
	{
		get
		{
			return slotItem_;
		}
		set
		{
			slotItem_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public EMoneyType MoneyType
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
	public int Limit
	{
		get
		{
			return limit_;
		}
		set
		{
			limit_ = value;
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
	public ShopSlotDetail()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ShopSlotDetail(ShopSlotDetail other)
		: this()
	{
		index_ = other.index_;
		slotItem_ = ((other.slotItem_ != null) ? other.slotItem_.Clone() : null);
		moneyType_ = other.moneyType_;
		price_ = other.price_;
		refreshType_ = other.refreshType_;
		limit_ = other.limit_;
		count_ = other.count_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ShopSlotDetail Clone()
	{
		return new ShopSlotDetail(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as ShopSlotDetail);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(ShopSlotDetail other)
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
		if (!object.Equals(SlotItem, other.SlotItem))
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
		if (RefreshType != other.RefreshType)
		{
			return false;
		}
		if (Limit != other.Limit)
		{
			return false;
		}
		if (Count != other.Count)
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
		if (slotItem_ != null)
		{
			num ^= SlotItem.GetHashCode();
		}
		if (MoneyType != EMoneyType.EmptyValue)
		{
			num ^= MoneyType.GetHashCode();
		}
		if (Price != 0)
		{
			num ^= Price.GetHashCode();
		}
		if (RefreshType != ERefreshType.EmptyValue)
		{
			num ^= RefreshType.GetHashCode();
		}
		if (Limit != 0)
		{
			num ^= Limit.GetHashCode();
		}
		if (Count != 0)
		{
			num ^= Count.GetHashCode();
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
		if (slotItem_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)18);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)SlotItem);
		}
		if (MoneyType != EMoneyType.EmptyValue)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)24);
			((WriteContext)(ref output)).WriteEnum((int)MoneyType);
		}
		if (Price != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)32);
			((WriteContext)(ref output)).WriteInt32(Price);
		}
		if (RefreshType != ERefreshType.EmptyValue)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)40);
			((WriteContext)(ref output)).WriteEnum((int)RefreshType);
		}
		if (Limit != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)48);
			((WriteContext)(ref output)).WriteInt32(Limit);
		}
		if (Count != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)56);
			((WriteContext)(ref output)).WriteInt32(Count);
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
		if (slotItem_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)SlotItem);
		}
		if (MoneyType != EMoneyType.EmptyValue)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)MoneyType);
		}
		if (Price != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(Price);
		}
		if (RefreshType != ERefreshType.EmptyValue)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)RefreshType);
		}
		if (Limit != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(Limit);
		}
		if (Count != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(Count);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(ShopSlotDetail other)
	{
		if (other == null)
		{
			return;
		}
		if (other.Index != 0)
		{
			Index = other.Index;
		}
		if (other.slotItem_ != null)
		{
			if (slotItem_ == null)
			{
				SlotItem = new AwardDetail();
			}
			SlotItem.MergeFrom(other.SlotItem);
		}
		if (other.MoneyType != EMoneyType.EmptyValue)
		{
			MoneyType = other.MoneyType;
		}
		if (other.Price != 0)
		{
			Price = other.Price;
		}
		if (other.RefreshType != ERefreshType.EmptyValue)
		{
			RefreshType = other.RefreshType;
		}
		if (other.Limit != 0)
		{
			Limit = other.Limit;
		}
		if (other.Count != 0)
		{
			Count = other.Count;
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
			case 18u:
				if (slotItem_ == null)
				{
					SlotItem = new AwardDetail();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)SlotItem);
				break;
			case 24u:
				MoneyType = (EMoneyType)((ParseContext)(ref input)).ReadEnum();
				break;
			case 32u:
				Price = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 40u:
				RefreshType = (ERefreshType)((ParseContext)(ref input)).ReadEnum();
				break;
			case 48u:
				Limit = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 56u:
				Count = ((ParseContext)(ref input)).ReadInt32();
				break;
			}
		}
	}
}
