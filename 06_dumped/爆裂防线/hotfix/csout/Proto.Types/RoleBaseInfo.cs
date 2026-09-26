using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class RoleBaseInfo : IMessage<RoleBaseInfo>, IMessage, IEquatable<RoleBaseInfo>, IDeepCloneable<RoleBaseInfo>, IBufferMessage
{
	private static readonly MessageParser<RoleBaseInfo> _parser = new MessageParser<RoleBaseInfo>((Func<RoleBaseInfo>)(() => new RoleBaseInfo()));

	private UnknownFieldSet _unknownFields;

	public const int RoleIdFieldNumber = 1;

	private long roleId_;

	public const int RoleNameFieldNumber = 2;

	private string roleName_ = "";

	public const int ServerIdFieldNumber = 3;

	private int serverId_;

	public const int LevelFieldNumber = 4;

	private int level_;

	public const int VipLevelFieldNumber = 5;

	private int vipLevel_;

	public const int AvatarIdFieldNumber = 6;

	private int avatarId_;

	public const int SuitIdFieldNumber = 7;

	private int suitId_;

	public const int GunIdFieldNumber = 8;

	private int gunId_;

	public const int SloganIdFieldNumber = 9;

	private int sloganId_;

	public const int HeadIdFieldNumber = 10;

	private int headId_;

	public const int HeadFrameIdFieldNumber = 11;

	private int headFrameId_;

	public const int TeamAttackFieldNumber = 12;

	private int teamAttack_;

	public const int MechaIdFieldNumber = 13;

	private int mechaId_;

	public const int FootholdIdFieldNumber = 14;

	private int footholdId_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<RoleBaseInfo> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeRoleBaseInfoReflection.Descriptor.MessageTypes[0];

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
	public string RoleName
	{
		get
		{
			return roleName_;
		}
		set
		{
			roleName_ = ProtoPreconditions.CheckNotNull<string>(value, "value");
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int ServerId
	{
		get
		{
			return serverId_;
		}
		set
		{
			serverId_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int Level
	{
		get
		{
			return level_;
		}
		set
		{
			level_ = value;
		}
	}

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
	public int AvatarId
	{
		get
		{
			return avatarId_;
		}
		set
		{
			avatarId_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int SuitId
	{
		get
		{
			return suitId_;
		}
		set
		{
			suitId_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int GunId
	{
		get
		{
			return gunId_;
		}
		set
		{
			gunId_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int SloganId
	{
		get
		{
			return sloganId_;
		}
		set
		{
			sloganId_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int HeadId
	{
		get
		{
			return headId_;
		}
		set
		{
			headId_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int HeadFrameId
	{
		get
		{
			return headFrameId_;
		}
		set
		{
			headFrameId_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int TeamAttack
	{
		get
		{
			return teamAttack_;
		}
		set
		{
			teamAttack_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int MechaId
	{
		get
		{
			return mechaId_;
		}
		set
		{
			mechaId_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int FootholdId
	{
		get
		{
			return footholdId_;
		}
		set
		{
			footholdId_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RoleBaseInfo()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RoleBaseInfo(RoleBaseInfo other)
		: this()
	{
		roleId_ = other.roleId_;
		roleName_ = other.roleName_;
		serverId_ = other.serverId_;
		level_ = other.level_;
		vipLevel_ = other.vipLevel_;
		avatarId_ = other.avatarId_;
		suitId_ = other.suitId_;
		gunId_ = other.gunId_;
		sloganId_ = other.sloganId_;
		headId_ = other.headId_;
		headFrameId_ = other.headFrameId_;
		teamAttack_ = other.teamAttack_;
		mechaId_ = other.mechaId_;
		footholdId_ = other.footholdId_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RoleBaseInfo Clone()
	{
		return new RoleBaseInfo(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as RoleBaseInfo);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(RoleBaseInfo other)
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
		if (RoleName != other.RoleName)
		{
			return false;
		}
		if (ServerId != other.ServerId)
		{
			return false;
		}
		if (Level != other.Level)
		{
			return false;
		}
		if (VipLevel != other.VipLevel)
		{
			return false;
		}
		if (AvatarId != other.AvatarId)
		{
			return false;
		}
		if (SuitId != other.SuitId)
		{
			return false;
		}
		if (GunId != other.GunId)
		{
			return false;
		}
		if (SloganId != other.SloganId)
		{
			return false;
		}
		if (HeadId != other.HeadId)
		{
			return false;
		}
		if (HeadFrameId != other.HeadFrameId)
		{
			return false;
		}
		if (TeamAttack != other.TeamAttack)
		{
			return false;
		}
		if (MechaId != other.MechaId)
		{
			return false;
		}
		if (FootholdId != other.FootholdId)
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
		if (RoleName.Length != 0)
		{
			num ^= RoleName.GetHashCode();
		}
		if (ServerId != 0)
		{
			num ^= ServerId.GetHashCode();
		}
		if (Level != 0)
		{
			num ^= Level.GetHashCode();
		}
		if (VipLevel != 0)
		{
			num ^= VipLevel.GetHashCode();
		}
		if (AvatarId != 0)
		{
			num ^= AvatarId.GetHashCode();
		}
		if (SuitId != 0)
		{
			num ^= SuitId.GetHashCode();
		}
		if (GunId != 0)
		{
			num ^= GunId.GetHashCode();
		}
		if (SloganId != 0)
		{
			num ^= SloganId.GetHashCode();
		}
		if (HeadId != 0)
		{
			num ^= HeadId.GetHashCode();
		}
		if (HeadFrameId != 0)
		{
			num ^= HeadFrameId.GetHashCode();
		}
		if (TeamAttack != 0)
		{
			num ^= TeamAttack.GetHashCode();
		}
		if (MechaId != 0)
		{
			num ^= MechaId.GetHashCode();
		}
		if (FootholdId != 0)
		{
			num ^= FootholdId.GetHashCode();
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
		if (RoleName.Length != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)18);
			((WriteContext)(ref output)).WriteString(RoleName);
		}
		if (ServerId != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)24);
			((WriteContext)(ref output)).WriteInt32(ServerId);
		}
		if (Level != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)32);
			((WriteContext)(ref output)).WriteInt32(Level);
		}
		if (VipLevel != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)40);
			((WriteContext)(ref output)).WriteInt32(VipLevel);
		}
		if (AvatarId != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)48);
			((WriteContext)(ref output)).WriteInt32(AvatarId);
		}
		if (SuitId != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)56);
			((WriteContext)(ref output)).WriteInt32(SuitId);
		}
		if (GunId != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)64);
			((WriteContext)(ref output)).WriteInt32(GunId);
		}
		if (SloganId != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)72);
			((WriteContext)(ref output)).WriteInt32(SloganId);
		}
		if (HeadId != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)80);
			((WriteContext)(ref output)).WriteInt32(HeadId);
		}
		if (HeadFrameId != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)88);
			((WriteContext)(ref output)).WriteInt32(HeadFrameId);
		}
		if (TeamAttack != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)96);
			((WriteContext)(ref output)).WriteInt32(TeamAttack);
		}
		if (MechaId != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)104);
			((WriteContext)(ref output)).WriteInt32(MechaId);
		}
		if (FootholdId != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)112);
			((WriteContext)(ref output)).WriteInt32(FootholdId);
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
		if (RoleName.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(RoleName);
		}
		if (ServerId != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(ServerId);
		}
		if (Level != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(Level);
		}
		if (VipLevel != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(VipLevel);
		}
		if (AvatarId != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(AvatarId);
		}
		if (SuitId != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(SuitId);
		}
		if (GunId != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(GunId);
		}
		if (SloganId != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(SloganId);
		}
		if (HeadId != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(HeadId);
		}
		if (HeadFrameId != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(HeadFrameId);
		}
		if (TeamAttack != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(TeamAttack);
		}
		if (MechaId != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(MechaId);
		}
		if (FootholdId != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(FootholdId);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(RoleBaseInfo other)
	{
		if (other != null)
		{
			if (other.RoleId != 0L)
			{
				RoleId = other.RoleId;
			}
			if (other.RoleName.Length != 0)
			{
				RoleName = other.RoleName;
			}
			if (other.ServerId != 0)
			{
				ServerId = other.ServerId;
			}
			if (other.Level != 0)
			{
				Level = other.Level;
			}
			if (other.VipLevel != 0)
			{
				VipLevel = other.VipLevel;
			}
			if (other.AvatarId != 0)
			{
				AvatarId = other.AvatarId;
			}
			if (other.SuitId != 0)
			{
				SuitId = other.SuitId;
			}
			if (other.GunId != 0)
			{
				GunId = other.GunId;
			}
			if (other.SloganId != 0)
			{
				SloganId = other.SloganId;
			}
			if (other.HeadId != 0)
			{
				HeadId = other.HeadId;
			}
			if (other.HeadFrameId != 0)
			{
				HeadFrameId = other.HeadFrameId;
			}
			if (other.TeamAttack != 0)
			{
				TeamAttack = other.TeamAttack;
			}
			if (other.MechaId != 0)
			{
				MechaId = other.MechaId;
			}
			if (other.FootholdId != 0)
			{
				FootholdId = other.FootholdId;
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
				RoleId = ((ParseContext)(ref input)).ReadInt64();
				break;
			case 18u:
				RoleName = ((ParseContext)(ref input)).ReadString();
				break;
			case 24u:
				ServerId = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 32u:
				Level = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 40u:
				VipLevel = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 48u:
				AvatarId = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 56u:
				SuitId = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 64u:
				GunId = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 72u:
				SloganId = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 80u:
				HeadId = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 88u:
				HeadFrameId = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 96u:
				TeamAttack = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 104u:
				MechaId = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 112u:
				FootholdId = ((ParseContext)(ref input)).ReadInt32();
				break;
			}
		}
	}
}
