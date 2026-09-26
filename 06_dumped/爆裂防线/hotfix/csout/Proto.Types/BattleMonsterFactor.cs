using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class BattleMonsterFactor : IMessage<BattleMonsterFactor>, IMessage, IEquatable<BattleMonsterFactor>, IDeepCloneable<BattleMonsterFactor>, IBufferMessage
{
	private static readonly MessageParser<BattleMonsterFactor> _parser = new MessageParser<BattleMonsterFactor>((Func<BattleMonsterFactor>)(() => new BattleMonsterFactor()));

	private UnknownFieldSet _unknownFields;

	public const int DamageFactorFieldNumber = 1;

	private float damageFactor_;

	public const int HpFactorFieldNumber = 2;

	private float hpFactor_;

	public const int DiffFactorFieldNumber = 3;

	private float diffFactor_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<BattleMonsterFactor> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeBattleMonsterFactorReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public float DamageFactor
	{
		get
		{
			return damageFactor_;
		}
		set
		{
			damageFactor_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public float HpFactor
	{
		get
		{
			return hpFactor_;
		}
		set
		{
			hpFactor_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public float DiffFactor
	{
		get
		{
			return diffFactor_;
		}
		set
		{
			diffFactor_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public BattleMonsterFactor()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public BattleMonsterFactor(BattleMonsterFactor other)
		: this()
	{
		damageFactor_ = other.damageFactor_;
		hpFactor_ = other.hpFactor_;
		diffFactor_ = other.diffFactor_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public BattleMonsterFactor Clone()
	{
		return new BattleMonsterFactor(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as BattleMonsterFactor);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(BattleMonsterFactor other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!ProtobufEqualityComparers.BitwiseSingleEqualityComparer.Equals(DamageFactor, other.DamageFactor))
		{
			return false;
		}
		if (!ProtobufEqualityComparers.BitwiseSingleEqualityComparer.Equals(HpFactor, other.HpFactor))
		{
			return false;
		}
		if (!ProtobufEqualityComparers.BitwiseSingleEqualityComparer.Equals(DiffFactor, other.DiffFactor))
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
		if (DamageFactor != 0f)
		{
			num ^= ProtobufEqualityComparers.BitwiseSingleEqualityComparer.GetHashCode(DamageFactor);
		}
		if (HpFactor != 0f)
		{
			num ^= ProtobufEqualityComparers.BitwiseSingleEqualityComparer.GetHashCode(HpFactor);
		}
		if (DiffFactor != 0f)
		{
			num ^= ProtobufEqualityComparers.BitwiseSingleEqualityComparer.GetHashCode(DiffFactor);
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
		if (DamageFactor != 0f)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)13);
			((WriteContext)(ref output)).WriteFloat(DamageFactor);
		}
		if (HpFactor != 0f)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)21);
			((WriteContext)(ref output)).WriteFloat(HpFactor);
		}
		if (DiffFactor != 0f)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)29);
			((WriteContext)(ref output)).WriteFloat(DiffFactor);
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
		if (DamageFactor != 0f)
		{
			num += 5;
		}
		if (HpFactor != 0f)
		{
			num += 5;
		}
		if (DiffFactor != 0f)
		{
			num += 5;
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(BattleMonsterFactor other)
	{
		if (other != null)
		{
			if (other.DamageFactor != 0f)
			{
				DamageFactor = other.DamageFactor;
			}
			if (other.HpFactor != 0f)
			{
				HpFactor = other.HpFactor;
			}
			if (other.DiffFactor != 0f)
			{
				DiffFactor = other.DiffFactor;
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
			case 13u:
				DamageFactor = ((ParseContext)(ref input)).ReadFloat();
				break;
			case 21u:
				HpFactor = ((ParseContext)(ref input)).ReadFloat();
				break;
			case 29u:
				DiffFactor = ((ParseContext)(ref input)).ReadFloat();
				break;
			}
		}
	}
}
