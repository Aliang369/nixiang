using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class FacilityRoomPreviewAck : IMessage<FacilityRoomPreviewAck>, IMessage, IEquatable<FacilityRoomPreviewAck>, IDeepCloneable<FacilityRoomPreviewAck>, IBufferMessage
{
	private static readonly MessageParser<FacilityRoomPreviewAck> _parser = new MessageParser<FacilityRoomPreviewAck>((Func<FacilityRoomPreviewAck>)(() => new FacilityRoomPreviewAck()));

	private UnknownFieldSet _unknownFields;

	public const int RoomDetailFieldNumber = 1;

	private FacilityRoomDetail roomDetail_;

	public const int RobotBaseInfoFieldNumber = 2;

	private RoleBaseInfo robotBaseInfo_;

	public const int ShopLeftInfoFieldNumber = 3;

	private FacilityShopLeftInfo shopLeftInfo_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<FacilityRoomPreviewAck> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeFacilityRoomPreviewAckReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

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
	public RoleBaseInfo RobotBaseInfo
	{
		get
		{
			return robotBaseInfo_;
		}
		set
		{
			robotBaseInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public FacilityShopLeftInfo ShopLeftInfo
	{
		get
		{
			return shopLeftInfo_;
		}
		set
		{
			shopLeftInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public FacilityRoomPreviewAck()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public FacilityRoomPreviewAck(FacilityRoomPreviewAck other)
		: this()
	{
		roomDetail_ = ((other.roomDetail_ != null) ? other.roomDetail_.Clone() : null);
		robotBaseInfo_ = ((other.robotBaseInfo_ != null) ? other.robotBaseInfo_.Clone() : null);
		shopLeftInfo_ = ((other.shopLeftInfo_ != null) ? other.shopLeftInfo_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public FacilityRoomPreviewAck Clone()
	{
		return new FacilityRoomPreviewAck(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as FacilityRoomPreviewAck);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(FacilityRoomPreviewAck other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!object.Equals(RoomDetail, other.RoomDetail))
		{
			return false;
		}
		if (!object.Equals(RobotBaseInfo, other.RobotBaseInfo))
		{
			return false;
		}
		if (!object.Equals(ShopLeftInfo, other.ShopLeftInfo))
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
		if (roomDetail_ != null)
		{
			num ^= RoomDetail.GetHashCode();
		}
		if (robotBaseInfo_ != null)
		{
			num ^= RobotBaseInfo.GetHashCode();
		}
		if (shopLeftInfo_ != null)
		{
			num ^= ShopLeftInfo.GetHashCode();
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
		if (roomDetail_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)10);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)RoomDetail);
		}
		if (robotBaseInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)18);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)RobotBaseInfo);
		}
		if (shopLeftInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)26);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)ShopLeftInfo);
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
		if (roomDetail_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)RoomDetail);
		}
		if (robotBaseInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)RobotBaseInfo);
		}
		if (shopLeftInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)ShopLeftInfo);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(FacilityRoomPreviewAck other)
	{
		if (other == null)
		{
			return;
		}
		if (other.roomDetail_ != null)
		{
			if (roomDetail_ == null)
			{
				RoomDetail = new FacilityRoomDetail();
			}
			RoomDetail.MergeFrom(other.RoomDetail);
		}
		if (other.robotBaseInfo_ != null)
		{
			if (robotBaseInfo_ == null)
			{
				RobotBaseInfo = new RoleBaseInfo();
			}
			RobotBaseInfo.MergeFrom(other.RobotBaseInfo);
		}
		if (other.shopLeftInfo_ != null)
		{
			if (shopLeftInfo_ == null)
			{
				ShopLeftInfo = new FacilityShopLeftInfo();
			}
			ShopLeftInfo.MergeFrom(other.ShopLeftInfo);
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
				if (roomDetail_ == null)
				{
					RoomDetail = new FacilityRoomDetail();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)RoomDetail);
				break;
			case 18u:
				if (robotBaseInfo_ == null)
				{
					RobotBaseInfo = new RoleBaseInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)RobotBaseInfo);
				break;
			case 26u:
				if (shopLeftInfo_ == null)
				{
					ShopLeftInfo = new FacilityShopLeftInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)ShopLeftInfo);
				break;
			}
		}
	}
}
