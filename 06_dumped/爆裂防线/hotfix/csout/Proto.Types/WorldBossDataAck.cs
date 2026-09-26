using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class WorldBossDataAck : IMessage<WorldBossDataAck>, IMessage, IEquatable<WorldBossDataAck>, IDeepCloneable<WorldBossDataAck>, IBufferMessage
{
	private static readonly MessageParser<WorldBossDataAck> _parser = new MessageParser<WorldBossDataAck>((Func<WorldBossDataAck>)(() => new WorldBossDataAck()));

	private UnknownFieldSet _unknownFields;

	public const int WorldBossSelfInfoFieldNumber = 1;

	private WorldBossSelfInfo worldBossSelfInfo_;

	public const int WorldBossSelfRankFieldNumber = 2;

	private RankData worldBossSelfRank_;

	public const int WorldBossInfoFieldNumber = 3;

	private WorldBossInfo worldBossInfo_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<WorldBossDataAck> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeWorldBossDataAckReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public WorldBossSelfInfo WorldBossSelfInfo
	{
		get
		{
			return worldBossSelfInfo_;
		}
		set
		{
			worldBossSelfInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RankData WorldBossSelfRank
	{
		get
		{
			return worldBossSelfRank_;
		}
		set
		{
			worldBossSelfRank_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public WorldBossInfo WorldBossInfo
	{
		get
		{
			return worldBossInfo_;
		}
		set
		{
			worldBossInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public WorldBossDataAck()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public WorldBossDataAck(WorldBossDataAck other)
		: this()
	{
		worldBossSelfInfo_ = ((other.worldBossSelfInfo_ != null) ? other.worldBossSelfInfo_.Clone() : null);
		worldBossSelfRank_ = ((other.worldBossSelfRank_ != null) ? other.worldBossSelfRank_.Clone() : null);
		worldBossInfo_ = ((other.worldBossInfo_ != null) ? other.worldBossInfo_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public WorldBossDataAck Clone()
	{
		return new WorldBossDataAck(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as WorldBossDataAck);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(WorldBossDataAck other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!object.Equals(WorldBossSelfInfo, other.WorldBossSelfInfo))
		{
			return false;
		}
		if (!object.Equals(WorldBossSelfRank, other.WorldBossSelfRank))
		{
			return false;
		}
		if (!object.Equals(WorldBossInfo, other.WorldBossInfo))
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
		if (worldBossSelfInfo_ != null)
		{
			num ^= WorldBossSelfInfo.GetHashCode();
		}
		if (worldBossSelfRank_ != null)
		{
			num ^= WorldBossSelfRank.GetHashCode();
		}
		if (worldBossInfo_ != null)
		{
			num ^= WorldBossInfo.GetHashCode();
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
		if (worldBossSelfInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)10);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)WorldBossSelfInfo);
		}
		if (worldBossSelfRank_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)18);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)WorldBossSelfRank);
		}
		if (worldBossInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)26);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)WorldBossInfo);
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
		if (worldBossSelfInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)WorldBossSelfInfo);
		}
		if (worldBossSelfRank_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)WorldBossSelfRank);
		}
		if (worldBossInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)WorldBossInfo);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(WorldBossDataAck other)
	{
		if (other == null)
		{
			return;
		}
		if (other.worldBossSelfInfo_ != null)
		{
			if (worldBossSelfInfo_ == null)
			{
				WorldBossSelfInfo = new WorldBossSelfInfo();
			}
			WorldBossSelfInfo.MergeFrom(other.WorldBossSelfInfo);
		}
		if (other.worldBossSelfRank_ != null)
		{
			if (worldBossSelfRank_ == null)
			{
				WorldBossSelfRank = new RankData();
			}
			WorldBossSelfRank.MergeFrom(other.WorldBossSelfRank);
		}
		if (other.worldBossInfo_ != null)
		{
			if (worldBossInfo_ == null)
			{
				WorldBossInfo = new WorldBossInfo();
			}
			WorldBossInfo.MergeFrom(other.WorldBossInfo);
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
				if (worldBossSelfInfo_ == null)
				{
					WorldBossSelfInfo = new WorldBossSelfInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)WorldBossSelfInfo);
				break;
			case 18u:
				if (worldBossSelfRank_ == null)
				{
					WorldBossSelfRank = new RankData();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)WorldBossSelfRank);
				break;
			case 26u:
				if (worldBossInfo_ == null)
				{
					WorldBossInfo = new WorldBossInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)WorldBossInfo);
				break;
			}
		}
	}
}
