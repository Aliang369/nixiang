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
public sealed class LegionWarSettlePlayerInfoAck : IMessage<LegionWarSettlePlayerInfoAck>, IMessage, IEquatable<LegionWarSettlePlayerInfoAck>, IDeepCloneable<LegionWarSettlePlayerInfoAck>, IBufferMessage
{
	private static readonly MessageParser<LegionWarSettlePlayerInfoAck> _parser = new MessageParser<LegionWarSettlePlayerInfoAck>((Func<LegionWarSettlePlayerInfoAck>)(() => new LegionWarSettlePlayerInfoAck()));

	private UnknownFieldSet _unknownFields;

	public const int MaxZoneFieldNumber = 1;

	private int maxZone_;

	public const int SettleZoneFieldNumber = 2;

	private int settleZone_;

	public const int SettleTmFieldNumber = 3;

	private long settleTm_;

	public const int WinForceIdFieldNumber = 4;

	private ELegionWarForce winForceId_;

	public const int ForceListFieldNumber = 5;

	private static readonly FieldCodec<LegionWarSettlePlayerInfo> _repeated_forceList_codec = FieldCodec.ForMessage<LegionWarSettlePlayerInfo>(42u, LegionWarSettlePlayerInfo.Parser);

	private readonly RepeatedField<LegionWarSettlePlayerInfo> forceList_ = new RepeatedField<LegionWarSettlePlayerInfo>();

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<LegionWarSettlePlayerInfoAck> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeLegionWarSettlePlayerInfoAckReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int MaxZone
	{
		get
		{
			return maxZone_;
		}
		set
		{
			maxZone_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int SettleZone
	{
		get
		{
			return settleZone_;
		}
		set
		{
			settleZone_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public long SettleTm
	{
		get
		{
			return settleTm_;
		}
		set
		{
			settleTm_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ELegionWarForce WinForceId
	{
		get
		{
			return winForceId_;
		}
		set
		{
			winForceId_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<LegionWarSettlePlayerInfo> ForceList => forceList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public LegionWarSettlePlayerInfoAck()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public LegionWarSettlePlayerInfoAck(LegionWarSettlePlayerInfoAck other)
		: this()
	{
		maxZone_ = other.maxZone_;
		settleZone_ = other.settleZone_;
		settleTm_ = other.settleTm_;
		winForceId_ = other.winForceId_;
		forceList_ = other.forceList_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public LegionWarSettlePlayerInfoAck Clone()
	{
		return new LegionWarSettlePlayerInfoAck(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as LegionWarSettlePlayerInfoAck);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(LegionWarSettlePlayerInfoAck other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (MaxZone != other.MaxZone)
		{
			return false;
		}
		if (SettleZone != other.SettleZone)
		{
			return false;
		}
		if (SettleTm != other.SettleTm)
		{
			return false;
		}
		if (WinForceId != other.WinForceId)
		{
			return false;
		}
		if (!forceList_.Equals(other.forceList_))
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
		if (MaxZone != 0)
		{
			num ^= MaxZone.GetHashCode();
		}
		if (SettleZone != 0)
		{
			num ^= SettleZone.GetHashCode();
		}
		if (SettleTm != 0L)
		{
			num ^= SettleTm.GetHashCode();
		}
		if (WinForceId != ELegionWarForce.EmptyValue)
		{
			num ^= WinForceId.GetHashCode();
		}
		num ^= ((object)forceList_).GetHashCode();
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
		if (MaxZone != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteInt32(MaxZone);
		}
		if (SettleZone != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteInt32(SettleZone);
		}
		if (SettleTm != 0L)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)24);
			((WriteContext)(ref output)).WriteInt64(SettleTm);
		}
		if (WinForceId != ELegionWarForce.EmptyValue)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)32);
			((WriteContext)(ref output)).WriteEnum((int)WinForceId);
		}
		forceList_.WriteTo(ref output, _repeated_forceList_codec);
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
		if (MaxZone != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(MaxZone);
		}
		if (SettleZone != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(SettleZone);
		}
		if (SettleTm != 0L)
		{
			num += 1 + CodedOutputStream.ComputeInt64Size(SettleTm);
		}
		if (WinForceId != ELegionWarForce.EmptyValue)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)WinForceId);
		}
		num += forceList_.CalculateSize(_repeated_forceList_codec);
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(LegionWarSettlePlayerInfoAck other)
	{
		if (other != null)
		{
			if (other.MaxZone != 0)
			{
				MaxZone = other.MaxZone;
			}
			if (other.SettleZone != 0)
			{
				SettleZone = other.SettleZone;
			}
			if (other.SettleTm != 0L)
			{
				SettleTm = other.SettleTm;
			}
			if (other.WinForceId != ELegionWarForce.EmptyValue)
			{
				WinForceId = other.WinForceId;
			}
			forceList_.Add((IEnumerable<LegionWarSettlePlayerInfo>)other.forceList_);
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
				MaxZone = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 16u:
				SettleZone = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 24u:
				SettleTm = ((ParseContext)(ref input)).ReadInt64();
				break;
			case 32u:
				WinForceId = (ELegionWarForce)((ParseContext)(ref input)).ReadEnum();
				break;
			case 42u:
				forceList_.AddEntriesFrom(ref input, _repeated_forceList_codec);
				break;
			}
		}
	}
}
