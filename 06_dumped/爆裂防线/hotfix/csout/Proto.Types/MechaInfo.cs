using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class MechaInfo : IMessage<MechaInfo>, IMessage, IEquatable<MechaInfo>, IDeepCloneable<MechaInfo>, IBufferMessage
{
	private static readonly MessageParser<MechaInfo> _parser = new MessageParser<MechaInfo>((Func<MechaInfo>)(() => new MechaInfo()));

	private UnknownFieldSet _unknownFields;

	public const int MechaListFieldNumber = 1;

	private static readonly FieldCodec<MechaDetail> _repeated_mechaList_codec = FieldCodec.ForMessage<MechaDetail>(10u, MechaDetail.Parser);

	private readonly RepeatedField<MechaDetail> mechaList_ = new RepeatedField<MechaDetail>();

	public const int TotalAttackFieldNumber = 2;

	private int totalAttack_;

	public const int EnergyFieldNumber = 3;

	private int energy_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<MechaInfo> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeMechaInfoReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<MechaDetail> MechaList => mechaList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int TotalAttack
	{
		get
		{
			return totalAttack_;
		}
		set
		{
			totalAttack_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int Energy
	{
		get
		{
			return energy_;
		}
		set
		{
			energy_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public MechaInfo()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public MechaInfo(MechaInfo other)
		: this()
	{
		mechaList_ = other.mechaList_.Clone();
		totalAttack_ = other.totalAttack_;
		energy_ = other.energy_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public MechaInfo Clone()
	{
		return new MechaInfo(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as MechaInfo);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(MechaInfo other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!mechaList_.Equals(other.mechaList_))
		{
			return false;
		}
		if (TotalAttack != other.TotalAttack)
		{
			return false;
		}
		if (Energy != other.Energy)
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
		num ^= ((object)mechaList_).GetHashCode();
		if (TotalAttack != 0)
		{
			num ^= TotalAttack.GetHashCode();
		}
		if (Energy != 0)
		{
			num ^= Energy.GetHashCode();
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
		mechaList_.WriteTo(ref output, _repeated_mechaList_codec);
		if (TotalAttack != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteInt32(TotalAttack);
		}
		if (Energy != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)24);
			((WriteContext)(ref output)).WriteInt32(Energy);
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
		num += mechaList_.CalculateSize(_repeated_mechaList_codec);
		if (TotalAttack != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(TotalAttack);
		}
		if (Energy != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(Energy);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(MechaInfo other)
	{
		if (other != null)
		{
			mechaList_.Add((IEnumerable<MechaDetail>)other.mechaList_);
			if (other.TotalAttack != 0)
			{
				TotalAttack = other.TotalAttack;
			}
			if (other.Energy != 0)
			{
				Energy = other.Energy;
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
				mechaList_.AddEntriesFrom(ref input, _repeated_mechaList_codec);
				break;
			case 16u:
				TotalAttack = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 24u:
				Energy = ((ParseContext)(ref input)).ReadInt32();
				break;
			}
		}
	}
}
