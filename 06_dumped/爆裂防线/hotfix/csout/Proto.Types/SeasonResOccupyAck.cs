using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class SeasonResOccupyAck : IMessage<SeasonResOccupyAck>, IMessage, IEquatable<SeasonResOccupyAck>, IDeepCloneable<SeasonResOccupyAck>, IBufferMessage
{
	private static readonly MessageParser<SeasonResOccupyAck> _parser = new MessageParser<SeasonResOccupyAck>((Func<SeasonResOccupyAck>)(() => new SeasonResOccupyAck()));

	private UnknownFieldSet _unknownFields;

	public const int ResNodeIdFieldNumber = 1;

	private int resNodeId_;

	public const int ProducePerHourFieldNumber = 2;

	private int producePerHour_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<SeasonResOccupyAck> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeSeasonResOccupyAckReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int ResNodeId
	{
		get
		{
			return resNodeId_;
		}
		set
		{
			resNodeId_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int ProducePerHour
	{
		get
		{
			return producePerHour_;
		}
		set
		{
			producePerHour_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SeasonResOccupyAck()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SeasonResOccupyAck(SeasonResOccupyAck other)
		: this()
	{
		resNodeId_ = other.resNodeId_;
		producePerHour_ = other.producePerHour_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SeasonResOccupyAck Clone()
	{
		return new SeasonResOccupyAck(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as SeasonResOccupyAck);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(SeasonResOccupyAck other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (ResNodeId != other.ResNodeId)
		{
			return false;
		}
		if (ProducePerHour != other.ProducePerHour)
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
		if (ResNodeId != 0)
		{
			num ^= ResNodeId.GetHashCode();
		}
		if (ProducePerHour != 0)
		{
			num ^= ProducePerHour.GetHashCode();
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
		if (ResNodeId != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteInt32(ResNodeId);
		}
		if (ProducePerHour != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteInt32(ProducePerHour);
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
		if (ResNodeId != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(ResNodeId);
		}
		if (ProducePerHour != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(ProducePerHour);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(SeasonResOccupyAck other)
	{
		if (other != null)
		{
			if (other.ResNodeId != 0)
			{
				ResNodeId = other.ResNodeId;
			}
			if (other.ProducePerHour != 0)
			{
				ProducePerHour = other.ProducePerHour;
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
				ResNodeId = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 16u:
				ProducePerHour = ((ParseContext)(ref input)).ReadInt32();
				break;
			}
		}
	}
}
