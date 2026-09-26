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
public sealed class SeasonCityDetailAck : IMessage<SeasonCityDetailAck>, IMessage, IEquatable<SeasonCityDetailAck>, IDeepCloneable<SeasonCityDetailAck>, IBufferMessage
{
	private static readonly MessageParser<SeasonCityDetailAck> _parser = new MessageParser<SeasonCityDetailAck>((Func<SeasonCityDetailAck>)(() => new SeasonCityDetailAck()));

	private UnknownFieldSet _unknownFields;

	public const int CityIdFieldNumber = 1;

	private int cityId_;

	public const int AreaTypeFieldNumber = 2;

	private ESeasonAreaType areaType_;

	public const int LevelFieldNumber = 3;

	private int level_;

	public const int ForceIdFieldNumber = 4;

	private ESeasonForce forceId_;

	public const int StateFieldNumber = 5;

	private ESeasonAreaState state_;

	public const int ResNodeListFieldNumber = 6;

	private static readonly FieldCodec<SeasonResNodeInfo> _repeated_resNodeList_codec = FieldCodec.ForMessage<SeasonResNodeInfo>(50u, SeasonResNodeInfo.Parser);

	private readonly RepeatedField<SeasonResNodeInfo> resNodeList_ = new RepeatedField<SeasonResNodeInfo>();

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<SeasonCityDetailAck> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeSeasonCityDetailAckReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int CityId
	{
		get
		{
			return cityId_;
		}
		set
		{
			cityId_ = value;
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
	public RepeatedField<SeasonResNodeInfo> ResNodeList => resNodeList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SeasonCityDetailAck()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SeasonCityDetailAck(SeasonCityDetailAck other)
		: this()
	{
		cityId_ = other.cityId_;
		areaType_ = other.areaType_;
		level_ = other.level_;
		forceId_ = other.forceId_;
		state_ = other.state_;
		resNodeList_ = other.resNodeList_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SeasonCityDetailAck Clone()
	{
		return new SeasonCityDetailAck(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as SeasonCityDetailAck);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(SeasonCityDetailAck other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (CityId != other.CityId)
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
		if (ForceId != other.ForceId)
		{
			return false;
		}
		if (State != other.State)
		{
			return false;
		}
		if (!resNodeList_.Equals(other.resNodeList_))
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
		if (CityId != 0)
		{
			num ^= CityId.GetHashCode();
		}
		if (AreaType != ESeasonAreaType.Birth)
		{
			num ^= AreaType.GetHashCode();
		}
		if (Level != 0)
		{
			num ^= Level.GetHashCode();
		}
		if (ForceId != ESeasonForce.ForceNeutral)
		{
			num ^= ForceId.GetHashCode();
		}
		if (State != ESeasonAreaState.EmptyValue)
		{
			num ^= State.GetHashCode();
		}
		num ^= ((object)resNodeList_).GetHashCode();
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
		if (CityId != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteInt32(CityId);
		}
		if (AreaType != ESeasonAreaType.Birth)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteEnum((int)AreaType);
		}
		if (Level != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)24);
			((WriteContext)(ref output)).WriteInt32(Level);
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
		resNodeList_.WriteTo(ref output, _repeated_resNodeList_codec);
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
		if (CityId != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(CityId);
		}
		if (AreaType != ESeasonAreaType.Birth)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)AreaType);
		}
		if (Level != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(Level);
		}
		if (ForceId != ESeasonForce.ForceNeutral)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)ForceId);
		}
		if (State != ESeasonAreaState.EmptyValue)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)State);
		}
		num += resNodeList_.CalculateSize(_repeated_resNodeList_codec);
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(SeasonCityDetailAck other)
	{
		if (other != null)
		{
			if (other.CityId != 0)
			{
				CityId = other.CityId;
			}
			if (other.AreaType != ESeasonAreaType.Birth)
			{
				AreaType = other.AreaType;
			}
			if (other.Level != 0)
			{
				Level = other.Level;
			}
			if (other.ForceId != ESeasonForce.ForceNeutral)
			{
				ForceId = other.ForceId;
			}
			if (other.State != ESeasonAreaState.EmptyValue)
			{
				State = other.State;
			}
			resNodeList_.Add((IEnumerable<SeasonResNodeInfo>)other.resNodeList_);
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
				CityId = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 16u:
				AreaType = (ESeasonAreaType)((ParseContext)(ref input)).ReadEnum();
				break;
			case 24u:
				Level = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 32u:
				ForceId = (ESeasonForce)((ParseContext)(ref input)).ReadEnum();
				break;
			case 40u:
				State = (ESeasonAreaState)((ParseContext)(ref input)).ReadEnum();
				break;
			case 50u:
				resNodeList_.AddEntriesFrom(ref input, _repeated_resNodeList_codec);
				break;
			}
		}
	}
}
