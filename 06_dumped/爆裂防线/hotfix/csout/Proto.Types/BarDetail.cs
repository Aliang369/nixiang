using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class BarDetail : IMessage<BarDetail>, IMessage, IEquatable<BarDetail>, IDeepCloneable<BarDetail>, IBufferMessage
{
	private static readonly MessageParser<BarDetail> _parser = new MessageParser<BarDetail>((Func<BarDetail>)(() => new BarDetail()));

	private UnknownFieldSet _unknownFields;

	public const int BarTypeFieldNumber = 1;

	private EBarType barType_;

	public const int RareHitFieldNumber = 2;

	private int rareHit_;

	public const int TotalCountFieldNumber = 3;

	private int totalCount_;

	public const int DailyFreeFieldNumber = 4;

	private EBoolState dailyFree_;

	public const int TargetAwardFieldNumber = 5;

	private BarTargetAward targetAward_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<BarDetail> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeBarDetailReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public EBarType BarType
	{
		get
		{
			return barType_;
		}
		set
		{
			barType_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int RareHit
	{
		get
		{
			return rareHit_;
		}
		set
		{
			rareHit_ = value;
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
	public EBoolState DailyFree
	{
		get
		{
			return dailyFree_;
		}
		set
		{
			dailyFree_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public BarTargetAward TargetAward
	{
		get
		{
			return targetAward_;
		}
		set
		{
			targetAward_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public BarDetail()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public BarDetail(BarDetail other)
		: this()
	{
		barType_ = other.barType_;
		rareHit_ = other.rareHit_;
		totalCount_ = other.totalCount_;
		dailyFree_ = other.dailyFree_;
		targetAward_ = ((other.targetAward_ != null) ? other.targetAward_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public BarDetail Clone()
	{
		return new BarDetail(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as BarDetail);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(BarDetail other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (BarType != other.BarType)
		{
			return false;
		}
		if (RareHit != other.RareHit)
		{
			return false;
		}
		if (TotalCount != other.TotalCount)
		{
			return false;
		}
		if (DailyFree != other.DailyFree)
		{
			return false;
		}
		if (!object.Equals(TargetAward, other.TargetAward))
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
		if (BarType != EBarType.EmptyValue)
		{
			num ^= BarType.GetHashCode();
		}
		if (RareHit != 0)
		{
			num ^= RareHit.GetHashCode();
		}
		if (TotalCount != 0)
		{
			num ^= TotalCount.GetHashCode();
		}
		if (DailyFree != EBoolState.False)
		{
			num ^= DailyFree.GetHashCode();
		}
		if (targetAward_ != null)
		{
			num ^= TargetAward.GetHashCode();
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
		if (BarType != EBarType.EmptyValue)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteEnum((int)BarType);
		}
		if (RareHit != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteInt32(RareHit);
		}
		if (TotalCount != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)24);
			((WriteContext)(ref output)).WriteInt32(TotalCount);
		}
		if (DailyFree != EBoolState.False)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)32);
			((WriteContext)(ref output)).WriteEnum((int)DailyFree);
		}
		if (targetAward_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)42);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)TargetAward);
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
		if (BarType != EBarType.EmptyValue)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)BarType);
		}
		if (RareHit != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(RareHit);
		}
		if (TotalCount != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(TotalCount);
		}
		if (DailyFree != EBoolState.False)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)DailyFree);
		}
		if (targetAward_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)TargetAward);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(BarDetail other)
	{
		if (other == null)
		{
			return;
		}
		if (other.BarType != EBarType.EmptyValue)
		{
			BarType = other.BarType;
		}
		if (other.RareHit != 0)
		{
			RareHit = other.RareHit;
		}
		if (other.TotalCount != 0)
		{
			TotalCount = other.TotalCount;
		}
		if (other.DailyFree != EBoolState.False)
		{
			DailyFree = other.DailyFree;
		}
		if (other.targetAward_ != null)
		{
			if (targetAward_ == null)
			{
				TargetAward = new BarTargetAward();
			}
			TargetAward.MergeFrom(other.TargetAward);
		}
		_unknownFields = UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
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
				BarType = (EBarType)((ParseContext)(ref input)).ReadEnum();
				break;
			case 16u:
				RareHit = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 24u:
				TotalCount = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 32u:
				DailyFree = (EBoolState)((ParseContext)(ref input)).ReadEnum();
				break;
			case 42u:
				if (targetAward_ == null)
				{
					TargetAward = new BarTargetAward();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)TargetAward);
				break;
			}
		}
	}
}
