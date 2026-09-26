using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class BagSelectBoxOpenReq : IMessage<BagSelectBoxOpenReq>, IMessage, IEquatable<BagSelectBoxOpenReq>, IDeepCloneable<BagSelectBoxOpenReq>, IBufferMessage
{
	private static readonly MessageParser<BagSelectBoxOpenReq> _parser = new MessageParser<BagSelectBoxOpenReq>((Func<BagSelectBoxOpenReq>)(() => new BagSelectBoxOpenReq()));

	private UnknownFieldSet _unknownFields;

	public const int BoxTidFieldNumber = 1;

	private int boxTid_;

	public const int OpenListFieldNumber = 2;

	private static readonly FieldCodec<SelectDetail> _repeated_openList_codec = FieldCodec.ForMessage<SelectDetail>(18u, SelectDetail.Parser);

	private readonly RepeatedField<SelectDetail> openList_ = new RepeatedField<SelectDetail>();

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<BagSelectBoxOpenReq> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeBagSelectBoxOpenReqReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int BoxTid
	{
		get
		{
			return boxTid_;
		}
		set
		{
			boxTid_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<SelectDetail> OpenList => openList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public BagSelectBoxOpenReq()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public BagSelectBoxOpenReq(BagSelectBoxOpenReq other)
		: this()
	{
		boxTid_ = other.boxTid_;
		openList_ = other.openList_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public BagSelectBoxOpenReq Clone()
	{
		return new BagSelectBoxOpenReq(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as BagSelectBoxOpenReq);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(BagSelectBoxOpenReq other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (BoxTid != other.BoxTid)
		{
			return false;
		}
		if (!openList_.Equals(other.openList_))
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
		if (BoxTid != 0)
		{
			num ^= BoxTid.GetHashCode();
		}
		num ^= ((object)openList_).GetHashCode();
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
		if (BoxTid != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteInt32(BoxTid);
		}
		openList_.WriteTo(ref output, _repeated_openList_codec);
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
		if (BoxTid != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(BoxTid);
		}
		num += openList_.CalculateSize(_repeated_openList_codec);
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(BagSelectBoxOpenReq other)
	{
		if (other != null)
		{
			if (other.BoxTid != 0)
			{
				BoxTid = other.BoxTid;
			}
			openList_.Add((IEnumerable<SelectDetail>)other.openList_);
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
				BoxTid = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 18u:
				openList_.AddEntriesFrom(ref input, _repeated_openList_codec);
				break;
			}
		}
	}
}
