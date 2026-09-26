using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class WorldDungeonEventFightStartAck : IMessage<WorldDungeonEventFightStartAck>, IMessage, IEquatable<WorldDungeonEventFightStartAck>, IDeepCloneable<WorldDungeonEventFightStartAck>, IBufferMessage
{
	private static readonly MessageParser<WorldDungeonEventFightStartAck> _parser = new MessageParser<WorldDungeonEventFightStartAck>((Func<WorldDungeonEventFightStartAck>)(() => new WorldDungeonEventFightStartAck()));

	private UnknownFieldSet _unknownFields;

	public const int DungeonIdFieldNumber = 1;

	private int dungeonId_;

	public const int DungeonBaseInfoFieldNumber = 2;

	private DungeonBaseInfo dungeonBaseInfo_;

	public const int DungeonMapInfoFieldNumber = 3;

	private DungeonMapInfo dungeonMapInfo_;

	public const int BattleInfoFieldNumber = 4;

	private BattleInfo battleInfo_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<WorldDungeonEventFightStartAck> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeWorldDungeonEventFightStartAckReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int DungeonId
	{
		get
		{
			return dungeonId_;
		}
		set
		{
			dungeonId_ = value;
		}
	}

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
	public BattleInfo BattleInfo
	{
		get
		{
			return battleInfo_;
		}
		set
		{
			battleInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public WorldDungeonEventFightStartAck()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public WorldDungeonEventFightStartAck(WorldDungeonEventFightStartAck other)
		: this()
	{
		dungeonId_ = other.dungeonId_;
		dungeonBaseInfo_ = ((other.dungeonBaseInfo_ != null) ? other.dungeonBaseInfo_.Clone() : null);
		dungeonMapInfo_ = ((other.dungeonMapInfo_ != null) ? other.dungeonMapInfo_.Clone() : null);
		battleInfo_ = ((other.battleInfo_ != null) ? other.battleInfo_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public WorldDungeonEventFightStartAck Clone()
	{
		return new WorldDungeonEventFightStartAck(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as WorldDungeonEventFightStartAck);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(WorldDungeonEventFightStartAck other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (DungeonId != other.DungeonId)
		{
			return false;
		}
		if (!object.Equals(DungeonBaseInfo, other.DungeonBaseInfo))
		{
			return false;
		}
		if (!object.Equals(DungeonMapInfo, other.DungeonMapInfo))
		{
			return false;
		}
		if (!object.Equals(BattleInfo, other.BattleInfo))
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
		if (DungeonId != 0)
		{
			num ^= DungeonId.GetHashCode();
		}
		if (dungeonBaseInfo_ != null)
		{
			num ^= DungeonBaseInfo.GetHashCode();
		}
		if (dungeonMapInfo_ != null)
		{
			num ^= DungeonMapInfo.GetHashCode();
		}
		if (battleInfo_ != null)
		{
			num ^= BattleInfo.GetHashCode();
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
		if (DungeonId != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteInt32(DungeonId);
		}
		if (dungeonBaseInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)18);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)DungeonBaseInfo);
		}
		if (dungeonMapInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)26);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)DungeonMapInfo);
		}
		if (battleInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)34);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)BattleInfo);
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
		if (DungeonId != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(DungeonId);
		}
		if (dungeonBaseInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)DungeonBaseInfo);
		}
		if (dungeonMapInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)DungeonMapInfo);
		}
		if (battleInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)BattleInfo);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(WorldDungeonEventFightStartAck other)
	{
		if (other == null)
		{
			return;
		}
		if (other.DungeonId != 0)
		{
			DungeonId = other.DungeonId;
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
		if (other.battleInfo_ != null)
		{
			if (battleInfo_ == null)
			{
				BattleInfo = new BattleInfo();
			}
			BattleInfo.MergeFrom(other.BattleInfo);
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
			case 8u:
				DungeonId = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 18u:
				if (dungeonBaseInfo_ == null)
				{
					DungeonBaseInfo = new DungeonBaseInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)DungeonBaseInfo);
				break;
			case 26u:
				if (dungeonMapInfo_ == null)
				{
					DungeonMapInfo = new DungeonMapInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)DungeonMapInfo);
				break;
			case 34u:
				if (battleInfo_ == null)
				{
					BattleInfo = new BattleInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)BattleInfo);
				break;
			}
		}
	}
}
