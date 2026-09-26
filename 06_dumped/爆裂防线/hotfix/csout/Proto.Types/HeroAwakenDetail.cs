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
public sealed class HeroAwakenDetail : IMessage<HeroAwakenDetail>, IMessage, IEquatable<HeroAwakenDetail>, IDeepCloneable<HeroAwakenDetail>, IBufferMessage
{
	private static readonly MessageParser<HeroAwakenDetail> _parser = new MessageParser<HeroAwakenDetail>((Func<HeroAwakenDetail>)(() => new HeroAwakenDetail()));

	private UnknownFieldSet _unknownFields;

	public const int SkillTypeFieldNumber = 1;

	private ESkillType skillType_;

	public const int LevelFieldNumber = 2;

	private int level_;

	public const int AttrDetailFieldNumber = 3;

	private HeroAwakenAttrDetail attrDetail_;

	public const int GeneSlotListFieldNumber = 4;

	private static readonly FieldCodec<GeneSlotDetail> _repeated_geneSlotList_codec = FieldCodec.ForMessage<GeneSlotDetail>(34u, GeneSlotDetail.Parser);

	private readonly RepeatedField<GeneSlotDetail> geneSlotList_ = new RepeatedField<GeneSlotDetail>();

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<HeroAwakenDetail> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeHeroAwakenDetailReflection.Descriptor.MessageTypes[0];

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
	public HeroAwakenAttrDetail AttrDetail
	{
		get
		{
			return attrDetail_;
		}
		set
		{
			attrDetail_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<GeneSlotDetail> GeneSlotList => geneSlotList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public HeroAwakenDetail()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public HeroAwakenDetail(HeroAwakenDetail other)
		: this()
	{
		skillType_ = other.skillType_;
		level_ = other.level_;
		attrDetail_ = ((other.attrDetail_ != null) ? other.attrDetail_.Clone() : null);
		geneSlotList_ = other.geneSlotList_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public HeroAwakenDetail Clone()
	{
		return new HeroAwakenDetail(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as HeroAwakenDetail);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(HeroAwakenDetail other)
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
		if (Level != other.Level)
		{
			return false;
		}
		if (!object.Equals(AttrDetail, other.AttrDetail))
		{
			return false;
		}
		if (!geneSlotList_.Equals(other.geneSlotList_))
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
		if (Level != 0)
		{
			num ^= Level.GetHashCode();
		}
		if (attrDetail_ != null)
		{
			num ^= AttrDetail.GetHashCode();
		}
		num ^= ((object)geneSlotList_).GetHashCode();
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
		if (Level != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteInt32(Level);
		}
		if (attrDetail_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)26);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)AttrDetail);
		}
		geneSlotList_.WriteTo(ref output, _repeated_geneSlotList_codec);
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
		if (Level != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(Level);
		}
		if (attrDetail_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)AttrDetail);
		}
		num += geneSlotList_.CalculateSize(_repeated_geneSlotList_codec);
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(HeroAwakenDetail other)
	{
		if (other == null)
		{
			return;
		}
		if (other.SkillType != ESkillType.EmptyValue)
		{
			SkillType = other.SkillType;
		}
		if (other.Level != 0)
		{
			Level = other.Level;
		}
		if (other.attrDetail_ != null)
		{
			if (attrDetail_ == null)
			{
				AttrDetail = new HeroAwakenAttrDetail();
			}
			AttrDetail.MergeFrom(other.AttrDetail);
		}
		geneSlotList_.Add((IEnumerable<GeneSlotDetail>)other.geneSlotList_);
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
			case 8u:
				SkillType = (ESkillType)((ParseContext)(ref input)).ReadEnum();
				break;
			case 16u:
				Level = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 26u:
				if (attrDetail_ == null)
				{
					AttrDetail = new HeroAwakenAttrDetail();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)AttrDetail);
				break;
			case 34u:
				geneSlotList_.AddEntriesFrom(ref input, _repeated_geneSlotList_codec);
				break;
			}
		}
	}
}
