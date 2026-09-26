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
public sealed class WarRoomDetail : IMessage<WarRoomDetail>, IMessage, IEquatable<WarRoomDetail>, IDeepCloneable<WarRoomDetail>, IBufferMessage
{
	private static readonly MessageParser<WarRoomDetail> _parser = new MessageParser<WarRoomDetail>((Func<WarRoomDetail>)(() => new WarRoomDetail()));

	private UnknownFieldSet _unknownFields;

	public const int SkillTypeFieldNumber = 1;

	private ESkillType skillType_;

	public const int RoomIdFieldNumber = 2;

	private int roomId_;

	public const int HeroTidFieldNumber = 3;

	private int heroTid_;

	public const int AttrListFieldNumber = 4;

	private static readonly FieldCodec<WarRoomAttrDetail> _repeated_attrList_codec = FieldCodec.ForMessage<WarRoomAttrDetail>(34u, WarRoomAttrDetail.Parser);

	private readonly RepeatedField<WarRoomAttrDetail> attrList_ = new RepeatedField<WarRoomAttrDetail>();

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<WarRoomDetail> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeWarRoomDetailReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ESkillType SkillType
	{
		get
		{
			return skillType_;
		}
		set
		{
			skillType_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int RoomId
	{
		get
		{
			return roomId_;
		}
		set
		{
			roomId_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int HeroTid
	{
		get
		{
			return heroTid_;
		}
		set
		{
			heroTid_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<WarRoomAttrDetail> AttrList => attrList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public WarRoomDetail()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public WarRoomDetail(WarRoomDetail other)
		: this()
	{
		skillType_ = other.skillType_;
		roomId_ = other.roomId_;
		heroTid_ = other.heroTid_;
		attrList_ = other.attrList_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public WarRoomDetail Clone()
	{
		return new WarRoomDetail(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as WarRoomDetail);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(WarRoomDetail other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (SkillType != other.SkillType)
		{
			return false;
		}
		if (RoomId != other.RoomId)
		{
			return false;
		}
		if (HeroTid != other.HeroTid)
		{
			return false;
		}
		if (!attrList_.Equals(other.attrList_))
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
		if (SkillType != ESkillType.EmptyValue)
		{
			num ^= SkillType.GetHashCode();
		}
		if (RoomId != 0)
		{
			num ^= RoomId.GetHashCode();
		}
		if (HeroTid != 0)
		{
			num ^= HeroTid.GetHashCode();
		}
		num ^= ((object)attrList_).GetHashCode();
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
		if (SkillType != ESkillType.EmptyValue)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteEnum((int)SkillType);
		}
		if (RoomId != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteInt32(RoomId);
		}
		if (HeroTid != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)24);
			((WriteContext)(ref output)).WriteInt32(HeroTid);
		}
		attrList_.WriteTo(ref output, _repeated_attrList_codec);
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
		if (SkillType != ESkillType.EmptyValue)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)SkillType);
		}
		if (RoomId != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(RoomId);
		}
		if (HeroTid != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(HeroTid);
		}
		num += attrList_.CalculateSize(_repeated_attrList_codec);
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(WarRoomDetail other)
	{
		if (other != null)
		{
			if (other.SkillType != ESkillType.EmptyValue)
			{
				SkillType = other.SkillType;
			}
			if (other.RoomId != 0)
			{
				RoomId = other.RoomId;
			}
			if (other.HeroTid != 0)
			{
				HeroTid = other.HeroTid;
			}
			attrList_.Add((IEnumerable<WarRoomAttrDetail>)other.attrList_);
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
				SkillType = (ESkillType)((ParseContext)(ref input)).ReadEnum();
				break;
			case 16u:
				RoomId = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 24u:
				HeroTid = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 34u:
				attrList_.AddEntriesFrom(ref input, _repeated_attrList_codec);
				break;
			}
		}
	}
}
