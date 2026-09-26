using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class SeasonGuardInfo : IMessage<SeasonGuardInfo>, IMessage, IEquatable<SeasonGuardInfo>, IDeepCloneable<SeasonGuardInfo>, IBufferMessage
{
	private static readonly MessageParser<SeasonGuardInfo> _parser = new MessageParser<SeasonGuardInfo>((Func<SeasonGuardInfo>)(() => new SeasonGuardInfo()));

	private UnknownFieldSet _unknownFields;

	public const int CityIdFieldNumber = 1;

	private int cityId_;

	public const int GuardCountFieldNumber = 2;

	private int guardCount_;

	public const int FallenFieldNumber = 3;

	private int fallen_;

	public const int FallenEndTmFieldNumber = 4;

	private long fallenEndTm_;

	public const int GarrisonRoleIdsFieldNumber = 5;

	private static readonly FieldCodec<long> _repeated_garrisonRoleIds_codec = FieldCodec.ForInt64(42u);

	private readonly RepeatedField<long> garrisonRoleIds_ = new RepeatedField<long>();

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<SeasonGuardInfo> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeSeasonGuardInfoReflection.Descriptor.MessageTypes[0];

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
	public int GuardCount
	{
		get
		{
			return guardCount_;
		}
		set
		{
			guardCount_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int Fallen
	{
		get
		{
			return fallen_;
		}
		set
		{
			fallen_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public long FallenEndTm
	{
		get
		{
			return fallenEndTm_;
		}
		set
		{
			fallenEndTm_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<long> GarrisonRoleIds => garrisonRoleIds_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SeasonGuardInfo()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SeasonGuardInfo(SeasonGuardInfo other)
		: this()
	{
		cityId_ = other.cityId_;
		guardCount_ = other.guardCount_;
		fallen_ = other.fallen_;
		fallenEndTm_ = other.fallenEndTm_;
		garrisonRoleIds_ = other.garrisonRoleIds_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SeasonGuardInfo Clone()
	{
		return new SeasonGuardInfo(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as SeasonGuardInfo);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(SeasonGuardInfo other)
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
		if (GuardCount != other.GuardCount)
		{
			return false;
		}
		if (Fallen != other.Fallen)
		{
			return false;
		}
		if (FallenEndTm != other.FallenEndTm)
		{
			return false;
		}
		if (!garrisonRoleIds_.Equals(other.garrisonRoleIds_))
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
		if (GuardCount != 0)
		{
			num ^= GuardCount.GetHashCode();
		}
		if (Fallen != 0)
		{
			num ^= Fallen.GetHashCode();
		}
		if (FallenEndTm != 0L)
		{
			num ^= FallenEndTm.GetHashCode();
		}
		num ^= ((object)garrisonRoleIds_).GetHashCode();
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
		if (GuardCount != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteInt32(GuardCount);
		}
		if (Fallen != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)24);
			((WriteContext)(ref output)).WriteInt32(Fallen);
		}
		if (FallenEndTm != 0L)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)32);
			((WriteContext)(ref output)).WriteInt64(FallenEndTm);
		}
		garrisonRoleIds_.WriteTo(ref output, _repeated_garrisonRoleIds_codec);
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
		if (GuardCount != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(GuardCount);
		}
		if (Fallen != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(Fallen);
		}
		if (FallenEndTm != 0L)
		{
			num += 1 + CodedOutputStream.ComputeInt64Size(FallenEndTm);
		}
		num += garrisonRoleIds_.CalculateSize(_repeated_garrisonRoleIds_codec);
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(SeasonGuardInfo other)
	{
		if (other != null)
		{
			if (other.CityId != 0)
			{
				CityId = other.CityId;
			}
			if (other.GuardCount != 0)
			{
				GuardCount = other.GuardCount;
			}
			if (other.Fallen != 0)
			{
				Fallen = other.Fallen;
			}
			if (other.FallenEndTm != 0L)
			{
				FallenEndTm = other.FallenEndTm;
			}
			garrisonRoleIds_.Add((IEnumerable<long>)other.garrisonRoleIds_);
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
				GuardCount = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 24u:
				Fallen = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 32u:
				FallenEndTm = ((ParseContext)(ref input)).ReadInt64();
				break;
			case 40u:
			case 42u:
				garrisonRoleIds_.AddEntriesFrom(ref input, _repeated_garrisonRoleIds_codec);
				break;
			}
		}
	}
}
