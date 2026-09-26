using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class OperateBossSubTaskAwardReq : IMessage<OperateBossSubTaskAwardReq>, IMessage, IEquatable<OperateBossSubTaskAwardReq>, IDeepCloneable<OperateBossSubTaskAwardReq>, IBufferMessage
{
	private static readonly MessageParser<OperateBossSubTaskAwardReq> _parser = new MessageParser<OperateBossSubTaskAwardReq>((Func<OperateBossSubTaskAwardReq>)(() => new OperateBossSubTaskAwardReq()));

	private UnknownFieldSet _unknownFields;

	public const int UUIDFieldNumber = 1;

	private string uUID_ = "";

	public const int TaskTypeFieldNumber = 2;

	private ETaskType taskType_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<OperateBossSubTaskAwardReq> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeOperateBossSubTaskAwardReqReflection.Descriptor.MessageTypes[0];

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
	public ETaskType TaskType
	{
		get
		{
			return taskType_;
		}
		set
		{
			taskType_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public OperateBossSubTaskAwardReq()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public OperateBossSubTaskAwardReq(OperateBossSubTaskAwardReq other)
		: this()
	{
		uUID_ = other.uUID_;
		taskType_ = other.taskType_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public OperateBossSubTaskAwardReq Clone()
	{
		return new OperateBossSubTaskAwardReq(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as OperateBossSubTaskAwardReq);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(OperateBossSubTaskAwardReq other)
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
		if (TaskType != other.TaskType)
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
		if (TaskType != ETaskType.EmptyValue)
		{
			num ^= TaskType.GetHashCode();
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
		if (TaskType != ETaskType.EmptyValue)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteEnum((int)TaskType);
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
		if (TaskType != ETaskType.EmptyValue)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)TaskType);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(OperateBossSubTaskAwardReq other)
	{
		if (other != null)
		{
			if (other.UUID.Length != 0)
			{
				UUID = other.UUID;
			}
			if (other.TaskType != ETaskType.EmptyValue)
			{
				TaskType = other.TaskType;
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
				TaskType = (ETaskType)((ParseContext)(ref input)).ReadEnum();
				break;
			}
		}
	}
}
