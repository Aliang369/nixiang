using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class OperateWallSubjectDataAck : IMessage<OperateWallSubjectDataAck>, IMessage, IEquatable<OperateWallSubjectDataAck>, IDeepCloneable<OperateWallSubjectDataAck>, IBufferMessage
{
	private static readonly MessageParser<OperateWallSubjectDataAck> _parser = new MessageParser<OperateWallSubjectDataAck>((Func<OperateWallSubjectDataAck>)(() => new OperateWallSubjectDataAck()));

	private UnknownFieldSet _unknownFields;

	public const int WallSubjectListFieldNumber = 1;

	private static readonly FieldCodec<WallSubjectDetail> _repeated_wallSubjectList_codec = FieldCodec.ForMessage<WallSubjectDetail>(10u, WallSubjectDetail.Parser);

	private readonly RepeatedField<WallSubjectDetail> wallSubjectList_ = new RepeatedField<WallSubjectDetail>();

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<OperateWallSubjectDataAck> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeOperateWallSubjectDataAckReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<WallSubjectDetail> WallSubjectList => wallSubjectList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public OperateWallSubjectDataAck()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public OperateWallSubjectDataAck(OperateWallSubjectDataAck other)
		: this()
	{
		wallSubjectList_ = other.wallSubjectList_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public OperateWallSubjectDataAck Clone()
	{
		return new OperateWallSubjectDataAck(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as OperateWallSubjectDataAck);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(OperateWallSubjectDataAck other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!wallSubjectList_.Equals(other.wallSubjectList_))
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
		num ^= ((object)wallSubjectList_).GetHashCode();
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
		wallSubjectList_.WriteTo(ref output, _repeated_wallSubjectList_codec);
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
		num += wallSubjectList_.CalculateSize(_repeated_wallSubjectList_codec);
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(OperateWallSubjectDataAck other)
	{
		if (other != null)
		{
			wallSubjectList_.Add((IEnumerable<WallSubjectDetail>)other.wallSubjectList_);
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
			if (num != 10)
			{
				_unknownFields = UnknownFieldSet.MergeFieldFrom(_unknownFields, ref input);
			}
			else
			{
				wallSubjectList_.AddEntriesFrom(ref input, _repeated_wallSubjectList_codec);
			}
		}
	}
}
