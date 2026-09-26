using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class EndlessMineMapRowDetail : IMessage<EndlessMineMapRowDetail>, IMessage, IEquatable<EndlessMineMapRowDetail>, IDeepCloneable<EndlessMineMapRowDetail>, IBufferMessage
{
	private static readonly MessageParser<EndlessMineMapRowDetail> _parser = new MessageParser<EndlessMineMapRowDetail>((Func<EndlessMineMapRowDetail>)(() => new EndlessMineMapRowDetail()));

	private UnknownFieldSet _unknownFields;

	public const int RowFieldNumber = 1;

	private int row_;

	public const int CellListFieldNumber = 2;

	private static readonly FieldCodec<EndlessMineMapCellDetail> _repeated_cellList_codec = FieldCodec.ForMessage<EndlessMineMapCellDetail>(18u, EndlessMineMapCellDetail.Parser);

	private readonly RepeatedField<EndlessMineMapCellDetail> cellList_ = new RepeatedField<EndlessMineMapCellDetail>();

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<EndlessMineMapRowDetail> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeEndlessMineMapRowDetailReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int Row
	{
		get
		{
			return row_;
		}
		set
		{
			row_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<EndlessMineMapCellDetail> CellList => cellList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public EndlessMineMapRowDetail()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public EndlessMineMapRowDetail(EndlessMineMapRowDetail other)
		: this()
	{
		row_ = other.row_;
		cellList_ = other.cellList_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public EndlessMineMapRowDetail Clone()
	{
		return new EndlessMineMapRowDetail(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as EndlessMineMapRowDetail);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(EndlessMineMapRowDetail other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (Row != other.Row)
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
		if (Row != 0)
		{
			num ^= Row.GetHashCode();
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
		if (Row != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteInt32(Row);
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
		if (Row != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(Row);
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
	public void MergeFrom(EndlessMineMapRowDetail other)
	{
		if (other != null)
		{
			if (other.Row != 0)
			{
				Row = other.Row;
			}
			cellList_.Add((IEnumerable<EndlessMineMapCellDetail>)other.cellList_);
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
				Row = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 18u:
				cellList_.AddEntriesFrom(ref input, _repeated_cellList_codec);
				break;
			}
		}
	}
}
