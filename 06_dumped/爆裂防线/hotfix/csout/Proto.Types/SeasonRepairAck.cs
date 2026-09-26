using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class SeasonRepairAck : IMessage<SeasonRepairAck>, IMessage, IEquatable<SeasonRepairAck>, IDeepCloneable<SeasonRepairAck>, IBufferMessage
{
	private static readonly MessageParser<SeasonRepairAck> _parser = new MessageParser<SeasonRepairAck>((Func<SeasonRepairAck>)(() => new SeasonRepairAck()));

	private UnknownFieldSet _unknownFields;

	public const int TeamIdFieldNumber = 1;

	private int teamId_;

	public const int DurabilityFieldNumber = 2;

	private int durability_;

	public const int RepairEndTmFieldNumber = 3;

	private long repairEndTm_;

	public const int GasLeftFieldNumber = 4;

	private int gasLeft_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<SeasonRepairAck> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeSeasonRepairAckReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int TeamId
	{
		get
		{
			return teamId_;
		}
		set
		{
			teamId_ = value;
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
	public long RepairEndTm
	{
		get
		{
			return repairEndTm_;
		}
		set
		{
			repairEndTm_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int GasLeft
	{
		get
		{
			return gasLeft_;
		}
		set
		{
			gasLeft_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SeasonRepairAck()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SeasonRepairAck(SeasonRepairAck other)
		: this()
	{
		teamId_ = other.teamId_;
		durability_ = other.durability_;
		repairEndTm_ = other.repairEndTm_;
		gasLeft_ = other.gasLeft_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SeasonRepairAck Clone()
	{
		return new SeasonRepairAck(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as SeasonRepairAck);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(SeasonRepairAck other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (TeamId != other.TeamId)
		{
			return false;
		}
		if (Durability != other.Durability)
		{
			return false;
		}
		if (RepairEndTm != other.RepairEndTm)
		{
			return false;
		}
		if (GasLeft != other.GasLeft)
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
		if (TeamId != 0)
		{
			num ^= TeamId.GetHashCode();
		}
		if (Durability != 0)
		{
			num ^= Durability.GetHashCode();
		}
		if (RepairEndTm != 0L)
		{
			num ^= RepairEndTm.GetHashCode();
		}
		if (GasLeft != 0)
		{
			num ^= GasLeft.GetHashCode();
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
		if (TeamId != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteInt32(TeamId);
		}
		if (Durability != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteInt32(Durability);
		}
		if (RepairEndTm != 0L)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)24);
			((WriteContext)(ref output)).WriteInt64(RepairEndTm);
		}
		if (GasLeft != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)32);
			((WriteContext)(ref output)).WriteInt32(GasLeft);
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
		if (TeamId != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(TeamId);
		}
		if (Durability != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(Durability);
		}
		if (RepairEndTm != 0L)
		{
			num += 1 + CodedOutputStream.ComputeInt64Size(RepairEndTm);
		}
		if (GasLeft != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(GasLeft);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(SeasonRepairAck other)
	{
		if (other != null)
		{
			if (other.TeamId != 0)
			{
				TeamId = other.TeamId;
			}
			if (other.Durability != 0)
			{
				Durability = other.Durability;
			}
			if (other.RepairEndTm != 0L)
			{
				RepairEndTm = other.RepairEndTm;
			}
			if (other.GasLeft != 0)
			{
				GasLeft = other.GasLeft;
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
				TeamId = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 16u:
				Durability = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 24u:
				RepairEndTm = ((ParseContext)(ref input)).ReadInt64();
				break;
			case 32u:
				GasLeft = ((ParseContext)(ref input)).ReadInt32();
				break;
			}
		}
	}
}
