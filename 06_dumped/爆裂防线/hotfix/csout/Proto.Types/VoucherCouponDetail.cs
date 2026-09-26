using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class VoucherCouponDetail : IMessage<VoucherCouponDetail>, IMessage, IEquatable<VoucherCouponDetail>, IDeepCloneable<VoucherCouponDetail>, IBufferMessage
{
	private static readonly MessageParser<VoucherCouponDetail> _parser = new MessageParser<VoucherCouponDetail>((Func<VoucherCouponDetail>)(() => new VoucherCouponDetail()));

	private UnknownFieldSet _unknownFields;

	public const int UUIDFieldNumber = 1;

	private string uUID_ = "";

	public const int CouponIdFieldNumber = 2;

	private int couponId_;

	public const int CouponTypeFieldNumber = 3;

	private EVoucherCouponType couponType_;

	public const int DeductionMinFieldNumber = 4;

	private int deductionMin_;

	public const int DeductionValueFieldNumber = 5;

	private int deductionValue_;

	public const int DiscountMaxFieldNumber = 6;

	private int discountMax_;

	public const int DiscountValueFieldNumber = 7;

	private int discountValue_;

	public const int ExpireTmFieldNumber = 8;

	private long expireTm_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<VoucherCouponDetail> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeVoucherCouponDetailReflection.Descriptor.MessageTypes[0];

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
	public int CouponId
	{
		get
		{
			return couponId_;
		}
		set
		{
			couponId_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public EVoucherCouponType CouponType
	{
		get
		{
			return couponType_;
		}
		set
		{
			couponType_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int DeductionMin
	{
		get
		{
			return deductionMin_;
		}
		set
		{
			deductionMin_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int DeductionValue
	{
		get
		{
			return deductionValue_;
		}
		set
		{
			deductionValue_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int DiscountMax
	{
		get
		{
			return discountMax_;
		}
		set
		{
			discountMax_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int DiscountValue
	{
		get
		{
			return discountValue_;
		}
		set
		{
			discountValue_ = value;
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
	public VoucherCouponDetail()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public VoucherCouponDetail(VoucherCouponDetail other)
		: this()
	{
		uUID_ = other.uUID_;
		couponId_ = other.couponId_;
		couponType_ = other.couponType_;
		deductionMin_ = other.deductionMin_;
		deductionValue_ = other.deductionValue_;
		discountMax_ = other.discountMax_;
		discountValue_ = other.discountValue_;
		expireTm_ = other.expireTm_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public VoucherCouponDetail Clone()
	{
		return new VoucherCouponDetail(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as VoucherCouponDetail);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(VoucherCouponDetail other)
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
		if (CouponId != other.CouponId)
		{
			return false;
		}
		if (CouponType != other.CouponType)
		{
			return false;
		}
		if (DeductionMin != other.DeductionMin)
		{
			return false;
		}
		if (DeductionValue != other.DeductionValue)
		{
			return false;
		}
		if (DiscountMax != other.DiscountMax)
		{
			return false;
		}
		if (DiscountValue != other.DiscountValue)
		{
			return false;
		}
		if (ExpireTm != other.ExpireTm)
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
		if (CouponId != 0)
		{
			num ^= CouponId.GetHashCode();
		}
		if (CouponType != EVoucherCouponType.EmptyValue)
		{
			num ^= CouponType.GetHashCode();
		}
		if (DeductionMin != 0)
		{
			num ^= DeductionMin.GetHashCode();
		}
		if (DeductionValue != 0)
		{
			num ^= DeductionValue.GetHashCode();
		}
		if (DiscountMax != 0)
		{
			num ^= DiscountMax.GetHashCode();
		}
		if (DiscountValue != 0)
		{
			num ^= DiscountValue.GetHashCode();
		}
		if (ExpireTm != 0L)
		{
			num ^= ExpireTm.GetHashCode();
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
		if (CouponId != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteInt32(CouponId);
		}
		if (CouponType != EVoucherCouponType.EmptyValue)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)24);
			((WriteContext)(ref output)).WriteEnum((int)CouponType);
		}
		if (DeductionMin != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)32);
			((WriteContext)(ref output)).WriteInt32(DeductionMin);
		}
		if (DeductionValue != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)40);
			((WriteContext)(ref output)).WriteInt32(DeductionValue);
		}
		if (DiscountMax != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)48);
			((WriteContext)(ref output)).WriteInt32(DiscountMax);
		}
		if (DiscountValue != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)56);
			((WriteContext)(ref output)).WriteInt32(DiscountValue);
		}
		if (ExpireTm != 0L)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)64);
			((WriteContext)(ref output)).WriteInt64(ExpireTm);
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
		if (CouponId != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(CouponId);
		}
		if (CouponType != EVoucherCouponType.EmptyValue)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)CouponType);
		}
		if (DeductionMin != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(DeductionMin);
		}
		if (DeductionValue != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(DeductionValue);
		}
		if (DiscountMax != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(DiscountMax);
		}
		if (DiscountValue != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(DiscountValue);
		}
		if (ExpireTm != 0L)
		{
			num += 1 + CodedOutputStream.ComputeInt64Size(ExpireTm);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(VoucherCouponDetail other)
	{
		if (other != null)
		{
			if (other.UUID.Length != 0)
			{
				UUID = other.UUID;
			}
			if (other.CouponId != 0)
			{
				CouponId = other.CouponId;
			}
			if (other.CouponType != EVoucherCouponType.EmptyValue)
			{
				CouponType = other.CouponType;
			}
			if (other.DeductionMin != 0)
			{
				DeductionMin = other.DeductionMin;
			}
			if (other.DeductionValue != 0)
			{
				DeductionValue = other.DeductionValue;
			}
			if (other.DiscountMax != 0)
			{
				DiscountMax = other.DiscountMax;
			}
			if (other.DiscountValue != 0)
			{
				DiscountValue = other.DiscountValue;
			}
			if (other.ExpireTm != 0L)
			{
				ExpireTm = other.ExpireTm;
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
				CouponId = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 24u:
				CouponType = (EVoucherCouponType)((ParseContext)(ref input)).ReadEnum();
				break;
			case 32u:
				DeductionMin = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 40u:
				DeductionValue = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 48u:
				DiscountMax = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 56u:
				DiscountValue = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 64u:
				ExpireTm = ((ParseContext)(ref input)).ReadInt64();
				break;
			}
		}
	}
}
