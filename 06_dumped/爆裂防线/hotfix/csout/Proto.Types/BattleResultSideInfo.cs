using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class BattleResultSideInfo : IMessage<BattleResultSideInfo>, IMessage, IEquatable<BattleResultSideInfo>, IDeepCloneable<BattleResultSideInfo>, IBufferMessage
{
	private static readonly MessageParser<BattleResultSideInfo> _parser = new MessageParser<BattleResultSideInfo>((Func<BattleResultSideInfo>)(() => new BattleResultSideInfo()));

	private UnknownFieldSet _unknownFields;

	public const int LevelFieldNumber = 1;

	private int level_;

	public const int RoundFieldNumber = 2;

	private int round_;

	public const int DefensePctFieldNumber = 3;

	private float defensePct_;

	public const int KillAllFieldNumber = 4;

	private EBoolState killAll_;

	public const int KillBossFieldNumber = 5;

	private EBoolState killBoss_;

	public const int KillCountFieldNumber = 6;

	private int killCount_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<BattleResultSideInfo> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeBattleResultSideInfoReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

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
	public int Round
	{
		get
		{
			return round_;
		}
		set
		{
			round_ = value;
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
	public EBoolState KillAll
	{
		get
		{
			return killAll_;
		}
		set
		{
			killAll_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public EBoolState KillBoss
	{
		get
		{
			return killBoss_;
		}
		set
		{
			killBoss_ = value;
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
	public BattleResultSideInfo()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public BattleResultSideInfo(BattleResultSideInfo other)
		: this()
	{
		level_ = other.level_;
		round_ = other.round_;
		defensePct_ = other.defensePct_;
		killAll_ = other.killAll_;
		killBoss_ = other.killBoss_;
		killCount_ = other.killCount_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public BattleResultSideInfo Clone()
	{
		return new BattleResultSideInfo(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as BattleResultSideInfo);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(BattleResultSideInfo other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (Level != other.Level)
		{
			return false;
		}
		if (Round != other.Round)
		{
			return false;
		}
		if (!ProtobufEqualityComparers.BitwiseSingleEqualityComparer.Equals(DefensePct, other.DefensePct))
		{
			return false;
		}
		if (KillAll != other.KillAll)
		{
			return false;
		}
		if (KillBoss != other.KillBoss)
		{
			return false;
		}
		if (KillCount != other.KillCount)
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
		if (Level != 0)
		{
			num ^= Level.GetHashCode();
		}
		if (Round != 0)
		{
			num ^= Round.GetHashCode();
		}
		if (DefensePct != 0f)
		{
			num ^= ProtobufEqualityComparers.BitwiseSingleEqualityComparer.GetHashCode(DefensePct);
		}
		if (KillAll != EBoolState.False)
		{
			num ^= KillAll.GetHashCode();
		}
		if (KillBoss != EBoolState.False)
		{
			num ^= KillBoss.GetHashCode();
		}
		if (KillCount != 0)
		{
			num ^= KillCount.GetHashCode();
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
		if (Level != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteInt32(Level);
		}
		if (Round != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteInt32(Round);
		}
		if (DefensePct != 0f)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)29);
			((WriteContext)(ref output)).WriteFloat(DefensePct);
		}
		if (KillAll != EBoolState.False)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)32);
			((WriteContext)(ref output)).WriteEnum((int)KillAll);
		}
		if (KillBoss != EBoolState.False)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)40);
			((WriteContext)(ref output)).WriteEnum((int)KillBoss);
		}
		if (KillCount != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)48);
			((WriteContext)(ref output)).WriteInt32(KillCount);
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
		if (Level != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(Level);
		}
		if (Round != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(Round);
		}
		if (DefensePct != 0f)
		{
			num += 5;
		}
		if (KillAll != EBoolState.False)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)KillAll);
		}
		if (KillBoss != EBoolState.False)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)KillBoss);
		}
		if (KillCount != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(KillCount);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(BattleResultSideInfo other)
	{
		if (other != null)
		{
			if (other.Level != 0)
			{
				Level = other.Level;
			}
			if (other.Round != 0)
			{
				Round = other.Round;
			}
			if (other.DefensePct != 0f)
			{
				DefensePct = other.DefensePct;
			}
			if (other.KillAll != EBoolState.False)
			{
				KillAll = other.KillAll;
			}
			if (other.KillBoss != EBoolState.False)
			{
				KillBoss = other.KillBoss;
			}
			if (other.KillCount != 0)
			{
				KillCount = other.KillCount;
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
				Level = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 16u:
				Round = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 29u:
				DefensePct = ((ParseContext)(ref input)).ReadFloat();
				break;
			case 32u:
				KillAll = (EBoolState)((ParseContext)(ref input)).ReadEnum();
				break;
			case 40u:
				KillBoss = (EBoolState)((ParseContext)(ref input)).ReadEnum();
				break;
			case 48u:
				KillCount = ((ParseContext)(ref input)).ReadInt32();
				break;
			}
		}
	}
}
