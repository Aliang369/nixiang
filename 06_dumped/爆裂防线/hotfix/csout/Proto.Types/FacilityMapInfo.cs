using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class FacilityMapInfo : IMessage<FacilityMapInfo>, IMessage, IEquatable<FacilityMapInfo>, IDeepCloneable<FacilityMapInfo>, IBufferMessage
{
	private static readonly MessageParser<FacilityMapInfo> _parser = new MessageParser<FacilityMapInfo>((Func<FacilityMapInfo>)(() => new FacilityMapInfo()));

	private UnknownFieldSet _unknownFields;

	public const int BaseInfoFieldNumber = 1;

	private FacilityBaseInfo baseInfo_;

	public const int RoomListFieldNumber = 2;

	private static readonly FieldCodec<FacilityRoomDetail> _repeated_roomList_codec = FieldCodec.ForMessage<FacilityRoomDetail>(18u, FacilityRoomDetail.Parser);

	private readonly RepeatedField<FacilityRoomDetail> roomList_ = new RepeatedField<FacilityRoomDetail>();

	public const int CurrentRoomFieldNumber = 3;

	private int currentRoom_;

	public const int ExploreCountFieldNumber = 4;

	private int exploreCount_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<FacilityMapInfo> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeFacilityMapInfoReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public FacilityBaseInfo BaseInfo
	{
		get
		{
			return baseInfo_;
		}
		set
		{
			baseInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<FacilityRoomDetail> RoomList => roomList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int CurrentRoom
	{
		get
		{
			return currentRoom_;
		}
		set
		{
			currentRoom_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int ExploreCount
	{
		get
		{
			return exploreCount_;
		}
		set
		{
			exploreCount_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public FacilityMapInfo()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public FacilityMapInfo(FacilityMapInfo other)
		: this()
	{
		baseInfo_ = ((other.baseInfo_ != null) ? other.baseInfo_.Clone() : null);
		roomList_ = other.roomList_.Clone();
		currentRoom_ = other.currentRoom_;
		exploreCount_ = other.exploreCount_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public FacilityMapInfo Clone()
	{
		return new FacilityMapInfo(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as FacilityMapInfo);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(FacilityMapInfo other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!object.Equals(BaseInfo, other.BaseInfo))
		{
			return false;
		}
		if (!roomList_.Equals(other.roomList_))
		{
			return false;
		}
		if (CurrentRoom != other.CurrentRoom)
		{
			return false;
		}
		if (ExploreCount != other.ExploreCount)
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
		if (baseInfo_ != null)
		{
			num ^= BaseInfo.GetHashCode();
		}
		num ^= ((object)roomList_).GetHashCode();
		if (CurrentRoom != 0)
		{
			num ^= CurrentRoom.GetHashCode();
		}
		if (ExploreCount != 0)
		{
			num ^= ExploreCount.GetHashCode();
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
		if (baseInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)10);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)BaseInfo);
		}
		roomList_.WriteTo(ref output, _repeated_roomList_codec);
		if (CurrentRoom != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)24);
			((WriteContext)(ref output)).WriteInt32(CurrentRoom);
		}
		if (ExploreCount != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)32);
			((WriteContext)(ref output)).WriteInt32(ExploreCount);
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
		if (baseInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)BaseInfo);
		}
		num += roomList_.CalculateSize(_repeated_roomList_codec);
		if (CurrentRoom != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(CurrentRoom);
		}
		if (ExploreCount != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(ExploreCount);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(FacilityMapInfo other)
	{
		if (other == null)
		{
			return;
		}
		if (other.baseInfo_ != null)
		{
			if (baseInfo_ == null)
			{
				BaseInfo = new FacilityBaseInfo();
			}
			BaseInfo.MergeFrom(other.BaseInfo);
		}
		roomList_.Add((IEnumerable<FacilityRoomDetail>)other.roomList_);
		if (other.CurrentRoom != 0)
		{
			CurrentRoom = other.CurrentRoom;
		}
		if (other.ExploreCount != 0)
		{
			ExploreCount = other.ExploreCount;
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
				if (baseInfo_ == null)
				{
					BaseInfo = new FacilityBaseInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)BaseInfo);
				break;
			case 18u:
				roomList_.AddEntriesFrom(ref input, _repeated_roomList_codec);
				break;
			case 24u:
				CurrentRoom = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 32u:
				ExploreCount = ((ParseContext)(ref input)).ReadInt32();
				break;
			}
		}
	}
}
