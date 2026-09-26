using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class TrainMatchInfo : IMessage<TrainMatchInfo>, IMessage, IEquatable<TrainMatchInfo>, IDeepCloneable<TrainMatchInfo>, IBufferMessage
{
	private static readonly MessageParser<TrainMatchInfo> _parser = new MessageParser<TrainMatchInfo>((Func<TrainMatchInfo>)(() => new TrainMatchInfo()));

	private UnknownFieldSet _unknownFields;

	public const int TransportIdFieldNumber = 1;

	private int transportId_;

	public const int NextTmFieldNumber = 2;

	private long nextTm_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<TrainMatchInfo> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeTrainMatchInfoReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int TransportId
	{
		get
		{
			return transportId_;
		}
		set
		{
			transportId_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public long NextTm
	{
		get
		{
			return nextTm_;
		}
		set
		{
			nextTm_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public TrainMatchInfo()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public TrainMatchInfo(TrainMatchInfo other)
		: this()
	{
		transportId_ = other.transportId_;
		nextTm_ = other.nextTm_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public TrainMatchInfo Clone()
	{
		return new TrainMatchInfo(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as TrainMatchInfo);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(TrainMatchInfo other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (TransportId != other.TransportId)
		{
			return false;
		}
		if (NextTm != other.NextTm)
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
		if (TransportId != 0)
		{
			num ^= TransportId.GetHashCode();
		}
		if (NextTm != 0L)
		{
			num ^= NextTm.GetHashCode();
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
		if (TransportId != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteInt32(TransportId);
		}
		if (NextTm != 0L)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteInt64(NextTm);
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
		if (TransportId != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(TransportId);
		}
		if (NextTm != 0L)
		{
			num += 1 + CodedOutputStream.ComputeInt64Size(NextTm);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(TrainMatchInfo other)
	{
		if (other != null)
		{
			if (other.TransportId != 0)
			{
				TransportId = other.TransportId;
			}
			if (other.NextTm != 0L)
			{
				NextTm = other.NextTm;
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
				TransportId = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 16u:
				NextTm = ((ParseContext)(ref input)).ReadInt64();
				break;
			}
		}
	}
}
