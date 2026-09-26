using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class SeasonResAttackReq : IMessage<SeasonResAttackReq>, IMessage, IEquatable<SeasonResAttackReq>, IDeepCloneable<SeasonResAttackReq>, IBufferMessage
{
	private static readonly MessageParser<SeasonResAttackReq> _parser = new MessageParser<SeasonResAttackReq>((Func<SeasonResAttackReq>)(() => new SeasonResAttackReq()));

	private UnknownFieldSet _unknownFields;

	public const int TeamIdFieldNumber = 1;

	private int teamId_;

	public const int CityIdFieldNumber = 2;

	private int cityId_;

	public const int ResIdxFieldNumber = 3;

	private int resIdx_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<SeasonResAttackReq> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeSeasonResAttackReqReflection.Descriptor.MessageTypes[0];

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
	public int ResIdx
	{
		get
		{
			return resIdx_;
		}
		set
		{
			resIdx_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SeasonResAttackReq()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SeasonResAttackReq(SeasonResAttackReq other)
		: this()
	{
		teamId_ = other.teamId_;
		cityId_ = other.cityId_;
		resIdx_ = other.resIdx_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SeasonResAttackReq Clone()
	{
		return new SeasonResAttackReq(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as SeasonResAttackReq);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(SeasonResAttackReq other)
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
		if (CityId != other.CityId)
		{
			return false;
		}
		if (ResIdx != other.ResIdx)
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
		if (CityId != 0)
		{
			num ^= CityId.GetHashCode();
		}
		if (ResIdx != 0)
		{
			num ^= ResIdx.GetHashCode();
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
		if (CityId != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteInt32(CityId);
		}
		if (ResIdx != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)24);
			((WriteContext)(ref output)).WriteInt32(ResIdx);
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
		if (CityId != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(CityId);
		}
		if (ResIdx != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(ResIdx);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(SeasonResAttackReq other)
	{
		if (other != null)
		{
			if (other.TeamId != 0)
			{
				TeamId = other.TeamId;
			}
			if (other.CityId != 0)
			{
				CityId = other.CityId;
			}
			if (other.ResIdx != 0)
			{
				ResIdx = other.ResIdx;
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
				CityId = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 24u:
				ResIdx = ((ParseContext)(ref input)).ReadInt32();
				break;
			}
		}
	}
}
