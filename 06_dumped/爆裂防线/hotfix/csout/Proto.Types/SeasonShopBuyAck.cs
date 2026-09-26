using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class SeasonShopBuyAck : IMessage<SeasonShopBuyAck>, IMessage, IEquatable<SeasonShopBuyAck>, IDeepCloneable<SeasonShopBuyAck>, IBufferMessage
{
	private static readonly MessageParser<SeasonShopBuyAck> _parser = new MessageParser<SeasonShopBuyAck>((Func<SeasonShopBuyAck>)(() => new SeasonShopBuyAck()));

	private UnknownFieldSet _unknownFields;

	public const int ShopIdFieldNumber = 1;

	private int shopId_;

	public const int CountFieldNumber = 2;

	private int count_;

	public const int MeritFieldNumber = 3;

	private int merit_;

	public const int MoneyTypeFieldNumber = 4;

	private int moneyType_;

	public const int MoneyLeftFieldNumber = 5;

	private int moneyLeft_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<SeasonShopBuyAck> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeSeasonShopBuyAckReflection.Descriptor.MessageTypes[0];

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
	public int Merit
	{
		get
		{
			return merit_;
		}
		set
		{
			merit_ = value;
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
	public int MoneyLeft
	{
		get
		{
			return moneyLeft_;
		}
		set
		{
			moneyLeft_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SeasonShopBuyAck()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SeasonShopBuyAck(SeasonShopBuyAck other)
		: this()
	{
		shopId_ = other.shopId_;
		count_ = other.count_;
		merit_ = other.merit_;
		moneyType_ = other.moneyType_;
		moneyLeft_ = other.moneyLeft_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SeasonShopBuyAck Clone()
	{
		return new SeasonShopBuyAck(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as SeasonShopBuyAck);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(SeasonShopBuyAck other)
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
		if (Count != other.Count)
		{
			return false;
		}
		if (Merit != other.Merit)
		{
			return false;
		}
		if (MoneyType != other.MoneyType)
		{
			return false;
		}
		if (MoneyLeft != other.MoneyLeft)
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
		if (Count != 0)
		{
			num ^= Count.GetHashCode();
		}
		if (Merit != 0)
		{
			num ^= Merit.GetHashCode();
		}
		if (MoneyType != 0)
		{
			num ^= MoneyType.GetHashCode();
		}
		if (MoneyLeft != 0)
		{
			num ^= MoneyLeft.GetHashCode();
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
		if (Count != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteInt32(Count);
		}
		if (Merit != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)24);
			((WriteContext)(ref output)).WriteInt32(Merit);
		}
		if (MoneyType != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)32);
			((WriteContext)(ref output)).WriteInt32(MoneyType);
		}
		if (MoneyLeft != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)40);
			((WriteContext)(ref output)).WriteInt32(MoneyLeft);
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
		if (Count != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(Count);
		}
		if (Merit != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(Merit);
		}
		if (MoneyType != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(MoneyType);
		}
		if (MoneyLeft != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(MoneyLeft);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(SeasonShopBuyAck other)
	{
		if (other != null)
		{
			if (other.ShopId != 0)
			{
				ShopId = other.ShopId;
			}
			if (other.Count != 0)
			{
				Count = other.Count;
			}
			if (other.Merit != 0)
			{
				Merit = other.Merit;
			}
			if (other.MoneyType != 0)
			{
				MoneyType = other.MoneyType;
			}
			if (other.MoneyLeft != 0)
			{
				MoneyLeft = other.MoneyLeft;
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
				Count = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 24u:
				Merit = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 32u:
				MoneyType = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 40u:
				MoneyLeft = ((ParseContext)(ref input)).ReadInt32();
				break;
			}
		}
	}
}
