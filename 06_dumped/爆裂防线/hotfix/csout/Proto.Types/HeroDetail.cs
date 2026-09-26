using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class HeroDetail : IMessage<HeroDetail>, IMessage, IEquatable<HeroDetail>, IDeepCloneable<HeroDetail>, IBufferMessage
{
	private static readonly MessageParser<HeroDetail> _parser = new MessageParser<HeroDetail>((Func<HeroDetail>)(() => new HeroDetail()));

	private UnknownFieldSet _unknownFields;

	public const int HeroIdFieldNumber = 1;

	private string heroId_ = "";

	public const int HeroTidFieldNumber = 2;

	private int heroTid_;

	public const int AvatarIdFieldNumber = 3;

	private int avatarId_;

	public const int QualityFieldNumber = 4;

	private EQuality quality_;

	public const int QualityStarFieldNumber = 5;

	private int qualityStar_;

	public const int RarityFieldNumber = 6;

	private ERarityType rarity_;

	public const int RarityUpgradeFieldNumber = 7;

	private int rarityUpgrade_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<HeroDetail> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeHeroDetailReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public string HeroId
	{
		get
		{
			return heroId_;
		}
		set
		{
			heroId_ = ProtoPreconditions.CheckNotNull<string>(value, "value");
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
	public int AvatarId
	{
		get
		{
			return avatarId_;
		}
		set
		{
			avatarId_ = value;
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
	public int QualityStar
	{
		get
		{
			return qualityStar_;
		}
		set
		{
			qualityStar_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ERarityType Rarity
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
	public int RarityUpgrade
	{
		get
		{
			return rarityUpgrade_;
		}
		set
		{
			rarityUpgrade_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public HeroDetail()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public HeroDetail(HeroDetail other)
		: this()
	{
		heroId_ = other.heroId_;
		heroTid_ = other.heroTid_;
		avatarId_ = other.avatarId_;
		quality_ = other.quality_;
		qualityStar_ = other.qualityStar_;
		rarity_ = other.rarity_;
		rarityUpgrade_ = other.rarityUpgrade_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public HeroDetail Clone()
	{
		return new HeroDetail(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as HeroDetail);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(HeroDetail other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (HeroId != other.HeroId)
		{
			return false;
		}
		if (HeroTid != other.HeroTid)
		{
			return false;
		}
		if (AvatarId != other.AvatarId)
		{
			return false;
		}
		if (Quality != other.Quality)
		{
			return false;
		}
		if (QualityStar != other.QualityStar)
		{
			return false;
		}
		if (Rarity != other.Rarity)
		{
			return false;
		}
		if (RarityUpgrade != other.RarityUpgrade)
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
		if (HeroId.Length != 0)
		{
			num ^= HeroId.GetHashCode();
		}
		if (HeroTid != 0)
		{
			num ^= HeroTid.GetHashCode();
		}
		if (AvatarId != 0)
		{
			num ^= AvatarId.GetHashCode();
		}
		if (Quality != EQuality.EmptyValue)
		{
			num ^= Quality.GetHashCode();
		}
		if (QualityStar != 0)
		{
			num ^= QualityStar.GetHashCode();
		}
		if (Rarity != ERarityType.EmptyValue)
		{
			num ^= Rarity.GetHashCode();
		}
		if (RarityUpgrade != 0)
		{
			num ^= RarityUpgrade.GetHashCode();
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
		if (HeroId.Length != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)10);
			((WriteContext)(ref output)).WriteString(HeroId);
		}
		if (HeroTid != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteInt32(HeroTid);
		}
		if (AvatarId != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)24);
			((WriteContext)(ref output)).WriteInt32(AvatarId);
		}
		if (Quality != EQuality.EmptyValue)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)32);
			((WriteContext)(ref output)).WriteEnum((int)Quality);
		}
		if (QualityStar != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)40);
			((WriteContext)(ref output)).WriteInt32(QualityStar);
		}
		if (Rarity != ERarityType.EmptyValue)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)48);
			((WriteContext)(ref output)).WriteEnum((int)Rarity);
		}
		if (RarityUpgrade != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)56);
			((WriteContext)(ref output)).WriteInt32(RarityUpgrade);
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
		if (HeroId.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(HeroId);
		}
		if (HeroTid != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(HeroTid);
		}
		if (AvatarId != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(AvatarId);
		}
		if (Quality != EQuality.EmptyValue)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)Quality);
		}
		if (QualityStar != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(QualityStar);
		}
		if (Rarity != ERarityType.EmptyValue)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)Rarity);
		}
		if (RarityUpgrade != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(RarityUpgrade);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(HeroDetail other)
	{
		if (other != null)
		{
			if (other.HeroId.Length != 0)
			{
				HeroId = other.HeroId;
			}
			if (other.HeroTid != 0)
			{
				HeroTid = other.HeroTid;
			}
			if (other.AvatarId != 0)
			{
				AvatarId = other.AvatarId;
			}
			if (other.Quality != EQuality.EmptyValue)
			{
				Quality = other.Quality;
			}
			if (other.QualityStar != 0)
			{
				QualityStar = other.QualityStar;
			}
			if (other.Rarity != ERarityType.EmptyValue)
			{
				Rarity = other.Rarity;
			}
			if (other.RarityUpgrade != 0)
			{
				RarityUpgrade = other.RarityUpgrade;
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
				HeroId = ((ParseContext)(ref input)).ReadString();
				break;
			case 16u:
				HeroTid = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 24u:
				AvatarId = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 32u:
				Quality = (EQuality)((ParseContext)(ref input)).ReadEnum();
				break;
			case 40u:
				QualityStar = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 48u:
				Rarity = (ERarityType)((ParseContext)(ref input)).ReadEnum();
				break;
			case 56u:
				RarityUpgrade = ((ParseContext)(ref input)).ReadInt32();
				break;
			}
		}
	}
}
