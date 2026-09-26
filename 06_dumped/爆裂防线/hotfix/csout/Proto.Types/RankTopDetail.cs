using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class RankTopDetail : IMessage<RankTopDetail>, IMessage, IEquatable<RankTopDetail>, IDeepCloneable<RankTopDetail>, IBufferMessage
{
	private static readonly MessageParser<RankTopDetail> _parser = new MessageParser<RankTopDetail>((Func<RankTopDetail>)(() => new RankTopDetail()));

	private UnknownFieldSet _unknownFields;

	public const int RankTypeFieldNumber = 1;

	private ERankType rankType_;

	public const int RankInfoFieldNumber = 2;

	private RankDetail rankInfo_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<RankTopDetail> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeRankTopDetailReflection.Descriptor.MessageTypes[0];

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
	public RankDetail RankInfo
	{
		get
		{
			return rankInfo_;
		}
		set
		{
			rankInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RankTopDetail()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RankTopDetail(RankTopDetail other)
		: this()
	{
		rankType_ = other.rankType_;
		rankInfo_ = ((other.rankInfo_ != null) ? other.rankInfo_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RankTopDetail Clone()
	{
		return new RankTopDetail(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as RankTopDetail);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(RankTopDetail other)
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
		if (!object.Equals(RankInfo, other.RankInfo))
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
		if (rankInfo_ != null)
		{
			num ^= RankInfo.GetHashCode();
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
		if (rankInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)18);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)RankInfo);
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
		if (rankInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)RankInfo);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(RankTopDetail other)
	{
		if (other == null)
		{
			return;
		}
		if (other.RankType != ERankType.EmptyValue)
		{
			RankType = other.RankType;
		}
		if (other.rankInfo_ != null)
		{
			if (rankInfo_ == null)
			{
				RankInfo = new RankDetail();
			}
			RankInfo.MergeFrom(other.RankInfo);
		}
		_unknownFields = UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
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
			case 18u:
				if (rankInfo_ == null)
				{
					RankInfo = new RankDetail();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)RankInfo);
				break;
			}
		}
	}
}
