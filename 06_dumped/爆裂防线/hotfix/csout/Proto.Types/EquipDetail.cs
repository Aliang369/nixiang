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
public sealed class EquipDetail : IMessage<EquipDetail>, IMessage, IEquatable<EquipDetail>, IDeepCloneable<EquipDetail>, IBufferMessage
{
	private static readonly MessageParser<EquipDetail> _parser = new MessageParser<EquipDetail>((Func<EquipDetail>)(() => new EquipDetail()));

	private UnknownFieldSet _unknownFields;

	public const int EquipIdFieldNumber = 1;

	private string equipId_ = "";

	public const int EquipTidFieldNumber = 2;

	private int equipTid_;

	public const int QualityFieldNumber = 3;

	private EQuality quality_;

	public const int RarityFieldNumber = 4;

	private EEquipRarityType rarity_;

	public const int SubLevelFieldNumber = 5;

	private int subLevel_;

	public const int SubAttrListFieldNumber = 6;

	private static readonly FieldCodec<EquipSubAttrDetail> _repeated_subAttrList_codec = FieldCodec.ForMessage<EquipSubAttrDetail>(50u, EquipSubAttrDetail.Parser);

	private readonly RepeatedField<EquipSubAttrDetail> subAttrList_ = new RepeatedField<EquipSubAttrDetail>();

	public const int LockFieldNumber = 7;

	private EBoolState lock_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<EquipDetail> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeEquipDetailReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public string EquipId
	{
		get
		{
			return equipId_;
		}
		set
		{
			equipId_ = ProtoPreconditions.CheckNotNull<string>(value, "value");
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int EquipTid
	{
		get
		{
			return equipTid_;
		}
		set
		{
			equipTid_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public EQuality Quality
	{
		get
		{
			return quality_;
		}
		set
		{
			quality_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public EEquipRarityType Rarity
	{
		get
		{
			return rarity_;
		}
		set
		{
			rarity_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int SubLevel
	{
		get
		{
			return subLevel_;
		}
		set
		{
			subLevel_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<EquipSubAttrDetail> SubAttrList => subAttrList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public EBoolState Lock
	{
		get
		{
			return lock_;
		}
		set
		{
			lock_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public EquipDetail()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public EquipDetail(EquipDetail other)
		: this()
	{
		equipId_ = other.equipId_;
		equipTid_ = other.equipTid_;
		quality_ = other.quality_;
		rarity_ = other.rarity_;
		subLevel_ = other.subLevel_;
		subAttrList_ = other.subAttrList_.Clone();
		lock_ = other.lock_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public EquipDetail Clone()
	{
		return new EquipDetail(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as EquipDetail);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(EquipDetail other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (EquipId != other.EquipId)
		{
			return false;
		}
		if (EquipTid != other.EquipTid)
		{
			return false;
		}
		if (Quality != other.Quality)
		{
			return false;
		}
		if (Rarity != other.Rarity)
		{
			return false;
		}
		if (SubLevel != other.SubLevel)
		{
			return false;
		}
		if (!subAttrList_.Equals(other.subAttrList_))
		{
			return false;
		}
		if (Lock != other.Lock)
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
		if (EquipId.Length != 0)
		{
			num ^= EquipId.GetHashCode();
		}
		if (EquipTid != 0)
		{
			num ^= EquipTid.GetHashCode();
		}
		if (Quality != EQuality.EmptyValue)
		{
			num ^= Quality.GetHashCode();
		}
		if (Rarity != EEquipRarityType.EmptyValue)
		{
			num ^= Rarity.GetHashCode();
		}
		if (SubLevel != 0)
		{
			num ^= SubLevel.GetHashCode();
		}
		num ^= ((object)subAttrList_).GetHashCode();
		if (Lock != EBoolState.False)
		{
			num ^= Lock.GetHashCode();
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
		if (EquipId.Length != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)10);
			((WriteContext)(ref output)).WriteString(EquipId);
		}
		if (EquipTid != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteInt32(EquipTid);
		}
		if (Quality != EQuality.EmptyValue)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)24);
			((WriteContext)(ref output)).WriteEnum((int)Quality);
		}
		if (Rarity != EEquipRarityType.EmptyValue)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)32);
			((WriteContext)(ref output)).WriteEnum((int)Rarity);
		}
		if (SubLevel != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)40);
			((WriteContext)(ref output)).WriteInt32(SubLevel);
		}
		subAttrList_.WriteTo(ref output, _repeated_subAttrList_codec);
		if (Lock != EBoolState.False)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)56);
			((WriteContext)(ref output)).WriteEnum((int)Lock);
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
		if (EquipId.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(EquipId);
		}
		if (EquipTid != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(EquipTid);
		}
		if (Quality != EQuality.EmptyValue)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)Quality);
		}
		if (Rarity != EEquipRarityType.EmptyValue)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)Rarity);
		}
		if (SubLevel != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(SubLevel);
		}
		num += subAttrList_.CalculateSize(_repeated_subAttrList_codec);
		if (Lock != EBoolState.False)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)Lock);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(EquipDetail other)
	{
		if (other != null)
		{
			if (other.EquipId.Length != 0)
			{
				EquipId = other.EquipId;
			}
			if (other.EquipTid != 0)
			{
				EquipTid = other.EquipTid;
			}
			if (other.Quality != EQuality.EmptyValue)
			{
				Quality = other.Quality;
			}
			if (other.Rarity != EEquipRarityType.EmptyValue)
			{
				Rarity = other.Rarity;
			}
			if (other.SubLevel != 0)
			{
				SubLevel = other.SubLevel;
			}
			subAttrList_.Add((IEnumerable<EquipSubAttrDetail>)other.subAttrList_);
			if (other.Lock != EBoolState.False)
			{
				Lock = other.Lock;
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
			case 10u:
				EquipId = ((ParseContext)(ref input)).ReadString();
				break;
			case 16u:
				EquipTid = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 24u:
				Quality = (EQuality)((ParseContext)(ref input)).ReadEnum();
				break;
			case 32u:
				Rarity = (EEquipRarityType)((ParseContext)(ref input)).ReadEnum();
				break;
			case 40u:
				SubLevel = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 50u:
				subAttrList_.AddEntriesFrom(ref input, _repeated_subAttrList_codec);
				break;
			case 56u:
				Lock = (EBoolState)((ParseContext)(ref input)).ReadEnum();
				break;
			}
		}
	}
}
