using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class PayCreateOrderAck : IMessage<PayCreateOrderAck>, IMessage, IEquatable<PayCreateOrderAck>, IDeepCloneable<PayCreateOrderAck>, IBufferMessage
{
	private static readonly MessageParser<PayCreateOrderAck> _parser = new MessageParser<PayCreateOrderAck>((Func<PayCreateOrderAck>)(() => new PayCreateOrderAck()));

	private UnknownFieldSet _unknownFields;

	public const int OrderIdFieldNumber = 1;

	private string orderId_ = "";

	public const int GoodsIdFieldNumber = 2;

	private int goodsId_;

	public const int CCYFieldNumber = 3;

	private ECCYType cCY_;

	public const int PriceFieldNumber = 4;

	private float price_;

	public const int OrderTmFieldNumber = 5;

	private long orderTm_;

	public const int VerifyFieldNumber = 6;

	private string verify_ = "";

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<PayCreateOrderAck> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypePayCreateOrderAckReflection.Descriptor.MessageTypes[0];

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
	public long OrderTm
	{
		get
		{
			return orderTm_;
		}
		set
		{
			orderTm_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public string Verify
	{
		get
		{
			return verify_;
		}
		set
		{
			verify_ = ProtoPreconditions.CheckNotNull<string>(value, "value");
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public PayCreateOrderAck()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public PayCreateOrderAck(PayCreateOrderAck other)
		: this()
	{
		orderId_ = other.orderId_;
		goodsId_ = other.goodsId_;
		cCY_ = other.cCY_;
		price_ = other.price_;
		orderTm_ = other.orderTm_;
		verify_ = other.verify_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public PayCreateOrderAck Clone()
	{
		return new PayCreateOrderAck(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as PayCreateOrderAck);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(PayCreateOrderAck other)
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
		if (OrderTm != other.OrderTm)
		{
			return false;
		}
		if (Verify != other.Verify)
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
		if (OrderTm != 0L)
		{
			num ^= OrderTm.GetHashCode();
		}
		if (Verify.Length != 0)
		{
			num ^= Verify.GetHashCode();
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
		if (GoodsId != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteInt32(GoodsId);
		}
		if (CCY != ECCYType.EmptyValue)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)24);
			((WriteContext)(ref output)).WriteEnum((int)CCY);
		}
		if (Price != 0f)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)37);
			((WriteContext)(ref output)).WriteFloat(Price);
		}
		if (OrderTm != 0L)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)40);
			((WriteContext)(ref output)).WriteInt64(OrderTm);
		}
		if (Verify.Length != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)50);
			((WriteContext)(ref output)).WriteString(Verify);
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
		if (OrderTm != 0L)
		{
			num += 1 + CodedOutputStream.ComputeInt64Size(OrderTm);
		}
		if (Verify.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(Verify);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(PayCreateOrderAck other)
	{
		if (other != null)
		{
			if (other.OrderId.Length != 0)
			{
				OrderId = other.OrderId;
			}
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
			if (other.OrderTm != 0L)
			{
				OrderTm = other.OrderTm;
			}
			if (other.Verify.Length != 0)
			{
				Verify = other.Verify;
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
				GoodsId = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 24u:
				CCY = (ECCYType)((ParseContext)(ref input)).ReadEnum();
				break;
			case 37u:
				Price = ((ParseContext)(ref input)).ReadFloat();
				break;
			case 40u:
				OrderTm = ((ParseContext)(ref input)).ReadInt64();
				break;
			case 50u:
				Verify = ((ParseContext)(ref input)).ReadString();
				break;
			}
		}
	}
}
