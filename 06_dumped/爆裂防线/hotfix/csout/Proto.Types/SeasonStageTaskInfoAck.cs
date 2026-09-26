using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class SeasonStageTaskInfoAck : IMessage<SeasonStageTaskInfoAck>, IMessage, IEquatable<SeasonStageTaskInfoAck>, IDeepCloneable<SeasonStageTaskInfoAck>, IBufferMessage
{
	private static readonly MessageParser<SeasonStageTaskInfoAck> _parser = new MessageParser<SeasonStageTaskInfoAck>((Func<SeasonStageTaskInfoAck>)(() => new SeasonStageTaskInfoAck()));

	private UnknownFieldSet _unknownFields;

	public const int PhaseFieldNumber = 1;

	private int phase_;

	public const int TasksFieldNumber = 2;

	private static readonly FieldCodec<SeasonStageTaskItem> _repeated_tasks_codec = FieldCodec.ForMessage<SeasonStageTaskItem>(18u, SeasonStageTaskItem.Parser);

	private readonly RepeatedField<SeasonStageTaskItem> tasks_ = new RepeatedField<SeasonStageTaskItem>();

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<SeasonStageTaskInfoAck> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeSeasonStageTaskInfoAckReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int Phase
	{
		get
		{
			return phase_;
		}
		set
		{
			phase_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<SeasonStageTaskItem> Tasks => tasks_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SeasonStageTaskInfoAck()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SeasonStageTaskInfoAck(SeasonStageTaskInfoAck other)
		: this()
	{
		phase_ = other.phase_;
		tasks_ = other.tasks_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SeasonStageTaskInfoAck Clone()
	{
		return new SeasonStageTaskInfoAck(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as SeasonStageTaskInfoAck);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(SeasonStageTaskInfoAck other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (Phase != other.Phase)
		{
			return false;
		}
		if (!tasks_.Equals(other.tasks_))
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
		if (Phase != 0)
		{
			num ^= Phase.GetHashCode();
		}
		num ^= ((object)tasks_).GetHashCode();
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
		if (Phase != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteInt32(Phase);
		}
		tasks_.WriteTo(ref output, _repeated_tasks_codec);
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
		if (Phase != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(Phase);
		}
		num += tasks_.CalculateSize(_repeated_tasks_codec);
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(SeasonStageTaskInfoAck other)
	{
		if (other != null)
		{
			if (other.Phase != 0)
			{
				Phase = other.Phase;
			}
			tasks_.Add((IEnumerable<SeasonStageTaskItem>)other.tasks_);
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
				Phase = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 18u:
				tasks_.AddEntriesFrom(ref input, _repeated_tasks_codec);
				break;
			}
		}
	}
}
