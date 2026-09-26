using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class DungeonMapInfo : IMessage<DungeonMapInfo>, IMessage, IEquatable<DungeonMapInfo>, IDeepCloneable<DungeonMapInfo>, IBufferMessage
{
	private static readonly MessageParser<DungeonMapInfo> _parser = new MessageParser<DungeonMapInfo>((Func<DungeonMapInfo>)(() => new DungeonMapInfo()));

	private UnknownFieldSet _unknownFields;

	public const int MaxXFieldNumber = 1;

	private int maxX_;

	public const int MaxYFieldNumber = 2;

	private int maxY_;

	public const int CellListFieldNumber = 3;

	private static readonly FieldCodec<DungeonMapCellDetail> _repeated_cellList_codec = FieldCodec.ForMessage<DungeonMapCellDetail>(26u, DungeonMapCellDetail.Parser);

	private readonly RepeatedField<DungeonMapCellDetail> cellList_ = new RepeatedField<DungeonMapCellDetail>();

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<DungeonMapInfo> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeDungeonMapInfoReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int MaxX
	{
		get
		{
			return maxX_;
		}
		set
		{
			maxX_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int MaxY
	{
		get
		{
			return maxY_;
		}
		set
		{
			maxY_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<DungeonMapCellDetail> CellList => cellList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public DungeonMapInfo()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public DungeonMapInfo(DungeonMapInfo other)
		: this()
	{
		maxX_ = other.maxX_;
		maxY_ = other.maxY_;
		cellList_ = other.cellList_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public DungeonMapInfo Clone()
	{
		return new DungeonMapInfo(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as DungeonMapInfo);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(DungeonMapInfo other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (MaxX != other.MaxX)
		{
			return false;
		}
		if (MaxY != other.MaxY)
		{
			return false;
		}
		if (!cellList_.Equals(other.cellList_))
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
		if (MaxX != 0)
		{
			num ^= MaxX.GetHashCode();
		}
		if (MaxY != 0)
		{
			num ^= MaxY.GetHashCode();
		}
		num ^= ((object)cellList_).GetHashCode();
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
		if (MaxX != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteInt32(MaxX);
		}
		if (MaxY != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteInt32(MaxY);
		}
		cellList_.WriteTo(ref output, _repeated_cellList_codec);
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
		if (MaxX != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(MaxX);
		}
		if (MaxY != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(MaxY);
		}
		num += cellList_.CalculateSize(_repeated_cellList_codec);
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(DungeonMapInfo other)
	{
		if (other != null)
		{
			if (other.MaxX != 0)
			{
				MaxX = other.MaxX;
			}
			if (other.MaxY != 0)
			{
				MaxY = other.MaxY;
			}
			cellList_.Add((IEnumerable<DungeonMapCellDetail>)other.cellList_);
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
				MaxX = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 16u:
				MaxY = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 26u:
				cellList_.AddEntriesFrom(ref input, _repeated_cellList_codec);
				break;
			}
		}
	}
}
