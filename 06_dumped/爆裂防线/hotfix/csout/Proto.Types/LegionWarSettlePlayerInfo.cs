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
public sealed class LegionWarSettlePlayerInfo : IMessage<LegionWarSettlePlayerInfo>, IMessage, IEquatable<LegionWarSettlePlayerInfo>, IDeepCloneable<LegionWarSettlePlayerInfo>, IBufferMessage
{
	private static readonly MessageParser<LegionWarSettlePlayerInfo> _parser = new MessageParser<LegionWarSettlePlayerInfo>((Func<LegionWarSettlePlayerInfo>)(() => new LegionWarSettlePlayerInfo()));

	private UnknownFieldSet _unknownFields;

	public const int ForceIdFieldNumber = 1;

	private ELegionWarForce forceId_;

	public const int ScoreCountFieldNumber = 2;

	private int scoreCount_;

	public const int RankListFieldNumber = 3;

	private static readonly FieldCodec<LegionWarPlayerRankDetail> _repeated_rankList_codec = FieldCodec.ForMessage<LegionWarPlayerRankDetail>(26u, LegionWarPlayerRankDetail.Parser);

	private readonly RepeatedField<LegionWarPlayerRankDetail> rankList_ = new RepeatedField<LegionWarPlayerRankDetail>();

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<LegionWarSettlePlayerInfo> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeLegionWarSettlePlayerInfoReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ELegionWarForce ForceId
	{
		get
		{
			return forceId_;
		}
		set
		{
			forceId_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int ScoreCount
	{
		get
		{
			return scoreCount_;
		}
		set
		{
			scoreCount_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<LegionWarPlayerRankDetail> RankList => rankList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public LegionWarSettlePlayerInfo()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public LegionWarSettlePlayerInfo(LegionWarSettlePlayerInfo other)
		: this()
	{
		forceId_ = other.forceId_;
		scoreCount_ = other.scoreCount_;
		rankList_ = other.rankList_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public LegionWarSettlePlayerInfo Clone()
	{
		return new LegionWarSettlePlayerInfo(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as LegionWarSettlePlayerInfo);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(LegionWarSettlePlayerInfo other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (ForceId != other.ForceId)
		{
			return false;
		}
		if (ScoreCount != other.ScoreCount)
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
		if (ForceId != ELegionWarForce.EmptyValue)
		{
			num ^= ForceId.GetHashCode();
		}
		if (ScoreCount != 0)
		{
			num ^= ScoreCount.GetHashCode();
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
		if (ForceId != ELegionWarForce.EmptyValue)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteEnum((int)ForceId);
		}
		if (ScoreCount != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteInt32(ScoreCount);
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
		if (ForceId != ELegionWarForce.EmptyValue)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)ForceId);
		}
		if (ScoreCount != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(ScoreCount);
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
	public void MergeFrom(LegionWarSettlePlayerInfo other)
	{
		if (other != null)
		{
			if (other.ForceId != ELegionWarForce.EmptyValue)
			{
				ForceId = other.ForceId;
			}
			if (other.ScoreCount != 0)
			{
				ScoreCount = other.ScoreCount;
			}
			rankList_.Add((IEnumerable<LegionWarPlayerRankDetail>)other.rankList_);
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
				ForceId = (ELegionWarForce)((ParseContext)(ref input)).ReadEnum();
				break;
			case 16u:
				ScoreCount = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 26u:
				rankList_.AddEntriesFrom(ref input, _repeated_rankList_codec);
				break;
			}
		}
	}
}
