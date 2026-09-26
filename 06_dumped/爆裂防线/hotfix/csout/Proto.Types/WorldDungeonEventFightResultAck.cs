using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class WorldDungeonEventFightResultAck : IMessage<WorldDungeonEventFightResultAck>, IMessage, IEquatable<WorldDungeonEventFightResultAck>, IDeepCloneable<WorldDungeonEventFightResultAck>, IBufferMessage
{
	private static readonly MessageParser<WorldDungeonEventFightResultAck> _parser = new MessageParser<WorldDungeonEventFightResultAck>((Func<WorldDungeonEventFightResultAck>)(() => new WorldDungeonEventFightResultAck()));

	private UnknownFieldSet _unknownFields;

	public const int BattleSettleFieldNumber = 1;

	private BattleResultInfo battleSettle_;

	public const int DungeonBaseInfoFieldNumber = 2;

	private DungeonBaseInfo dungeonBaseInfo_;

	public const int DungeonMapInfoFieldNumber = 3;

	private DungeonMapInfo dungeonMapInfo_;

	public const int DungeonAwardInfoFieldNumber = 4;

	private DungeonAwardInfo dungeonAwardInfo_;

	public const int WorldInfoFieldNumber = 5;

	private WorldInfo worldInfo_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<WorldDungeonEventFightResultAck> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeWorldDungeonEventFightResultAckReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public BattleResultInfo BattleSettle
	{
		get
		{
			return battleSettle_;
		}
		set
		{
			battleSettle_ = value;
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
	public WorldInfo WorldInfo
	{
		get
		{
			return worldInfo_;
		}
		set
		{
			worldInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public WorldDungeonEventFightResultAck()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public WorldDungeonEventFightResultAck(WorldDungeonEventFightResultAck other)
		: this()
	{
		battleSettle_ = ((other.battleSettle_ != null) ? other.battleSettle_.Clone() : null);
		dungeonBaseInfo_ = ((other.dungeonBaseInfo_ != null) ? other.dungeonBaseInfo_.Clone() : null);
		dungeonMapInfo_ = ((other.dungeonMapInfo_ != null) ? other.dungeonMapInfo_.Clone() : null);
		dungeonAwardInfo_ = ((other.dungeonAwardInfo_ != null) ? other.dungeonAwardInfo_.Clone() : null);
		worldInfo_ = ((other.worldInfo_ != null) ? other.worldInfo_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public WorldDungeonEventFightResultAck Clone()
	{
		return new WorldDungeonEventFightResultAck(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as WorldDungeonEventFightResultAck);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(WorldDungeonEventFightResultAck other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!object.Equals(BattleSettle, other.BattleSettle))
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
		if (!object.Equals(DungeonAwardInfo, other.DungeonAwardInfo))
		{
			return false;
		}
		if (!object.Equals(WorldInfo, other.WorldInfo))
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
		if (battleSettle_ != null)
		{
			num ^= BattleSettle.GetHashCode();
		}
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
		if (worldInfo_ != null)
		{
			num ^= WorldInfo.GetHashCode();
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
		if (battleSettle_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)10);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)BattleSettle);
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
		if (dungeonAwardInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)34);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)DungeonAwardInfo);
		}
		if (worldInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)42);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)WorldInfo);
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
		if (battleSettle_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)BattleSettle);
		}
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
		if (worldInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)WorldInfo);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(WorldDungeonEventFightResultAck other)
	{
		if (other == null)
		{
			return;
		}
		if (other.battleSettle_ != null)
		{
			if (battleSettle_ == null)
			{
				BattleSettle = new BattleResultInfo();
			}
			BattleSettle.MergeFrom(other.BattleSettle);
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
		if (other.worldInfo_ != null)
		{
			if (worldInfo_ == null)
			{
				WorldInfo = new WorldInfo();
			}
			WorldInfo.MergeFrom(other.WorldInfo);
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
				if (battleSettle_ == null)
				{
					BattleSettle = new BattleResultInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)BattleSettle);
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
				if (dungeonAwardInfo_ == null)
				{
					DungeonAwardInfo = new DungeonAwardInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)DungeonAwardInfo);
				break;
			case 42u:
				if (worldInfo_ == null)
				{
					WorldInfo = new WorldInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)WorldInfo);
				break;
			}
		}
	}
}
