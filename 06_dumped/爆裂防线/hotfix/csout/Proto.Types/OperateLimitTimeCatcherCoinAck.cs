using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class OperateLimitTimeCatcherCoinAck : IMessage<OperateLimitTimeCatcherCoinAck>, IMessage, IEquatable<OperateLimitTimeCatcherCoinAck>, IDeepCloneable<OperateLimitTimeCatcherCoinAck>, IBufferMessage
{
	private static readonly MessageParser<OperateLimitTimeCatcherCoinAck> _parser = new MessageParser<OperateLimitTimeCatcherCoinAck>((Func<OperateLimitTimeCatcherCoinAck>)(() => new OperateLimitTimeCatcherCoinAck()));

	private UnknownFieldSet _unknownFields;

	public const int LimitTimeCatcherListFieldNumber = 1;

	private static readonly FieldCodec<LimitTimeCatcherDetail> _repeated_limitTimeCatcherList_codec = FieldCodec.ForMessage<LimitTimeCatcherDetail>(10u, LimitTimeCatcherDetail.Parser);

	private readonly RepeatedField<LimitTimeCatcherDetail> limitTimeCatcherList_ = new RepeatedField<LimitTimeCatcherDetail>();

	public const int DataChangeFieldNumber = 2;

	private DataChange dataChange_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<OperateLimitTimeCatcherCoinAck> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeOperateLimitTimeCatcherCoinAckReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<LimitTimeCatcherDetail> LimitTimeCatcherList => limitTimeCatcherList_;

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
	public OperateLimitTimeCatcherCoinAck()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public OperateLimitTimeCatcherCoinAck(OperateLimitTimeCatcherCoinAck other)
		: this()
	{
		limitTimeCatcherList_ = other.limitTimeCatcherList_.Clone();
		dataChange_ = ((other.dataChange_ != null) ? other.dataChange_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public OperateLimitTimeCatcherCoinAck Clone()
	{
		return new OperateLimitTimeCatcherCoinAck(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as OperateLimitTimeCatcherCoinAck);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(OperateLimitTimeCatcherCoinAck other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!limitTimeCatcherList_.Equals(other.limitTimeCatcherList_))
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
		num ^= ((object)limitTimeCatcherList_).GetHashCode();
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
		limitTimeCatcherList_.WriteTo(ref output, _repeated_limitTimeCatcherList_codec);
		if (dataChange_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)18);
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
		num += limitTimeCatcherList_.CalculateSize(_repeated_limitTimeCatcherList_codec);
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
	public void MergeFrom(OperateLimitTimeCatcherCoinAck other)
	{
		if (other == null)
		{
			return;
		}
		limitTimeCatcherList_.Add((IEnumerable<LimitTimeCatcherDetail>)other.limitTimeCatcherList_);
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
				limitTimeCatcherList_.AddEntriesFrom(ref input, _repeated_limitTimeCatcherList_codec);
				break;
			case 18u:
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
