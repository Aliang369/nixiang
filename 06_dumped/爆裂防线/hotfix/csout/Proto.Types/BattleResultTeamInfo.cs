using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class BattleResultTeamInfo : IMessage<BattleResultTeamInfo>, IMessage, IEquatable<BattleResultTeamInfo>, IDeepCloneable<BattleResultTeamInfo>, IBufferMessage
{
	private static readonly MessageParser<BattleResultTeamInfo> _parser = new MessageParser<BattleResultTeamInfo>((Func<BattleResultTeamInfo>)(() => new BattleResultTeamInfo()));

	private UnknownFieldSet _unknownFields;

	public const int RoleBaseInfoFieldNumber = 1;

	private RoleBaseInfo roleBaseInfo_;

	public const int KillCountFieldNumber = 2;

	private int killCount_;

	public const int HeroListFieldNumber = 3;

	private static readonly FieldCodec<BattleResultHeroDetail> _repeated_heroList_codec = FieldCodec.ForMessage<BattleResultHeroDetail>(26u, BattleResultHeroDetail.Parser);

	private readonly RepeatedField<BattleResultHeroDetail> heroList_ = new RepeatedField<BattleResultHeroDetail>();

	public const int ModListFieldNumber = 4;

	private static readonly FieldCodec<BattleResultModDetail> _repeated_modList_codec = FieldCodec.ForMessage<BattleResultModDetail>(34u, BattleResultModDetail.Parser);

	private readonly RepeatedField<BattleResultModDetail> modList_ = new RepeatedField<BattleResultModDetail>();

	public const int MechaEnergyFieldNumber = 5;

	private int mechaEnergy_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<BattleResultTeamInfo> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeBattleResultTeamInfoReflection.Descriptor.MessageTypes[0];

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
	public int KillCount
	{
		get
		{
			return killCount_;
		}
		set
		{
			killCount_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<BattleResultHeroDetail> HeroList => heroList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<BattleResultModDetail> ModList => modList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int MechaEnergy
	{
		get
		{
			return mechaEnergy_;
		}
		set
		{
			mechaEnergy_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public BattleResultTeamInfo()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public BattleResultTeamInfo(BattleResultTeamInfo other)
		: this()
	{
		roleBaseInfo_ = ((other.roleBaseInfo_ != null) ? other.roleBaseInfo_.Clone() : null);
		killCount_ = other.killCount_;
		heroList_ = other.heroList_.Clone();
		modList_ = other.modList_.Clone();
		mechaEnergy_ = other.mechaEnergy_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public BattleResultTeamInfo Clone()
	{
		return new BattleResultTeamInfo(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as BattleResultTeamInfo);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(BattleResultTeamInfo other)
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
		if (KillCount != other.KillCount)
		{
			return false;
		}
		if (!heroList_.Equals(other.heroList_))
		{
			return false;
		}
		if (!modList_.Equals(other.modList_))
		{
			return false;
		}
		if (MechaEnergy != other.MechaEnergy)
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
		if (KillCount != 0)
		{
			num ^= KillCount.GetHashCode();
		}
		num ^= ((object)heroList_).GetHashCode();
		num ^= ((object)modList_).GetHashCode();
		if (MechaEnergy != 0)
		{
			num ^= MechaEnergy.GetHashCode();
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
		if (KillCount != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteInt32(KillCount);
		}
		heroList_.WriteTo(ref output, _repeated_heroList_codec);
		modList_.WriteTo(ref output, _repeated_modList_codec);
		if (MechaEnergy != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)40);
			((WriteContext)(ref output)).WriteInt32(MechaEnergy);
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
		if (KillCount != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(KillCount);
		}
		num += heroList_.CalculateSize(_repeated_heroList_codec);
		num += modList_.CalculateSize(_repeated_modList_codec);
		if (MechaEnergy != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(MechaEnergy);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(BattleResultTeamInfo other)
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
		if (other.KillCount != 0)
		{
			KillCount = other.KillCount;
		}
		heroList_.Add((IEnumerable<BattleResultHeroDetail>)other.heroList_);
		modList_.Add((IEnumerable<BattleResultModDetail>)other.modList_);
		if (other.MechaEnergy != 0)
		{
			MechaEnergy = other.MechaEnergy;
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
				KillCount = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 26u:
				heroList_.AddEntriesFrom(ref input, _repeated_heroList_codec);
				break;
			case 34u:
				modList_.AddEntriesFrom(ref input, _repeated_modList_codec);
				break;
			case 40u:
				MechaEnergy = ((ParseContext)(ref input)).ReadInt32();
				break;
			}
		}
	}
}
