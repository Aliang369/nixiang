using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class BattleTeamInfo : IMessage<BattleTeamInfo>, IMessage, IEquatable<BattleTeamInfo>, IDeepCloneable<BattleTeamInfo>, IBufferMessage
{
	private static readonly MessageParser<BattleTeamInfo> _parser = new MessageParser<BattleTeamInfo>((Func<BattleTeamInfo>)(() => new BattleTeamInfo()));

	private UnknownFieldSet _unknownFields;

	public const int RoleBaseInfoFieldNumber = 1;

	private RoleBaseInfo roleBaseInfo_;

	public const int SkillListFieldNumber = 2;

	private static readonly FieldCodec<BattleSkillDetail> _repeated_skillList_codec = FieldCodec.ForMessage<BattleSkillDetail>(18u, BattleSkillDetail.Parser);

	private readonly RepeatedField<BattleSkillDetail> skillList_ = new RepeatedField<BattleSkillDetail>();

	public const int AttrListFieldNumber = 3;

	private static readonly FieldCodec<BattleTeamAttrDetail> _repeated_attrList_codec = FieldCodec.ForMessage<BattleTeamAttrDetail>(26u, BattleTeamAttrDetail.Parser);

	private readonly RepeatedField<BattleTeamAttrDetail> attrList_ = new RepeatedField<BattleTeamAttrDetail>();

	public const int ModListFieldNumber = 4;

	private static readonly FieldCodec<int> _repeated_modList_codec = FieldCodec.ForInt32(34u);

	private readonly RepeatedField<int> modList_ = new RepeatedField<int>();

	public const int SelectListFieldNumber = 5;

	private static readonly FieldCodec<BattleResultModDetail> _repeated_selectList_codec = FieldCodec.ForMessage<BattleResultModDetail>(42u, BattleResultModDetail.Parser);

	private readonly RepeatedField<BattleResultModDetail> selectList_ = new RepeatedField<BattleResultModDetail>();

	public const int MechaInfoFieldNumber = 6;

	private BattleMechaInfo mechaInfo_;

	public const int DefenseInfoFieldNumber = 7;

	private BattleDefenseDetail defenseInfo_;

	public const int FootholdInfoFieldNumber = 8;

	private BattleTeamFootholdInfo footholdInfo_;

	public const int ServantListFieldNumber = 9;

	private static readonly FieldCodec<BattleServantDetail> _repeated_servantList_codec = FieldCodec.ForMessage<BattleServantDetail>(74u, BattleServantDetail.Parser);

	private readonly RepeatedField<BattleServantDetail> servantList_ = new RepeatedField<BattleServantDetail>();

	public const int TacticListFieldNumber = 10;

	private static readonly FieldCodec<BattleTacticDetail> _repeated_tacticList_codec = FieldCodec.ForMessage<BattleTacticDetail>(82u, BattleTacticDetail.Parser);

	private readonly RepeatedField<BattleTacticDetail> tacticList_ = new RepeatedField<BattleTacticDetail>();

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<BattleTeamInfo> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeBattleTeamInfoReflection.Descriptor.MessageTypes[0];

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
	public RepeatedField<BattleSkillDetail> SkillList => skillList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<BattleTeamAttrDetail> AttrList => attrList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<int> ModList => modList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<BattleResultModDetail> SelectList => selectList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public BattleMechaInfo MechaInfo
	{
		get
		{
			return mechaInfo_;
		}
		set
		{
			mechaInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public BattleDefenseDetail DefenseInfo
	{
		get
		{
			return defenseInfo_;
		}
		set
		{
			defenseInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public BattleTeamFootholdInfo FootholdInfo
	{
		get
		{
			return footholdInfo_;
		}
		set
		{
			footholdInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<BattleServantDetail> ServantList => servantList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<BattleTacticDetail> TacticList => tacticList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public BattleTeamInfo()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public BattleTeamInfo(BattleTeamInfo other)
		: this()
	{
		roleBaseInfo_ = ((other.roleBaseInfo_ != null) ? other.roleBaseInfo_.Clone() : null);
		skillList_ = other.skillList_.Clone();
		attrList_ = other.attrList_.Clone();
		modList_ = other.modList_.Clone();
		selectList_ = other.selectList_.Clone();
		mechaInfo_ = ((other.mechaInfo_ != null) ? other.mechaInfo_.Clone() : null);
		defenseInfo_ = ((other.defenseInfo_ != null) ? other.defenseInfo_.Clone() : null);
		footholdInfo_ = ((other.footholdInfo_ != null) ? other.footholdInfo_.Clone() : null);
		servantList_ = other.servantList_.Clone();
		tacticList_ = other.tacticList_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public BattleTeamInfo Clone()
	{
		return new BattleTeamInfo(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as BattleTeamInfo);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(BattleTeamInfo other)
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
		if (!skillList_.Equals(other.skillList_))
		{
			return false;
		}
		if (!attrList_.Equals(other.attrList_))
		{
			return false;
		}
		if (!modList_.Equals(other.modList_))
		{
			return false;
		}
		if (!selectList_.Equals(other.selectList_))
		{
			return false;
		}
		if (!object.Equals(MechaInfo, other.MechaInfo))
		{
			return false;
		}
		if (!object.Equals(DefenseInfo, other.DefenseInfo))
		{
			return false;
		}
		if (!object.Equals(FootholdInfo, other.FootholdInfo))
		{
			return false;
		}
		if (!servantList_.Equals(other.servantList_))
		{
			return false;
		}
		if (!tacticList_.Equals(other.tacticList_))
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
		num ^= ((object)skillList_).GetHashCode();
		num ^= ((object)attrList_).GetHashCode();
		num ^= ((object)modList_).GetHashCode();
		num ^= ((object)selectList_).GetHashCode();
		if (mechaInfo_ != null)
		{
			num ^= MechaInfo.GetHashCode();
		}
		if (defenseInfo_ != null)
		{
			num ^= DefenseInfo.GetHashCode();
		}
		if (footholdInfo_ != null)
		{
			num ^= FootholdInfo.GetHashCode();
		}
		num ^= ((object)servantList_).GetHashCode();
		num ^= ((object)tacticList_).GetHashCode();
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
		skillList_.WriteTo(ref output, _repeated_skillList_codec);
		attrList_.WriteTo(ref output, _repeated_attrList_codec);
		modList_.WriteTo(ref output, _repeated_modList_codec);
		selectList_.WriteTo(ref output, _repeated_selectList_codec);
		if (mechaInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)50);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)MechaInfo);
		}
		if (defenseInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)58);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)DefenseInfo);
		}
		if (footholdInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)66);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)FootholdInfo);
		}
		servantList_.WriteTo(ref output, _repeated_servantList_codec);
		tacticList_.WriteTo(ref output, _repeated_tacticList_codec);
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
		num += skillList_.CalculateSize(_repeated_skillList_codec);
		num += attrList_.CalculateSize(_repeated_attrList_codec);
		num += modList_.CalculateSize(_repeated_modList_codec);
		num += selectList_.CalculateSize(_repeated_selectList_codec);
		if (mechaInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)MechaInfo);
		}
		if (defenseInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)DefenseInfo);
		}
		if (footholdInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)FootholdInfo);
		}
		num += servantList_.CalculateSize(_repeated_servantList_codec);
		num += tacticList_.CalculateSize(_repeated_tacticList_codec);
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(BattleTeamInfo other)
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
		skillList_.Add((IEnumerable<BattleSkillDetail>)other.skillList_);
		attrList_.Add((IEnumerable<BattleTeamAttrDetail>)other.attrList_);
		modList_.Add((IEnumerable<int>)other.modList_);
		selectList_.Add((IEnumerable<BattleResultModDetail>)other.selectList_);
		if (other.mechaInfo_ != null)
		{
			if (mechaInfo_ == null)
			{
				MechaInfo = new BattleMechaInfo();
			}
			MechaInfo.MergeFrom(other.MechaInfo);
		}
		if (other.defenseInfo_ != null)
		{
			if (defenseInfo_ == null)
			{
				DefenseInfo = new BattleDefenseDetail();
			}
			DefenseInfo.MergeFrom(other.DefenseInfo);
		}
		if (other.footholdInfo_ != null)
		{
			if (footholdInfo_ == null)
			{
				FootholdInfo = new BattleTeamFootholdInfo();
			}
			FootholdInfo.MergeFrom(other.FootholdInfo);
		}
		servantList_.Add((IEnumerable<BattleServantDetail>)other.servantList_);
		tacticList_.Add((IEnumerable<BattleTacticDetail>)other.tacticList_);
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
			case 18u:
				skillList_.AddEntriesFrom(ref input, _repeated_skillList_codec);
				break;
			case 26u:
				attrList_.AddEntriesFrom(ref input, _repeated_attrList_codec);
				break;
			case 32u:
			case 34u:
				modList_.AddEntriesFrom(ref input, _repeated_modList_codec);
				break;
			case 42u:
				selectList_.AddEntriesFrom(ref input, _repeated_selectList_codec);
				break;
			case 50u:
				if (mechaInfo_ == null)
				{
					MechaInfo = new BattleMechaInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)MechaInfo);
				break;
			case 58u:
				if (defenseInfo_ == null)
				{
					DefenseInfo = new BattleDefenseDetail();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)DefenseInfo);
				break;
			case 66u:
				if (footholdInfo_ == null)
				{
					FootholdInfo = new BattleTeamFootholdInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)FootholdInfo);
				break;
			case 74u:
				servantList_.AddEntriesFrom(ref input, _repeated_servantList_codec);
				break;
			case 82u:
				tacticList_.AddEntriesFrom(ref input, _repeated_tacticList_codec);
				break;
			}
		}
	}
}
