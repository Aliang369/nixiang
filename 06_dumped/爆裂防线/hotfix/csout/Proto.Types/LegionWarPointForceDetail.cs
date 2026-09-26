using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class LegionWarPointForceDetail : IMessage<LegionWarPointForceDetail>, IMessage, IEquatable<LegionWarPointForceDetail>, IDeepCloneable<LegionWarPointForceDetail>, IBufferMessage
{
	private static readonly MessageParser<LegionWarPointForceDetail> _parser = new MessageParser<LegionWarPointForceDetail>((Func<LegionWarPointForceDetail>)(() => new LegionWarPointForceDetail()));

	private UnknownFieldSet _unknownFields;

	public const int ForceIdFieldNumber = 1;

	private ELegionWarForce forceId_;

	public const int PlayerCountFieldNumber = 2;

	private int playerCount_;

	public const int CrowdedStateFieldNumber = 3;

	private ELegionWarCrowdedState crowdedState_;

	public const int OccupyPctFieldNumber = 4;

	private float occupyPct_;

	public const int OccupyRateFieldNumber = 5;

	private float occupyRate_;

	public const int ScoreRateFieldNumber = 6;

	private float scoreRate_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<LegionWarPointForceDetail> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeLegionWarPointForceDetailReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ELegionWarForce ForceId
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
	public int PlayerCount
	{
		get
		{
			return playerCount_;
		}
		set
		{
			playerCount_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ELegionWarCrowdedState CrowdedState
	{
		get
		{
			return crowdedState_;
		}
		set
		{
			crowdedState_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public float OccupyPct
	{
		get
		{
			return occupyPct_;
		}
		set
		{
			occupyPct_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public float OccupyRate
	{
		get
		{
			return occupyRate_;
		}
		set
		{
			occupyRate_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public float ScoreRate
	{
		get
		{
			return scoreRate_;
		}
		set
		{
			scoreRate_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public LegionWarPointForceDetail()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public LegionWarPointForceDetail(LegionWarPointForceDetail other)
		: this()
	{
		forceId_ = other.forceId_;
		playerCount_ = other.playerCount_;
		crowdedState_ = other.crowdedState_;
		occupyPct_ = other.occupyPct_;
		occupyRate_ = other.occupyRate_;
		scoreRate_ = other.scoreRate_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public LegionWarPointForceDetail Clone()
	{
		return new LegionWarPointForceDetail(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as LegionWarPointForceDetail);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(LegionWarPointForceDetail other)
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
		if (PlayerCount != other.PlayerCount)
		{
			return false;
		}
		if (CrowdedState != other.CrowdedState)
		{
			return false;
		}
		if (!ProtobufEqualityComparers.BitwiseSingleEqualityComparer.Equals(OccupyPct, other.OccupyPct))
		{
			return false;
		}
		if (!ProtobufEqualityComparers.BitwiseSingleEqualityComparer.Equals(OccupyRate, other.OccupyRate))
		{
			return false;
		}
		if (!ProtobufEqualityComparers.BitwiseSingleEqualityComparer.Equals(ScoreRate, other.ScoreRate))
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
		if (ForceId != ELegionWarForce.EmptyValue)
		{
			num ^= ForceId.GetHashCode();
		}
		if (PlayerCount != 0)
		{
			num ^= PlayerCount.GetHashCode();
		}
		if (CrowdedState != ELegionWarCrowdedState.EmptyValue)
		{
			num ^= CrowdedState.GetHashCode();
		}
		if (OccupyPct != 0f)
		{
			num ^= ProtobufEqualityComparers.BitwiseSingleEqualityComparer.GetHashCode(OccupyPct);
		}
		if (OccupyRate != 0f)
		{
			num ^= ProtobufEqualityComparers.BitwiseSingleEqualityComparer.GetHashCode(OccupyRate);
		}
		if (ScoreRate != 0f)
		{
			num ^= ProtobufEqualityComparers.BitwiseSingleEqualityComparer.GetHashCode(ScoreRate);
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
		if (ForceId != ELegionWarForce.EmptyValue)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteEnum((int)ForceId);
		}
		if (PlayerCount != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteInt32(PlayerCount);
		}
		if (CrowdedState != ELegionWarCrowdedState.EmptyValue)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)24);
			((WriteContext)(ref output)).WriteEnum((int)CrowdedState);
		}
		if (OccupyPct != 0f)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)37);
			((WriteContext)(ref output)).WriteFloat(OccupyPct);
		}
		if (OccupyRate != 0f)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)45);
			((WriteContext)(ref output)).WriteFloat(OccupyRate);
		}
		if (ScoreRate != 0f)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)53);
			((WriteContext)(ref output)).WriteFloat(ScoreRate);
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
		if (ForceId != ELegionWarForce.EmptyValue)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)ForceId);
		}
		if (PlayerCount != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(PlayerCount);
		}
		if (CrowdedState != ELegionWarCrowdedState.EmptyValue)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)CrowdedState);
		}
		if (OccupyPct != 0f)
		{
			num += 5;
		}
		if (OccupyRate != 0f)
		{
			num += 5;
		}
		if (ScoreRate != 0f)
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
	public void MergeFrom(LegionWarPointForceDetail other)
	{
		if (other != null)
		{
			if (other.ForceId != ELegionWarForce.EmptyValue)
			{
				ForceId = other.ForceId;
			}
			if (other.PlayerCount != 0)
			{
				PlayerCount = other.PlayerCount;
			}
			if (other.CrowdedState != ELegionWarCrowdedState.EmptyValue)
			{
				CrowdedState = other.CrowdedState;
			}
			if (other.OccupyPct != 0f)
			{
				OccupyPct = other.OccupyPct;
			}
			if (other.OccupyRate != 0f)
			{
				OccupyRate = other.OccupyRate;
			}
			if (other.ScoreRate != 0f)
			{
				ScoreRate = other.ScoreRate;
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
				ForceId = (ELegionWarForce)((ParseContext)(ref input)).ReadEnum();
				break;
			case 16u:
				PlayerCount = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 24u:
				CrowdedState = (ELegionWarCrowdedState)((ParseContext)(ref input)).ReadEnum();
				break;
			case 37u:
				OccupyPct = ((ParseContext)(ref input)).ReadFloat();
				break;
			case 45u:
				OccupyRate = ((ParseContext)(ref input)).ReadFloat();
				break;
			case 53u:
				ScoreRate = ((ParseContext)(ref input)).ReadFloat();
				break;
			}
		}
	}
}
