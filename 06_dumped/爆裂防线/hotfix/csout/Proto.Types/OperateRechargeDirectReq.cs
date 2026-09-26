using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class OperateRechargeDirectReq : IMessage<OperateRechargeDirectReq>, IMessage, IEquatable<OperateRechargeDirectReq>, IDeepCloneable<OperateRechargeDirectReq>, IBufferMessage
{
	private static readonly MessageParser<OperateRechargeDirectReq> _parser = new MessageParser<OperateRechargeDirectReq>((Func<OperateRechargeDirectReq>)(() => new OperateRechargeDirectReq()));

	private UnknownFieldSet _unknownFields;

	public const int UUIDFieldNumber = 1;

	private string uUID_ = "";

	public const int DirectIdFieldNumber = 2;

	private int directId_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<OperateRechargeDirectReq> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeOperateRechargeDirectReqReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public string UUID
	{
		get
		{
			return uUID_;
		}
		set
		{
			uUID_ = ProtoPreconditions.CheckNotNull<string>(value, "value");
		}
	}

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
	public OperateRechargeDirectReq()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public OperateRechargeDirectReq(OperateRechargeDirectReq other)
		: this()
	{
		uUID_ = other.uUID_;
		directId_ = other.directId_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public OperateRechargeDirectReq Clone()
	{
		return new OperateRechargeDirectReq(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as OperateRechargeDirectReq);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(OperateRechargeDirectReq other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (UUID != other.UUID)
		{
			return false;
		}
		if (DirectId != other.DirectId)
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
		if (UUID.Length != 0)
		{
			num ^= UUID.GetHashCode();
		}
		if (DirectId != 0)
		{
			num ^= DirectId.GetHashCode();
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
		if (UUID.Length != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)10);
			((WriteContext)(ref output)).WriteString(UUID);
		}
		if (DirectId != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteInt32(DirectId);
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
		if (UUID.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(UUID);
		}
		if (DirectId != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(DirectId);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(OperateRechargeDirectReq other)
	{
		if (other != null)
		{
			if (other.UUID.Length != 0)
			{
				UUID = other.UUID;
			}
			if (other.DirectId != 0)
			{
				DirectId = other.DirectId;
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
			case 10u:
				UUID = ((ParseContext)(ref input)).ReadString();
				break;
			case 16u:
				DirectId = ((ParseContext)(ref input)).ReadInt32();
				break;
			}
		}
	}
}
