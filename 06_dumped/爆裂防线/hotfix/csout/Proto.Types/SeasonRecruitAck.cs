using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class SeasonRecruitAck : IMessage<SeasonRecruitAck>, IMessage, IEquatable<SeasonRecruitAck>, IDeepCloneable<SeasonRecruitAck>, IBufferMessage
{
	private static readonly MessageParser<SeasonRecruitAck> _parser = new MessageParser<SeasonRecruitAck>((Func<SeasonRecruitAck>)(() => new SeasonRecruitAck()));

	private UnknownFieldSet _unknownFields;

	public const int MeritFieldNumber = 1;

	private int merit_;

	public const int GuardCountFieldNumber = 2;

	private int guardCount_;

	public const int GuardMaxFieldNumber = 3;

	private int guardMax_;

	public const int DailyUsedFieldNumber = 4;

	private int dailyUsed_;

	public const int DailyLimitFieldNumber = 5;

	private int dailyLimit_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<SeasonRecruitAck> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeSeasonRecruitAckReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int Merit
	{
		get
		{
			return merit_;
		}
		set
		{
			merit_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int GuardCount
	{
		get
		{
			return guardCount_;
		}
		set
		{
			guardCount_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int GuardMax
	{
		get
		{
			return guardMax_;
		}
		set
		{
			guardMax_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int DailyUsed
	{
		get
		{
			return dailyUsed_;
		}
		set
		{
			dailyUsed_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int DailyLimit
	{
		get
		{
			return dailyLimit_;
		}
		set
		{
			dailyLimit_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SeasonRecruitAck()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SeasonRecruitAck(SeasonRecruitAck other)
		: this()
	{
		merit_ = other.merit_;
		guardCount_ = other.guardCount_;
		guardMax_ = other.guardMax_;
		dailyUsed_ = other.dailyUsed_;
		dailyLimit_ = other.dailyLimit_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SeasonRecruitAck Clone()
	{
		return new SeasonRecruitAck(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as SeasonRecruitAck);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(SeasonRecruitAck other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (Merit != other.Merit)
		{
			return false;
		}
		if (GuardCount != other.GuardCount)
		{
			return false;
		}
		if (GuardMax != other.GuardMax)
		{
			return false;
		}
		if (DailyUsed != other.DailyUsed)
		{
			return false;
		}
		if (DailyLimit != other.DailyLimit)
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
		if (Merit != 0)
		{
			num ^= Merit.GetHashCode();
		}
		if (GuardCount != 0)
		{
			num ^= GuardCount.GetHashCode();
		}
		if (GuardMax != 0)
		{
			num ^= GuardMax.GetHashCode();
		}
		if (DailyUsed != 0)
		{
			num ^= DailyUsed.GetHashCode();
		}
		if (DailyLimit != 0)
		{
			num ^= DailyLimit.GetHashCode();
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
		if (Merit != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteInt32(Merit);
		}
		if (GuardCount != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteInt32(GuardCount);
		}
		if (GuardMax != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)24);
			((WriteContext)(ref output)).WriteInt32(GuardMax);
		}
		if (DailyUsed != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)32);
			((WriteContext)(ref output)).WriteInt32(DailyUsed);
		}
		if (DailyLimit != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)40);
			((WriteContext)(ref output)).WriteInt32(DailyLimit);
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
		if (Merit != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(Merit);
		}
		if (GuardCount != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(GuardCount);
		}
		if (GuardMax != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(GuardMax);
		}
		if (DailyUsed != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(DailyUsed);
		}
		if (DailyLimit != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(DailyLimit);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(SeasonRecruitAck other)
	{
		if (other != null)
		{
			if (other.Merit != 0)
			{
				Merit = other.Merit;
			}
			if (other.GuardCount != 0)
			{
				GuardCount = other.GuardCount;
			}
			if (other.GuardMax != 0)
			{
				GuardMax = other.GuardMax;
			}
			if (other.DailyUsed != 0)
			{
				DailyUsed = other.DailyUsed;
			}
			if (other.DailyLimit != 0)
			{
				DailyLimit = other.DailyLimit;
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
				Merit = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 16u:
				GuardCount = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 24u:
				GuardMax = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 32u:
				DailyUsed = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 40u:
				DailyLimit = ((ParseContext)(ref input)).ReadInt32();
				break;
			}
		}
	}
}
