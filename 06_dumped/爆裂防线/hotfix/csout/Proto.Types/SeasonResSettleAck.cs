using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class SeasonResSettleAck : IMessage<SeasonResSettleAck>, IMessage, IEquatable<SeasonResSettleAck>, IDeepCloneable<SeasonResSettleAck>, IBufferMessage
{
	private static readonly MessageParser<SeasonResSettleAck> _parser = new MessageParser<SeasonResSettleAck>((Func<SeasonResSettleAck>)(() => new SeasonResSettleAck()));

	private UnknownFieldSet _unknownFields;

	public const int GasGainFieldNumber = 1;

	private int gasGain_;

	public const int TechGainFieldNumber = 2;

	private int techGain_;

	public const int ProduceListFieldNumber = 3;

	private static readonly FieldCodec<SeasonProduceItem> _repeated_produceList_codec = FieldCodec.ForMessage<SeasonProduceItem>(26u, SeasonProduceItem.Parser);

	private readonly RepeatedField<SeasonProduceItem> produceList_ = new RepeatedField<SeasonProduceItem>();

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<SeasonResSettleAck> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeSeasonResSettleAckReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int GasGain
	{
		get
		{
			return gasGain_;
		}
		set
		{
			gasGain_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int TechGain
	{
		get
		{
			return techGain_;
		}
		set
		{
			techGain_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<SeasonProduceItem> ProduceList => produceList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SeasonResSettleAck()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SeasonResSettleAck(SeasonResSettleAck other)
		: this()
	{
		gasGain_ = other.gasGain_;
		techGain_ = other.techGain_;
		produceList_ = other.produceList_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SeasonResSettleAck Clone()
	{
		return new SeasonResSettleAck(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as SeasonResSettleAck);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(SeasonResSettleAck other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (GasGain != other.GasGain)
		{
			return false;
		}
		if (TechGain != other.TechGain)
		{
			return false;
		}
		if (!produceList_.Equals(other.produceList_))
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
		if (GasGain != 0)
		{
			num ^= GasGain.GetHashCode();
		}
		if (TechGain != 0)
		{
			num ^= TechGain.GetHashCode();
		}
		num ^= ((object)produceList_).GetHashCode();
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
		if (GasGain != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteInt32(GasGain);
		}
		if (TechGain != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteInt32(TechGain);
		}
		produceList_.WriteTo(ref output, _repeated_produceList_codec);
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
		if (GasGain != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(GasGain);
		}
		if (TechGain != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(TechGain);
		}
		num += produceList_.CalculateSize(_repeated_produceList_codec);
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(SeasonResSettleAck other)
	{
		if (other != null)
		{
			if (other.GasGain != 0)
			{
				GasGain = other.GasGain;
			}
			if (other.TechGain != 0)
			{
				TechGain = other.TechGain;
			}
			produceList_.Add((IEnumerable<SeasonProduceItem>)other.produceList_);
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
				GasGain = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 16u:
				TechGain = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 26u:
				produceList_.AddEntriesFrom(ref input, _repeated_produceList_codec);
				break;
			}
		}
	}
}
