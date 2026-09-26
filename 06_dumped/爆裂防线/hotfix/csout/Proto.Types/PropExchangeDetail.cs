using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class PropExchangeDetail : IMessage<PropExchangeDetail>, IMessage, IEquatable<PropExchangeDetail>, IDeepCloneable<PropExchangeDetail>, IBufferMessage
{
	private static readonly MessageParser<PropExchangeDetail> _parser = new MessageParser<PropExchangeDetail>((Func<PropExchangeDetail>)(() => new PropExchangeDetail()));

	private UnknownFieldSet _unknownFields;

	public const int BaseInfoFieldNumber = 1;

	private OperateBaseInfo baseInfo_;

	public const int PropIdFieldNumber = 2;

	private int propId_;

	public const int CountFieldNumber = 3;

	private int count_;

	public const int TargetListFieldNumber = 4;

	private static readonly FieldCodec<OperateTargetDetail> _repeated_targetList_codec = FieldCodec.ForMessage<OperateTargetDetail>(34u, OperateTargetDetail.Parser);

	private readonly RepeatedField<OperateTargetDetail> targetList_ = new RepeatedField<OperateTargetDetail>();

	public const int ExchangeListFieldNumber = 5;

	private static readonly FieldCodec<OperatePropExchangeDetail> _repeated_exchangeList_codec = FieldCodec.ForMessage<OperatePropExchangeDetail>(42u, OperatePropExchangeDetail.Parser);

	private readonly RepeatedField<OperatePropExchangeDetail> exchangeList_ = new RepeatedField<OperatePropExchangeDetail>();

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<PropExchangeDetail> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypePropExchangeDetailReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public OperateBaseInfo BaseInfo
	{
		get
		{
			return baseInfo_;
		}
		set
		{
			baseInfo_ = value;
		}
	}

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
	public RepeatedField<OperateTargetDetail> TargetList => targetList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<OperatePropExchangeDetail> ExchangeList => exchangeList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public PropExchangeDetail()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public PropExchangeDetail(PropExchangeDetail other)
		: this()
	{
		baseInfo_ = ((other.baseInfo_ != null) ? other.baseInfo_.Clone() : null);
		propId_ = other.propId_;
		count_ = other.count_;
		targetList_ = other.targetList_.Clone();
		exchangeList_ = other.exchangeList_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public PropExchangeDetail Clone()
	{
		return new PropExchangeDetail(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as PropExchangeDetail);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(PropExchangeDetail other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!object.Equals(BaseInfo, other.BaseInfo))
		{
			return false;
		}
		if (PropId != other.PropId)
		{
			return false;
		}
		if (Count != other.Count)
		{
			return false;
		}
		if (!targetList_.Equals(other.targetList_))
		{
			return false;
		}
		if (!exchangeList_.Equals(other.exchangeList_))
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
		if (baseInfo_ != null)
		{
			num ^= BaseInfo.GetHashCode();
		}
		if (PropId != 0)
		{
			num ^= PropId.GetHashCode();
		}
		if (Count != 0)
		{
			num ^= Count.GetHashCode();
		}
		num ^= ((object)targetList_).GetHashCode();
		num ^= ((object)exchangeList_).GetHashCode();
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
		if (baseInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)10);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)BaseInfo);
		}
		if (PropId != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteInt32(PropId);
		}
		if (Count != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)24);
			((WriteContext)(ref output)).WriteInt32(Count);
		}
		targetList_.WriteTo(ref output, _repeated_targetList_codec);
		exchangeList_.WriteTo(ref output, _repeated_exchangeList_codec);
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
		if (baseInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)BaseInfo);
		}
		if (PropId != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(PropId);
		}
		if (Count != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(Count);
		}
		num += targetList_.CalculateSize(_repeated_targetList_codec);
		num += exchangeList_.CalculateSize(_repeated_exchangeList_codec);
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(PropExchangeDetail other)
	{
		if (other == null)
		{
			return;
		}
		if (other.baseInfo_ != null)
		{
			if (baseInfo_ == null)
			{
				BaseInfo = new OperateBaseInfo();
			}
			BaseInfo.MergeFrom(other.BaseInfo);
		}
		if (other.PropId != 0)
		{
			PropId = other.PropId;
		}
		if (other.Count != 0)
		{
			Count = other.Count;
		}
		targetList_.Add((IEnumerable<OperateTargetDetail>)other.targetList_);
		exchangeList_.Add((IEnumerable<OperatePropExchangeDetail>)other.exchangeList_);
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
				if (baseInfo_ == null)
				{
					BaseInfo = new OperateBaseInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)BaseInfo);
				break;
			case 16u:
				PropId = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 24u:
				Count = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 34u:
				targetList_.AddEntriesFrom(ref input, _repeated_targetList_codec);
				break;
			case 42u:
				exchangeList_.AddEntriesFrom(ref input, _repeated_exchangeList_codec);
				break;
			}
		}
	}
}
