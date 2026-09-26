using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class EquipSlotEquipLockReq : IMessage<EquipSlotEquipLockReq>, IMessage, IEquatable<EquipSlotEquipLockReq>, IDeepCloneable<EquipSlotEquipLockReq>, IBufferMessage
{
	private static readonly MessageParser<EquipSlotEquipLockReq> _parser = new MessageParser<EquipSlotEquipLockReq>((Func<EquipSlotEquipLockReq>)(() => new EquipSlotEquipLockReq()));

	private UnknownFieldSet _unknownFields;

	public const int EquipTypeFieldNumber = 1;

	private EEquipType equipType_;

	public const int LockFieldNumber = 2;

	private EBoolState lock_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<EquipSlotEquipLockReq> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeEquipSlotEquipLockReqReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public EEquipType EquipType
	{
		get
		{
			return equipType_;
		}
		set
		{
			equipType_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public EBoolState Lock
	{
		get
		{
			return lock_;
		}
		set
		{
			lock_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public EquipSlotEquipLockReq()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public EquipSlotEquipLockReq(EquipSlotEquipLockReq other)
		: this()
	{
		equipType_ = other.equipType_;
		lock_ = other.lock_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public EquipSlotEquipLockReq Clone()
	{
		return new EquipSlotEquipLockReq(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as EquipSlotEquipLockReq);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(EquipSlotEquipLockReq other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (EquipType != other.EquipType)
		{
			return false;
		}
		if (Lock != other.Lock)
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
		if (EquipType != EEquipType.EmptyValue)
		{
			num ^= EquipType.GetHashCode();
		}
		if (Lock != EBoolState.False)
		{
			num ^= Lock.GetHashCode();
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
		if (EquipType != EEquipType.EmptyValue)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteEnum((int)EquipType);
		}
		if (Lock != EBoolState.False)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteEnum((int)Lock);
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
		if (EquipType != EEquipType.EmptyValue)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)EquipType);
		}
		if (Lock != EBoolState.False)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)Lock);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(EquipSlotEquipLockReq other)
	{
		if (other != null)
		{
			if (other.EquipType != EEquipType.EmptyValue)
			{
				EquipType = other.EquipType;
			}
			if (other.Lock != EBoolState.False)
			{
				Lock = other.Lock;
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
				EquipType = (EEquipType)((ParseContext)(ref input)).ReadEnum();
				break;
			case 16u:
				Lock = (EBoolState)((ParseContext)(ref input)).ReadEnum();
				break;
			}
		}
	}
}
