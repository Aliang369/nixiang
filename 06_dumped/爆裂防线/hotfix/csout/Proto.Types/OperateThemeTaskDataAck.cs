using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class OperateThemeTaskDataAck : IMessage<OperateThemeTaskDataAck>, IMessage, IEquatable<OperateThemeTaskDataAck>, IDeepCloneable<OperateThemeTaskDataAck>, IBufferMessage
{
	private static readonly MessageParser<OperateThemeTaskDataAck> _parser = new MessageParser<OperateThemeTaskDataAck>((Func<OperateThemeTaskDataAck>)(() => new OperateThemeTaskDataAck()));

	private UnknownFieldSet _unknownFields;

	public const int ThemeTaskListFieldNumber = 1;

	private static readonly FieldCodec<ThemeTaskDetail> _repeated_themeTaskList_codec = FieldCodec.ForMessage<ThemeTaskDetail>(10u, ThemeTaskDetail.Parser);

	private readonly RepeatedField<ThemeTaskDetail> themeTaskList_ = new RepeatedField<ThemeTaskDetail>();

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<OperateThemeTaskDataAck> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeOperateThemeTaskDataAckReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<ThemeTaskDetail> ThemeTaskList => themeTaskList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public OperateThemeTaskDataAck()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public OperateThemeTaskDataAck(OperateThemeTaskDataAck other)
		: this()
	{
		themeTaskList_ = other.themeTaskList_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public OperateThemeTaskDataAck Clone()
	{
		return new OperateThemeTaskDataAck(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as OperateThemeTaskDataAck);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(OperateThemeTaskDataAck other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!themeTaskList_.Equals(other.themeTaskList_))
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
		num ^= ((object)themeTaskList_).GetHashCode();
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
		themeTaskList_.WriteTo(ref output, _repeated_themeTaskList_codec);
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
		num += themeTaskList_.CalculateSize(_repeated_themeTaskList_codec);
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(OperateThemeTaskDataAck other)
	{
		if (other != null)
		{
			themeTaskList_.Add((IEnumerable<ThemeTaskDetail>)other.themeTaskList_);
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
				themeTaskList_.AddEntriesFrom(ref input, _repeated_themeTaskList_codec);
			}
		}
	}
}
