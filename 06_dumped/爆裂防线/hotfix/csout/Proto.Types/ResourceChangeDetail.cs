using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class ResourceChangeDetail : IMessage<ResourceChangeDetail>, IMessage, IEquatable<ResourceChangeDetail>, IDeepCloneable<ResourceChangeDetail>, IBufferMessage
{
	private static readonly MessageParser<ResourceChangeDetail> _parser = new MessageParser<ResourceChangeDetail>((Func<ResourceChangeDetail>)(() => new ResourceChangeDetail()));

	private UnknownFieldSet _unknownFields;

	public const int UpdateFieldNumber = 1;

	private static readonly FieldCodec<ResourceDetail> _repeated_update_codec = FieldCodec.ForMessage<ResourceDetail>(10u, ResourceDetail.Parser);

	private readonly RepeatedField<ResourceDetail> update_ = new RepeatedField<ResourceDetail>();

	public const int DelFieldNumber = 2;

	private static readonly FieldCodec<int> _repeated_del_codec = FieldCodec.ForInt32(18u);

	private readonly RepeatedField<int> del_ = new RepeatedField<int>();

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<ResourceChangeDetail> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeResourceChangeDetailReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<ResourceDetail> Update => update_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<int> Del => del_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ResourceChangeDetail()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ResourceChangeDetail(ResourceChangeDetail other)
		: this()
	{
		update_ = other.update_.Clone();
		del_ = other.del_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ResourceChangeDetail Clone()
	{
		return new ResourceChangeDetail(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as ResourceChangeDetail);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(ResourceChangeDetail other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!update_.Equals(other.update_))
		{
			return false;
		}
		if (!del_.Equals(other.del_))
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
		num ^= ((object)update_).GetHashCode();
		num ^= ((object)del_).GetHashCode();
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
		update_.WriteTo(ref output, _repeated_update_codec);
		del_.WriteTo(ref output, _repeated_del_codec);
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
		num += update_.CalculateSize(_repeated_update_codec);
		num += del_.CalculateSize(_repeated_del_codec);
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(ResourceChangeDetail other)
	{
		if (other != null)
		{
			update_.Add((IEnumerable<ResourceDetail>)other.update_);
			del_.Add((IEnumerable<int>)other.del_);
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
			case 10u:
				update_.AddEntriesFrom(ref input, _repeated_update_codec);
				break;
			case 16u:
			case 18u:
				del_.AddEntriesFrom(ref input, _repeated_del_codec);
				break;
			}
		}
	}
}
