using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class SeasonTeamPosInfo : IMessage<SeasonTeamPosInfo>, IMessage, IEquatable<SeasonTeamPosInfo>, IDeepCloneable<SeasonTeamPosInfo>, IBufferMessage
{
	private static readonly MessageParser<SeasonTeamPosInfo> _parser = new MessageParser<SeasonTeamPosInfo>((Func<SeasonTeamPosInfo>)(() => new SeasonTeamPosInfo()));

	private UnknownFieldSet _unknownFields;

	public const int RoleIdFieldNumber = 1;

	private long roleId_;

	public const int TeamIdFieldNumber = 2;

	private int teamId_;

	public const int ForceIdFieldNumber = 3;

	private ESeasonForce forceId_;

	public const int StateFieldNumber = 4;

	private int state_;

	public const int CityIdFieldNumber = 5;

	private int cityId_;

	public const int PosXFieldNumber = 6;

	private float posX_;

	public const int PosYFieldNumber = 7;

	private float posY_;

	public const int DurabilityFieldNumber = 8;

	private int durability_;

	public const int MarchFieldNumber = 9;

	private SeasonTeamMarchInfo march_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<SeasonTeamPosInfo> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeSeasonTeamPosInfoReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public long RoleId
	{
		get
		{
			return roleId_;
		}
		set
		{
			roleId_ = value;
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
	public int State
	{
		get
		{
			return state_;
		}
		set
		{
			state_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int CityId
	{
		get
		{
			return cityId_;
		}
		set
		{
			cityId_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public float PosX
	{
		get
		{
			return posX_;
		}
		set
		{
			posX_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public float PosY
	{
		get
		{
			return posY_;
		}
		set
		{
			posY_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int Durability
	{
		get
		{
			return durability_;
		}
		set
		{
			durability_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SeasonTeamMarchInfo March
	{
		get
		{
			return march_;
		}
		set
		{
			march_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SeasonTeamPosInfo()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SeasonTeamPosInfo(SeasonTeamPosInfo other)
		: this()
	{
		roleId_ = other.roleId_;
		teamId_ = other.teamId_;
		forceId_ = other.forceId_;
		state_ = other.state_;
		cityId_ = other.cityId_;
		posX_ = other.posX_;
		posY_ = other.posY_;
		durability_ = other.durability_;
		march_ = ((other.march_ != null) ? other.march_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SeasonTeamPosInfo Clone()
	{
		return new SeasonTeamPosInfo(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as SeasonTeamPosInfo);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(SeasonTeamPosInfo other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (RoleId != other.RoleId)
		{
			return false;
		}
		if (TeamId != other.TeamId)
		{
			return false;
		}
		if (ForceId != other.ForceId)
		{
			return false;
		}
		if (State != other.State)
		{
			return false;
		}
		if (CityId != other.CityId)
		{
			return false;
		}
		if (!ProtobufEqualityComparers.BitwiseSingleEqualityComparer.Equals(PosX, other.PosX))
		{
			return false;
		}
		if (!ProtobufEqualityComparers.BitwiseSingleEqualityComparer.Equals(PosY, other.PosY))
		{
			return false;
		}
		if (Durability != other.Durability)
		{
			return false;
		}
		if (!object.Equals(March, other.March))
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
		if (RoleId != 0L)
		{
			num ^= RoleId.GetHashCode();
		}
		if (TeamId != 0)
		{
			num ^= TeamId.GetHashCode();
		}
		if (ForceId != ESeasonForce.ForceNeutral)
		{
			num ^= ForceId.GetHashCode();
		}
		if (State != 0)
		{
			num ^= State.GetHashCode();
		}
		if (CityId != 0)
		{
			num ^= CityId.GetHashCode();
		}
		if (PosX != 0f)
		{
			num ^= ProtobufEqualityComparers.BitwiseSingleEqualityComparer.GetHashCode(PosX);
		}
		if (PosY != 0f)
		{
			num ^= ProtobufEqualityComparers.BitwiseSingleEqualityComparer.GetHashCode(PosY);
		}
		if (Durability != 0)
		{
			num ^= Durability.GetHashCode();
		}
		if (march_ != null)
		{
			num ^= March.GetHashCode();
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
		if (RoleId != 0L)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteInt64(RoleId);
		}
		if (TeamId != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteInt32(TeamId);
		}
		if (ForceId != ESeasonForce.ForceNeutral)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)24);
			((WriteContext)(ref output)).WriteEnum((int)ForceId);
		}
		if (State != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)32);
			((WriteContext)(ref output)).WriteInt32(State);
		}
		if (CityId != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)40);
			((WriteContext)(ref output)).WriteInt32(CityId);
		}
		if (PosX != 0f)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)53);
			((WriteContext)(ref output)).WriteFloat(PosX);
		}
		if (PosY != 0f)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)61);
			((WriteContext)(ref output)).WriteFloat(PosY);
		}
		if (Durability != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)64);
			((WriteContext)(ref output)).WriteInt32(Durability);
		}
		if (march_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)74);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)March);
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
		if (RoleId != 0L)
		{
			num += 1 + CodedOutputStream.ComputeInt64Size(RoleId);
		}
		if (TeamId != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(TeamId);
		}
		if (ForceId != ESeasonForce.ForceNeutral)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)ForceId);
		}
		if (State != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(State);
		}
		if (CityId != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(CityId);
		}
		if (PosX != 0f)
		{
			num += 5;
		}
		if (PosY != 0f)
		{
			num += 5;
		}
		if (Durability != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(Durability);
		}
		if (march_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)March);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(SeasonTeamPosInfo other)
	{
		if (other == null)
		{
			return;
		}
		if (other.RoleId != 0L)
		{
			RoleId = other.RoleId;
		}
		if (other.TeamId != 0)
		{
			TeamId = other.TeamId;
		}
		if (other.ForceId != ESeasonForce.ForceNeutral)
		{
			ForceId = other.ForceId;
		}
		if (other.State != 0)
		{
			State = other.State;
		}
		if (other.CityId != 0)
		{
			CityId = other.CityId;
		}
		if (other.PosX != 0f)
		{
			PosX = other.PosX;
		}
		if (other.PosY != 0f)
		{
			PosY = other.PosY;
		}
		if (other.Durability != 0)
		{
			Durability = other.Durability;
		}
		if (other.march_ != null)
		{
			if (march_ == null)
			{
				March = new SeasonTeamMarchInfo();
			}
			March.MergeFrom(other.March);
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
				RoleId = ((ParseContext)(ref input)).ReadInt64();
				break;
			case 16u:
				TeamId = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 24u:
				ForceId = (ESeasonForce)((ParseContext)(ref input)).ReadEnum();
				break;
			case 32u:
				State = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 40u:
				CityId = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 53u:
				PosX = ((ParseContext)(ref input)).ReadFloat();
				break;
			case 61u:
				PosY = ((ParseContext)(ref input)).ReadFloat();
				break;
			case 64u:
				Durability = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 74u:
				if (march_ == null)
				{
					March = new SeasonTeamMarchInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)March);
				break;
			}
		}
	}
}
