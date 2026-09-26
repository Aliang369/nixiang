using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class ChargebackInfo : IMessage<ChargebackInfo>, IMessage, IEquatable<ChargebackInfo>, IDeepCloneable<ChargebackInfo>, IBufferMessage
{
	private static readonly MessageParser<ChargebackInfo> _parser = new MessageParser<ChargebackInfo>((Func<ChargebackInfo>)(() => new ChargebackInfo()));

	private UnknownFieldSet _unknownFields;

	public const int StateFieldNumber = 1;

	private EChargebackState state_;

	public const int VoidCountFieldNumber = 2;

	private float voidCount_;

	public const int VoidAmountFieldNumber = 3;

	private float voidAmount_;

	public const int RepaymentFieldNumber = 4;

	private float repayment_;

	public const int ExpireTmFieldNumber = 5;

	private long expireTm_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<ChargebackInfo> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeChargebackInfoReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public EChargebackState State
	{
		get
		{
			return state_;
		}
		set
		{
			state_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public float VoidCount
	{
		get
		{
			return voidCount_;
		}
		set
		{
			voidCount_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public float VoidAmount
	{
		get
		{
			return voidAmount_;
		}
		set
		{
			voidAmount_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public float Repayment
	{
		get
		{
			return repayment_;
		}
		set
		{
			repayment_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public long ExpireTm
	{
		get
		{
			return expireTm_;
		}
		set
		{
			expireTm_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ChargebackInfo()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ChargebackInfo(ChargebackInfo other)
		: this()
	{
		state_ = other.state_;
		voidCount_ = other.voidCount_;
		voidAmount_ = other.voidAmount_;
		repayment_ = other.repayment_;
		expireTm_ = other.expireTm_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ChargebackInfo Clone()
	{
		return new ChargebackInfo(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as ChargebackInfo);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(ChargebackInfo other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (State != other.State)
		{
			return false;
		}
		if (!ProtobufEqualityComparers.BitwiseSingleEqualityComparer.Equals(VoidCount, other.VoidCount))
		{
			return false;
		}
		if (!ProtobufEqualityComparers.BitwiseSingleEqualityComparer.Equals(VoidAmount, other.VoidAmount))
		{
			return false;
		}
		if (!ProtobufEqualityComparers.BitwiseSingleEqualityComparer.Equals(Repayment, other.Repayment))
		{
			return false;
		}
		if (ExpireTm != other.ExpireTm)
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
		if (State != EChargebackState.None)
		{
			num ^= State.GetHashCode();
		}
		if (VoidCount != 0f)
		{
			num ^= ProtobufEqualityComparers.BitwiseSingleEqualityComparer.GetHashCode(VoidCount);
		}
		if (VoidAmount != 0f)
		{
			num ^= ProtobufEqualityComparers.BitwiseSingleEqualityComparer.GetHashCode(VoidAmount);
		}
		if (Repayment != 0f)
		{
			num ^= ProtobufEqualityComparers.BitwiseSingleEqualityComparer.GetHashCode(Repayment);
		}
		if (ExpireTm != 0L)
		{
			num ^= ExpireTm.GetHashCode();
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
		if (State != EChargebackState.None)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteEnum((int)State);
		}
		if (VoidCount != 0f)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)21);
			((WriteContext)(ref output)).WriteFloat(VoidCount);
		}
		if (VoidAmount != 0f)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)29);
			((WriteContext)(ref output)).WriteFloat(VoidAmount);
		}
		if (Repayment != 0f)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)37);
			((WriteContext)(ref output)).WriteFloat(Repayment);
		}
		if (ExpireTm != 0L)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)40);
			((WriteContext)(ref output)).WriteInt64(ExpireTm);
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
		if (State != EChargebackState.None)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)State);
		}
		if (VoidCount != 0f)
		{
			num += 5;
		}
		if (VoidAmount != 0f)
		{
			num += 5;
		}
		if (Repayment != 0f)
		{
			num += 5;
		}
		if (ExpireTm != 0L)
		{
			num += 1 + CodedOutputStream.ComputeInt64Size(ExpireTm);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(ChargebackInfo other)
	{
		if (other != null)
		{
			if (other.State != EChargebackState.None)
			{
				State = other.State;
			}
			if (other.VoidCount != 0f)
			{
				VoidCount = other.VoidCount;
			}
			if (other.VoidAmount != 0f)
			{
				VoidAmount = other.VoidAmount;
			}
			if (other.Repayment != 0f)
			{
				Repayment = other.Repayment;
			}
			if (other.ExpireTm != 0L)
			{
				ExpireTm = other.ExpireTm;
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
				State = (EChargebackState)((ParseContext)(ref input)).ReadEnum();
				break;
			case 21u:
				VoidCount = ((ParseContext)(ref input)).ReadFloat();
				break;
			case 29u:
				VoidAmount = ((ParseContext)(ref input)).ReadFloat();
				break;
			case 37u:
				Repayment = ((ParseContext)(ref input)).ReadFloat();
				break;
			case 40u:
				ExpireTm = ((ParseContext)(ref input)).ReadInt64();
				break;
			}
		}
	}
}
