using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class WorldMapInfo : IMessage<WorldMapInfo>, IMessage, IEquatable<WorldMapInfo>, IDeepCloneable<WorldMapInfo>, IBufferMessage
{
	private static readonly MessageParser<WorldMapInfo> _parser = new MessageParser<WorldMapInfo>((Func<WorldMapInfo>)(() => new WorldMapInfo()));

	private UnknownFieldSet _unknownFields;

	public const int MaxColFieldNumber = 1;

	private int maxCol_;

	public const int MaxRowFieldNumber = 2;

	private int maxRow_;

	public const int GridListFieldNumber = 3;

	private static readonly FieldCodec<WorldMapGridDetail> _repeated_gridList_codec = FieldCodec.ForMessage<WorldMapGridDetail>(26u, WorldMapGridDetail.Parser);

	private readonly RepeatedField<WorldMapGridDetail> gridList_ = new RepeatedField<WorldMapGridDetail>();

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<WorldMapInfo> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeWorldMapInfoReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int MaxCol
	{
		get
		{
			return maxCol_;
		}
		set
		{
			maxCol_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int MaxRow
	{
		get
		{
			return maxRow_;
		}
		set
		{
			maxRow_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<WorldMapGridDetail> GridList => gridList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public WorldMapInfo()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public WorldMapInfo(WorldMapInfo other)
		: this()
	{
		maxCol_ = other.maxCol_;
		maxRow_ = other.maxRow_;
		gridList_ = other.gridList_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public WorldMapInfo Clone()
	{
		return new WorldMapInfo(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as WorldMapInfo);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(WorldMapInfo other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (MaxCol != other.MaxCol)
		{
			return false;
		}
		if (MaxRow != other.MaxRow)
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
		if (MaxCol != 0)
		{
			num ^= MaxCol.GetHashCode();
		}
		if (MaxRow != 0)
		{
			num ^= MaxRow.GetHashCode();
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
		if (MaxCol != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteInt32(MaxCol);
		}
		if (MaxRow != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteInt32(MaxRow);
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
		if (MaxCol != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(MaxCol);
		}
		if (MaxRow != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(MaxRow);
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
	public void MergeFrom(WorldMapInfo other)
	{
		if (other != null)
		{
			if (other.MaxCol != 0)
			{
				MaxCol = other.MaxCol;
			}
			if (other.MaxRow != 0)
			{
				MaxRow = other.MaxRow;
			}
			gridList_.Add((IEnumerable<WorldMapGridDetail>)other.gridList_);
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
				MaxCol = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 16u:
				MaxRow = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 26u:
				gridList_.AddEntriesFrom(ref input, _repeated_gridList_codec);
				break;
			}
		}
	}
}
