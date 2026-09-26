using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class BattleServantDetail : IMessage<BattleServantDetail>, IMessage, IEquatable<BattleServantDetail>, IDeepCloneable<BattleServantDetail>, IBufferMessage
{
	private static readonly MessageParser<BattleServantDetail> _parser = new MessageParser<BattleServantDetail>((Func<BattleServantDetail>)(() => new BattleServantDetail()));

	private UnknownFieldSet _unknownFields;

	public const int SkillTypeFieldNumber = 1;

	private int skillType_;

	public const int ServantTidFieldNumber = 2;

	private int servantTid_;

	public const int MainSkillFieldNumber = 3;

	private BattleServantSkillDetail mainSkill_;

	public const int SubSkillListFieldNumber = 4;

	private static readonly FieldCodec<BattleServantSkillDetail> _repeated_subSkillList_codec = FieldCodec.ForMessage<BattleServantSkillDetail>(34u, BattleServantSkillDetail.Parser);

	private readonly RepeatedField<BattleServantSkillDetail> subSkillList_ = new RepeatedField<BattleServantSkillDetail>();

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<BattleServantDetail> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeBattleServantDetailReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int SkillType
	{
		get
		{
			return skillType_;
		}
		set
		{
			skillType_ = value;
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
	public BattleServantSkillDetail MainSkill
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
	public RepeatedField<BattleServantSkillDetail> SubSkillList => subSkillList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public BattleServantDetail()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public BattleServantDetail(BattleServantDetail other)
		: this()
	{
		skillType_ = other.skillType_;
		servantTid_ = other.servantTid_;
		mainSkill_ = ((other.mainSkill_ != null) ? other.mainSkill_.Clone() : null);
		subSkillList_ = other.subSkillList_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public BattleServantDetail Clone()
	{
		return new BattleServantDetail(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as BattleServantDetail);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(BattleServantDetail other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (SkillType != other.SkillType)
		{
			return false;
		}
		if (ServantTid != other.ServantTid)
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
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override int GetHashCode()
	{
		int num = 1;
		if (SkillType != 0)
		{
			num ^= SkillType.GetHashCode();
		}
		if (ServantTid != 0)
		{
			num ^= ServantTid.GetHashCode();
		}
		if (mainSkill_ != null)
		{
			num ^= MainSkill.GetHashCode();
		}
		num ^= ((object)subSkillList_).GetHashCode();
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
		if (SkillType != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteInt32(SkillType);
		}
		if (ServantTid != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteInt32(ServantTid);
		}
		if (mainSkill_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)26);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)MainSkill);
		}
		subSkillList_.WriteTo(ref output, _repeated_subSkillList_codec);
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
		if (SkillType != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(SkillType);
		}
		if (ServantTid != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(ServantTid);
		}
		if (mainSkill_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)MainSkill);
		}
		num += subSkillList_.CalculateSize(_repeated_subSkillList_codec);
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(BattleServantDetail other)
	{
		if (other == null)
		{
			return;
		}
		if (other.SkillType != 0)
		{
			SkillType = other.SkillType;
		}
		if (other.ServantTid != 0)
		{
			ServantTid = other.ServantTid;
		}
		if (other.mainSkill_ != null)
		{
			if (mainSkill_ == null)
			{
				MainSkill = new BattleServantSkillDetail();
			}
			MainSkill.MergeFrom(other.MainSkill);
		}
		subSkillList_.Add((IEnumerable<BattleServantSkillDetail>)other.subSkillList_);
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
			case 8u:
				SkillType = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 16u:
				ServantTid = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 26u:
				if (mainSkill_ == null)
				{
					MainSkill = new BattleServantSkillDetail();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)MainSkill);
				break;
			case 34u:
				subSkillList_.AddEntriesFrom(ref input, _repeated_subSkillList_codec);
				break;
			}
		}
	}
}
