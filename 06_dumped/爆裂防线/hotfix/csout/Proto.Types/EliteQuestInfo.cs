using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class EliteQuestInfo : IMessage<EliteQuestInfo>, IMessage, IEquatable<EliteQuestInfo>, IDeepCloneable<EliteQuestInfo>, IBufferMessage
{
	private static readonly MessageParser<EliteQuestInfo> _parser = new MessageParser<EliteQuestInfo>((Func<EliteQuestInfo>)(() => new EliteQuestInfo()));

	private UnknownFieldSet _unknownFields;

	public const int EliteLevelFieldNumber = 1;

	private int eliteLevel_;

	public const int NextLevelFieldNumber = 2;

	private int nextLevel_;

	public const int EliteListFieldNumber = 3;

	private static readonly FieldCodec<MainQuestDetail> _repeated_eliteList_codec = FieldCodec.ForMessage<MainQuestDetail>(26u, MainQuestDetail.Parser);

	private readonly RepeatedField<MainQuestDetail> eliteList_ = new RepeatedField<MainQuestDetail>();

	public const int DoubleAwardFieldNumber = 4;

	private int doubleAward_;

	public const int CombatCountFieldNumber = 5;

	private int combatCount_;

	public const int BuyCombatFieldNumber = 6;

	private int buyCombat_;

	public const int DailyCountFieldNumber = 7;

	private int dailyCount_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<EliteQuestInfo> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeEliteQuestInfoReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int EliteLevel
	{
		get
		{
			return eliteLevel_;
		}
		set
		{
			eliteLevel_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int NextLevel
	{
		get
		{
			return nextLevel_;
		}
		set
		{
			nextLevel_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<MainQuestDetail> EliteList => eliteList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int DoubleAward
	{
		get
		{
			return doubleAward_;
		}
		set
		{
			doubleAward_ = value;
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
	public int BuyCombat
	{
		get
		{
			return buyCombat_;
		}
		set
		{
			buyCombat_ = value;
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
	public EliteQuestInfo()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public EliteQuestInfo(EliteQuestInfo other)
		: this()
	{
		eliteLevel_ = other.eliteLevel_;
		nextLevel_ = other.nextLevel_;
		eliteList_ = other.eliteList_.Clone();
		doubleAward_ = other.doubleAward_;
		combatCount_ = other.combatCount_;
		buyCombat_ = other.buyCombat_;
		dailyCount_ = other.dailyCount_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public EliteQuestInfo Clone()
	{
		return new EliteQuestInfo(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as EliteQuestInfo);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(EliteQuestInfo other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (EliteLevel != other.EliteLevel)
		{
			return false;
		}
		if (NextLevel != other.NextLevel)
		{
			return false;
		}
		if (!eliteList_.Equals(other.eliteList_))
		{
			return false;
		}
		if (DoubleAward != other.DoubleAward)
		{
			return false;
		}
		if (CombatCount != other.CombatCount)
		{
			return false;
		}
		if (BuyCombat != other.BuyCombat)
		{
			return false;
		}
		if (DailyCount != other.DailyCount)
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
		if (EliteLevel != 0)
		{
			num ^= EliteLevel.GetHashCode();
		}
		if (NextLevel != 0)
		{
			num ^= NextLevel.GetHashCode();
		}
		num ^= ((object)eliteList_).GetHashCode();
		if (DoubleAward != 0)
		{
			num ^= DoubleAward.GetHashCode();
		}
		if (CombatCount != 0)
		{
			num ^= CombatCount.GetHashCode();
		}
		if (BuyCombat != 0)
		{
			num ^= BuyCombat.GetHashCode();
		}
		if (DailyCount != 0)
		{
			num ^= DailyCount.GetHashCode();
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
		if (EliteLevel != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteInt32(EliteLevel);
		}
		if (NextLevel != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteInt32(NextLevel);
		}
		eliteList_.WriteTo(ref output, _repeated_eliteList_codec);
		if (DoubleAward != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)32);
			((WriteContext)(ref output)).WriteInt32(DoubleAward);
		}
		if (CombatCount != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)40);
			((WriteContext)(ref output)).WriteInt32(CombatCount);
		}
		if (BuyCombat != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)48);
			((WriteContext)(ref output)).WriteInt32(BuyCombat);
		}
		if (DailyCount != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)56);
			((WriteContext)(ref output)).WriteInt32(DailyCount);
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
		if (EliteLevel != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(EliteLevel);
		}
		if (NextLevel != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(NextLevel);
		}
		num += eliteList_.CalculateSize(_repeated_eliteList_codec);
		if (DoubleAward != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(DoubleAward);
		}
		if (CombatCount != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(CombatCount);
		}
		if (BuyCombat != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(BuyCombat);
		}
		if (DailyCount != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(DailyCount);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(EliteQuestInfo other)
	{
		if (other != null)
		{
			if (other.EliteLevel != 0)
			{
				EliteLevel = other.EliteLevel;
			}
			if (other.NextLevel != 0)
			{
				NextLevel = other.NextLevel;
			}
			eliteList_.Add((IEnumerable<MainQuestDetail>)other.eliteList_);
			if (other.DoubleAward != 0)
			{
				DoubleAward = other.DoubleAward;
			}
			if (other.CombatCount != 0)
			{
				CombatCount = other.CombatCount;
			}
			if (other.BuyCombat != 0)
			{
				BuyCombat = other.BuyCombat;
			}
			if (other.DailyCount != 0)
			{
				DailyCount = other.DailyCount;
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
				EliteLevel = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 16u:
				NextLevel = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 26u:
				eliteList_.AddEntriesFrom(ref input, _repeated_eliteList_codec);
				break;
			case 32u:
				DoubleAward = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 40u:
				CombatCount = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 48u:
				BuyCombat = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 56u:
				DailyCount = ((ParseContext)(ref input)).ReadInt32();
				break;
			}
		}
	}
}
