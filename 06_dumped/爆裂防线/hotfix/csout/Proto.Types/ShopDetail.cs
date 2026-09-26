using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class ShopDetail : IMessage<ShopDetail>, IMessage, IEquatable<ShopDetail>, IDeepCloneable<ShopDetail>, IBufferMessage
{
	private static readonly MessageParser<ShopDetail> _parser = new MessageParser<ShopDetail>((Func<ShopDetail>)(() => new ShopDetail()));

	private UnknownFieldSet _unknownFields;

	public const int ShopTypeFieldNumber = 1;

	private EShopType shopType_;

	public const int SlotListFieldNumber = 2;

	private static readonly FieldCodec<ShopSlotDetail> _repeated_slotList_codec = FieldCodec.ForMessage<ShopSlotDetail>(18u, ShopSlotDetail.Parser);

	private readonly RepeatedField<ShopSlotDetail> slotList_ = new RepeatedField<ShopSlotDetail>();

	public const int VipLevelFieldNumber = 3;

	private int vipLevel_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<ShopDetail> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeShopDetailReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public EShopType ShopType
	{
		get
		{
			return shopType_;
		}
		set
		{
			shopType_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<ShopSlotDetail> SlotList => slotList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int VipLevel
	{
		get
		{
			return vipLevel_;
		}
		set
		{
			vipLevel_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ShopDetail()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ShopDetail(ShopDetail other)
		: this()
	{
		shopType_ = other.shopType_;
		slotList_ = other.slotList_.Clone();
		vipLevel_ = other.vipLevel_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ShopDetail Clone()
	{
		return new ShopDetail(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as ShopDetail);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(ShopDetail other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (ShopType != other.ShopType)
		{
			return false;
		}
		if (!slotList_.Equals(other.slotList_))
		{
			return false;
		}
		if (VipLevel != other.VipLevel)
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
		if (ShopType != EShopType.EmptyValue)
		{
			num ^= ShopType.GetHashCode();
		}
		num ^= ((object)slotList_).GetHashCode();
		if (VipLevel != 0)
		{
			num ^= VipLevel.GetHashCode();
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
		if (ShopType != EShopType.EmptyValue)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteEnum((int)ShopType);
		}
		slotList_.WriteTo(ref output, _repeated_slotList_codec);
		if (VipLevel != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)24);
			((WriteContext)(ref output)).WriteInt32(VipLevel);
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
		if (ShopType != EShopType.EmptyValue)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)ShopType);
		}
		num += slotList_.CalculateSize(_repeated_slotList_codec);
		if (VipLevel != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(VipLevel);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(ShopDetail other)
	{
		if (other != null)
		{
			if (other.ShopType != EShopType.EmptyValue)
			{
				ShopType = other.ShopType;
			}
			slotList_.Add((IEnumerable<ShopSlotDetail>)other.slotList_);
			if (other.VipLevel != 0)
			{
				VipLevel = other.VipLevel;
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
				ShopType = (EShopType)((ParseContext)(ref input)).ReadEnum();
				break;
			case 18u:
				slotList_.AddEntriesFrom(ref input, _repeated_slotList_codec);
				break;
			case 24u:
				VipLevel = ((ParseContext)(ref input)).ReadInt32();
				break;
			}
		}
	}
}
