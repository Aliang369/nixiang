using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class OperateMechaSubjectDataAck : IMessage<OperateMechaSubjectDataAck>, IMessage, IEquatable<OperateMechaSubjectDataAck>, IDeepCloneable<OperateMechaSubjectDataAck>, IBufferMessage
{
	private static readonly MessageParser<OperateMechaSubjectDataAck> _parser = new MessageParser<OperateMechaSubjectDataAck>((Func<OperateMechaSubjectDataAck>)(() => new OperateMechaSubjectDataAck()));

	private UnknownFieldSet _unknownFields;

	public const int MechaSubjectListFieldNumber = 1;

	private static readonly FieldCodec<MechaSubjectDetail> _repeated_mechaSubjectList_codec = FieldCodec.ForMessage<MechaSubjectDetail>(10u, MechaSubjectDetail.Parser);

	private readonly RepeatedField<MechaSubjectDetail> mechaSubjectList_ = new RepeatedField<MechaSubjectDetail>();

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<OperateMechaSubjectDataAck> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeOperateMechaSubjectDataAckReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<MechaSubjectDetail> MechaSubjectList => mechaSubjectList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public OperateMechaSubjectDataAck()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public OperateMechaSubjectDataAck(OperateMechaSubjectDataAck other)
		: this()
	{
		mechaSubjectList_ = other.mechaSubjectList_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public OperateMechaSubjectDataAck Clone()
	{
		return new OperateMechaSubjectDataAck(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as OperateMechaSubjectDataAck);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(OperateMechaSubjectDataAck other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!mechaSubjectList_.Equals(other.mechaSubjectList_))
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
		num ^= ((object)mechaSubjectList_).GetHashCode();
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
		mechaSubjectList_.WriteTo(ref output, _repeated_mechaSubjectList_codec);
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
		num += mechaSubjectList_.CalculateSize(_repeated_mechaSubjectList_codec);
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(OperateMechaSubjectDataAck other)
	{
		if (other != null)
		{
			mechaSubjectList_.Add((IEnumerable<MechaSubjectDetail>)other.mechaSubjectList_);
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
				mechaSubjectList_.AddEntriesFrom(ref input, _repeated_mechaSubjectList_codec);
			}
		}
	}
}
