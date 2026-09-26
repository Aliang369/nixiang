using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class SeasonCityOwnerNtf : IMessage<SeasonCityOwnerNtf>, IMessage, IEquatable<SeasonCityOwnerNtf>, IDeepCloneable<SeasonCityOwnerNtf>, IBufferMessage
{
	private static readonly MessageParser<SeasonCityOwnerNtf> _parser = new MessageParser<SeasonCityOwnerNtf>((Func<SeasonCityOwnerNtf>)(() => new SeasonCityOwnerNtf()));

	private UnknownFieldSet _unknownFields;

	public const int CityIdFieldNumber = 1;

	private int cityId_;

	public const int OldForceFieldNumber = 2;

	private ESeasonForce oldForce_;

	public const int NewForceFieldNumber = 3;

	private ESeasonForce newForce_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<SeasonCityOwnerNtf> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeSeasonCityOwnerNtfReflection.Descriptor.MessageTypes[0];

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
	public ESeasonForce OldForce
	{
		get
		{
			return oldForce_;
		}
		set
		{
			oldForce_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ESeasonForce NewForce
	{
		get
		{
			return newForce_;
		}
		set
		{
			newForce_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SeasonCityOwnerNtf()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SeasonCityOwnerNtf(SeasonCityOwnerNtf other)
		: this()
	{
		cityId_ = other.cityId_;
		oldForce_ = other.oldForce_;
		newForce_ = other.newForce_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SeasonCityOwnerNtf Clone()
	{
		return new SeasonCityOwnerNtf(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as SeasonCityOwnerNtf);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(SeasonCityOwnerNtf other)
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
		if (OldForce != other.OldForce)
		{
			return false;
		}
		if (NewForce != other.NewForce)
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
		if (OldForce != ESeasonForce.ForceNeutral)
		{
			num ^= OldForce.GetHashCode();
		}
		if (NewForce != ESeasonForce.ForceNeutral)
		{
			num ^= NewForce.GetHashCode();
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
		if (CityId != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteInt32(CityId);
		}
		if (OldForce != ESeasonForce.ForceNeutral)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteEnum((int)OldForce);
		}
		if (NewForce != ESeasonForce.ForceNeutral)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)24);
			((WriteContext)(ref output)).WriteEnum((int)NewForce);
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
		if (CityId != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(CityId);
		}
		if (OldForce != ESeasonForce.ForceNeutral)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)OldForce);
		}
		if (NewForce != ESeasonForce.ForceNeutral)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)NewForce);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(SeasonCityOwnerNtf other)
	{
		if (other != null)
		{
			if (other.CityId != 0)
			{
				CityId = other.CityId;
			}
			if (other.OldForce != ESeasonForce.ForceNeutral)
			{
				OldForce = other.OldForce;
			}
			if (other.NewForce != ESeasonForce.ForceNeutral)
			{
				NewForce = other.NewForce;
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
				CityId = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 16u:
				OldForce = (ESeasonForce)((ParseContext)(ref input)).ReadEnum();
				break;
			case 24u:
				NewForce = (ESeasonForce)((ParseContext)(ref input)).ReadEnum();
				break;
			}
		}
	}
}
