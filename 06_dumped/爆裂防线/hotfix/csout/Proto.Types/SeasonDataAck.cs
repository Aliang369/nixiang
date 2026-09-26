using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class SeasonDataAck : IMessage<SeasonDataAck>, IMessage, IEquatable<SeasonDataAck>, IDeepCloneable<SeasonDataAck>, IBufferMessage
{
	private static readonly MessageParser<SeasonDataAck> _parser = new MessageParser<SeasonDataAck>((Func<SeasonDataAck>)(() => new SeasonDataAck()));

	private UnknownFieldSet _unknownFields;

	public const int ForceIdFieldNumber = 1;

	private ESeasonForce forceId_;

	public const int MeritFieldNumber = 2;

	private int merit_;

	public const int DurabilityFieldNumber = 3;

	private int durability_;

	public const int DefDurabilityFieldNumber = 4;

	private int defDurability_;

	public const int GasFieldNumber = 5;

	private int gas_;

	public const int TechPointFieldNumber = 6;

	private int techPoint_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<SeasonDataAck> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeSeasonDataAckReflection.Descriptor.MessageTypes[0];

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
	public int Merit
	{
		get
		{
			return merit_;
		}
		set
		{
			merit_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int Durability
	{
		get
		{
			return durability_;
		}
		set
		{
			durability_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int DefDurability
	{
		get
		{
			return defDurability_;
		}
		set
		{
			defDurability_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int Gas
	{
		get
		{
			return gas_;
		}
		set
		{
			gas_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int TechPoint
	{
		get
		{
			return techPoint_;
		}
		set
		{
			techPoint_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SeasonDataAck()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SeasonDataAck(SeasonDataAck other)
		: this()
	{
		forceId_ = other.forceId_;
		merit_ = other.merit_;
		durability_ = other.durability_;
		defDurability_ = other.defDurability_;
		gas_ = other.gas_;
		techPoint_ = other.techPoint_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SeasonDataAck Clone()
	{
		return new SeasonDataAck(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as SeasonDataAck);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(SeasonDataAck other)
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
		if (Merit != other.Merit)
		{
			return false;
		}
		if (Durability != other.Durability)
		{
			return false;
		}
		if (DefDurability != other.DefDurability)
		{
			return false;
		}
		if (Gas != other.Gas)
		{
			return false;
		}
		if (TechPoint != other.TechPoint)
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
		if (Merit != 0)
		{
			num ^= Merit.GetHashCode();
		}
		if (Durability != 0)
		{
			num ^= Durability.GetHashCode();
		}
		if (DefDurability != 0)
		{
			num ^= DefDurability.GetHashCode();
		}
		if (Gas != 0)
		{
			num ^= Gas.GetHashCode();
		}
		if (TechPoint != 0)
		{
			num ^= TechPoint.GetHashCode();
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
		if (Merit != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteInt32(Merit);
		}
		if (Durability != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)24);
			((WriteContext)(ref output)).WriteInt32(Durability);
		}
		if (DefDurability != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)32);
			((WriteContext)(ref output)).WriteInt32(DefDurability);
		}
		if (Gas != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)40);
			((WriteContext)(ref output)).WriteInt32(Gas);
		}
		if (TechPoint != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)48);
			((WriteContext)(ref output)).WriteInt32(TechPoint);
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
		if (Merit != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(Merit);
		}
		if (Durability != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(Durability);
		}
		if (DefDurability != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(DefDurability);
		}
		if (Gas != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(Gas);
		}
		if (TechPoint != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(TechPoint);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(SeasonDataAck other)
	{
		if (other != null)
		{
			if (other.ForceId != ESeasonForce.ForceNeutral)
			{
				ForceId = other.ForceId;
			}
			if (other.Merit != 0)
			{
				Merit = other.Merit;
			}
			if (other.Durability != 0)
			{
				Durability = other.Durability;
			}
			if (other.DefDurability != 0)
			{
				DefDurability = other.DefDurability;
			}
			if (other.Gas != 0)
			{
				Gas = other.Gas;
			}
			if (other.TechPoint != 0)
			{
				TechPoint = other.TechPoint;
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
				ForceId = (ESeasonForce)((ParseContext)(ref input)).ReadEnum();
				break;
			case 16u:
				Merit = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 24u:
				Durability = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 32u:
				DefDurability = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 40u:
				Gas = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 48u:
				TechPoint = ((ParseContext)(ref input)).ReadInt32();
				break;
			}
		}
	}
}
