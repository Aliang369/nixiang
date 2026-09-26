using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class SeasonTechInfoAck : IMessage<SeasonTechInfoAck>, IMessage, IEquatable<SeasonTechInfoAck>, IDeepCloneable<SeasonTechInfoAck>, IBufferMessage
{
	private static readonly MessageParser<SeasonTechInfoAck> _parser = new MessageParser<SeasonTechInfoAck>((Func<SeasonTechInfoAck>)(() => new SeasonTechInfoAck()));

	private UnknownFieldSet _unknownFields;

	public const int TechPointFieldNumber = 1;

	private int techPoint_;

	public const int NodeListFieldNumber = 2;

	private static readonly FieldCodec<SeasonTechNode> _repeated_nodeList_codec = FieldCodec.ForMessage<SeasonTechNode>(18u, SeasonTechNode.Parser);

	private readonly RepeatedField<SeasonTechNode> nodeList_ = new RepeatedField<SeasonTechNode>();

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<SeasonTechInfoAck> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeSeasonTechInfoAckReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

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
	public RepeatedField<SeasonTechNode> NodeList => nodeList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SeasonTechInfoAck()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SeasonTechInfoAck(SeasonTechInfoAck other)
		: this()
	{
		techPoint_ = other.techPoint_;
		nodeList_ = other.nodeList_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SeasonTechInfoAck Clone()
	{
		return new SeasonTechInfoAck(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as SeasonTechInfoAck);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(SeasonTechInfoAck other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (TechPoint != other.TechPoint)
		{
			return false;
		}
		if (!nodeList_.Equals(other.nodeList_))
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
		if (TechPoint != 0)
		{
			num ^= TechPoint.GetHashCode();
		}
		num ^= ((object)nodeList_).GetHashCode();
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
		if (TechPoint != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteInt32(TechPoint);
		}
		nodeList_.WriteTo(ref output, _repeated_nodeList_codec);
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
		if (TechPoint != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(TechPoint);
		}
		num += nodeList_.CalculateSize(_repeated_nodeList_codec);
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(SeasonTechInfoAck other)
	{
		if (other != null)
		{
			if (other.TechPoint != 0)
			{
				TechPoint = other.TechPoint;
			}
			nodeList_.Add((IEnumerable<SeasonTechNode>)other.nodeList_);
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
				TechPoint = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 18u:
				nodeList_.AddEntriesFrom(ref input, _repeated_nodeList_codec);
				break;
			}
		}
	}
}
