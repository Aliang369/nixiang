using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class BattleBaseInfo : IMessage<BattleBaseInfo>, IMessage, IEquatable<BattleBaseInfo>, IDeepCloneable<BattleBaseInfo>, IBufferMessage
{
	private static readonly MessageParser<BattleBaseInfo> _parser = new MessageParser<BattleBaseInfo>((Func<BattleBaseInfo>)(() => new BattleBaseInfo()));

	private UnknownFieldSet _unknownFields;

	public const int UUIDFieldNumber = 1;

	private string uUID_ = "";

	public const int EnemyIdFieldNumber = 2;

	private int enemyId_;

	public const int StartTmFieldNumber = 3;

	private long startTm_;

	public const int EndTmFieldNumber = 4;

	private long endTm_;

	public const int BattleTypeFieldNumber = 5;

	private EBattleFuncType battleType_;

	public const int BattleModelFieldNumber = 6;

	private EBattleModelType battleModel_;

	public const int ModRandIdFieldNumber = 7;

	private int modRandId_;

	public const int InitLevelFieldNumber = 8;

	private int initLevel_;

	public const int MaxLevelFieldNumber = 9;

	private int maxLevel_;

	public const int TimeLimitFieldNumber = 10;

	private int timeLimit_;

	public const int VisionRangeFieldNumber = 11;

	private int visionRange_;

	public const int RefreshModFieldNumber = 12;

	private int refreshMod_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<BattleBaseInfo> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeBattleBaseInfoReflection.Descriptor.MessageTypes[0];

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
	public int EnemyId
	{
		get
		{
			return enemyId_;
		}
		set
		{
			enemyId_ = value;
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
	public EBattleModelType BattleModel
	{
		get
		{
			return battleModel_;
		}
		set
		{
			battleModel_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int ModRandId
	{
		get
		{
			return modRandId_;
		}
		set
		{
			modRandId_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int InitLevel
	{
		get
		{
			return initLevel_;
		}
		set
		{
			initLevel_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int MaxLevel
	{
		get
		{
			return maxLevel_;
		}
		set
		{
			maxLevel_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int TimeLimit
	{
		get
		{
			return timeLimit_;
		}
		set
		{
			timeLimit_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int VisionRange
	{
		get
		{
			return visionRange_;
		}
		set
		{
			visionRange_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int RefreshMod
	{
		get
		{
			return refreshMod_;
		}
		set
		{
			refreshMod_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public BattleBaseInfo()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public BattleBaseInfo(BattleBaseInfo other)
		: this()
	{
		uUID_ = other.uUID_;
		enemyId_ = other.enemyId_;
		startTm_ = other.startTm_;
		endTm_ = other.endTm_;
		battleType_ = other.battleType_;
		battleModel_ = other.battleModel_;
		modRandId_ = other.modRandId_;
		initLevel_ = other.initLevel_;
		maxLevel_ = other.maxLevel_;
		timeLimit_ = other.timeLimit_;
		visionRange_ = other.visionRange_;
		refreshMod_ = other.refreshMod_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public BattleBaseInfo Clone()
	{
		return new BattleBaseInfo(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as BattleBaseInfo);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(BattleBaseInfo other)
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
		if (EnemyId != other.EnemyId)
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
		if (BattleType != other.BattleType)
		{
			return false;
		}
		if (BattleModel != other.BattleModel)
		{
			return false;
		}
		if (ModRandId != other.ModRandId)
		{
			return false;
		}
		if (InitLevel != other.InitLevel)
		{
			return false;
		}
		if (MaxLevel != other.MaxLevel)
		{
			return false;
		}
		if (TimeLimit != other.TimeLimit)
		{
			return false;
		}
		if (VisionRange != other.VisionRange)
		{
			return false;
		}
		if (RefreshMod != other.RefreshMod)
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
		if (EnemyId != 0)
		{
			num ^= EnemyId.GetHashCode();
		}
		if (StartTm != 0L)
		{
			num ^= StartTm.GetHashCode();
		}
		if (EndTm != 0L)
		{
			num ^= EndTm.GetHashCode();
		}
		if (BattleType != EBattleFuncType.EmptyValue)
		{
			num ^= BattleType.GetHashCode();
		}
		if (BattleModel != EBattleModelType.EmptyValue)
		{
			num ^= BattleModel.GetHashCode();
		}
		if (ModRandId != 0)
		{
			num ^= ModRandId.GetHashCode();
		}
		if (InitLevel != 0)
		{
			num ^= InitLevel.GetHashCode();
		}
		if (MaxLevel != 0)
		{
			num ^= MaxLevel.GetHashCode();
		}
		if (TimeLimit != 0)
		{
			num ^= TimeLimit.GetHashCode();
		}
		if (VisionRange != 0)
		{
			num ^= VisionRange.GetHashCode();
		}
		if (RefreshMod != 0)
		{
			num ^= RefreshMod.GetHashCode();
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
		if (EnemyId != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteInt32(EnemyId);
		}
		if (StartTm != 0L)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)24);
			((WriteContext)(ref output)).WriteInt64(StartTm);
		}
		if (EndTm != 0L)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)32);
			((WriteContext)(ref output)).WriteInt64(EndTm);
		}
		if (BattleType != EBattleFuncType.EmptyValue)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)40);
			((WriteContext)(ref output)).WriteEnum((int)BattleType);
		}
		if (BattleModel != EBattleModelType.EmptyValue)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)48);
			((WriteContext)(ref output)).WriteEnum((int)BattleModel);
		}
		if (ModRandId != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)56);
			((WriteContext)(ref output)).WriteInt32(ModRandId);
		}
		if (InitLevel != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)64);
			((WriteContext)(ref output)).WriteInt32(InitLevel);
		}
		if (MaxLevel != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)72);
			((WriteContext)(ref output)).WriteInt32(MaxLevel);
		}
		if (TimeLimit != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)80);
			((WriteContext)(ref output)).WriteInt32(TimeLimit);
		}
		if (VisionRange != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)88);
			((WriteContext)(ref output)).WriteInt32(VisionRange);
		}
		if (RefreshMod != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)96);
			((WriteContext)(ref output)).WriteInt32(RefreshMod);
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
		if (EnemyId != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(EnemyId);
		}
		if (StartTm != 0L)
		{
			num += 1 + CodedOutputStream.ComputeInt64Size(StartTm);
		}
		if (EndTm != 0L)
		{
			num += 1 + CodedOutputStream.ComputeInt64Size(EndTm);
		}
		if (BattleType != EBattleFuncType.EmptyValue)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)BattleType);
		}
		if (BattleModel != EBattleModelType.EmptyValue)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)BattleModel);
		}
		if (ModRandId != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(ModRandId);
		}
		if (InitLevel != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(InitLevel);
		}
		if (MaxLevel != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(MaxLevel);
		}
		if (TimeLimit != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(TimeLimit);
		}
		if (VisionRange != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(VisionRange);
		}
		if (RefreshMod != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(RefreshMod);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(BattleBaseInfo other)
	{
		if (other != null)
		{
			if (other.UUID.Length != 0)
			{
				UUID = other.UUID;
			}
			if (other.EnemyId != 0)
			{
				EnemyId = other.EnemyId;
			}
			if (other.StartTm != 0L)
			{
				StartTm = other.StartTm;
			}
			if (other.EndTm != 0L)
			{
				EndTm = other.EndTm;
			}
			if (other.BattleType != EBattleFuncType.EmptyValue)
			{
				BattleType = other.BattleType;
			}
			if (other.BattleModel != EBattleModelType.EmptyValue)
			{
				BattleModel = other.BattleModel;
			}
			if (other.ModRandId != 0)
			{
				ModRandId = other.ModRandId;
			}
			if (other.InitLevel != 0)
			{
				InitLevel = other.InitLevel;
			}
			if (other.MaxLevel != 0)
			{
				MaxLevel = other.MaxLevel;
			}
			if (other.TimeLimit != 0)
			{
				TimeLimit = other.TimeLimit;
			}
			if (other.VisionRange != 0)
			{
				VisionRange = other.VisionRange;
			}
			if (other.RefreshMod != 0)
			{
				RefreshMod = other.RefreshMod;
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
				EnemyId = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 24u:
				StartTm = ((ParseContext)(ref input)).ReadInt64();
				break;
			case 32u:
				EndTm = ((ParseContext)(ref input)).ReadInt64();
				break;
			case 40u:
				BattleType = (EBattleFuncType)((ParseContext)(ref input)).ReadEnum();
				break;
			case 48u:
				BattleModel = (EBattleModelType)((ParseContext)(ref input)).ReadEnum();
				break;
			case 56u:
				ModRandId = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 64u:
				InitLevel = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 72u:
				MaxLevel = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 80u:
				TimeLimit = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 88u:
				VisionRange = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 96u:
				RefreshMod = ((ParseContext)(ref input)).ReadInt32();
				break;
			}
		}
	}
}
