using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class OperateFusionUpdateAck : IMessage<OperateFusionUpdateAck>, IMessage, IEquatable<OperateFusionUpdateAck>, IDeepCloneable<OperateFusionUpdateAck>, IBufferMessage
{
	private static readonly MessageParser<OperateFusionUpdateAck> _parser = new MessageParser<OperateFusionUpdateAck>((Func<OperateFusionUpdateAck>)(() => new OperateFusionUpdateAck()));

	private UnknownFieldSet _unknownFields;

	public const int FusionInfoFieldNumber = 1;

	private FusionInfo fusionInfo_;

	public const int DataFieldNumber = 2;

	private string data_ = "";

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<OperateFusionUpdateAck> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeOperateFusionUpdateAckReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public FusionInfo FusionInfo
	{
		get
		{
			return fusionInfo_;
		}
		set
		{
			fusionInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public string Data
	{
		get
		{
			return data_;
		}
		set
		{
			data_ = ProtoPreconditions.CheckNotNull<string>(value, "value");
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public OperateFusionUpdateAck()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public OperateFusionUpdateAck(OperateFusionUpdateAck other)
		: this()
	{
		fusionInfo_ = ((other.fusionInfo_ != null) ? other.fusionInfo_.Clone() : null);
		data_ = other.data_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public OperateFusionUpdateAck Clone()
	{
		return new OperateFusionUpdateAck(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as OperateFusionUpdateAck);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(OperateFusionUpdateAck other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!object.Equals(FusionInfo, other.FusionInfo))
		{
			return false;
		}
		if (Data != other.Data)
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
		if (fusionInfo_ != null)
		{
			num ^= FusionInfo.GetHashCode();
		}
		if (Data.Length != 0)
		{
			num ^= Data.GetHashCode();
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
		if (fusionInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)10);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)FusionInfo);
		}
		if (Data.Length != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)18);
			((WriteContext)(ref output)).WriteString(Data);
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
		if (fusionInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)FusionInfo);
		}
		if (Data.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(Data);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(OperateFusionUpdateAck other)
	{
		if (other == null)
		{
			return;
		}
		if (other.fusionInfo_ != null)
		{
			if (fusionInfo_ == null)
			{
				FusionInfo = new FusionInfo();
			}
			FusionInfo.MergeFrom(other.FusionInfo);
		}
		if (other.Data.Length != 0)
		{
			Data = other.Data;
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
				if (fusionInfo_ == null)
				{
					FusionInfo = new FusionInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)FusionInfo);
				break;
			case 18u:
				Data = ((ParseContext)(ref input)).ReadString();
				break;
			}
		}
	}
}
