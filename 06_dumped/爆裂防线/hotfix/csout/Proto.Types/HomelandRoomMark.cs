using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class HomelandRoomMark : IMessage<HomelandRoomMark>, IMessage, IEquatable<HomelandRoomMark>, IDeepCloneable<HomelandRoomMark>, IBufferMessage
{
	private static readonly MessageParser<HomelandRoomMark> _parser = new MessageParser<HomelandRoomMark>((Func<HomelandRoomMark>)(() => new HomelandRoomMark()));

	private UnknownFieldSet _unknownFields;

	public const int HallIdFieldNumber = 1;

	private int hallId_;

	public const int CommRadarIdFieldNumber = 2;

	private int commRadarId_;

	public const int SpecimenIdFieldNumber = 3;

	private int specimenId_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<HomelandRoomMark> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeHomelandRoomMarkReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int HallId
	{
		get
		{
			return hallId_;
		}
		set
		{
			hallId_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int CommRadarId
	{
		get
		{
			return commRadarId_;
		}
		set
		{
			commRadarId_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int SpecimenId
	{
		get
		{
			return specimenId_;
		}
		set
		{
			specimenId_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public HomelandRoomMark()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public HomelandRoomMark(HomelandRoomMark other)
		: this()
	{
		hallId_ = other.hallId_;
		commRadarId_ = other.commRadarId_;
		specimenId_ = other.specimenId_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public HomelandRoomMark Clone()
	{
		return new HomelandRoomMark(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as HomelandRoomMark);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(HomelandRoomMark other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (HallId != other.HallId)
		{
			return false;
		}
		if (CommRadarId != other.CommRadarId)
		{
			return false;
		}
		if (SpecimenId != other.SpecimenId)
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
		if (HallId != 0)
		{
			num ^= HallId.GetHashCode();
		}
		if (CommRadarId != 0)
		{
			num ^= CommRadarId.GetHashCode();
		}
		if (SpecimenId != 0)
		{
			num ^= SpecimenId.GetHashCode();
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
		if (HallId != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteInt32(HallId);
		}
		if (CommRadarId != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteInt32(CommRadarId);
		}
		if (SpecimenId != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)24);
			((WriteContext)(ref output)).WriteInt32(SpecimenId);
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
		if (HallId != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(HallId);
		}
		if (CommRadarId != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(CommRadarId);
		}
		if (SpecimenId != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(SpecimenId);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(HomelandRoomMark other)
	{
		if (other != null)
		{
			if (other.HallId != 0)
			{
				HallId = other.HallId;
			}
			if (other.CommRadarId != 0)
			{
				CommRadarId = other.CommRadarId;
			}
			if (other.SpecimenId != 0)
			{
				SpecimenId = other.SpecimenId;
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
				HallId = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 16u:
				CommRadarId = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 24u:
				SpecimenId = ((ParseContext)(ref input)).ReadInt32();
				break;
			}
		}
	}
}
