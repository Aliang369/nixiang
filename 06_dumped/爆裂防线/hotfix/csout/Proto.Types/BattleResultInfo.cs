using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class BattleResultInfo : IMessage<BattleResultInfo>, IMessage, IEquatable<BattleResultInfo>, IDeepCloneable<BattleResultInfo>, IBufferMessage
{
	private static readonly MessageParser<BattleResultInfo> _parser = new MessageParser<BattleResultInfo>((Func<BattleResultInfo>)(() => new BattleResultInfo()));

	private UnknownFieldSet _unknownFields;

	public const int UUIDFieldNumber = 1;

	private string uUID_ = "";

	public const int BattleTypeFieldNumber = 2;

	private EBattleFuncType battleType_;

	public const int ResultScoreFieldNumber = 3;

	private EBattleResultScore resultScore_;

	public const int TimeUsedFieldNumber = 4;

	private int timeUsed_;

	public const int AttackerTeamInfoFieldNumber = 5;

	private BattleResultSideTeamInfo attackerTeamInfo_;

	public const int DefenderTeamInfoFieldNumber = 6;

	private BattleResultSideTeamInfo defenderTeamInfo_;

	public const int DamageWatchInfoFieldNumber = 7;

	private BattleResultDamageWatchInfo damageWatchInfo_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<BattleResultInfo> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeBattleResultInfoReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public string UUID
	{
		get
		{
			return uUID_;
		}
		set
		{
			uUID_ = ProtoPreconditions.CheckNotNull<string>(value, "value");
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public EBattleFuncType BattleType
	{
		get
		{
			return battleType_;
		}
		set
		{
			battleType_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public EBattleResultScore ResultScore
	{
		get
		{
			return resultScore_;
		}
		set
		{
			resultScore_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int TimeUsed
	{
		get
		{
			return timeUsed_;
		}
		set
		{
			timeUsed_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public BattleResultSideTeamInfo AttackerTeamInfo
	{
		get
		{
			return attackerTeamInfo_;
		}
		set
		{
			attackerTeamInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public BattleResultSideTeamInfo DefenderTeamInfo
	{
		get
		{
			return defenderTeamInfo_;
		}
		set
		{
			defenderTeamInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public BattleResultDamageWatchInfo DamageWatchInfo
	{
		get
		{
			return damageWatchInfo_;
		}
		set
		{
			damageWatchInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public BattleResultInfo()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public BattleResultInfo(BattleResultInfo other)
		: this()
	{
		uUID_ = other.uUID_;
		battleType_ = other.battleType_;
		resultScore_ = other.resultScore_;
		timeUsed_ = other.timeUsed_;
		attackerTeamInfo_ = ((other.attackerTeamInfo_ != null) ? other.attackerTeamInfo_.Clone() : null);
		defenderTeamInfo_ = ((other.defenderTeamInfo_ != null) ? other.defenderTeamInfo_.Clone() : null);
		damageWatchInfo_ = ((other.damageWatchInfo_ != null) ? other.damageWatchInfo_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public BattleResultInfo Clone()
	{
		return new BattleResultInfo(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as BattleResultInfo);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(BattleResultInfo other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (UUID != other.UUID)
		{
			return false;
		}
		if (BattleType != other.BattleType)
		{
			return false;
		}
		if (ResultScore != other.ResultScore)
		{
			return false;
		}
		if (TimeUsed != other.TimeUsed)
		{
			return false;
		}
		if (!object.Equals(AttackerTeamInfo, other.AttackerTeamInfo))
		{
			return false;
		}
		if (!object.Equals(DefenderTeamInfo, other.DefenderTeamInfo))
		{
			return false;
		}
		if (!object.Equals(DamageWatchInfo, other.DamageWatchInfo))
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
		if (UUID.Length != 0)
		{
			num ^= UUID.GetHashCode();
		}
		if (BattleType != EBattleFuncType.EmptyValue)
		{
			num ^= BattleType.GetHashCode();
		}
		if (ResultScore != EBattleResultScore.Fail)
		{
			num ^= ResultScore.GetHashCode();
		}
		if (TimeUsed != 0)
		{
			num ^= TimeUsed.GetHashCode();
		}
		if (attackerTeamInfo_ != null)
		{
			num ^= AttackerTeamInfo.GetHashCode();
		}
		if (defenderTeamInfo_ != null)
		{
			num ^= DefenderTeamInfo.GetHashCode();
		}
		if (damageWatchInfo_ != null)
		{
			num ^= DamageWatchInfo.GetHashCode();
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
		if (UUID.Length != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)10);
			((WriteContext)(ref output)).WriteString(UUID);
		}
		if (BattleType != EBattleFuncType.EmptyValue)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteEnum((int)BattleType);
		}
		if (ResultScore != EBattleResultScore.Fail)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)24);
			((WriteContext)(ref output)).WriteEnum((int)ResultScore);
		}
		if (TimeUsed != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)32);
			((WriteContext)(ref output)).WriteInt32(TimeUsed);
		}
		if (attackerTeamInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)42);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)AttackerTeamInfo);
		}
		if (defenderTeamInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)50);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)DefenderTeamInfo);
		}
		if (damageWatchInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)58);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)DamageWatchInfo);
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
		if (UUID.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(UUID);
		}
		if (BattleType != EBattleFuncType.EmptyValue)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)BattleType);
		}
		if (ResultScore != EBattleResultScore.Fail)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)ResultScore);
		}
		if (TimeUsed != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(TimeUsed);
		}
		if (attackerTeamInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)AttackerTeamInfo);
		}
		if (defenderTeamInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)DefenderTeamInfo);
		}
		if (damageWatchInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)DamageWatchInfo);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(BattleResultInfo other)
	{
		if (other == null)
		{
			return;
		}
		if (other.UUID.Length != 0)
		{
			UUID = other.UUID;
		}
		if (other.BattleType != EBattleFuncType.EmptyValue)
		{
			BattleType = other.BattleType;
		}
		if (other.ResultScore != EBattleResultScore.Fail)
		{
			ResultScore = other.ResultScore;
		}
		if (other.TimeUsed != 0)
		{
			TimeUsed = other.TimeUsed;
		}
		if (other.attackerTeamInfo_ != null)
		{
			if (attackerTeamInfo_ == null)
			{
				AttackerTeamInfo = new BattleResultSideTeamInfo();
			}
			AttackerTeamInfo.MergeFrom(other.AttackerTeamInfo);
		}
		if (other.defenderTeamInfo_ != null)
		{
			if (defenderTeamInfo_ == null)
			{
				DefenderTeamInfo = new BattleResultSideTeamInfo();
			}
			DefenderTeamInfo.MergeFrom(other.DefenderTeamInfo);
		}
		if (other.damageWatchInfo_ != null)
		{
			if (damageWatchInfo_ == null)
			{
				DamageWatchInfo = new BattleResultDamageWatchInfo();
			}
			DamageWatchInfo.MergeFrom(other.DamageWatchInfo);
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
				UUID = ((ParseContext)(ref input)).ReadString();
				break;
			case 16u:
				BattleType = (EBattleFuncType)((ParseContext)(ref input)).ReadEnum();
				break;
			case 24u:
				ResultScore = (EBattleResultScore)((ParseContext)(ref input)).ReadEnum();
				break;
			case 32u:
				TimeUsed = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 42u:
				if (attackerTeamInfo_ == null)
				{
					AttackerTeamInfo = new BattleResultSideTeamInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)AttackerTeamInfo);
				break;
			case 50u:
				if (defenderTeamInfo_ == null)
				{
					DefenderTeamInfo = new BattleResultSideTeamInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)DefenderTeamInfo);
				break;
			case 58u:
				if (damageWatchInfo_ == null)
				{
					DamageWatchInfo = new BattleResultDamageWatchInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)DamageWatchInfo);
				break;
			}
		}
	}
}
