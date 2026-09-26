using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class EndlessMineBaseInfo : IMessage<EndlessMineBaseInfo>, IMessage, IEquatable<EndlessMineBaseInfo>, IDeepCloneable<EndlessMineBaseInfo>, IBufferMessage
{
	private static readonly MessageParser<EndlessMineBaseInfo> _parser = new MessageParser<EndlessMineBaseInfo>((Func<EndlessMineBaseInfo>)(() => new EndlessMineBaseInfo()));

	private UnknownFieldSet _unknownFields;

	public const int ActivateAwardFieldNumber = 1;

	private EBoolState activateAward_;

	public const int StartTmFieldNumber = 2;

	private long startTm_;

	public const int EndTmFieldNumber = 3;

	private long endTm_;

	public const int DepthFieldNumber = 4;

	private int depth_;

	public const int PropListFieldNumber = 5;

	private static readonly FieldCodec<EndlessMinePropDetail> _repeated_propList_codec = FieldCodec.ForMessage<EndlessMinePropDetail>(42u, EndlessMinePropDetail.Parser);

	private readonly RepeatedField<EndlessMinePropDetail> propList_ = new RepeatedField<EndlessMinePropDetail>();

	public const int ExchangeDetailFieldNumber = 6;

	private EndlessMineExchangeDetail exchangeDetail_;

	public const int ExchangeNextGrandFieldNumber = 7;

	private OperateTargetNextGrandDetail exchangeNextGrand_;

	public const int TargetListFieldNumber = 8;

	private static readonly FieldCodec<OperateTargetDetail> _repeated_targetList_codec = FieldCodec.ForMessage<OperateTargetDetail>(66u, OperateTargetDetail.Parser);

	private readonly RepeatedField<OperateTargetDetail> targetList_ = new RepeatedField<OperateTargetDetail>();

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<EndlessMineBaseInfo> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeEndlessMineBaseInfoReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public EBoolState ActivateAward
	{
		get
		{
			return activateAward_;
		}
		set
		{
			activateAward_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public long StartTm
	{
		get
		{
			return startTm_;
		}
		set
		{
			startTm_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public long EndTm
	{
		get
		{
			return endTm_;
		}
		set
		{
			endTm_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int Depth
	{
		get
		{
			return depth_;
		}
		set
		{
			depth_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<EndlessMinePropDetail> PropList => propList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public EndlessMineExchangeDetail ExchangeDetail
	{
		get
		{
			return exchangeDetail_;
		}
		set
		{
			exchangeDetail_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public OperateTargetNextGrandDetail ExchangeNextGrand
	{
		get
		{
			return exchangeNextGrand_;
		}
		set
		{
			exchangeNextGrand_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<OperateTargetDetail> TargetList => targetList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public EndlessMineBaseInfo()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public EndlessMineBaseInfo(EndlessMineBaseInfo other)
		: this()
	{
		activateAward_ = other.activateAward_;
		startTm_ = other.startTm_;
		endTm_ = other.endTm_;
		depth_ = other.depth_;
		propList_ = other.propList_.Clone();
		exchangeDetail_ = ((other.exchangeDetail_ != null) ? other.exchangeDetail_.Clone() : null);
		exchangeNextGrand_ = ((other.exchangeNextGrand_ != null) ? other.exchangeNextGrand_.Clone() : null);
		targetList_ = other.targetList_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public EndlessMineBaseInfo Clone()
	{
		return new EndlessMineBaseInfo(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as EndlessMineBaseInfo);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(EndlessMineBaseInfo other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (ActivateAward != other.ActivateAward)
		{
			return false;
		}
		if (StartTm != other.StartTm)
		{
			return false;
		}
		if (EndTm != other.EndTm)
		{
			return false;
		}
		if (Depth != other.Depth)
		{
			return false;
		}
		if (!propList_.Equals(other.propList_))
		{
			return false;
		}
		if (!object.Equals(ExchangeDetail, other.ExchangeDetail))
		{
			return false;
		}
		if (!object.Equals(ExchangeNextGrand, other.ExchangeNextGrand))
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
		if (ActivateAward != EBoolState.False)
		{
			num ^= ActivateAward.GetHashCode();
		}
		if (StartTm != 0L)
		{
			num ^= StartTm.GetHashCode();
		}
		if (EndTm != 0L)
		{
			num ^= EndTm.GetHashCode();
		}
		if (Depth != 0)
		{
			num ^= Depth.GetHashCode();
		}
		num ^= ((object)propList_).GetHashCode();
		if (exchangeDetail_ != null)
		{
			num ^= ExchangeDetail.GetHashCode();
		}
		if (exchangeNextGrand_ != null)
		{
			num ^= ExchangeNextGrand.GetHashCode();
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
		if (ActivateAward != EBoolState.False)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteEnum((int)ActivateAward);
		}
		if (StartTm != 0L)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteInt64(StartTm);
		}
		if (EndTm != 0L)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)24);
			((WriteContext)(ref output)).WriteInt64(EndTm);
		}
		if (Depth != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)32);
			((WriteContext)(ref output)).WriteInt32(Depth);
		}
		propList_.WriteTo(ref output, _repeated_propList_codec);
		if (exchangeDetail_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)50);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)ExchangeDetail);
		}
		if (exchangeNextGrand_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)58);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)ExchangeNextGrand);
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
		if (ActivateAward != EBoolState.False)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)ActivateAward);
		}
		if (StartTm != 0L)
		{
			num += 1 + CodedOutputStream.ComputeInt64Size(StartTm);
		}
		if (EndTm != 0L)
		{
			num += 1 + CodedOutputStream.ComputeInt64Size(EndTm);
		}
		if (Depth != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(Depth);
		}
		num += propList_.CalculateSize(_repeated_propList_codec);
		if (exchangeDetail_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)ExchangeDetail);
		}
		if (exchangeNextGrand_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)ExchangeNextGrand);
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
	public void MergeFrom(EndlessMineBaseInfo other)
	{
		if (other == null)
		{
			return;
		}
		if (other.ActivateAward != EBoolState.False)
		{
			ActivateAward = other.ActivateAward;
		}
		if (other.StartTm != 0L)
		{
			StartTm = other.StartTm;
		}
		if (other.EndTm != 0L)
		{
			EndTm = other.EndTm;
		}
		if (other.Depth != 0)
		{
			Depth = other.Depth;
		}
		propList_.Add((IEnumerable<EndlessMinePropDetail>)other.propList_);
		if (other.exchangeDetail_ != null)
		{
			if (exchangeDetail_ == null)
			{
				ExchangeDetail = new EndlessMineExchangeDetail();
			}
			ExchangeDetail.MergeFrom(other.ExchangeDetail);
		}
		if (other.exchangeNextGrand_ != null)
		{
			if (exchangeNextGrand_ == null)
			{
				ExchangeNextGrand = new OperateTargetNextGrandDetail();
			}
			ExchangeNextGrand.MergeFrom(other.ExchangeNextGrand);
		}
		targetList_.Add((IEnumerable<OperateTargetDetail>)other.targetList_);
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
				ActivateAward = (EBoolState)((ParseContext)(ref input)).ReadEnum();
				break;
			case 16u:
				StartTm = ((ParseContext)(ref input)).ReadInt64();
				break;
			case 24u:
				EndTm = ((ParseContext)(ref input)).ReadInt64();
				break;
			case 32u:
				Depth = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 42u:
				propList_.AddEntriesFrom(ref input, _repeated_propList_codec);
				break;
			case 50u:
				if (exchangeDetail_ == null)
				{
					ExchangeDetail = new EndlessMineExchangeDetail();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)ExchangeDetail);
				break;
			case 58u:
				if (exchangeNextGrand_ == null)
				{
					ExchangeNextGrand = new OperateTargetNextGrandDetail();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)ExchangeNextGrand);
				break;
			case 66u:
				targetList_.AddEntriesFrom(ref input, _repeated_targetList_codec);
				break;
			}
		}
	}
}
