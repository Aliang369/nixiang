using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class DailySign : IMessage<DailySign>, IMessage, IEquatable<DailySign>, IDeepCloneable<DailySign>, IBufferMessage
{
	private static readonly MessageParser<DailySign> _parser = new MessageParser<DailySign>((Func<DailySign>)(() => new DailySign()));

	private UnknownFieldSet _unknownFields;

	public const int SignTotalFieldNumber = 1;

	private int signTotal_;

	public const int SignDayFieldNumber = 2;

	private int signDay_;

	public const int DailyStateFieldNumber = 3;

	private ECommonState dailyState_;

	public const int TargetAwardFieldNumber = 4;

	private DailySignTargetAwardDetail targetAward_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<DailySign> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeDailySignReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int SignTotal
	{
		get
		{
			return signTotal_;
		}
		set
		{
			signTotal_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int SignDay
	{
		get
		{
			return signDay_;
		}
		set
		{
			signDay_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ECommonState DailyState
	{
		get
		{
			return dailyState_;
		}
		set
		{
			dailyState_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public DailySignTargetAwardDetail TargetAward
	{
		get
		{
			return targetAward_;
		}
		set
		{
			targetAward_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public DailySign()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public DailySign(DailySign other)
		: this()
	{
		signTotal_ = other.signTotal_;
		signDay_ = other.signDay_;
		dailyState_ = other.dailyState_;
		targetAward_ = ((other.targetAward_ != null) ? other.targetAward_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public DailySign Clone()
	{
		return new DailySign(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as DailySign);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(DailySign other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (SignTotal != other.SignTotal)
		{
			return false;
		}
		if (SignDay != other.SignDay)
		{
			return false;
		}
		if (DailyState != other.DailyState)
		{
			return false;
		}
		if (!object.Equals(TargetAward, other.TargetAward))
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
		if (SignTotal != 0)
		{
			num ^= SignTotal.GetHashCode();
		}
		if (SignDay != 0)
		{
			num ^= SignDay.GetHashCode();
		}
		if (DailyState != ECommonState.No)
		{
			num ^= DailyState.GetHashCode();
		}
		if (targetAward_ != null)
		{
			num ^= TargetAward.GetHashCode();
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
		if (SignTotal != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteInt32(SignTotal);
		}
		if (SignDay != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteInt32(SignDay);
		}
		if (DailyState != ECommonState.No)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)24);
			((WriteContext)(ref output)).WriteEnum((int)DailyState);
		}
		if (targetAward_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)34);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)TargetAward);
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
		if (SignTotal != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(SignTotal);
		}
		if (SignDay != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(SignDay);
		}
		if (DailyState != ECommonState.No)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)DailyState);
		}
		if (targetAward_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)TargetAward);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(DailySign other)
	{
		if (other == null)
		{
			return;
		}
		if (other.SignTotal != 0)
		{
			SignTotal = other.SignTotal;
		}
		if (other.SignDay != 0)
		{
			SignDay = other.SignDay;
		}
		if (other.DailyState != ECommonState.No)
		{
			DailyState = other.DailyState;
		}
		if (other.targetAward_ != null)
		{
			if (targetAward_ == null)
			{
				TargetAward = new DailySignTargetAwardDetail();
			}
			TargetAward.MergeFrom(other.TargetAward);
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
				SignTotal = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 16u:
				SignDay = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 24u:
				DailyState = (ECommonState)((ParseContext)(ref input)).ReadEnum();
				break;
			case 34u:
				if (targetAward_ == null)
				{
					TargetAward = new DailySignTargetAwardDetail();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)TargetAward);
				break;
			}
		}
	}
}
