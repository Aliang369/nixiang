using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class OperateZhanLingDataAck : IMessage<OperateZhanLingDataAck>, IMessage, IEquatable<OperateZhanLingDataAck>, IDeepCloneable<OperateZhanLingDataAck>, IBufferMessage
{
	private static readonly MessageParser<OperateZhanLingDataAck> _parser = new MessageParser<OperateZhanLingDataAck>((Func<OperateZhanLingDataAck>)(() => new OperateZhanLingDataAck()));

	private UnknownFieldSet _unknownFields;

	public const int ZhanLingKillListFieldNumber = 1;

	private static readonly FieldCodec<ZhanLingKillDetail> _repeated_zhanLingKillList_codec = FieldCodec.ForMessage<ZhanLingKillDetail>(10u, ZhanLingKillDetail.Parser);

	private readonly RepeatedField<ZhanLingKillDetail> zhanLingKillList_ = new RepeatedField<ZhanLingKillDetail>();

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<OperateZhanLingDataAck> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeOperateZhanLingDataAckReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<ZhanLingKillDetail> ZhanLingKillList => zhanLingKillList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public OperateZhanLingDataAck()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public OperateZhanLingDataAck(OperateZhanLingDataAck other)
		: this()
	{
		zhanLingKillList_ = other.zhanLingKillList_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public OperateZhanLingDataAck Clone()
	{
		return new OperateZhanLingDataAck(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as OperateZhanLingDataAck);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(OperateZhanLingDataAck other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!zhanLingKillList_.Equals(other.zhanLingKillList_))
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
		num ^= ((object)zhanLingKillList_).GetHashCode();
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
		zhanLingKillList_.WriteTo(ref output, _repeated_zhanLingKillList_codec);
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
		num += zhanLingKillList_.CalculateSize(_repeated_zhanLingKillList_codec);
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(OperateZhanLingDataAck other)
	{
		if (other != null)
		{
			zhanLingKillList_.Add((IEnumerable<ZhanLingKillDetail>)other.zhanLingKillList_);
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
				zhanLingKillList_.AddEntriesFrom(ref input, _repeated_zhanLingKillList_codec);
			}
		}
	}
}
