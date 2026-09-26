using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class WorldLairDetail : IMessage<WorldLairDetail>, IMessage, IEquatable<WorldLairDetail>, IDeepCloneable<WorldLairDetail>, IBufferMessage
{
	private static readonly MessageParser<WorldLairDetail> _parser = new MessageParser<WorldLairDetail>((Func<WorldLairDetail>)(() => new WorldLairDetail()));

	private UnknownFieldSet _unknownFields;

	public const int UUIDFieldNumber = 1;

	private string uUID_ = "";

	public const int LairIdFieldNumber = 2;

	private int lairId_;

	public const int LevelFieldNumber = 3;

	private int level_;

	public const int KillCountFieldNumber = 4;

	private int killCount_;

	public const int BestKillFieldNumber = 5;

	private int bestKill_;

	public const int PassTargetFieldNumber = 6;

	private int passTarget_;

	public const int PassStateFieldNumber = 7;

	private EBoolState passState_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<WorldLairDetail> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeWorldLairDetailReflection.Descriptor.MessageTypes[0];

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
	public int LairId
	{
		get
		{
			return lairId_;
		}
		set
		{
			lairId_ = value;
		}
	}

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
	public int BestKill
	{
		get
		{
			return bestKill_;
		}
		set
		{
			bestKill_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int PassTarget
	{
		get
		{
			return passTarget_;
		}
		set
		{
			passTarget_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public EBoolState PassState
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
	public WorldLairDetail()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public WorldLairDetail(WorldLairDetail other)
		: this()
	{
		uUID_ = other.uUID_;
		lairId_ = other.lairId_;
		level_ = other.level_;
		killCount_ = other.killCount_;
		bestKill_ = other.bestKill_;
		passTarget_ = other.passTarget_;
		passState_ = other.passState_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public WorldLairDetail Clone()
	{
		return new WorldLairDetail(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as WorldLairDetail);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(WorldLairDetail other)
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
		if (LairId != other.LairId)
		{
			return false;
		}
		if (Level != other.Level)
		{
			return false;
		}
		if (KillCount != other.KillCount)
		{
			return false;
		}
		if (BestKill != other.BestKill)
		{
			return false;
		}
		if (PassTarget != other.PassTarget)
		{
			return false;
		}
		if (PassState != other.PassState)
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
		if (LairId != 0)
		{
			num ^= LairId.GetHashCode();
		}
		if (Level != 0)
		{
			num ^= Level.GetHashCode();
		}
		if (KillCount != 0)
		{
			num ^= KillCount.GetHashCode();
		}
		if (BestKill != 0)
		{
			num ^= BestKill.GetHashCode();
		}
		if (PassTarget != 0)
		{
			num ^= PassTarget.GetHashCode();
		}
		if (PassState != EBoolState.False)
		{
			num ^= PassState.GetHashCode();
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
		if (LairId != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteInt32(LairId);
		}
		if (Level != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)24);
			((WriteContext)(ref output)).WriteInt32(Level);
		}
		if (KillCount != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)32);
			((WriteContext)(ref output)).WriteInt32(KillCount);
		}
		if (BestKill != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)40);
			((WriteContext)(ref output)).WriteInt32(BestKill);
		}
		if (PassTarget != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)48);
			((WriteContext)(ref output)).WriteInt32(PassTarget);
		}
		if (PassState != EBoolState.False)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)56);
			((WriteContext)(ref output)).WriteEnum((int)PassState);
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
		if (LairId != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(LairId);
		}
		if (Level != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(Level);
		}
		if (KillCount != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(KillCount);
		}
		if (BestKill != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(BestKill);
		}
		if (PassTarget != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(PassTarget);
		}
		if (PassState != EBoolState.False)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)PassState);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(WorldLairDetail other)
	{
		if (other != null)
		{
			if (other.UUID.Length != 0)
			{
				UUID = other.UUID;
			}
			if (other.LairId != 0)
			{
				LairId = other.LairId;
			}
			if (other.Level != 0)
			{
				Level = other.Level;
			}
			if (other.KillCount != 0)
			{
				KillCount = other.KillCount;
			}
			if (other.BestKill != 0)
			{
				BestKill = other.BestKill;
			}
			if (other.PassTarget != 0)
			{
				PassTarget = other.PassTarget;
			}
			if (other.PassState != EBoolState.False)
			{
				PassState = other.PassState;
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
			case 10u:
				UUID = ((ParseContext)(ref input)).ReadString();
				break;
			case 16u:
				LairId = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 24u:
				Level = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 32u:
				KillCount = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 40u:
				BestKill = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 48u:
				PassTarget = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 56u:
				PassState = (EBoolState)((ParseContext)(ref input)).ReadEnum();
				break;
			}
		}
	}
}
