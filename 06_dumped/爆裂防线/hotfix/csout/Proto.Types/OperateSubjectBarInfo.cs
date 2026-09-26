using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class OperateSubjectBarInfo : IMessage<OperateSubjectBarInfo>, IMessage, IEquatable<OperateSubjectBarInfo>, IDeepCloneable<OperateSubjectBarInfo>, IBufferMessage
{
	private static readonly MessageParser<OperateSubjectBarInfo> _parser = new MessageParser<OperateSubjectBarInfo>((Func<OperateSubjectBarInfo>)(() => new OperateSubjectBarInfo()));

	private UnknownFieldSet _unknownFields;

	public const int RareHitFieldNumber = 1;

	private int rareHit_;

	public const int TotalCountFieldNumber = 2;

	private int totalCount_;

	public const int RelicListFieldNumber = 3;

	private static readonly FieldCodec<int> _repeated_relicList_codec = FieldCodec.ForInt32(26u);

	private readonly RepeatedField<int> relicList_ = new RepeatedField<int>();

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<OperateSubjectBarInfo> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeOperateSubjectBarInfoReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int RareHit
	{
		get
		{
			return rareHit_;
		}
		set
		{
			rareHit_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int TotalCount
	{
		get
		{
			return totalCount_;
		}
		set
		{
			totalCount_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<int> RelicList => relicList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public OperateSubjectBarInfo()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public OperateSubjectBarInfo(OperateSubjectBarInfo other)
		: this()
	{
		rareHit_ = other.rareHit_;
		totalCount_ = other.totalCount_;
		relicList_ = other.relicList_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public OperateSubjectBarInfo Clone()
	{
		return new OperateSubjectBarInfo(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as OperateSubjectBarInfo);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(OperateSubjectBarInfo other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (RareHit != other.RareHit)
		{
			return false;
		}
		if (TotalCount != other.TotalCount)
		{
			return false;
		}
		if (!relicList_.Equals(other.relicList_))
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
		if (RareHit != 0)
		{
			num ^= RareHit.GetHashCode();
		}
		if (TotalCount != 0)
		{
			num ^= TotalCount.GetHashCode();
		}
		num ^= ((object)relicList_).GetHashCode();
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
		if (RareHit != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteInt32(RareHit);
		}
		if (TotalCount != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteInt32(TotalCount);
		}
		relicList_.WriteTo(ref output, _repeated_relicList_codec);
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
		if (RareHit != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(RareHit);
		}
		if (TotalCount != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(TotalCount);
		}
		num += relicList_.CalculateSize(_repeated_relicList_codec);
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(OperateSubjectBarInfo other)
	{
		if (other != null)
		{
			if (other.RareHit != 0)
			{
				RareHit = other.RareHit;
			}
			if (other.TotalCount != 0)
			{
				TotalCount = other.TotalCount;
			}
			relicList_.Add((IEnumerable<int>)other.relicList_);
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
				RareHit = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 16u:
				TotalCount = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 24u:
			case 26u:
				relicList_.AddEntriesFrom(ref input, _repeated_relicList_codec);
				break;
			}
		}
	}
}
