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
public sealed class OpsRankInfo : IMessage<OpsRankInfo>, IMessage, IEquatable<OpsRankInfo>, IDeepCloneable<OpsRankInfo>, IBufferMessage
{
	private static readonly MessageParser<OpsRankInfo> _parser = new MessageParser<OpsRankInfo>((Func<OpsRankInfo>)(() => new OpsRankInfo()));

	private UnknownFieldSet _unknownFields;

	public const int RankTypeFieldNumber = 1;

	private ERankType rankType_;

	public const int RankListFieldNumber = 2;

	private static readonly FieldCodec<OpsRankDetail> _repeated_rankList_codec = FieldCodec.ForMessage<OpsRankDetail>(18u, OpsRankDetail.Parser);

	private readonly RepeatedField<OpsRankDetail> rankList_ = new RepeatedField<OpsRankDetail>();

	public const int RefreshTmFieldNumber = 3;

	private long refreshTm_;

	public const int SettleTmFieldNumber = 4;

	private long settleTm_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<OpsRankInfo> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeOpsRankInfoReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ERankType RankType
	{
		get
		{
			return rankType_;
		}
		set
		{
			rankType_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<OpsRankDetail> RankList => rankList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public long RefreshTm
	{
		get
		{
			return refreshTm_;
		}
		set
		{
			refreshTm_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public long SettleTm
	{
		get
		{
			return settleTm_;
		}
		set
		{
			settleTm_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public OpsRankInfo()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public OpsRankInfo(OpsRankInfo other)
		: this()
	{
		rankType_ = other.rankType_;
		rankList_ = other.rankList_.Clone();
		refreshTm_ = other.refreshTm_;
		settleTm_ = other.settleTm_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public OpsRankInfo Clone()
	{
		return new OpsRankInfo(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as OpsRankInfo);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(OpsRankInfo other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (RankType != other.RankType)
		{
			return false;
		}
		if (!rankList_.Equals(other.rankList_))
		{
			return false;
		}
		if (RefreshTm != other.RefreshTm)
		{
			return false;
		}
		if (SettleTm != other.SettleTm)
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
		if (RankType != ERankType.EmptyValue)
		{
			num ^= RankType.GetHashCode();
		}
		num ^= ((object)rankList_).GetHashCode();
		if (RefreshTm != 0L)
		{
			num ^= RefreshTm.GetHashCode();
		}
		if (SettleTm != 0L)
		{
			num ^= SettleTm.GetHashCode();
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
		if (RankType != ERankType.EmptyValue)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteEnum((int)RankType);
		}
		rankList_.WriteTo(ref output, _repeated_rankList_codec);
		if (RefreshTm != 0L)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)24);
			((WriteContext)(ref output)).WriteInt64(RefreshTm);
		}
		if (SettleTm != 0L)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)32);
			((WriteContext)(ref output)).WriteInt64(SettleTm);
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
		if (RankType != ERankType.EmptyValue)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)RankType);
		}
		num += rankList_.CalculateSize(_repeated_rankList_codec);
		if (RefreshTm != 0L)
		{
			num += 1 + CodedOutputStream.ComputeInt64Size(RefreshTm);
		}
		if (SettleTm != 0L)
		{
			num += 1 + CodedOutputStream.ComputeInt64Size(SettleTm);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(OpsRankInfo other)
	{
		if (other != null)
		{
			if (other.RankType != ERankType.EmptyValue)
			{
				RankType = other.RankType;
			}
			rankList_.Add((IEnumerable<OpsRankDetail>)other.rankList_);
			if (other.RefreshTm != 0L)
			{
				RefreshTm = other.RefreshTm;
			}
			if (other.SettleTm != 0L)
			{
				SettleTm = other.SettleTm;
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
			case 8u:
				RankType = (ERankType)((ParseContext)(ref input)).ReadEnum();
				break;
			case 18u:
				rankList_.AddEntriesFrom(ref input, _repeated_rankList_codec);
				break;
			case 24u:
				RefreshTm = ((ParseContext)(ref input)).ReadInt64();
				break;
			case 32u:
				SettleTm = ((ParseContext)(ref input)).ReadInt64();
				break;
			}
		}
	}
}
