using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class OperateAdsZhanLingDataAck : IMessage<OperateAdsZhanLingDataAck>, IMessage, IEquatable<OperateAdsZhanLingDataAck>, IDeepCloneable<OperateAdsZhanLingDataAck>, IBufferMessage
{
	private static readonly MessageParser<OperateAdsZhanLingDataAck> _parser = new MessageParser<OperateAdsZhanLingDataAck>((Func<OperateAdsZhanLingDataAck>)(() => new OperateAdsZhanLingDataAck()));

	private UnknownFieldSet _unknownFields;

	public const int AdsZhanLingListFieldNumber = 1;

	private static readonly FieldCodec<AdsZhanLingDetail> _repeated_adsZhanLingList_codec = FieldCodec.ForMessage<AdsZhanLingDetail>(10u, AdsZhanLingDetail.Parser);

	private readonly RepeatedField<AdsZhanLingDetail> adsZhanLingList_ = new RepeatedField<AdsZhanLingDetail>();

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<OperateAdsZhanLingDataAck> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeOperateAdsZhanLingDataAckReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<AdsZhanLingDetail> AdsZhanLingList => adsZhanLingList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public OperateAdsZhanLingDataAck()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public OperateAdsZhanLingDataAck(OperateAdsZhanLingDataAck other)
		: this()
	{
		adsZhanLingList_ = other.adsZhanLingList_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public OperateAdsZhanLingDataAck Clone()
	{
		return new OperateAdsZhanLingDataAck(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as OperateAdsZhanLingDataAck);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(OperateAdsZhanLingDataAck other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!adsZhanLingList_.Equals(other.adsZhanLingList_))
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
		num ^= ((object)adsZhanLingList_).GetHashCode();
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
		adsZhanLingList_.WriteTo(ref output, _repeated_adsZhanLingList_codec);
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
		num += adsZhanLingList_.CalculateSize(_repeated_adsZhanLingList_codec);
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(OperateAdsZhanLingDataAck other)
	{
		if (other != null)
		{
			adsZhanLingList_.Add((IEnumerable<AdsZhanLingDetail>)other.adsZhanLingList_);
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
				adsZhanLingList_.AddEntriesFrom(ref input, _repeated_adsZhanLingList_codec);
			}
		}
	}
}
