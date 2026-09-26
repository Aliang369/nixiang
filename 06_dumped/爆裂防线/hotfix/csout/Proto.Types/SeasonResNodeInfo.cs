using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class SeasonResNodeInfo : IMessage<SeasonResNodeInfo>, IMessage, IEquatable<SeasonResNodeInfo>, IDeepCloneable<SeasonResNodeInfo>, IBufferMessage
{
	private static readonly MessageParser<SeasonResNodeInfo> _parser = new MessageParser<SeasonResNodeInfo>((Func<SeasonResNodeInfo>)(() => new SeasonResNodeInfo()));

	private UnknownFieldSet _unknownFields;

	public const int ResNodeIdFieldNumber = 1;

	private int resNodeId_;

	public const int ResTypeFieldNumber = 2;

	private ESeasonResType resType_;

	public const int LevelFieldNumber = 3;

	private int level_;

	public const int OwnerRoleIdFieldNumber = 4;

	private int ownerRoleId_;

	public const int ForceIdFieldNumber = 5;

	private ESeasonForce forceId_;

	public const int ProducePerHourFieldNumber = 6;

	private int producePerHour_;

	public const int ActiveFieldNumber = 7;

	private int active_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<SeasonResNodeInfo> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeSeasonResNodeInfoReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int ResNodeId
	{
		get
		{
			return resNodeId_;
		}
		set
		{
			resNodeId_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ESeasonResType ResType
	{
		get
		{
			return resType_;
		}
		set
		{
			resType_ = value;
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
	public int OwnerRoleId
	{
		get
		{
			return ownerRoleId_;
		}
		set
		{
			ownerRoleId_ = value;
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
	public int ProducePerHour
	{
		get
		{
			return producePerHour_;
		}
		set
		{
			producePerHour_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int Active
	{
		get
		{
			return active_;
		}
		set
		{
			active_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SeasonResNodeInfo()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SeasonResNodeInfo(SeasonResNodeInfo other)
		: this()
	{
		resNodeId_ = other.resNodeId_;
		resType_ = other.resType_;
		level_ = other.level_;
		ownerRoleId_ = other.ownerRoleId_;
		forceId_ = other.forceId_;
		producePerHour_ = other.producePerHour_;
		active_ = other.active_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SeasonResNodeInfo Clone()
	{
		return new SeasonResNodeInfo(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as SeasonResNodeInfo);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(SeasonResNodeInfo other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (ResNodeId != other.ResNodeId)
		{
			return false;
		}
		if (ResType != other.ResType)
		{
			return false;
		}
		if (Level != other.Level)
		{
			return false;
		}
		if (OwnerRoleId != other.OwnerRoleId)
		{
			return false;
		}
		if (ForceId != other.ForceId)
		{
			return false;
		}
		if (ProducePerHour != other.ProducePerHour)
		{
			return false;
		}
		if (Active != other.Active)
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
		if (ResNodeId != 0)
		{
			num ^= ResNodeId.GetHashCode();
		}
		if (ResType != ESeasonResType.EmptyValue)
		{
			num ^= ResType.GetHashCode();
		}
		if (Level != 0)
		{
			num ^= Level.GetHashCode();
		}
		if (OwnerRoleId != 0)
		{
			num ^= OwnerRoleId.GetHashCode();
		}
		if (ForceId != ESeasonForce.ForceNeutral)
		{
			num ^= ForceId.GetHashCode();
		}
		if (ProducePerHour != 0)
		{
			num ^= ProducePerHour.GetHashCode();
		}
		if (Active != 0)
		{
			num ^= Active.GetHashCode();
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
		if (ResNodeId != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteInt32(ResNodeId);
		}
		if (ResType != ESeasonResType.EmptyValue)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteEnum((int)ResType);
		}
		if (Level != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)24);
			((WriteContext)(ref output)).WriteInt32(Level);
		}
		if (OwnerRoleId != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)32);
			((WriteContext)(ref output)).WriteInt32(OwnerRoleId);
		}
		if (ForceId != ESeasonForce.ForceNeutral)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)40);
			((WriteContext)(ref output)).WriteEnum((int)ForceId);
		}
		if (ProducePerHour != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)48);
			((WriteContext)(ref output)).WriteInt32(ProducePerHour);
		}
		if (Active != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)56);
			((WriteContext)(ref output)).WriteInt32(Active);
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
		if (ResNodeId != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(ResNodeId);
		}
		if (ResType != ESeasonResType.EmptyValue)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)ResType);
		}
		if (Level != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(Level);
		}
		if (OwnerRoleId != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(OwnerRoleId);
		}
		if (ForceId != ESeasonForce.ForceNeutral)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)ForceId);
		}
		if (ProducePerHour != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(ProducePerHour);
		}
		if (Active != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(Active);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(SeasonResNodeInfo other)
	{
		if (other != null)
		{
			if (other.ResNodeId != 0)
			{
				ResNodeId = other.ResNodeId;
			}
			if (other.ResType != ESeasonResType.EmptyValue)
			{
				ResType = other.ResType;
			}
			if (other.Level != 0)
			{
				Level = other.Level;
			}
			if (other.OwnerRoleId != 0)
			{
				OwnerRoleId = other.OwnerRoleId;
			}
			if (other.ForceId != ESeasonForce.ForceNeutral)
			{
				ForceId = other.ForceId;
			}
			if (other.ProducePerHour != 0)
			{
				ProducePerHour = other.ProducePerHour;
			}
			if (other.Active != 0)
			{
				Active = other.Active;
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
				ResNodeId = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 16u:
				ResType = (ESeasonResType)((ParseContext)(ref input)).ReadEnum();
				break;
			case 24u:
				Level = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 32u:
				OwnerRoleId = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 40u:
				ForceId = (ESeasonForce)((ParseContext)(ref input)).ReadEnum();
				break;
			case 48u:
				ProducePerHour = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 56u:
				Active = ((ParseContext)(ref input)).ReadInt32();
				break;
			}
		}
	}
}
