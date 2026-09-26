using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class SeasonCellPlayers : IMessage<SeasonCellPlayers>, IMessage, IEquatable<SeasonCellPlayers>, IDeepCloneable<SeasonCellPlayers>, IBufferMessage
{
	private static readonly MessageParser<SeasonCellPlayers> _parser = new MessageParser<SeasonCellPlayers>((Func<SeasonCellPlayers>)(() => new SeasonCellPlayers()));

	private UnknownFieldSet _unknownFields;

	public const int AreaIdFieldNumber = 1;

	private int areaId_;

	public const int StationaryFieldNumber = 2;

	private static readonly FieldCodec<SeasonPlayerPos> _repeated_stationary_codec = FieldCodec.ForMessage<SeasonPlayerPos>(18u, SeasonPlayerPos.Parser);

	private readonly RepeatedField<SeasonPlayerPos> stationary_ = new RepeatedField<SeasonPlayerPos>();

	public const int IncomingFieldNumber = 3;

	private static readonly FieldCodec<SeasonMoveDetail> _repeated_incoming_codec = FieldCodec.ForMessage<SeasonMoveDetail>(26u, SeasonMoveDetail.Parser);

	private readonly RepeatedField<SeasonMoveDetail> incoming_ = new RepeatedField<SeasonMoveDetail>();

	public const int DepartingFieldNumber = 4;

	private static readonly FieldCodec<SeasonMoveDetail> _repeated_departing_codec = FieldCodec.ForMessage<SeasonMoveDetail>(34u, SeasonMoveDetail.Parser);

	private readonly RepeatedField<SeasonMoveDetail> departing_ = new RepeatedField<SeasonMoveDetail>();

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<SeasonCellPlayers> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeSeasonCellPlayersReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int AreaId
	{
		get
		{
			return areaId_;
		}
		set
		{
			areaId_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<SeasonPlayerPos> Stationary => stationary_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<SeasonMoveDetail> Incoming => incoming_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<SeasonMoveDetail> Departing => departing_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SeasonCellPlayers()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SeasonCellPlayers(SeasonCellPlayers other)
		: this()
	{
		areaId_ = other.areaId_;
		stationary_ = other.stationary_.Clone();
		incoming_ = other.incoming_.Clone();
		departing_ = other.departing_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SeasonCellPlayers Clone()
	{
		return new SeasonCellPlayers(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as SeasonCellPlayers);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(SeasonCellPlayers other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (AreaId != other.AreaId)
		{
			return false;
		}
		if (!stationary_.Equals(other.stationary_))
		{
			return false;
		}
		if (!incoming_.Equals(other.incoming_))
		{
			return false;
		}
		if (!departing_.Equals(other.departing_))
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
		if (AreaId != 0)
		{
			num ^= AreaId.GetHashCode();
		}
		num ^= ((object)stationary_).GetHashCode();
		num ^= ((object)incoming_).GetHashCode();
		num ^= ((object)departing_).GetHashCode();
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
		if (AreaId != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteInt32(AreaId);
		}
		stationary_.WriteTo(ref output, _repeated_stationary_codec);
		incoming_.WriteTo(ref output, _repeated_incoming_codec);
		departing_.WriteTo(ref output, _repeated_departing_codec);
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
		if (AreaId != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(AreaId);
		}
		num += stationary_.CalculateSize(_repeated_stationary_codec);
		num += incoming_.CalculateSize(_repeated_incoming_codec);
		num += departing_.CalculateSize(_repeated_departing_codec);
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(SeasonCellPlayers other)
	{
		if (other != null)
		{
			if (other.AreaId != 0)
			{
				AreaId = other.AreaId;
			}
			stationary_.Add((IEnumerable<SeasonPlayerPos>)other.stationary_);
			incoming_.Add((IEnumerable<SeasonMoveDetail>)other.incoming_);
			departing_.Add((IEnumerable<SeasonMoveDetail>)other.departing_);
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
				AreaId = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 18u:
				stationary_.AddEntriesFrom(ref input, _repeated_stationary_codec);
				break;
			case 26u:
				incoming_.AddEntriesFrom(ref input, _repeated_incoming_codec);
				break;
			case 34u:
				departing_.AddEntriesFrom(ref input, _repeated_departing_codec);
				break;
			}
		}
	}
}
