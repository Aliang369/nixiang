using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class WorldReconDetail : IMessage<WorldReconDetail>, IMessage, IEquatable<WorldReconDetail>, IDeepCloneable<WorldReconDetail>, IBufferMessage
{
	private static readonly MessageParser<WorldReconDetail> _parser = new MessageParser<WorldReconDetail>((Func<WorldReconDetail>)(() => new WorldReconDetail()));

	private UnknownFieldSet _unknownFields;

	public const int ReconIdFieldNumber = 1;

	private int reconId_;

	public const int GridIdFieldNumber = 2;

	private int gridId_;

	public const int StateFieldNumber = 3;

	private EWorldReconState state_;

	public const int StateCdFieldNumber = 4;

	private int stateCd_;

	public const int StateTmFieldNumber = 5;

	private long stateTm_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<WorldReconDetail> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeWorldReconDetailReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int ReconId
	{
		get
		{
			return reconId_;
		}
		set
		{
			reconId_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int GridId
	{
		get
		{
			return gridId_;
		}
		set
		{
			gridId_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public EWorldReconState State
	{
		get
		{
			return state_;
		}
		set
		{
			state_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int StateCd
	{
		get
		{
			return stateCd_;
		}
		set
		{
			stateCd_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public long StateTm
	{
		get
		{
			return stateTm_;
		}
		set
		{
			stateTm_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public WorldReconDetail()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public WorldReconDetail(WorldReconDetail other)
		: this()
	{
		reconId_ = other.reconId_;
		gridId_ = other.gridId_;
		state_ = other.state_;
		stateCd_ = other.stateCd_;
		stateTm_ = other.stateTm_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public WorldReconDetail Clone()
	{
		return new WorldReconDetail(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as WorldReconDetail);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(WorldReconDetail other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (ReconId != other.ReconId)
		{
			return false;
		}
		if (GridId != other.GridId)
		{
			return false;
		}
		if (State != other.State)
		{
			return false;
		}
		if (StateCd != other.StateCd)
		{
			return false;
		}
		if (StateTm != other.StateTm)
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
		if (ReconId != 0)
		{
			num ^= ReconId.GetHashCode();
		}
		if (GridId != 0)
		{
			num ^= GridId.GetHashCode();
		}
		if (State != EWorldReconState.EmptyValue)
		{
			num ^= State.GetHashCode();
		}
		if (StateCd != 0)
		{
			num ^= StateCd.GetHashCode();
		}
		if (StateTm != 0L)
		{
			num ^= StateTm.GetHashCode();
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
		if (ReconId != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteInt32(ReconId);
		}
		if (GridId != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteInt32(GridId);
		}
		if (State != EWorldReconState.EmptyValue)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)24);
			((WriteContext)(ref output)).WriteEnum((int)State);
		}
		if (StateCd != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)32);
			((WriteContext)(ref output)).WriteInt32(StateCd);
		}
		if (StateTm != 0L)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)40);
			((WriteContext)(ref output)).WriteInt64(StateTm);
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
		if (ReconId != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(ReconId);
		}
		if (GridId != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(GridId);
		}
		if (State != EWorldReconState.EmptyValue)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)State);
		}
		if (StateCd != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(StateCd);
		}
		if (StateTm != 0L)
		{
			num += 1 + CodedOutputStream.ComputeInt64Size(StateTm);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(WorldReconDetail other)
	{
		if (other != null)
		{
			if (other.ReconId != 0)
			{
				ReconId = other.ReconId;
			}
			if (other.GridId != 0)
			{
				GridId = other.GridId;
			}
			if (other.State != EWorldReconState.EmptyValue)
			{
				State = other.State;
			}
			if (other.StateCd != 0)
			{
				StateCd = other.StateCd;
			}
			if (other.StateTm != 0L)
			{
				StateTm = other.StateTm;
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
				ReconId = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 16u:
				GridId = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 24u:
				State = (EWorldReconState)((ParseContext)(ref input)).ReadEnum();
				break;
			case 32u:
				StateCd = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 40u:
				StateTm = ((ParseContext)(ref input)).ReadInt64();
				break;
			}
		}
	}
}
