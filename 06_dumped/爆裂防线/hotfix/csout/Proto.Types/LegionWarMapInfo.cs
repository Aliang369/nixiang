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
public sealed class LegionWarMapInfo : IMessage<LegionWarMapInfo>, IMessage, IEquatable<LegionWarMapInfo>, IDeepCloneable<LegionWarMapInfo>, IBufferMessage
{
	private static readonly MessageParser<LegionWarMapInfo> _parser = new MessageParser<LegionWarMapInfo>((Func<LegionWarMapInfo>)(() => new LegionWarMapInfo()));

	private UnknownFieldSet _unknownFields;

	public const int WarStateFieldNumber = 1;

	private ELegionWarState warState_;

	public const int SettleTmFieldNumber = 2;

	private long settleTm_;

	public const int WinForceIdFieldNumber = 3;

	private ELegionWarForce winForceId_;

	public const int ForceStateListFieldNumber = 4;

	private static readonly FieldCodec<LegionWarForceStateDetail> _repeated_forceStateList_codec = FieldCodec.ForMessage<LegionWarForceStateDetail>(34u, LegionWarForceStateDetail.Parser);

	private readonly RepeatedField<LegionWarForceStateDetail> forceStateList_ = new RepeatedField<LegionWarForceStateDetail>();

	public const int PointStateListFieldNumber = 5;

	private static readonly FieldCodec<LegionWarPointStateDetail> _repeated_pointStateList_codec = FieldCodec.ForMessage<LegionWarPointStateDetail>(42u, LegionWarPointStateDetail.Parser);

	private readonly RepeatedField<LegionWarPointStateDetail> pointStateList_ = new RepeatedField<LegionWarPointStateDetail>();

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<LegionWarMapInfo> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeLegionWarMapInfoReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ELegionWarState WarState
	{
		get
		{
			return warState_;
		}
		set
		{
			warState_ = value;
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
	public RepeatedField<LegionWarForceStateDetail> ForceStateList => forceStateList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<LegionWarPointStateDetail> PointStateList => pointStateList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public LegionWarMapInfo()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public LegionWarMapInfo(LegionWarMapInfo other)
		: this()
	{
		warState_ = other.warState_;
		settleTm_ = other.settleTm_;
		winForceId_ = other.winForceId_;
		forceStateList_ = other.forceStateList_.Clone();
		pointStateList_ = other.pointStateList_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public LegionWarMapInfo Clone()
	{
		return new LegionWarMapInfo(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as LegionWarMapInfo);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(LegionWarMapInfo other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (WarState != other.WarState)
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
		if (!forceStateList_.Equals(other.forceStateList_))
		{
			return false;
		}
		if (!pointStateList_.Equals(other.pointStateList_))
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
		if (WarState != ELegionWarState.EmptyValue)
		{
			num ^= WarState.GetHashCode();
		}
		if (SettleTm != 0L)
		{
			num ^= SettleTm.GetHashCode();
		}
		if (WinForceId != ELegionWarForce.EmptyValue)
		{
			num ^= WinForceId.GetHashCode();
		}
		num ^= ((object)forceStateList_).GetHashCode();
		num ^= ((object)pointStateList_).GetHashCode();
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
		if (WarState != ELegionWarState.EmptyValue)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteEnum((int)WarState);
		}
		if (SettleTm != 0L)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteInt64(SettleTm);
		}
		if (WinForceId != ELegionWarForce.EmptyValue)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)24);
			((WriteContext)(ref output)).WriteEnum((int)WinForceId);
		}
		forceStateList_.WriteTo(ref output, _repeated_forceStateList_codec);
		pointStateList_.WriteTo(ref output, _repeated_pointStateList_codec);
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
		if (WarState != ELegionWarState.EmptyValue)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)WarState);
		}
		if (SettleTm != 0L)
		{
			num += 1 + CodedOutputStream.ComputeInt64Size(SettleTm);
		}
		if (WinForceId != ELegionWarForce.EmptyValue)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)WinForceId);
		}
		num += forceStateList_.CalculateSize(_repeated_forceStateList_codec);
		num += pointStateList_.CalculateSize(_repeated_pointStateList_codec);
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(LegionWarMapInfo other)
	{
		if (other != null)
		{
			if (other.WarState != ELegionWarState.EmptyValue)
			{
				WarState = other.WarState;
			}
			if (other.SettleTm != 0L)
			{
				SettleTm = other.SettleTm;
			}
			if (other.WinForceId != ELegionWarForce.EmptyValue)
			{
				WinForceId = other.WinForceId;
			}
			forceStateList_.Add((IEnumerable<LegionWarForceStateDetail>)other.forceStateList_);
			pointStateList_.Add((IEnumerable<LegionWarPointStateDetail>)other.pointStateList_);
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
				WarState = (ELegionWarState)((ParseContext)(ref input)).ReadEnum();
				break;
			case 16u:
				SettleTm = ((ParseContext)(ref input)).ReadInt64();
				break;
			case 24u:
				WinForceId = (ELegionWarForce)((ParseContext)(ref input)).ReadEnum();
				break;
			case 34u:
				forceStateList_.AddEntriesFrom(ref input, _repeated_forceStateList_codec);
				break;
			case 42u:
				pointStateList_.AddEntriesFrom(ref input, _repeated_pointStateList_codec);
				break;
			}
		}
	}
}
