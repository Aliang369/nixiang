using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class AdvertisementWallSubjectRouletteAck : IMessage<AdvertisementWallSubjectRouletteAck>, IMessage, IEquatable<AdvertisementWallSubjectRouletteAck>, IDeepCloneable<AdvertisementWallSubjectRouletteAck>, IBufferMessage
{
	private static readonly MessageParser<AdvertisementWallSubjectRouletteAck> _parser = new MessageParser<AdvertisementWallSubjectRouletteAck>((Func<AdvertisementWallSubjectRouletteAck>)(() => new AdvertisementWallSubjectRouletteAck()));

	private UnknownFieldSet _unknownFields;

	public const int AdvertisementInfoFieldNumber = 1;

	private AdvertisementInfo advertisementInfo_;

	public const int WallSubjectListFieldNumber = 2;

	private static readonly FieldCodec<WallSubjectDetail> _repeated_wallSubjectList_codec = FieldCodec.ForMessage<WallSubjectDetail>(18u, WallSubjectDetail.Parser);

	private readonly RepeatedField<WallSubjectDetail> wallSubjectList_ = new RepeatedField<WallSubjectDetail>();

	public const int HitListFieldNumber = 3;

	private static readonly FieldCodec<int> _repeated_hitList_codec = FieldCodec.ForInt32(26u);

	private readonly RepeatedField<int> hitList_ = new RepeatedField<int>();

	public const int AwardListFieldNumber = 4;

	private static readonly FieldCodec<AwardDetail> _repeated_awardList_codec = FieldCodec.ForMessage<AwardDetail>(34u, AwardDetail.Parser);

	private readonly RepeatedField<AwardDetail> awardList_ = new RepeatedField<AwardDetail>();

	public const int DataChangeFieldNumber = 5;

	private DataChange dataChange_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<AdvertisementWallSubjectRouletteAck> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeAdvertisementWallSubjectRouletteAckReflection.Descriptor.MessageTypes[0];

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
	public RepeatedField<WallSubjectDetail> WallSubjectList => wallSubjectList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<int> HitList => hitList_;

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
	public AdvertisementWallSubjectRouletteAck()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public AdvertisementWallSubjectRouletteAck(AdvertisementWallSubjectRouletteAck other)
		: this()
	{
		advertisementInfo_ = ((other.advertisementInfo_ != null) ? other.advertisementInfo_.Clone() : null);
		wallSubjectList_ = other.wallSubjectList_.Clone();
		hitList_ = other.hitList_.Clone();
		awardList_ = other.awardList_.Clone();
		dataChange_ = ((other.dataChange_ != null) ? other.dataChange_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public AdvertisementWallSubjectRouletteAck Clone()
	{
		return new AdvertisementWallSubjectRouletteAck(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as AdvertisementWallSubjectRouletteAck);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(AdvertisementWallSubjectRouletteAck other)
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
		if (!wallSubjectList_.Equals(other.wallSubjectList_))
		{
			return false;
		}
		if (!hitList_.Equals(other.hitList_))
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
		num ^= ((object)wallSubjectList_).GetHashCode();
		num ^= ((object)hitList_).GetHashCode();
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
		wallSubjectList_.WriteTo(ref output, _repeated_wallSubjectList_codec);
		hitList_.WriteTo(ref output, _repeated_hitList_codec);
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
		num += wallSubjectList_.CalculateSize(_repeated_wallSubjectList_codec);
		num += hitList_.CalculateSize(_repeated_hitList_codec);
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
	public void MergeFrom(AdvertisementWallSubjectRouletteAck other)
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
		wallSubjectList_.Add((IEnumerable<WallSubjectDetail>)other.wallSubjectList_);
		hitList_.Add((IEnumerable<int>)other.hitList_);
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
				wallSubjectList_.AddEntriesFrom(ref input, _repeated_wallSubjectList_codec);
				break;
			case 24u:
			case 26u:
				hitList_.AddEntriesFrom(ref input, _repeated_hitList_codec);
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
