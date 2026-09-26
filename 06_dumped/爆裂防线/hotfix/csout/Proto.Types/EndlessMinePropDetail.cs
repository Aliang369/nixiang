using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class EndlessMinePropDetail : IMessage<EndlessMinePropDetail>, IMessage, IEquatable<EndlessMinePropDetail>, IDeepCloneable<EndlessMinePropDetail>, IBufferMessage
{
	private static readonly MessageParser<EndlessMinePropDetail> _parser = new MessageParser<EndlessMinePropDetail>((Func<EndlessMinePropDetail>)(() => new EndlessMinePropDetail()));

	private UnknownFieldSet _unknownFields;

	public const int PropIdFieldNumber = 1;

	private int propId_;

	public const int CountFieldNumber = 2;

	private int count_;

	public const int DailyCountFieldNumber = 3;

	private int dailyCount_;

	public const int BuyCountFieldNumber = 4;

	private int buyCount_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<EndlessMinePropDetail> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeEndlessMinePropDetailReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int PropId
	{
		get
		{
			return propId_;
		}
		set
		{
			propId_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int Count
	{
		get
		{
			return count_;
		}
		set
		{
			count_ = value;
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
	public EndlessMinePropDetail()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public EndlessMinePropDetail(EndlessMinePropDetail other)
		: this()
	{
		propId_ = other.propId_;
		count_ = other.count_;
		dailyCount_ = other.dailyCount_;
		buyCount_ = other.buyCount_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public EndlessMinePropDetail Clone()
	{
		return new EndlessMinePropDetail(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as EndlessMinePropDetail);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(EndlessMinePropDetail other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (PropId != other.PropId)
		{
			return false;
		}
		if (Count != other.Count)
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
		if (PropId != 0)
		{
			num ^= PropId.GetHashCode();
		}
		if (Count != 0)
		{
			num ^= Count.GetHashCode();
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
		if (PropId != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteInt32(PropId);
		}
		if (Count != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteInt32(Count);
		}
		if (DailyCount != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)24);
			((WriteContext)(ref output)).WriteInt32(DailyCount);
		}
		if (BuyCount != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)32);
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
		if (PropId != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(PropId);
		}
		if (Count != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(Count);
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
	public void MergeFrom(EndlessMinePropDetail other)
	{
		if (other != null)
		{
			if (other.PropId != 0)
			{
				PropId = other.PropId;
			}
			if (other.Count != 0)
			{
				Count = other.Count;
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
				PropId = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 16u:
				Count = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 24u:
				DailyCount = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 32u:
				BuyCount = ((ParseContext)(ref input)).ReadInt32();
				break;
			}
		}
	}
}
