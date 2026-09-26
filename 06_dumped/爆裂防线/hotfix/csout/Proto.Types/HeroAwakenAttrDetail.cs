using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class HeroAwakenAttrDetail : IMessage<HeroAwakenAttrDetail>, IMessage, IEquatable<HeroAwakenAttrDetail>, IDeepCloneable<HeroAwakenAttrDetail>, IBufferMessage
{
	private static readonly MessageParser<HeroAwakenAttrDetail> _parser = new MessageParser<HeroAwakenAttrDetail>((Func<HeroAwakenAttrDetail>)(() => new HeroAwakenAttrDetail()));

	private UnknownFieldSet _unknownFields;

	public const int TypeFieldNumber = 1;

	private EAttrSkillType type_;

	public const int ValueFieldNumber = 2;

	private float value_;

	public const int GrowthFieldNumber = 3;

	private float growth_;

	public const int CurrentMaxFieldNumber = 4;

	private float currentMax_;

	public const int NextMaxFieldNumber = 5;

	private float nextMax_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<HeroAwakenAttrDetail> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeHeroAwakenAttrDetailReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public EAttrSkillType Type
	{
		get
		{
			return type_;
		}
		set
		{
			type_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public float Value
	{
		get
		{
			return value_;
		}
		set
		{
			value_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public float Growth
	{
		get
		{
			return growth_;
		}
		set
		{
			growth_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public float CurrentMax
	{
		get
		{
			return currentMax_;
		}
		set
		{
			currentMax_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public float NextMax
	{
		get
		{
			return nextMax_;
		}
		set
		{
			nextMax_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public HeroAwakenAttrDetail()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public HeroAwakenAttrDetail(HeroAwakenAttrDetail other)
		: this()
	{
		type_ = other.type_;
		value_ = other.value_;
		growth_ = other.growth_;
		currentMax_ = other.currentMax_;
		nextMax_ = other.nextMax_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public HeroAwakenAttrDetail Clone()
	{
		return new HeroAwakenAttrDetail(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as HeroAwakenAttrDetail);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(HeroAwakenAttrDetail other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (Type != other.Type)
		{
			return false;
		}
		if (!ProtobufEqualityComparers.BitwiseSingleEqualityComparer.Equals(Value, other.Value))
		{
			return false;
		}
		if (!ProtobufEqualityComparers.BitwiseSingleEqualityComparer.Equals(Growth, other.Growth))
		{
			return false;
		}
		if (!ProtobufEqualityComparers.BitwiseSingleEqualityComparer.Equals(CurrentMax, other.CurrentMax))
		{
			return false;
		}
		if (!ProtobufEqualityComparers.BitwiseSingleEqualityComparer.Equals(NextMax, other.NextMax))
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
		if (Type != EAttrSkillType.EmptyValue)
		{
			num ^= Type.GetHashCode();
		}
		if (Value != 0f)
		{
			num ^= ProtobufEqualityComparers.BitwiseSingleEqualityComparer.GetHashCode(Value);
		}
		if (Growth != 0f)
		{
			num ^= ProtobufEqualityComparers.BitwiseSingleEqualityComparer.GetHashCode(Growth);
		}
		if (CurrentMax != 0f)
		{
			num ^= ProtobufEqualityComparers.BitwiseSingleEqualityComparer.GetHashCode(CurrentMax);
		}
		if (NextMax != 0f)
		{
			num ^= ProtobufEqualityComparers.BitwiseSingleEqualityComparer.GetHashCode(NextMax);
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
		if (Type != EAttrSkillType.EmptyValue)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteEnum((int)Type);
		}
		if (Value != 0f)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)21);
			((WriteContext)(ref output)).WriteFloat(Value);
		}
		if (Growth != 0f)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)29);
			((WriteContext)(ref output)).WriteFloat(Growth);
		}
		if (CurrentMax != 0f)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)37);
			((WriteContext)(ref output)).WriteFloat(CurrentMax);
		}
		if (NextMax != 0f)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)45);
			((WriteContext)(ref output)).WriteFloat(NextMax);
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
		if (Type != EAttrSkillType.EmptyValue)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)Type);
		}
		if (Value != 0f)
		{
			num += 5;
		}
		if (Growth != 0f)
		{
			num += 5;
		}
		if (CurrentMax != 0f)
		{
			num += 5;
		}
		if (NextMax != 0f)
		{
			num += 5;
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(HeroAwakenAttrDetail other)
	{
		if (other != null)
		{
			if (other.Type != EAttrSkillType.EmptyValue)
			{
				Type = other.Type;
			}
			if (other.Value != 0f)
			{
				Value = other.Value;
			}
			if (other.Growth != 0f)
			{
				Growth = other.Growth;
			}
			if (other.CurrentMax != 0f)
			{
				CurrentMax = other.CurrentMax;
			}
			if (other.NextMax != 0f)
			{
				NextMax = other.NextMax;
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
				Type = (EAttrSkillType)((ParseContext)(ref input)).ReadEnum();
				break;
			case 21u:
				Value = ((ParseContext)(ref input)).ReadFloat();
				break;
			case 29u:
				Growth = ((ParseContext)(ref input)).ReadFloat();
				break;
			case 37u:
				CurrentMax = ((ParseContext)(ref input)).ReadFloat();
				break;
			case 45u:
				NextMax = ((ParseContext)(ref input)).ReadFloat();
				break;
			}
		}
	}
}
