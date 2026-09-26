using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class LegionWarSelfInfo : IMessage<LegionWarSelfInfo>, IMessage, IEquatable<LegionWarSelfInfo>, IDeepCloneable<LegionWarSelfInfo>, IBufferMessage
{
	private static readonly MessageParser<LegionWarSelfInfo> _parser = new MessageParser<LegionWarSelfInfo>((Func<LegionWarSelfInfo>)(() => new LegionWarSelfInfo()));

	private UnknownFieldSet _unknownFields;

	public const int AttackNumFieldNumber = 1;

	private int attackNum_;

	public const int KillStreakFieldNumber = 2;

	private int killStreak_;

	public const int AwardNumFieldNumber = 3;

	private int awardNum_;

	public const int AwardLastTmFieldNumber = 4;

	private long awardLastTm_;

	public const int SelfScoreFieldNumber = 5;

	private int selfScore_;

	public const int ActionPointsFieldNumber = 6;

	private int actionPoints_;

	public const int FromPointIdFieldNumber = 7;

	private int fromPointId_;

	public const int ToPointIdFieldNumber = 8;

	private int toPointId_;

	public const int ArrivalTmFieldNumber = 9;

	private long arrivalTm_;

	public const int MoveCDFieldNumber = 10;

	private int moveCD_;

	public const int RestStartTmFieldNumber = 11;

	private long restStartTm_;

	public const int BebackStateFieldNumber = 12;

	private EBoolState bebackState_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<LegionWarSelfInfo> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeLegionWarSelfInfoReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int AttackNum
	{
		get
		{
			return attackNum_;
		}
		set
		{
			attackNum_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int KillStreak
	{
		get
		{
			return killStreak_;
		}
		set
		{
			killStreak_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int AwardNum
	{
		get
		{
			return awardNum_;
		}
		set
		{
			awardNum_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public long AwardLastTm
	{
		get
		{
			return awardLastTm_;
		}
		set
		{
			awardLastTm_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int SelfScore
	{
		get
		{
			return selfScore_;
		}
		set
		{
			selfScore_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int ActionPoints
	{
		get
		{
			return actionPoints_;
		}
		set
		{
			actionPoints_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int FromPointId
	{
		get
		{
			return fromPointId_;
		}
		set
		{
			fromPointId_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int ToPointId
	{
		get
		{
			return toPointId_;
		}
		set
		{
			toPointId_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public long ArrivalTm
	{
		get
		{
			return arrivalTm_;
		}
		set
		{
			arrivalTm_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int MoveCD
	{
		get
		{
			return moveCD_;
		}
		set
		{
			moveCD_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public long RestStartTm
	{
		get
		{
			return restStartTm_;
		}
		set
		{
			restStartTm_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public EBoolState BebackState
	{
		get
		{
			return bebackState_;
		}
		set
		{
			bebackState_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public LegionWarSelfInfo()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public LegionWarSelfInfo(LegionWarSelfInfo other)
		: this()
	{
		attackNum_ = other.attackNum_;
		killStreak_ = other.killStreak_;
		awardNum_ = other.awardNum_;
		awardLastTm_ = other.awardLastTm_;
		selfScore_ = other.selfScore_;
		actionPoints_ = other.actionPoints_;
		fromPointId_ = other.fromPointId_;
		toPointId_ = other.toPointId_;
		arrivalTm_ = other.arrivalTm_;
		moveCD_ = other.moveCD_;
		restStartTm_ = other.restStartTm_;
		bebackState_ = other.bebackState_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public LegionWarSelfInfo Clone()
	{
		return new LegionWarSelfInfo(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as LegionWarSelfInfo);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(LegionWarSelfInfo other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (AttackNum != other.AttackNum)
		{
			return false;
		}
		if (KillStreak != other.KillStreak)
		{
			return false;
		}
		if (AwardNum != other.AwardNum)
		{
			return false;
		}
		if (AwardLastTm != other.AwardLastTm)
		{
			return false;
		}
		if (SelfScore != other.SelfScore)
		{
			return false;
		}
		if (ActionPoints != other.ActionPoints)
		{
			return false;
		}
		if (FromPointId != other.FromPointId)
		{
			return false;
		}
		if (ToPointId != other.ToPointId)
		{
			return false;
		}
		if (ArrivalTm != other.ArrivalTm)
		{
			return false;
		}
		if (MoveCD != other.MoveCD)
		{
			return false;
		}
		if (RestStartTm != other.RestStartTm)
		{
			return false;
		}
		if (BebackState != other.BebackState)
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
		if (AttackNum != 0)
		{
			num ^= AttackNum.GetHashCode();
		}
		if (KillStreak != 0)
		{
			num ^= KillStreak.GetHashCode();
		}
		if (AwardNum != 0)
		{
			num ^= AwardNum.GetHashCode();
		}
		if (AwardLastTm != 0L)
		{
			num ^= AwardLastTm.GetHashCode();
		}
		if (SelfScore != 0)
		{
			num ^= SelfScore.GetHashCode();
		}
		if (ActionPoints != 0)
		{
			num ^= ActionPoints.GetHashCode();
		}
		if (FromPointId != 0)
		{
			num ^= FromPointId.GetHashCode();
		}
		if (ToPointId != 0)
		{
			num ^= ToPointId.GetHashCode();
		}
		if (ArrivalTm != 0L)
		{
			num ^= ArrivalTm.GetHashCode();
		}
		if (MoveCD != 0)
		{
			num ^= MoveCD.GetHashCode();
		}
		if (RestStartTm != 0L)
		{
			num ^= RestStartTm.GetHashCode();
		}
		if (BebackState != EBoolState.False)
		{
			num ^= BebackState.GetHashCode();
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
		if (AttackNum != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteInt32(AttackNum);
		}
		if (KillStreak != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteInt32(KillStreak);
		}
		if (AwardNum != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)24);
			((WriteContext)(ref output)).WriteInt32(AwardNum);
		}
		if (AwardLastTm != 0L)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)32);
			((WriteContext)(ref output)).WriteInt64(AwardLastTm);
		}
		if (SelfScore != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)40);
			((WriteContext)(ref output)).WriteInt32(SelfScore);
		}
		if (ActionPoints != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)48);
			((WriteContext)(ref output)).WriteInt32(ActionPoints);
		}
		if (FromPointId != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)56);
			((WriteContext)(ref output)).WriteInt32(FromPointId);
		}
		if (ToPointId != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)64);
			((WriteContext)(ref output)).WriteInt32(ToPointId);
		}
		if (ArrivalTm != 0L)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)72);
			((WriteContext)(ref output)).WriteInt64(ArrivalTm);
		}
		if (MoveCD != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)80);
			((WriteContext)(ref output)).WriteInt32(MoveCD);
		}
		if (RestStartTm != 0L)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)88);
			((WriteContext)(ref output)).WriteInt64(RestStartTm);
		}
		if (BebackState != EBoolState.False)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)96);
			((WriteContext)(ref output)).WriteEnum((int)BebackState);
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
		if (AttackNum != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(AttackNum);
		}
		if (KillStreak != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(KillStreak);
		}
		if (AwardNum != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(AwardNum);
		}
		if (AwardLastTm != 0L)
		{
			num += 1 + CodedOutputStream.ComputeInt64Size(AwardLastTm);
		}
		if (SelfScore != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(SelfScore);
		}
		if (ActionPoints != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(ActionPoints);
		}
		if (FromPointId != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(FromPointId);
		}
		if (ToPointId != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(ToPointId);
		}
		if (ArrivalTm != 0L)
		{
			num += 1 + CodedOutputStream.ComputeInt64Size(ArrivalTm);
		}
		if (MoveCD != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(MoveCD);
		}
		if (RestStartTm != 0L)
		{
			num += 1 + CodedOutputStream.ComputeInt64Size(RestStartTm);
		}
		if (BebackState != EBoolState.False)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)BebackState);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(LegionWarSelfInfo other)
	{
		if (other != null)
		{
			if (other.AttackNum != 0)
			{
				AttackNum = other.AttackNum;
			}
			if (other.KillStreak != 0)
			{
				KillStreak = other.KillStreak;
			}
			if (other.AwardNum != 0)
			{
				AwardNum = other.AwardNum;
			}
			if (other.AwardLastTm != 0L)
			{
				AwardLastTm = other.AwardLastTm;
			}
			if (other.SelfScore != 0)
			{
				SelfScore = other.SelfScore;
			}
			if (other.ActionPoints != 0)
			{
				ActionPoints = other.ActionPoints;
			}
			if (other.FromPointId != 0)
			{
				FromPointId = other.FromPointId;
			}
			if (other.ToPointId != 0)
			{
				ToPointId = other.ToPointId;
			}
			if (other.ArrivalTm != 0L)
			{
				ArrivalTm = other.ArrivalTm;
			}
			if (other.MoveCD != 0)
			{
				MoveCD = other.MoveCD;
			}
			if (other.RestStartTm != 0L)
			{
				RestStartTm = other.RestStartTm;
			}
			if (other.BebackState != EBoolState.False)
			{
				BebackState = other.BebackState;
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
				AttackNum = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 16u:
				KillStreak = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 24u:
				AwardNum = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 32u:
				AwardLastTm = ((ParseContext)(ref input)).ReadInt64();
				break;
			case 40u:
				SelfScore = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 48u:
				ActionPoints = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 56u:
				FromPointId = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 64u:
				ToPointId = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 72u:
				ArrivalTm = ((ParseContext)(ref input)).ReadInt64();
				break;
			case 80u:
				MoveCD = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 88u:
				RestStartTm = ((ParseContext)(ref input)).ReadInt64();
				break;
			case 96u:
				BebackState = (EBoolState)((ParseContext)(ref input)).ReadEnum();
				break;
			}
		}
	}
}
