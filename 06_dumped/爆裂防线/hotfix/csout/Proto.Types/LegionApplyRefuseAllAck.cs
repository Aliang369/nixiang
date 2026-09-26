using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class LegionApplyRefuseAllAck : IMessage<LegionApplyRefuseAllAck>, IMessage, IEquatable<LegionApplyRefuseAllAck>, IDeepCloneable<LegionApplyRefuseAllAck>, IBufferMessage
{
	private static readonly MessageParser<LegionApplyRefuseAllAck> _parser = new MessageParser<LegionApplyRefuseAllAck>((Func<LegionApplyRefuseAllAck>)(() => new LegionApplyRefuseAllAck()));

	private UnknownFieldSet _unknownFields;

	public const int ApplyListFieldNumber = 1;

	private static readonly FieldCodec<LegionApplyDetail> _repeated_applyList_codec = FieldCodec.ForMessage<LegionApplyDetail>(10u, LegionApplyDetail.Parser);

	private readonly RepeatedField<LegionApplyDetail> applyList_ = new RepeatedField<LegionApplyDetail>();

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<LegionApplyRefuseAllAck> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeLegionApplyRefuseAllAckReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<LegionApplyDetail> ApplyList => applyList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public LegionApplyRefuseAllAck()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public LegionApplyRefuseAllAck(LegionApplyRefuseAllAck other)
		: this()
	{
		applyList_ = other.applyList_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public LegionApplyRefuseAllAck Clone()
	{
		return new LegionApplyRefuseAllAck(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as LegionApplyRefuseAllAck);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(LegionApplyRefuseAllAck other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!applyList_.Equals(other.applyList_))
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
		num ^= ((object)applyList_).GetHashCode();
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
		applyList_.WriteTo(ref output, _repeated_applyList_codec);
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
		num += applyList_.CalculateSize(_repeated_applyList_codec);
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(LegionApplyRefuseAllAck other)
	{
		if (other != null)
		{
			applyList_.Add((IEnumerable<LegionApplyDetail>)other.applyList_);
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
				applyList_.AddEntriesFrom(ref input, _repeated_applyList_codec);
			}
		}
	}
}
