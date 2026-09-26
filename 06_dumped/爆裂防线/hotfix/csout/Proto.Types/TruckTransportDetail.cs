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
public sealed class TruckTransportDetail : IMessage<TruckTransportDetail>, IMessage, IEquatable<TruckTransportDetail>, IDeepCloneable<TruckTransportDetail>, IBufferMessage
{
	private static readonly MessageParser<TruckTransportDetail> _parser = new MessageParser<TruckTransportDetail>((Func<TruckTransportDetail>)(() => new TruckTransportDetail()));

	private UnknownFieldSet _unknownFields;

	public const int TransportIdFieldNumber = 1;

	private int transportId_;

	public const int QualityFieldNumber = 2;

	private EQuality quality_;

	public const int ArriveTmFieldNumber = 3;

	private long arriveTm_;

	public const int AwardListFieldNumber = 4;

	private static readonly FieldCodec<AwardDetail> _repeated_awardList_codec = FieldCodec.ForMessage<AwardDetail>(34u, AwardDetail.Parser);

	private readonly RepeatedField<AwardDetail> awardList_ = new RepeatedField<AwardDetail>();

	public const int RobListFieldNumber = 5;

	private static readonly FieldCodec<int> _repeated_robList_codec = FieldCodec.ForInt32(42u);

	private readonly RepeatedField<int> robList_ = new RepeatedField<int>();

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<TruckTransportDetail> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeTruckTransportDetailReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int TransportId
	{
		get
		{
			return transportId_;
		}
		set
		{
			transportId_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public EQuality Quality
	{
		get
		{
			return quality_;
		}
		set
		{
			quality_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public long ArriveTm
	{
		get
		{
			return arriveTm_;
		}
		set
		{
			arriveTm_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<AwardDetail> AwardList => awardList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<int> RobList => robList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public TruckTransportDetail()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public TruckTransportDetail(TruckTransportDetail other)
		: this()
	{
		transportId_ = other.transportId_;
		quality_ = other.quality_;
		arriveTm_ = other.arriveTm_;
		awardList_ = other.awardList_.Clone();
		robList_ = other.robList_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public TruckTransportDetail Clone()
	{
		return new TruckTransportDetail(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as TruckTransportDetail);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(TruckTransportDetail other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (TransportId != other.TransportId)
		{
			return false;
		}
		if (Quality != other.Quality)
		{
			return false;
		}
		if (ArriveTm != other.ArriveTm)
		{
			return false;
		}
		if (!awardList_.Equals(other.awardList_))
		{
			return false;
		}
		if (!robList_.Equals(other.robList_))
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
		if (TransportId != 0)
		{
			num ^= TransportId.GetHashCode();
		}
		if (Quality != EQuality.EmptyValue)
		{
			num ^= Quality.GetHashCode();
		}
		if (ArriveTm != 0L)
		{
			num ^= ArriveTm.GetHashCode();
		}
		num ^= ((object)awardList_).GetHashCode();
		num ^= ((object)robList_).GetHashCode();
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
		if (TransportId != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteInt32(TransportId);
		}
		if (Quality != EQuality.EmptyValue)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteEnum((int)Quality);
		}
		if (ArriveTm != 0L)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)24);
			((WriteContext)(ref output)).WriteInt64(ArriveTm);
		}
		awardList_.WriteTo(ref output, _repeated_awardList_codec);
		robList_.WriteTo(ref output, _repeated_robList_codec);
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
		if (TransportId != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(TransportId);
		}
		if (Quality != EQuality.EmptyValue)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)Quality);
		}
		if (ArriveTm != 0L)
		{
			num += 1 + CodedOutputStream.ComputeInt64Size(ArriveTm);
		}
		num += awardList_.CalculateSize(_repeated_awardList_codec);
		num += robList_.CalculateSize(_repeated_robList_codec);
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(TruckTransportDetail other)
	{
		if (other != null)
		{
			if (other.TransportId != 0)
			{
				TransportId = other.TransportId;
			}
			if (other.Quality != EQuality.EmptyValue)
			{
				Quality = other.Quality;
			}
			if (other.ArriveTm != 0L)
			{
				ArriveTm = other.ArriveTm;
			}
			awardList_.Add((IEnumerable<AwardDetail>)other.awardList_);
			robList_.Add((IEnumerable<int>)other.robList_);
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
				TransportId = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 16u:
				Quality = (EQuality)((ParseContext)(ref input)).ReadEnum();
				break;
			case 24u:
				ArriveTm = ((ParseContext)(ref input)).ReadInt64();
				break;
			case 34u:
				awardList_.AddEntriesFrom(ref input, _repeated_awardList_codec);
				break;
			case 40u:
			case 42u:
				robList_.AddEntriesFrom(ref input, _repeated_robList_codec);
				break;
			}
		}
	}
}
