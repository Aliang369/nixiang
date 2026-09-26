using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class LegionJoinSetReq : IMessage<LegionJoinSetReq>, IMessage, IEquatable<LegionJoinSetReq>, IDeepCloneable<LegionJoinSetReq>, IBufferMessage
{
	private static readonly MessageParser<LegionJoinSetReq> _parser = new MessageParser<LegionJoinSetReq>((Func<LegionJoinSetReq>)(() => new LegionJoinSetReq()));

	private UnknownFieldSet _unknownFields;

	public const int JoinLvlFieldNumber = 1;

	private int joinLvl_;

	public const int ConfirmFieldNumber = 2;

	private EBoolState confirm_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<LegionJoinSetReq> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeLegionJoinSetReqReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int JoinLvl
	{
		get
		{
			return joinLvl_;
		}
		set
		{
			joinLvl_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public EBoolState Confirm
	{
		get
		{
			return confirm_;
		}
		set
		{
			confirm_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public LegionJoinSetReq()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public LegionJoinSetReq(LegionJoinSetReq other)
		: this()
	{
		joinLvl_ = other.joinLvl_;
		confirm_ = other.confirm_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public LegionJoinSetReq Clone()
	{
		return new LegionJoinSetReq(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as LegionJoinSetReq);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(LegionJoinSetReq other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (JoinLvl != other.JoinLvl)
		{
			return false;
		}
		if (Confirm != other.Confirm)
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
		if (JoinLvl != 0)
		{
			num ^= JoinLvl.GetHashCode();
		}
		if (Confirm != EBoolState.False)
		{
			num ^= Confirm.GetHashCode();
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
		if (JoinLvl != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteInt32(JoinLvl);
		}
		if (Confirm != EBoolState.False)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteEnum((int)Confirm);
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
		if (JoinLvl != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(JoinLvl);
		}
		if (Confirm != EBoolState.False)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)Confirm);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(LegionJoinSetReq other)
	{
		if (other != null)
		{
			if (other.JoinLvl != 0)
			{
				JoinLvl = other.JoinLvl;
			}
			if (other.Confirm != EBoolState.False)
			{
				Confirm = other.Confirm;
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
				JoinLvl = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 16u:
				Confirm = (EBoolState)((ParseContext)(ref input)).ReadEnum();
				break;
			}
		}
	}
}
