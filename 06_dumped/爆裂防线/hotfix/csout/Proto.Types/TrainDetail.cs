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
public sealed class TrainDetail : IMessage<TrainDetail>, IMessage, IEquatable<TrainDetail>, IDeepCloneable<TrainDetail>, IBufferMessage
{
	private static readonly MessageParser<TrainDetail> _parser = new MessageParser<TrainDetail>((Func<TrainDetail>)(() => new TrainDetail()));

	private UnknownFieldSet _unknownFields;

	public const int TransportIdFieldNumber = 1;

	private int transportId_;

	public const int QualityFieldNumber = 2;

	private EQuality quality_;

	public const int StartTmFieldNumber = 3;

	private long startTm_;

	public const int ArriveTmFieldNumber = 4;

	private long arriveTm_;

	public const int CreatorRoleIdFieldNumber = 5;

	private long creatorRoleId_;

	public const int CarriageListFieldNumber = 6;

	private static readonly FieldCodec<TrainCarriageDetail> _repeated_carriageList_codec = FieldCodec.ForMessage<TrainCarriageDetail>(50u, TrainCarriageDetail.Parser);

	private readonly RepeatedField<TrainCarriageDetail> carriageList_ = new RepeatedField<TrainCarriageDetail>();

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<TrainDetail> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeTrainDetailReflection.Descriptor.MessageTypes[0];

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
	public long CreatorRoleId
	{
		get
		{
			return creatorRoleId_;
		}
		set
		{
			creatorRoleId_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<TrainCarriageDetail> CarriageList => carriageList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public TrainDetail()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public TrainDetail(TrainDetail other)
		: this()
	{
		transportId_ = other.transportId_;
		quality_ = other.quality_;
		startTm_ = other.startTm_;
		arriveTm_ = other.arriveTm_;
		creatorRoleId_ = other.creatorRoleId_;
		carriageList_ = other.carriageList_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public TrainDetail Clone()
	{
		return new TrainDetail(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as TrainDetail);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(TrainDetail other)
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
		if (StartTm != other.StartTm)
		{
			return false;
		}
		if (ArriveTm != other.ArriveTm)
		{
			return false;
		}
		if (CreatorRoleId != other.CreatorRoleId)
		{
			return false;
		}
		if (!carriageList_.Equals(other.carriageList_))
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
		if (StartTm != 0L)
		{
			num ^= StartTm.GetHashCode();
		}
		if (ArriveTm != 0L)
		{
			num ^= ArriveTm.GetHashCode();
		}
		if (CreatorRoleId != 0L)
		{
			num ^= CreatorRoleId.GetHashCode();
		}
		num ^= ((object)carriageList_).GetHashCode();
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
		if (StartTm != 0L)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)24);
			((WriteContext)(ref output)).WriteInt64(StartTm);
		}
		if (ArriveTm != 0L)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)32);
			((WriteContext)(ref output)).WriteInt64(ArriveTm);
		}
		if (CreatorRoleId != 0L)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)40);
			((WriteContext)(ref output)).WriteInt64(CreatorRoleId);
		}
		carriageList_.WriteTo(ref output, _repeated_carriageList_codec);
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
		if (StartTm != 0L)
		{
			num += 1 + CodedOutputStream.ComputeInt64Size(StartTm);
		}
		if (ArriveTm != 0L)
		{
			num += 1 + CodedOutputStream.ComputeInt64Size(ArriveTm);
		}
		if (CreatorRoleId != 0L)
		{
			num += 1 + CodedOutputStream.ComputeInt64Size(CreatorRoleId);
		}
		num += carriageList_.CalculateSize(_repeated_carriageList_codec);
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(TrainDetail other)
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
			if (other.StartTm != 0L)
			{
				StartTm = other.StartTm;
			}
			if (other.ArriveTm != 0L)
			{
				ArriveTm = other.ArriveTm;
			}
			if (other.CreatorRoleId != 0L)
			{
				CreatorRoleId = other.CreatorRoleId;
			}
			carriageList_.Add((IEnumerable<TrainCarriageDetail>)other.carriageList_);
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
				StartTm = ((ParseContext)(ref input)).ReadInt64();
				break;
			case 32u:
				ArriveTm = ((ParseContext)(ref input)).ReadInt64();
				break;
			case 40u:
				CreatorRoleId = ((ParseContext)(ref input)).ReadInt64();
				break;
			case 50u:
				carriageList_.AddEntriesFrom(ref input, _repeated_carriageList_codec);
				break;
			}
		}
	}
}
