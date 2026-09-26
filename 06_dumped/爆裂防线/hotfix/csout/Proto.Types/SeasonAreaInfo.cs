using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class SeasonAreaInfo : IMessage<SeasonAreaInfo>, IMessage, IEquatable<SeasonAreaInfo>, IDeepCloneable<SeasonAreaInfo>, IBufferMessage
{
	private static readonly MessageParser<SeasonAreaInfo> _parser = new MessageParser<SeasonAreaInfo>((Func<SeasonAreaInfo>)(() => new SeasonAreaInfo()));

	private UnknownFieldSet _unknownFields;

	public const int AreaIdFieldNumber = 1;

	private int areaId_;

	public const int GridXFieldNumber = 2;

	private int gridX_;

	public const int GridYFieldNumber = 3;

	private int gridY_;

	public const int ForceIdFieldNumber = 4;

	private ESeasonForce forceId_;

	public const int StateFieldNumber = 5;

	private ESeasonAreaState state_;

	public const int AreaTypeFieldNumber = 6;

	private ESeasonAreaType areaType_;

	public const int LevelFieldNumber = 7;

	private int level_;

	public const int BirthForceFieldNumber = 8;

	private ESeasonForce birthForce_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<SeasonAreaInfo> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeSeasonAreaInfoReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

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
	public int GridX
	{
		get
		{
			return gridX_;
		}
		set
		{
			gridX_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int GridY
	{
		get
		{
			return gridY_;
		}
		set
		{
			gridY_ = value;
		}
	}

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
	public ESeasonAreaState State
	{
		get
		{
			return state_;
		}
		set
		{
			state_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ESeasonAreaType AreaType
	{
		get
		{
			return areaType_;
		}
		set
		{
			areaType_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int Level
	{
		get
		{
			return level_;
		}
		set
		{
			level_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ESeasonForce BirthForce
	{
		get
		{
			return birthForce_;
		}
		set
		{
			birthForce_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SeasonAreaInfo()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SeasonAreaInfo(SeasonAreaInfo other)
		: this()
	{
		areaId_ = other.areaId_;
		gridX_ = other.gridX_;
		gridY_ = other.gridY_;
		forceId_ = other.forceId_;
		state_ = other.state_;
		areaType_ = other.areaType_;
		level_ = other.level_;
		birthForce_ = other.birthForce_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SeasonAreaInfo Clone()
	{
		return new SeasonAreaInfo(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as SeasonAreaInfo);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(SeasonAreaInfo other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (AreaId != other.AreaId)
		{
			return false;
		}
		if (GridX != other.GridX)
		{
			return false;
		}
		if (GridY != other.GridY)
		{
			return false;
		}
		if (ForceId != other.ForceId)
		{
			return false;
		}
		if (State != other.State)
		{
			return false;
		}
		if (AreaType != other.AreaType)
		{
			return false;
		}
		if (Level != other.Level)
		{
			return false;
		}
		if (BirthForce != other.BirthForce)
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
		if (AreaId != 0)
		{
			num ^= AreaId.GetHashCode();
		}
		if (GridX != 0)
		{
			num ^= GridX.GetHashCode();
		}
		if (GridY != 0)
		{
			num ^= GridY.GetHashCode();
		}
		if (ForceId != ESeasonForce.ForceNeutral)
		{
			num ^= ForceId.GetHashCode();
		}
		if (State != ESeasonAreaState.EmptyValue)
		{
			num ^= State.GetHashCode();
		}
		if (AreaType != ESeasonAreaType.Birth)
		{
			num ^= AreaType.GetHashCode();
		}
		if (Level != 0)
		{
			num ^= Level.GetHashCode();
		}
		if (BirthForce != ESeasonForce.ForceNeutral)
		{
			num ^= BirthForce.GetHashCode();
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
		if (AreaId != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteInt32(AreaId);
		}
		if (GridX != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteInt32(GridX);
		}
		if (GridY != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)24);
			((WriteContext)(ref output)).WriteInt32(GridY);
		}
		if (ForceId != ESeasonForce.ForceNeutral)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)32);
			((WriteContext)(ref output)).WriteEnum((int)ForceId);
		}
		if (State != ESeasonAreaState.EmptyValue)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)40);
			((WriteContext)(ref output)).WriteEnum((int)State);
		}
		if (AreaType != ESeasonAreaType.Birth)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)48);
			((WriteContext)(ref output)).WriteEnum((int)AreaType);
		}
		if (Level != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)56);
			((WriteContext)(ref output)).WriteInt32(Level);
		}
		if (BirthForce != ESeasonForce.ForceNeutral)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)64);
			((WriteContext)(ref output)).WriteEnum((int)BirthForce);
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
		if (AreaId != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(AreaId);
		}
		if (GridX != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(GridX);
		}
		if (GridY != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(GridY);
		}
		if (ForceId != ESeasonForce.ForceNeutral)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)ForceId);
		}
		if (State != ESeasonAreaState.EmptyValue)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)State);
		}
		if (AreaType != ESeasonAreaType.Birth)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)AreaType);
		}
		if (Level != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(Level);
		}
		if (BirthForce != ESeasonForce.ForceNeutral)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)BirthForce);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(SeasonAreaInfo other)
	{
		if (other != null)
		{
			if (other.AreaId != 0)
			{
				AreaId = other.AreaId;
			}
			if (other.GridX != 0)
			{
				GridX = other.GridX;
			}
			if (other.GridY != 0)
			{
				GridY = other.GridY;
			}
			if (other.ForceId != ESeasonForce.ForceNeutral)
			{
				ForceId = other.ForceId;
			}
			if (other.State != ESeasonAreaState.EmptyValue)
			{
				State = other.State;
			}
			if (other.AreaType != ESeasonAreaType.Birth)
			{
				AreaType = other.AreaType;
			}
			if (other.Level != 0)
			{
				Level = other.Level;
			}
			if (other.BirthForce != ESeasonForce.ForceNeutral)
			{
				BirthForce = other.BirthForce;
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
				AreaId = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 16u:
				GridX = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 24u:
				GridY = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 32u:
				ForceId = (ESeasonForce)((ParseContext)(ref input)).ReadEnum();
				break;
			case 40u:
				State = (ESeasonAreaState)((ParseContext)(ref input)).ReadEnum();
				break;
			case 48u:
				AreaType = (ESeasonAreaType)((ParseContext)(ref input)).ReadEnum();
				break;
			case 56u:
				Level = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 64u:
				BirthForce = (ESeasonForce)((ParseContext)(ref input)).ReadEnum();
				break;
			}
		}
	}
}
