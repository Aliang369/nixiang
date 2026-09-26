using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class SnowVeilTurntableDetail : IMessage<SnowVeilTurntableDetail>, IMessage, IEquatable<SnowVeilTurntableDetail>, IDeepCloneable<SnowVeilTurntableDetail>, IBufferMessage
{
	private static readonly MessageParser<SnowVeilTurntableDetail> _parser = new MessageParser<SnowVeilTurntableDetail>((Func<SnowVeilTurntableDetail>)(() => new SnowVeilTurntableDetail()));

	private UnknownFieldSet _unknownFields;

	public const int RunCountFieldNumber = 1;

	private int runCount_;

	public const int SlotListFieldNumber = 2;

	private static readonly FieldCodec<SnowVeilTurntableSlotDetail> _repeated_slotList_codec = FieldCodec.ForMessage<SnowVeilTurntableSlotDetail>(18u, SnowVeilTurntableSlotDetail.Parser);

	private readonly RepeatedField<SnowVeilTurntableSlotDetail> slotList_ = new RepeatedField<SnowVeilTurntableSlotDetail>();

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<SnowVeilTurntableDetail> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeSnowVeilTurntableDetailReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int RunCount
	{
		get
		{
			return runCount_;
		}
		set
		{
			runCount_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<SnowVeilTurntableSlotDetail> SlotList => slotList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SnowVeilTurntableDetail()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SnowVeilTurntableDetail(SnowVeilTurntableDetail other)
		: this()
	{
		runCount_ = other.runCount_;
		slotList_ = other.slotList_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SnowVeilTurntableDetail Clone()
	{
		return new SnowVeilTurntableDetail(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as SnowVeilTurntableDetail);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(SnowVeilTurntableDetail other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (RunCount != other.RunCount)
		{
			return false;
		}
		if (!slotList_.Equals(other.slotList_))
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
		if (RunCount != 0)
		{
			num ^= RunCount.GetHashCode();
		}
		num ^= ((object)slotList_).GetHashCode();
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
		if (RunCount != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteInt32(RunCount);
		}
		slotList_.WriteTo(ref output, _repeated_slotList_codec);
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
		if (RunCount != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(RunCount);
		}
		num += slotList_.CalculateSize(_repeated_slotList_codec);
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(SnowVeilTurntableDetail other)
	{
		if (other != null)
		{
			if (other.RunCount != 0)
			{
				RunCount = other.RunCount;
			}
			slotList_.Add((IEnumerable<SnowVeilTurntableSlotDetail>)other.slotList_);
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
				RunCount = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 18u:
				slotList_.AddEntriesFrom(ref input, _repeated_slotList_codec);
				break;
			}
		}
	}
}
