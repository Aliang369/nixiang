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
public sealed class ErrorCodeAck : IMessage<ErrorCodeAck>, IMessage, IEquatable<ErrorCodeAck>, IDeepCloneable<ErrorCodeAck>, IBufferMessage
{
	private static readonly MessageParser<ErrorCodeAck> _parser = new MessageParser<ErrorCodeAck>((Func<ErrorCodeAck>)(() => new ErrorCodeAck()));

	private UnknownFieldSet _unknownFields;

	public const int ErrorCodeFieldNumber = 1;

	private EErrorCode errorCode_;

	public const int ErrorParamsFieldNumber = 2;

	private static readonly FieldCodec<string> _repeated_errorParams_codec = FieldCodec.ForString(18u);

	private readonly RepeatedField<string> errorParams_ = new RepeatedField<string>();

	public const int ReqIdFieldNumber = 3;

	private int reqId_;

	public const int SyncIdFieldNumber = 4;

	private int syncId_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<ErrorCodeAck> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeErrorCodeAckReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public EErrorCode ErrorCode
	{
		get
		{
			return errorCode_;
		}
		set
		{
			errorCode_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<string> ErrorParams => errorParams_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int ReqId
	{
		get
		{
			return reqId_;
		}
		set
		{
			reqId_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int SyncId
	{
		get
		{
			return syncId_;
		}
		set
		{
			syncId_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ErrorCodeAck()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ErrorCodeAck(ErrorCodeAck other)
		: this()
	{
		errorCode_ = other.errorCode_;
		errorParams_ = other.errorParams_.Clone();
		reqId_ = other.reqId_;
		syncId_ = other.syncId_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ErrorCodeAck Clone()
	{
		return new ErrorCodeAck(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as ErrorCodeAck);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(ErrorCodeAck other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (ErrorCode != other.ErrorCode)
		{
			return false;
		}
		if (!errorParams_.Equals(other.errorParams_))
		{
			return false;
		}
		if (ReqId != other.ReqId)
		{
			return false;
		}
		if (SyncId != other.SyncId)
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
		if (ErrorCode != EErrorCode.EmptyValue)
		{
			num ^= ErrorCode.GetHashCode();
		}
		num ^= ((object)errorParams_).GetHashCode();
		if (ReqId != 0)
		{
			num ^= ReqId.GetHashCode();
		}
		if (SyncId != 0)
		{
			num ^= SyncId.GetHashCode();
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
		if (ErrorCode != EErrorCode.EmptyValue)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteEnum((int)ErrorCode);
		}
		errorParams_.WriteTo(ref output, _repeated_errorParams_codec);
		if (ReqId != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)24);
			((WriteContext)(ref output)).WriteInt32(ReqId);
		}
		if (SyncId != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)32);
			((WriteContext)(ref output)).WriteInt32(SyncId);
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
		if (ErrorCode != EErrorCode.EmptyValue)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)ErrorCode);
		}
		num += errorParams_.CalculateSize(_repeated_errorParams_codec);
		if (ReqId != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(ReqId);
		}
		if (SyncId != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(SyncId);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(ErrorCodeAck other)
	{
		if (other != null)
		{
			if (other.ErrorCode != EErrorCode.EmptyValue)
			{
				ErrorCode = other.ErrorCode;
			}
			errorParams_.Add((IEnumerable<string>)other.errorParams_);
			if (other.ReqId != 0)
			{
				ReqId = other.ReqId;
			}
			if (other.SyncId != 0)
			{
				SyncId = other.SyncId;
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
				ErrorCode = (EErrorCode)((ParseContext)(ref input)).ReadEnum();
				break;
			case 18u:
				errorParams_.AddEntriesFrom(ref input, _repeated_errorParams_codec);
				break;
			case 24u:
				ReqId = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 32u:
				SyncId = ((ParseContext)(ref input)).ReadInt32();
				break;
			}
		}
	}
}
