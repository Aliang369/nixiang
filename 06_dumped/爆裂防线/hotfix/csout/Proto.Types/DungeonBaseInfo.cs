using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class DungeonBaseInfo : IMessage<DungeonBaseInfo>, IMessage, IEquatable<DungeonBaseInfo>, IDeepCloneable<DungeonBaseInfo>, IBufferMessage
{
	private static readonly MessageParser<DungeonBaseInfo> _parser = new MessageParser<DungeonBaseInfo>((Func<DungeonBaseInfo>)(() => new DungeonBaseInfo()));

	private UnknownFieldSet _unknownFields;

	public const int DungeonIdFieldNumber = 1;

	private int dungeonId_;

	public const int PassStateFieldNumber = 2;

	private EPassState passState_;

	public const int RiotValueFieldNumber = 3;

	private int riotValue_;

	public const int EventCountFieldNumber = 4;

	private int eventCount_;

	public const int TriggerCountFieldNumber = 5;

	private int triggerCount_;

	public const int KillCountFieldNumber = 6;

	private int killCount_;

	public const int DefenseMaxFieldNumber = 7;

	private int defenseMax_;

	public const int DefensePctFieldNumber = 8;

	private float defensePct_;

	public const int ModListFieldNumber = 9;

	private static readonly FieldCodec<int> _repeated_modList_codec = FieldCodec.ForInt32(74u);

	private readonly RepeatedField<int> modList_ = new RepeatedField<int>();

	public const int SkillListFieldNumber = 10;

	private static readonly FieldCodec<SkillDetail> _repeated_skillList_codec = FieldCodec.ForMessage<SkillDetail>(82u, SkillDetail.Parser);

	private readonly RepeatedField<SkillDetail> skillList_ = new RepeatedField<SkillDetail>();

	public const int RoleSiteInfoFieldNumber = 11;

	private DungeonRoleSiteInfo roleSiteInfo_;

	public const int TriggerEventFieldNumber = 12;

	private DungeonTriggerEventInfo triggerEvent_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<DungeonBaseInfo> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeDungeonBaseInfoReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int DungeonId
	{
		get
		{
			return dungeonId_;
		}
		set
		{
			dungeonId_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public EPassState PassState
	{
		get
		{
			return passState_;
		}
		set
		{
			passState_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int RiotValue
	{
		get
		{
			return riotValue_;
		}
		set
		{
			riotValue_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int EventCount
	{
		get
		{
			return eventCount_;
		}
		set
		{
			eventCount_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int TriggerCount
	{
		get
		{
			return triggerCount_;
		}
		set
		{
			triggerCount_ = value;
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
	public int DefenseMax
	{
		get
		{
			return defenseMax_;
		}
		set
		{
			defenseMax_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public float DefensePct
	{
		get
		{
			return defensePct_;
		}
		set
		{
			defensePct_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<int> ModList => modList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<SkillDetail> SkillList => skillList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public DungeonRoleSiteInfo RoleSiteInfo
	{
		get
		{
			return roleSiteInfo_;
		}
		set
		{
			roleSiteInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public DungeonTriggerEventInfo TriggerEvent
	{
		get
		{
			return triggerEvent_;
		}
		set
		{
			triggerEvent_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public DungeonBaseInfo()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public DungeonBaseInfo(DungeonBaseInfo other)
		: this()
	{
		dungeonId_ = other.dungeonId_;
		passState_ = other.passState_;
		riotValue_ = other.riotValue_;
		eventCount_ = other.eventCount_;
		triggerCount_ = other.triggerCount_;
		killCount_ = other.killCount_;
		defenseMax_ = other.defenseMax_;
		defensePct_ = other.defensePct_;
		modList_ = other.modList_.Clone();
		skillList_ = other.skillList_.Clone();
		roleSiteInfo_ = ((other.roleSiteInfo_ != null) ? other.roleSiteInfo_.Clone() : null);
		triggerEvent_ = ((other.triggerEvent_ != null) ? other.triggerEvent_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public DungeonBaseInfo Clone()
	{
		return new DungeonBaseInfo(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as DungeonBaseInfo);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(DungeonBaseInfo other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (DungeonId != other.DungeonId)
		{
			return false;
		}
		if (PassState != other.PassState)
		{
			return false;
		}
		if (RiotValue != other.RiotValue)
		{
			return false;
		}
		if (EventCount != other.EventCount)
		{
			return false;
		}
		if (TriggerCount != other.TriggerCount)
		{
			return false;
		}
		if (KillCount != other.KillCount)
		{
			return false;
		}
		if (DefenseMax != other.DefenseMax)
		{
			return false;
		}
		if (!ProtobufEqualityComparers.BitwiseSingleEqualityComparer.Equals(DefensePct, other.DefensePct))
		{
			return false;
		}
		if (!modList_.Equals(other.modList_))
		{
			return false;
		}
		if (!skillList_.Equals(other.skillList_))
		{
			return false;
		}
		if (!object.Equals(RoleSiteInfo, other.RoleSiteInfo))
		{
			return false;
		}
		if (!object.Equals(TriggerEvent, other.TriggerEvent))
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
		if (DungeonId != 0)
		{
			num ^= DungeonId.GetHashCode();
		}
		if (PassState != EPassState.NotStarted)
		{
			num ^= PassState.GetHashCode();
		}
		if (RiotValue != 0)
		{
			num ^= RiotValue.GetHashCode();
		}
		if (EventCount != 0)
		{
			num ^= EventCount.GetHashCode();
		}
		if (TriggerCount != 0)
		{
			num ^= TriggerCount.GetHashCode();
		}
		if (KillCount != 0)
		{
			num ^= KillCount.GetHashCode();
		}
		if (DefenseMax != 0)
		{
			num ^= DefenseMax.GetHashCode();
		}
		if (DefensePct != 0f)
		{
			num ^= ProtobufEqualityComparers.BitwiseSingleEqualityComparer.GetHashCode(DefensePct);
		}
		num ^= ((object)modList_).GetHashCode();
		num ^= ((object)skillList_).GetHashCode();
		if (roleSiteInfo_ != null)
		{
			num ^= RoleSiteInfo.GetHashCode();
		}
		if (triggerEvent_ != null)
		{
			num ^= TriggerEvent.GetHashCode();
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
		if (DungeonId != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteInt32(DungeonId);
		}
		if (PassState != EPassState.NotStarted)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteEnum((int)PassState);
		}
		if (RiotValue != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)24);
			((WriteContext)(ref output)).WriteInt32(RiotValue);
		}
		if (EventCount != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)32);
			((WriteContext)(ref output)).WriteInt32(EventCount);
		}
		if (TriggerCount != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)40);
			((WriteContext)(ref output)).WriteInt32(TriggerCount);
		}
		if (KillCount != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)48);
			((WriteContext)(ref output)).WriteInt32(KillCount);
		}
		if (DefenseMax != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)56);
			((WriteContext)(ref output)).WriteInt32(DefenseMax);
		}
		if (DefensePct != 0f)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)69);
			((WriteContext)(ref output)).WriteFloat(DefensePct);
		}
		modList_.WriteTo(ref output, _repeated_modList_codec);
		skillList_.WriteTo(ref output, _repeated_skillList_codec);
		if (roleSiteInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)90);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)RoleSiteInfo);
		}
		if (triggerEvent_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)98);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)TriggerEvent);
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
		if (DungeonId != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(DungeonId);
		}
		if (PassState != EPassState.NotStarted)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)PassState);
		}
		if (RiotValue != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(RiotValue);
		}
		if (EventCount != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(EventCount);
		}
		if (TriggerCount != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(TriggerCount);
		}
		if (KillCount != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(KillCount);
		}
		if (DefenseMax != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(DefenseMax);
		}
		if (DefensePct != 0f)
		{
			num += 5;
		}
		num += modList_.CalculateSize(_repeated_modList_codec);
		num += skillList_.CalculateSize(_repeated_skillList_codec);
		if (roleSiteInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)RoleSiteInfo);
		}
		if (triggerEvent_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)TriggerEvent);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(DungeonBaseInfo other)
	{
		if (other == null)
		{
			return;
		}
		if (other.DungeonId != 0)
		{
			DungeonId = other.DungeonId;
		}
		if (other.PassState != EPassState.NotStarted)
		{
			PassState = other.PassState;
		}
		if (other.RiotValue != 0)
		{
			RiotValue = other.RiotValue;
		}
		if (other.EventCount != 0)
		{
			EventCount = other.EventCount;
		}
		if (other.TriggerCount != 0)
		{
			TriggerCount = other.TriggerCount;
		}
		if (other.KillCount != 0)
		{
			KillCount = other.KillCount;
		}
		if (other.DefenseMax != 0)
		{
			DefenseMax = other.DefenseMax;
		}
		if (other.DefensePct != 0f)
		{
			DefensePct = other.DefensePct;
		}
		modList_.Add((IEnumerable<int>)other.modList_);
		skillList_.Add((IEnumerable<SkillDetail>)other.skillList_);
		if (other.roleSiteInfo_ != null)
		{
			if (roleSiteInfo_ == null)
			{
				RoleSiteInfo = new DungeonRoleSiteInfo();
			}
			RoleSiteInfo.MergeFrom(other.RoleSiteInfo);
		}
		if (other.triggerEvent_ != null)
		{
			if (triggerEvent_ == null)
			{
				TriggerEvent = new DungeonTriggerEventInfo();
			}
			TriggerEvent.MergeFrom(other.TriggerEvent);
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
				DungeonId = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 16u:
				PassState = (EPassState)((ParseContext)(ref input)).ReadEnum();
				break;
			case 24u:
				RiotValue = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 32u:
				EventCount = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 40u:
				TriggerCount = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 48u:
				KillCount = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 56u:
				DefenseMax = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 69u:
				DefensePct = ((ParseContext)(ref input)).ReadFloat();
				break;
			case 72u:
			case 74u:
				modList_.AddEntriesFrom(ref input, _repeated_modList_codec);
				break;
			case 82u:
				skillList_.AddEntriesFrom(ref input, _repeated_skillList_codec);
				break;
			case 90u:
				if (roleSiteInfo_ == null)
				{
					RoleSiteInfo = new DungeonRoleSiteInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)RoleSiteInfo);
				break;
			case 98u:
				if (triggerEvent_ == null)
				{
					TriggerEvent = new DungeonTriggerEventInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)TriggerEvent);
				break;
			}
		}
	}
}
