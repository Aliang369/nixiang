using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class NovicePackInfo : IMessage<NovicePackInfo>, IMessage, IEquatable<NovicePackInfo>, IDeepCloneable<NovicePackInfo>, IBufferMessage
{
	private static readonly MessageParser<NovicePackInfo> _parser = new MessageParser<NovicePackInfo>((Func<NovicePackInfo>)(() => new NovicePackInfo()));

	private UnknownFieldSet _unknownFields;

	public const int GoodsBaseInfoFieldNumber = 1;

	private RechargeGoodsBaseInfo goodsBaseInfo_;

	public const int StateFieldNumber = 2;

	private EBoolState state_;

	public const int TodayStateFieldNumber = 3;

	private EActiveState todayState_;

	public const int CountFieldNumber = 4;

	private int count_;

	public const int TotalFieldNumber = 5;

	private int total_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<NovicePackInfo> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeNovicePackInfoReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RechargeGoodsBaseInfo GoodsBaseInfo
	{
		get
		{
			return goodsBaseInfo_;
		}
		set
		{
			goodsBaseInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public EBoolState State
	{
		get
		{
			return state_;
		}
		set
		{
			state_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public EActiveState TodayState
	{
		get
		{
			return todayState_;
		}
		set
		{
			todayState_ = value;
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
	public int Total
	{
		get
		{
			return total_;
		}
		set
		{
			total_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public NovicePackInfo()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public NovicePackInfo(NovicePackInfo other)
		: this()
	{
		goodsBaseInfo_ = ((other.goodsBaseInfo_ != null) ? other.goodsBaseInfo_.Clone() : null);
		state_ = other.state_;
		todayState_ = other.todayState_;
		count_ = other.count_;
		total_ = other.total_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public NovicePackInfo Clone()
	{
		return new NovicePackInfo(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as NovicePackInfo);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(NovicePackInfo other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!object.Equals(GoodsBaseInfo, other.GoodsBaseInfo))
		{
			return false;
		}
		if (State != other.State)
		{
			return false;
		}
		if (TodayState != other.TodayState)
		{
			return false;
		}
		if (Count != other.Count)
		{
			return false;
		}
		if (Total != other.Total)
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
		if (goodsBaseInfo_ != null)
		{
			num ^= GoodsBaseInfo.GetHashCode();
		}
		if (State != EBoolState.False)
		{
			num ^= State.GetHashCode();
		}
		if (TodayState != EActiveState.Pending)
		{
			num ^= TodayState.GetHashCode();
		}
		if (Count != 0)
		{
			num ^= Count.GetHashCode();
		}
		if (Total != 0)
		{
			num ^= Total.GetHashCode();
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
		if (goodsBaseInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)10);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)GoodsBaseInfo);
		}
		if (State != EBoolState.False)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteEnum((int)State);
		}
		if (TodayState != EActiveState.Pending)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)24);
			((WriteContext)(ref output)).WriteEnum((int)TodayState);
		}
		if (Count != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)32);
			((WriteContext)(ref output)).WriteInt32(Count);
		}
		if (Total != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)40);
			((WriteContext)(ref output)).WriteInt32(Total);
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
		if (goodsBaseInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)GoodsBaseInfo);
		}
		if (State != EBoolState.False)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)State);
		}
		if (TodayState != EActiveState.Pending)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)TodayState);
		}
		if (Count != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(Count);
		}
		if (Total != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(Total);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(NovicePackInfo other)
	{
		if (other == null)
		{
			return;
		}
		if (other.goodsBaseInfo_ != null)
		{
			if (goodsBaseInfo_ == null)
			{
				GoodsBaseInfo = new RechargeGoodsBaseInfo();
			}
			GoodsBaseInfo.MergeFrom(other.GoodsBaseInfo);
		}
		if (other.State != EBoolState.False)
		{
			State = other.State;
		}
		if (other.TodayState != EActiveState.Pending)
		{
			TodayState = other.TodayState;
		}
		if (other.Count != 0)
		{
			Count = other.Count;
		}
		if (other.Total != 0)
		{
			Total = other.Total;
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
			case 10u:
				if (goodsBaseInfo_ == null)
				{
					GoodsBaseInfo = new RechargeGoodsBaseInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)GoodsBaseInfo);
				break;
			case 16u:
				State = (EBoolState)((ParseContext)(ref input)).ReadEnum();
				break;
			case 24u:
				TodayState = (EActiveState)((ParseContext)(ref input)).ReadEnum();
				break;
			case 32u:
				Count = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 40u:
				Total = ((ParseContext)(ref input)).ReadInt32();
				break;
			}
		}
	}
}
