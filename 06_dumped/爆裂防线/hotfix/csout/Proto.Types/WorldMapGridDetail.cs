using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class WorldMapGridDetail : IMessage<WorldMapGridDetail>, IMessage, IEquatable<WorldMapGridDetail>, IDeepCloneable<WorldMapGridDetail>, IBufferMessage
{
	private static readonly MessageParser<WorldMapGridDetail> _parser = new MessageParser<WorldMapGridDetail>((Func<WorldMapGridDetail>)(() => new WorldMapGridDetail()));

	private UnknownFieldSet _unknownFields;

	public const int GridIdFieldNumber = 1;

	private int gridId_;

	public const int XFieldNumber = 2;

	private int x_;

	public const int YFieldNumber = 3;

	private int y_;

	public const int BuildingIdFieldNumber = 4;

	private int buildingId_;

	public const int StateFieldNumber = 5;

	private EWorldMapGridState state_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<WorldMapGridDetail> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeWorldMapGridDetailReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

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
	public int X
	{
		get
		{
			return x_;
		}
		set
		{
			x_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int Y
	{
		get
		{
			return y_;
		}
		set
		{
			y_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int BuildingId
	{
		get
		{
			return buildingId_;
		}
		set
		{
			buildingId_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public EWorldMapGridState State
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
	public WorldMapGridDetail()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public WorldMapGridDetail(WorldMapGridDetail other)
		: this()
	{
		gridId_ = other.gridId_;
		x_ = other.x_;
		y_ = other.y_;
		buildingId_ = other.buildingId_;
		state_ = other.state_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public WorldMapGridDetail Clone()
	{
		return new WorldMapGridDetail(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as WorldMapGridDetail);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(WorldMapGridDetail other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (GridId != other.GridId)
		{
			return false;
		}
		if (X != other.X)
		{
			return false;
		}
		if (Y != other.Y)
		{
			return false;
		}
		if (BuildingId != other.BuildingId)
		{
			return false;
		}
		if (State != other.State)
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
		if (GridId != 0)
		{
			num ^= GridId.GetHashCode();
		}
		if (X != 0)
		{
			num ^= X.GetHashCode();
		}
		if (Y != 0)
		{
			num ^= Y.GetHashCode();
		}
		if (BuildingId != 0)
		{
			num ^= BuildingId.GetHashCode();
		}
		if (State != EWorldMapGridState.Invisible)
		{
			num ^= State.GetHashCode();
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
		if (GridId != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteInt32(GridId);
		}
		if (X != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteInt32(X);
		}
		if (Y != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)24);
			((WriteContext)(ref output)).WriteInt32(Y);
		}
		if (BuildingId != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)32);
			((WriteContext)(ref output)).WriteInt32(BuildingId);
		}
		if (State != EWorldMapGridState.Invisible)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)40);
			((WriteContext)(ref output)).WriteEnum((int)State);
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
		if (GridId != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(GridId);
		}
		if (X != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(X);
		}
		if (Y != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(Y);
		}
		if (BuildingId != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(BuildingId);
		}
		if (State != EWorldMapGridState.Invisible)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)State);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(WorldMapGridDetail other)
	{
		if (other != null)
		{
			if (other.GridId != 0)
			{
				GridId = other.GridId;
			}
			if (other.X != 0)
			{
				X = other.X;
			}
			if (other.Y != 0)
			{
				Y = other.Y;
			}
			if (other.BuildingId != 0)
			{
				BuildingId = other.BuildingId;
			}
			if (other.State != EWorldMapGridState.Invisible)
			{
				State = other.State;
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
				GridId = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 16u:
				X = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 24u:
				Y = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 32u:
				BuildingId = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 40u:
				State = (EWorldMapGridState)((ParseContext)(ref input)).ReadEnum();
				break;
			}
		}
	}
}
