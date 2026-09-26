using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class TranslateBaiduReq : IMessage<TranslateBaiduReq>, IMessage, IEquatable<TranslateBaiduReq>, IDeepCloneable<TranslateBaiduReq>, IBufferMessage
{
	private static readonly MessageParser<TranslateBaiduReq> _parser = new MessageParser<TranslateBaiduReq>((Func<TranslateBaiduReq>)(() => new TranslateBaiduReq()));

	private UnknownFieldSet _unknownFields;

	public const int QueryFieldNumber = 1;

	private string query_ = "";

	public const int FromFieldNumber = 2;

	private ETranslateType from_;

	public const int ToFieldNumber = 3;

	private ETranslateType to_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<TranslateBaiduReq> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeTranslateBaiduReqReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public string Query
	{
		get
		{
			return query_;
		}
		set
		{
			query_ = ProtoPreconditions.CheckNotNull<string>(value, "value");
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ETranslateType From
	{
		get
		{
			return from_;
		}
		set
		{
			from_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ETranslateType To
	{
		get
		{
			return to_;
		}
		set
		{
			to_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public TranslateBaiduReq()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public TranslateBaiduReq(TranslateBaiduReq other)
		: this()
	{
		query_ = other.query_;
		from_ = other.from_;
		to_ = other.to_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public TranslateBaiduReq Clone()
	{
		return new TranslateBaiduReq(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as TranslateBaiduReq);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(TranslateBaiduReq other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (Query != other.Query)
		{
			return false;
		}
		if (From != other.From)
		{
			return false;
		}
		if (To != other.To)
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
		if (Query.Length != 0)
		{
			num ^= Query.GetHashCode();
		}
		if (From != ETranslateType.EmptyValue)
		{
			num ^= From.GetHashCode();
		}
		if (To != ETranslateType.EmptyValue)
		{
			num ^= To.GetHashCode();
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
		if (Query.Length != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)10);
			((WriteContext)(ref output)).WriteString(Query);
		}
		if (From != ETranslateType.EmptyValue)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteEnum((int)From);
		}
		if (To != ETranslateType.EmptyValue)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)24);
			((WriteContext)(ref output)).WriteEnum((int)To);
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
		if (Query.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(Query);
		}
		if (From != ETranslateType.EmptyValue)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)From);
		}
		if (To != ETranslateType.EmptyValue)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)To);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(TranslateBaiduReq other)
	{
		if (other != null)
		{
			if (other.Query.Length != 0)
			{
				Query = other.Query;
			}
			if (other.From != ETranslateType.EmptyValue)
			{
				From = other.From;
			}
			if (other.To != ETranslateType.EmptyValue)
			{
				To = other.To;
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
				Query = ((ParseContext)(ref input)).ReadString();
				break;
			case 16u:
				From = (ETranslateType)((ParseContext)(ref input)).ReadEnum();
				break;
			case 24u:
				To = (ETranslateType)((ParseContext)(ref input)).ReadEnum();
				break;
			}
		}
	}
}
