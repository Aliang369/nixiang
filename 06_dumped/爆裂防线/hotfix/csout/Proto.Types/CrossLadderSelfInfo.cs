using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class CrossLadderSelfInfo : IMessage<CrossLadderSelfInfo>, IMessage, IEquatable<CrossLadderSelfInfo>, IDeepCloneable<CrossLadderSelfInfo>, IBufferMessage
{
	private static readonly MessageParser<CrossLadderSelfInfo> _parser = new MessageParser<CrossLadderSelfInfo>((Func<CrossLadderSelfInfo>)(() => new CrossLadderSelfInfo()));

	private UnknownFieldSet _unknownFields;

	public const int CombatCountFieldNumber = 1;

	private int combatCount_;

	public const int BuyCombatFieldNumber = 2;

	private int buyCombat_;

	public const int DailyCountFieldNumber = 3;

	private int dailyCount_;

	public const int TargetAwardFieldNumber = 4;

	private int targetAward_;

	public const int SeasonAwardFieldNumber = 5;

	private CrossLadderSeasonAward seasonAward_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<CrossLadderSelfInfo> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeCrossLadderSelfInfoReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

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
	public int TargetAward
	{
		get
		{
			return targetAward_;
		}
		set
		{
			targetAward_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public CrossLadderSeasonAward SeasonAward
	{
		get
		{
			return seasonAward_;
		}
		set
		{
			seasonAward_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public CrossLadderSelfInfo()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public CrossLadderSelfInfo(CrossLadderSelfInfo other)
		: this()
	{
		combatCount_ = other.combatCount_;
		buyCombat_ = other.buyCombat_;
		dailyCount_ = other.dailyCount_;
		targetAward_ = other.targetAward_;
		seasonAward_ = ((other.seasonAward_ != null) ? other.seasonAward_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public CrossLadderSelfInfo Clone()
	{
		return new CrossLadderSelfInfo(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as CrossLadderSelfInfo);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(CrossLadderSelfInfo other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
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
		if (TargetAward != other.TargetAward)
		{
			return false;
		}
		if (!object.Equals(SeasonAward, other.SeasonAward))
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
		if (TargetAward != 0)
		{
			num ^= TargetAward.GetHashCode();
		}
		if (seasonAward_ != null)
		{
			num ^= SeasonAward.GetHashCode();
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
		if (CombatCount != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteInt32(CombatCount);
		}
		if (BuyCombat != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteInt32(BuyCombat);
		}
		if (DailyCount != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)24);
			((WriteContext)(ref output)).WriteInt32(DailyCount);
		}
		if (TargetAward != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)32);
			((WriteContext)(ref output)).WriteInt32(TargetAward);
		}
		if (seasonAward_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)42);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)SeasonAward);
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
		if (TargetAward != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(TargetAward);
		}
		if (seasonAward_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)SeasonAward);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(CrossLadderSelfInfo other)
	{
		if (other == null)
		{
			return;
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
		if (other.TargetAward != 0)
		{
			TargetAward = other.TargetAward;
		}
		if (other.seasonAward_ != null)
		{
			if (seasonAward_ == null)
			{
				SeasonAward = new CrossLadderSeasonAward();
			}
			SeasonAward.MergeFrom(other.SeasonAward);
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
				CombatCount = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 16u:
				BuyCombat = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 24u:
				DailyCount = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 32u:
				TargetAward = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 42u:
				if (seasonAward_ == null)
				{
					SeasonAward = new CrossLadderSeasonAward();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)SeasonAward);
				break;
			}
		}
	}
}
