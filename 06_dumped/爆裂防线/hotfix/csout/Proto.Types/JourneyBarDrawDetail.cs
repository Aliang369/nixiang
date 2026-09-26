using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class JourneyBarDrawDetail : IMessage<JourneyBarDrawDetail>, IMessage, IEquatable<JourneyBarDrawDetail>, IDeepCloneable<JourneyBarDrawDetail>, IBufferMessage
{
	private static readonly MessageParser<JourneyBarDrawDetail> _parser = new MessageParser<JourneyBarDrawDetail>((Func<JourneyBarDrawDetail>)(() => new JourneyBarDrawDetail()));

	private UnknownFieldSet _unknownFields;

	public const int RareHitFieldNumber = 1;

	private int rareHit_;

	public const int TotalCountFieldNumber = 2;

	private int totalCount_;

	public const int TargetListFieldNumber = 3;

	private static readonly FieldCodec<JourneyTargetRecordDetail> _repeated_targetList_codec = FieldCodec.ForMessage<JourneyTargetRecordDetail>(26u, JourneyTargetRecordDetail.Parser);

	private readonly RepeatedField<JourneyTargetRecordDetail> targetList_ = new RepeatedField<JourneyTargetRecordDetail>();

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<JourneyBarDrawDetail> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeJourneyBarDrawDetailReflection.Descriptor.MessageTypes[0];

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
	public RepeatedField<JourneyTargetRecordDetail> TargetList => targetList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public JourneyBarDrawDetail()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public JourneyBarDrawDetail(JourneyBarDrawDetail other)
		: this()
	{
		rareHit_ = other.rareHit_;
		totalCount_ = other.totalCount_;
		targetList_ = other.targetList_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public JourneyBarDrawDetail Clone()
	{
		return new JourneyBarDrawDetail(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as JourneyBarDrawDetail);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(JourneyBarDrawDetail other)
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
		if (!targetList_.Equals(other.targetList_))
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
		num ^= ((object)targetList_).GetHashCode();
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
		targetList_.WriteTo(ref output, _repeated_targetList_codec);
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
		num += targetList_.CalculateSize(_repeated_targetList_codec);
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(JourneyBarDrawDetail other)
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
			targetList_.Add((IEnumerable<JourneyTargetRecordDetail>)other.targetList_);
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
			case 26u:
				targetList_.AddEntriesFrom(ref input, _repeated_targetList_codec);
				break;
			}
		}
	}
}
