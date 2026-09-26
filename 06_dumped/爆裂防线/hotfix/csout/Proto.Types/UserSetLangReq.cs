using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class UserSetLangReq : IMessage<UserSetLangReq>, IMessage, IEquatable<UserSetLangReq>, IDeepCloneable<UserSetLangReq>, IBufferMessage
{
	private static readonly MessageParser<UserSetLangReq> _parser = new MessageParser<UserSetLangReq>((Func<UserSetLangReq>)(() => new UserSetLangReq()));

	private UnknownFieldSet _unknownFields;

	public const int GuidFieldNumber = 1;

	private long guid_;

	public const int LangFieldNumber = 2;

	private ELanguageType lang_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<UserSetLangReq> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeUserSetLangReqReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public long Guid
	{
		get
		{
			return guid_;
		}
		set
		{
			guid_ = value;
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
	public UserSetLangReq()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public UserSetLangReq(UserSetLangReq other)
		: this()
	{
		guid_ = other.guid_;
		lang_ = other.lang_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public UserSetLangReq Clone()
	{
		return new UserSetLangReq(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as UserSetLangReq);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(UserSetLangReq other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (Guid != other.Guid)
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
		if (Guid != 0L)
		{
			num ^= Guid.GetHashCode();
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
		if (Guid != 0L)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteInt64(Guid);
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
		if (Guid != 0L)
		{
			num += 1 + CodedOutputStream.ComputeInt64Size(Guid);
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
	public void MergeFrom(UserSetLangReq other)
	{
		if (other != null)
		{
			if (other.Guid != 0L)
			{
				Guid = other.Guid;
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
			case 8u:
				Guid = ((ParseContext)(ref input)).ReadInt64();
				break;
			case 16u:
				Lang = (ELanguageType)((ParseContext)(ref input)).ReadEnum();
				break;
			}
		}
	}
}
