using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class FacilityCreateAck : IMessage<FacilityCreateAck>, IMessage, IEquatable<FacilityCreateAck>, IDeepCloneable<FacilityCreateAck>, IBufferMessage
{
	private static readonly MessageParser<FacilityCreateAck> _parser = new MessageParser<FacilityCreateAck>((Func<FacilityCreateAck>)(() => new FacilityCreateAck()));

	private UnknownFieldSet _unknownFields;

	public const int FacilitySelfInfoFieldNumber = 1;

	private FacilitySelfInfo facilitySelfInfo_;

	public const int FacilityMapInfoFieldNumber = 2;

	private FacilityMapInfo facilityMapInfo_;

	public const int DataChangeFieldNumber = 3;

	private DataChange dataChange_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<FacilityCreateAck> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeFacilityCreateAckReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public FacilitySelfInfo FacilitySelfInfo
	{
		get
		{
			return facilitySelfInfo_;
		}
		set
		{
			facilitySelfInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public FacilityMapInfo FacilityMapInfo
	{
		get
		{
			return facilityMapInfo_;
		}
		set
		{
			facilityMapInfo_ = value;
		}
	}

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
	public FacilityCreateAck()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public FacilityCreateAck(FacilityCreateAck other)
		: this()
	{
		facilitySelfInfo_ = ((other.facilitySelfInfo_ != null) ? other.facilitySelfInfo_.Clone() : null);
		facilityMapInfo_ = ((other.facilityMapInfo_ != null) ? other.facilityMapInfo_.Clone() : null);
		dataChange_ = ((other.dataChange_ != null) ? other.dataChange_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public FacilityCreateAck Clone()
	{
		return new FacilityCreateAck(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as FacilityCreateAck);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(FacilityCreateAck other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!object.Equals(FacilitySelfInfo, other.FacilitySelfInfo))
		{
			return false;
		}
		if (!object.Equals(FacilityMapInfo, other.FacilityMapInfo))
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
		if (facilitySelfInfo_ != null)
		{
			num ^= FacilitySelfInfo.GetHashCode();
		}
		if (facilityMapInfo_ != null)
		{
			num ^= FacilityMapInfo.GetHashCode();
		}
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
		if (facilitySelfInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)10);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)FacilitySelfInfo);
		}
		if (facilityMapInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)18);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)FacilityMapInfo);
		}
		if (dataChange_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)26);
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
		if (facilitySelfInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)FacilitySelfInfo);
		}
		if (facilityMapInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)FacilityMapInfo);
		}
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
	public void MergeFrom(FacilityCreateAck other)
	{
		if (other == null)
		{
			return;
		}
		if (other.facilitySelfInfo_ != null)
		{
			if (facilitySelfInfo_ == null)
			{
				FacilitySelfInfo = new FacilitySelfInfo();
			}
			FacilitySelfInfo.MergeFrom(other.FacilitySelfInfo);
		}
		if (other.facilityMapInfo_ != null)
		{
			if (facilityMapInfo_ == null)
			{
				FacilityMapInfo = new FacilityMapInfo();
			}
			FacilityMapInfo.MergeFrom(other.FacilityMapInfo);
		}
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
				if (facilitySelfInfo_ == null)
				{
					FacilitySelfInfo = new FacilitySelfInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)FacilitySelfInfo);
				break;
			case 18u:
				if (facilityMapInfo_ == null)
				{
					FacilityMapInfo = new FacilityMapInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)FacilityMapInfo);
				break;
			case 26u:
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
