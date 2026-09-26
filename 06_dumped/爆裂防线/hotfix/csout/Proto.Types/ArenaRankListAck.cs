using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class ArenaRankListAck : IMessage<ArenaRankListAck>, IMessage, IEquatable<ArenaRankListAck>, IDeepCloneable<ArenaRankListAck>, IBufferMessage
{
	private static readonly MessageParser<ArenaRankListAck> _parser = new MessageParser<ArenaRankListAck>((Func<ArenaRankListAck>)(() => new ArenaRankListAck()));

	private UnknownFieldSet _unknownFields;

	public const int RankListFieldNumber = 1;

	private static readonly FieldCodec<RankDetail> _repeated_rankList_codec = FieldCodec.ForMessage<RankDetail>(10u, RankDetail.Parser);

	private readonly RepeatedField<RankDetail> rankList_ = new RepeatedField<RankDetail>();

	public const int SelfRankFieldNumber = 2;

	private RankData selfRank_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<ArenaRankListAck> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeArenaRankListAckReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<RankDetail> RankList => rankList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RankData SelfRank
	{
		get
		{
			return selfRank_;
		}
		set
		{
			selfRank_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ArenaRankListAck()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ArenaRankListAck(ArenaRankListAck other)
		: this()
	{
		rankList_ = other.rankList_.Clone();
		selfRank_ = ((other.selfRank_ != null) ? other.selfRank_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ArenaRankListAck Clone()
	{
		return new ArenaRankListAck(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as ArenaRankListAck);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(ArenaRankListAck other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!rankList_.Equals(other.rankList_))
		{
			return false;
		}
		if (!object.Equals(SelfRank, other.SelfRank))
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
		num ^= ((object)rankList_).GetHashCode();
		if (selfRank_ != null)
		{
			num ^= SelfRank.GetHashCode();
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
		rankList_.WriteTo(ref output, _repeated_rankList_codec);
		if (selfRank_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)18);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)SelfRank);
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
		num += rankList_.CalculateSize(_repeated_rankList_codec);
		if (selfRank_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)SelfRank);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(ArenaRankListAck other)
	{
		if (other == null)
		{
			return;
		}
		rankList_.Add((IEnumerable<RankDetail>)other.rankList_);
		if (other.selfRank_ != null)
		{
			if (selfRank_ == null)
			{
				SelfRank = new RankData();
			}
			SelfRank.MergeFrom(other.SelfRank);
		}
		_unknownFields = UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
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
				rankList_.AddEntriesFrom(ref input, _repeated_rankList_codec);
				break;
			case 18u:
				if (selfRank_ == null)
				{
					SelfRank = new RankData();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)SelfRank);
				break;
			}
		}
	}
}
