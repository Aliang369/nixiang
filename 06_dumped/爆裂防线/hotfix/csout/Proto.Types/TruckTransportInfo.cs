using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class TruckTransportInfo : IMessage<TruckTransportInfo>, IMessage, IEquatable<TruckTransportInfo>, IDeepCloneable<TruckTransportInfo>, IBufferMessage
{
	private static readonly MessageParser<TruckTransportInfo> _parser = new MessageParser<TruckTransportInfo>((Func<TruckTransportInfo>)(() => new TruckTransportInfo()));

	private UnknownFieldSet _unknownFields;

	public const int TransportInfoFieldNumber = 1;

	private TruckTransportDetail transportInfo_;

	public const int RecordListFieldNumber = 2;

	private static readonly FieldCodec<TruckRobRecordDetail> _repeated_recordList_codec = FieldCodec.ForMessage<TruckRobRecordDetail>(18u, TruckRobRecordDetail.Parser);

	private readonly RepeatedField<TruckRobRecordDetail> recordList_ = new RepeatedField<TruckRobRecordDetail>();

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<TruckTransportInfo> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeTruckTransportInfoReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public TruckTransportDetail TransportInfo
	{
		get
		{
			return transportInfo_;
		}
		set
		{
			transportInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<TruckRobRecordDetail> RecordList => recordList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public TruckTransportInfo()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public TruckTransportInfo(TruckTransportInfo other)
		: this()
	{
		transportInfo_ = ((other.transportInfo_ != null) ? other.transportInfo_.Clone() : null);
		recordList_ = other.recordList_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public TruckTransportInfo Clone()
	{
		return new TruckTransportInfo(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as TruckTransportInfo);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(TruckTransportInfo other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!object.Equals(TransportInfo, other.TransportInfo))
		{
			return false;
		}
		if (!recordList_.Equals(other.recordList_))
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
		if (transportInfo_ != null)
		{
			num ^= TransportInfo.GetHashCode();
		}
		num ^= ((object)recordList_).GetHashCode();
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
		if (transportInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)10);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)TransportInfo);
		}
		recordList_.WriteTo(ref output, _repeated_recordList_codec);
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
		if (transportInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)TransportInfo);
		}
		num += recordList_.CalculateSize(_repeated_recordList_codec);
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(TruckTransportInfo other)
	{
		if (other == null)
		{
			return;
		}
		if (other.transportInfo_ != null)
		{
			if (transportInfo_ == null)
			{
				TransportInfo = new TruckTransportDetail();
			}
			TransportInfo.MergeFrom(other.TransportInfo);
		}
		recordList_.Add((IEnumerable<TruckRobRecordDetail>)other.recordList_);
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
				if (transportInfo_ == null)
				{
					TransportInfo = new TruckTransportDetail();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)TransportInfo);
				break;
			case 18u:
				recordList_.AddEntriesFrom(ref input, _repeated_recordList_codec);
				break;
			}
		}
	}
}
