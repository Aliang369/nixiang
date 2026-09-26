using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class SeasonDeclareInfo : IMessage<SeasonDeclareInfo>, IMessage, IEquatable<SeasonDeclareInfo>, IDeepCloneable<SeasonDeclareInfo>, IBufferMessage
{
	private static readonly MessageParser<SeasonDeclareInfo> _parser = new MessageParser<SeasonDeclareInfo>((Func<SeasonDeclareInfo>)(() => new SeasonDeclareInfo()));

	private UnknownFieldSet _unknownFields;

	public const int CityIdFieldNumber = 1;

	private int cityId_;

	public const int AtkForceFieldNumber = 2;

	private ESeasonForce atkForce_;

	public const int DeclareTmFieldNumber = 3;

	private long declareTm_;

	public const int EndTmFieldNumber = 4;

	private long endTm_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<SeasonDeclareInfo> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeSeasonDeclareInfoReflection.Descriptor.MessageTypes[0];

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
	public ESeasonForce AtkForce
	{
		get
		{
			return atkForce_;
		}
		set
		{
			atkForce_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public long DeclareTm
	{
		get
		{
			return declareTm_;
		}
		set
		{
			declareTm_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public long EndTm
	{
		get
		{
			return endTm_;
		}
		set
		{
			endTm_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SeasonDeclareInfo()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SeasonDeclareInfo(SeasonDeclareInfo other)
		: this()
	{
		cityId_ = other.cityId_;
		atkForce_ = other.atkForce_;
		declareTm_ = other.declareTm_;
		endTm_ = other.endTm_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SeasonDeclareInfo Clone()
	{
		return new SeasonDeclareInfo(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as SeasonDeclareInfo);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(SeasonDeclareInfo other)
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
		if (AtkForce != other.AtkForce)
		{
			return false;
		}
		if (DeclareTm != other.DeclareTm)
		{
			return false;
		}
		if (EndTm != other.EndTm)
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
		if (AtkForce != ESeasonForce.ForceNeutral)
		{
			num ^= AtkForce.GetHashCode();
		}
		if (DeclareTm != 0L)
		{
			num ^= DeclareTm.GetHashCode();
		}
		if (EndTm != 0L)
		{
			num ^= EndTm.GetHashCode();
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
		if (AtkForce != ESeasonForce.ForceNeutral)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteEnum((int)AtkForce);
		}
		if (DeclareTm != 0L)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)24);
			((WriteContext)(ref output)).WriteInt64(DeclareTm);
		}
		if (EndTm != 0L)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)32);
			((WriteContext)(ref output)).WriteInt64(EndTm);
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
		if (AtkForce != ESeasonForce.ForceNeutral)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)AtkForce);
		}
		if (DeclareTm != 0L)
		{
			num += 1 + CodedOutputStream.ComputeInt64Size(DeclareTm);
		}
		if (EndTm != 0L)
		{
			num += 1 + CodedOutputStream.ComputeInt64Size(EndTm);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(SeasonDeclareInfo other)
	{
		if (other != null)
		{
			if (other.CityId != 0)
			{
				CityId = other.CityId;
			}
			if (other.AtkForce != ESeasonForce.ForceNeutral)
			{
				AtkForce = other.AtkForce;
			}
			if (other.DeclareTm != 0L)
			{
				DeclareTm = other.DeclareTm;
			}
			if (other.EndTm != 0L)
			{
				EndTm = other.EndTm;
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
				AtkForce = (ESeasonForce)((ParseContext)(ref input)).ReadEnum();
				break;
			case 24u:
				DeclareTm = ((ParseContext)(ref input)).ReadInt64();
				break;
			case 32u:
				EndTm = ((ParseContext)(ref input)).ReadInt64();
				break;
			}
		}
	}
}
