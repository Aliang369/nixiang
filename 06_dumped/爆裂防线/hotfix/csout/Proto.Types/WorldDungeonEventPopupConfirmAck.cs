using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class WorldDungeonEventPopupConfirmAck : IMessage<WorldDungeonEventPopupConfirmAck>, IMessage, IEquatable<WorldDungeonEventPopupConfirmAck>, IDeepCloneable<WorldDungeonEventPopupConfirmAck>, IBufferMessage
{
	private static readonly MessageParser<WorldDungeonEventPopupConfirmAck> _parser = new MessageParser<WorldDungeonEventPopupConfirmAck>((Func<WorldDungeonEventPopupConfirmAck>)(() => new WorldDungeonEventPopupConfirmAck()));

	private UnknownFieldSet _unknownFields;

	public const int DungeonBaseInfoFieldNumber = 1;

	private DungeonBaseInfo dungeonBaseInfo_;

	public const int DungeonMapInfoFieldNumber = 2;

	private DungeonMapInfo dungeonMapInfo_;

	public const int DungeonAwardInfoFieldNumber = 3;

	private DungeonAwardInfo dungeonAwardInfo_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<WorldDungeonEventPopupConfirmAck> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeWorldDungeonEventPopupConfirmAckReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public DungeonBaseInfo DungeonBaseInfo
	{
		get
		{
			return dungeonBaseInfo_;
		}
		set
		{
			dungeonBaseInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public DungeonMapInfo DungeonMapInfo
	{
		get
		{
			return dungeonMapInfo_;
		}
		set
		{
			dungeonMapInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public DungeonAwardInfo DungeonAwardInfo
	{
		get
		{
			return dungeonAwardInfo_;
		}
		set
		{
			dungeonAwardInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public WorldDungeonEventPopupConfirmAck()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public WorldDungeonEventPopupConfirmAck(WorldDungeonEventPopupConfirmAck other)
		: this()
	{
		dungeonBaseInfo_ = ((other.dungeonBaseInfo_ != null) ? other.dungeonBaseInfo_.Clone() : null);
		dungeonMapInfo_ = ((other.dungeonMapInfo_ != null) ? other.dungeonMapInfo_.Clone() : null);
		dungeonAwardInfo_ = ((other.dungeonAwardInfo_ != null) ? other.dungeonAwardInfo_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public WorldDungeonEventPopupConfirmAck Clone()
	{
		return new WorldDungeonEventPopupConfirmAck(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as WorldDungeonEventPopupConfirmAck);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(WorldDungeonEventPopupConfirmAck other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!object.Equals(DungeonBaseInfo, other.DungeonBaseInfo))
		{
			return false;
		}
		if (!object.Equals(DungeonMapInfo, other.DungeonMapInfo))
		{
			return false;
		}
		if (!object.Equals(DungeonAwardInfo, other.DungeonAwardInfo))
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
		if (dungeonBaseInfo_ != null)
		{
			num ^= DungeonBaseInfo.GetHashCode();
		}
		if (dungeonMapInfo_ != null)
		{
			num ^= DungeonMapInfo.GetHashCode();
		}
		if (dungeonAwardInfo_ != null)
		{
			num ^= DungeonAwardInfo.GetHashCode();
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
		if (dungeonBaseInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)10);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)DungeonBaseInfo);
		}
		if (dungeonMapInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)18);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)DungeonMapInfo);
		}
		if (dungeonAwardInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)26);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)DungeonAwardInfo);
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
		if (dungeonBaseInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)DungeonBaseInfo);
		}
		if (dungeonMapInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)DungeonMapInfo);
		}
		if (dungeonAwardInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)DungeonAwardInfo);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(WorldDungeonEventPopupConfirmAck other)
	{
		if (other == null)
		{
			return;
		}
		if (other.dungeonBaseInfo_ != null)
		{
			if (dungeonBaseInfo_ == null)
			{
				DungeonBaseInfo = new DungeonBaseInfo();
			}
			DungeonBaseInfo.MergeFrom(other.DungeonBaseInfo);
		}
		if (other.dungeonMapInfo_ != null)
		{
			if (dungeonMapInfo_ == null)
			{
				DungeonMapInfo = new DungeonMapInfo();
			}
			DungeonMapInfo.MergeFrom(other.DungeonMapInfo);
		}
		if (other.dungeonAwardInfo_ != null)
		{
			if (dungeonAwardInfo_ == null)
			{
				DungeonAwardInfo = new DungeonAwardInfo();
			}
			DungeonAwardInfo.MergeFrom(other.DungeonAwardInfo);
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
			switch (num)
			{
			default:
				_unknownFields = UnknownFieldSet.MergeFieldFrom(_unknownFields, ref input);
				break;
			case 10u:
				if (dungeonBaseInfo_ == null)
				{
					DungeonBaseInfo = new DungeonBaseInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)DungeonBaseInfo);
				break;
			case 18u:
				if (dungeonMapInfo_ == null)
				{
					DungeonMapInfo = new DungeonMapInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)DungeonMapInfo);
				break;
			case 26u:
				if (dungeonAwardInfo_ == null)
				{
					DungeonAwardInfo = new DungeonAwardInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)DungeonAwardInfo);
				break;
			}
		}
	}
}
