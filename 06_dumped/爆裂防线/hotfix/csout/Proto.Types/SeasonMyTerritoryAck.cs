using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class SeasonMyTerritoryAck : IMessage<SeasonMyTerritoryAck>, IMessage, IEquatable<SeasonMyTerritoryAck>, IDeepCloneable<SeasonMyTerritoryAck>, IBufferMessage
{
	private static readonly MessageParser<SeasonMyTerritoryAck> _parser = new MessageParser<SeasonMyTerritoryAck>((Func<SeasonMyTerritoryAck>)(() => new SeasonMyTerritoryAck()));

	private UnknownFieldSet _unknownFields;

	public const int TerritoryListFieldNumber = 1;

	private static readonly FieldCodec<SeasonMyTerritory> _repeated_territoryList_codec = FieldCodec.ForMessage<SeasonMyTerritory>(10u, SeasonMyTerritory.Parser);

	private readonly RepeatedField<SeasonMyTerritory> territoryList_ = new RepeatedField<SeasonMyTerritory>();

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<SeasonMyTerritoryAck> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeSeasonMyTerritoryAckReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<SeasonMyTerritory> TerritoryList => territoryList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SeasonMyTerritoryAck()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SeasonMyTerritoryAck(SeasonMyTerritoryAck other)
		: this()
	{
		territoryList_ = other.territoryList_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SeasonMyTerritoryAck Clone()
	{
		return new SeasonMyTerritoryAck(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as SeasonMyTerritoryAck);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(SeasonMyTerritoryAck other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!territoryList_.Equals(other.territoryList_))
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
		num ^= ((object)territoryList_).GetHashCode();
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
		territoryList_.WriteTo(ref output, _repeated_territoryList_codec);
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
		num += territoryList_.CalculateSize(_repeated_territoryList_codec);
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(SeasonMyTerritoryAck other)
	{
		if (other != null)
		{
			territoryList_.Add((IEnumerable<SeasonMyTerritory>)other.territoryList_);
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
				territoryList_.AddEntriesFrom(ref input, _repeated_territoryList_codec);
			}
		}
	}
}
