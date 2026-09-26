using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class WorldDungeonOpenCellReq : IMessage<WorldDungeonOpenCellReq>, IMessage, IEquatable<WorldDungeonOpenCellReq>, IDeepCloneable<WorldDungeonOpenCellReq>, IBufferMessage
{
	private static readonly MessageParser<WorldDungeonOpenCellReq> _parser = new MessageParser<WorldDungeonOpenCellReq>((Func<WorldDungeonOpenCellReq>)(() => new WorldDungeonOpenCellReq()));

	private UnknownFieldSet _unknownFields;

	public const int FromCellIdFieldNumber = 1;

	private string fromCellId_ = "";

	public const int OpenCellIdFieldNumber = 2;

	private string openCellId_ = "";

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<WorldDungeonOpenCellReq> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeWorldDungeonOpenCellReqReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public string FromCellId
	{
		get
		{
			return fromCellId_;
		}
		set
		{
			fromCellId_ = ProtoPreconditions.CheckNotNull<string>(value, "value");
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public string OpenCellId
	{
		get
		{
			return openCellId_;
		}
		set
		{
			openCellId_ = ProtoPreconditions.CheckNotNull<string>(value, "value");
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public WorldDungeonOpenCellReq()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public WorldDungeonOpenCellReq(WorldDungeonOpenCellReq other)
		: this()
	{
		fromCellId_ = other.fromCellId_;
		openCellId_ = other.openCellId_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public WorldDungeonOpenCellReq Clone()
	{
		return new WorldDungeonOpenCellReq(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as WorldDungeonOpenCellReq);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(WorldDungeonOpenCellReq other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (FromCellId != other.FromCellId)
		{
			return false;
		}
		if (OpenCellId != other.OpenCellId)
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
		if (FromCellId.Length != 0)
		{
			num ^= FromCellId.GetHashCode();
		}
		if (OpenCellId.Length != 0)
		{
			num ^= OpenCellId.GetHashCode();
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
		if (FromCellId.Length != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)10);
			((WriteContext)(ref output)).WriteString(FromCellId);
		}
		if (OpenCellId.Length != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)18);
			((WriteContext)(ref output)).WriteString(OpenCellId);
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
		if (FromCellId.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(FromCellId);
		}
		if (OpenCellId.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(OpenCellId);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(WorldDungeonOpenCellReq other)
	{
		if (other != null)
		{
			if (other.FromCellId.Length != 0)
			{
				FromCellId = other.FromCellId;
			}
			if (other.OpenCellId.Length != 0)
			{
				OpenCellId = other.OpenCellId;
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
				FromCellId = ((ParseContext)(ref input)).ReadString();
				break;
			case 18u:
				OpenCellId = ((ParseContext)(ref input)).ReadString();
				break;
			}
		}
	}
}
