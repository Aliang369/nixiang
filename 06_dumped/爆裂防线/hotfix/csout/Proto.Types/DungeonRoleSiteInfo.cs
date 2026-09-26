using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class DungeonRoleSiteInfo : IMessage<DungeonRoleSiteInfo>, IMessage, IEquatable<DungeonRoleSiteInfo>, IDeepCloneable<DungeonRoleSiteInfo>, IBufferMessage
{
	private static readonly MessageParser<DungeonRoleSiteInfo> _parser = new MessageParser<DungeonRoleSiteInfo>((Func<DungeonRoleSiteInfo>)(() => new DungeonRoleSiteInfo()));

	private UnknownFieldSet _unknownFields;

	public const int CellIdFieldNumber = 1;

	private string cellId_ = "";

	public const int GridIdFieldNumber = 2;

	private int gridId_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<DungeonRoleSiteInfo> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeDungeonRoleSiteInfoReflection.Descriptor.MessageTypes[0];

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
	public DungeonRoleSiteInfo()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public DungeonRoleSiteInfo(DungeonRoleSiteInfo other)
		: this()
	{
		cellId_ = other.cellId_;
		gridId_ = other.gridId_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public DungeonRoleSiteInfo Clone()
	{
		return new DungeonRoleSiteInfo(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as DungeonRoleSiteInfo);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(DungeonRoleSiteInfo other)
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
		if (GridId != other.GridId)
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
		if (GridId != 0)
		{
			num ^= GridId.GetHashCode();
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
		if (CellId.Length != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)10);
			((WriteContext)(ref output)).WriteString(CellId);
		}
		if (GridId != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteInt32(GridId);
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
		if (CellId.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(CellId);
		}
		if (GridId != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(GridId);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(DungeonRoleSiteInfo other)
	{
		if (other != null)
		{
			if (other.CellId.Length != 0)
			{
				CellId = other.CellId;
			}
			if (other.GridId != 0)
			{
				GridId = other.GridId;
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
			case 10u:
				CellId = ((ParseContext)(ref input)).ReadString();
				break;
			case 16u:
				GridId = ((ParseContext)(ref input)).ReadInt32();
				break;
			}
		}
	}
}
