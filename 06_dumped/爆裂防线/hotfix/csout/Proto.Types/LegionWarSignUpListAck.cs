using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class LegionWarSignUpListAck : IMessage<LegionWarSignUpListAck>, IMessage, IEquatable<LegionWarSignUpListAck>, IDeepCloneable<LegionWarSignUpListAck>, IBufferMessage
{
	private static readonly MessageParser<LegionWarSignUpListAck> _parser = new MessageParser<LegionWarSignUpListAck>((Func<LegionWarSignUpListAck>)(() => new LegionWarSignUpListAck()));

	private UnknownFieldSet _unknownFields;

	public const int PageFieldNumber = 1;

	private int page_;

	public const int SizeFieldNumber = 2;

	private int size_;

	public const int CountFieldNumber = 3;

	private int count_;

	public const int LegionListFieldNumber = 4;

	private static readonly FieldCodec<LegionBaseInfo> _repeated_legionList_codec = FieldCodec.ForMessage<LegionBaseInfo>(34u, LegionBaseInfo.Parser);

	private readonly RepeatedField<LegionBaseInfo> legionList_ = new RepeatedField<LegionBaseInfo>();

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<LegionWarSignUpListAck> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeLegionWarSignUpListAckReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int Page
	{
		get
		{
			return page_;
		}
		set
		{
			page_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int Size
	{
		get
		{
			return size_;
		}
		set
		{
			size_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int Count
	{
		get
		{
			return count_;
		}
		set
		{
			count_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<LegionBaseInfo> LegionList => legionList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public LegionWarSignUpListAck()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public LegionWarSignUpListAck(LegionWarSignUpListAck other)
		: this()
	{
		page_ = other.page_;
		size_ = other.size_;
		count_ = other.count_;
		legionList_ = other.legionList_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public LegionWarSignUpListAck Clone()
	{
		return new LegionWarSignUpListAck(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as LegionWarSignUpListAck);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(LegionWarSignUpListAck other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (Page != other.Page)
		{
			return false;
		}
		if (Size != other.Size)
		{
			return false;
		}
		if (Count != other.Count)
		{
			return false;
		}
		if (!legionList_.Equals(other.legionList_))
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
		if (Page != 0)
		{
			num ^= Page.GetHashCode();
		}
		if (Size != 0)
		{
			num ^= Size.GetHashCode();
		}
		if (Count != 0)
		{
			num ^= Count.GetHashCode();
		}
		num ^= ((object)legionList_).GetHashCode();
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
		if (Page != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteInt32(Page);
		}
		if (Size != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteInt32(Size);
		}
		if (Count != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)24);
			((WriteContext)(ref output)).WriteInt32(Count);
		}
		legionList_.WriteTo(ref output, _repeated_legionList_codec);
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
		if (Page != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(Page);
		}
		if (Size != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(Size);
		}
		if (Count != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(Count);
		}
		num += legionList_.CalculateSize(_repeated_legionList_codec);
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(LegionWarSignUpListAck other)
	{
		if (other != null)
		{
			if (other.Page != 0)
			{
				Page = other.Page;
			}
			if (other.Size != 0)
			{
				Size = other.Size;
			}
			if (other.Count != 0)
			{
				Count = other.Count;
			}
			legionList_.Add((IEnumerable<LegionBaseInfo>)other.legionList_);
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
				Page = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 16u:
				Size = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 24u:
				Count = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 34u:
				legionList_.AddEntriesFrom(ref input, _repeated_legionList_codec);
				break;
			}
		}
	}
}
