using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class SnowVeilPropDetail : IMessage<SnowVeilPropDetail>, IMessage, IEquatable<SnowVeilPropDetail>, IDeepCloneable<SnowVeilPropDetail>, IBufferMessage
{
	private static readonly MessageParser<SnowVeilPropDetail> _parser = new MessageParser<SnowVeilPropDetail>((Func<SnowVeilPropDetail>)(() => new SnowVeilPropDetail()));

	private UnknownFieldSet _unknownFields;

	public const int PropIdFieldNumber = 1;

	private int propId_;

	public const int DailyLimitFieldNumber = 2;

	private int dailyLimit_;

	public const int BuyCountFieldNumber = 3;

	private int buyCount_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<SnowVeilPropDetail> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeSnowVeilPropDetailReflection.Descriptor.MessageTypes[0];

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
	public SnowVeilPropDetail()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SnowVeilPropDetail(SnowVeilPropDetail other)
		: this()
	{
		propId_ = other.propId_;
		dailyLimit_ = other.dailyLimit_;
		buyCount_ = other.buyCount_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SnowVeilPropDetail Clone()
	{
		return new SnowVeilPropDetail(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as SnowVeilPropDetail);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(SnowVeilPropDetail other)
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
		if (DailyLimit != other.DailyLimit)
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
		if (DailyLimit != 0)
		{
			num ^= DailyLimit.GetHashCode();
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
		if (DailyLimit != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteInt32(DailyLimit);
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
		if (PropId != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(PropId);
		}
		if (DailyLimit != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(DailyLimit);
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
	public void MergeFrom(SnowVeilPropDetail other)
	{
		if (other != null)
		{
			if (other.PropId != 0)
			{
				PropId = other.PropId;
			}
			if (other.DailyLimit != 0)
			{
				DailyLimit = other.DailyLimit;
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
				DailyLimit = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 24u:
				BuyCount = ((ParseContext)(ref input)).ReadInt32();
				break;
			}
		}
	}
}
