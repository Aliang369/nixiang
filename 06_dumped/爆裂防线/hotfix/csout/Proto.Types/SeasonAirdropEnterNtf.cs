using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class SeasonAirdropEnterNtf : IMessage<SeasonAirdropEnterNtf>, IMessage, IEquatable<SeasonAirdropEnterNtf>, IDeepCloneable<SeasonAirdropEnterNtf>, IBufferMessage
{
	private static readonly MessageParser<SeasonAirdropEnterNtf> _parser = new MessageParser<SeasonAirdropEnterNtf>((Func<SeasonAirdropEnterNtf>)(() => new SeasonAirdropEnterNtf()));

	private UnknownFieldSet _unknownFields;

	public const int DropIdFieldNumber = 1;

	private int dropId_;

	public const int BattleUuidFieldNumber = 2;

	private string battleUuid_ = "";

	public const int BattleInfoFieldNumber = 3;

	private BattleInfo battleInfo_;

	public const int RoleFieldNumber = 4;

	private int role_;

	public const int TargetIdFieldNumber = 5;

	private long targetId_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<SeasonAirdropEnterNtf> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeSeasonAirdropEnterNtfReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int DropId
	{
		get
		{
			return dropId_;
		}
		set
		{
			dropId_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public string BattleUuid
	{
		get
		{
			return battleUuid_;
		}
		set
		{
			battleUuid_ = ProtoPreconditions.CheckNotNull<string>(value, "value");
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public BattleInfo BattleInfo
	{
		get
		{
			return battleInfo_;
		}
		set
		{
			battleInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int Role
	{
		get
		{
			return role_;
		}
		set
		{
			role_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public long TargetId
	{
		get
		{
			return targetId_;
		}
		set
		{
			targetId_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SeasonAirdropEnterNtf()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SeasonAirdropEnterNtf(SeasonAirdropEnterNtf other)
		: this()
	{
		dropId_ = other.dropId_;
		battleUuid_ = other.battleUuid_;
		battleInfo_ = ((other.battleInfo_ != null) ? other.battleInfo_.Clone() : null);
		role_ = other.role_;
		targetId_ = other.targetId_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SeasonAirdropEnterNtf Clone()
	{
		return new SeasonAirdropEnterNtf(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as SeasonAirdropEnterNtf);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(SeasonAirdropEnterNtf other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (DropId != other.DropId)
		{
			return false;
		}
		if (BattleUuid != other.BattleUuid)
		{
			return false;
		}
		if (!object.Equals(BattleInfo, other.BattleInfo))
		{
			return false;
		}
		if (Role != other.Role)
		{
			return false;
		}
		if (TargetId != other.TargetId)
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
		if (DropId != 0)
		{
			num ^= DropId.GetHashCode();
		}
		if (BattleUuid.Length != 0)
		{
			num ^= BattleUuid.GetHashCode();
		}
		if (battleInfo_ != null)
		{
			num ^= BattleInfo.GetHashCode();
		}
		if (Role != 0)
		{
			num ^= Role.GetHashCode();
		}
		if (TargetId != 0L)
		{
			num ^= TargetId.GetHashCode();
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
		if (DropId != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteInt32(DropId);
		}
		if (BattleUuid.Length != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)18);
			((WriteContext)(ref output)).WriteString(BattleUuid);
		}
		if (battleInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)26);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)BattleInfo);
		}
		if (Role != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)32);
			((WriteContext)(ref output)).WriteInt32(Role);
		}
		if (TargetId != 0L)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)40);
			((WriteContext)(ref output)).WriteInt64(TargetId);
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
		if (DropId != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(DropId);
		}
		if (BattleUuid.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(BattleUuid);
		}
		if (battleInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)BattleInfo);
		}
		if (Role != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(Role);
		}
		if (TargetId != 0L)
		{
			num += 1 + CodedOutputStream.ComputeInt64Size(TargetId);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(SeasonAirdropEnterNtf other)
	{
		if (other == null)
		{
			return;
		}
		if (other.DropId != 0)
		{
			DropId = other.DropId;
		}
		if (other.BattleUuid.Length != 0)
		{
			BattleUuid = other.BattleUuid;
		}
		if (other.battleInfo_ != null)
		{
			if (battleInfo_ == null)
			{
				BattleInfo = new BattleInfo();
			}
			BattleInfo.MergeFrom(other.BattleInfo);
		}
		if (other.Role != 0)
		{
			Role = other.Role;
		}
		if (other.TargetId != 0L)
		{
			TargetId = other.TargetId;
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
				DropId = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 18u:
				BattleUuid = ((ParseContext)(ref input)).ReadString();
				break;
			case 26u:
				if (battleInfo_ == null)
				{
					BattleInfo = new BattleInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)BattleInfo);
				break;
			case 32u:
				Role = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 40u:
				TargetId = ((ParseContext)(ref input)).ReadInt64();
				break;
			}
		}
	}
}
