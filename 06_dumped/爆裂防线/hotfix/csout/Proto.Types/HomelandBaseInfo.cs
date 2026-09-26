using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class HomelandBaseInfo : IMessage<HomelandBaseInfo>, IMessage, IEquatable<HomelandBaseInfo>, IDeepCloneable<HomelandBaseInfo>, IBufferMessage
{
	private static readonly MessageParser<HomelandBaseInfo> _parser = new MessageParser<HomelandBaseInfo>((Func<HomelandBaseInfo>)(() => new HomelandBaseInfo()));

	private UnknownFieldSet _unknownFields;

	public const int FloorListFieldNumber = 1;

	private static readonly FieldCodec<HomelandFloorDetail> _repeated_floorList_codec = FieldCodec.ForMessage<HomelandFloorDetail>(10u, HomelandFloorDetail.Parser);

	private readonly RepeatedField<HomelandFloorDetail> floorList_ = new RepeatedField<HomelandFloorDetail>();

	public const int RoomMarkFieldNumber = 2;

	private HomelandRoomMark roomMark_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<HomelandBaseInfo> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeHomelandBaseInfoReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<HomelandFloorDetail> FloorList => floorList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public HomelandRoomMark RoomMark
	{
		get
		{
			return roomMark_;
		}
		set
		{
			roomMark_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public HomelandBaseInfo()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public HomelandBaseInfo(HomelandBaseInfo other)
		: this()
	{
		floorList_ = other.floorList_.Clone();
		roomMark_ = ((other.roomMark_ != null) ? other.roomMark_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public HomelandBaseInfo Clone()
	{
		return new HomelandBaseInfo(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as HomelandBaseInfo);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(HomelandBaseInfo other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!floorList_.Equals(other.floorList_))
		{
			return false;
		}
		if (!object.Equals(RoomMark, other.RoomMark))
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
		num ^= ((object)floorList_).GetHashCode();
		if (roomMark_ != null)
		{
			num ^= RoomMark.GetHashCode();
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
		floorList_.WriteTo(ref output, _repeated_floorList_codec);
		if (roomMark_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)18);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)RoomMark);
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
		num += floorList_.CalculateSize(_repeated_floorList_codec);
		if (roomMark_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)RoomMark);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(HomelandBaseInfo other)
	{
		if (other == null)
		{
			return;
		}
		floorList_.Add((IEnumerable<HomelandFloorDetail>)other.floorList_);
		if (other.roomMark_ != null)
		{
			if (roomMark_ == null)
			{
				RoomMark = new HomelandRoomMark();
			}
			RoomMark.MergeFrom(other.RoomMark);
		}
		_unknownFields = UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
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
			case 10u:
				floorList_.AddEntriesFrom(ref input, _repeated_floorList_codec);
				break;
			case 18u:
				if (roomMark_ == null)
				{
					RoomMark = new HomelandRoomMark();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)RoomMark);
				break;
			}
		}
	}
}
