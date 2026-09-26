using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class LegionCreateReq : IMessage<LegionCreateReq>, IMessage, IEquatable<LegionCreateReq>, IDeepCloneable<LegionCreateReq>, IBufferMessage
{
	private static readonly MessageParser<LegionCreateReq> _parser = new MessageParser<LegionCreateReq>((Func<LegionCreateReq>)(() => new LegionCreateReq()));

	private UnknownFieldSet _unknownFields;

	public const int LegionNameFieldNumber = 1;

	private string legionName_ = "";

	public const int IconFieldNumber = 2;

	private int icon_;

	public const int JoinLvlFieldNumber = 3;

	private int joinLvl_;

	public const int ConfirmFieldNumber = 4;

	private EBoolState confirm_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<LegionCreateReq> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeLegionCreateReqReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public string LegionName
	{
		get
		{
			return legionName_;
		}
		set
		{
			legionName_ = ProtoPreconditions.CheckNotNull<string>(value, "value");
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int Icon
	{
		get
		{
			return icon_;
		}
		set
		{
			icon_ = value;
		}
	}

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
	public LegionCreateReq()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public LegionCreateReq(LegionCreateReq other)
		: this()
	{
		legionName_ = other.legionName_;
		icon_ = other.icon_;
		joinLvl_ = other.joinLvl_;
		confirm_ = other.confirm_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public LegionCreateReq Clone()
	{
		return new LegionCreateReq(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as LegionCreateReq);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(LegionCreateReq other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (LegionName != other.LegionName)
		{
			return false;
		}
		if (Icon != other.Icon)
		{
			return false;
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
		if (LegionName.Length != 0)
		{
			num ^= LegionName.GetHashCode();
		}
		if (Icon != 0)
		{
			num ^= Icon.GetHashCode();
		}
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
		if (LegionName.Length != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)10);
			((WriteContext)(ref output)).WriteString(LegionName);
		}
		if (Icon != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteInt32(Icon);
		}
		if (JoinLvl != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)24);
			((WriteContext)(ref output)).WriteInt32(JoinLvl);
		}
		if (Confirm != EBoolState.False)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)32);
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
		if (LegionName.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(LegionName);
		}
		if (Icon != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(Icon);
		}
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
	public void MergeFrom(LegionCreateReq other)
	{
		if (other != null)
		{
			if (other.LegionName.Length != 0)
			{
				LegionName = other.LegionName;
			}
			if (other.Icon != 0)
			{
				Icon = other.Icon;
			}
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
			case 10u:
				LegionName = ((ParseContext)(ref input)).ReadString();
				break;
			case 16u:
				Icon = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 24u:
				JoinLvl = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 32u:
				Confirm = (EBoolState)((ParseContext)(ref input)).ReadEnum();
				break;
			}
		}
	}
}
