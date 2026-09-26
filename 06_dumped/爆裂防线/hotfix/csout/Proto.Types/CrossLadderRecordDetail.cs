using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class CrossLadderRecordDetail : IMessage<CrossLadderRecordDetail>, IMessage, IEquatable<CrossLadderRecordDetail>, IDeepCloneable<CrossLadderRecordDetail>, IBufferMessage
{
	private static readonly MessageParser<CrossLadderRecordDetail> _parser = new MessageParser<CrossLadderRecordDetail>((Func<CrossLadderRecordDetail>)(() => new CrossLadderRecordDetail()));

	private UnknownFieldSet _unknownFields;

	public const int UUIDFieldNumber = 1;

	private string uUID_ = "";

	public const int FightTypeFieldNumber = 2;

	private EFightType fightType_;

	public const int ResultScoreFieldNumber = 3;

	private EBattleResultScore resultScore_;

	public const int ChangeScoreFieldNumber = 4;

	private int changeScore_;

	public const int RecordTmFieldNumber = 5;

	private long recordTm_;

	public const int RoleBaseInfoFieldNumber = 6;

	private RoleBaseInfo roleBaseInfo_;

	public const int LadderIdFieldNumber = 7;

	private int ladderId_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<CrossLadderRecordDetail> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeCrossLadderRecordDetailReflection.Descriptor.MessageTypes[0];

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
	public int ChangeScore
	{
		get
		{
			return changeScore_;
		}
		set
		{
			changeScore_ = value;
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
	public int LadderId
	{
		get
		{
			return ladderId_;
		}
		set
		{
			ladderId_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public CrossLadderRecordDetail()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public CrossLadderRecordDetail(CrossLadderRecordDetail other)
		: this()
	{
		uUID_ = other.uUID_;
		fightType_ = other.fightType_;
		resultScore_ = other.resultScore_;
		changeScore_ = other.changeScore_;
		recordTm_ = other.recordTm_;
		roleBaseInfo_ = ((other.roleBaseInfo_ != null) ? other.roleBaseInfo_.Clone() : null);
		ladderId_ = other.ladderId_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public CrossLadderRecordDetail Clone()
	{
		return new CrossLadderRecordDetail(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as CrossLadderRecordDetail);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(CrossLadderRecordDetail other)
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
		if (ChangeScore != other.ChangeScore)
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
		if (LadderId != other.LadderId)
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
		if (ChangeScore != 0)
		{
			num ^= ChangeScore.GetHashCode();
		}
		if (RecordTm != 0L)
		{
			num ^= RecordTm.GetHashCode();
		}
		if (roleBaseInfo_ != null)
		{
			num ^= RoleBaseInfo.GetHashCode();
		}
		if (LadderId != 0)
		{
			num ^= LadderId.GetHashCode();
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
		if (ChangeScore != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)32);
			((WriteContext)(ref output)).WriteInt32(ChangeScore);
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
		if (LadderId != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)56);
			((WriteContext)(ref output)).WriteInt32(LadderId);
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
		if (ChangeScore != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(ChangeScore);
		}
		if (RecordTm != 0L)
		{
			num += 1 + CodedOutputStream.ComputeInt64Size(RecordTm);
		}
		if (roleBaseInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)RoleBaseInfo);
		}
		if (LadderId != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(LadderId);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(CrossLadderRecordDetail other)
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
		if (other.ChangeScore != 0)
		{
			ChangeScore = other.ChangeScore;
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
		if (other.LadderId != 0)
		{
			LadderId = other.LadderId;
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
				ChangeScore = ((ParseContext)(ref input)).ReadInt32();
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
			case 56u:
				LadderId = ((ParseContext)(ref input)).ReadInt32();
				break;
			}
		}
	}
}
