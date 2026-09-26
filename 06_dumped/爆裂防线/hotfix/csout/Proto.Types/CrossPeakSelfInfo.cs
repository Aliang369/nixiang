using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class CrossPeakSelfInfo : IMessage<CrossPeakSelfInfo>, IMessage, IEquatable<CrossPeakSelfInfo>, IDeepCloneable<CrossPeakSelfInfo>, IBufferMessage
{
	private static readonly MessageParser<CrossPeakSelfInfo> _parser = new MessageParser<CrossPeakSelfInfo>((Func<CrossPeakSelfInfo>)(() => new CrossPeakSelfInfo()));

	private UnknownFieldSet _unknownFields;

	public const int CombatCountFieldNumber = 1;

	private int combatCount_;

	public const int DailyCountFieldNumber = 2;

	private int dailyCount_;

	public const int BuyCountFieldNumber = 3;

	private int buyCount_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<CrossPeakSelfInfo> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeCrossPeakSelfInfoReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int CombatCount
	{
		get
		{
			return combatCount_;
		}
		set
		{
			combatCount_ = value;
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
	public int BuyCount
	{
		get
		{
			return buyCount_;
		}
		set
		{
			buyCount_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public CrossPeakSelfInfo()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public CrossPeakSelfInfo(CrossPeakSelfInfo other)
		: this()
	{
		combatCount_ = other.combatCount_;
		dailyCount_ = other.dailyCount_;
		buyCount_ = other.buyCount_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public CrossPeakSelfInfo Clone()
	{
		return new CrossPeakSelfInfo(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as CrossPeakSelfInfo);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(CrossPeakSelfInfo other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (CombatCount != other.CombatCount)
		{
			return false;
		}
		if (DailyCount != other.DailyCount)
		{
			return false;
		}
		if (BuyCount != other.BuyCount)
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
		if (CombatCount != 0)
		{
			num ^= CombatCount.GetHashCode();
		}
		if (DailyCount != 0)
		{
			num ^= DailyCount.GetHashCode();
		}
		if (BuyCount != 0)
		{
			num ^= BuyCount.GetHashCode();
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
		if (CombatCount != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteInt32(CombatCount);
		}
		if (DailyCount != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteInt32(DailyCount);
		}
		if (BuyCount != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)24);
			((WriteContext)(ref output)).WriteInt32(BuyCount);
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
		if (CombatCount != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(CombatCount);
		}
		if (DailyCount != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(DailyCount);
		}
		if (BuyCount != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(BuyCount);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(CrossPeakSelfInfo other)
	{
		if (other != null)
		{
			if (other.CombatCount != 0)
			{
				CombatCount = other.CombatCount;
			}
			if (other.DailyCount != 0)
			{
				DailyCount = other.DailyCount;
			}
			if (other.BuyCount != 0)
			{
				BuyCount = other.BuyCount;
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
				CombatCount = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 16u:
				DailyCount = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 24u:
				BuyCount = ((ParseContext)(ref input)).ReadInt32();
				break;
			}
		}
	}
}
