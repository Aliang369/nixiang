using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class WorldBossHurtRecordDetail : IMessage<WorldBossHurtRecordDetail>, IMessage, IEquatable<WorldBossHurtRecordDetail>, IDeepCloneable<WorldBossHurtRecordDetail>, IBufferMessage
{
	private static readonly MessageParser<WorldBossHurtRecordDetail> _parser = new MessageParser<WorldBossHurtRecordDetail>((Func<WorldBossHurtRecordDetail>)(() => new WorldBossHurtRecordDetail()));

	private UnknownFieldSet _unknownFields;

	public const int RoleBaseInfoFieldNumber = 1;

	private RoleBaseInfo roleBaseInfo_;

	public const int BattleResultFieldNumber = 2;

	private BattleResultInfo battleResult_;

	public const int HurtFieldNumber = 3;

	private long hurt_;

	public const int TimeFieldNumber = 4;

	private long time_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<WorldBossHurtRecordDetail> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeWorldBossHurtRecordDetailReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RoleBaseInfo RoleBaseInfo
	{
		get
		{
			return roleBaseInfo_;
		}
		set
		{
			roleBaseInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public BattleResultInfo BattleResult
	{
		get
		{
			return battleResult_;
		}
		set
		{
			battleResult_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public long Hurt
	{
		get
		{
			return hurt_;
		}
		set
		{
			hurt_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public long Time
	{
		get
		{
			return time_;
		}
		set
		{
			time_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public WorldBossHurtRecordDetail()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public WorldBossHurtRecordDetail(WorldBossHurtRecordDetail other)
		: this()
	{
		roleBaseInfo_ = ((other.roleBaseInfo_ != null) ? other.roleBaseInfo_.Clone() : null);
		battleResult_ = ((other.battleResult_ != null) ? other.battleResult_.Clone() : null);
		hurt_ = other.hurt_;
		time_ = other.time_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public WorldBossHurtRecordDetail Clone()
	{
		return new WorldBossHurtRecordDetail(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as WorldBossHurtRecordDetail);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(WorldBossHurtRecordDetail other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!object.Equals(RoleBaseInfo, other.RoleBaseInfo))
		{
			return false;
		}
		if (!object.Equals(BattleResult, other.BattleResult))
		{
			return false;
		}
		if (Hurt != other.Hurt)
		{
			return false;
		}
		if (Time != other.Time)
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
		if (roleBaseInfo_ != null)
		{
			num ^= RoleBaseInfo.GetHashCode();
		}
		if (battleResult_ != null)
		{
			num ^= BattleResult.GetHashCode();
		}
		if (Hurt != 0L)
		{
			num ^= Hurt.GetHashCode();
		}
		if (Time != 0L)
		{
			num ^= Time.GetHashCode();
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
		if (roleBaseInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)10);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)RoleBaseInfo);
		}
		if (battleResult_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)18);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)BattleResult);
		}
		if (Hurt != 0L)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)24);
			((WriteContext)(ref output)).WriteInt64(Hurt);
		}
		if (Time != 0L)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)32);
			((WriteContext)(ref output)).WriteInt64(Time);
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
		if (roleBaseInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)RoleBaseInfo);
		}
		if (battleResult_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)BattleResult);
		}
		if (Hurt != 0L)
		{
			num += 1 + CodedOutputStream.ComputeInt64Size(Hurt);
		}
		if (Time != 0L)
		{
			num += 1 + CodedOutputStream.ComputeInt64Size(Time);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(WorldBossHurtRecordDetail other)
	{
		if (other == null)
		{
			return;
		}
		if (other.roleBaseInfo_ != null)
		{
			if (roleBaseInfo_ == null)
			{
				RoleBaseInfo = new RoleBaseInfo();
			}
			RoleBaseInfo.MergeFrom(other.RoleBaseInfo);
		}
		if (other.battleResult_ != null)
		{
			if (battleResult_ == null)
			{
				BattleResult = new BattleResultInfo();
			}
			BattleResult.MergeFrom(other.BattleResult);
		}
		if (other.Hurt != 0L)
		{
			Hurt = other.Hurt;
		}
		if (other.Time != 0L)
		{
			Time = other.Time;
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
			case 10u:
				if (roleBaseInfo_ == null)
				{
					RoleBaseInfo = new RoleBaseInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)RoleBaseInfo);
				break;
			case 18u:
				if (battleResult_ == null)
				{
					BattleResult = new BattleResultInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)BattleResult);
				break;
			case 24u:
				Hurt = ((ParseContext)(ref input)).ReadInt64();
				break;
			case 32u:
				Time = ((ParseContext)(ref input)).ReadInt64();
				break;
			}
		}
	}
}
