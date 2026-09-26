using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class AuxResourceQuickBuyReq : IMessage<AuxResourceQuickBuyReq>, IMessage, IEquatable<AuxResourceQuickBuyReq>, IDeepCloneable<AuxResourceQuickBuyReq>, IBufferMessage
{
	private static readonly MessageParser<AuxResourceQuickBuyReq> _parser = new MessageParser<AuxResourceQuickBuyReq>((Func<AuxResourceQuickBuyReq>)(() => new AuxResourceQuickBuyReq()));

	private UnknownFieldSet _unknownFields;

	public const int TypeFieldNumber = 1;

	private EResourceType type_;

	public const int TidFieldNumber = 2;

	private long tid_;

	public const int GroupCountFieldNumber = 3;

	private int groupCount_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<AuxResourceQuickBuyReq> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeAuxResourceQuickBuyReqReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public EResourceType Type
	{
		get
		{
			return type_;
		}
		set
		{
			type_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public long Tid
	{
		get
		{
			return tid_;
		}
		set
		{
			tid_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int GroupCount
	{
		get
		{
			return groupCount_;
		}
		set
		{
			groupCount_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public AuxResourceQuickBuyReq()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public AuxResourceQuickBuyReq(AuxResourceQuickBuyReq other)
		: this()
	{
		type_ = other.type_;
		tid_ = other.tid_;
		groupCount_ = other.groupCount_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public AuxResourceQuickBuyReq Clone()
	{
		return new AuxResourceQuickBuyReq(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as AuxResourceQuickBuyReq);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(AuxResourceQuickBuyReq other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (Type != other.Type)
		{
			return false;
		}
		if (Tid != other.Tid)
		{
			return false;
		}
		if (GroupCount != other.GroupCount)
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
		if (Type != EResourceType.EmptyValue)
		{
			num ^= Type.GetHashCode();
		}
		if (Tid != 0L)
		{
			num ^= Tid.GetHashCode();
		}
		if (GroupCount != 0)
		{
			num ^= GroupCount.GetHashCode();
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
		if (Type != EResourceType.EmptyValue)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteEnum((int)Type);
		}
		if (Tid != 0L)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteInt64(Tid);
		}
		if (GroupCount != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)24);
			((WriteContext)(ref output)).WriteInt32(GroupCount);
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
		if (Type != EResourceType.EmptyValue)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)Type);
		}
		if (Tid != 0L)
		{
			num += 1 + CodedOutputStream.ComputeInt64Size(Tid);
		}
		if (GroupCount != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(GroupCount);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(AuxResourceQuickBuyReq other)
	{
		if (other != null)
		{
			if (other.Type != EResourceType.EmptyValue)
			{
				Type = other.Type;
			}
			if (other.Tid != 0L)
			{
				Tid = other.Tid;
			}
			if (other.GroupCount != 0)
			{
				GroupCount = other.GroupCount;
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
				Type = (EResourceType)((ParseContext)(ref input)).ReadEnum();
				break;
			case 16u:
				Tid = ((ParseContext)(ref input)).ReadInt64();
				break;
			case 24u:
				GroupCount = ((ParseContext)(ref input)).ReadInt32();
				break;
			}
		}
	}
}
