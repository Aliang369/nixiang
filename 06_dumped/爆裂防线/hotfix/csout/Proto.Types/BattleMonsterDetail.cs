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
public sealed class BattleMonsterDetail : IMessage<BattleMonsterDetail>, IMessage, IEquatable<BattleMonsterDetail>, IDeepCloneable<BattleMonsterDetail>, IBufferMessage
{
	private static readonly MessageParser<BattleMonsterDetail> _parser = new MessageParser<BattleMonsterDetail>((Func<BattleMonsterDetail>)(() => new BattleMonsterDetail()));

	private UnknownFieldSet _unknownFields;

	public const int MonsterIdFieldNumber = 1;

	private int monsterId_;

	public const int AttrListFieldNumber = 2;

	private static readonly FieldCodec<BattleMonsterAttrDetail> _repeated_attrList_codec = FieldCodec.ForMessage<BattleMonsterAttrDetail>(18u, BattleMonsterAttrDetail.Parser);

	private readonly RepeatedField<BattleMonsterAttrDetail> attrList_ = new RepeatedField<BattleMonsterAttrDetail>();

	public const int SummonFieldNumber = 3;

	private EBoolState summon_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<BattleMonsterDetail> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeBattleMonsterDetailReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int MonsterId
	{
		get
		{
			return monsterId_;
		}
		set
		{
			monsterId_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<BattleMonsterAttrDetail> AttrList => attrList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public EBoolState Summon
	{
		get
		{
			return summon_;
		}
		set
		{
			summon_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public BattleMonsterDetail()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public BattleMonsterDetail(BattleMonsterDetail other)
		: this()
	{
		monsterId_ = other.monsterId_;
		attrList_ = other.attrList_.Clone();
		summon_ = other.summon_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public BattleMonsterDetail Clone()
	{
		return new BattleMonsterDetail(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as BattleMonsterDetail);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(BattleMonsterDetail other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (MonsterId != other.MonsterId)
		{
			return false;
		}
		if (!attrList_.Equals(other.attrList_))
		{
			return false;
		}
		if (Summon != other.Summon)
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
		if (MonsterId != 0)
		{
			num ^= MonsterId.GetHashCode();
		}
		num ^= ((object)attrList_).GetHashCode();
		if (Summon != EBoolState.False)
		{
			num ^= Summon.GetHashCode();
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
		if (MonsterId != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteInt32(MonsterId);
		}
		attrList_.WriteTo(ref output, _repeated_attrList_codec);
		if (Summon != EBoolState.False)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)24);
			((WriteContext)(ref output)).WriteEnum((int)Summon);
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
		if (MonsterId != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(MonsterId);
		}
		num += attrList_.CalculateSize(_repeated_attrList_codec);
		if (Summon != EBoolState.False)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)Summon);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(BattleMonsterDetail other)
	{
		if (other != null)
		{
			if (other.MonsterId != 0)
			{
				MonsterId = other.MonsterId;
			}
			attrList_.Add((IEnumerable<BattleMonsterAttrDetail>)other.attrList_);
			if (other.Summon != EBoolState.False)
			{
				Summon = other.Summon;
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
				MonsterId = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 18u:
				attrList_.AddEntriesFrom(ref input, _repeated_attrList_codec);
				break;
			case 24u:
				Summon = (EBoolState)((ParseContext)(ref input)).ReadEnum();
				break;
			}
		}
	}
}
