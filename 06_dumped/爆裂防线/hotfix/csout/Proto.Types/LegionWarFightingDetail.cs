using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class LegionWarFightingDetail : IMessage<LegionWarFightingDetail>, IMessage, IEquatable<LegionWarFightingDetail>, IDeepCloneable<LegionWarFightingDetail>, IBufferMessage
{
	private static readonly MessageParser<LegionWarFightingDetail> _parser = new MessageParser<LegionWarFightingDetail>((Func<LegionWarFightingDetail>)(() => new LegionWarFightingDetail()));

	private UnknownFieldSet _unknownFields;

	public const int AttackerRoleIdFieldNumber = 1;

	private int attackerRoleId_;

	public const int DefenderRoleIdFieldNumber = 2;

	private int defenderRoleId_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<LegionWarFightingDetail> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeLegionWarFightingDetailReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int AttackerRoleId
	{
		get
		{
			return attackerRoleId_;
		}
		set
		{
			attackerRoleId_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int DefenderRoleId
	{
		get
		{
			return defenderRoleId_;
		}
		set
		{
			defenderRoleId_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public LegionWarFightingDetail()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public LegionWarFightingDetail(LegionWarFightingDetail other)
		: this()
	{
		attackerRoleId_ = other.attackerRoleId_;
		defenderRoleId_ = other.defenderRoleId_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public LegionWarFightingDetail Clone()
	{
		return new LegionWarFightingDetail(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as LegionWarFightingDetail);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(LegionWarFightingDetail other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (AttackerRoleId != other.AttackerRoleId)
		{
			return false;
		}
		if (DefenderRoleId != other.DefenderRoleId)
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
		if (AttackerRoleId != 0)
		{
			num ^= AttackerRoleId.GetHashCode();
		}
		if (DefenderRoleId != 0)
		{
			num ^= DefenderRoleId.GetHashCode();
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
		if (AttackerRoleId != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteInt32(AttackerRoleId);
		}
		if (DefenderRoleId != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteInt32(DefenderRoleId);
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
		if (AttackerRoleId != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(AttackerRoleId);
		}
		if (DefenderRoleId != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(DefenderRoleId);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(LegionWarFightingDetail other)
	{
		if (other != null)
		{
			if (other.AttackerRoleId != 0)
			{
				AttackerRoleId = other.AttackerRoleId;
			}
			if (other.DefenderRoleId != 0)
			{
				DefenderRoleId = other.DefenderRoleId;
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
				AttackerRoleId = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 16u:
				DefenderRoleId = ((ParseContext)(ref input)).ReadInt32();
				break;
			}
		}
	}
}
