using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class TruckDetail : IMessage<TruckDetail>, IMessage, IEquatable<TruckDetail>, IDeepCloneable<TruckDetail>, IBufferMessage
{
	private static readonly MessageParser<TruckDetail> _parser = new MessageParser<TruckDetail>((Func<TruckDetail>)(() => new TruckDetail()));

	private UnknownFieldSet _unknownFields;

	public const int TruckIdFieldNumber = 1;

	private int truckId_;

	public const int TransportInfoFieldNumber = 2;

	private TruckTransportDetail transportInfo_;

	public const int RefreshCountFieldNumber = 3;

	private int refreshCount_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<TruckDetail> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeTruckDetailReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int TruckId
	{
		get
		{
			return truckId_;
		}
		set
		{
			truckId_ = value;
		}
	}

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
	public int RefreshCount
	{
		get
		{
			return refreshCount_;
		}
		set
		{
			refreshCount_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public TruckDetail()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public TruckDetail(TruckDetail other)
		: this()
	{
		truckId_ = other.truckId_;
		transportInfo_ = ((other.transportInfo_ != null) ? other.transportInfo_.Clone() : null);
		refreshCount_ = other.refreshCount_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public TruckDetail Clone()
	{
		return new TruckDetail(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as TruckDetail);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(TruckDetail other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (TruckId != other.TruckId)
		{
			return false;
		}
		if (!object.Equals(TransportInfo, other.TransportInfo))
		{
			return false;
		}
		if (RefreshCount != other.RefreshCount)
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
		if (TruckId != 0)
		{
			num ^= TruckId.GetHashCode();
		}
		if (transportInfo_ != null)
		{
			num ^= TransportInfo.GetHashCode();
		}
		if (RefreshCount != 0)
		{
			num ^= RefreshCount.GetHashCode();
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
		if (TruckId != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteInt32(TruckId);
		}
		if (transportInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)18);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)TransportInfo);
		}
		if (RefreshCount != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)24);
			((WriteContext)(ref output)).WriteInt32(RefreshCount);
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
		if (TruckId != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(TruckId);
		}
		if (transportInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)TransportInfo);
		}
		if (RefreshCount != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(RefreshCount);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(TruckDetail other)
	{
		if (other == null)
		{
			return;
		}
		if (other.TruckId != 0)
		{
			TruckId = other.TruckId;
		}
		if (other.transportInfo_ != null)
		{
			if (transportInfo_ == null)
			{
				TransportInfo = new TruckTransportDetail();
			}
			TransportInfo.MergeFrom(other.TransportInfo);
		}
		if (other.RefreshCount != 0)
		{
			RefreshCount = other.RefreshCount;
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
			case 8u:
				TruckId = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 18u:
				if (transportInfo_ == null)
				{
					TransportInfo = new TruckTransportDetail();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)TransportInfo);
				break;
			case 24u:
				RefreshCount = ((ParseContext)(ref input)).ReadInt32();
				break;
			}
		}
	}
}
