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
public sealed class TruckSelfInfo : IMessage<TruckSelfInfo>, IMessage, IEquatable<TruckSelfInfo>, IDeepCloneable<TruckSelfInfo>, IBufferMessage
{
	private static readonly MessageParser<TruckSelfInfo> _parser = new MessageParser<TruckSelfInfo>((Func<TruckSelfInfo>)(() => new TruckSelfInfo()));

	private UnknownFieldSet _unknownFields;

	public const int TransportUsedFieldNumber = 1;

	private int transportUsed_;

	public const int TransportLimitFieldNumber = 2;

	private int transportLimit_;

	public const int TransportBoughtFieldNumber = 3;

	private int transportBought_;

	public const int RobUsedFieldNumber = 4;

	private int robUsed_;

	public const int RobLimitFieldNumber = 5;

	private int robLimit_;

	public const int RobBoughtFieldNumber = 6;

	private int robBought_;

	public const int TruckListFieldNumber = 7;

	private static readonly FieldCodec<TruckDetail> _repeated_truckList_codec = FieldCodec.ForMessage<TruckDetail>(58u, TruckDetail.Parser);

	private readonly RepeatedField<TruckDetail> truckList_ = new RepeatedField<TruckDetail>();

	public const int WantedListFieldNumber = 8;

	private static readonly FieldCodec<int> _repeated_wantedList_codec = FieldCodec.ForInt32(66u);

	private readonly RepeatedField<int> wantedList_ = new RepeatedField<int>();

	public const int PrivilegeFieldNumber = 9;

	private EBoolState privilege_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<TruckSelfInfo> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeTruckSelfInfoReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int TransportUsed
	{
		get
		{
			return transportUsed_;
		}
		set
		{
			transportUsed_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int TransportLimit
	{
		get
		{
			return transportLimit_;
		}
		set
		{
			transportLimit_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int TransportBought
	{
		get
		{
			return transportBought_;
		}
		set
		{
			transportBought_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int RobUsed
	{
		get
		{
			return robUsed_;
		}
		set
		{
			robUsed_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int RobLimit
	{
		get
		{
			return robLimit_;
		}
		set
		{
			robLimit_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int RobBought
	{
		get
		{
			return robBought_;
		}
		set
		{
			robBought_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<TruckDetail> TruckList => truckList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<int> WantedList => wantedList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public EBoolState Privilege
	{
		get
		{
			return privilege_;
		}
		set
		{
			privilege_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public TruckSelfInfo()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public TruckSelfInfo(TruckSelfInfo other)
		: this()
	{
		transportUsed_ = other.transportUsed_;
		transportLimit_ = other.transportLimit_;
		transportBought_ = other.transportBought_;
		robUsed_ = other.robUsed_;
		robLimit_ = other.robLimit_;
		robBought_ = other.robBought_;
		truckList_ = other.truckList_.Clone();
		wantedList_ = other.wantedList_.Clone();
		privilege_ = other.privilege_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public TruckSelfInfo Clone()
	{
		return new TruckSelfInfo(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as TruckSelfInfo);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(TruckSelfInfo other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (TransportUsed != other.TransportUsed)
		{
			return false;
		}
		if (TransportLimit != other.TransportLimit)
		{
			return false;
		}
		if (TransportBought != other.TransportBought)
		{
			return false;
		}
		if (RobUsed != other.RobUsed)
		{
			return false;
		}
		if (RobLimit != other.RobLimit)
		{
			return false;
		}
		if (RobBought != other.RobBought)
		{
			return false;
		}
		if (!truckList_.Equals(other.truckList_))
		{
			return false;
		}
		if (!wantedList_.Equals(other.wantedList_))
		{
			return false;
		}
		if (Privilege != other.Privilege)
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
		if (TransportUsed != 0)
		{
			num ^= TransportUsed.GetHashCode();
		}
		if (TransportLimit != 0)
		{
			num ^= TransportLimit.GetHashCode();
		}
		if (TransportBought != 0)
		{
			num ^= TransportBought.GetHashCode();
		}
		if (RobUsed != 0)
		{
			num ^= RobUsed.GetHashCode();
		}
		if (RobLimit != 0)
		{
			num ^= RobLimit.GetHashCode();
		}
		if (RobBought != 0)
		{
			num ^= RobBought.GetHashCode();
		}
		num ^= ((object)truckList_).GetHashCode();
		num ^= ((object)wantedList_).GetHashCode();
		if (Privilege != EBoolState.False)
		{
			num ^= Privilege.GetHashCode();
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
		if (TransportUsed != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteInt32(TransportUsed);
		}
		if (TransportLimit != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteInt32(TransportLimit);
		}
		if (TransportBought != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)24);
			((WriteContext)(ref output)).WriteInt32(TransportBought);
		}
		if (RobUsed != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)32);
			((WriteContext)(ref output)).WriteInt32(RobUsed);
		}
		if (RobLimit != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)40);
			((WriteContext)(ref output)).WriteInt32(RobLimit);
		}
		if (RobBought != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)48);
			((WriteContext)(ref output)).WriteInt32(RobBought);
		}
		truckList_.WriteTo(ref output, _repeated_truckList_codec);
		wantedList_.WriteTo(ref output, _repeated_wantedList_codec);
		if (Privilege != EBoolState.False)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)72);
			((WriteContext)(ref output)).WriteEnum((int)Privilege);
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
		if (TransportUsed != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(TransportUsed);
		}
		if (TransportLimit != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(TransportLimit);
		}
		if (TransportBought != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(TransportBought);
		}
		if (RobUsed != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(RobUsed);
		}
		if (RobLimit != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(RobLimit);
		}
		if (RobBought != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(RobBought);
		}
		num += truckList_.CalculateSize(_repeated_truckList_codec);
		num += wantedList_.CalculateSize(_repeated_wantedList_codec);
		if (Privilege != EBoolState.False)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)Privilege);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(TruckSelfInfo other)
	{
		if (other != null)
		{
			if (other.TransportUsed != 0)
			{
				TransportUsed = other.TransportUsed;
			}
			if (other.TransportLimit != 0)
			{
				TransportLimit = other.TransportLimit;
			}
			if (other.TransportBought != 0)
			{
				TransportBought = other.TransportBought;
			}
			if (other.RobUsed != 0)
			{
				RobUsed = other.RobUsed;
			}
			if (other.RobLimit != 0)
			{
				RobLimit = other.RobLimit;
			}
			if (other.RobBought != 0)
			{
				RobBought = other.RobBought;
			}
			truckList_.Add((IEnumerable<TruckDetail>)other.truckList_);
			wantedList_.Add((IEnumerable<int>)other.wantedList_);
			if (other.Privilege != EBoolState.False)
			{
				Privilege = other.Privilege;
			}
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
				TransportUsed = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 16u:
				TransportLimit = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 24u:
				TransportBought = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 32u:
				RobUsed = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 40u:
				RobLimit = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 48u:
				RobBought = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 58u:
				truckList_.AddEntriesFrom(ref input, _repeated_truckList_codec);
				break;
			case 64u:
			case 66u:
				wantedList_.AddEntriesFrom(ref input, _repeated_wantedList_codec);
				break;
			case 72u:
				Privilege = (EBoolState)((ParseContext)(ref input)).ReadEnum();
				break;
			}
		}
	}
}
