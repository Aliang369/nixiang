using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class SeasonTechNode : IMessage<SeasonTechNode>, IMessage, IEquatable<SeasonTechNode>, IDeepCloneable<SeasonTechNode>, IBufferMessage
{
	private static readonly MessageParser<SeasonTechNode> _parser = new MessageParser<SeasonTechNode>((Func<SeasonTechNode>)(() => new SeasonTechNode()));

	private UnknownFieldSet _unknownFields;

	public const int TechIdFieldNumber = 1;

	private int techId_;

	public const int CategoryFieldNumber = 2;

	private int category_;

	public const int LevelFieldNumber = 3;

	private int level_;

	public const int MaxLevelFieldNumber = 4;

	private int maxLevel_;

	public const int NeedPointFieldNumber = 5;

	private int needPoint_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<SeasonTechNode> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeSeasonTechNodeReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int TechId
	{
		get
		{
			return techId_;
		}
		set
		{
			techId_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int Category
	{
		get
		{
			return category_;
		}
		set
		{
			category_ = value;
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
	public int MaxLevel
	{
		get
		{
			return maxLevel_;
		}
		set
		{
			maxLevel_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int NeedPoint
	{
		get
		{
			return needPoint_;
		}
		set
		{
			needPoint_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SeasonTechNode()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SeasonTechNode(SeasonTechNode other)
		: this()
	{
		techId_ = other.techId_;
		category_ = other.category_;
		level_ = other.level_;
		maxLevel_ = other.maxLevel_;
		needPoint_ = other.needPoint_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SeasonTechNode Clone()
	{
		return new SeasonTechNode(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as SeasonTechNode);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(SeasonTechNode other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (TechId != other.TechId)
		{
			return false;
		}
		if (Category != other.Category)
		{
			return false;
		}
		if (Level != other.Level)
		{
			return false;
		}
		if (MaxLevel != other.MaxLevel)
		{
			return false;
		}
		if (NeedPoint != other.NeedPoint)
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
		if (TechId != 0)
		{
			num ^= TechId.GetHashCode();
		}
		if (Category != 0)
		{
			num ^= Category.GetHashCode();
		}
		if (Level != 0)
		{
			num ^= Level.GetHashCode();
		}
		if (MaxLevel != 0)
		{
			num ^= MaxLevel.GetHashCode();
		}
		if (NeedPoint != 0)
		{
			num ^= NeedPoint.GetHashCode();
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
		if (TechId != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteInt32(TechId);
		}
		if (Category != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteInt32(Category);
		}
		if (Level != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)24);
			((WriteContext)(ref output)).WriteInt32(Level);
		}
		if (MaxLevel != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)32);
			((WriteContext)(ref output)).WriteInt32(MaxLevel);
		}
		if (NeedPoint != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)40);
			((WriteContext)(ref output)).WriteInt32(NeedPoint);
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
		if (TechId != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(TechId);
		}
		if (Category != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(Category);
		}
		if (Level != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(Level);
		}
		if (MaxLevel != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(MaxLevel);
		}
		if (NeedPoint != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(NeedPoint);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(SeasonTechNode other)
	{
		if (other != null)
		{
			if (other.TechId != 0)
			{
				TechId = other.TechId;
			}
			if (other.Category != 0)
			{
				Category = other.Category;
			}
			if (other.Level != 0)
			{
				Level = other.Level;
			}
			if (other.MaxLevel != 0)
			{
				MaxLevel = other.MaxLevel;
			}
			if (other.NeedPoint != 0)
			{
				NeedPoint = other.NeedPoint;
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
				TechId = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 16u:
				Category = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 24u:
				Level = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 32u:
				MaxLevel = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 40u:
				NeedPoint = ((ParseContext)(ref input)).ReadInt32();
				break;
			}
		}
	}
}
