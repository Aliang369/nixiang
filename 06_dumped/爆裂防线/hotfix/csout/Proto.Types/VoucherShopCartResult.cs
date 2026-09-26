using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class VoucherShopCartResult : IMessage<VoucherShopCartResult>, IMessage, IEquatable<VoucherShopCartResult>, IDeepCloneable<VoucherShopCartResult>, IBufferMessage
{
	private static readonly MessageParser<VoucherShopCartResult> _parser = new MessageParser<VoucherShopCartResult>((Func<VoucherShopCartResult>)(() => new VoucherShopCartResult()));

	private UnknownFieldSet _unknownFields;

	public const int UUIDFieldNumber = 1;

	private string uUID_ = "";

	public const int CouponOffFieldNumber = 2;

	private int couponOff_;

	public const int TotalOffFieldNumber = 3;

	private int totalOff_;

	public const int TotalPriceFieldNumber = 4;

	private int totalPrice_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<VoucherShopCartResult> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeVoucherShopCartResultReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public string UUID
	{
		get
		{
			return uUID_;
		}
		set
		{
			uUID_ = ProtoPreconditions.CheckNotNull<string>(value, "value");
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int CouponOff
	{
		get
		{
			return couponOff_;
		}
		set
		{
			couponOff_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int TotalOff
	{
		get
		{
			return totalOff_;
		}
		set
		{
			totalOff_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int TotalPrice
	{
		get
		{
			return totalPrice_;
		}
		set
		{
			totalPrice_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public VoucherShopCartResult()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public VoucherShopCartResult(VoucherShopCartResult other)
		: this()
	{
		uUID_ = other.uUID_;
		couponOff_ = other.couponOff_;
		totalOff_ = other.totalOff_;
		totalPrice_ = other.totalPrice_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public VoucherShopCartResult Clone()
	{
		return new VoucherShopCartResult(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as VoucherShopCartResult);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(VoucherShopCartResult other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (UUID != other.UUID)
		{
			return false;
		}
		if (CouponOff != other.CouponOff)
		{
			return false;
		}
		if (TotalOff != other.TotalOff)
		{
			return false;
		}
		if (TotalPrice != other.TotalPrice)
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
		if (UUID.Length != 0)
		{
			num ^= UUID.GetHashCode();
		}
		if (CouponOff != 0)
		{
			num ^= CouponOff.GetHashCode();
		}
		if (TotalOff != 0)
		{
			num ^= TotalOff.GetHashCode();
		}
		if (TotalPrice != 0)
		{
			num ^= TotalPrice.GetHashCode();
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
		if (UUID.Length != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)10);
			((WriteContext)(ref output)).WriteString(UUID);
		}
		if (CouponOff != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteInt32(CouponOff);
		}
		if (TotalOff != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)24);
			((WriteContext)(ref output)).WriteInt32(TotalOff);
		}
		if (TotalPrice != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)32);
			((WriteContext)(ref output)).WriteInt32(TotalPrice);
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
		if (UUID.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(UUID);
		}
		if (CouponOff != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(CouponOff);
		}
		if (TotalOff != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(TotalOff);
		}
		if (TotalPrice != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(TotalPrice);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(VoucherShopCartResult other)
	{
		if (other != null)
		{
			if (other.UUID.Length != 0)
			{
				UUID = other.UUID;
			}
			if (other.CouponOff != 0)
			{
				CouponOff = other.CouponOff;
			}
			if (other.TotalOff != 0)
			{
				TotalOff = other.TotalOff;
			}
			if (other.TotalPrice != 0)
			{
				TotalPrice = other.TotalPrice;
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
				UUID = ((ParseContext)(ref input)).ReadString();
				break;
			case 16u:
				CouponOff = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 24u:
				TotalOff = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 32u:
				TotalPrice = ((ParseContext)(ref input)).ReadInt32();
				break;
			}
		}
	}
}
