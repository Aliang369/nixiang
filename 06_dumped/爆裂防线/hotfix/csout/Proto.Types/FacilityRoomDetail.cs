using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class FacilityRoomDetail : IMessage<FacilityRoomDetail>, IMessage, IEquatable<FacilityRoomDetail>, IDeepCloneable<FacilityRoomDetail>, IBufferMessage
{
	private static readonly MessageParser<FacilityRoomDetail> _parser = new MessageParser<FacilityRoomDetail>((Func<FacilityRoomDetail>)(() => new FacilityRoomDetail()));

	private UnknownFieldSet _unknownFields;

	public const int RoomIdFieldNumber = 1;

	private int roomId_;

	public const int RoomTypeFieldNumber = 2;

	private EFacilityRoomType roomType_;

	public const int ExplorerRoleFieldNumber = 3;

	private int explorerRole_;

	public const int ExplorerExpireFieldNumber = 4;

	private long explorerExpire_;

	public const int RoomRobotFieldNumber = 5;

	private RoleBaseInfo roomRobot_;

	public const int RoomOnlineFieldNumber = 6;

	private RoleBaseInfo roomOnline_;

	public const int RoomAwardFieldNumber = 7;

	private AwardDetail roomAward_;

	public const int RoomStateFieldNumber = 8;

	private EFacilityRoomState roomState_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<FacilityRoomDetail> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeFacilityRoomDetailReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

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
	public EFacilityRoomType RoomType
	{
		get
		{
			return roomType_;
		}
		set
		{
			roomType_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int ExplorerRole
	{
		get
		{
			return explorerRole_;
		}
		set
		{
			explorerRole_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public long ExplorerExpire
	{
		get
		{
			return explorerExpire_;
		}
		set
		{
			explorerExpire_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RoleBaseInfo RoomRobot
	{
		get
		{
			return roomRobot_;
		}
		set
		{
			roomRobot_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RoleBaseInfo RoomOnline
	{
		get
		{
			return roomOnline_;
		}
		set
		{
			roomOnline_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public AwardDetail RoomAward
	{
		get
		{
			return roomAward_;
		}
		set
		{
			roomAward_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public EFacilityRoomState RoomState
	{
		get
		{
			return roomState_;
		}
		set
		{
			roomState_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public FacilityRoomDetail()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public FacilityRoomDetail(FacilityRoomDetail other)
		: this()
	{
		roomId_ = other.roomId_;
		roomType_ = other.roomType_;
		explorerRole_ = other.explorerRole_;
		explorerExpire_ = other.explorerExpire_;
		roomRobot_ = ((other.roomRobot_ != null) ? other.roomRobot_.Clone() : null);
		roomOnline_ = ((other.roomOnline_ != null) ? other.roomOnline_.Clone() : null);
		roomAward_ = ((other.roomAward_ != null) ? other.roomAward_.Clone() : null);
		roomState_ = other.roomState_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public FacilityRoomDetail Clone()
	{
		return new FacilityRoomDetail(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as FacilityRoomDetail);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(FacilityRoomDetail other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (RoomId != other.RoomId)
		{
			return false;
		}
		if (RoomType != other.RoomType)
		{
			return false;
		}
		if (ExplorerRole != other.ExplorerRole)
		{
			return false;
		}
		if (ExplorerExpire != other.ExplorerExpire)
		{
			return false;
		}
		if (!object.Equals(RoomRobot, other.RoomRobot))
		{
			return false;
		}
		if (!object.Equals(RoomOnline, other.RoomOnline))
		{
			return false;
		}
		if (!object.Equals(RoomAward, other.RoomAward))
		{
			return false;
		}
		if (RoomState != other.RoomState)
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
		if (RoomId != 0)
		{
			num ^= RoomId.GetHashCode();
		}
		if (RoomType != EFacilityRoomType.EmptyValue)
		{
			num ^= RoomType.GetHashCode();
		}
		if (ExplorerRole != 0)
		{
			num ^= ExplorerRole.GetHashCode();
		}
		if (ExplorerExpire != 0L)
		{
			num ^= ExplorerExpire.GetHashCode();
		}
		if (roomRobot_ != null)
		{
			num ^= RoomRobot.GetHashCode();
		}
		if (roomOnline_ != null)
		{
			num ^= RoomOnline.GetHashCode();
		}
		if (roomAward_ != null)
		{
			num ^= RoomAward.GetHashCode();
		}
		if (RoomState != EFacilityRoomState.EmptyValue)
		{
			num ^= RoomState.GetHashCode();
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
		if (RoomId != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteInt32(RoomId);
		}
		if (RoomType != EFacilityRoomType.EmptyValue)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteEnum((int)RoomType);
		}
		if (ExplorerRole != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)24);
			((WriteContext)(ref output)).WriteInt32(ExplorerRole);
		}
		if (ExplorerExpire != 0L)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)32);
			((WriteContext)(ref output)).WriteInt64(ExplorerExpire);
		}
		if (roomRobot_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)42);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)RoomRobot);
		}
		if (roomOnline_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)50);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)RoomOnline);
		}
		if (roomAward_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)58);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)RoomAward);
		}
		if (RoomState != EFacilityRoomState.EmptyValue)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)64);
			((WriteContext)(ref output)).WriteEnum((int)RoomState);
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
		if (RoomId != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(RoomId);
		}
		if (RoomType != EFacilityRoomType.EmptyValue)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)RoomType);
		}
		if (ExplorerRole != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(ExplorerRole);
		}
		if (ExplorerExpire != 0L)
		{
			num += 1 + CodedOutputStream.ComputeInt64Size(ExplorerExpire);
		}
		if (roomRobot_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)RoomRobot);
		}
		if (roomOnline_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)RoomOnline);
		}
		if (roomAward_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)RoomAward);
		}
		if (RoomState != EFacilityRoomState.EmptyValue)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)RoomState);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(FacilityRoomDetail other)
	{
		if (other == null)
		{
			return;
		}
		if (other.RoomId != 0)
		{
			RoomId = other.RoomId;
		}
		if (other.RoomType != EFacilityRoomType.EmptyValue)
		{
			RoomType = other.RoomType;
		}
		if (other.ExplorerRole != 0)
		{
			ExplorerRole = other.ExplorerRole;
		}
		if (other.ExplorerExpire != 0L)
		{
			ExplorerExpire = other.ExplorerExpire;
		}
		if (other.roomRobot_ != null)
		{
			if (roomRobot_ == null)
			{
				RoomRobot = new RoleBaseInfo();
			}
			RoomRobot.MergeFrom(other.RoomRobot);
		}
		if (other.roomOnline_ != null)
		{
			if (roomOnline_ == null)
			{
				RoomOnline = new RoleBaseInfo();
			}
			RoomOnline.MergeFrom(other.RoomOnline);
		}
		if (other.roomAward_ != null)
		{
			if (roomAward_ == null)
			{
				RoomAward = new AwardDetail();
			}
			RoomAward.MergeFrom(other.RoomAward);
		}
		if (other.RoomState != EFacilityRoomState.EmptyValue)
		{
			RoomState = other.RoomState;
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
			case 8u:
				RoomId = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 16u:
				RoomType = (EFacilityRoomType)((ParseContext)(ref input)).ReadEnum();
				break;
			case 24u:
				ExplorerRole = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 32u:
				ExplorerExpire = ((ParseContext)(ref input)).ReadInt64();
				break;
			case 42u:
				if (roomRobot_ == null)
				{
					RoomRobot = new RoleBaseInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)RoomRobot);
				break;
			case 50u:
				if (roomOnline_ == null)
				{
					RoomOnline = new RoleBaseInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)RoomOnline);
				break;
			case 58u:
				if (roomAward_ == null)
				{
					RoomAward = new AwardDetail();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)RoomAward);
				break;
			case 64u:
				RoomState = (EFacilityRoomState)((ParseContext)(ref input)).ReadEnum();
				break;
			}
		}
	}
}
