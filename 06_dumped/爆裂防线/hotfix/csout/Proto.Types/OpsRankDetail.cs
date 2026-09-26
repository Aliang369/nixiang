using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class OpsRankDetail : IMessage<OpsRankDetail>, IMessage, IEquatable<OpsRankDetail>, IDeepCloneable<OpsRankDetail>, IBufferMessage
{
	private static readonly MessageParser<OpsRankDetail> _parser = new MessageParser<OpsRankDetail>((Func<OpsRankDetail>)(() => new OpsRankDetail()));

	private UnknownFieldSet _unknownFields;

	public const int RankFieldNumber = 1;

	private int rank_;

	public const int RankLimitFieldNumber = 2;

	private int rankLimit_;

	public const int RankInfoFieldNumber = 3;

	private RankDetail rankInfo_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<OpsRankDetail> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeOpsRankDetailReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int Rank
	{
		get
		{
			return rank_;
		}
		set
		{
			rank_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int RankLimit
	{
		get
		{
			return rankLimit_;
		}
		set
		{
			rankLimit_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RankDetail RankInfo
	{
		get
		{
			return rankInfo_;
		}
		set
		{
			rankInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public OpsRankDetail()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public OpsRankDetail(OpsRankDetail other)
		: this()
	{
		rank_ = other.rank_;
		rankLimit_ = other.rankLimit_;
		rankInfo_ = ((other.rankInfo_ != null) ? other.rankInfo_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public OpsRankDetail Clone()
	{
		return new OpsRankDetail(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as OpsRankDetail);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(OpsRankDetail other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (Rank != other.Rank)
		{
			return false;
		}
		if (RankLimit != other.RankLimit)
		{
			return false;
		}
		if (!object.Equals(RankInfo, other.RankInfo))
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
		if (Rank != 0)
		{
			num ^= Rank.GetHashCode();
		}
		if (RankLimit != 0)
		{
			num ^= RankLimit.GetHashCode();
		}
		if (rankInfo_ != null)
		{
			num ^= RankInfo.GetHashCode();
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
		if (Rank != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteInt32(Rank);
		}
		if (RankLimit != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteInt32(RankLimit);
		}
		if (rankInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)26);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)RankInfo);
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
		if (Rank != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(Rank);
		}
		if (RankLimit != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(RankLimit);
		}
		if (rankInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)RankInfo);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(OpsRankDetail other)
	{
		if (other == null)
		{
			return;
		}
		if (other.Rank != 0)
		{
			Rank = other.Rank;
		}
		if (other.RankLimit != 0)
		{
			RankLimit = other.RankLimit;
		}
		if (other.rankInfo_ != null)
		{
			if (rankInfo_ == null)
			{
				RankInfo = new RankDetail();
			}
			RankInfo.MergeFrom(other.RankInfo);
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
			case 8u:
				Rank = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 16u:
				RankLimit = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 26u:
				if (rankInfo_ == null)
				{
					RankInfo = new RankDetail();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)RankInfo);
				break;
			}
		}
	}
}
