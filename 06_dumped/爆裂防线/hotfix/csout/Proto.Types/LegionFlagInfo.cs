using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class LegionFlagInfo : IMessage<LegionFlagInfo>, IMessage, IEquatable<LegionFlagInfo>, IDeepCloneable<LegionFlagInfo>, IBufferMessage
{
	private static readonly MessageParser<LegionFlagInfo> _parser = new MessageParser<LegionFlagInfo>((Func<LegionFlagInfo>)(() => new LegionFlagInfo()));

	private UnknownFieldSet _unknownFields;

	public const int LegionIdFieldNumber = 1;

	private int legionId_;

	public const int LegionNameFieldNumber = 2;

	private string legionName_ = "";

	public const int IconFieldNumber = 3;

	private int icon_;

	public const int LevelFieldNumber = 4;

	private int level_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<LegionFlagInfo> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeLegionFlagInfoReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int LegionId
	{
		get
		{
			return legionId_;
		}
		set
		{
			legionId_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public string LegionName
	{
		get
		{
			return legionName_;
		}
		set
		{
			legionName_ = ProtoPreconditions.CheckNotNull<string>(value, "value");
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int Icon
	{
		get
		{
			return icon_;
		}
		set
		{
			icon_ = value;
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
	public LegionFlagInfo()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public LegionFlagInfo(LegionFlagInfo other)
		: this()
	{
		legionId_ = other.legionId_;
		legionName_ = other.legionName_;
		icon_ = other.icon_;
		level_ = other.level_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public LegionFlagInfo Clone()
	{
		return new LegionFlagInfo(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as LegionFlagInfo);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(LegionFlagInfo other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (LegionId != other.LegionId)
		{
			return false;
		}
		if (LegionName != other.LegionName)
		{
			return false;
		}
		if (Icon != other.Icon)
		{
			return false;
		}
		if (Level != other.Level)
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
		if (LegionId != 0)
		{
			num ^= LegionId.GetHashCode();
		}
		if (LegionName.Length != 0)
		{
			num ^= LegionName.GetHashCode();
		}
		if (Icon != 0)
		{
			num ^= Icon.GetHashCode();
		}
		if (Level != 0)
		{
			num ^= Level.GetHashCode();
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
		if (LegionId != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteInt32(LegionId);
		}
		if (LegionName.Length != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)18);
			((WriteContext)(ref output)).WriteString(LegionName);
		}
		if (Icon != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)24);
			((WriteContext)(ref output)).WriteInt32(Icon);
		}
		if (Level != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)32);
			((WriteContext)(ref output)).WriteInt32(Level);
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
		if (LegionId != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(LegionId);
		}
		if (LegionName.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(LegionName);
		}
		if (Icon != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(Icon);
		}
		if (Level != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(Level);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(LegionFlagInfo other)
	{
		if (other != null)
		{
			if (other.LegionId != 0)
			{
				LegionId = other.LegionId;
			}
			if (other.LegionName.Length != 0)
			{
				LegionName = other.LegionName;
			}
			if (other.Icon != 0)
			{
				Icon = other.Icon;
			}
			if (other.Level != 0)
			{
				Level = other.Level;
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
				LegionId = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 18u:
				LegionName = ((ParseContext)(ref input)).ReadString();
				break;
			case 24u:
				Icon = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 32u:
				Level = ((ParseContext)(ref input)).ReadInt32();
				break;
			}
		}
	}
}
