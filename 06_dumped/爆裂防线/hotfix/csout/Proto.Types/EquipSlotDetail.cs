using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class EquipSlotDetail : IMessage<EquipSlotDetail>, IMessage, IEquatable<EquipSlotDetail>, IDeepCloneable<EquipSlotDetail>, IBufferMessage
{
	private static readonly MessageParser<EquipSlotDetail> _parser = new MessageParser<EquipSlotDetail>((Func<EquipSlotDetail>)(() => new EquipSlotDetail()));

	private UnknownFieldSet _unknownFields;

	public const int EquipTypeFieldNumber = 1;

	private EEquipType equipType_;

	public const int LevelFieldNumber = 2;

	private int level_;

	public const int EquipFieldNumber = 3;

	private EquipDetail equip_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<EquipSlotDetail> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeEquipSlotDetailReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public EEquipType EquipType
	{
		get
		{
			return equipType_;
		}
		set
		{
			equipType_ = value;
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
	public EquipDetail Equip
	{
		get
		{
			return equip_;
		}
		set
		{
			equip_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public EquipSlotDetail()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public EquipSlotDetail(EquipSlotDetail other)
		: this()
	{
		equipType_ = other.equipType_;
		level_ = other.level_;
		equip_ = ((other.equip_ != null) ? other.equip_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public EquipSlotDetail Clone()
	{
		return new EquipSlotDetail(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as EquipSlotDetail);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(EquipSlotDetail other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (EquipType != other.EquipType)
		{
			return false;
		}
		if (Level != other.Level)
		{
			return false;
		}
		if (!object.Equals(Equip, other.Equip))
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
		if (EquipType != EEquipType.EmptyValue)
		{
			num ^= EquipType.GetHashCode();
		}
		if (Level != 0)
		{
			num ^= Level.GetHashCode();
		}
		if (equip_ != null)
		{
			num ^= Equip.GetHashCode();
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
		if (EquipType != EEquipType.EmptyValue)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteEnum((int)EquipType);
		}
		if (Level != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteInt32(Level);
		}
		if (equip_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)26);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)Equip);
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
		if (EquipType != EEquipType.EmptyValue)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)EquipType);
		}
		if (Level != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(Level);
		}
		if (equip_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)Equip);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(EquipSlotDetail other)
	{
		if (other == null)
		{
			return;
		}
		if (other.EquipType != EEquipType.EmptyValue)
		{
			EquipType = other.EquipType;
		}
		if (other.Level != 0)
		{
			Level = other.Level;
		}
		if (other.equip_ != null)
		{
			if (equip_ == null)
			{
				Equip = new EquipDetail();
			}
			Equip.MergeFrom(other.Equip);
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
			case 8u:
				EquipType = (EEquipType)((ParseContext)(ref input)).ReadEnum();
				break;
			case 16u:
				Level = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 26u:
				if (equip_ == null)
				{
					Equip = new EquipDetail();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)Equip);
				break;
			}
		}
	}
}
