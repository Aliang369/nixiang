using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class OpsRankBaseDetail : IMessage<OpsRankBaseDetail>, IMessage, IEquatable<OpsRankBaseDetail>, IDeepCloneable<OpsRankBaseDetail>, IBufferMessage
{
	private static readonly MessageParser<OpsRankBaseDetail> _parser = new MessageParser<OpsRankBaseDetail>((Func<OpsRankBaseDetail>)(() => new OpsRankBaseDetail()));

	private UnknownFieldSet _unknownFields;

	public const int RankTypeFieldNumber = 1;

	private ERankType rankType_;

	public const int SettleTmFieldNumber = 2;

	private long settleTm_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<OpsRankBaseDetail> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeOpsRankBaseDetailReflection.Descriptor.MessageTypes[0];

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
	public long SettleTm
	{
		get
		{
			return settleTm_;
		}
		set
		{
			settleTm_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public OpsRankBaseDetail()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public OpsRankBaseDetail(OpsRankBaseDetail other)
		: this()
	{
		rankType_ = other.rankType_;
		settleTm_ = other.settleTm_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public OpsRankBaseDetail Clone()
	{
		return new OpsRankBaseDetail(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as OpsRankBaseDetail);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(OpsRankBaseDetail other)
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
		if (SettleTm != other.SettleTm)
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
		if (SettleTm != 0L)
		{
			num ^= SettleTm.GetHashCode();
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
		if (SettleTm != 0L)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteInt64(SettleTm);
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
		if (SettleTm != 0L)
		{
			num += 1 + CodedOutputStream.ComputeInt64Size(SettleTm);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(OpsRankBaseDetail other)
	{
		if (other != null)
		{
			if (other.RankType != ERankType.EmptyValue)
			{
				RankType = other.RankType;
			}
			if (other.SettleTm != 0L)
			{
				SettleTm = other.SettleTm;
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
				SettleTm = ((ParseContext)(ref input)).ReadInt64();
				break;
			}
		}
	}
}
