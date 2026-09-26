using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class RankFirstReachAwardDetail : IMessage<RankFirstReachAwardDetail>, IMessage, IEquatable<RankFirstReachAwardDetail>, IDeepCloneable<RankFirstReachAwardDetail>, IBufferMessage
{
	private static readonly MessageParser<RankFirstReachAwardDetail> _parser = new MessageParser<RankFirstReachAwardDetail>((Func<RankFirstReachAwardDetail>)(() => new RankFirstReachAwardDetail()));

	private UnknownFieldSet _unknownFields;

	public const int RankTypeFieldNumber = 1;

	private ERankType rankType_;

	public const int RecordIdFieldNumber = 2;

	private int recordId_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<RankFirstReachAwardDetail> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeRankFirstReachAwardDetailReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ERankType RankType
	{
		get
		{
			return rankType_;
		}
		set
		{
			rankType_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int RecordId
	{
		get
		{
			return recordId_;
		}
		set
		{
			recordId_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RankFirstReachAwardDetail()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RankFirstReachAwardDetail(RankFirstReachAwardDetail other)
		: this()
	{
		rankType_ = other.rankType_;
		recordId_ = other.recordId_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RankFirstReachAwardDetail Clone()
	{
		return new RankFirstReachAwardDetail(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as RankFirstReachAwardDetail);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(RankFirstReachAwardDetail other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (RankType != other.RankType)
		{
			return false;
		}
		if (RecordId != other.RecordId)
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
		if (RankType != ERankType.EmptyValue)
		{
			num ^= RankType.GetHashCode();
		}
		if (RecordId != 0)
		{
			num ^= RecordId.GetHashCode();
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
		if (RankType != ERankType.EmptyValue)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteEnum((int)RankType);
		}
		if (RecordId != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteInt32(RecordId);
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
		if (RankType != ERankType.EmptyValue)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)RankType);
		}
		if (RecordId != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(RecordId);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(RankFirstReachAwardDetail other)
	{
		if (other != null)
		{
			if (other.RankType != ERankType.EmptyValue)
			{
				RankType = other.RankType;
			}
			if (other.RecordId != 0)
			{
				RecordId = other.RecordId;
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
				RankType = (ERankType)((ParseContext)(ref input)).ReadEnum();
				break;
			case 16u:
				RecordId = ((ParseContext)(ref input)).ReadInt32();
				break;
			}
		}
	}
}
