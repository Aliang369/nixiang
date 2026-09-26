using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class OpenServerRankTopAck : IMessage<OpenServerRankTopAck>, IMessage, IEquatable<OpenServerRankTopAck>, IDeepCloneable<OpenServerRankTopAck>, IBufferMessage
{
	private static readonly MessageParser<OpenServerRankTopAck> _parser = new MessageParser<OpenServerRankTopAck>((Func<OpenServerRankTopAck>)(() => new OpenServerRankTopAck()));

	private UnknownFieldSet _unknownFields;

	public const int RankTopListFieldNumber = 1;

	private static readonly FieldCodec<OpsRankTopDetail> _repeated_rankTopList_codec = FieldCodec.ForMessage<OpsRankTopDetail>(10u, OpsRankTopDetail.Parser);

	private readonly RepeatedField<OpsRankTopDetail> rankTopList_ = new RepeatedField<OpsRankTopDetail>();

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<OpenServerRankTopAck> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeOpenServerRankTopAckReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<OpsRankTopDetail> RankTopList => rankTopList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public OpenServerRankTopAck()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public OpenServerRankTopAck(OpenServerRankTopAck other)
		: this()
	{
		rankTopList_ = other.rankTopList_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public OpenServerRankTopAck Clone()
	{
		return new OpenServerRankTopAck(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as OpenServerRankTopAck);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(OpenServerRankTopAck other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!rankTopList_.Equals(other.rankTopList_))
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
		num ^= ((object)rankTopList_).GetHashCode();
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
		rankTopList_.WriteTo(ref output, _repeated_rankTopList_codec);
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
		num += rankTopList_.CalculateSize(_repeated_rankTopList_codec);
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(OpenServerRankTopAck other)
	{
		if (other != null)
		{
			rankTopList_.Add((IEnumerable<OpsRankTopDetail>)other.rankTopList_);
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
				rankTopList_.AddEntriesFrom(ref input, _repeated_rankTopList_codec);
			}
		}
	}
}
