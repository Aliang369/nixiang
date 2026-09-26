using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class BattleInfo : IMessage<BattleInfo>, IMessage, IEquatable<BattleInfo>, IDeepCloneable<BattleInfo>, IBufferMessage
{
	private static readonly MessageParser<BattleInfo> _parser = new MessageParser<BattleInfo>((Func<BattleInfo>)(() => new BattleInfo()));

	private UnknownFieldSet _unknownFields;

	public const int BattleBaseInfoFieldNumber = 1;

	private BattleBaseInfo battleBaseInfo_;

	public const int AttackerTeamInfoFieldNumber = 2;

	private BattleSideTeamInfo attackerTeamInfo_;

	public const int DefenderTeamInfoFieldNumber = 3;

	private BattleSideTeamInfo defenderTeamInfo_;

	public const int MonsterInfoFieldNumber = 4;

	private BattleMonsterInfo monsterInfo_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<BattleInfo> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeBattleInfoReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public BattleBaseInfo BattleBaseInfo
	{
		get
		{
			return battleBaseInfo_;
		}
		set
		{
			battleBaseInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public BattleSideTeamInfo AttackerTeamInfo
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
	public BattleSideTeamInfo DefenderTeamInfo
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
	public BattleMonsterInfo MonsterInfo
	{
		get
		{
			return monsterInfo_;
		}
		set
		{
			monsterInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public BattleInfo()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public BattleInfo(BattleInfo other)
		: this()
	{
		battleBaseInfo_ = ((other.battleBaseInfo_ != null) ? other.battleBaseInfo_.Clone() : null);
		attackerTeamInfo_ = ((other.attackerTeamInfo_ != null) ? other.attackerTeamInfo_.Clone() : null);
		defenderTeamInfo_ = ((other.defenderTeamInfo_ != null) ? other.defenderTeamInfo_.Clone() : null);
		monsterInfo_ = ((other.monsterInfo_ != null) ? other.monsterInfo_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public BattleInfo Clone()
	{
		return new BattleInfo(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as BattleInfo);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(BattleInfo other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!object.Equals(BattleBaseInfo, other.BattleBaseInfo))
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
		if (!object.Equals(MonsterInfo, other.MonsterInfo))
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
		if (battleBaseInfo_ != null)
		{
			num ^= BattleBaseInfo.GetHashCode();
		}
		if (attackerTeamInfo_ != null)
		{
			num ^= AttackerTeamInfo.GetHashCode();
		}
		if (defenderTeamInfo_ != null)
		{
			num ^= DefenderTeamInfo.GetHashCode();
		}
		if (monsterInfo_ != null)
		{
			num ^= MonsterInfo.GetHashCode();
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
		if (battleBaseInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)10);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)BattleBaseInfo);
		}
		if (attackerTeamInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)18);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)AttackerTeamInfo);
		}
		if (defenderTeamInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)26);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)DefenderTeamInfo);
		}
		if (monsterInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)34);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)MonsterInfo);
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
		if (battleBaseInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)BattleBaseInfo);
		}
		if (attackerTeamInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)AttackerTeamInfo);
		}
		if (defenderTeamInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)DefenderTeamInfo);
		}
		if (monsterInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)MonsterInfo);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(BattleInfo other)
	{
		if (other == null)
		{
			return;
		}
		if (other.battleBaseInfo_ != null)
		{
			if (battleBaseInfo_ == null)
			{
				BattleBaseInfo = new BattleBaseInfo();
			}
			BattleBaseInfo.MergeFrom(other.BattleBaseInfo);
		}
		if (other.attackerTeamInfo_ != null)
		{
			if (attackerTeamInfo_ == null)
			{
				AttackerTeamInfo = new BattleSideTeamInfo();
			}
			AttackerTeamInfo.MergeFrom(other.AttackerTeamInfo);
		}
		if (other.defenderTeamInfo_ != null)
		{
			if (defenderTeamInfo_ == null)
			{
				DefenderTeamInfo = new BattleSideTeamInfo();
			}
			DefenderTeamInfo.MergeFrom(other.DefenderTeamInfo);
		}
		if (other.monsterInfo_ != null)
		{
			if (monsterInfo_ == null)
			{
				MonsterInfo = new BattleMonsterInfo();
			}
			MonsterInfo.MergeFrom(other.MonsterInfo);
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
				if (battleBaseInfo_ == null)
				{
					BattleBaseInfo = new BattleBaseInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)BattleBaseInfo);
				break;
			case 18u:
				if (attackerTeamInfo_ == null)
				{
					AttackerTeamInfo = new BattleSideTeamInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)AttackerTeamInfo);
				break;
			case 26u:
				if (defenderTeamInfo_ == null)
				{
					DefenderTeamInfo = new BattleSideTeamInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)DefenderTeamInfo);
				break;
			case 34u:
				if (monsterInfo_ == null)
				{
					MonsterInfo = new BattleMonsterInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)MonsterInfo);
				break;
			}
		}
	}
}
