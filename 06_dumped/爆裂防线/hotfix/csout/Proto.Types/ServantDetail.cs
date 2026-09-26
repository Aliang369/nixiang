using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class ServantDetail : IMessage<ServantDetail>, IMessage, IEquatable<ServantDetail>, IDeepCloneable<ServantDetail>, IBufferMessage
{
	private static readonly MessageParser<ServantDetail> _parser = new MessageParser<ServantDetail>((Func<ServantDetail>)(() => new ServantDetail()));

	private UnknownFieldSet _unknownFields;

	public const int ServantIdFieldNumber = 1;

	private string servantId_ = "";

	public const int ServantTidFieldNumber = 2;

	private int servantTid_;

	public const int QualityFieldNumber = 3;

	private EQuality quality_;

	public const int QualityCountFieldNumber = 4;

	private int qualityCount_;

	public const int LevelFieldNumber = 5;

	private int level_;

	public const int MainSkillFieldNumber = 6;

	private ServantSkillDetail mainSkill_;

	public const int SubSkillListFieldNumber = 7;

	private static readonly FieldCodec<ServantSkillDetail> _repeated_subSkillList_codec = FieldCodec.ForMessage<ServantSkillDetail>(58u, ServantSkillDetail.Parser);

	private readonly RepeatedField<ServantSkillDetail> subSkillList_ = new RepeatedField<ServantSkillDetail>();

	public const int SubAttrListFieldNumber = 8;

	private static readonly FieldCodec<ServantAttrDetail> _repeated_subAttrList_codec = FieldCodec.ForMessage<ServantAttrDetail>(66u, ServantAttrDetail.Parser);

	private readonly RepeatedField<ServantAttrDetail> subAttrList_ = new RepeatedField<ServantAttrDetail>();

	public const int SubAttrUpgradeCountFieldNumber = 9;

	private int subAttrUpgradeCount_;

	public const int SubAttrUpgradeCountedFieldNumber = 10;

	private int subAttrUpgradeCounted_;

	public const int LockFieldNumber = 11;

	private EBoolState lock_;

	public const int DeployFieldNumber = 12;

	private EBoolState deploy_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<ServantDetail> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeServantDetailReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public string ServantId
	{
		get
		{
			return servantId_;
		}
		set
		{
			servantId_ = ProtoPreconditions.CheckNotNull<string>(value, "value");
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int ServantTid
	{
		get
		{
			return servantTid_;
		}
		set
		{
			servantTid_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public EQuality Quality
	{
		get
		{
			return quality_;
		}
		set
		{
			quality_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int QualityCount
	{
		get
		{
			return qualityCount_;
		}
		set
		{
			qualityCount_ = value;
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
	public ServantSkillDetail MainSkill
	{
		get
		{
			return mainSkill_;
		}
		set
		{
			mainSkill_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<ServantSkillDetail> SubSkillList => subSkillList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<ServantAttrDetail> SubAttrList => subAttrList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int SubAttrUpgradeCount
	{
		get
		{
			return subAttrUpgradeCount_;
		}
		set
		{
			subAttrUpgradeCount_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int SubAttrUpgradeCounted
	{
		get
		{
			return subAttrUpgradeCounted_;
		}
		set
		{
			subAttrUpgradeCounted_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public EBoolState Lock
	{
		get
		{
			return lock_;
		}
		set
		{
			lock_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public EBoolState Deploy
	{
		get
		{
			return deploy_;
		}
		set
		{
			deploy_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ServantDetail()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ServantDetail(ServantDetail other)
		: this()
	{
		servantId_ = other.servantId_;
		servantTid_ = other.servantTid_;
		quality_ = other.quality_;
		qualityCount_ = other.qualityCount_;
		level_ = other.level_;
		mainSkill_ = ((other.mainSkill_ != null) ? other.mainSkill_.Clone() : null);
		subSkillList_ = other.subSkillList_.Clone();
		subAttrList_ = other.subAttrList_.Clone();
		subAttrUpgradeCount_ = other.subAttrUpgradeCount_;
		subAttrUpgradeCounted_ = other.subAttrUpgradeCounted_;
		lock_ = other.lock_;
		deploy_ = other.deploy_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ServantDetail Clone()
	{
		return new ServantDetail(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as ServantDetail);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(ServantDetail other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (ServantId != other.ServantId)
		{
			return false;
		}
		if (ServantTid != other.ServantTid)
		{
			return false;
		}
		if (Quality != other.Quality)
		{
			return false;
		}
		if (QualityCount != other.QualityCount)
		{
			return false;
		}
		if (Level != other.Level)
		{
			return false;
		}
		if (!object.Equals(MainSkill, other.MainSkill))
		{
			return false;
		}
		if (!subSkillList_.Equals(other.subSkillList_))
		{
			return false;
		}
		if (!subAttrList_.Equals(other.subAttrList_))
		{
			return false;
		}
		if (SubAttrUpgradeCount != other.SubAttrUpgradeCount)
		{
			return false;
		}
		if (SubAttrUpgradeCounted != other.SubAttrUpgradeCounted)
		{
			return false;
		}
		if (Lock != other.Lock)
		{
			return false;
		}
		if (Deploy != other.Deploy)
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
		if (ServantId.Length != 0)
		{
			num ^= ServantId.GetHashCode();
		}
		if (ServantTid != 0)
		{
			num ^= ServantTid.GetHashCode();
		}
		if (Quality != EQuality.EmptyValue)
		{
			num ^= Quality.GetHashCode();
		}
		if (QualityCount != 0)
		{
			num ^= QualityCount.GetHashCode();
		}
		if (Level != 0)
		{
			num ^= Level.GetHashCode();
		}
		if (mainSkill_ != null)
		{
			num ^= MainSkill.GetHashCode();
		}
		num ^= ((object)subSkillList_).GetHashCode();
		num ^= ((object)subAttrList_).GetHashCode();
		if (SubAttrUpgradeCount != 0)
		{
			num ^= SubAttrUpgradeCount.GetHashCode();
		}
		if (SubAttrUpgradeCounted != 0)
		{
			num ^= SubAttrUpgradeCounted.GetHashCode();
		}
		if (Lock != EBoolState.False)
		{
			num ^= Lock.GetHashCode();
		}
		if (Deploy != EBoolState.False)
		{
			num ^= Deploy.GetHashCode();
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
		if (ServantId.Length != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)10);
			((WriteContext)(ref output)).WriteString(ServantId);
		}
		if (ServantTid != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteInt32(ServantTid);
		}
		if (Quality != EQuality.EmptyValue)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)24);
			((WriteContext)(ref output)).WriteEnum((int)Quality);
		}
		if (QualityCount != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)32);
			((WriteContext)(ref output)).WriteInt32(QualityCount);
		}
		if (Level != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)40);
			((WriteContext)(ref output)).WriteInt32(Level);
		}
		if (mainSkill_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)50);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)MainSkill);
		}
		subSkillList_.WriteTo(ref output, _repeated_subSkillList_codec);
		subAttrList_.WriteTo(ref output, _repeated_subAttrList_codec);
		if (SubAttrUpgradeCount != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)72);
			((WriteContext)(ref output)).WriteInt32(SubAttrUpgradeCount);
		}
		if (SubAttrUpgradeCounted != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)80);
			((WriteContext)(ref output)).WriteInt32(SubAttrUpgradeCounted);
		}
		if (Lock != EBoolState.False)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)88);
			((WriteContext)(ref output)).WriteEnum((int)Lock);
		}
		if (Deploy != EBoolState.False)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)96);
			((WriteContext)(ref output)).WriteEnum((int)Deploy);
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
		if (ServantId.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(ServantId);
		}
		if (ServantTid != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(ServantTid);
		}
		if (Quality != EQuality.EmptyValue)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)Quality);
		}
		if (QualityCount != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(QualityCount);
		}
		if (Level != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(Level);
		}
		if (mainSkill_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)MainSkill);
		}
		num += subSkillList_.CalculateSize(_repeated_subSkillList_codec);
		num += subAttrList_.CalculateSize(_repeated_subAttrList_codec);
		if (SubAttrUpgradeCount != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(SubAttrUpgradeCount);
		}
		if (SubAttrUpgradeCounted != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(SubAttrUpgradeCounted);
		}
		if (Lock != EBoolState.False)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)Lock);
		}
		if (Deploy != EBoolState.False)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)Deploy);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(ServantDetail other)
	{
		if (other == null)
		{
			return;
		}
		if (other.ServantId.Length != 0)
		{
			ServantId = other.ServantId;
		}
		if (other.ServantTid != 0)
		{
			ServantTid = other.ServantTid;
		}
		if (other.Quality != EQuality.EmptyValue)
		{
			Quality = other.Quality;
		}
		if (other.QualityCount != 0)
		{
			QualityCount = other.QualityCount;
		}
		if (other.Level != 0)
		{
			Level = other.Level;
		}
		if (other.mainSkill_ != null)
		{
			if (mainSkill_ == null)
			{
				MainSkill = new ServantSkillDetail();
			}
			MainSkill.MergeFrom(other.MainSkill);
		}
		subSkillList_.Add((IEnumerable<ServantSkillDetail>)other.subSkillList_);
		subAttrList_.Add((IEnumerable<ServantAttrDetail>)other.subAttrList_);
		if (other.SubAttrUpgradeCount != 0)
		{
			SubAttrUpgradeCount = other.SubAttrUpgradeCount;
		}
		if (other.SubAttrUpgradeCounted != 0)
		{
			SubAttrUpgradeCounted = other.SubAttrUpgradeCounted;
		}
		if (other.Lock != EBoolState.False)
		{
			Lock = other.Lock;
		}
		if (other.Deploy != EBoolState.False)
		{
			Deploy = other.Deploy;
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
				ServantId = ((ParseContext)(ref input)).ReadString();
				break;
			case 16u:
				ServantTid = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 24u:
				Quality = (EQuality)((ParseContext)(ref input)).ReadEnum();
				break;
			case 32u:
				QualityCount = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 40u:
				Level = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 50u:
				if (mainSkill_ == null)
				{
					MainSkill = new ServantSkillDetail();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)MainSkill);
				break;
			case 58u:
				subSkillList_.AddEntriesFrom(ref input, _repeated_subSkillList_codec);
				break;
			case 66u:
				subAttrList_.AddEntriesFrom(ref input, _repeated_subAttrList_codec);
				break;
			case 72u:
				SubAttrUpgradeCount = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 80u:
				SubAttrUpgradeCounted = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 88u:
				Lock = (EBoolState)((ParseContext)(ref input)).ReadEnum();
				break;
			case 96u:
				Deploy = (EBoolState)((ParseContext)(ref input)).ReadEnum();
				break;
			}
		}
	}
}
