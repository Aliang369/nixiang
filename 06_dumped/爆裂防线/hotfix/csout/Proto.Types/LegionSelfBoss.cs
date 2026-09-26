using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class LegionSelfBoss : IMessage<LegionSelfBoss>, IMessage, IEquatable<LegionSelfBoss>, IDeepCloneable<LegionSelfBoss>, IBufferMessage
{
	private static readonly MessageParser<LegionSelfBoss> _parser = new MessageParser<LegionSelfBoss>((Func<LegionSelfBoss>)(() => new LegionSelfBoss()));

	private UnknownFieldSet _unknownFields;

	public const int CombatCountFieldNumber = 1;

	private int combatCount_;

	public const int BuyCombatFieldNumber = 2;

	private int buyCombat_;

	public const int DailyCountFieldNumber = 3;

	private int dailyCount_;

	public const int PassAwardFieldNumber = 4;

	private static readonly FieldCodec<int> _repeated_passAward_codec = FieldCodec.ForInt32(34u);

	private readonly RepeatedField<int> passAward_ = new RepeatedField<int>();

	public const int MaxKillFieldNumber = 5;

	private int maxKill_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<LegionSelfBoss> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeLegionSelfBossReflection.Descriptor.MessageTypes[0];

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
	public RepeatedField<int> PassAward => passAward_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int MaxKill
	{
		get
		{
			return maxKill_;
		}
		set
		{
			maxKill_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public LegionSelfBoss()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public LegionSelfBoss(LegionSelfBoss other)
		: this()
	{
		combatCount_ = other.combatCount_;
		buyCombat_ = other.buyCombat_;
		dailyCount_ = other.dailyCount_;
		passAward_ = other.passAward_.Clone();
		maxKill_ = other.maxKill_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public LegionSelfBoss Clone()
	{
		return new LegionSelfBoss(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as LegionSelfBoss);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(LegionSelfBoss other)
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
		if (!passAward_.Equals(other.passAward_))
		{
			return false;
		}
		if (MaxKill != other.MaxKill)
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
		num ^= ((object)passAward_).GetHashCode();
		if (MaxKill != 0)
		{
			num ^= MaxKill.GetHashCode();
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
		passAward_.WriteTo(ref output, _repeated_passAward_codec);
		if (MaxKill != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)40);
			((WriteContext)(ref output)).WriteInt32(MaxKill);
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
		num += passAward_.CalculateSize(_repeated_passAward_codec);
		if (MaxKill != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(MaxKill);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(LegionSelfBoss other)
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
			passAward_.Add((IEnumerable<int>)other.passAward_);
			if (other.MaxKill != 0)
			{
				MaxKill = other.MaxKill;
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
			case 34u:
				passAward_.AddEntriesFrom(ref input, _repeated_passAward_codec);
				break;
			case 40u:
				MaxKill = ((ParseContext)(ref input)).ReadInt32();
				break;
			}
		}
	}
}
