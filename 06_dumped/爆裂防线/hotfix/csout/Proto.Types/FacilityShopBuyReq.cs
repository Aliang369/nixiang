using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class FacilityShopBuyReq : IMessage<FacilityShopBuyReq>, IMessage, IEquatable<FacilityShopBuyReq>, IDeepCloneable<FacilityShopBuyReq>, IBufferMessage
{
	private static readonly MessageParser<FacilityShopBuyReq> _parser = new MessageParser<FacilityShopBuyReq>((Func<FacilityShopBuyReq>)(() => new FacilityShopBuyReq()));

	private UnknownFieldSet _unknownFields;

	public const int FacilityIdFieldNumber = 1;

	private int facilityId_;

	public const int RoomIdFieldNumber = 2;

	private int roomId_;

	public const int ShopSlotFieldNumber = 3;

	private int shopSlot_;

	public const int BuyCountFieldNumber = 4;

	private int buyCount_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<FacilityShopBuyReq> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeFacilityShopBuyReqReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int FacilityId
	{
		get
		{
			return facilityId_;
		}
		set
		{
			facilityId_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int RoomId
	{
		get
		{
			return roomId_;
		}
		set
		{
			roomId_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int ShopSlot
	{
		get
		{
			return shopSlot_;
		}
		set
		{
			shopSlot_ = value;
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
	public FacilityShopBuyReq()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public FacilityShopBuyReq(FacilityShopBuyReq other)
		: this()
	{
		facilityId_ = other.facilityId_;
		roomId_ = other.roomId_;
		shopSlot_ = other.shopSlot_;
		buyCount_ = other.buyCount_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public FacilityShopBuyReq Clone()
	{
		return new FacilityShopBuyReq(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as FacilityShopBuyReq);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(FacilityShopBuyReq other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (FacilityId != other.FacilityId)
		{
			return false;
		}
		if (RoomId != other.RoomId)
		{
			return false;
		}
		if (ShopSlot != other.ShopSlot)
		{
			return false;
		}
		if (BuyCount != other.BuyCount)
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
		if (FacilityId != 0)
		{
			num ^= FacilityId.GetHashCode();
		}
		if (RoomId != 0)
		{
			num ^= RoomId.GetHashCode();
		}
		if (ShopSlot != 0)
		{
			num ^= ShopSlot.GetHashCode();
		}
		if (BuyCount != 0)
		{
			num ^= BuyCount.GetHashCode();
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
		if (FacilityId != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteInt32(FacilityId);
		}
		if (RoomId != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteInt32(RoomId);
		}
		if (ShopSlot != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)24);
			((WriteContext)(ref output)).WriteInt32(ShopSlot);
		}
		if (BuyCount != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)32);
			((WriteContext)(ref output)).WriteInt32(BuyCount);
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
		if (FacilityId != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(FacilityId);
		}
		if (RoomId != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(RoomId);
		}
		if (ShopSlot != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(ShopSlot);
		}
		if (BuyCount != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(BuyCount);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(FacilityShopBuyReq other)
	{
		if (other != null)
		{
			if (other.FacilityId != 0)
			{
				FacilityId = other.FacilityId;
			}
			if (other.RoomId != 0)
			{
				RoomId = other.RoomId;
			}
			if (other.ShopSlot != 0)
			{
				ShopSlot = other.ShopSlot;
			}
			if (other.BuyCount != 0)
			{
				BuyCount = other.BuyCount;
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
				FacilityId = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 16u:
				RoomId = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 24u:
				ShopSlot = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 32u:
				BuyCount = ((ParseContext)(ref input)).ReadInt32();
				break;
			}
		}
	}
}
