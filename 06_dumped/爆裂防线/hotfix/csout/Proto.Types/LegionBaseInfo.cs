using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class LegionBaseInfo : IMessage<LegionBaseInfo>, IMessage, IEquatable<LegionBaseInfo>, IDeepCloneable<LegionBaseInfo>, IBufferMessage
{
	private static readonly MessageParser<LegionBaseInfo> _parser = new MessageParser<LegionBaseInfo>((Func<LegionBaseInfo>)(() => new LegionBaseInfo()));

	private UnknownFieldSet _unknownFields;

	public const int LegionIdFieldNumber = 1;

	private int legionId_;

	public const int LegionNameFieldNumber = 2;

	private string legionName_ = "";

	public const int IconFieldNumber = 3;

	private int icon_;

	public const int LevelFieldNumber = 4;

	private int level_;

	public const int MemberCountFieldNumber = 5;

	private int memberCount_;

	public const int MemberMaxFieldNumber = 6;

	private int memberMax_;

	public const int JoinLvlFieldNumber = 7;

	private int joinLvl_;

	public const int ConfirmFieldNumber = 8;

	private EBoolState confirm_;

	public const int ActiveFieldNumber = 9;

	private int active_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<LegionBaseInfo> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeLegionBaseInfoReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int LegionId
	{
		get
		{
			return legionId_;
		}
		set
		{
			legionId_ = value;
		}
	}

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
	public int Level
	{
		get
		{
			return level_;
		}
		set
		{
			level_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int MemberCount
	{
		get
		{
			return memberCount_;
		}
		set
		{
			memberCount_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int MemberMax
	{
		get
		{
			return memberMax_;
		}
		set
		{
			memberMax_ = value;
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
	public int Active
	{
		get
		{
			return active_;
		}
		set
		{
			active_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public LegionBaseInfo()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public LegionBaseInfo(LegionBaseInfo other)
		: this()
	{
		legionId_ = other.legionId_;
		legionName_ = other.legionName_;
		icon_ = other.icon_;
		level_ = other.level_;
		memberCount_ = other.memberCount_;
		memberMax_ = other.memberMax_;
		joinLvl_ = other.joinLvl_;
		confirm_ = other.confirm_;
		active_ = other.active_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public LegionBaseInfo Clone()
	{
		return new LegionBaseInfo(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as LegionBaseInfo);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(LegionBaseInfo other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (LegionId != other.LegionId)
		{
			return false;
		}
		if (LegionName != other.LegionName)
		{
			return false;
		}
		if (Icon != other.Icon)
		{
			return false;
		}
		if (Level != other.Level)
		{
			return false;
		}
		if (MemberCount != other.MemberCount)
		{
			return false;
		}
		if (MemberMax != other.MemberMax)
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
		if (Active != other.Active)
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
		if (LegionId != 0)
		{
			num ^= LegionId.GetHashCode();
		}
		if (LegionName.Length != 0)
		{
			num ^= LegionName.GetHashCode();
		}
		if (Icon != 0)
		{
			num ^= Icon.GetHashCode();
		}
		if (Level != 0)
		{
			num ^= Level.GetHashCode();
		}
		if (MemberCount != 0)
		{
			num ^= MemberCount.GetHashCode();
		}
		if (MemberMax != 0)
		{
			num ^= MemberMax.GetHashCode();
		}
		if (JoinLvl != 0)
		{
			num ^= JoinLvl.GetHashCode();
		}
		if (Confirm != EBoolState.False)
		{
			num ^= Confirm.GetHashCode();
		}
		if (Active != 0)
		{
			num ^= Active.GetHashCode();
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
		if (LegionId != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteInt32(LegionId);
		}
		if (LegionName.Length != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)18);
			((WriteContext)(ref output)).WriteString(LegionName);
		}
		if (Icon != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)24);
			((WriteContext)(ref output)).WriteInt32(Icon);
		}
		if (Level != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)32);
			((WriteContext)(ref output)).WriteInt32(Level);
		}
		if (MemberCount != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)40);
			((WriteContext)(ref output)).WriteInt32(MemberCount);
		}
		if (MemberMax != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)48);
			((WriteContext)(ref output)).WriteInt32(MemberMax);
		}
		if (JoinLvl != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)56);
			((WriteContext)(ref output)).WriteInt32(JoinLvl);
		}
		if (Confirm != EBoolState.False)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)64);
			((WriteContext)(ref output)).WriteEnum((int)Confirm);
		}
		if (Active != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)72);
			((WriteContext)(ref output)).WriteInt32(Active);
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
		if (LegionId != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(LegionId);
		}
		if (LegionName.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(LegionName);
		}
		if (Icon != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(Icon);
		}
		if (Level != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(Level);
		}
		if (MemberCount != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(MemberCount);
		}
		if (MemberMax != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(MemberMax);
		}
		if (JoinLvl != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(JoinLvl);
		}
		if (Confirm != EBoolState.False)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)Confirm);
		}
		if (Active != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(Active);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(LegionBaseInfo other)
	{
		if (other != null)
		{
			if (other.LegionId != 0)
			{
				LegionId = other.LegionId;
			}
			if (other.LegionName.Length != 0)
			{
				LegionName = other.LegionName;
			}
			if (other.Icon != 0)
			{
				Icon = other.Icon;
			}
			if (other.Level != 0)
			{
				Level = other.Level;
			}
			if (other.MemberCount != 0)
			{
				MemberCount = other.MemberCount;
			}
			if (other.MemberMax != 0)
			{
				MemberMax = other.MemberMax;
			}
			if (other.JoinLvl != 0)
			{
				JoinLvl = other.JoinLvl;
			}
			if (other.Confirm != EBoolState.False)
			{
				Confirm = other.Confirm;
			}
			if (other.Active != 0)
			{
				Active = other.Active;
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
				LegionId = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 18u:
				LegionName = ((ParseContext)(ref input)).ReadString();
				break;
			case 24u:
				Icon = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 32u:
				Level = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 40u:
				MemberCount = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 48u:
				MemberMax = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 56u:
				JoinLvl = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 64u:
				Confirm = (EBoolState)((ParseContext)(ref input)).ReadEnum();
				break;
			case 72u:
				Active = ((ParseContext)(ref input)).ReadInt32();
				break;
			}
		}
	}
}
