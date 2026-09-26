using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class SuitActiveAck : IMessage<SuitActiveAck>, IMessage, IEquatable<SuitActiveAck>, IDeepCloneable<SuitActiveAck>, IBufferMessage
{
	private static readonly MessageParser<SuitActiveAck> _parser = new MessageParser<SuitActiveAck>((Func<SuitActiveAck>)(() => new SuitActiveAck()));

	private UnknownFieldSet _unknownFields;

	public const int SuitIdFieldNumber = 1;

	private int suitId_;

	public const int SuitInfoFieldNumber = 2;

	private PlayerSuitInfo suitInfo_;

	public const int HeadListFieldNumber = 3;

	private static readonly FieldCodec<int> _repeated_headList_codec = FieldCodec.ForInt32(26u);

	private readonly RepeatedField<int> headList_ = new RepeatedField<int>();

	public const int DataChangeFieldNumber = 4;

	private DataChange dataChange_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<SuitActiveAck> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeSuitActiveAckReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int SuitId
	{
		get
		{
			return suitId_;
		}
		set
		{
			suitId_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public PlayerSuitInfo SuitInfo
	{
		get
		{
			return suitInfo_;
		}
		set
		{
			suitInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<int> HeadList => headList_;

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
	public SuitActiveAck()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SuitActiveAck(SuitActiveAck other)
		: this()
	{
		suitId_ = other.suitId_;
		suitInfo_ = ((other.suitInfo_ != null) ? other.suitInfo_.Clone() : null);
		headList_ = other.headList_.Clone();
		dataChange_ = ((other.dataChange_ != null) ? other.dataChange_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SuitActiveAck Clone()
	{
		return new SuitActiveAck(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as SuitActiveAck);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(SuitActiveAck other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (SuitId != other.SuitId)
		{
			return false;
		}
		if (!object.Equals(SuitInfo, other.SuitInfo))
		{
			return false;
		}
		if (!headList_.Equals(other.headList_))
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
		if (SuitId != 0)
		{
			num ^= SuitId.GetHashCode();
		}
		if (suitInfo_ != null)
		{
			num ^= SuitInfo.GetHashCode();
		}
		num ^= ((object)headList_).GetHashCode();
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
		if (SuitId != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteInt32(SuitId);
		}
		if (suitInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)18);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)SuitInfo);
		}
		headList_.WriteTo(ref output, _repeated_headList_codec);
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
		if (SuitId != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(SuitId);
		}
		if (suitInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)SuitInfo);
		}
		num += headList_.CalculateSize(_repeated_headList_codec);
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
	public void MergeFrom(SuitActiveAck other)
	{
		if (other == null)
		{
			return;
		}
		if (other.SuitId != 0)
		{
			SuitId = other.SuitId;
		}
		if (other.suitInfo_ != null)
		{
			if (suitInfo_ == null)
			{
				SuitInfo = new PlayerSuitInfo();
			}
			SuitInfo.MergeFrom(other.SuitInfo);
		}
		headList_.Add((IEnumerable<int>)other.headList_);
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
			case 8u:
				SuitId = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 18u:
				if (suitInfo_ == null)
				{
					SuitInfo = new PlayerSuitInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)SuitInfo);
				break;
			case 24u:
			case 26u:
				headList_.AddEntriesFrom(ref input, _repeated_headList_codec);
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
