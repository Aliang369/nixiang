using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class MechaDetail : IMessage<MechaDetail>, IMessage, IEquatable<MechaDetail>, IDeepCloneable<MechaDetail>, IBufferMessage
{
	private static readonly MessageParser<MechaDetail> _parser = new MessageParser<MechaDetail>((Func<MechaDetail>)(() => new MechaDetail()));

	private UnknownFieldSet _unknownFields;

	public const int MechaIdFieldNumber = 1;

	private int mechaId_;

	public const int LevelFieldNumber = 2;

	private int level_;

	public const int StarFieldNumber = 3;

	private int star_;

	public const int SkillListFieldNumber = 4;

	private static readonly FieldCodec<MechaSkillDetail> _repeated_skillList_codec = FieldCodec.ForMessage<MechaSkillDetail>(34u, MechaSkillDetail.Parser);

	private readonly RepeatedField<MechaSkillDetail> skillList_ = new RepeatedField<MechaSkillDetail>();

	public const int AttrListFieldNumber = 5;

	private static readonly FieldCodec<BattleMechaAttrDetail> _repeated_attrList_codec = FieldCodec.ForMessage<BattleMechaAttrDetail>(42u, BattleMechaAttrDetail.Parser);

	private readonly RepeatedField<BattleMechaAttrDetail> attrList_ = new RepeatedField<BattleMechaAttrDetail>();

	public const int AttackFieldNumber = 6;

	private int attack_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<MechaDetail> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeMechaDetailReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int MechaId
	{
		get
		{
			return mechaId_;
		}
		set
		{
			mechaId_ = value;
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
	public int Star
	{
		get
		{
			return star_;
		}
		set
		{
			star_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<MechaSkillDetail> SkillList => skillList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<BattleMechaAttrDetail> AttrList => attrList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int Attack
	{
		get
		{
			return attack_;
		}
		set
		{
			attack_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public MechaDetail()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public MechaDetail(MechaDetail other)
		: this()
	{
		mechaId_ = other.mechaId_;
		level_ = other.level_;
		star_ = other.star_;
		skillList_ = other.skillList_.Clone();
		attrList_ = other.attrList_.Clone();
		attack_ = other.attack_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public MechaDetail Clone()
	{
		return new MechaDetail(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as MechaDetail);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(MechaDetail other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (MechaId != other.MechaId)
		{
			return false;
		}
		if (Level != other.Level)
		{
			return false;
		}
		if (Star != other.Star)
		{
			return false;
		}
		if (!skillList_.Equals(other.skillList_))
		{
			return false;
		}
		if (!attrList_.Equals(other.attrList_))
		{
			return false;
		}
		if (Attack != other.Attack)
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
		if (MechaId != 0)
		{
			num ^= MechaId.GetHashCode();
		}
		if (Level != 0)
		{
			num ^= Level.GetHashCode();
		}
		if (Star != 0)
		{
			num ^= Star.GetHashCode();
		}
		num ^= ((object)skillList_).GetHashCode();
		num ^= ((object)attrList_).GetHashCode();
		if (Attack != 0)
		{
			num ^= Attack.GetHashCode();
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
		if (MechaId != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteInt32(MechaId);
		}
		if (Level != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteInt32(Level);
		}
		if (Star != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)24);
			((WriteContext)(ref output)).WriteInt32(Star);
		}
		skillList_.WriteTo(ref output, _repeated_skillList_codec);
		attrList_.WriteTo(ref output, _repeated_attrList_codec);
		if (Attack != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)48);
			((WriteContext)(ref output)).WriteInt32(Attack);
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
		if (MechaId != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(MechaId);
		}
		if (Level != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(Level);
		}
		if (Star != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(Star);
		}
		num += skillList_.CalculateSize(_repeated_skillList_codec);
		num += attrList_.CalculateSize(_repeated_attrList_codec);
		if (Attack != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(Attack);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(MechaDetail other)
	{
		if (other != null)
		{
			if (other.MechaId != 0)
			{
				MechaId = other.MechaId;
			}
			if (other.Level != 0)
			{
				Level = other.Level;
			}
			if (other.Star != 0)
			{
				Star = other.Star;
			}
			skillList_.Add((IEnumerable<MechaSkillDetail>)other.skillList_);
			attrList_.Add((IEnumerable<BattleMechaAttrDetail>)other.attrList_);
			if (other.Attack != 0)
			{
				Attack = other.Attack;
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
				MechaId = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 16u:
				Level = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 24u:
				Star = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 34u:
				skillList_.AddEntriesFrom(ref input, _repeated_skillList_codec);
				break;
			case 42u:
				attrList_.AddEntriesFrom(ref input, _repeated_attrList_codec);
				break;
			case 48u:
				Attack = ((ParseContext)(ref input)).ReadInt32();
				break;
			}
		}
	}
}
