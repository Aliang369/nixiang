using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class SeasonTechUpgradeAck : IMessage<SeasonTechUpgradeAck>, IMessage, IEquatable<SeasonTechUpgradeAck>, IDeepCloneable<SeasonTechUpgradeAck>, IBufferMessage
{
	private static readonly MessageParser<SeasonTechUpgradeAck> _parser = new MessageParser<SeasonTechUpgradeAck>((Func<SeasonTechUpgradeAck>)(() => new SeasonTechUpgradeAck()));

	private UnknownFieldSet _unknownFields;

	public const int TechIdFieldNumber = 1;

	private int techId_;

	public const int NewLevelFieldNumber = 2;

	private int newLevel_;

	public const int TechPointFieldNumber = 3;

	private int techPoint_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<SeasonTechUpgradeAck> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeSeasonTechUpgradeAckReflection.Descriptor.MessageTypes[0];

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
	public int NewLevel
	{
		get
		{
			return newLevel_;
		}
		set
		{
			newLevel_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int TechPoint
	{
		get
		{
			return techPoint_;
		}
		set
		{
			techPoint_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SeasonTechUpgradeAck()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SeasonTechUpgradeAck(SeasonTechUpgradeAck other)
		: this()
	{
		techId_ = other.techId_;
		newLevel_ = other.newLevel_;
		techPoint_ = other.techPoint_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SeasonTechUpgradeAck Clone()
	{
		return new SeasonTechUpgradeAck(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as SeasonTechUpgradeAck);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(SeasonTechUpgradeAck other)
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
		if (NewLevel != other.NewLevel)
		{
			return false;
		}
		if (TechPoint != other.TechPoint)
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
		if (NewLevel != 0)
		{
			num ^= NewLevel.GetHashCode();
		}
		if (TechPoint != 0)
		{
			num ^= TechPoint.GetHashCode();
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
		if (NewLevel != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteInt32(NewLevel);
		}
		if (TechPoint != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)24);
			((WriteContext)(ref output)).WriteInt32(TechPoint);
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
		if (NewLevel != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(NewLevel);
		}
		if (TechPoint != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(TechPoint);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(SeasonTechUpgradeAck other)
	{
		if (other != null)
		{
			if (other.TechId != 0)
			{
				TechId = other.TechId;
			}
			if (other.NewLevel != 0)
			{
				NewLevel = other.NewLevel;
			}
			if (other.TechPoint != 0)
			{
				TechPoint = other.TechPoint;
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
				NewLevel = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 24u:
				TechPoint = ((ParseContext)(ref input)).ReadInt32();
				break;
			}
		}
	}
}
