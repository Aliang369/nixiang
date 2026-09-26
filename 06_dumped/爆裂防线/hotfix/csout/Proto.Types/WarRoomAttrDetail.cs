using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class WarRoomAttrDetail : IMessage<WarRoomAttrDetail>, IMessage, IEquatable<WarRoomAttrDetail>, IDeepCloneable<WarRoomAttrDetail>, IBufferMessage
{
	private static readonly MessageParser<WarRoomAttrDetail> _parser = new MessageParser<WarRoomAttrDetail>((Func<WarRoomAttrDetail>)(() => new WarRoomAttrDetail()));

	private UnknownFieldSet _unknownFields;

	public const int TypeFieldNumber = 1;

	private EAttrSkillType type_;

	public const int ValueFieldNumber = 2;

	private float value_;

	public const int GrowthFieldNumber = 3;

	private float growth_;

	public const int FromInfoFieldNumber = 4;

	private WarRoomAttrFromInfo fromInfo_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<WarRoomAttrDetail> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeWarRoomAttrDetailReflection.Descriptor.MessageTypes[0];

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
	public WarRoomAttrFromInfo FromInfo
	{
		get
		{
			return fromInfo_;
		}
		set
		{
			fromInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public WarRoomAttrDetail()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public WarRoomAttrDetail(WarRoomAttrDetail other)
		: this()
	{
		type_ = other.type_;
		value_ = other.value_;
		growth_ = other.growth_;
		fromInfo_ = ((other.fromInfo_ != null) ? other.fromInfo_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public WarRoomAttrDetail Clone()
	{
		return new WarRoomAttrDetail(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as WarRoomAttrDetail);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(WarRoomAttrDetail other)
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
		if (!object.Equals(FromInfo, other.FromInfo))
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
		if (fromInfo_ != null)
		{
			num ^= FromInfo.GetHashCode();
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
		if (fromInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)34);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)FromInfo);
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
		if (fromInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)FromInfo);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(WarRoomAttrDetail other)
	{
		if (other == null)
		{
			return;
		}
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
		if (other.fromInfo_ != null)
		{
			if (fromInfo_ == null)
			{
				FromInfo = new WarRoomAttrFromInfo();
			}
			FromInfo.MergeFrom(other.FromInfo);
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
			case 8u:
				Type = (EAttrSkillType)((ParseContext)(ref input)).ReadEnum();
				break;
			case 21u:
				Value = ((ParseContext)(ref input)).ReadFloat();
				break;
			case 29u:
				Growth = ((ParseContext)(ref input)).ReadFloat();
				break;
			case 34u:
				if (fromInfo_ == null)
				{
					FromInfo = new WarRoomAttrFromInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)FromInfo);
				break;
			}
		}
	}
}
