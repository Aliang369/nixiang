using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class SeasonForceInfo : IMessage<SeasonForceInfo>, IMessage, IEquatable<SeasonForceInfo>, IDeepCloneable<SeasonForceInfo>, IBufferMessage
{
	private static readonly MessageParser<SeasonForceInfo> _parser = new MessageParser<SeasonForceInfo>((Func<SeasonForceInfo>)(() => new SeasonForceInfo()));

	private UnknownFieldSet _unknownFields;

	public const int ForceIdFieldNumber = 1;

	private ESeasonForce forceId_;

	public const int NameFieldNumber = 2;

	private string name_ = "";

	public const int FlagFieldNumber = 3;

	private string flag_ = "";

	public const int NoticeFieldNumber = 4;

	private string notice_ = "";

	public const int LevelFieldNumber = 5;

	private int level_;

	public const int ExpFieldNumber = 6;

	private int exp_;

	public const int LeaderFieldNumber = 7;

	private RoleBaseInfo leader_;

	public const int MemberCountFieldNumber = 8;

	private int memberCount_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<SeasonForceInfo> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeSeasonForceInfoReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ESeasonForce ForceId
	{
		get
		{
			return forceId_;
		}
		set
		{
			forceId_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public string Name
	{
		get
		{
			return name_;
		}
		set
		{
			name_ = ProtoPreconditions.CheckNotNull<string>(value, "value");
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public string Flag
	{
		get
		{
			return flag_;
		}
		set
		{
			flag_ = ProtoPreconditions.CheckNotNull<string>(value, "value");
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public string Notice
	{
		get
		{
			return notice_;
		}
		set
		{
			notice_ = ProtoPreconditions.CheckNotNull<string>(value, "value");
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
	public int Exp
	{
		get
		{
			return exp_;
		}
		set
		{
			exp_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RoleBaseInfo Leader
	{
		get
		{
			return leader_;
		}
		set
		{
			leader_ = value;
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
	public SeasonForceInfo()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SeasonForceInfo(SeasonForceInfo other)
		: this()
	{
		forceId_ = other.forceId_;
		name_ = other.name_;
		flag_ = other.flag_;
		notice_ = other.notice_;
		level_ = other.level_;
		exp_ = other.exp_;
		leader_ = ((other.leader_ != null) ? other.leader_.Clone() : null);
		memberCount_ = other.memberCount_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SeasonForceInfo Clone()
	{
		return new SeasonForceInfo(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as SeasonForceInfo);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(SeasonForceInfo other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (ForceId != other.ForceId)
		{
			return false;
		}
		if (Name != other.Name)
		{
			return false;
		}
		if (Flag != other.Flag)
		{
			return false;
		}
		if (Notice != other.Notice)
		{
			return false;
		}
		if (Level != other.Level)
		{
			return false;
		}
		if (Exp != other.Exp)
		{
			return false;
		}
		if (!object.Equals(Leader, other.Leader))
		{
			return false;
		}
		if (MemberCount != other.MemberCount)
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
		if (ForceId != ESeasonForce.ForceNeutral)
		{
			num ^= ForceId.GetHashCode();
		}
		if (Name.Length != 0)
		{
			num ^= Name.GetHashCode();
		}
		if (Flag.Length != 0)
		{
			num ^= Flag.GetHashCode();
		}
		if (Notice.Length != 0)
		{
			num ^= Notice.GetHashCode();
		}
		if (Level != 0)
		{
			num ^= Level.GetHashCode();
		}
		if (Exp != 0)
		{
			num ^= Exp.GetHashCode();
		}
		if (leader_ != null)
		{
			num ^= Leader.GetHashCode();
		}
		if (MemberCount != 0)
		{
			num ^= MemberCount.GetHashCode();
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
		if (ForceId != ESeasonForce.ForceNeutral)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteEnum((int)ForceId);
		}
		if (Name.Length != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)18);
			((WriteContext)(ref output)).WriteString(Name);
		}
		if (Flag.Length != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)26);
			((WriteContext)(ref output)).WriteString(Flag);
		}
		if (Notice.Length != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)34);
			((WriteContext)(ref output)).WriteString(Notice);
		}
		if (Level != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)40);
			((WriteContext)(ref output)).WriteInt32(Level);
		}
		if (Exp != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)48);
			((WriteContext)(ref output)).WriteInt32(Exp);
		}
		if (leader_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)58);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)Leader);
		}
		if (MemberCount != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)64);
			((WriteContext)(ref output)).WriteInt32(MemberCount);
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
		if (ForceId != ESeasonForce.ForceNeutral)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)ForceId);
		}
		if (Name.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(Name);
		}
		if (Flag.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(Flag);
		}
		if (Notice.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(Notice);
		}
		if (Level != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(Level);
		}
		if (Exp != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(Exp);
		}
		if (leader_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)Leader);
		}
		if (MemberCount != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(MemberCount);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(SeasonForceInfo other)
	{
		if (other == null)
		{
			return;
		}
		if (other.ForceId != ESeasonForce.ForceNeutral)
		{
			ForceId = other.ForceId;
		}
		if (other.Name.Length != 0)
		{
			Name = other.Name;
		}
		if (other.Flag.Length != 0)
		{
			Flag = other.Flag;
		}
		if (other.Notice.Length != 0)
		{
			Notice = other.Notice;
		}
		if (other.Level != 0)
		{
			Level = other.Level;
		}
		if (other.Exp != 0)
		{
			Exp = other.Exp;
		}
		if (other.leader_ != null)
		{
			if (leader_ == null)
			{
				Leader = new RoleBaseInfo();
			}
			Leader.MergeFrom(other.Leader);
		}
		if (other.MemberCount != 0)
		{
			MemberCount = other.MemberCount;
		}
		_unknownFields = UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
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
				ForceId = (ESeasonForce)((ParseContext)(ref input)).ReadEnum();
				break;
			case 18u:
				Name = ((ParseContext)(ref input)).ReadString();
				break;
			case 26u:
				Flag = ((ParseContext)(ref input)).ReadString();
				break;
			case 34u:
				Notice = ((ParseContext)(ref input)).ReadString();
				break;
			case 40u:
				Level = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 48u:
				Exp = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 58u:
				if (leader_ == null)
				{
					Leader = new RoleBaseInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)Leader);
				break;
			case 64u:
				MemberCount = ((ParseContext)(ref input)).ReadInt32();
				break;
			}
		}
	}
}
