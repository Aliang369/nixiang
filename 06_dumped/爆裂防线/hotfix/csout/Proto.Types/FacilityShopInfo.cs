using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class FacilityShopInfo : IMessage<FacilityShopInfo>, IMessage, IEquatable<FacilityShopInfo>, IDeepCloneable<FacilityShopInfo>, IBufferMessage
{
	private static readonly MessageParser<FacilityShopInfo> _parser = new MessageParser<FacilityShopInfo>((Func<FacilityShopInfo>)(() => new FacilityShopInfo()));

	private UnknownFieldSet _unknownFields;

	public const int FacilityIdFieldNumber = 1;

	private int facilityId_;

	public const int RoomIdFieldNumber = 2;

	private int roomId_;

	public const int GoodsListFieldNumber = 3;

	private static readonly FieldCodec<FacilityGoodsDetail> _repeated_goodsList_codec = FieldCodec.ForMessage<FacilityGoodsDetail>(26u, FacilityGoodsDetail.Parser);

	private readonly RepeatedField<FacilityGoodsDetail> goodsList_ = new RepeatedField<FacilityGoodsDetail>();

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<FacilityShopInfo> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeFacilityShopInfoReflection.Descriptor.MessageTypes[0];

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
	public RepeatedField<FacilityGoodsDetail> GoodsList => goodsList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public FacilityShopInfo()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public FacilityShopInfo(FacilityShopInfo other)
		: this()
	{
		facilityId_ = other.facilityId_;
		roomId_ = other.roomId_;
		goodsList_ = other.goodsList_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public FacilityShopInfo Clone()
	{
		return new FacilityShopInfo(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as FacilityShopInfo);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(FacilityShopInfo other)
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
		if (!goodsList_.Equals(other.goodsList_))
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
		num ^= ((object)goodsList_).GetHashCode();
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
		goodsList_.WriteTo(ref output, _repeated_goodsList_codec);
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
		num += goodsList_.CalculateSize(_repeated_goodsList_codec);
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(FacilityShopInfo other)
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
			goodsList_.Add((IEnumerable<FacilityGoodsDetail>)other.goodsList_);
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
			case 26u:
				goodsList_.AddEntriesFrom(ref input, _repeated_goodsList_codec);
				break;
			}
		}
	}
}
