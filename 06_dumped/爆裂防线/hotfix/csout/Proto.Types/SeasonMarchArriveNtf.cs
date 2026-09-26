using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class SeasonMarchArriveNtf : IMessage<SeasonMarchArriveNtf>, IMessage, IEquatable<SeasonMarchArriveNtf>, IDeepCloneable<SeasonMarchArriveNtf>, IBufferMessage
{
	private static readonly MessageParser<SeasonMarchArriveNtf> _parser = new MessageParser<SeasonMarchArriveNtf>((Func<SeasonMarchArriveNtf>)(() => new SeasonMarchArriveNtf()));

	private UnknownFieldSet _unknownFields;

	public const int TeamIdFieldNumber = 1;

	private int teamId_;

	public const int PosXFieldNumber = 2;

	private float posX_;

	public const int PosYFieldNumber = 3;

	private float posY_;

	public const int AreaIdFieldNumber = 4;

	private int areaId_;

	public const int OccupyChgFieldNumber = 5;

	private int occupyChg_;

	public const int GasLeftFieldNumber = 6;

	private int gasLeft_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<SeasonMarchArriveNtf> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeSeasonMarchArriveNtfReflection.Descriptor.MessageTypes[0];

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
	public float PosX
	{
		get
		{
			return posX_;
		}
		set
		{
			posX_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public float PosY
	{
		get
		{
			return posY_;
		}
		set
		{
			posY_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int AreaId
	{
		get
		{
			return areaId_;
		}
		set
		{
			areaId_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int OccupyChg
	{
		get
		{
			return occupyChg_;
		}
		set
		{
			occupyChg_ = value;
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
	public SeasonMarchArriveNtf()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SeasonMarchArriveNtf(SeasonMarchArriveNtf other)
		: this()
	{
		teamId_ = other.teamId_;
		posX_ = other.posX_;
		posY_ = other.posY_;
		areaId_ = other.areaId_;
		occupyChg_ = other.occupyChg_;
		gasLeft_ = other.gasLeft_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SeasonMarchArriveNtf Clone()
	{
		return new SeasonMarchArriveNtf(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as SeasonMarchArriveNtf);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(SeasonMarchArriveNtf other)
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
		if (!ProtobufEqualityComparers.BitwiseSingleEqualityComparer.Equals(PosX, other.PosX))
		{
			return false;
		}
		if (!ProtobufEqualityComparers.BitwiseSingleEqualityComparer.Equals(PosY, other.PosY))
		{
			return false;
		}
		if (AreaId != other.AreaId)
		{
			return false;
		}
		if (OccupyChg != other.OccupyChg)
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
		if (PosX != 0f)
		{
			num ^= ProtobufEqualityComparers.BitwiseSingleEqualityComparer.GetHashCode(PosX);
		}
		if (PosY != 0f)
		{
			num ^= ProtobufEqualityComparers.BitwiseSingleEqualityComparer.GetHashCode(PosY);
		}
		if (AreaId != 0)
		{
			num ^= AreaId.GetHashCode();
		}
		if (OccupyChg != 0)
		{
			num ^= OccupyChg.GetHashCode();
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
		if (PosX != 0f)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)21);
			((WriteContext)(ref output)).WriteFloat(PosX);
		}
		if (PosY != 0f)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)29);
			((WriteContext)(ref output)).WriteFloat(PosY);
		}
		if (AreaId != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)32);
			((WriteContext)(ref output)).WriteInt32(AreaId);
		}
		if (OccupyChg != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)40);
			((WriteContext)(ref output)).WriteInt32(OccupyChg);
		}
		if (GasLeft != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)48);
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
		if (PosX != 0f)
		{
			num += 5;
		}
		if (PosY != 0f)
		{
			num += 5;
		}
		if (AreaId != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(AreaId);
		}
		if (OccupyChg != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(OccupyChg);
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
	public void MergeFrom(SeasonMarchArriveNtf other)
	{
		if (other != null)
		{
			if (other.TeamId != 0)
			{
				TeamId = other.TeamId;
			}
			if (other.PosX != 0f)
			{
				PosX = other.PosX;
			}
			if (other.PosY != 0f)
			{
				PosY = other.PosY;
			}
			if (other.AreaId != 0)
			{
				AreaId = other.AreaId;
			}
			if (other.OccupyChg != 0)
			{
				OccupyChg = other.OccupyChg;
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
			case 21u:
				PosX = ((ParseContext)(ref input)).ReadFloat();
				break;
			case 29u:
				PosY = ((ParseContext)(ref input)).ReadFloat();
				break;
			case 32u:
				AreaId = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 40u:
				OccupyChg = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 48u:
				GasLeft = ((ParseContext)(ref input)).ReadInt32();
				break;
			}
		}
	}
}
