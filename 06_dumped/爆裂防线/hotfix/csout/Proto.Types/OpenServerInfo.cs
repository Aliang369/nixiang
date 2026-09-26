using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class OpenServerInfo : IMessage<OpenServerInfo>, IMessage, IEquatable<OpenServerInfo>, IDeepCloneable<OpenServerInfo>, IBufferMessage
{
	private static readonly MessageParser<OpenServerInfo> _parser = new MessageParser<OpenServerInfo>((Func<OpenServerInfo>)(() => new OpenServerInfo()));

	private UnknownFieldSet _unknownFields;

	public const int RankBaseListFieldNumber = 1;

	private static readonly FieldCodec<OpsRankBaseDetail> _repeated_rankBaseList_codec = FieldCodec.ForMessage<OpsRankBaseDetail>(10u, OpsRankBaseDetail.Parser);

	private readonly RepeatedField<OpsRankBaseDetail> rankBaseList_ = new RepeatedField<OpsRankBaseDetail>();

	public const int RankEndTmFieldNumber = 2;

	private long rankEndTm_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<OpenServerInfo> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeOpenServerInfoReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<OpsRankBaseDetail> RankBaseList => rankBaseList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public long RankEndTm
	{
		get
		{
			return rankEndTm_;
		}
		set
		{
			rankEndTm_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public OpenServerInfo()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public OpenServerInfo(OpenServerInfo other)
		: this()
	{
		rankBaseList_ = other.rankBaseList_.Clone();
		rankEndTm_ = other.rankEndTm_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public OpenServerInfo Clone()
	{
		return new OpenServerInfo(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as OpenServerInfo);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(OpenServerInfo other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!rankBaseList_.Equals(other.rankBaseList_))
		{
			return false;
		}
		if (RankEndTm != other.RankEndTm)
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
		num ^= ((object)rankBaseList_).GetHashCode();
		if (RankEndTm != 0L)
		{
			num ^= RankEndTm.GetHashCode();
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
		rankBaseList_.WriteTo(ref output, _repeated_rankBaseList_codec);
		if (RankEndTm != 0L)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteInt64(RankEndTm);
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
		num += rankBaseList_.CalculateSize(_repeated_rankBaseList_codec);
		if (RankEndTm != 0L)
		{
			num += 1 + CodedOutputStream.ComputeInt64Size(RankEndTm);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(OpenServerInfo other)
	{
		if (other != null)
		{
			rankBaseList_.Add((IEnumerable<OpsRankBaseDetail>)other.rankBaseList_);
			if (other.RankEndTm != 0L)
			{
				RankEndTm = other.RankEndTm;
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
				rankBaseList_.AddEntriesFrom(ref input, _repeated_rankBaseList_codec);
				break;
			case 16u:
				RankEndTm = ((ParseContext)(ref input)).ReadInt64();
				break;
			}
		}
	}
}
