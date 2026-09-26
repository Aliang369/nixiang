using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class BattleResultDamageWatchInfo : IMessage<BattleResultDamageWatchInfo>, IMessage, IEquatable<BattleResultDamageWatchInfo>, IDeepCloneable<BattleResultDamageWatchInfo>, IBufferMessage
{
	private static readonly MessageParser<BattleResultDamageWatchInfo> _parser = new MessageParser<BattleResultDamageWatchInfo>((Func<BattleResultDamageWatchInfo>)(() => new BattleResultDamageWatchInfo()));

	private UnknownFieldSet _unknownFields;

	public const int DamageInitFieldNumber = 1;

	private int damageInit_;

	public const int DamageFinalFieldNumber = 2;

	private int damageFinal_;

	public const int HurtMinFieldNumber = 3;

	private long hurtMin_;

	public const int HurtMaxFieldNumber = 4;

	private long hurtMax_;

	public const int MonsterIdFieldNumber = 5;

	private int monsterId_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<BattleResultDamageWatchInfo> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeBattleResultDamageWatchInfoReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int DamageInit
	{
		get
		{
			return damageInit_;
		}
		set
		{
			damageInit_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int DamageFinal
	{
		get
		{
			return damageFinal_;
		}
		set
		{
			damageFinal_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public long HurtMin
	{
		get
		{
			return hurtMin_;
		}
		set
		{
			hurtMin_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public long HurtMax
	{
		get
		{
			return hurtMax_;
		}
		set
		{
			hurtMax_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int MonsterId
	{
		get
		{
			return monsterId_;
		}
		set
		{
			monsterId_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public BattleResultDamageWatchInfo()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public BattleResultDamageWatchInfo(BattleResultDamageWatchInfo other)
		: this()
	{
		damageInit_ = other.damageInit_;
		damageFinal_ = other.damageFinal_;
		hurtMin_ = other.hurtMin_;
		hurtMax_ = other.hurtMax_;
		monsterId_ = other.monsterId_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public BattleResultDamageWatchInfo Clone()
	{
		return new BattleResultDamageWatchInfo(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as BattleResultDamageWatchInfo);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(BattleResultDamageWatchInfo other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (DamageInit != other.DamageInit)
		{
			return false;
		}
		if (DamageFinal != other.DamageFinal)
		{
			return false;
		}
		if (HurtMin != other.HurtMin)
		{
			return false;
		}
		if (HurtMax != other.HurtMax)
		{
			return false;
		}
		if (MonsterId != other.MonsterId)
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
		if (DamageInit != 0)
		{
			num ^= DamageInit.GetHashCode();
		}
		if (DamageFinal != 0)
		{
			num ^= DamageFinal.GetHashCode();
		}
		if (HurtMin != 0L)
		{
			num ^= HurtMin.GetHashCode();
		}
		if (HurtMax != 0L)
		{
			num ^= HurtMax.GetHashCode();
		}
		if (MonsterId != 0)
		{
			num ^= MonsterId.GetHashCode();
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
		if (DamageInit != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteInt32(DamageInit);
		}
		if (DamageFinal != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteInt32(DamageFinal);
		}
		if (HurtMin != 0L)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)24);
			((WriteContext)(ref output)).WriteInt64(HurtMin);
		}
		if (HurtMax != 0L)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)32);
			((WriteContext)(ref output)).WriteInt64(HurtMax);
		}
		if (MonsterId != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)40);
			((WriteContext)(ref output)).WriteInt32(MonsterId);
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
		if (DamageInit != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(DamageInit);
		}
		if (DamageFinal != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(DamageFinal);
		}
		if (HurtMin != 0L)
		{
			num += 1 + CodedOutputStream.ComputeInt64Size(HurtMin);
		}
		if (HurtMax != 0L)
		{
			num += 1 + CodedOutputStream.ComputeInt64Size(HurtMax);
		}
		if (MonsterId != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(MonsterId);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(BattleResultDamageWatchInfo other)
	{
		if (other != null)
		{
			if (other.DamageInit != 0)
			{
				DamageInit = other.DamageInit;
			}
			if (other.DamageFinal != 0)
			{
				DamageFinal = other.DamageFinal;
			}
			if (other.HurtMin != 0L)
			{
				HurtMin = other.HurtMin;
			}
			if (other.HurtMax != 0L)
			{
				HurtMax = other.HurtMax;
			}
			if (other.MonsterId != 0)
			{
				MonsterId = other.MonsterId;
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
				DamageInit = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 16u:
				DamageFinal = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 24u:
				HurtMin = ((ParseContext)(ref input)).ReadInt64();
				break;
			case 32u:
				HurtMax = ((ParseContext)(ref input)).ReadInt64();
				break;
			case 40u:
				MonsterId = ((ParseContext)(ref input)).ReadInt32();
				break;
			}
		}
	}
}
