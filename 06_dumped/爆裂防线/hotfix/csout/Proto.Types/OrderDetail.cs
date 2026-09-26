using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class OrderDetail : IMessage<OrderDetail>, IMessage, IEquatable<OrderDetail>, IDeepCloneable<OrderDetail>, IBufferMessage
{
	private static readonly MessageParser<OrderDetail> _parser = new MessageParser<OrderDetail>((Func<OrderDetail>)(() => new OrderDetail()));

	private UnknownFieldSet _unknownFields;

	public const int OrderIdFieldNumber = 1;

	private string orderId_ = "";

	public const int AppChannelFieldNumber = 2;

	private EChannelType appChannel_;

	public const int CCYFieldNumber = 3;

	private ECCYType cCY_;

	public const int BuyCountFieldNumber = 4;

	private int buyCount_;

	public const int AmountFieldNumber = 5;

	private float amount_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<OrderDetail> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeOrderDetailReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public string OrderId
	{
		get
		{
			return orderId_;
		}
		set
		{
			orderId_ = ProtoPreconditions.CheckNotNull<string>(value, "value");
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public EChannelType AppChannel
	{
		get
		{
			return appChannel_;
		}
		set
		{
			appChannel_ = value;
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
	public int BuyCount
	{
		get
		{
			return buyCount_;
		}
		set
		{
			buyCount_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public float Amount
	{
		get
		{
			return amount_;
		}
		set
		{
			amount_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public OrderDetail()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public OrderDetail(OrderDetail other)
		: this()
	{
		orderId_ = other.orderId_;
		appChannel_ = other.appChannel_;
		cCY_ = other.cCY_;
		buyCount_ = other.buyCount_;
		amount_ = other.amount_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public OrderDetail Clone()
	{
		return new OrderDetail(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as OrderDetail);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(OrderDetail other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (OrderId != other.OrderId)
		{
			return false;
		}
		if (AppChannel != other.AppChannel)
		{
			return false;
		}
		if (CCY != other.CCY)
		{
			return false;
		}
		if (BuyCount != other.BuyCount)
		{
			return false;
		}
		if (!ProtobufEqualityComparers.BitwiseSingleEqualityComparer.Equals(Amount, other.Amount))
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
		if (OrderId.Length != 0)
		{
			num ^= OrderId.GetHashCode();
		}
		if (AppChannel != EChannelType.EmptyValue)
		{
			num ^= AppChannel.GetHashCode();
		}
		if (CCY != ECCYType.EmptyValue)
		{
			num ^= CCY.GetHashCode();
		}
		if (BuyCount != 0)
		{
			num ^= BuyCount.GetHashCode();
		}
		if (Amount != 0f)
		{
			num ^= ProtobufEqualityComparers.BitwiseSingleEqualityComparer.GetHashCode(Amount);
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
		if (OrderId.Length != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)10);
			((WriteContext)(ref output)).WriteString(OrderId);
		}
		if (AppChannel != EChannelType.EmptyValue)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteEnum((int)AppChannel);
		}
		if (CCY != ECCYType.EmptyValue)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)24);
			((WriteContext)(ref output)).WriteEnum((int)CCY);
		}
		if (BuyCount != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)32);
			((WriteContext)(ref output)).WriteInt32(BuyCount);
		}
		if (Amount != 0f)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)45);
			((WriteContext)(ref output)).WriteFloat(Amount);
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
		if (OrderId.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(OrderId);
		}
		if (AppChannel != EChannelType.EmptyValue)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)AppChannel);
		}
		if (CCY != ECCYType.EmptyValue)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)CCY);
		}
		if (BuyCount != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(BuyCount);
		}
		if (Amount != 0f)
		{
			num += 5;
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(OrderDetail other)
	{
		if (other != null)
		{
			if (other.OrderId.Length != 0)
			{
				OrderId = other.OrderId;
			}
			if (other.AppChannel != EChannelType.EmptyValue)
			{
				AppChannel = other.AppChannel;
			}
			if (other.CCY != ECCYType.EmptyValue)
			{
				CCY = other.CCY;
			}
			if (other.BuyCount != 0)
			{
				BuyCount = other.BuyCount;
			}
			if (other.Amount != 0f)
			{
				Amount = other.Amount;
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
			case 10u:
				OrderId = ((ParseContext)(ref input)).ReadString();
				break;
			case 16u:
				AppChannel = (EChannelType)((ParseContext)(ref input)).ReadEnum();
				break;
			case 24u:
				CCY = (ECCYType)((ParseContext)(ref input)).ReadEnum();
				break;
			case 32u:
				BuyCount = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 45u:
				Amount = ((ParseContext)(ref input)).ReadFloat();
				break;
			}
		}
	}
}
