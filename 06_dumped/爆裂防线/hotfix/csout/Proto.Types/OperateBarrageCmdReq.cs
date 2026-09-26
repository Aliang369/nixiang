using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class OperateBarrageCmdReq : IMessage<OperateBarrageCmdReq>, IMessage, IEquatable<OperateBarrageCmdReq>, IDeepCloneable<OperateBarrageCmdReq>, IBufferMessage
{
	private static readonly MessageParser<OperateBarrageCmdReq> _parser = new MessageParser<OperateBarrageCmdReq>((Func<OperateBarrageCmdReq>)(() => new OperateBarrageCmdReq()));

	private UnknownFieldSet _unknownFields;

	public const int UUIDFieldNumber = 1;

	private string uUID_ = "";

	public const int LangFieldNumber = 2;

	private ELanguageType lang_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<OperateBarrageCmdReq> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeOperateBarrageCmdReqReflection.Descriptor.MessageTypes[0];

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
	public ELanguageType Lang
	{
		get
		{
			return lang_;
		}
		set
		{
			lang_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public OperateBarrageCmdReq()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public OperateBarrageCmdReq(OperateBarrageCmdReq other)
		: this()
	{
		uUID_ = other.uUID_;
		lang_ = other.lang_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public OperateBarrageCmdReq Clone()
	{
		return new OperateBarrageCmdReq(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as OperateBarrageCmdReq);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(OperateBarrageCmdReq other)
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
		if (Lang != other.Lang)
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
		if (Lang != ELanguageType.EmptyValue)
		{
			num ^= Lang.GetHashCode();
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
		if (Lang != ELanguageType.EmptyValue)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteEnum((int)Lang);
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
		if (Lang != ELanguageType.EmptyValue)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)Lang);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(OperateBarrageCmdReq other)
	{
		if (other != null)
		{
			if (other.UUID.Length != 0)
			{
				UUID = other.UUID;
			}
			if (other.Lang != ELanguageType.EmptyValue)
			{
				Lang = other.Lang;
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
				Lang = (ELanguageType)((ParseContext)(ref input)).ReadEnum();
				break;
			}
		}
	}
}
