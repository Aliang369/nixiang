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
public sealed class DungeonMapCellDetail : IMessage<DungeonMapCellDetail>, IMessage, IEquatable<DungeonMapCellDetail>, IDeepCloneable<DungeonMapCellDetail>, IBufferMessage
{
	private static readonly MessageParser<DungeonMapCellDetail> _parser = new MessageParser<DungeonMapCellDetail>((Func<DungeonMapCellDetail>)(() => new DungeonMapCellDetail()));

	private UnknownFieldSet _unknownFields;

	public const int CellIdFieldNumber = 1;

	private string cellId_ = "";

	public const int CellTypeFieldNumber = 2;

	private int cellType_;

	public const int CellStyleFieldNumber = 3;

	private int cellStyle_;

	public const int FunTypeFieldNumber = 4;

	private EDungeonMapCellFunType funType_;

	public const int XFieldNumber = 5;

	private int x_;

	public const int YFieldNumber = 6;

	private int y_;

	public const int StateFieldNumber = 7;

	private EDungeonMapCellState state_;

	public const int GridListFieldNumber = 8;

	private static readonly FieldCodec<DungeonMapGridDetail> _repeated_gridList_codec = FieldCodec.ForMessage<DungeonMapGridDetail>(66u, DungeonMapGridDetail.Parser);

	private readonly RepeatedField<DungeonMapGridDetail> gridList_ = new RepeatedField<DungeonMapGridDetail>();

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<DungeonMapCellDetail> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeDungeonMapCellDetailReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public string CellId
	{
		get
		{
			return cellId_;
		}
		set
		{
			cellId_ = ProtoPreconditions.CheckNotNull<string>(value, "value");
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int CellType
	{
		get
		{
			return cellType_;
		}
		set
		{
			cellType_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int CellStyle
	{
		get
		{
			return cellStyle_;
		}
		set
		{
			cellStyle_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public EDungeonMapCellFunType FunType
	{
		get
		{
			return funType_;
		}
		set
		{
			funType_ = value;
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
	public EDungeonMapCellState State
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
	public RepeatedField<DungeonMapGridDetail> GridList => gridList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public DungeonMapCellDetail()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public DungeonMapCellDetail(DungeonMapCellDetail other)
		: this()
	{
		cellId_ = other.cellId_;
		cellType_ = other.cellType_;
		cellStyle_ = other.cellStyle_;
		funType_ = other.funType_;
		x_ = other.x_;
		y_ = other.y_;
		state_ = other.state_;
		gridList_ = other.gridList_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public DungeonMapCellDetail Clone()
	{
		return new DungeonMapCellDetail(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as DungeonMapCellDetail);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(DungeonMapCellDetail other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (CellId != other.CellId)
		{
			return false;
		}
		if (CellType != other.CellType)
		{
			return false;
		}
		if (CellStyle != other.CellStyle)
		{
			return false;
		}
		if (FunType != other.FunType)
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
		if (State != other.State)
		{
			return false;
		}
		if (!gridList_.Equals(other.gridList_))
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
		if (CellId.Length != 0)
		{
			num ^= CellId.GetHashCode();
		}
		if (CellType != 0)
		{
			num ^= CellType.GetHashCode();
		}
		if (CellStyle != 0)
		{
			num ^= CellStyle.GetHashCode();
		}
		if (FunType != EDungeonMapCellFunType.EmptyValue)
		{
			num ^= FunType.GetHashCode();
		}
		if (X != 0)
		{
			num ^= X.GetHashCode();
		}
		if (Y != 0)
		{
			num ^= Y.GetHashCode();
		}
		if (State != EDungeonMapCellState.Invisible)
		{
			num ^= State.GetHashCode();
		}
		num ^= ((object)gridList_).GetHashCode();
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
		if (CellId.Length != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)10);
			((WriteContext)(ref output)).WriteString(CellId);
		}
		if (CellType != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteInt32(CellType);
		}
		if (CellStyle != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)24);
			((WriteContext)(ref output)).WriteInt32(CellStyle);
		}
		if (FunType != EDungeonMapCellFunType.EmptyValue)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)32);
			((WriteContext)(ref output)).WriteEnum((int)FunType);
		}
		if (X != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)40);
			((WriteContext)(ref output)).WriteInt32(X);
		}
		if (Y != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)48);
			((WriteContext)(ref output)).WriteInt32(Y);
		}
		if (State != EDungeonMapCellState.Invisible)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)56);
			((WriteContext)(ref output)).WriteEnum((int)State);
		}
		gridList_.WriteTo(ref output, _repeated_gridList_codec);
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
		if (CellId.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(CellId);
		}
		if (CellType != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(CellType);
		}
		if (CellStyle != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(CellStyle);
		}
		if (FunType != EDungeonMapCellFunType.EmptyValue)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)FunType);
		}
		if (X != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(X);
		}
		if (Y != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(Y);
		}
		if (State != EDungeonMapCellState.Invisible)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)State);
		}
		num += gridList_.CalculateSize(_repeated_gridList_codec);
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(DungeonMapCellDetail other)
	{
		if (other != null)
		{
			if (other.CellId.Length != 0)
			{
				CellId = other.CellId;
			}
			if (other.CellType != 0)
			{
				CellType = other.CellType;
			}
			if (other.CellStyle != 0)
			{
				CellStyle = other.CellStyle;
			}
			if (other.FunType != EDungeonMapCellFunType.EmptyValue)
			{
				FunType = other.FunType;
			}
			if (other.X != 0)
			{
				X = other.X;
			}
			if (other.Y != 0)
			{
				Y = other.Y;
			}
			if (other.State != EDungeonMapCellState.Invisible)
			{
				State = other.State;
			}
			gridList_.Add((IEnumerable<DungeonMapGridDetail>)other.gridList_);
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
				CellId = ((ParseContext)(ref input)).ReadString();
				break;
			case 16u:
				CellType = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 24u:
				CellStyle = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 32u:
				FunType = (EDungeonMapCellFunType)((ParseContext)(ref input)).ReadEnum();
				break;
			case 40u:
				X = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 48u:
				Y = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 56u:
				State = (EDungeonMapCellState)((ParseContext)(ref input)).ReadEnum();
				break;
			case 66u:
				gridList_.AddEntriesFrom(ref input, _repeated_gridList_codec);
				break;
			}
		}
	}
}
