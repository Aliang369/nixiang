using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class SeasonAirdropListAck : IMessage<SeasonAirdropListAck>, IMessage, IEquatable<SeasonAirdropListAck>, IDeepCloneable<SeasonAirdropListAck>, IBufferMessage
{
	private static readonly MessageParser<SeasonAirdropListAck> _parser = new MessageParser<SeasonAirdropListAck>((Func<SeasonAirdropListAck>)(() => new SeasonAirdropListAck()));

	private UnknownFieldSet _unknownFields;

	public const int ListFieldNumber = 1;

	private static readonly FieldCodec<SeasonAirdropInfo> _repeated_list_codec = FieldCodec.ForMessage<SeasonAirdropInfo>(10u, SeasonAirdropInfo.Parser);

	private readonly RepeatedField<SeasonAirdropInfo> list_ = new RepeatedField<SeasonAirdropInfo>();

	public const int NextRefreshTmFieldNumber = 2;

	private long nextRefreshTm_;

	public const int DailyWinFieldNumber = 3;

	private int dailyWin_;

	public const int DailyLimitFieldNumber = 4;

	private int dailyLimit_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<SeasonAirdropListAck> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeSeasonAirdropListAckReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<SeasonAirdropInfo> List => list_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public long NextRefreshTm
	{
		get
		{
			return nextRefreshTm_;
		}
		set
		{
			nextRefreshTm_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int DailyWin
	{
		get
		{
			return dailyWin_;
		}
		set
		{
			dailyWin_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int DailyLimit
	{
		get
		{
			return dailyLimit_;
		}
		set
		{
			dailyLimit_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SeasonAirdropListAck()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SeasonAirdropListAck(SeasonAirdropListAck other)
		: this()
	{
		list_ = other.list_.Clone();
		nextRefreshTm_ = other.nextRefreshTm_;
		dailyWin_ = other.dailyWin_;
		dailyLimit_ = other.dailyLimit_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SeasonAirdropListAck Clone()
	{
		return new SeasonAirdropListAck(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as SeasonAirdropListAck);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(SeasonAirdropListAck other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!list_.Equals(other.list_))
		{
			return false;
		}
		if (NextRefreshTm != other.NextRefreshTm)
		{
			return false;
		}
		if (DailyWin != other.DailyWin)
		{
			return false;
		}
		if (DailyLimit != other.DailyLimit)
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
		num ^= ((object)list_).GetHashCode();
		if (NextRefreshTm != 0L)
		{
			num ^= NextRefreshTm.GetHashCode();
		}
		if (DailyWin != 0)
		{
			num ^= DailyWin.GetHashCode();
		}
		if (DailyLimit != 0)
		{
			num ^= DailyLimit.GetHashCode();
		}
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
		list_.WriteTo(ref output, _repeated_list_codec);
		if (NextRefreshTm != 0L)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteInt64(NextRefreshTm);
		}
		if (DailyWin != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)24);
			((WriteContext)(ref output)).WriteInt32(DailyWin);
		}
		if (DailyLimit != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)32);
			((WriteContext)(ref output)).WriteInt32(DailyLimit);
		}
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
		num += list_.CalculateSize(_repeated_list_codec);
		if (NextRefreshTm != 0L)
		{
			num += 1 + CodedOutputStream.ComputeInt64Size(NextRefreshTm);
		}
		if (DailyWin != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(DailyWin);
		}
		if (DailyLimit != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(DailyLimit);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(SeasonAirdropListAck other)
	{
		if (other != null)
		{
			list_.Add((IEnumerable<SeasonAirdropInfo>)other.list_);
			if (other.NextRefreshTm != 0L)
			{
				NextRefreshTm = other.NextRefreshTm;
			}
			if (other.DailyWin != 0)
			{
				DailyWin = other.DailyWin;
			}
			if (other.DailyLimit != 0)
			{
				DailyLimit = other.DailyLimit;
			}
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
			case 10u:
				list_.AddEntriesFrom(ref input, _repeated_list_codec);
				break;
			case 16u:
				NextRefreshTm = ((ParseContext)(ref input)).ReadInt64();
				break;
			case 24u:
				DailyWin = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 32u:
				DailyLimit = ((ParseContext)(ref input)).ReadInt32();
				break;
			}
		}
	}
}
