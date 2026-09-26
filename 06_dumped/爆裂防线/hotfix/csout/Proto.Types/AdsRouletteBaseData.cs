using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class AdsRouletteBaseData : IMessage<AdsRouletteBaseData>, IMessage, IEquatable<AdsRouletteBaseData>, IDeepCloneable<AdsRouletteBaseData>, IBufferMessage
{
	private static readonly MessageParser<AdsRouletteBaseData> _parser = new MessageParser<AdsRouletteBaseData>((Func<AdsRouletteBaseData>)(() => new AdsRouletteBaseData()));

	private UnknownFieldSet _unknownFields;

	public const int OnceLuckyFieldNumber = 1;

	private int onceLucky_;

	public const int MaxLuckyFieldNumber = 2;

	private int maxLucky_;

	public const int LuckyFieldNumber = 3;

	private int lucky_;

	public const int DailyLimitFieldNumber = 4;

	private int dailyLimit_;

	public const int DailyCountFieldNumber = 5;

	private int dailyCount_;

	public const int TotalCountFieldNumber = 6;

	private int totalCount_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<AdsRouletteBaseData> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeAdsRouletteBaseDataReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int OnceLucky
	{
		get
		{
			return onceLucky_;
		}
		set
		{
			onceLucky_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int MaxLucky
	{
		get
		{
			return maxLucky_;
		}
		set
		{
			maxLucky_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int Lucky
	{
		get
		{
			return lucky_;
		}
		set
		{
			lucky_ = value;
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
	public int TotalCount
	{
		get
		{
			return totalCount_;
		}
		set
		{
			totalCount_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public AdsRouletteBaseData()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public AdsRouletteBaseData(AdsRouletteBaseData other)
		: this()
	{
		onceLucky_ = other.onceLucky_;
		maxLucky_ = other.maxLucky_;
		lucky_ = other.lucky_;
		dailyLimit_ = other.dailyLimit_;
		dailyCount_ = other.dailyCount_;
		totalCount_ = other.totalCount_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public AdsRouletteBaseData Clone()
	{
		return new AdsRouletteBaseData(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as AdsRouletteBaseData);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(AdsRouletteBaseData other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (OnceLucky != other.OnceLucky)
		{
			return false;
		}
		if (MaxLucky != other.MaxLucky)
		{
			return false;
		}
		if (Lucky != other.Lucky)
		{
			return false;
		}
		if (DailyLimit != other.DailyLimit)
		{
			return false;
		}
		if (DailyCount != other.DailyCount)
		{
			return false;
		}
		if (TotalCount != other.TotalCount)
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
		if (OnceLucky != 0)
		{
			num ^= OnceLucky.GetHashCode();
		}
		if (MaxLucky != 0)
		{
			num ^= MaxLucky.GetHashCode();
		}
		if (Lucky != 0)
		{
			num ^= Lucky.GetHashCode();
		}
		if (DailyLimit != 0)
		{
			num ^= DailyLimit.GetHashCode();
		}
		if (DailyCount != 0)
		{
			num ^= DailyCount.GetHashCode();
		}
		if (TotalCount != 0)
		{
			num ^= TotalCount.GetHashCode();
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
		if (OnceLucky != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteInt32(OnceLucky);
		}
		if (MaxLucky != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteInt32(MaxLucky);
		}
		if (Lucky != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)24);
			((WriteContext)(ref output)).WriteInt32(Lucky);
		}
		if (DailyLimit != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)32);
			((WriteContext)(ref output)).WriteInt32(DailyLimit);
		}
		if (DailyCount != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)40);
			((WriteContext)(ref output)).WriteInt32(DailyCount);
		}
		if (TotalCount != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)48);
			((WriteContext)(ref output)).WriteInt32(TotalCount);
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
		if (OnceLucky != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(OnceLucky);
		}
		if (MaxLucky != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(MaxLucky);
		}
		if (Lucky != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(Lucky);
		}
		if (DailyLimit != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(DailyLimit);
		}
		if (DailyCount != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(DailyCount);
		}
		if (TotalCount != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(TotalCount);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(AdsRouletteBaseData other)
	{
		if (other != null)
		{
			if (other.OnceLucky != 0)
			{
				OnceLucky = other.OnceLucky;
			}
			if (other.MaxLucky != 0)
			{
				MaxLucky = other.MaxLucky;
			}
			if (other.Lucky != 0)
			{
				Lucky = other.Lucky;
			}
			if (other.DailyLimit != 0)
			{
				DailyLimit = other.DailyLimit;
			}
			if (other.DailyCount != 0)
			{
				DailyCount = other.DailyCount;
			}
			if (other.TotalCount != 0)
			{
				TotalCount = other.TotalCount;
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
				OnceLucky = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 16u:
				MaxLucky = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 24u:
				Lucky = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 32u:
				DailyLimit = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 40u:
				DailyCount = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 48u:
				TotalCount = ((ParseContext)(ref input)).ReadInt32();
				break;
			}
		}
	}
}
