using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class FacilityEventShopAck : IMessage<FacilityEventShopAck>, IMessage, IEquatable<FacilityEventShopAck>, IDeepCloneable<FacilityEventShopAck>, IBufferMessage
{
	private static readonly MessageParser<FacilityEventShopAck> _parser = new MessageParser<FacilityEventShopAck>((Func<FacilityEventShopAck>)(() => new FacilityEventShopAck()));

	private UnknownFieldSet _unknownFields;

	public const int FacilityMapInfoFieldNumber = 1;

	private FacilityMapInfo facilityMapInfo_;

	public const int RoomDetailFieldNumber = 2;

	private FacilityRoomDetail roomDetail_;

	public const int FacilityShopInfoFieldNumber = 3;

	private FacilityShopInfo facilityShopInfo_;

	public const int AwardListFieldNumber = 4;

	private static readonly FieldCodec<AwardDetail> _repeated_awardList_codec = FieldCodec.ForMessage<AwardDetail>(34u, AwardDetail.Parser);

	private readonly RepeatedField<AwardDetail> awardList_ = new RepeatedField<AwardDetail>();

	public const int DataChangeFieldNumber = 5;

	private DataChange dataChange_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<FacilityEventShopAck> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeFacilityEventShopAckReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public FacilityMapInfo FacilityMapInfo
	{
		get
		{
			return facilityMapInfo_;
		}
		set
		{
			facilityMapInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public FacilityRoomDetail RoomDetail
	{
		get
		{
			return roomDetail_;
		}
		set
		{
			roomDetail_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public FacilityShopInfo FacilityShopInfo
	{
		get
		{
			return facilityShopInfo_;
		}
		set
		{
			facilityShopInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<AwardDetail> AwardList => awardList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public DataChange DataChange
	{
		get
		{
			return dataChange_;
		}
		set
		{
			dataChange_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public FacilityEventShopAck()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public FacilityEventShopAck(FacilityEventShopAck other)
		: this()
	{
		facilityMapInfo_ = ((other.facilityMapInfo_ != null) ? other.facilityMapInfo_.Clone() : null);
		roomDetail_ = ((other.roomDetail_ != null) ? other.roomDetail_.Clone() : null);
		facilityShopInfo_ = ((other.facilityShopInfo_ != null) ? other.facilityShopInfo_.Clone() : null);
		awardList_ = other.awardList_.Clone();
		dataChange_ = ((other.dataChange_ != null) ? other.dataChange_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public FacilityEventShopAck Clone()
	{
		return new FacilityEventShopAck(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as FacilityEventShopAck);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(FacilityEventShopAck other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!object.Equals(FacilityMapInfo, other.FacilityMapInfo))
		{
			return false;
		}
		if (!object.Equals(RoomDetail, other.RoomDetail))
		{
			return false;
		}
		if (!object.Equals(FacilityShopInfo, other.FacilityShopInfo))
		{
			return false;
		}
		if (!awardList_.Equals(other.awardList_))
		{
			return false;
		}
		if (!object.Equals(DataChange, other.DataChange))
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
		if (facilityMapInfo_ != null)
		{
			num ^= FacilityMapInfo.GetHashCode();
		}
		if (roomDetail_ != null)
		{
			num ^= RoomDetail.GetHashCode();
		}
		if (facilityShopInfo_ != null)
		{
			num ^= FacilityShopInfo.GetHashCode();
		}
		num ^= ((object)awardList_).GetHashCode();
		if (dataChange_ != null)
		{
			num ^= DataChange.GetHashCode();
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
		if (facilityMapInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)10);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)FacilityMapInfo);
		}
		if (roomDetail_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)18);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)RoomDetail);
		}
		if (facilityShopInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)26);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)FacilityShopInfo);
		}
		awardList_.WriteTo(ref output, _repeated_awardList_codec);
		if (dataChange_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)42);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)DataChange);
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
		if (facilityMapInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)FacilityMapInfo);
		}
		if (roomDetail_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)RoomDetail);
		}
		if (facilityShopInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)FacilityShopInfo);
		}
		num += awardList_.CalculateSize(_repeated_awardList_codec);
		if (dataChange_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)DataChange);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(FacilityEventShopAck other)
	{
		if (other == null)
		{
			return;
		}
		if (other.facilityMapInfo_ != null)
		{
			if (facilityMapInfo_ == null)
			{
				FacilityMapInfo = new FacilityMapInfo();
			}
			FacilityMapInfo.MergeFrom(other.FacilityMapInfo);
		}
		if (other.roomDetail_ != null)
		{
			if (roomDetail_ == null)
			{
				RoomDetail = new FacilityRoomDetail();
			}
			RoomDetail.MergeFrom(other.RoomDetail);
		}
		if (other.facilityShopInfo_ != null)
		{
			if (facilityShopInfo_ == null)
			{
				FacilityShopInfo = new FacilityShopInfo();
			}
			FacilityShopInfo.MergeFrom(other.FacilityShopInfo);
		}
		awardList_.Add((IEnumerable<AwardDetail>)other.awardList_);
		if (other.dataChange_ != null)
		{
			if (dataChange_ == null)
			{
				DataChange = new DataChange();
			}
			DataChange.MergeFrom(other.DataChange);
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
			case 10u:
				if (facilityMapInfo_ == null)
				{
					FacilityMapInfo = new FacilityMapInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)FacilityMapInfo);
				break;
			case 18u:
				if (roomDetail_ == null)
				{
					RoomDetail = new FacilityRoomDetail();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)RoomDetail);
				break;
			case 26u:
				if (facilityShopInfo_ == null)
				{
					FacilityShopInfo = new FacilityShopInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)FacilityShopInfo);
				break;
			case 34u:
				awardList_.AddEntriesFrom(ref input, _repeated_awardList_codec);
				break;
			case 42u:
				if (dataChange_ == null)
				{
					DataChange = new DataChange();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)DataChange);
				break;
			}
		}
	}
}
