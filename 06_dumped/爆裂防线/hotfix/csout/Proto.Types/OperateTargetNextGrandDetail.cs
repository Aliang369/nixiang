using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class OperateTargetNextGrandDetail : IMessage<OperateTargetNextGrandDetail>, IMessage, IEquatable<OperateTargetNextGrandDetail>, IDeepCloneable<OperateTargetNextGrandDetail>, IBufferMessage
{
	private static readonly MessageParser<OperateTargetNextGrandDetail> _parser = new MessageParser<OperateTargetNextGrandDetail>((Func<OperateTargetNextGrandDetail>)(() => new OperateTargetNextGrandDetail()));

	private UnknownFieldSet _unknownFields;

	public const int CurrentIndexFieldNumber = 1;

	private int currentIndex_;

	public const int GrandIndexFieldNumber = 2;

	private int grandIndex_;

	public const int GrandAwardListFieldNumber = 3;

	private static readonly FieldCodec<AwardDetail> _repeated_grandAwardList_codec = FieldCodec.ForMessage<AwardDetail>(26u, AwardDetail.Parser);

	private readonly RepeatedField<AwardDetail> grandAwardList_ = new RepeatedField<AwardDetail>();

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<OperateTargetNextGrandDetail> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeOperateTargetNextGrandDetailReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int CurrentIndex
	{
		get
		{
			return currentIndex_;
		}
		set
		{
			currentIndex_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int GrandIndex
	{
		get
		{
			return grandIndex_;
		}
		set
		{
			grandIndex_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<AwardDetail> GrandAwardList => grandAwardList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public OperateTargetNextGrandDetail()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public OperateTargetNextGrandDetail(OperateTargetNextGrandDetail other)
		: this()
	{
		currentIndex_ = other.currentIndex_;
		grandIndex_ = other.grandIndex_;
		grandAwardList_ = other.grandAwardList_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public OperateTargetNextGrandDetail Clone()
	{
		return new OperateTargetNextGrandDetail(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as OperateTargetNextGrandDetail);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(OperateTargetNextGrandDetail other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (CurrentIndex != other.CurrentIndex)
		{
			return false;
		}
		if (GrandIndex != other.GrandIndex)
		{
			return false;
		}
		if (!grandAwardList_.Equals(other.grandAwardList_))
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
		if (CurrentIndex != 0)
		{
			num ^= CurrentIndex.GetHashCode();
		}
		if (GrandIndex != 0)
		{
			num ^= GrandIndex.GetHashCode();
		}
		num ^= ((object)grandAwardList_).GetHashCode();
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
		if (CurrentIndex != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteInt32(CurrentIndex);
		}
		if (GrandIndex != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteInt32(GrandIndex);
		}
		grandAwardList_.WriteTo(ref output, _repeated_grandAwardList_codec);
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
		if (CurrentIndex != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(CurrentIndex);
		}
		if (GrandIndex != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(GrandIndex);
		}
		num += grandAwardList_.CalculateSize(_repeated_grandAwardList_codec);
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(OperateTargetNextGrandDetail other)
	{
		if (other != null)
		{
			if (other.CurrentIndex != 0)
			{
				CurrentIndex = other.CurrentIndex;
			}
			if (other.GrandIndex != 0)
			{
				GrandIndex = other.GrandIndex;
			}
			grandAwardList_.Add((IEnumerable<AwardDetail>)other.grandAwardList_);
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
				CurrentIndex = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 16u:
				GrandIndex = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 26u:
				grandAwardList_.AddEntriesFrom(ref input, _repeated_grandAwardList_codec);
				break;
			}
		}
	}
}
