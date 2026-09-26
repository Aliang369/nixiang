using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class CrossLadderOpenInfo : IMessage<CrossLadderOpenInfo>, IMessage, IEquatable<CrossLadderOpenInfo>, IDeepCloneable<CrossLadderOpenInfo>, IBufferMessage
{
	private static readonly MessageParser<CrossLadderOpenInfo> _parser = new MessageParser<CrossLadderOpenInfo>((Func<CrossLadderOpenInfo>)(() => new CrossLadderOpenInfo()));

	private UnknownFieldSet _unknownFields;

	public const int GroupIdFieldNumber = 1;

	private int groupId_;

	public const int GroupRangeFieldNumber = 2;

	private static readonly FieldCodec<int> _repeated_groupRange_codec = FieldCodec.ForInt32(18u);

	private readonly RepeatedField<int> groupRange_ = new RepeatedField<int>();

	public const int StartTmFieldNumber = 3;

	private long startTm_;

	public const int EndTmFieldNumber = 4;

	private long endTm_;

	public const int MonthFieldNumber = 5;

	private int month_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<CrossLadderOpenInfo> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeCrossLadderOpenInfoReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int GroupId
	{
		get
		{
			return groupId_;
		}
		set
		{
			groupId_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<int> GroupRange => groupRange_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public long StartTm
	{
		get
		{
			return startTm_;
		}
		set
		{
			startTm_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public long EndTm
	{
		get
		{
			return endTm_;
		}
		set
		{
			endTm_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int Month
	{
		get
		{
			return month_;
		}
		set
		{
			month_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public CrossLadderOpenInfo()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public CrossLadderOpenInfo(CrossLadderOpenInfo other)
		: this()
	{
		groupId_ = other.groupId_;
		groupRange_ = other.groupRange_.Clone();
		startTm_ = other.startTm_;
		endTm_ = other.endTm_;
		month_ = other.month_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public CrossLadderOpenInfo Clone()
	{
		return new CrossLadderOpenInfo(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as CrossLadderOpenInfo);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(CrossLadderOpenInfo other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (GroupId != other.GroupId)
		{
			return false;
		}
		if (!groupRange_.Equals(other.groupRange_))
		{
			return false;
		}
		if (StartTm != other.StartTm)
		{
			return false;
		}
		if (EndTm != other.EndTm)
		{
			return false;
		}
		if (Month != other.Month)
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
		if (GroupId != 0)
		{
			num ^= GroupId.GetHashCode();
		}
		num ^= ((object)groupRange_).GetHashCode();
		if (StartTm != 0L)
		{
			num ^= StartTm.GetHashCode();
		}
		if (EndTm != 0L)
		{
			num ^= EndTm.GetHashCode();
		}
		if (Month != 0)
		{
			num ^= Month.GetHashCode();
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
		if (GroupId != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteInt32(GroupId);
		}
		groupRange_.WriteTo(ref output, _repeated_groupRange_codec);
		if (StartTm != 0L)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)24);
			((WriteContext)(ref output)).WriteInt64(StartTm);
		}
		if (EndTm != 0L)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)32);
			((WriteContext)(ref output)).WriteInt64(EndTm);
		}
		if (Month != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)40);
			((WriteContext)(ref output)).WriteInt32(Month);
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
		if (GroupId != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(GroupId);
		}
		num += groupRange_.CalculateSize(_repeated_groupRange_codec);
		if (StartTm != 0L)
		{
			num += 1 + CodedOutputStream.ComputeInt64Size(StartTm);
		}
		if (EndTm != 0L)
		{
			num += 1 + CodedOutputStream.ComputeInt64Size(EndTm);
		}
		if (Month != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(Month);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(CrossLadderOpenInfo other)
	{
		if (other != null)
		{
			if (other.GroupId != 0)
			{
				GroupId = other.GroupId;
			}
			groupRange_.Add((IEnumerable<int>)other.groupRange_);
			if (other.StartTm != 0L)
			{
				StartTm = other.StartTm;
			}
			if (other.EndTm != 0L)
			{
				EndTm = other.EndTm;
			}
			if (other.Month != 0)
			{
				Month = other.Month;
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
			case 8u:
				GroupId = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 16u:
			case 18u:
				groupRange_.AddEntriesFrom(ref input, _repeated_groupRange_codec);
				break;
			case 24u:
				StartTm = ((ParseContext)(ref input)).ReadInt64();
				break;
			case 32u:
				EndTm = ((ParseContext)(ref input)).ReadInt64();
				break;
			case 40u:
				Month = ((ParseContext)(ref input)).ReadInt32();
				break;
			}
		}
	}
}
