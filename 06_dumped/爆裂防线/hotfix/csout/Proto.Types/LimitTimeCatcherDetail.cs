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
public sealed class LimitTimeCatcherDetail : IMessage<LimitTimeCatcherDetail>, IMessage, IEquatable<LimitTimeCatcherDetail>, IDeepCloneable<LimitTimeCatcherDetail>, IBufferMessage
{
	private static readonly MessageParser<LimitTimeCatcherDetail> _parser = new MessageParser<LimitTimeCatcherDetail>((Func<LimitTimeCatcherDetail>)(() => new LimitTimeCatcherDetail()));

	private UnknownFieldSet _unknownFields;

	public const int BaseInfoFieldNumber = 1;

	private OperateBaseInfo baseInfo_;

	public const int CoinCountFieldNumber = 2;

	private int coinCount_;

	public const int HeroListFieldNumber = 3;

	private static readonly FieldCodec<int> _repeated_heroList_codec = FieldCodec.ForInt32(26u);

	private readonly RepeatedField<int> heroList_ = new RepeatedField<int>();

	public const int TargetListFieldNumber = 4;

	private static readonly FieldCodec<LimitTimeCatcherTargetDetail> _repeated_targetList_codec = FieldCodec.ForMessage<LimitTimeCatcherTargetDetail>(34u, LimitTimeCatcherTargetDetail.Parser);

	private readonly RepeatedField<LimitTimeCatcherTargetDetail> targetList_ = new RepeatedField<LimitTimeCatcherTargetDetail>();

	public const int AwardStateFieldNumber = 5;

	private ECommonState awardState_;

	public const int FragPerCoinFieldNumber = 6;

	private int fragPerCoin_;

	public const int AwardListFieldNumber = 7;

	private static readonly FieldCodec<AwardDetail> _repeated_awardList_codec = FieldCodec.ForMessage<AwardDetail>(58u, AwardDetail.Parser);

	private readonly RepeatedField<AwardDetail> awardList_ = new RepeatedField<AwardDetail>();

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<LimitTimeCatcherDetail> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeLimitTimeCatcherDetailReflection.Descriptor.MessageTypes[0];

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
	public int CoinCount
	{
		get
		{
			return coinCount_;
		}
		set
		{
			coinCount_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<int> HeroList => heroList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<LimitTimeCatcherTargetDetail> TargetList => targetList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ECommonState AwardState
	{
		get
		{
			return awardState_;
		}
		set
		{
			awardState_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int FragPerCoin
	{
		get
		{
			return fragPerCoin_;
		}
		set
		{
			fragPerCoin_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<AwardDetail> AwardList => awardList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public LimitTimeCatcherDetail()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public LimitTimeCatcherDetail(LimitTimeCatcherDetail other)
		: this()
	{
		baseInfo_ = ((other.baseInfo_ != null) ? other.baseInfo_.Clone() : null);
		coinCount_ = other.coinCount_;
		heroList_ = other.heroList_.Clone();
		targetList_ = other.targetList_.Clone();
		awardState_ = other.awardState_;
		fragPerCoin_ = other.fragPerCoin_;
		awardList_ = other.awardList_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public LimitTimeCatcherDetail Clone()
	{
		return new LimitTimeCatcherDetail(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as LimitTimeCatcherDetail);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(LimitTimeCatcherDetail other)
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
		if (CoinCount != other.CoinCount)
		{
			return false;
		}
		if (!heroList_.Equals(other.heroList_))
		{
			return false;
		}
		if (!targetList_.Equals(other.targetList_))
		{
			return false;
		}
		if (AwardState != other.AwardState)
		{
			return false;
		}
		if (FragPerCoin != other.FragPerCoin)
		{
			return false;
		}
		if (!awardList_.Equals(other.awardList_))
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
		if (CoinCount != 0)
		{
			num ^= CoinCount.GetHashCode();
		}
		num ^= ((object)heroList_).GetHashCode();
		num ^= ((object)targetList_).GetHashCode();
		if (AwardState != ECommonState.No)
		{
			num ^= AwardState.GetHashCode();
		}
		if (FragPerCoin != 0)
		{
			num ^= FragPerCoin.GetHashCode();
		}
		num ^= ((object)awardList_).GetHashCode();
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
		if (CoinCount != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteInt32(CoinCount);
		}
		heroList_.WriteTo(ref output, _repeated_heroList_codec);
		targetList_.WriteTo(ref output, _repeated_targetList_codec);
		if (AwardState != ECommonState.No)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)40);
			((WriteContext)(ref output)).WriteEnum((int)AwardState);
		}
		if (FragPerCoin != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)48);
			((WriteContext)(ref output)).WriteInt32(FragPerCoin);
		}
		awardList_.WriteTo(ref output, _repeated_awardList_codec);
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
		if (CoinCount != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(CoinCount);
		}
		num += heroList_.CalculateSize(_repeated_heroList_codec);
		num += targetList_.CalculateSize(_repeated_targetList_codec);
		if (AwardState != ECommonState.No)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)AwardState);
		}
		if (FragPerCoin != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(FragPerCoin);
		}
		num += awardList_.CalculateSize(_repeated_awardList_codec);
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(LimitTimeCatcherDetail other)
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
		if (other.CoinCount != 0)
		{
			CoinCount = other.CoinCount;
		}
		heroList_.Add((IEnumerable<int>)other.heroList_);
		targetList_.Add((IEnumerable<LimitTimeCatcherTargetDetail>)other.targetList_);
		if (other.AwardState != ECommonState.No)
		{
			AwardState = other.AwardState;
		}
		if (other.FragPerCoin != 0)
		{
			FragPerCoin = other.FragPerCoin;
		}
		awardList_.Add((IEnumerable<AwardDetail>)other.awardList_);
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
				CoinCount = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 24u:
			case 26u:
				heroList_.AddEntriesFrom(ref input, _repeated_heroList_codec);
				break;
			case 34u:
				targetList_.AddEntriesFrom(ref input, _repeated_targetList_codec);
				break;
			case 40u:
				AwardState = (ECommonState)((ParseContext)(ref input)).ReadEnum();
				break;
			case 48u:
				FragPerCoin = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 58u:
				awardList_.AddEntriesFrom(ref input, _repeated_awardList_codec);
				break;
			}
		}
	}
}
