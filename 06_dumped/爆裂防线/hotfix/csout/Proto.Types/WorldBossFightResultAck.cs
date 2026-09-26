using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class WorldBossFightResultAck : IMessage<WorldBossFightResultAck>, IMessage, IEquatable<WorldBossFightResultAck>, IDeepCloneable<WorldBossFightResultAck>, IBufferMessage
{
	private static readonly MessageParser<WorldBossFightResultAck> _parser = new MessageParser<WorldBossFightResultAck>((Func<WorldBossFightResultAck>)(() => new WorldBossFightResultAck()));

	private UnknownFieldSet _unknownFields;

	public const int BattleSettleFieldNumber = 1;

	private BattleResultInfo battleSettle_;

	public const int WorldBossSelfInfoFieldNumber = 2;

	private WorldBossSelfInfo worldBossSelfInfo_;

	public const int WorldBossSelfRankFieldNumber = 3;

	private RankData worldBossSelfRank_;

	public const int WorldBossInfoFieldNumber = 4;

	private WorldBossInfo worldBossInfo_;

	public const int AwardListFieldNumber = 5;

	private static readonly FieldCodec<AwardDetail> _repeated_awardList_codec = FieldCodec.ForMessage<AwardDetail>(42u, AwardDetail.Parser);

	private readonly RepeatedField<AwardDetail> awardList_ = new RepeatedField<AwardDetail>();

	public const int DataChangeFieldNumber = 6;

	private DataChange dataChange_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<WorldBossFightResultAck> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeWorldBossFightResultAckReflection.Descriptor.MessageTypes[0];

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
	public RepeatedField<AwardDetail> AwardList => awardList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public DataChange DataChange
	{
		get
		{
			return dataChange_;
		}
		set
		{
			dataChange_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public WorldBossFightResultAck()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public WorldBossFightResultAck(WorldBossFightResultAck other)
		: this()
	{
		battleSettle_ = ((other.battleSettle_ != null) ? other.battleSettle_.Clone() : null);
		worldBossSelfInfo_ = ((other.worldBossSelfInfo_ != null) ? other.worldBossSelfInfo_.Clone() : null);
		worldBossSelfRank_ = ((other.worldBossSelfRank_ != null) ? other.worldBossSelfRank_.Clone() : null);
		worldBossInfo_ = ((other.worldBossInfo_ != null) ? other.worldBossInfo_.Clone() : null);
		awardList_ = other.awardList_.Clone();
		dataChange_ = ((other.dataChange_ != null) ? other.dataChange_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public WorldBossFightResultAck Clone()
	{
		return new WorldBossFightResultAck(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as WorldBossFightResultAck);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(WorldBossFightResultAck other)
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
		if (!awardList_.Equals(other.awardList_))
		{
			return false;
		}
		if (!object.Equals(DataChange, other.DataChange))
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
		num ^= ((object)awardList_).GetHashCode();
		if (dataChange_ != null)
		{
			num ^= DataChange.GetHashCode();
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
		if (worldBossSelfInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)18);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)WorldBossSelfInfo);
		}
		if (worldBossSelfRank_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)26);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)WorldBossSelfRank);
		}
		if (worldBossInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)34);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)WorldBossInfo);
		}
		awardList_.WriteTo(ref output, _repeated_awardList_codec);
		if (dataChange_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)50);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)DataChange);
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
		num += awardList_.CalculateSize(_repeated_awardList_codec);
		if (dataChange_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)DataChange);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(WorldBossFightResultAck other)
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
		awardList_.Add((IEnumerable<AwardDetail>)other.awardList_);
		if (other.dataChange_ != null)
		{
			if (dataChange_ == null)
			{
				DataChange = new DataChange();
			}
			DataChange.MergeFrom(other.DataChange);
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
				if (worldBossSelfInfo_ == null)
				{
					WorldBossSelfInfo = new WorldBossSelfInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)WorldBossSelfInfo);
				break;
			case 26u:
				if (worldBossSelfRank_ == null)
				{
					WorldBossSelfRank = new RankData();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)WorldBossSelfRank);
				break;
			case 34u:
				if (worldBossInfo_ == null)
				{
					WorldBossInfo = new WorldBossInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)WorldBossInfo);
				break;
			case 42u:
				awardList_.AddEntriesFrom(ref input, _repeated_awardList_codec);
				break;
			case 50u:
				if (dataChange_ == null)
				{
					DataChange = new DataChange();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)DataChange);
				break;
			}
		}
	}
}
