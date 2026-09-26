using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class SeasonTeamPosAck : IMessage<SeasonTeamPosAck>, IMessage, IEquatable<SeasonTeamPosAck>, IDeepCloneable<SeasonTeamPosAck>, IBufferMessage
{
	private static readonly MessageParser<SeasonTeamPosAck> _parser = new MessageParser<SeasonTeamPosAck>((Func<SeasonTeamPosAck>)(() => new SeasonTeamPosAck()));

	private UnknownFieldSet _unknownFields;

	public const int TeamPosListFieldNumber = 1;

	private static readonly FieldCodec<SeasonTeamPosInfo> _repeated_teamPosList_codec = FieldCodec.ForMessage<SeasonTeamPosInfo>(10u, SeasonTeamPosInfo.Parser);

	private readonly RepeatedField<SeasonTeamPosInfo> teamPosList_ = new RepeatedField<SeasonTeamPosInfo>();

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<SeasonTeamPosAck> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeSeasonTeamPosAckReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<SeasonTeamPosInfo> TeamPosList => teamPosList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SeasonTeamPosAck()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SeasonTeamPosAck(SeasonTeamPosAck other)
		: this()
	{
		teamPosList_ = other.teamPosList_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SeasonTeamPosAck Clone()
	{
		return new SeasonTeamPosAck(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as SeasonTeamPosAck);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(SeasonTeamPosAck other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!teamPosList_.Equals(other.teamPosList_))
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
		num ^= ((object)teamPosList_).GetHashCode();
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
		teamPosList_.WriteTo(ref output, _repeated_teamPosList_codec);
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
		num += teamPosList_.CalculateSize(_repeated_teamPosList_codec);
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(SeasonTeamPosAck other)
	{
		if (other != null)
		{
			teamPosList_.Add((IEnumerable<SeasonTeamPosInfo>)other.teamPosList_);
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
				teamPosList_.AddEntriesFrom(ref input, _repeated_teamPosList_codec);
			}
		}
	}
}
