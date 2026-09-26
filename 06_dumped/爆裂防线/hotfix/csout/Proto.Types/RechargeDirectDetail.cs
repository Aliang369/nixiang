using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class RechargeDirectDetail : IMessage<RechargeDirectDetail>, IMessage, IEquatable<RechargeDirectDetail>, IDeepCloneable<RechargeDirectDetail>, IBufferMessage
{
	private static readonly MessageParser<RechargeDirectDetail> _parser = new MessageParser<RechargeDirectDetail>((Func<RechargeDirectDetail>)(() => new RechargeDirectDetail()));

	private UnknownFieldSet _unknownFields;

	public const int DirectIdFieldNumber = 1;

	private int directId_;

	public const int LimitNumFieldNumber = 2;

	private int limitNum_;

	public const int LimitVipFieldNumber = 3;

	private int limitVip_;

	public const int BoughtCountFieldNumber = 4;

	private int boughtCount_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<RechargeDirectDetail> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeRechargeDirectDetailReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int DirectId
	{
		get
		{
			return directId_;
		}
		set
		{
			directId_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int LimitNum
	{
		get
		{
			return limitNum_;
		}
		set
		{
			limitNum_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int LimitVip
	{
		get
		{
			return limitVip_;
		}
		set
		{
			limitVip_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int BoughtCount
	{
		get
		{
			return boughtCount_;
		}
		set
		{
			boughtCount_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RechargeDirectDetail()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RechargeDirectDetail(RechargeDirectDetail other)
		: this()
	{
		directId_ = other.directId_;
		limitNum_ = other.limitNum_;
		limitVip_ = other.limitVip_;
		boughtCount_ = other.boughtCount_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RechargeDirectDetail Clone()
	{
		return new RechargeDirectDetail(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as RechargeDirectDetail);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(RechargeDirectDetail other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (DirectId != other.DirectId)
		{
			return false;
		}
		if (LimitNum != other.LimitNum)
		{
			return false;
		}
		if (LimitVip != other.LimitVip)
		{
			return false;
		}
		if (BoughtCount != other.BoughtCount)
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
		if (DirectId != 0)
		{
			num ^= DirectId.GetHashCode();
		}
		if (LimitNum != 0)
		{
			num ^= LimitNum.GetHashCode();
		}
		if (LimitVip != 0)
		{
			num ^= LimitVip.GetHashCode();
		}
		if (BoughtCount != 0)
		{
			num ^= BoughtCount.GetHashCode();
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
		if (DirectId != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteInt32(DirectId);
		}
		if (LimitNum != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteInt32(LimitNum);
		}
		if (LimitVip != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)24);
			((WriteContext)(ref output)).WriteInt32(LimitVip);
		}
		if (BoughtCount != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)32);
			((WriteContext)(ref output)).WriteInt32(BoughtCount);
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
		if (DirectId != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(DirectId);
		}
		if (LimitNum != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(LimitNum);
		}
		if (LimitVip != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(LimitVip);
		}
		if (BoughtCount != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(BoughtCount);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(RechargeDirectDetail other)
	{
		if (other != null)
		{
			if (other.DirectId != 0)
			{
				DirectId = other.DirectId;
			}
			if (other.LimitNum != 0)
			{
				LimitNum = other.LimitNum;
			}
			if (other.LimitVip != 0)
			{
				LimitVip = other.LimitVip;
			}
			if (other.BoughtCount != 0)
			{
				BoughtCount = other.BoughtCount;
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
				DirectId = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 16u:
				LimitNum = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 24u:
				LimitVip = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 32u:
				BoughtCount = ((ParseContext)(ref input)).ReadInt32();
				break;
			}
		}
	}
}
