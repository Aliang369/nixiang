using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class SeasonMoveDetail : IMessage<SeasonMoveDetail>, IMessage, IEquatable<SeasonMoveDetail>, IDeepCloneable<SeasonMoveDetail>, IBufferMessage
{
	private static readonly MessageParser<SeasonMoveDetail> _parser = new MessageParser<SeasonMoveDetail>((Func<SeasonMoveDetail>)(() => new SeasonMoveDetail()));

	private UnknownFieldSet _unknownFields;

	public const int RoleBaseInfoFieldNumber = 1;

	private RoleBaseInfo roleBaseInfo_;

	public const int ForceIdFieldNumber = 2;

	private ESeasonForce forceId_;

	public const int TeamIdFieldNumber = 3;

	private int teamId_;

	public const int FromXFieldNumber = 4;

	private float fromX_;

	public const int FromYFieldNumber = 5;

	private float fromY_;

	public const int ToXFieldNumber = 6;

	private float toX_;

	public const int ToYFieldNumber = 7;

	private float toY_;

	public const int StartTmFieldNumber = 8;

	private long startTm_;

	public const int ArrivalTmFieldNumber = 9;

	private long arrivalTm_;

	public const int MoveTypeFieldNumber = 10;

	private int moveType_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<SeasonMoveDetail> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeSeasonMoveDetailReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RoleBaseInfo RoleBaseInfo
	{
		get
		{
			return roleBaseInfo_;
		}
		set
		{
			roleBaseInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ESeasonForce ForceId
	{
		get
		{
			return forceId_;
		}
		set
		{
			forceId_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int TeamId
	{
		get
		{
			return teamId_;
		}
		set
		{
			teamId_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public float FromX
	{
		get
		{
			return fromX_;
		}
		set
		{
			fromX_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public float FromY
	{
		get
		{
			return fromY_;
		}
		set
		{
			fromY_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public float ToX
	{
		get
		{
			return toX_;
		}
		set
		{
			toX_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public float ToY
	{
		get
		{
			return toY_;
		}
		set
		{
			toY_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public long StartTm
	{
		get
		{
			return startTm_;
		}
		set
		{
			startTm_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public long ArrivalTm
	{
		get
		{
			return arrivalTm_;
		}
		set
		{
			arrivalTm_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int MoveType
	{
		get
		{
			return moveType_;
		}
		set
		{
			moveType_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SeasonMoveDetail()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SeasonMoveDetail(SeasonMoveDetail other)
		: this()
	{
		roleBaseInfo_ = ((other.roleBaseInfo_ != null) ? other.roleBaseInfo_.Clone() : null);
		forceId_ = other.forceId_;
		teamId_ = other.teamId_;
		fromX_ = other.fromX_;
		fromY_ = other.fromY_;
		toX_ = other.toX_;
		toY_ = other.toY_;
		startTm_ = other.startTm_;
		arrivalTm_ = other.arrivalTm_;
		moveType_ = other.moveType_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SeasonMoveDetail Clone()
	{
		return new SeasonMoveDetail(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as SeasonMoveDetail);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(SeasonMoveDetail other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!object.Equals(RoleBaseInfo, other.RoleBaseInfo))
		{
			return false;
		}
		if (ForceId != other.ForceId)
		{
			return false;
		}
		if (TeamId != other.TeamId)
		{
			return false;
		}
		if (!ProtobufEqualityComparers.BitwiseSingleEqualityComparer.Equals(FromX, other.FromX))
		{
			return false;
		}
		if (!ProtobufEqualityComparers.BitwiseSingleEqualityComparer.Equals(FromY, other.FromY))
		{
			return false;
		}
		if (!ProtobufEqualityComparers.BitwiseSingleEqualityComparer.Equals(ToX, other.ToX))
		{
			return false;
		}
		if (!ProtobufEqualityComparers.BitwiseSingleEqualityComparer.Equals(ToY, other.ToY))
		{
			return false;
		}
		if (StartTm != other.StartTm)
		{
			return false;
		}
		if (ArrivalTm != other.ArrivalTm)
		{
			return false;
		}
		if (MoveType != other.MoveType)
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
		if (roleBaseInfo_ != null)
		{
			num ^= RoleBaseInfo.GetHashCode();
		}
		if (ForceId != ESeasonForce.ForceNeutral)
		{
			num ^= ForceId.GetHashCode();
		}
		if (TeamId != 0)
		{
			num ^= TeamId.GetHashCode();
		}
		if (FromX != 0f)
		{
			num ^= ProtobufEqualityComparers.BitwiseSingleEqualityComparer.GetHashCode(FromX);
		}
		if (FromY != 0f)
		{
			num ^= ProtobufEqualityComparers.BitwiseSingleEqualityComparer.GetHashCode(FromY);
		}
		if (ToX != 0f)
		{
			num ^= ProtobufEqualityComparers.BitwiseSingleEqualityComparer.GetHashCode(ToX);
		}
		if (ToY != 0f)
		{
			num ^= ProtobufEqualityComparers.BitwiseSingleEqualityComparer.GetHashCode(ToY);
		}
		if (StartTm != 0L)
		{
			num ^= StartTm.GetHashCode();
		}
		if (ArrivalTm != 0L)
		{
			num ^= ArrivalTm.GetHashCode();
		}
		if (MoveType != 0)
		{
			num ^= MoveType.GetHashCode();
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
		if (roleBaseInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)10);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)RoleBaseInfo);
		}
		if (ForceId != ESeasonForce.ForceNeutral)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteEnum((int)ForceId);
		}
		if (TeamId != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)24);
			((WriteContext)(ref output)).WriteInt32(TeamId);
		}
		if (FromX != 0f)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)37);
			((WriteContext)(ref output)).WriteFloat(FromX);
		}
		if (FromY != 0f)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)45);
			((WriteContext)(ref output)).WriteFloat(FromY);
		}
		if (ToX != 0f)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)53);
			((WriteContext)(ref output)).WriteFloat(ToX);
		}
		if (ToY != 0f)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)61);
			((WriteContext)(ref output)).WriteFloat(ToY);
		}
		if (StartTm != 0L)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)64);
			((WriteContext)(ref output)).WriteInt64(StartTm);
		}
		if (ArrivalTm != 0L)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)72);
			((WriteContext)(ref output)).WriteInt64(ArrivalTm);
		}
		if (MoveType != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)80);
			((WriteContext)(ref output)).WriteInt32(MoveType);
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
		if (roleBaseInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)RoleBaseInfo);
		}
		if (ForceId != ESeasonForce.ForceNeutral)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)ForceId);
		}
		if (TeamId != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(TeamId);
		}
		if (FromX != 0f)
		{
			num += 5;
		}
		if (FromY != 0f)
		{
			num += 5;
		}
		if (ToX != 0f)
		{
			num += 5;
		}
		if (ToY != 0f)
		{
			num += 5;
		}
		if (StartTm != 0L)
		{
			num += 1 + CodedOutputStream.ComputeInt64Size(StartTm);
		}
		if (ArrivalTm != 0L)
		{
			num += 1 + CodedOutputStream.ComputeInt64Size(ArrivalTm);
		}
		if (MoveType != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(MoveType);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(SeasonMoveDetail other)
	{
		if (other == null)
		{
			return;
		}
		if (other.roleBaseInfo_ != null)
		{
			if (roleBaseInfo_ == null)
			{
				RoleBaseInfo = new RoleBaseInfo();
			}
			RoleBaseInfo.MergeFrom(other.RoleBaseInfo);
		}
		if (other.ForceId != ESeasonForce.ForceNeutral)
		{
			ForceId = other.ForceId;
		}
		if (other.TeamId != 0)
		{
			TeamId = other.TeamId;
		}
		if (other.FromX != 0f)
		{
			FromX = other.FromX;
		}
		if (other.FromY != 0f)
		{
			FromY = other.FromY;
		}
		if (other.ToX != 0f)
		{
			ToX = other.ToX;
		}
		if (other.ToY != 0f)
		{
			ToY = other.ToY;
		}
		if (other.StartTm != 0L)
		{
			StartTm = other.StartTm;
		}
		if (other.ArrivalTm != 0L)
		{
			ArrivalTm = other.ArrivalTm;
		}
		if (other.MoveType != 0)
		{
			MoveType = other.MoveType;
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
				if (roleBaseInfo_ == null)
				{
					RoleBaseInfo = new RoleBaseInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)RoleBaseInfo);
				break;
			case 16u:
				ForceId = (ESeasonForce)((ParseContext)(ref input)).ReadEnum();
				break;
			case 24u:
				TeamId = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 37u:
				FromX = ((ParseContext)(ref input)).ReadFloat();
				break;
			case 45u:
				FromY = ((ParseContext)(ref input)).ReadFloat();
				break;
			case 53u:
				ToX = ((ParseContext)(ref input)).ReadFloat();
				break;
			case 61u:
				ToY = ((ParseContext)(ref input)).ReadFloat();
				break;
			case 64u:
				StartTm = ((ParseContext)(ref input)).ReadInt64();
				break;
			case 72u:
				ArrivalTm = ((ParseContext)(ref input)).ReadInt64();
				break;
			case 80u:
				MoveType = ((ParseContext)(ref input)).ReadInt32();
				break;
			}
		}
	}
}
