using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class WorldBossSelfInfo : IMessage<WorldBossSelfInfo>, IMessage, IEquatable<WorldBossSelfInfo>, IDeepCloneable<WorldBossSelfInfo>, IBufferMessage
{
	private static readonly MessageParser<WorldBossSelfInfo> _parser = new MessageParser<WorldBossSelfInfo>((Func<WorldBossSelfInfo>)(() => new WorldBossSelfInfo()));

	private UnknownFieldSet _unknownFields;

	public const int CombatCountFieldNumber = 1;

	private int combatCount_;

	public const int BuyCombatFieldNumber = 2;

	private int buyCombat_;

	public const int DailyCountFieldNumber = 3;

	private int dailyCount_;

	public const int MaxHurtFieldNumber = 4;

	private long maxHurt_;

	public const int ShowStartFieldNumber = 5;

	private long showStart_;

	public const int ShowEndFieldNumber = 6;

	private long showEnd_;

	public const int StartTmFieldNumber = 7;

	private long startTm_;

	public const int EndTmFieldNumber = 8;

	private long endTm_;

	public const int MonthRefreshFieldNumber = 9;

	private long monthRefresh_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<WorldBossSelfInfo> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeWorldBossSelfInfoReflection.Descriptor.MessageTypes[0];

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
	public long MaxHurt
	{
		get
		{
			return maxHurt_;
		}
		set
		{
			maxHurt_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public long ShowStart
	{
		get
		{
			return showStart_;
		}
		set
		{
			showStart_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public long ShowEnd
	{
		get
		{
			return showEnd_;
		}
		set
		{
			showEnd_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public long StartTm
	{
		get
		{
			return startTm_;
		}
		set
		{
			startTm_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public long EndTm
	{
		get
		{
			return endTm_;
		}
		set
		{
			endTm_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public long MonthRefresh
	{
		get
		{
			return monthRefresh_;
		}
		set
		{
			monthRefresh_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public WorldBossSelfInfo()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public WorldBossSelfInfo(WorldBossSelfInfo other)
		: this()
	{
		combatCount_ = other.combatCount_;
		buyCombat_ = other.buyCombat_;
		dailyCount_ = other.dailyCount_;
		maxHurt_ = other.maxHurt_;
		showStart_ = other.showStart_;
		showEnd_ = other.showEnd_;
		startTm_ = other.startTm_;
		endTm_ = other.endTm_;
		monthRefresh_ = other.monthRefresh_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public WorldBossSelfInfo Clone()
	{
		return new WorldBossSelfInfo(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as WorldBossSelfInfo);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(WorldBossSelfInfo other)
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
		if (MaxHurt != other.MaxHurt)
		{
			return false;
		}
		if (ShowStart != other.ShowStart)
		{
			return false;
		}
		if (ShowEnd != other.ShowEnd)
		{
			return false;
		}
		if (StartTm != other.StartTm)
		{
			return false;
		}
		if (EndTm != other.EndTm)
		{
			return false;
		}
		if (MonthRefresh != other.MonthRefresh)
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
		if (MaxHurt != 0L)
		{
			num ^= MaxHurt.GetHashCode();
		}
		if (ShowStart != 0L)
		{
			num ^= ShowStart.GetHashCode();
		}
		if (ShowEnd != 0L)
		{
			num ^= ShowEnd.GetHashCode();
		}
		if (StartTm != 0L)
		{
			num ^= StartTm.GetHashCode();
		}
		if (EndTm != 0L)
		{
			num ^= EndTm.GetHashCode();
		}
		if (MonthRefresh != 0L)
		{
			num ^= MonthRefresh.GetHashCode();
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
		if (MaxHurt != 0L)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)32);
			((WriteContext)(ref output)).WriteInt64(MaxHurt);
		}
		if (ShowStart != 0L)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)40);
			((WriteContext)(ref output)).WriteInt64(ShowStart);
		}
		if (ShowEnd != 0L)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)48);
			((WriteContext)(ref output)).WriteInt64(ShowEnd);
		}
		if (StartTm != 0L)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)56);
			((WriteContext)(ref output)).WriteInt64(StartTm);
		}
		if (EndTm != 0L)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)64);
			((WriteContext)(ref output)).WriteInt64(EndTm);
		}
		if (MonthRefresh != 0L)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)72);
			((WriteContext)(ref output)).WriteInt64(MonthRefresh);
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
		if (MaxHurt != 0L)
		{
			num += 1 + CodedOutputStream.ComputeInt64Size(MaxHurt);
		}
		if (ShowStart != 0L)
		{
			num += 1 + CodedOutputStream.ComputeInt64Size(ShowStart);
		}
		if (ShowEnd != 0L)
		{
			num += 1 + CodedOutputStream.ComputeInt64Size(ShowEnd);
		}
		if (StartTm != 0L)
		{
			num += 1 + CodedOutputStream.ComputeInt64Size(StartTm);
		}
		if (EndTm != 0L)
		{
			num += 1 + CodedOutputStream.ComputeInt64Size(EndTm);
		}
		if (MonthRefresh != 0L)
		{
			num += 1 + CodedOutputStream.ComputeInt64Size(MonthRefresh);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(WorldBossSelfInfo other)
	{
		if (other != null)
		{
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
			if (other.MaxHurt != 0L)
			{
				MaxHurt = other.MaxHurt;
			}
			if (other.ShowStart != 0L)
			{
				ShowStart = other.ShowStart;
			}
			if (other.ShowEnd != 0L)
			{
				ShowEnd = other.ShowEnd;
			}
			if (other.StartTm != 0L)
			{
				StartTm = other.StartTm;
			}
			if (other.EndTm != 0L)
			{
				EndTm = other.EndTm;
			}
			if (other.MonthRefresh != 0L)
			{
				MonthRefresh = other.MonthRefresh;
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
				CombatCount = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 16u:
				BuyCombat = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 24u:
				DailyCount = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 32u:
				MaxHurt = ((ParseContext)(ref input)).ReadInt64();
				break;
			case 40u:
				ShowStart = ((ParseContext)(ref input)).ReadInt64();
				break;
			case 48u:
				ShowEnd = ((ParseContext)(ref input)).ReadInt64();
				break;
			case 56u:
				StartTm = ((ParseContext)(ref input)).ReadInt64();
				break;
			case 64u:
				EndTm = ((ParseContext)(ref input)).ReadInt64();
				break;
			case 72u:
				MonthRefresh = ((ParseContext)(ref input)).ReadInt64();
				break;
			}
		}
	}
}
