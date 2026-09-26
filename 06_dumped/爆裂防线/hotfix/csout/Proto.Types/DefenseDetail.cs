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
public sealed class DefenseDetail : IMessage<DefenseDetail>, IMessage, IEquatable<DefenseDetail>, IDeepCloneable<DefenseDetail>, IBufferMessage
{
	private static readonly MessageParser<DefenseDetail> _parser = new MessageParser<DefenseDetail>((Func<DefenseDetail>)(() => new DefenseDetail()));

	private UnknownFieldSet _unknownFields;

	public const int DefenseTypeFieldNumber = 1;

	private EDefenseType defenseType_;

	public const int AttrListFieldNumber = 2;

	private static readonly FieldCodec<BattleDefenseAttrDetail> _repeated_attrList_codec = FieldCodec.ForMessage<BattleDefenseAttrDetail>(18u, BattleDefenseAttrDetail.Parser);

	private readonly RepeatedField<BattleDefenseAttrDetail> attrList_ = new RepeatedField<BattleDefenseAttrDetail>();

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<DefenseDetail> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeDefenseDetailReflection.Descriptor.MessageTypes[0];

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
	public RepeatedField<BattleDefenseAttrDetail> AttrList => attrList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public DefenseDetail()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public DefenseDetail(DefenseDetail other)
		: this()
	{
		defenseType_ = other.defenseType_;
		attrList_ = other.attrList_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public DefenseDetail Clone()
	{
		return new DefenseDetail(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as DefenseDetail);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(DefenseDetail other)
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
		if (!attrList_.Equals(other.attrList_))
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
		num ^= ((object)attrList_).GetHashCode();
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
		attrList_.WriteTo(ref output, _repeated_attrList_codec);
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
		num += attrList_.CalculateSize(_repeated_attrList_codec);
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(DefenseDetail other)
	{
		if (other != null)
		{
			if (other.DefenseType != EDefenseType.EmptyValue)
			{
				DefenseType = other.DefenseType;
			}
			attrList_.Add((IEnumerable<BattleDefenseAttrDetail>)other.attrList_);
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
			case 18u:
				attrList_.AddEntriesFrom(ref input, _repeated_attrList_codec);
				break;
			}
		}
	}
}
