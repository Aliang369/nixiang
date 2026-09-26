using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class JourneySignDetail : IMessage<JourneySignDetail>, IMessage, IEquatable<JourneySignDetail>, IDeepCloneable<JourneySignDetail>, IBufferMessage
{
	private static readonly MessageParser<JourneySignDetail> _parser = new MessageParser<JourneySignDetail>((Func<JourneySignDetail>)(() => new JourneySignDetail()));

	private UnknownFieldSet _unknownFields;

	public const int DayFieldNumber = 1;

	private int day_;

	public const int ReachFieldNumber = 2;

	private int reach_;

	public const int TargetListFieldNumber = 3;

	private static readonly FieldCodec<JourneyTargetRecordDetail> _repeated_targetList_codec = FieldCodec.ForMessage<JourneyTargetRecordDetail>(26u, JourneyTargetRecordDetail.Parser);

	private readonly RepeatedField<JourneyTargetRecordDetail> targetList_ = new RepeatedField<JourneyTargetRecordDetail>();

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<JourneySignDetail> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeJourneySignDetailReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int Day
	{
		get
		{
			return day_;
		}
		set
		{
			day_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int Reach
	{
		get
		{
			return reach_;
		}
		set
		{
			reach_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<JourneyTargetRecordDetail> TargetList => targetList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public JourneySignDetail()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public JourneySignDetail(JourneySignDetail other)
		: this()
	{
		day_ = other.day_;
		reach_ = other.reach_;
		targetList_ = other.targetList_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public JourneySignDetail Clone()
	{
		return new JourneySignDetail(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as JourneySignDetail);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(JourneySignDetail other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (Day != other.Day)
		{
			return false;
		}
		if (Reach != other.Reach)
		{
			return false;
		}
		if (!targetList_.Equals(other.targetList_))
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
		if (Day != 0)
		{
			num ^= Day.GetHashCode();
		}
		if (Reach != 0)
		{
			num ^= Reach.GetHashCode();
		}
		num ^= ((object)targetList_).GetHashCode();
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
		if (Day != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteInt32(Day);
		}
		if (Reach != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteInt32(Reach);
		}
		targetList_.WriteTo(ref output, _repeated_targetList_codec);
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
		if (Day != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(Day);
		}
		if (Reach != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(Reach);
		}
		num += targetList_.CalculateSize(_repeated_targetList_codec);
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(JourneySignDetail other)
	{
		if (other != null)
		{
			if (other.Day != 0)
			{
				Day = other.Day;
			}
			if (other.Reach != 0)
			{
				Reach = other.Reach;
			}
			targetList_.Add((IEnumerable<JourneyTargetRecordDetail>)other.targetList_);
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
				Day = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 16u:
				Reach = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 26u:
				targetList_.AddEntriesFrom(ref input, _repeated_targetList_codec);
				break;
			}
		}
	}
}
