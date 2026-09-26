using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class LegionSelfDonate : IMessage<LegionSelfDonate>, IMessage, IEquatable<LegionSelfDonate>, IDeepCloneable<LegionSelfDonate>, IBufferMessage
{
	private static readonly MessageParser<LegionSelfDonate> _parser = new MessageParser<LegionSelfDonate>((Func<LegionSelfDonate>)(() => new LegionSelfDonate()));

	private UnknownFieldSet _unknownFields;

	public const int DailyCountFieldNumber = 1;

	private int dailyCount_;

	public const int WeekDonateFieldNumber = 2;

	private int weekDonate_;

	public const int WeekTargetFieldNumber = 3;

	private static readonly FieldCodec<int> _repeated_weekTarget_codec = FieldCodec.ForInt32(26u);

	private readonly RepeatedField<int> weekTarget_ = new RepeatedField<int>();

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<LegionSelfDonate> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeLegionSelfDonateReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int DailyCount
	{
		get
		{
			return dailyCount_;
		}
		set
		{
			dailyCount_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int WeekDonate
	{
		get
		{
			return weekDonate_;
		}
		set
		{
			weekDonate_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<int> WeekTarget => weekTarget_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public LegionSelfDonate()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public LegionSelfDonate(LegionSelfDonate other)
		: this()
	{
		dailyCount_ = other.dailyCount_;
		weekDonate_ = other.weekDonate_;
		weekTarget_ = other.weekTarget_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public LegionSelfDonate Clone()
	{
		return new LegionSelfDonate(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as LegionSelfDonate);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(LegionSelfDonate other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (DailyCount != other.DailyCount)
		{
			return false;
		}
		if (WeekDonate != other.WeekDonate)
		{
			return false;
		}
		if (!weekTarget_.Equals(other.weekTarget_))
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
		if (DailyCount != 0)
		{
			num ^= DailyCount.GetHashCode();
		}
		if (WeekDonate != 0)
		{
			num ^= WeekDonate.GetHashCode();
		}
		num ^= ((object)weekTarget_).GetHashCode();
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
		if (DailyCount != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteInt32(DailyCount);
		}
		if (WeekDonate != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteInt32(WeekDonate);
		}
		weekTarget_.WriteTo(ref output, _repeated_weekTarget_codec);
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
		if (DailyCount != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(DailyCount);
		}
		if (WeekDonate != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(WeekDonate);
		}
		num += weekTarget_.CalculateSize(_repeated_weekTarget_codec);
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(LegionSelfDonate other)
	{
		if (other != null)
		{
			if (other.DailyCount != 0)
			{
				DailyCount = other.DailyCount;
			}
			if (other.WeekDonate != 0)
			{
				WeekDonate = other.WeekDonate;
			}
			weekTarget_.Add((IEnumerable<int>)other.weekTarget_);
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
				DailyCount = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 16u:
				WeekDonate = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 24u:
			case 26u:
				weekTarget_.AddEntriesFrom(ref input, _repeated_weekTarget_codec);
				break;
			}
		}
	}
}
