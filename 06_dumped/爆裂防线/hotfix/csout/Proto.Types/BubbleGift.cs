using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class BubbleGift : IMessage<BubbleGift>, IMessage, IEquatable<BubbleGift>, IDeepCloneable<BubbleGift>, IBufferMessage
{
	private static readonly MessageParser<BubbleGift> _parser = new MessageParser<BubbleGift>((Func<BubbleGift>)(() => new BubbleGift()));

	private UnknownFieldSet _unknownFields;

	public const int IsOpenFieldNumber = 1;

	private EBoolState isOpen_;

	public const int DailyCountFieldNumber = 2;

	private int dailyCount_;

	public const int RandCountFieldNumber = 3;

	private int randCount_;

	public const int WaitStartFieldNumber = 4;

	private long waitStart_;

	public const int WaitEndFieldNumber = 5;

	private long waitEnd_;

	public const int GoodsItemFieldNumber = 6;

	private BubbleGiftGoodsDetail goodsItem_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<BubbleGift> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeBubbleGiftReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public EBoolState IsOpen
	{
		get
		{
			return isOpen_;
		}
		set
		{
			isOpen_ = value;
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
	public int RandCount
	{
		get
		{
			return randCount_;
		}
		set
		{
			randCount_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public long WaitStart
	{
		get
		{
			return waitStart_;
		}
		set
		{
			waitStart_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public long WaitEnd
	{
		get
		{
			return waitEnd_;
		}
		set
		{
			waitEnd_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public BubbleGiftGoodsDetail GoodsItem
	{
		get
		{
			return goodsItem_;
		}
		set
		{
			goodsItem_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public BubbleGift()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public BubbleGift(BubbleGift other)
		: this()
	{
		isOpen_ = other.isOpen_;
		dailyCount_ = other.dailyCount_;
		randCount_ = other.randCount_;
		waitStart_ = other.waitStart_;
		waitEnd_ = other.waitEnd_;
		goodsItem_ = ((other.goodsItem_ != null) ? other.goodsItem_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public BubbleGift Clone()
	{
		return new BubbleGift(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as BubbleGift);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(BubbleGift other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (IsOpen != other.IsOpen)
		{
			return false;
		}
		if (DailyCount != other.DailyCount)
		{
			return false;
		}
		if (RandCount != other.RandCount)
		{
			return false;
		}
		if (WaitStart != other.WaitStart)
		{
			return false;
		}
		if (WaitEnd != other.WaitEnd)
		{
			return false;
		}
		if (!object.Equals(GoodsItem, other.GoodsItem))
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
		if (IsOpen != EBoolState.False)
		{
			num ^= IsOpen.GetHashCode();
		}
		if (DailyCount != 0)
		{
			num ^= DailyCount.GetHashCode();
		}
		if (RandCount != 0)
		{
			num ^= RandCount.GetHashCode();
		}
		if (WaitStart != 0L)
		{
			num ^= WaitStart.GetHashCode();
		}
		if (WaitEnd != 0L)
		{
			num ^= WaitEnd.GetHashCode();
		}
		if (goodsItem_ != null)
		{
			num ^= GoodsItem.GetHashCode();
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
		if (IsOpen != EBoolState.False)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteEnum((int)IsOpen);
		}
		if (DailyCount != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteInt32(DailyCount);
		}
		if (RandCount != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)24);
			((WriteContext)(ref output)).WriteInt32(RandCount);
		}
		if (WaitStart != 0L)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)32);
			((WriteContext)(ref output)).WriteInt64(WaitStart);
		}
		if (WaitEnd != 0L)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)40);
			((WriteContext)(ref output)).WriteInt64(WaitEnd);
		}
		if (goodsItem_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)50);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)GoodsItem);
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
		if (IsOpen != EBoolState.False)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)IsOpen);
		}
		if (DailyCount != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(DailyCount);
		}
		if (RandCount != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(RandCount);
		}
		if (WaitStart != 0L)
		{
			num += 1 + CodedOutputStream.ComputeInt64Size(WaitStart);
		}
		if (WaitEnd != 0L)
		{
			num += 1 + CodedOutputStream.ComputeInt64Size(WaitEnd);
		}
		if (goodsItem_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)GoodsItem);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(BubbleGift other)
	{
		if (other == null)
		{
			return;
		}
		if (other.IsOpen != EBoolState.False)
		{
			IsOpen = other.IsOpen;
		}
		if (other.DailyCount != 0)
		{
			DailyCount = other.DailyCount;
		}
		if (other.RandCount != 0)
		{
			RandCount = other.RandCount;
		}
		if (other.WaitStart != 0L)
		{
			WaitStart = other.WaitStart;
		}
		if (other.WaitEnd != 0L)
		{
			WaitEnd = other.WaitEnd;
		}
		if (other.goodsItem_ != null)
		{
			if (goodsItem_ == null)
			{
				GoodsItem = new BubbleGiftGoodsDetail();
			}
			GoodsItem.MergeFrom(other.GoodsItem);
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
				IsOpen = (EBoolState)((ParseContext)(ref input)).ReadEnum();
				break;
			case 16u:
				DailyCount = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 24u:
				RandCount = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 32u:
				WaitStart = ((ParseContext)(ref input)).ReadInt64();
				break;
			case 40u:
				WaitEnd = ((ParseContext)(ref input)).ReadInt64();
				break;
			case 50u:
				if (goodsItem_ == null)
				{
					GoodsItem = new BubbleGiftGoodsDetail();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)GoodsItem);
				break;
			}
		}
	}
}
