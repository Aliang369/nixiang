using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class AdvertisementLimitTimeTreasureAck : IMessage<AdvertisementLimitTimeTreasureAck>, IMessage, IEquatable<AdvertisementLimitTimeTreasureAck>, IDeepCloneable<AdvertisementLimitTimeTreasureAck>, IBufferMessage
{
	private static readonly MessageParser<AdvertisementLimitTimeTreasureAck> _parser = new MessageParser<AdvertisementLimitTimeTreasureAck>((Func<AdvertisementLimitTimeTreasureAck>)(() => new AdvertisementLimitTimeTreasureAck()));

	private UnknownFieldSet _unknownFields;

	public const int AdvertisementInfoFieldNumber = 1;

	private AdvertisementInfo advertisementInfo_;

	public const int LimitTimeTreasureListFieldNumber = 2;

	private static readonly FieldCodec<LimitTimeRouletteDetail> _repeated_limitTimeTreasureList_codec = FieldCodec.ForMessage<LimitTimeRouletteDetail>(18u, LimitTimeRouletteDetail.Parser);

	private readonly RepeatedField<LimitTimeRouletteDetail> limitTimeTreasureList_ = new RepeatedField<LimitTimeRouletteDetail>();

	public const int AwardListFieldNumber = 3;

	private static readonly FieldCodec<AwardDetail> _repeated_awardList_codec = FieldCodec.ForMessage<AwardDetail>(26u, AwardDetail.Parser);

	private readonly RepeatedField<AwardDetail> awardList_ = new RepeatedField<AwardDetail>();

	public const int DataChangeFieldNumber = 4;

	private DataChange dataChange_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<AdvertisementLimitTimeTreasureAck> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeAdvertisementLimitTimeTreasureAckReflection.Descriptor.MessageTypes[0];

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
	public RepeatedField<LimitTimeRouletteDetail> LimitTimeTreasureList => limitTimeTreasureList_;

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
	public AdvertisementLimitTimeTreasureAck()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public AdvertisementLimitTimeTreasureAck(AdvertisementLimitTimeTreasureAck other)
		: this()
	{
		advertisementInfo_ = ((other.advertisementInfo_ != null) ? other.advertisementInfo_.Clone() : null);
		limitTimeTreasureList_ = other.limitTimeTreasureList_.Clone();
		awardList_ = other.awardList_.Clone();
		dataChange_ = ((other.dataChange_ != null) ? other.dataChange_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public AdvertisementLimitTimeTreasureAck Clone()
	{
		return new AdvertisementLimitTimeTreasureAck(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as AdvertisementLimitTimeTreasureAck);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(AdvertisementLimitTimeTreasureAck other)
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
		if (!limitTimeTreasureList_.Equals(other.limitTimeTreasureList_))
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
		num ^= ((object)limitTimeTreasureList_).GetHashCode();
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
		limitTimeTreasureList_.WriteTo(ref output, _repeated_limitTimeTreasureList_codec);
		awardList_.WriteTo(ref output, _repeated_awardList_codec);
		if (dataChange_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)34);
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
		num += limitTimeTreasureList_.CalculateSize(_repeated_limitTimeTreasureList_codec);
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
	public void MergeFrom(AdvertisementLimitTimeTreasureAck other)
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
		limitTimeTreasureList_.Add((IEnumerable<LimitTimeRouletteDetail>)other.limitTimeTreasureList_);
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
				limitTimeTreasureList_.AddEntriesFrom(ref input, _repeated_limitTimeTreasureList_codec);
				break;
			case 26u:
				awardList_.AddEntriesFrom(ref input, _repeated_awardList_codec);
				break;
			case 34u:
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
