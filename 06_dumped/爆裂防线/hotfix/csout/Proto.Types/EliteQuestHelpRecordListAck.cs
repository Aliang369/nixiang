using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class EliteQuestHelpRecordListAck : IMessage<EliteQuestHelpRecordListAck>, IMessage, IEquatable<EliteQuestHelpRecordListAck>, IDeepCloneable<EliteQuestHelpRecordListAck>, IBufferMessage
{
	private static readonly MessageParser<EliteQuestHelpRecordListAck> _parser = new MessageParser<EliteQuestHelpRecordListAck>((Func<EliteQuestHelpRecordListAck>)(() => new EliteQuestHelpRecordListAck()));

	private UnknownFieldSet _unknownFields;

	public const int RecordListFieldNumber = 1;

	private static readonly FieldCodec<HelpRecordDetail> _repeated_recordList_codec = FieldCodec.ForMessage<HelpRecordDetail>(10u, HelpRecordDetail.Parser);

	private readonly RepeatedField<HelpRecordDetail> recordList_ = new RepeatedField<HelpRecordDetail>();

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<EliteQuestHelpRecordListAck> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeEliteQuestHelpRecordListAckReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<HelpRecordDetail> RecordList => recordList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public EliteQuestHelpRecordListAck()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public EliteQuestHelpRecordListAck(EliteQuestHelpRecordListAck other)
		: this()
	{
		recordList_ = other.recordList_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public EliteQuestHelpRecordListAck Clone()
	{
		return new EliteQuestHelpRecordListAck(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as EliteQuestHelpRecordListAck);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(EliteQuestHelpRecordListAck other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!recordList_.Equals(other.recordList_))
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
		num ^= ((object)recordList_).GetHashCode();
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
		recordList_.WriteTo(ref output, _repeated_recordList_codec);
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
		num += recordList_.CalculateSize(_repeated_recordList_codec);
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(EliteQuestHelpRecordListAck other)
	{
		if (other != null)
		{
			recordList_.Add((IEnumerable<HelpRecordDetail>)other.recordList_);
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
			if (num != 10)
			{
				_unknownFields = UnknownFieldSet.MergeFieldFrom(_unknownFields, ref input);
			}
			else
			{
				recordList_.AddEntriesFrom(ref input, _repeated_recordList_codec);
			}
		}
	}
}
