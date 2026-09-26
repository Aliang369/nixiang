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
public sealed class LegionWarPointStateDetail : IMessage<LegionWarPointStateDetail>, IMessage, IEquatable<LegionWarPointStateDetail>, IDeepCloneable<LegionWarPointStateDetail>, IBufferMessage
{
	private static readonly MessageParser<LegionWarPointStateDetail> _parser = new MessageParser<LegionWarPointStateDetail>((Func<LegionWarPointStateDetail>)(() => new LegionWarPointStateDetail()));

	private UnknownFieldSet _unknownFields;

	public const int PointIdFieldNumber = 1;

	private int pointId_;

	public const int OccupyForceIdFieldNumber = 2;

	private ELegionWarForce occupyForceId_;

	public const int PointForceListFieldNumber = 3;

	private static readonly FieldCodec<LegionWarPointForceDetail> _repeated_pointForceList_codec = FieldCodec.ForMessage<LegionWarPointForceDetail>(26u, LegionWarPointForceDetail.Parser);

	private readonly RepeatedField<LegionWarPointForceDetail> pointForceList_ = new RepeatedField<LegionWarPointForceDetail>();

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<LegionWarPointStateDetail> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeLegionWarPointStateDetailReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int PointId
	{
		get
		{
			return pointId_;
		}
		set
		{
			pointId_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ELegionWarForce OccupyForceId
	{
		get
		{
			return occupyForceId_;
		}
		set
		{
			occupyForceId_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<LegionWarPointForceDetail> PointForceList => pointForceList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public LegionWarPointStateDetail()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public LegionWarPointStateDetail(LegionWarPointStateDetail other)
		: this()
	{
		pointId_ = other.pointId_;
		occupyForceId_ = other.occupyForceId_;
		pointForceList_ = other.pointForceList_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public LegionWarPointStateDetail Clone()
	{
		return new LegionWarPointStateDetail(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as LegionWarPointStateDetail);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(LegionWarPointStateDetail other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (PointId != other.PointId)
		{
			return false;
		}
		if (OccupyForceId != other.OccupyForceId)
		{
			return false;
		}
		if (!pointForceList_.Equals(other.pointForceList_))
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
		if (PointId != 0)
		{
			num ^= PointId.GetHashCode();
		}
		if (OccupyForceId != ELegionWarForce.EmptyValue)
		{
			num ^= OccupyForceId.GetHashCode();
		}
		num ^= ((object)pointForceList_).GetHashCode();
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
		if (PointId != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteInt32(PointId);
		}
		if (OccupyForceId != ELegionWarForce.EmptyValue)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteEnum((int)OccupyForceId);
		}
		pointForceList_.WriteTo(ref output, _repeated_pointForceList_codec);
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
		if (PointId != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(PointId);
		}
		if (OccupyForceId != ELegionWarForce.EmptyValue)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)OccupyForceId);
		}
		num += pointForceList_.CalculateSize(_repeated_pointForceList_codec);
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(LegionWarPointStateDetail other)
	{
		if (other != null)
		{
			if (other.PointId != 0)
			{
				PointId = other.PointId;
			}
			if (other.OccupyForceId != ELegionWarForce.EmptyValue)
			{
				OccupyForceId = other.OccupyForceId;
			}
			pointForceList_.Add((IEnumerable<LegionWarPointForceDetail>)other.pointForceList_);
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
				PointId = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 16u:
				OccupyForceId = (ELegionWarForce)((ParseContext)(ref input)).ReadEnum();
				break;
			case 26u:
				pointForceList_.AddEntriesFrom(ref input, _repeated_pointForceList_codec);
				break;
			}
		}
	}
}
