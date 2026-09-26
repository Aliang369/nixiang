using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class ArenaRecordDetail : IMessage<ArenaRecordDetail>, IMessage, IEquatable<ArenaRecordDetail>, IDeepCloneable<ArenaRecordDetail>, IBufferMessage
{
	private static readonly MessageParser<ArenaRecordDetail> _parser = new MessageParser<ArenaRecordDetail>((Func<ArenaRecordDetail>)(() => new ArenaRecordDetail()));

	private UnknownFieldSet _unknownFields;

	public const int UUIDFieldNumber = 1;

	private string uUID_ = "";

	public const int FightTypeFieldNumber = 2;

	private EFightType fightType_;

	public const int ResultScoreFieldNumber = 3;

	private EBattleResultScore resultScore_;

	public const int ChangeRankFieldNumber = 4;

	private int changeRank_;

	public const int RecordTmFieldNumber = 5;

	private long recordTm_;

	public const int RoleBaseInfoFieldNumber = 6;

	private RoleBaseInfo roleBaseInfo_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<ArenaRecordDetail> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeArenaRecordDetailReflection.Descriptor.MessageTypes[0];

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
	public EFightType FightType
	{
		get
		{
			return fightType_;
		}
		set
		{
			fightType_ = value;
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
	public int ChangeRank
	{
		get
		{
			return changeRank_;
		}
		set
		{
			changeRank_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public long RecordTm
	{
		get
		{
			return recordTm_;
		}
		set
		{
			recordTm_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RoleBaseInfo RoleBaseInfo
	{
		get
		{
			return roleBaseInfo_;
		}
		set
		{
			roleBaseInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ArenaRecordDetail()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ArenaRecordDetail(ArenaRecordDetail other)
		: this()
	{
		uUID_ = other.uUID_;
		fightType_ = other.fightType_;
		resultScore_ = other.resultScore_;
		changeRank_ = other.changeRank_;
		recordTm_ = other.recordTm_;
		roleBaseInfo_ = ((other.roleBaseInfo_ != null) ? other.roleBaseInfo_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ArenaRecordDetail Clone()
	{
		return new ArenaRecordDetail(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as ArenaRecordDetail);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(ArenaRecordDetail other)
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
		if (FightType != other.FightType)
		{
			return false;
		}
		if (ResultScore != other.ResultScore)
		{
			return false;
		}
		if (ChangeRank != other.ChangeRank)
		{
			return false;
		}
		if (RecordTm != other.RecordTm)
		{
			return false;
		}
		if (!object.Equals(RoleBaseInfo, other.RoleBaseInfo))
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
		if (FightType != EFightType.EmptyValue)
		{
			num ^= FightType.GetHashCode();
		}
		if (ResultScore != EBattleResultScore.Fail)
		{
			num ^= ResultScore.GetHashCode();
		}
		if (ChangeRank != 0)
		{
			num ^= ChangeRank.GetHashCode();
		}
		if (RecordTm != 0L)
		{
			num ^= RecordTm.GetHashCode();
		}
		if (roleBaseInfo_ != null)
		{
			num ^= RoleBaseInfo.GetHashCode();
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
		if (FightType != EFightType.EmptyValue)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteEnum((int)FightType);
		}
		if (ResultScore != EBattleResultScore.Fail)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)24);
			((WriteContext)(ref output)).WriteEnum((int)ResultScore);
		}
		if (ChangeRank != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)32);
			((WriteContext)(ref output)).WriteInt32(ChangeRank);
		}
		if (RecordTm != 0L)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)40);
			((WriteContext)(ref output)).WriteInt64(RecordTm);
		}
		if (roleBaseInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)50);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)RoleBaseInfo);
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
		if (FightType != EFightType.EmptyValue)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)FightType);
		}
		if (ResultScore != EBattleResultScore.Fail)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)ResultScore);
		}
		if (ChangeRank != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(ChangeRank);
		}
		if (RecordTm != 0L)
		{
			num += 1 + CodedOutputStream.ComputeInt64Size(RecordTm);
		}
		if (roleBaseInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)RoleBaseInfo);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(ArenaRecordDetail other)
	{
		if (other == null)
		{
			return;
		}
		if (other.UUID.Length != 0)
		{
			UUID = other.UUID;
		}
		if (other.FightType != EFightType.EmptyValue)
		{
			FightType = other.FightType;
		}
		if (other.ResultScore != EBattleResultScore.Fail)
		{
			ResultScore = other.ResultScore;
		}
		if (other.ChangeRank != 0)
		{
			ChangeRank = other.ChangeRank;
		}
		if (other.RecordTm != 0L)
		{
			RecordTm = other.RecordTm;
		}
		if (other.roleBaseInfo_ != null)
		{
			if (roleBaseInfo_ == null)
			{
				RoleBaseInfo = new RoleBaseInfo();
			}
			RoleBaseInfo.MergeFrom(other.RoleBaseInfo);
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
				FightType = (EFightType)((ParseContext)(ref input)).ReadEnum();
				break;
			case 24u:
				ResultScore = (EBattleResultScore)((ParseContext)(ref input)).ReadEnum();
				break;
			case 32u:
				ChangeRank = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 40u:
				RecordTm = ((ParseContext)(ref input)).ReadInt64();
				break;
			case 50u:
				if (roleBaseInfo_ == null)
				{
					RoleBaseInfo = new RoleBaseInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)RoleBaseInfo);
				break;
			}
		}
	}
}
