using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class WorldEventInfo : IMessage<WorldEventInfo>, IMessage, IEquatable<WorldEventInfo>, IDeepCloneable<WorldEventInfo>, IBufferMessage
{
	private static readonly MessageParser<WorldEventInfo> _parser = new MessageParser<WorldEventInfo>((Func<WorldEventInfo>)(() => new WorldEventInfo()));

	private UnknownFieldSet _unknownFields;

	public const int EventListFieldNumber = 1;

	private static readonly FieldCodec<WorldEventDetail> _repeated_eventList_codec = FieldCodec.ForMessage<WorldEventDetail>(10u, WorldEventDetail.Parser);

	private readonly RepeatedField<WorldEventDetail> eventList_ = new RepeatedField<WorldEventDetail>();

	public const int NextTmFieldNumber = 2;

	private long nextTm_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<WorldEventInfo> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeWorldEventInfoReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<WorldEventDetail> EventList => eventList_;

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
	public WorldEventInfo()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public WorldEventInfo(WorldEventInfo other)
		: this()
	{
		eventList_ = other.eventList_.Clone();
		nextTm_ = other.nextTm_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public WorldEventInfo Clone()
	{
		return new WorldEventInfo(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as WorldEventInfo);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(WorldEventInfo other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!eventList_.Equals(other.eventList_))
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
		num ^= ((object)eventList_).GetHashCode();
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
		eventList_.WriteTo(ref output, _repeated_eventList_codec);
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
		num += eventList_.CalculateSize(_repeated_eventList_codec);
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
	public void MergeFrom(WorldEventInfo other)
	{
		if (other != null)
		{
			eventList_.Add((IEnumerable<WorldEventDetail>)other.eventList_);
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
			case 10u:
				eventList_.AddEntriesFrom(ref input, _repeated_eventList_codec);
				break;
			case 16u:
				NextTm = ((ParseContext)(ref input)).ReadInt64();
				break;
			}
		}
	}
}
