using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class EndlessHordeInfo : IMessage<EndlessHordeInfo>, IMessage, IEquatable<EndlessHordeInfo>, IDeepCloneable<EndlessHordeInfo>, IBufferMessage
{
	private static readonly MessageParser<EndlessHordeInfo> _parser = new MessageParser<EndlessHordeInfo>((Func<EndlessHordeInfo>)(() => new EndlessHordeInfo()));

	private UnknownFieldSet _unknownFields;

	public const int LevelFieldNumber = 1;

	private int level_;

	public const int PassDistanceFieldNumber = 2;

	private int passDistance_;

	public const int CombatCountFieldNumber = 3;

	private int combatCount_;

	public const int BuyCombatFieldNumber = 4;

	private int buyCombat_;

	public const int DailyCountFieldNumber = 5;

	private int dailyCount_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<EndlessHordeInfo> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeEndlessHordeInfoReflection.Descriptor.MessageTypes[0];

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
	public int PassDistance
	{
		get
		{
			return passDistance_;
		}
		set
		{
			passDistance_ = value;
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
	public EndlessHordeInfo()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public EndlessHordeInfo(EndlessHordeInfo other)
		: this()
	{
		level_ = other.level_;
		passDistance_ = other.passDistance_;
		combatCount_ = other.combatCount_;
		buyCombat_ = other.buyCombat_;
		dailyCount_ = other.dailyCount_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public EndlessHordeInfo Clone()
	{
		return new EndlessHordeInfo(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as EndlessHordeInfo);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(EndlessHordeInfo other)
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
		if (PassDistance != other.PassDistance)
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
		if (Level != 0)
		{
			num ^= Level.GetHashCode();
		}
		if (PassDistance != 0)
		{
			num ^= PassDistance.GetHashCode();
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
		if (Level != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteInt32(Level);
		}
		if (PassDistance != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteInt32(PassDistance);
		}
		if (CombatCount != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)24);
			((WriteContext)(ref output)).WriteInt32(CombatCount);
		}
		if (BuyCombat != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)32);
			((WriteContext)(ref output)).WriteInt32(BuyCombat);
		}
		if (DailyCount != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)40);
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
		if (Level != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(Level);
		}
		if (PassDistance != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(PassDistance);
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
	public void MergeFrom(EndlessHordeInfo other)
	{
		if (other != null)
		{
			if (other.Level != 0)
			{
				Level = other.Level;
			}
			if (other.PassDistance != 0)
			{
				PassDistance = other.PassDistance;
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
				Level = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 16u:
				PassDistance = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 24u:
				CombatCount = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 32u:
				BuyCombat = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 40u:
				DailyCount = ((ParseContext)(ref input)).ReadInt32();
				break;
			}
		}
	}
}
