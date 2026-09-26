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
public sealed class BattleDefenseDetail : IMessage<BattleDefenseDetail>, IMessage, IEquatable<BattleDefenseDetail>, IDeepCloneable<BattleDefenseDetail>, IBufferMessage
{
	private static readonly MessageParser<BattleDefenseDetail> _parser = new MessageParser<BattleDefenseDetail>((Func<BattleDefenseDetail>)(() => new BattleDefenseDetail()));

	private UnknownFieldSet _unknownFields;

	public const int DefenseTypeFieldNumber = 1;

	private EDefenseType defenseType_;

	public const int DefensePctFieldNumber = 2;

	private float defensePct_;

	public const int AttrListFieldNumber = 3;

	private static readonly FieldCodec<BattleDefenseAttrDetail> _repeated_attrList_codec = FieldCodec.ForMessage<BattleDefenseAttrDetail>(26u, BattleDefenseAttrDetail.Parser);

	private readonly RepeatedField<BattleDefenseAttrDetail> attrList_ = new RepeatedField<BattleDefenseAttrDetail>();

	public const int WallIdFieldNumber = 4;

	private int wallId_;

	public const int WallSkillListFieldNumber = 5;

	private static readonly FieldCodec<int> _repeated_wallSkillList_codec = FieldCodec.ForInt32(42u);

	private readonly RepeatedField<int> wallSkillList_ = new RepeatedField<int>();

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<BattleDefenseDetail> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeBattleDefenseDetailReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public EDefenseType DefenseType
	{
		get
		{
			return defenseType_;
		}
		set
		{
			defenseType_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public float DefensePct
	{
		get
		{
			return defensePct_;
		}
		set
		{
			defensePct_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<BattleDefenseAttrDetail> AttrList => attrList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int WallId
	{
		get
		{
			return wallId_;
		}
		set
		{
			wallId_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<int> WallSkillList => wallSkillList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public BattleDefenseDetail()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public BattleDefenseDetail(BattleDefenseDetail other)
		: this()
	{
		defenseType_ = other.defenseType_;
		defensePct_ = other.defensePct_;
		attrList_ = other.attrList_.Clone();
		wallId_ = other.wallId_;
		wallSkillList_ = other.wallSkillList_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public BattleDefenseDetail Clone()
	{
		return new BattleDefenseDetail(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as BattleDefenseDetail);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(BattleDefenseDetail other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (DefenseType != other.DefenseType)
		{
			return false;
		}
		if (!ProtobufEqualityComparers.BitwiseSingleEqualityComparer.Equals(DefensePct, other.DefensePct))
		{
			return false;
		}
		if (!attrList_.Equals(other.attrList_))
		{
			return false;
		}
		if (WallId != other.WallId)
		{
			return false;
		}
		if (!wallSkillList_.Equals(other.wallSkillList_))
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
		if (DefenseType != EDefenseType.EmptyValue)
		{
			num ^= DefenseType.GetHashCode();
		}
		if (DefensePct != 0f)
		{
			num ^= ProtobufEqualityComparers.BitwiseSingleEqualityComparer.GetHashCode(DefensePct);
		}
		num ^= ((object)attrList_).GetHashCode();
		if (WallId != 0)
		{
			num ^= WallId.GetHashCode();
		}
		num ^= ((object)wallSkillList_).GetHashCode();
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
		if (DefenseType != EDefenseType.EmptyValue)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteEnum((int)DefenseType);
		}
		if (DefensePct != 0f)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)21);
			((WriteContext)(ref output)).WriteFloat(DefensePct);
		}
		attrList_.WriteTo(ref output, _repeated_attrList_codec);
		if (WallId != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)32);
			((WriteContext)(ref output)).WriteInt32(WallId);
		}
		wallSkillList_.WriteTo(ref output, _repeated_wallSkillList_codec);
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
		if (DefenseType != EDefenseType.EmptyValue)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)DefenseType);
		}
		if (DefensePct != 0f)
		{
			num += 5;
		}
		num += attrList_.CalculateSize(_repeated_attrList_codec);
		if (WallId != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(WallId);
		}
		num += wallSkillList_.CalculateSize(_repeated_wallSkillList_codec);
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(BattleDefenseDetail other)
	{
		if (other != null)
		{
			if (other.DefenseType != EDefenseType.EmptyValue)
			{
				DefenseType = other.DefenseType;
			}
			if (other.DefensePct != 0f)
			{
				DefensePct = other.DefensePct;
			}
			attrList_.Add((IEnumerable<BattleDefenseAttrDetail>)other.attrList_);
			if (other.WallId != 0)
			{
				WallId = other.WallId;
			}
			wallSkillList_.Add((IEnumerable<int>)other.wallSkillList_);
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
				DefenseType = (EDefenseType)((ParseContext)(ref input)).ReadEnum();
				break;
			case 21u:
				DefensePct = ((ParseContext)(ref input)).ReadFloat();
				break;
			case 26u:
				attrList_.AddEntriesFrom(ref input, _repeated_attrList_codec);
				break;
			case 32u:
				WallId = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 40u:
			case 42u:
				wallSkillList_.AddEntriesFrom(ref input, _repeated_wallSkillList_codec);
				break;
			}
		}
	}
}
