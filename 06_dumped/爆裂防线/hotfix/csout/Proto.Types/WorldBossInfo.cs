using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class WorldBossInfo : IMessage<WorldBossInfo>, IMessage, IEquatable<WorldBossInfo>, IDeepCloneable<WorldBossInfo>, IBufferMessage
{
	private static readonly MessageParser<WorldBossInfo> _parser = new MessageParser<WorldBossInfo>((Func<WorldBossInfo>)(() => new WorldBossInfo()));

	private UnknownFieldSet _unknownFields;

	public const int UUIDFieldNumber = 1;

	private int uUID_;

	public const int BossIdFieldNumber = 2;

	private int bossId_;

	public const int TotalHurtFieldNumber = 3;

	private long totalHurt_;

	public const int PassHurtFieldNumber = 4;

	private long passHurt_;

	public const int TargetListFieldNumber = 5;

	private static readonly FieldCodec<long> _repeated_targetList_codec = FieldCodec.ForInt64(42u);

	private readonly RepeatedField<long> targetList_ = new RepeatedField<long>();

	public const int PassStateFieldNumber = 6;

	private EBoolState passState_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<WorldBossInfo> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeWorldBossInfoReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int UUID
	{
		get
		{
			return uUID_;
		}
		set
		{
			uUID_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int BossId
	{
		get
		{
			return bossId_;
		}
		set
		{
			bossId_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public long TotalHurt
	{
		get
		{
			return totalHurt_;
		}
		set
		{
			totalHurt_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public long PassHurt
	{
		get
		{
			return passHurt_;
		}
		set
		{
			passHurt_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<long> TargetList => targetList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public EBoolState PassState
	{
		get
		{
			return passState_;
		}
		set
		{
			passState_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public WorldBossInfo()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public WorldBossInfo(WorldBossInfo other)
		: this()
	{
		uUID_ = other.uUID_;
		bossId_ = other.bossId_;
		totalHurt_ = other.totalHurt_;
		passHurt_ = other.passHurt_;
		targetList_ = other.targetList_.Clone();
		passState_ = other.passState_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public WorldBossInfo Clone()
	{
		return new WorldBossInfo(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as WorldBossInfo);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(WorldBossInfo other)
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
		if (BossId != other.BossId)
		{
			return false;
		}
		if (TotalHurt != other.TotalHurt)
		{
			return false;
		}
		if (PassHurt != other.PassHurt)
		{
			return false;
		}
		if (!targetList_.Equals(other.targetList_))
		{
			return false;
		}
		if (PassState != other.PassState)
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
		if (UUID != 0)
		{
			num ^= UUID.GetHashCode();
		}
		if (BossId != 0)
		{
			num ^= BossId.GetHashCode();
		}
		if (TotalHurt != 0L)
		{
			num ^= TotalHurt.GetHashCode();
		}
		if (PassHurt != 0L)
		{
			num ^= PassHurt.GetHashCode();
		}
		num ^= ((object)targetList_).GetHashCode();
		if (PassState != EBoolState.False)
		{
			num ^= PassState.GetHashCode();
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
		if (UUID != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteInt32(UUID);
		}
		if (BossId != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteInt32(BossId);
		}
		if (TotalHurt != 0L)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)24);
			((WriteContext)(ref output)).WriteInt64(TotalHurt);
		}
		if (PassHurt != 0L)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)32);
			((WriteContext)(ref output)).WriteInt64(PassHurt);
		}
		targetList_.WriteTo(ref output, _repeated_targetList_codec);
		if (PassState != EBoolState.False)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)48);
			((WriteContext)(ref output)).WriteEnum((int)PassState);
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
		if (UUID != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(UUID);
		}
		if (BossId != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(BossId);
		}
		if (TotalHurt != 0L)
		{
			num += 1 + CodedOutputStream.ComputeInt64Size(TotalHurt);
		}
		if (PassHurt != 0L)
		{
			num += 1 + CodedOutputStream.ComputeInt64Size(PassHurt);
		}
		num += targetList_.CalculateSize(_repeated_targetList_codec);
		if (PassState != EBoolState.False)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)PassState);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(WorldBossInfo other)
	{
		if (other != null)
		{
			if (other.UUID != 0)
			{
				UUID = other.UUID;
			}
			if (other.BossId != 0)
			{
				BossId = other.BossId;
			}
			if (other.TotalHurt != 0L)
			{
				TotalHurt = other.TotalHurt;
			}
			if (other.PassHurt != 0L)
			{
				PassHurt = other.PassHurt;
			}
			targetList_.Add((IEnumerable<long>)other.targetList_);
			if (other.PassState != EBoolState.False)
			{
				PassState = other.PassState;
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
				UUID = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 16u:
				BossId = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 24u:
				TotalHurt = ((ParseContext)(ref input)).ReadInt64();
				break;
			case 32u:
				PassHurt = ((ParseContext)(ref input)).ReadInt64();
				break;
			case 40u:
			case 42u:
				targetList_.AddEntriesFrom(ref input, _repeated_targetList_codec);
				break;
			case 48u:
				PassState = (EBoolState)((ParseContext)(ref input)).ReadEnum();
				break;
			}
		}
	}
}
