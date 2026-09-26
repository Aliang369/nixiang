using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class WarRoomAttrFromInfo : IMessage<WarRoomAttrFromInfo>, IMessage, IEquatable<WarRoomAttrFromInfo>, IDeepCloneable<WarRoomAttrFromInfo>, IBufferMessage
{
	private static readonly MessageParser<WarRoomAttrFromInfo> _parser = new MessageParser<WarRoomAttrFromInfo>((Func<WarRoomAttrFromInfo>)(() => new WarRoomAttrFromInfo()));

	private UnknownFieldSet _unknownFields;

	public const int SkillBaseFieldNumber = 1;

	private float skillBase_;

	public const int WarRoomFieldNumber = 2;

	private float warRoom_;

	public const int EquipSlotFieldNumber = 3;

	private float equipSlot_;

	public const int HeroFieldNumber = 4;

	private float hero_;

	public const int GunFieldNumber = 5;

	private float gun_;

	public const int ServantFieldNumber = 6;

	private float servant_;

	public const int TacticFieldNumber = 7;

	private float tactic_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<WarRoomAttrFromInfo> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeWarRoomAttrFromInfoReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public float SkillBase
	{
		get
		{
			return skillBase_;
		}
		set
		{
			skillBase_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public float WarRoom
	{
		get
		{
			return warRoom_;
		}
		set
		{
			warRoom_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public float EquipSlot
	{
		get
		{
			return equipSlot_;
		}
		set
		{
			equipSlot_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public float Hero
	{
		get
		{
			return hero_;
		}
		set
		{
			hero_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public float Gun
	{
		get
		{
			return gun_;
		}
		set
		{
			gun_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public float Servant
	{
		get
		{
			return servant_;
		}
		set
		{
			servant_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public float Tactic
	{
		get
		{
			return tactic_;
		}
		set
		{
			tactic_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public WarRoomAttrFromInfo()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public WarRoomAttrFromInfo(WarRoomAttrFromInfo other)
		: this()
	{
		skillBase_ = other.skillBase_;
		warRoom_ = other.warRoom_;
		equipSlot_ = other.equipSlot_;
		hero_ = other.hero_;
		gun_ = other.gun_;
		servant_ = other.servant_;
		tactic_ = other.tactic_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public WarRoomAttrFromInfo Clone()
	{
		return new WarRoomAttrFromInfo(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as WarRoomAttrFromInfo);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(WarRoomAttrFromInfo other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!ProtobufEqualityComparers.BitwiseSingleEqualityComparer.Equals(SkillBase, other.SkillBase))
		{
			return false;
		}
		if (!ProtobufEqualityComparers.BitwiseSingleEqualityComparer.Equals(WarRoom, other.WarRoom))
		{
			return false;
		}
		if (!ProtobufEqualityComparers.BitwiseSingleEqualityComparer.Equals(EquipSlot, other.EquipSlot))
		{
			return false;
		}
		if (!ProtobufEqualityComparers.BitwiseSingleEqualityComparer.Equals(Hero, other.Hero))
		{
			return false;
		}
		if (!ProtobufEqualityComparers.BitwiseSingleEqualityComparer.Equals(Gun, other.Gun))
		{
			return false;
		}
		if (!ProtobufEqualityComparers.BitwiseSingleEqualityComparer.Equals(Servant, other.Servant))
		{
			return false;
		}
		if (!ProtobufEqualityComparers.BitwiseSingleEqualityComparer.Equals(Tactic, other.Tactic))
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
		if (SkillBase != 0f)
		{
			num ^= ProtobufEqualityComparers.BitwiseSingleEqualityComparer.GetHashCode(SkillBase);
		}
		if (WarRoom != 0f)
		{
			num ^= ProtobufEqualityComparers.BitwiseSingleEqualityComparer.GetHashCode(WarRoom);
		}
		if (EquipSlot != 0f)
		{
			num ^= ProtobufEqualityComparers.BitwiseSingleEqualityComparer.GetHashCode(EquipSlot);
		}
		if (Hero != 0f)
		{
			num ^= ProtobufEqualityComparers.BitwiseSingleEqualityComparer.GetHashCode(Hero);
		}
		if (Gun != 0f)
		{
			num ^= ProtobufEqualityComparers.BitwiseSingleEqualityComparer.GetHashCode(Gun);
		}
		if (Servant != 0f)
		{
			num ^= ProtobufEqualityComparers.BitwiseSingleEqualityComparer.GetHashCode(Servant);
		}
		if (Tactic != 0f)
		{
			num ^= ProtobufEqualityComparers.BitwiseSingleEqualityComparer.GetHashCode(Tactic);
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
		if (SkillBase != 0f)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)13);
			((WriteContext)(ref output)).WriteFloat(SkillBase);
		}
		if (WarRoom != 0f)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)21);
			((WriteContext)(ref output)).WriteFloat(WarRoom);
		}
		if (EquipSlot != 0f)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)29);
			((WriteContext)(ref output)).WriteFloat(EquipSlot);
		}
		if (Hero != 0f)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)37);
			((WriteContext)(ref output)).WriteFloat(Hero);
		}
		if (Gun != 0f)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)45);
			((WriteContext)(ref output)).WriteFloat(Gun);
		}
		if (Servant != 0f)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)53);
			((WriteContext)(ref output)).WriteFloat(Servant);
		}
		if (Tactic != 0f)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)61);
			((WriteContext)(ref output)).WriteFloat(Tactic);
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
		if (SkillBase != 0f)
		{
			num += 5;
		}
		if (WarRoom != 0f)
		{
			num += 5;
		}
		if (EquipSlot != 0f)
		{
			num += 5;
		}
		if (Hero != 0f)
		{
			num += 5;
		}
		if (Gun != 0f)
		{
			num += 5;
		}
		if (Servant != 0f)
		{
			num += 5;
		}
		if (Tactic != 0f)
		{
			num += 5;
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(WarRoomAttrFromInfo other)
	{
		if (other != null)
		{
			if (other.SkillBase != 0f)
			{
				SkillBase = other.SkillBase;
			}
			if (other.WarRoom != 0f)
			{
				WarRoom = other.WarRoom;
			}
			if (other.EquipSlot != 0f)
			{
				EquipSlot = other.EquipSlot;
			}
			if (other.Hero != 0f)
			{
				Hero = other.Hero;
			}
			if (other.Gun != 0f)
			{
				Gun = other.Gun;
			}
			if (other.Servant != 0f)
			{
				Servant = other.Servant;
			}
			if (other.Tactic != 0f)
			{
				Tactic = other.Tactic;
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
			case 13u:
				SkillBase = ((ParseContext)(ref input)).ReadFloat();
				break;
			case 21u:
				WarRoom = ((ParseContext)(ref input)).ReadFloat();
				break;
			case 29u:
				EquipSlot = ((ParseContext)(ref input)).ReadFloat();
				break;
			case 37u:
				Hero = ((ParseContext)(ref input)).ReadFloat();
				break;
			case 45u:
				Gun = ((ParseContext)(ref input)).ReadFloat();
				break;
			case 53u:
				Servant = ((ParseContext)(ref input)).ReadFloat();
				break;
			case 61u:
				Tactic = ((ParseContext)(ref input)).ReadFloat();
				break;
			}
		}
	}
}
