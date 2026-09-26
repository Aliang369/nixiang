using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class AuxPlayerSuggestAck : IMessage<AuxPlayerSuggestAck>, IMessage, IEquatable<AuxPlayerSuggestAck>, IDeepCloneable<AuxPlayerSuggestAck>, IBufferMessage
{
	private static readonly MessageParser<AuxPlayerSuggestAck> _parser = new MessageParser<AuxPlayerSuggestAck>((Func<AuxPlayerSuggestAck>)(() => new AuxPlayerSuggestAck()));

	private UnknownFieldSet _unknownFields;

	public const int SuggestListFieldNumber = 1;

	private static readonly FieldCodec<PlayerSuggestDetail> _repeated_suggestList_codec = FieldCodec.ForMessage<PlayerSuggestDetail>(10u, PlayerSuggestDetail.Parser);

	private readonly RepeatedField<PlayerSuggestDetail> suggestList_ = new RepeatedField<PlayerSuggestDetail>();

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<AuxPlayerSuggestAck> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeAuxPlayerSuggestAckReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<PlayerSuggestDetail> SuggestList => suggestList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public AuxPlayerSuggestAck()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public AuxPlayerSuggestAck(AuxPlayerSuggestAck other)
		: this()
	{
		suggestList_ = other.suggestList_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public AuxPlayerSuggestAck Clone()
	{
		return new AuxPlayerSuggestAck(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as AuxPlayerSuggestAck);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(AuxPlayerSuggestAck other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!suggestList_.Equals(other.suggestList_))
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
		num ^= ((object)suggestList_).GetHashCode();
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
		suggestList_.WriteTo(ref output, _repeated_suggestList_codec);
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
		num += suggestList_.CalculateSize(_repeated_suggestList_codec);
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(AuxPlayerSuggestAck other)
	{
		if (other != null)
		{
			suggestList_.Add((IEnumerable<PlayerSuggestDetail>)other.suggestList_);
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
				suggestList_.AddEntriesFrom(ref input, _repeated_suggestList_codec);
			}
		}
	}
}
