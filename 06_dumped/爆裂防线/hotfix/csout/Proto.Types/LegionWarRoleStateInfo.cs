using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class LegionWarRoleStateInfo : IMessage<LegionWarRoleStateInfo>, IMessage, IEquatable<LegionWarRoleStateInfo>, IDeepCloneable<LegionWarRoleStateInfo>, IBufferMessage
{
	private static readonly MessageParser<LegionWarRoleStateInfo> _parser = new MessageParser<LegionWarRoleStateInfo>((Func<LegionWarRoleStateInfo>)(() => new LegionWarRoleStateInfo()));

	private UnknownFieldSet _unknownFields;

	public const int ForceIdFieldNumber = 1;

	private ELegionWarForce forceId_;

	public const int ActionLimitFieldNumber = 2;

	private int actionLimit_;

	public const int ActionPointsFieldNumber = 3;

	private int actionPoints_;

	public const int ProtectTmFieldNumber = 4;

	private long protectTm_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<LegionWarRoleStateInfo> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeLegionWarRoleStateInfoReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ELegionWarForce ForceId
	{
		get
		{
			return forceId_;
		}
		set
		{
			forceId_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int ActionLimit
	{
		get
		{
			return actionLimit_;
		}
		set
		{
			actionLimit_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int ActionPoints
	{
		get
		{
			return actionPoints_;
		}
		set
		{
			actionPoints_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public long ProtectTm
	{
		get
		{
			return protectTm_;
		}
		set
		{
			protectTm_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public LegionWarRoleStateInfo()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public LegionWarRoleStateInfo(LegionWarRoleStateInfo other)
		: this()
	{
		forceId_ = other.forceId_;
		actionLimit_ = other.actionLimit_;
		actionPoints_ = other.actionPoints_;
		protectTm_ = other.protectTm_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public LegionWarRoleStateInfo Clone()
	{
		return new LegionWarRoleStateInfo(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as LegionWarRoleStateInfo);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(LegionWarRoleStateInfo other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (ForceId != other.ForceId)
		{
			return false;
		}
		if (ActionLimit != other.ActionLimit)
		{
			return false;
		}
		if (ActionPoints != other.ActionPoints)
		{
			return false;
		}
		if (ProtectTm != other.ProtectTm)
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
		if (ForceId != ELegionWarForce.EmptyValue)
		{
			num ^= ForceId.GetHashCode();
		}
		if (ActionLimit != 0)
		{
			num ^= ActionLimit.GetHashCode();
		}
		if (ActionPoints != 0)
		{
			num ^= ActionPoints.GetHashCode();
		}
		if (ProtectTm != 0L)
		{
			num ^= ProtectTm.GetHashCode();
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
		if (ForceId != ELegionWarForce.EmptyValue)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteEnum((int)ForceId);
		}
		if (ActionLimit != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteInt32(ActionLimit);
		}
		if (ActionPoints != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)24);
			((WriteContext)(ref output)).WriteInt32(ActionPoints);
		}
		if (ProtectTm != 0L)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)32);
			((WriteContext)(ref output)).WriteInt64(ProtectTm);
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
		if (ForceId != ELegionWarForce.EmptyValue)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)ForceId);
		}
		if (ActionLimit != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(ActionLimit);
		}
		if (ActionPoints != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(ActionPoints);
		}
		if (ProtectTm != 0L)
		{
			num += 1 + CodedOutputStream.ComputeInt64Size(ProtectTm);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(LegionWarRoleStateInfo other)
	{
		if (other != null)
		{
			if (other.ForceId != ELegionWarForce.EmptyValue)
			{
				ForceId = other.ForceId;
			}
			if (other.ActionLimit != 0)
			{
				ActionLimit = other.ActionLimit;
			}
			if (other.ActionPoints != 0)
			{
				ActionPoints = other.ActionPoints;
			}
			if (other.ProtectTm != 0L)
			{
				ProtectTm = other.ProtectTm;
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
				ForceId = (ELegionWarForce)((ParseContext)(ref input)).ReadEnum();
				break;
			case 16u:
				ActionLimit = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 24u:
				ActionPoints = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 32u:
				ProtectTm = ((ParseContext)(ref input)).ReadInt64();
				break;
			}
		}
	}
}
