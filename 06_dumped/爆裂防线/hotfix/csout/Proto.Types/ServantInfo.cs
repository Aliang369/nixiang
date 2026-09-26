using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class ServantInfo : IMessage<ServantInfo>, IMessage, IEquatable<ServantInfo>, IDeepCloneable<ServantInfo>, IBufferMessage
{
	private static readonly MessageParser<ServantInfo> _parser = new MessageParser<ServantInfo>((Func<ServantInfo>)(() => new ServantInfo()));

	private UnknownFieldSet _unknownFields;

	public const int ServantListFieldNumber = 1;

	private static readonly FieldCodec<ServantDetail> _repeated_servantList_codec = FieldCodec.ForMessage<ServantDetail>(10u, ServantDetail.Parser);

	private readonly RepeatedField<ServantDetail> servantList_ = new RepeatedField<ServantDetail>();

	public const int BagLimitFieldNumber = 2;

	private int bagLimit_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<ServantInfo> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeServantInfoReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<ServantDetail> ServantList => servantList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int BagLimit
	{
		get
		{
			return bagLimit_;
		}
		set
		{
			bagLimit_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ServantInfo()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ServantInfo(ServantInfo other)
		: this()
	{
		servantList_ = other.servantList_.Clone();
		bagLimit_ = other.bagLimit_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ServantInfo Clone()
	{
		return new ServantInfo(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as ServantInfo);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(ServantInfo other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!servantList_.Equals(other.servantList_))
		{
			return false;
		}
		if (BagLimit != other.BagLimit)
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
		num ^= ((object)servantList_).GetHashCode();
		if (BagLimit != 0)
		{
			num ^= BagLimit.GetHashCode();
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
		servantList_.WriteTo(ref output, _repeated_servantList_codec);
		if (BagLimit != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteInt32(BagLimit);
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
		num += servantList_.CalculateSize(_repeated_servantList_codec);
		if (BagLimit != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(BagLimit);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(ServantInfo other)
	{
		if (other != null)
		{
			servantList_.Add((IEnumerable<ServantDetail>)other.servantList_);
			if (other.BagLimit != 0)
			{
				BagLimit = other.BagLimit;
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
			case 10u:
				servantList_.AddEntriesFrom(ref input, _repeated_servantList_codec);
				break;
			case 16u:
				BagLimit = ((ParseContext)(ref input)).ReadInt32();
				break;
			}
		}
	}
}
