using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class SeasonCityWarInfoAck : IMessage<SeasonCityWarInfoAck>, IMessage, IEquatable<SeasonCityWarInfoAck>, IDeepCloneable<SeasonCityWarInfoAck>, IBufferMessage
{
	private static readonly MessageParser<SeasonCityWarInfoAck> _parser = new MessageParser<SeasonCityWarInfoAck>((Func<SeasonCityWarInfoAck>)(() => new SeasonCityWarInfoAck()));

	private UnknownFieldSet _unknownFields;

	public const int StateFieldNumber = 1;

	private ESeasonCityWarState state_;

	public const int EndTmFieldNumber = 2;

	private long endTm_;

	public const int DeclaresFieldNumber = 3;

	private static readonly FieldCodec<SeasonDeclareInfo> _repeated_declares_codec = FieldCodec.ForMessage<SeasonDeclareInfo>(26u, SeasonDeclareInfo.Parser);

	private readonly RepeatedField<SeasonDeclareInfo> declares_ = new RepeatedField<SeasonDeclareInfo>();

	public const int RewardsFieldNumber = 4;

	private static readonly FieldCodec<SeasonOnlineReward> _repeated_rewards_codec = FieldCodec.ForMessage<SeasonOnlineReward>(34u, SeasonOnlineReward.Parser);

	private readonly RepeatedField<SeasonOnlineReward> rewards_ = new RepeatedField<SeasonOnlineReward>();

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<SeasonCityWarInfoAck> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeSeasonCityWarInfoAckReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ESeasonCityWarState State
	{
		get
		{
			return state_;
		}
		set
		{
			state_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public long EndTm
	{
		get
		{
			return endTm_;
		}
		set
		{
			endTm_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<SeasonDeclareInfo> Declares => declares_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<SeasonOnlineReward> Rewards => rewards_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SeasonCityWarInfoAck()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SeasonCityWarInfoAck(SeasonCityWarInfoAck other)
		: this()
	{
		state_ = other.state_;
		endTm_ = other.endTm_;
		declares_ = other.declares_.Clone();
		rewards_ = other.rewards_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SeasonCityWarInfoAck Clone()
	{
		return new SeasonCityWarInfoAck(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as SeasonCityWarInfoAck);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(SeasonCityWarInfoAck other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (State != other.State)
		{
			return false;
		}
		if (EndTm != other.EndTm)
		{
			return false;
		}
		if (!declares_.Equals(other.declares_))
		{
			return false;
		}
		if (!rewards_.Equals(other.rewards_))
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
		if (State != ESeasonCityWarState.EmptyValue)
		{
			num ^= State.GetHashCode();
		}
		if (EndTm != 0L)
		{
			num ^= EndTm.GetHashCode();
		}
		num ^= ((object)declares_).GetHashCode();
		num ^= ((object)rewards_).GetHashCode();
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
		if (State != ESeasonCityWarState.EmptyValue)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteEnum((int)State);
		}
		if (EndTm != 0L)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteInt64(EndTm);
		}
		declares_.WriteTo(ref output, _repeated_declares_codec);
		rewards_.WriteTo(ref output, _repeated_rewards_codec);
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
		if (State != ESeasonCityWarState.EmptyValue)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)State);
		}
		if (EndTm != 0L)
		{
			num += 1 + CodedOutputStream.ComputeInt64Size(EndTm);
		}
		num += declares_.CalculateSize(_repeated_declares_codec);
		num += rewards_.CalculateSize(_repeated_rewards_codec);
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(SeasonCityWarInfoAck other)
	{
		if (other != null)
		{
			if (other.State != ESeasonCityWarState.EmptyValue)
			{
				State = other.State;
			}
			if (other.EndTm != 0L)
			{
				EndTm = other.EndTm;
			}
			declares_.Add((IEnumerable<SeasonDeclareInfo>)other.declares_);
			rewards_.Add((IEnumerable<SeasonOnlineReward>)other.rewards_);
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
				State = (ESeasonCityWarState)((ParseContext)(ref input)).ReadEnum();
				break;
			case 16u:
				EndTm = ((ParseContext)(ref input)).ReadInt64();
				break;
			case 26u:
				declares_.AddEntriesFrom(ref input, _repeated_declares_codec);
				break;
			case 34u:
				rewards_.AddEntriesFrom(ref input, _repeated_rewards_codec);
				break;
			}
		}
	}
}
