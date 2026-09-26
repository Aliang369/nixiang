using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class RoleTipsInfo : IMessage<RoleTipsInfo>, IMessage, IEquatable<RoleTipsInfo>, IDeepCloneable<RoleTipsInfo>, IBufferMessage
{
	private static readonly MessageParser<RoleTipsInfo> _parser = new MessageParser<RoleTipsInfo>((Func<RoleTipsInfo>)(() => new RoleTipsInfo()));

	private UnknownFieldSet _unknownFields;

	public const int RoleBaseInfoFieldNumber = 1;

	private RoleBaseInfo roleBaseInfo_;

	public const int LegionFlagInfoFieldNumber = 2;

	private LegionFlagInfo legionFlagInfo_;

	public const int RoleAuxInfoFieldNumber = 3;

	private RoleAuxInfo roleAuxInfo_;

	public const int EquipSlotInfoFieldNumber = 4;

	private RoleTipsEquipInfo equipSlotInfo_;

	public const int RoleSuitInfoFieldNumber = 5;

	private RoleTipsSuitInfo roleSuitInfo_;

	public const int RoleWallInfoFieldNumber = 6;

	private RoleTipsWallInfo roleWallInfo_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<RoleTipsInfo> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeRoleTipsInfoReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RoleBaseInfo RoleBaseInfo
	{
		get
		{
			return roleBaseInfo_;
		}
		set
		{
			roleBaseInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public LegionFlagInfo LegionFlagInfo
	{
		get
		{
			return legionFlagInfo_;
		}
		set
		{
			legionFlagInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RoleAuxInfo RoleAuxInfo
	{
		get
		{
			return roleAuxInfo_;
		}
		set
		{
			roleAuxInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RoleTipsEquipInfo EquipSlotInfo
	{
		get
		{
			return equipSlotInfo_;
		}
		set
		{
			equipSlotInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RoleTipsSuitInfo RoleSuitInfo
	{
		get
		{
			return roleSuitInfo_;
		}
		set
		{
			roleSuitInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RoleTipsWallInfo RoleWallInfo
	{
		get
		{
			return roleWallInfo_;
		}
		set
		{
			roleWallInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RoleTipsInfo()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RoleTipsInfo(RoleTipsInfo other)
		: this()
	{
		roleBaseInfo_ = ((other.roleBaseInfo_ != null) ? other.roleBaseInfo_.Clone() : null);
		legionFlagInfo_ = ((other.legionFlagInfo_ != null) ? other.legionFlagInfo_.Clone() : null);
		roleAuxInfo_ = ((other.roleAuxInfo_ != null) ? other.roleAuxInfo_.Clone() : null);
		equipSlotInfo_ = ((other.equipSlotInfo_ != null) ? other.equipSlotInfo_.Clone() : null);
		roleSuitInfo_ = ((other.roleSuitInfo_ != null) ? other.roleSuitInfo_.Clone() : null);
		roleWallInfo_ = ((other.roleWallInfo_ != null) ? other.roleWallInfo_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RoleTipsInfo Clone()
	{
		return new RoleTipsInfo(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as RoleTipsInfo);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(RoleTipsInfo other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!object.Equals(RoleBaseInfo, other.RoleBaseInfo))
		{
			return false;
		}
		if (!object.Equals(LegionFlagInfo, other.LegionFlagInfo))
		{
			return false;
		}
		if (!object.Equals(RoleAuxInfo, other.RoleAuxInfo))
		{
			return false;
		}
		if (!object.Equals(EquipSlotInfo, other.EquipSlotInfo))
		{
			return false;
		}
		if (!object.Equals(RoleSuitInfo, other.RoleSuitInfo))
		{
			return false;
		}
		if (!object.Equals(RoleWallInfo, other.RoleWallInfo))
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
		if (roleBaseInfo_ != null)
		{
			num ^= RoleBaseInfo.GetHashCode();
		}
		if (legionFlagInfo_ != null)
		{
			num ^= LegionFlagInfo.GetHashCode();
		}
		if (roleAuxInfo_ != null)
		{
			num ^= RoleAuxInfo.GetHashCode();
		}
		if (equipSlotInfo_ != null)
		{
			num ^= EquipSlotInfo.GetHashCode();
		}
		if (roleSuitInfo_ != null)
		{
			num ^= RoleSuitInfo.GetHashCode();
		}
		if (roleWallInfo_ != null)
		{
			num ^= RoleWallInfo.GetHashCode();
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
		if (roleBaseInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)10);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)RoleBaseInfo);
		}
		if (legionFlagInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)18);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)LegionFlagInfo);
		}
		if (roleAuxInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)26);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)RoleAuxInfo);
		}
		if (equipSlotInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)34);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)EquipSlotInfo);
		}
		if (roleSuitInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)42);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)RoleSuitInfo);
		}
		if (roleWallInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)50);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)RoleWallInfo);
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
		if (roleBaseInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)RoleBaseInfo);
		}
		if (legionFlagInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)LegionFlagInfo);
		}
		if (roleAuxInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)RoleAuxInfo);
		}
		if (equipSlotInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)EquipSlotInfo);
		}
		if (roleSuitInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)RoleSuitInfo);
		}
		if (roleWallInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)RoleWallInfo);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(RoleTipsInfo other)
	{
		if (other == null)
		{
			return;
		}
		if (other.roleBaseInfo_ != null)
		{
			if (roleBaseInfo_ == null)
			{
				RoleBaseInfo = new RoleBaseInfo();
			}
			RoleBaseInfo.MergeFrom(other.RoleBaseInfo);
		}
		if (other.legionFlagInfo_ != null)
		{
			if (legionFlagInfo_ == null)
			{
				LegionFlagInfo = new LegionFlagInfo();
			}
			LegionFlagInfo.MergeFrom(other.LegionFlagInfo);
		}
		if (other.roleAuxInfo_ != null)
		{
			if (roleAuxInfo_ == null)
			{
				RoleAuxInfo = new RoleAuxInfo();
			}
			RoleAuxInfo.MergeFrom(other.RoleAuxInfo);
		}
		if (other.equipSlotInfo_ != null)
		{
			if (equipSlotInfo_ == null)
			{
				EquipSlotInfo = new RoleTipsEquipInfo();
			}
			EquipSlotInfo.MergeFrom(other.EquipSlotInfo);
		}
		if (other.roleSuitInfo_ != null)
		{
			if (roleSuitInfo_ == null)
			{
				RoleSuitInfo = new RoleTipsSuitInfo();
			}
			RoleSuitInfo.MergeFrom(other.RoleSuitInfo);
		}
		if (other.roleWallInfo_ != null)
		{
			if (roleWallInfo_ == null)
			{
				RoleWallInfo = new RoleTipsWallInfo();
			}
			RoleWallInfo.MergeFrom(other.RoleWallInfo);
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
				if (roleBaseInfo_ == null)
				{
					RoleBaseInfo = new RoleBaseInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)RoleBaseInfo);
				break;
			case 18u:
				if (legionFlagInfo_ == null)
				{
					LegionFlagInfo = new LegionFlagInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)LegionFlagInfo);
				break;
			case 26u:
				if (roleAuxInfo_ == null)
				{
					RoleAuxInfo = new RoleAuxInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)RoleAuxInfo);
				break;
			case 34u:
				if (equipSlotInfo_ == null)
				{
					EquipSlotInfo = new RoleTipsEquipInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)EquipSlotInfo);
				break;
			case 42u:
				if (roleSuitInfo_ == null)
				{
					RoleSuitInfo = new RoleTipsSuitInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)RoleSuitInfo);
				break;
			case 50u:
				if (roleWallInfo_ == null)
				{
					RoleWallInfo = new RoleTipsWallInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)RoleWallInfo);
				break;
			}
		}
	}
}
