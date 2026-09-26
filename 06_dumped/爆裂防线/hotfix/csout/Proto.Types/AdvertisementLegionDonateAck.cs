using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class AdvertisementLegionDonateAck : IMessage<AdvertisementLegionDonateAck>, IMessage, IEquatable<AdvertisementLegionDonateAck>, IDeepCloneable<AdvertisementLegionDonateAck>, IBufferMessage
{
	private static readonly MessageParser<AdvertisementLegionDonateAck> _parser = new MessageParser<AdvertisementLegionDonateAck>((Func<AdvertisementLegionDonateAck>)(() => new AdvertisementLegionDonateAck()));

	private UnknownFieldSet _unknownFields;

	public const int AdvertisementInfoFieldNumber = 1;

	private AdvertisementInfo advertisementInfo_;

	public const int LegionSelfInfoFieldNumber = 2;

	private LegionSelfInfo legionSelfInfo_;

	public const int LegionInfoFieldNumber = 3;

	private LegionInfo legionInfo_;

	public const int AwardListFieldNumber = 4;

	private static readonly FieldCodec<AwardDetail> _repeated_awardList_codec = FieldCodec.ForMessage<AwardDetail>(34u, AwardDetail.Parser);

	private readonly RepeatedField<AwardDetail> awardList_ = new RepeatedField<AwardDetail>();

	public const int DataChangeFieldNumber = 5;

	private DataChange dataChange_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<AdvertisementLegionDonateAck> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeAdvertisementLegionDonateAckReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public AdvertisementInfo AdvertisementInfo
	{
		get
		{
			return advertisementInfo_;
		}
		set
		{
			advertisementInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public LegionSelfInfo LegionSelfInfo
	{
		get
		{
			return legionSelfInfo_;
		}
		set
		{
			legionSelfInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public LegionInfo LegionInfo
	{
		get
		{
			return legionInfo_;
		}
		set
		{
			legionInfo_ = value;
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
	public AdvertisementLegionDonateAck()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public AdvertisementLegionDonateAck(AdvertisementLegionDonateAck other)
		: this()
	{
		advertisementInfo_ = ((other.advertisementInfo_ != null) ? other.advertisementInfo_.Clone() : null);
		legionSelfInfo_ = ((other.legionSelfInfo_ != null) ? other.legionSelfInfo_.Clone() : null);
		legionInfo_ = ((other.legionInfo_ != null) ? other.legionInfo_.Clone() : null);
		awardList_ = other.awardList_.Clone();
		dataChange_ = ((other.dataChange_ != null) ? other.dataChange_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public AdvertisementLegionDonateAck Clone()
	{
		return new AdvertisementLegionDonateAck(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as AdvertisementLegionDonateAck);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(AdvertisementLegionDonateAck other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!object.Equals(AdvertisementInfo, other.AdvertisementInfo))
		{
			return false;
		}
		if (!object.Equals(LegionSelfInfo, other.LegionSelfInfo))
		{
			return false;
		}
		if (!object.Equals(LegionInfo, other.LegionInfo))
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
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override int GetHashCode()
	{
		int num = 1;
		if (advertisementInfo_ != null)
		{
			num ^= AdvertisementInfo.GetHashCode();
		}
		if (legionSelfInfo_ != null)
		{
			num ^= LegionSelfInfo.GetHashCode();
		}
		if (legionInfo_ != null)
		{
			num ^= LegionInfo.GetHashCode();
		}
		num ^= ((object)awardList_).GetHashCode();
		if (dataChange_ != null)
		{
			num ^= DataChange.GetHashCode();
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
		if (advertisementInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)10);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)AdvertisementInfo);
		}
		if (legionSelfInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)18);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)LegionSelfInfo);
		}
		if (legionInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)26);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)LegionInfo);
		}
		awardList_.WriteTo(ref output, _repeated_awardList_codec);
		if (dataChange_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)42);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)DataChange);
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
		if (advertisementInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)AdvertisementInfo);
		}
		if (legionSelfInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)LegionSelfInfo);
		}
		if (legionInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)LegionInfo);
		}
		num += awardList_.CalculateSize(_repeated_awardList_codec);
		if (dataChange_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)DataChange);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(AdvertisementLegionDonateAck other)
	{
		if (other == null)
		{
			return;
		}
		if (other.advertisementInfo_ != null)
		{
			if (advertisementInfo_ == null)
			{
				AdvertisementInfo = new AdvertisementInfo();
			}
			AdvertisementInfo.MergeFrom(other.AdvertisementInfo);
		}
		if (other.legionSelfInfo_ != null)
		{
			if (legionSelfInfo_ == null)
			{
				LegionSelfInfo = new LegionSelfInfo();
			}
			LegionSelfInfo.MergeFrom(other.LegionSelfInfo);
		}
		if (other.legionInfo_ != null)
		{
			if (legionInfo_ == null)
			{
				LegionInfo = new LegionInfo();
			}
			LegionInfo.MergeFrom(other.LegionInfo);
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
				if (advertisementInfo_ == null)
				{
					AdvertisementInfo = new AdvertisementInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)AdvertisementInfo);
				break;
			case 18u:
				if (legionSelfInfo_ == null)
				{
					LegionSelfInfo = new LegionSelfInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)LegionSelfInfo);
				break;
			case 26u:
				if (legionInfo_ == null)
				{
					LegionInfo = new LegionInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)LegionInfo);
				break;
			case 34u:
				awardList_.AddEntriesFrom(ref input, _repeated_awardList_codec);
				break;
			case 42u:
				if (dataChange_ == null)
				{
					DataChange = new DataChange();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)DataChange);
				break;
			}
		}
	}
}
