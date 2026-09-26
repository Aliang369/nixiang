using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class OperateTacticRouletteSelectAck : IMessage<OperateTacticRouletteSelectAck>, IMessage, IEquatable<OperateTacticRouletteSelectAck>, IDeepCloneable<OperateTacticRouletteSelectAck>, IBufferMessage
{
	private static readonly MessageParser<OperateTacticRouletteSelectAck> _parser = new MessageParser<OperateTacticRouletteSelectAck>((Func<OperateTacticRouletteSelectAck>)(() => new OperateTacticRouletteSelectAck()));

	private UnknownFieldSet _unknownFields;

	public const int TacticRouletteListFieldNumber = 1;

	private static readonly FieldCodec<TacticRouletteDetail> _repeated_tacticRouletteList_codec = FieldCodec.ForMessage<TacticRouletteDetail>(10u, TacticRouletteDetail.Parser);

	private readonly RepeatedField<TacticRouletteDetail> tacticRouletteList_ = new RepeatedField<TacticRouletteDetail>();

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<OperateTacticRouletteSelectAck> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeOperateTacticRouletteSelectAckReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<TacticRouletteDetail> TacticRouletteList => tacticRouletteList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public OperateTacticRouletteSelectAck()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public OperateTacticRouletteSelectAck(OperateTacticRouletteSelectAck other)
		: this()
	{
		tacticRouletteList_ = other.tacticRouletteList_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public OperateTacticRouletteSelectAck Clone()
	{
		return new OperateTacticRouletteSelectAck(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as OperateTacticRouletteSelectAck);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(OperateTacticRouletteSelectAck other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!tacticRouletteList_.Equals(other.tacticRouletteList_))
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
		num ^= ((object)tacticRouletteList_).GetHashCode();
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
		tacticRouletteList_.WriteTo(ref output, _repeated_tacticRouletteList_codec);
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
		num += tacticRouletteList_.CalculateSize(_repeated_tacticRouletteList_codec);
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(OperateTacticRouletteSelectAck other)
	{
		if (other != null)
		{
			tacticRouletteList_.Add((IEnumerable<TacticRouletteDetail>)other.tacticRouletteList_);
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
				tacticRouletteList_.AddEntriesFrom(ref input, _repeated_tacticRouletteList_codec);
			}
		}
	}
}
