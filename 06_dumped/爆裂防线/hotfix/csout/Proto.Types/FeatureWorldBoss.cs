using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class FeatureWorldBoss : IMessage<FeatureWorldBoss>, IMessage, IEquatable<FeatureWorldBoss>, IDeepCloneable<FeatureWorldBoss>, IBufferMessage
{
	private static readonly MessageParser<FeatureWorldBoss> _parser = new MessageParser<FeatureWorldBoss>((Func<FeatureWorldBoss>)(() => new FeatureWorldBoss()));

	private UnknownFieldSet _unknownFields;

	public const int WorldBossSelfInfoFieldNumber = 1;

	private WorldBossSelfInfo worldBossSelfInfo_;

	public const int WorldBossInfoFieldNumber = 2;

	private WorldBossInfo worldBossInfo_;

	public const int SelfRankFieldNumber = 3;

	private int selfRank_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<FeatureWorldBoss> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeFeatureWorldBossReflection.Descriptor.MessageTypes[0];

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
	public int SelfRank
	{
		get
		{
			return selfRank_;
		}
		set
		{
			selfRank_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public FeatureWorldBoss()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public FeatureWorldBoss(FeatureWorldBoss other)
		: this()
	{
		worldBossSelfInfo_ = ((other.worldBossSelfInfo_ != null) ? other.worldBossSelfInfo_.Clone() : null);
		worldBossInfo_ = ((other.worldBossInfo_ != null) ? other.worldBossInfo_.Clone() : null);
		selfRank_ = other.selfRank_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public FeatureWorldBoss Clone()
	{
		return new FeatureWorldBoss(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as FeatureWorldBoss);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(FeatureWorldBoss other)
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
		if (!object.Equals(WorldBossInfo, other.WorldBossInfo))
		{
			return false;
		}
		if (SelfRank != other.SelfRank)
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
		if (worldBossInfo_ != null)
		{
			num ^= WorldBossInfo.GetHashCode();
		}
		if (SelfRank != 0)
		{
			num ^= SelfRank.GetHashCode();
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
		if (worldBossInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)18);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)WorldBossInfo);
		}
		if (SelfRank != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)24);
			((WriteContext)(ref output)).WriteInt32(SelfRank);
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
		if (worldBossInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)WorldBossInfo);
		}
		if (SelfRank != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(SelfRank);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(FeatureWorldBoss other)
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
		if (other.worldBossInfo_ != null)
		{
			if (worldBossInfo_ == null)
			{
				WorldBossInfo = new WorldBossInfo();
			}
			WorldBossInfo.MergeFrom(other.WorldBossInfo);
		}
		if (other.SelfRank != 0)
		{
			SelfRank = other.SelfRank;
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
				if (worldBossInfo_ == null)
				{
					WorldBossInfo = new WorldBossInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)WorldBossInfo);
				break;
			case 24u:
				SelfRank = ((ParseContext)(ref input)).ReadInt32();
				break;
			}
		}
	}
}
