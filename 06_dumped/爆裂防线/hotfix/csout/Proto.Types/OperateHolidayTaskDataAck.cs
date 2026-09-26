using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class OperateHolidayTaskDataAck : IMessage<OperateHolidayTaskDataAck>, IMessage, IEquatable<OperateHolidayTaskDataAck>, IDeepCloneable<OperateHolidayTaskDataAck>, IBufferMessage
{
	private static readonly MessageParser<OperateHolidayTaskDataAck> _parser = new MessageParser<OperateHolidayTaskDataAck>((Func<OperateHolidayTaskDataAck>)(() => new OperateHolidayTaskDataAck()));

	private UnknownFieldSet _unknownFields;

	public const int HolidayTaskListFieldNumber = 1;

	private static readonly FieldCodec<HolidayTaskDetail> _repeated_holidayTaskList_codec = FieldCodec.ForMessage<HolidayTaskDetail>(10u, HolidayTaskDetail.Parser);

	private readonly RepeatedField<HolidayTaskDetail> holidayTaskList_ = new RepeatedField<HolidayTaskDetail>();

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<OperateHolidayTaskDataAck> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeOperateHolidayTaskDataAckReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<HolidayTaskDetail> HolidayTaskList => holidayTaskList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public OperateHolidayTaskDataAck()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public OperateHolidayTaskDataAck(OperateHolidayTaskDataAck other)
		: this()
	{
		holidayTaskList_ = other.holidayTaskList_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public OperateHolidayTaskDataAck Clone()
	{
		return new OperateHolidayTaskDataAck(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as OperateHolidayTaskDataAck);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(OperateHolidayTaskDataAck other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!holidayTaskList_.Equals(other.holidayTaskList_))
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
		num ^= ((object)holidayTaskList_).GetHashCode();
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
		holidayTaskList_.WriteTo(ref output, _repeated_holidayTaskList_codec);
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
		num += holidayTaskList_.CalculateSize(_repeated_holidayTaskList_codec);
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(OperateHolidayTaskDataAck other)
	{
		if (other != null)
		{
			holidayTaskList_.Add((IEnumerable<HolidayTaskDetail>)other.holidayTaskList_);
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
				holidayTaskList_.AddEntriesFrom(ref input, _repeated_holidayTaskList_codec);
			}
		}
	}
}
