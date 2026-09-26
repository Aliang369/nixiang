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
public sealed class BattleSkillDetail : IMessage<BattleSkillDetail>, IMessage, IEquatable<BattleSkillDetail>, IDeepCloneable<BattleSkillDetail>, IBufferMessage
{
	private static readonly MessageParser<BattleSkillDetail> _parser = new MessageParser<BattleSkillDetail>((Func<BattleSkillDetail>)(() => new BattleSkillDetail()));

	private UnknownFieldSet _unknownFields;

	public const int SkillTypeFieldNumber = 1;

	private ESkillType skillType_;

	public const int SkillIdFieldNumber = 2;

	private int skillId_;

	public const int HeroTidFieldNumber = 3;

	private int heroTid_;

	public const int AttrListFieldNumber = 4;

	private static readonly FieldCodec<BattleSkillAttrDetail> _repeated_attrList_codec = FieldCodec.ForMessage<BattleSkillAttrDetail>(34u, BattleSkillAttrDetail.Parser);

	private readonly RepeatedField<BattleSkillAttrDetail> attrList_ = new RepeatedField<BattleSkillAttrDetail>();

	public const int ModListFieldNumber = 5;

	private static readonly FieldCodec<int> _repeated_modList_codec = FieldCodec.ForInt32(42u);

	private readonly RepeatedField<int> modList_ = new RepeatedField<int>();

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<BattleSkillDetail> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeBattleSkillDetailReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ESkillType SkillType
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
	public int SkillId
	{
		get
		{
			return skillId_;
		}
		set
		{
			skillId_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int HeroTid
	{
		get
		{
			return heroTid_;
		}
		set
		{
			heroTid_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<BattleSkillAttrDetail> AttrList => attrList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<int> ModList => modList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public BattleSkillDetail()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public BattleSkillDetail(BattleSkillDetail other)
		: this()
	{
		skillType_ = other.skillType_;
		skillId_ = other.skillId_;
		heroTid_ = other.heroTid_;
		attrList_ = other.attrList_.Clone();
		modList_ = other.modList_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public BattleSkillDetail Clone()
	{
		return new BattleSkillDetail(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as BattleSkillDetail);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(BattleSkillDetail other)
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
		if (SkillId != other.SkillId)
		{
			return false;
		}
		if (HeroTid != other.HeroTid)
		{
			return false;
		}
		if (!attrList_.Equals(other.attrList_))
		{
			return false;
		}
		if (!modList_.Equals(other.modList_))
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
		if (SkillType != ESkillType.EmptyValue)
		{
			num ^= SkillType.GetHashCode();
		}
		if (SkillId != 0)
		{
			num ^= SkillId.GetHashCode();
		}
		if (HeroTid != 0)
		{
			num ^= HeroTid.GetHashCode();
		}
		num ^= ((object)attrList_).GetHashCode();
		num ^= ((object)modList_).GetHashCode();
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
		if (SkillType != ESkillType.EmptyValue)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteEnum((int)SkillType);
		}
		if (SkillId != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteInt32(SkillId);
		}
		if (HeroTid != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)24);
			((WriteContext)(ref output)).WriteInt32(HeroTid);
		}
		attrList_.WriteTo(ref output, _repeated_attrList_codec);
		modList_.WriteTo(ref output, _repeated_modList_codec);
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
		if (SkillType != ESkillType.EmptyValue)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)SkillType);
		}
		if (SkillId != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(SkillId);
		}
		if (HeroTid != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(HeroTid);
		}
		num += attrList_.CalculateSize(_repeated_attrList_codec);
		num += modList_.CalculateSize(_repeated_modList_codec);
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(BattleSkillDetail other)
	{
		if (other != null)
		{
			if (other.SkillType != ESkillType.EmptyValue)
			{
				SkillType = other.SkillType;
			}
			if (other.SkillId != 0)
			{
				SkillId = other.SkillId;
			}
			if (other.HeroTid != 0)
			{
				HeroTid = other.HeroTid;
			}
			attrList_.Add((IEnumerable<BattleSkillAttrDetail>)other.attrList_);
			modList_.Add((IEnumerable<int>)other.modList_);
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
				SkillType = (ESkillType)((ParseContext)(ref input)).ReadEnum();
				break;
			case 16u:
				SkillId = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 24u:
				HeroTid = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 34u:
				attrList_.AddEntriesFrom(ref input, _repeated_attrList_codec);
				break;
			case 40u:
			case 42u:
				modList_.AddEntriesFrom(ref input, _repeated_modList_codec);
				break;
			}
		}
	}
}
