using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class GemPlanDetail : IMessage<GemPlanDetail>, IMessage, IEquatable<GemPlanDetail>, IDeepCloneable<GemPlanDetail>, IBufferMessage
{
	private static readonly MessageParser<GemPlanDetail> _parser = new MessageParser<GemPlanDetail>((Func<GemPlanDetail>)(() => new GemPlanDetail()));

	private UnknownFieldSet _unknownFields;

	public const int PlanIdFieldNumber = 1;

	private int planId_;

	public const int GemPlanSlotListFieldNumber = 2;

	private static readonly FieldCodec<GemPlanSlotDetail> _repeated_gemPlanSlotList_codec = FieldCodec.ForMessage<GemPlanSlotDetail>(18u, GemPlanSlotDetail.Parser);

	private readonly RepeatedField<GemPlanSlotDetail> gemPlanSlotList_ = new RepeatedField<GemPlanSlotDetail>();

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<GemPlanDetail> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeGemPlanDetailReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int PlanId
	{
		get
		{
			return planId_;
		}
		set
		{
			planId_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<GemPlanSlotDetail> GemPlanSlotList => gemPlanSlotList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public GemPlanDetail()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public GemPlanDetail(GemPlanDetail other)
		: this()
	{
		planId_ = other.planId_;
		gemPlanSlotList_ = other.gemPlanSlotList_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public GemPlanDetail Clone()
	{
		return new GemPlanDetail(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as GemPlanDetail);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(GemPlanDetail other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (PlanId != other.PlanId)
		{
			return false;
		}
		if (!gemPlanSlotList_.Equals(other.gemPlanSlotList_))
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
		if (PlanId != 0)
		{
			num ^= PlanId.GetHashCode();
		}
		num ^= ((object)gemPlanSlotList_).GetHashCode();
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
		if (PlanId != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteInt32(PlanId);
		}
		gemPlanSlotList_.WriteTo(ref output, _repeated_gemPlanSlotList_codec);
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
		if (PlanId != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(PlanId);
		}
		num += gemPlanSlotList_.CalculateSize(_repeated_gemPlanSlotList_codec);
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(GemPlanDetail other)
	{
		if (other != null)
		{
			if (other.PlanId != 0)
			{
				PlanId = other.PlanId;
			}
			gemPlanSlotList_.Add((IEnumerable<GemPlanSlotDetail>)other.gemPlanSlotList_);
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
				PlanId = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 18u:
				gemPlanSlotList_.AddEntriesFrom(ref input, _repeated_gemPlanSlotList_codec);
				break;
			}
		}
	}
}
