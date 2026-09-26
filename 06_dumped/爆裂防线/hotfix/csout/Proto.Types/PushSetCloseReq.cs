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
public sealed class PushSetCloseReq : IMessage<PushSetCloseReq>, IMessage, IEquatable<PushSetCloseReq>, IDeepCloneable<PushSetCloseReq>, IBufferMessage
{
	private static readonly MessageParser<PushSetCloseReq> _parser = new MessageParser<PushSetCloseReq>((Func<PushSetCloseReq>)(() => new PushSetCloseReq()));

	private UnknownFieldSet _unknownFields;

	public const int CloseListFieldNumber = 1;

	private static readonly FieldCodec<EPushType> _repeated_closeList_codec = FieldCodec.ForEnum<EPushType>(10u, (Func<EPushType, int>)((EPushType x) => (int)x), (Func<int, EPushType>)((int x) => (EPushType)x));

	private readonly RepeatedField<EPushType> closeList_ = new RepeatedField<EPushType>();

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<PushSetCloseReq> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypePushSetCloseReqReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<EPushType> CloseList => closeList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public PushSetCloseReq()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public PushSetCloseReq(PushSetCloseReq other)
		: this()
	{
		closeList_ = other.closeList_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public PushSetCloseReq Clone()
	{
		return new PushSetCloseReq(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as PushSetCloseReq);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(PushSetCloseReq other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!closeList_.Equals(other.closeList_))
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
		num ^= ((object)closeList_).GetHashCode();
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
		closeList_.WriteTo(ref output, _repeated_closeList_codec);
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
		num += closeList_.CalculateSize(_repeated_closeList_codec);
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(PushSetCloseReq other)
	{
		if (other != null)
		{
			closeList_.Add((IEnumerable<EPushType>)other.closeList_);
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
			if (num != 8 && num != 10)
			{
				_unknownFields = UnknownFieldSet.MergeFieldFrom(_unknownFields, ref input);
			}
			else
			{
				closeList_.AddEntriesFrom(ref input, _repeated_closeList_codec);
			}
		}
	}
}
