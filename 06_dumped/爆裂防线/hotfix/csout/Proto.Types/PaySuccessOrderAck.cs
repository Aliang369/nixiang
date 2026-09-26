using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class PaySuccessOrderAck : IMessage<PaySuccessOrderAck>, IMessage, IEquatable<PaySuccessOrderAck>, IDeepCloneable<PaySuccessOrderAck>, IBufferMessage
{
	private static readonly MessageParser<PaySuccessOrderAck> _parser = new MessageParser<PaySuccessOrderAck>((Func<PaySuccessOrderAck>)(() => new PaySuccessOrderAck()));

	private UnknownFieldSet _unknownFields;

	public const int TotalPriceFieldNumber = 1;

	private float totalPrice_;

	public const int RechargeShopInfoFieldNumber = 2;

	private RechargeShopInfo rechargeShopInfo_;

	public const int RechargeActiveInfoFieldNumber = 3;

	private RechargeActiveInfo rechargeActiveInfo_;

	public const int VipInfoFieldNumber = 4;

	private VipInfo vipInfo_;

	public const int SubscribeInfoFieldNumber = 5;

	private SubscribeInfo subscribeInfo_;

	public const int AwardListFieldNumber = 6;

	private static readonly FieldCodec<AwardDetail> _repeated_awardList_codec = FieldCodec.ForMessage<AwardDetail>(50u, AwardDetail.Parser);

	private readonly RepeatedField<AwardDetail> awardList_ = new RepeatedField<AwardDetail>();

	public const int DataChangeFieldNumber = 7;

	private DataChange dataChange_;

	public const int OrderListFieldNumber = 8;

	private static readonly FieldCodec<OrderDetail> _repeated_orderList_codec = FieldCodec.ForMessage<OrderDetail>(66u, OrderDetail.Parser);

	private readonly RepeatedField<OrderDetail> orderList_ = new RepeatedField<OrderDetail>();

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<PaySuccessOrderAck> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypePaySuccessOrderAckReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public float TotalPrice
	{
		get
		{
			return totalPrice_;
		}
		set
		{
			totalPrice_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RechargeShopInfo RechargeShopInfo
	{
		get
		{
			return rechargeShopInfo_;
		}
		set
		{
			rechargeShopInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RechargeActiveInfo RechargeActiveInfo
	{
		get
		{
			return rechargeActiveInfo_;
		}
		set
		{
			rechargeActiveInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public VipInfo VipInfo
	{
		get
		{
			return vipInfo_;
		}
		set
		{
			vipInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SubscribeInfo SubscribeInfo
	{
		get
		{
			return subscribeInfo_;
		}
		set
		{
			subscribeInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<AwardDetail> AwardList => awardList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public DataChange DataChange
	{
		get
		{
			return dataChange_;
		}
		set
		{
			dataChange_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<OrderDetail> OrderList => orderList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public PaySuccessOrderAck()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public PaySuccessOrderAck(PaySuccessOrderAck other)
		: this()
	{
		totalPrice_ = other.totalPrice_;
		rechargeShopInfo_ = ((other.rechargeShopInfo_ != null) ? other.rechargeShopInfo_.Clone() : null);
		rechargeActiveInfo_ = ((other.rechargeActiveInfo_ != null) ? other.rechargeActiveInfo_.Clone() : null);
		vipInfo_ = ((other.vipInfo_ != null) ? other.vipInfo_.Clone() : null);
		subscribeInfo_ = ((other.subscribeInfo_ != null) ? other.subscribeInfo_.Clone() : null);
		awardList_ = other.awardList_.Clone();
		dataChange_ = ((other.dataChange_ != null) ? other.dataChange_.Clone() : null);
		orderList_ = other.orderList_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public PaySuccessOrderAck Clone()
	{
		return new PaySuccessOrderAck(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as PaySuccessOrderAck);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(PaySuccessOrderAck other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!ProtobufEqualityComparers.BitwiseSingleEqualityComparer.Equals(TotalPrice, other.TotalPrice))
		{
			return false;
		}
		if (!object.Equals(RechargeShopInfo, other.RechargeShopInfo))
		{
			return false;
		}
		if (!object.Equals(RechargeActiveInfo, other.RechargeActiveInfo))
		{
			return false;
		}
		if (!object.Equals(VipInfo, other.VipInfo))
		{
			return false;
		}
		if (!object.Equals(SubscribeInfo, other.SubscribeInfo))
		{
			return false;
		}
		if (!awardList_.Equals(other.awardList_))
		{
			return false;
		}
		if (!object.Equals(DataChange, other.DataChange))
		{
			return false;
		}
		if (!orderList_.Equals(other.orderList_))
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
		if (TotalPrice != 0f)
		{
			num ^= ProtobufEqualityComparers.BitwiseSingleEqualityComparer.GetHashCode(TotalPrice);
		}
		if (rechargeShopInfo_ != null)
		{
			num ^= RechargeShopInfo.GetHashCode();
		}
		if (rechargeActiveInfo_ != null)
		{
			num ^= RechargeActiveInfo.GetHashCode();
		}
		if (vipInfo_ != null)
		{
			num ^= VipInfo.GetHashCode();
		}
		if (subscribeInfo_ != null)
		{
			num ^= SubscribeInfo.GetHashCode();
		}
		num ^= ((object)awardList_).GetHashCode();
		if (dataChange_ != null)
		{
			num ^= DataChange.GetHashCode();
		}
		num ^= ((object)orderList_).GetHashCode();
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
		if (TotalPrice != 0f)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)13);
			((WriteContext)(ref output)).WriteFloat(TotalPrice);
		}
		if (rechargeShopInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)18);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)RechargeShopInfo);
		}
		if (rechargeActiveInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)26);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)RechargeActiveInfo);
		}
		if (vipInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)34);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)VipInfo);
		}
		if (subscribeInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)42);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)SubscribeInfo);
		}
		awardList_.WriteTo(ref output, _repeated_awardList_codec);
		if (dataChange_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)58);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)DataChange);
		}
		orderList_.WriteTo(ref output, _repeated_orderList_codec);
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
		if (TotalPrice != 0f)
		{
			num += 5;
		}
		if (rechargeShopInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)RechargeShopInfo);
		}
		if (rechargeActiveInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)RechargeActiveInfo);
		}
		if (vipInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)VipInfo);
		}
		if (subscribeInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)SubscribeInfo);
		}
		num += awardList_.CalculateSize(_repeated_awardList_codec);
		if (dataChange_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)DataChange);
		}
		num += orderList_.CalculateSize(_repeated_orderList_codec);
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(PaySuccessOrderAck other)
	{
		if (other == null)
		{
			return;
		}
		if (other.TotalPrice != 0f)
		{
			TotalPrice = other.TotalPrice;
		}
		if (other.rechargeShopInfo_ != null)
		{
			if (rechargeShopInfo_ == null)
			{
				RechargeShopInfo = new RechargeShopInfo();
			}
			RechargeShopInfo.MergeFrom(other.RechargeShopInfo);
		}
		if (other.rechargeActiveInfo_ != null)
		{
			if (rechargeActiveInfo_ == null)
			{
				RechargeActiveInfo = new RechargeActiveInfo();
			}
			RechargeActiveInfo.MergeFrom(other.RechargeActiveInfo);
		}
		if (other.vipInfo_ != null)
		{
			if (vipInfo_ == null)
			{
				VipInfo = new VipInfo();
			}
			VipInfo.MergeFrom(other.VipInfo);
		}
		if (other.subscribeInfo_ != null)
		{
			if (subscribeInfo_ == null)
			{
				SubscribeInfo = new SubscribeInfo();
			}
			SubscribeInfo.MergeFrom(other.SubscribeInfo);
		}
		awardList_.Add((IEnumerable<AwardDetail>)other.awardList_);
		if (other.dataChange_ != null)
		{
			if (dataChange_ == null)
			{
				DataChange = new DataChange();
			}
			DataChange.MergeFrom(other.DataChange);
		}
		orderList_.Add((IEnumerable<OrderDetail>)other.orderList_);
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
			case 13u:
				TotalPrice = ((ParseContext)(ref input)).ReadFloat();
				break;
			case 18u:
				if (rechargeShopInfo_ == null)
				{
					RechargeShopInfo = new RechargeShopInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)RechargeShopInfo);
				break;
			case 26u:
				if (rechargeActiveInfo_ == null)
				{
					RechargeActiveInfo = new RechargeActiveInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)RechargeActiveInfo);
				break;
			case 34u:
				if (vipInfo_ == null)
				{
					VipInfo = new VipInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)VipInfo);
				break;
			case 42u:
				if (subscribeInfo_ == null)
				{
					SubscribeInfo = new SubscribeInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)SubscribeInfo);
				break;
			case 50u:
				awardList_.AddEntriesFrom(ref input, _repeated_awardList_codec);
				break;
			case 58u:
				if (dataChange_ == null)
				{
					DataChange = new DataChange();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)DataChange);
				break;
			case 66u:
				orderList_.AddEntriesFrom(ref input, _repeated_orderList_codec);
				break;
			}
		}
	}
}
