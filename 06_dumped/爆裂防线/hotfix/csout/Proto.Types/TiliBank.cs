using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class TiliBank : IMessage<TiliBank>, IMessage, IEquatable<TiliBank>, IDeepCloneable<TiliBank>, IBufferMessage
{
	private static readonly MessageParser<TiliBank> _parser = new MessageParser<TiliBank>((Func<TiliBank>)(() => new TiliBank()));

	private UnknownFieldSet _unknownFields;

	public const int MaxCountFieldNumber = 1;

	private int maxCount_;

	public const int TiliListFieldNumber = 2;

	private static readonly FieldCodec<TiliBankDetail> _repeated_tiliList_codec = FieldCodec.ForMessage<TiliBankDetail>(18u, TiliBankDetail.Parser);

	private readonly RepeatedField<TiliBankDetail> tiliList_ = new RepeatedField<TiliBankDetail>();

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<TiliBank> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeTiliBankReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int MaxCount
	{
		get
		{
			return maxCount_;
		}
		set
		{
			maxCount_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<TiliBankDetail> TiliList => tiliList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public TiliBank()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public TiliBank(TiliBank other)
		: this()
	{
		maxCount_ = other.maxCount_;
		tiliList_ = other.tiliList_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public TiliBank Clone()
	{
		return new TiliBank(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as TiliBank);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(TiliBank other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (MaxCount != other.MaxCount)
		{
			return false;
		}
		if (!tiliList_.Equals(other.tiliList_))
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
		if (MaxCount != 0)
		{
			num ^= MaxCount.GetHashCode();
		}
		num ^= ((object)tiliList_).GetHashCode();
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
		if (MaxCount != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteInt32(MaxCount);
		}
		tiliList_.WriteTo(ref output, _repeated_tiliList_codec);
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
		if (MaxCount != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(MaxCount);
		}
		num += tiliList_.CalculateSize(_repeated_tiliList_codec);
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(TiliBank other)
	{
		if (other != null)
		{
			if (other.MaxCount != 0)
			{
				MaxCount = other.MaxCount;
			}
			tiliList_.Add((IEnumerable<TiliBankDetail>)other.tiliList_);
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
				MaxCount = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 18u:
				tiliList_.AddEntriesFrom(ref input, _repeated_tiliList_codec);
				break;
			}
		}
	}
}
