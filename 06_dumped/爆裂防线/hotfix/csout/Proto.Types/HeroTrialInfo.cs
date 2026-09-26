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
public sealed class HeroTrialInfo : IMessage<HeroTrialInfo>, IMessage, IEquatable<HeroTrialInfo>, IDeepCloneable<HeroTrialInfo>, IBufferMessage
{
	private static readonly MessageParser<HeroTrialInfo> _parser = new MessageParser<HeroTrialInfo>((Func<HeroTrialInfo>)(() => new HeroTrialInfo()));

	private UnknownFieldSet _unknownFields;

	public const int CurrentTypeFieldNumber = 1;

	private ESkillType currentType_;

	public const int CurrentLevelFieldNumber = 2;

	private int currentLevel_;

	public const int ShowEndTmFieldNumber = 3;

	private long showEndTm_;

	public const int RefreshTmFieldNumber = 4;

	private long refreshTm_;

	public const int CombatCountFieldNumber = 5;

	private int combatCount_;

	public const int DailyCountFieldNumber = 6;

	private int dailyCount_;

	public const int TrialListFieldNumber = 7;

	private static readonly FieldCodec<TrialDetail> _repeated_trialList_codec = FieldCodec.ForMessage<TrialDetail>(58u, TrialDetail.Parser);

	private readonly RepeatedField<TrialDetail> trialList_ = new RepeatedField<TrialDetail>();

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<HeroTrialInfo> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeHeroTrialInfoReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ESkillType CurrentType
	{
		get
		{
			return currentType_;
		}
		set
		{
			currentType_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int CurrentLevel
	{
		get
		{
			return currentLevel_;
		}
		set
		{
			currentLevel_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public long ShowEndTm
	{
		get
		{
			return showEndTm_;
		}
		set
		{
			showEndTm_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public long RefreshTm
	{
		get
		{
			return refreshTm_;
		}
		set
		{
			refreshTm_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int CombatCount
	{
		get
		{
			return combatCount_;
		}
		set
		{
			combatCount_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int DailyCount
	{
		get
		{
			return dailyCount_;
		}
		set
		{
			dailyCount_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<TrialDetail> TrialList => trialList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public HeroTrialInfo()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public HeroTrialInfo(HeroTrialInfo other)
		: this()
	{
		currentType_ = other.currentType_;
		currentLevel_ = other.currentLevel_;
		showEndTm_ = other.showEndTm_;
		refreshTm_ = other.refreshTm_;
		combatCount_ = other.combatCount_;
		dailyCount_ = other.dailyCount_;
		trialList_ = other.trialList_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public HeroTrialInfo Clone()
	{
		return new HeroTrialInfo(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as HeroTrialInfo);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(HeroTrialInfo other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (CurrentType != other.CurrentType)
		{
			return false;
		}
		if (CurrentLevel != other.CurrentLevel)
		{
			return false;
		}
		if (ShowEndTm != other.ShowEndTm)
		{
			return false;
		}
		if (RefreshTm != other.RefreshTm)
		{
			return false;
		}
		if (CombatCount != other.CombatCount)
		{
			return false;
		}
		if (DailyCount != other.DailyCount)
		{
			return false;
		}
		if (!trialList_.Equals(other.trialList_))
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
		if (CurrentType != ESkillType.EmptyValue)
		{
			num ^= CurrentType.GetHashCode();
		}
		if (CurrentLevel != 0)
		{
			num ^= CurrentLevel.GetHashCode();
		}
		if (ShowEndTm != 0L)
		{
			num ^= ShowEndTm.GetHashCode();
		}
		if (RefreshTm != 0L)
		{
			num ^= RefreshTm.GetHashCode();
		}
		if (CombatCount != 0)
		{
			num ^= CombatCount.GetHashCode();
		}
		if (DailyCount != 0)
		{
			num ^= DailyCount.GetHashCode();
		}
		num ^= ((object)trialList_).GetHashCode();
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
		if (CurrentType != ESkillType.EmptyValue)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteEnum((int)CurrentType);
		}
		if (CurrentLevel != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteInt32(CurrentLevel);
		}
		if (ShowEndTm != 0L)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)24);
			((WriteContext)(ref output)).WriteInt64(ShowEndTm);
		}
		if (RefreshTm != 0L)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)32);
			((WriteContext)(ref output)).WriteInt64(RefreshTm);
		}
		if (CombatCount != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)40);
			((WriteContext)(ref output)).WriteInt32(CombatCount);
		}
		if (DailyCount != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)48);
			((WriteContext)(ref output)).WriteInt32(DailyCount);
		}
		trialList_.WriteTo(ref output, _repeated_trialList_codec);
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
		if (CurrentType != ESkillType.EmptyValue)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)CurrentType);
		}
		if (CurrentLevel != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(CurrentLevel);
		}
		if (ShowEndTm != 0L)
		{
			num += 1 + CodedOutputStream.ComputeInt64Size(ShowEndTm);
		}
		if (RefreshTm != 0L)
		{
			num += 1 + CodedOutputStream.ComputeInt64Size(RefreshTm);
		}
		if (CombatCount != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(CombatCount);
		}
		if (DailyCount != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(DailyCount);
		}
		num += trialList_.CalculateSize(_repeated_trialList_codec);
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(HeroTrialInfo other)
	{
		if (other != null)
		{
			if (other.CurrentType != ESkillType.EmptyValue)
			{
				CurrentType = other.CurrentType;
			}
			if (other.CurrentLevel != 0)
			{
				CurrentLevel = other.CurrentLevel;
			}
			if (other.ShowEndTm != 0L)
			{
				ShowEndTm = other.ShowEndTm;
			}
			if (other.RefreshTm != 0L)
			{
				RefreshTm = other.RefreshTm;
			}
			if (other.CombatCount != 0)
			{
				CombatCount = other.CombatCount;
			}
			if (other.DailyCount != 0)
			{
				DailyCount = other.DailyCount;
			}
			trialList_.Add((IEnumerable<TrialDetail>)other.trialList_);
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
				CurrentType = (ESkillType)((ParseContext)(ref input)).ReadEnum();
				break;
			case 16u:
				CurrentLevel = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 24u:
				ShowEndTm = ((ParseContext)(ref input)).ReadInt64();
				break;
			case 32u:
				RefreshTm = ((ParseContext)(ref input)).ReadInt64();
				break;
			case 40u:
				CombatCount = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 48u:
				DailyCount = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 58u:
				trialList_.AddEntriesFrom(ref input, _repeated_trialList_codec);
				break;
			}
		}
	}
}
