using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class WorldDungeonLogInfoAck : IMessage<WorldDungeonLogInfoAck>, IMessage, IEquatable<WorldDungeonLogInfoAck>, IDeepCloneable<WorldDungeonLogInfoAck>, IBufferMessage
{
	private static readonly MessageParser<WorldDungeonLogInfoAck> _parser = new MessageParser<WorldDungeonLogInfoAck>((Func<WorldDungeonLogInfoAck>)(() => new WorldDungeonLogInfoAck()));

	private UnknownFieldSet _unknownFields;

	public const int DungeonLogInfoFieldNumber = 1;

	private DungeonLogInfo dungeonLogInfo_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<WorldDungeonLogInfoAck> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeWorldDungeonLogInfoAckReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public DungeonLogInfo DungeonLogInfo
	{
		get
		{
			return dungeonLogInfo_;
		}
		set
		{
			dungeonLogInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public WorldDungeonLogInfoAck()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public WorldDungeonLogInfoAck(WorldDungeonLogInfoAck other)
		: this()
	{
		dungeonLogInfo_ = ((other.dungeonLogInfo_ != null) ? other.dungeonLogInfo_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public WorldDungeonLogInfoAck Clone()
	{
		return new WorldDungeonLogInfoAck(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as WorldDungeonLogInfoAck);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(WorldDungeonLogInfoAck other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!object.Equals(DungeonLogInfo, other.DungeonLogInfo))
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
		if (dungeonLogInfo_ != null)
		{
			num ^= DungeonLogInfo.GetHashCode();
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
		if (dungeonLogInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)10);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)DungeonLogInfo);
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
		if (dungeonLogInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)DungeonLogInfo);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(WorldDungeonLogInfoAck other)
	{
		if (other == null)
		{
			return;
		}
		if (other.dungeonLogInfo_ != null)
		{
			if (dungeonLogInfo_ == null)
			{
				DungeonLogInfo = new DungeonLogInfo();
			}
			DungeonLogInfo.MergeFrom(other.DungeonLogInfo);
		}
		_unknownFields = UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
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
			if (num != 10)
			{
				_unknownFields = UnknownFieldSet.MergeFieldFrom(_unknownFields, ref input);
				continue;
			}
			if (dungeonLogInfo_ == null)
			{
				DungeonLogInfo = new DungeonLogInfo();
			}
			((ParseContext)(ref input)).ReadMessage((IMessage)(object)DungeonLogInfo);
		}
	}
}
