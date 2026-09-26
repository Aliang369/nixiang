using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class CrossPeakDataAck : IMessage<CrossPeakDataAck>, IMessage, IEquatable<CrossPeakDataAck>, IDeepCloneable<CrossPeakDataAck>, IBufferMessage
{
	private static readonly MessageParser<CrossPeakDataAck> _parser = new MessageParser<CrossPeakDataAck>((Func<CrossPeakDataAck>)(() => new CrossPeakDataAck()));

	private UnknownFieldSet _unknownFields;

	public const int CrossPeakSelfInfoFieldNumber = 1;

	private CrossPeakSelfInfo crossPeakSelfInfo_;

	public const int CrossPeakRankInfoFieldNumber = 2;

	private CrossPeakRankInfo crossPeakRankInfo_;

	public const int RankListFieldNumber = 3;

	private static readonly FieldCodec<CrossPeakRankDetail> _repeated_rankList_codec = FieldCodec.ForMessage<CrossPeakRankDetail>(26u, CrossPeakRankDetail.Parser);

	private readonly RepeatedField<CrossPeakRankDetail> rankList_ = new RepeatedField<CrossPeakRankDetail>();

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<CrossPeakDataAck> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeCrossPeakDataAckReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public CrossPeakSelfInfo CrossPeakSelfInfo
	{
		get
		{
			return crossPeakSelfInfo_;
		}
		set
		{
			crossPeakSelfInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public CrossPeakRankInfo CrossPeakRankInfo
	{
		get
		{
			return crossPeakRankInfo_;
		}
		set
		{
			crossPeakRankInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<CrossPeakRankDetail> RankList => rankList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public CrossPeakDataAck()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public CrossPeakDataAck(CrossPeakDataAck other)
		: this()
	{
		crossPeakSelfInfo_ = ((other.crossPeakSelfInfo_ != null) ? other.crossPeakSelfInfo_.Clone() : null);
		crossPeakRankInfo_ = ((other.crossPeakRankInfo_ != null) ? other.crossPeakRankInfo_.Clone() : null);
		rankList_ = other.rankList_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public CrossPeakDataAck Clone()
	{
		return new CrossPeakDataAck(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as CrossPeakDataAck);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(CrossPeakDataAck other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!object.Equals(CrossPeakSelfInfo, other.CrossPeakSelfInfo))
		{
			return false;
		}
		if (!object.Equals(CrossPeakRankInfo, other.CrossPeakRankInfo))
		{
			return false;
		}
		if (!rankList_.Equals(other.rankList_))
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
		if (crossPeakSelfInfo_ != null)
		{
			num ^= CrossPeakSelfInfo.GetHashCode();
		}
		if (crossPeakRankInfo_ != null)
		{
			num ^= CrossPeakRankInfo.GetHashCode();
		}
		num ^= ((object)rankList_).GetHashCode();
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
		if (crossPeakSelfInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)10);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)CrossPeakSelfInfo);
		}
		if (crossPeakRankInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)18);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)CrossPeakRankInfo);
		}
		rankList_.WriteTo(ref output, _repeated_rankList_codec);
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
		if (crossPeakSelfInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)CrossPeakSelfInfo);
		}
		if (crossPeakRankInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)CrossPeakRankInfo);
		}
		num += rankList_.CalculateSize(_repeated_rankList_codec);
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(CrossPeakDataAck other)
	{
		if (other == null)
		{
			return;
		}
		if (other.crossPeakSelfInfo_ != null)
		{
			if (crossPeakSelfInfo_ == null)
			{
				CrossPeakSelfInfo = new CrossPeakSelfInfo();
			}
			CrossPeakSelfInfo.MergeFrom(other.CrossPeakSelfInfo);
		}
		if (other.crossPeakRankInfo_ != null)
		{
			if (crossPeakRankInfo_ == null)
			{
				CrossPeakRankInfo = new CrossPeakRankInfo();
			}
			CrossPeakRankInfo.MergeFrom(other.CrossPeakRankInfo);
		}
		rankList_.Add((IEnumerable<CrossPeakRankDetail>)other.rankList_);
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
				if (crossPeakSelfInfo_ == null)
				{
					CrossPeakSelfInfo = new CrossPeakSelfInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)CrossPeakSelfInfo);
				break;
			case 18u:
				if (crossPeakRankInfo_ == null)
				{
					CrossPeakRankInfo = new CrossPeakRankInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)CrossPeakRankInfo);
				break;
			case 26u:
				rankList_.AddEntriesFrom(ref input, _repeated_rankList_codec);
				break;
			}
		}
	}
}
