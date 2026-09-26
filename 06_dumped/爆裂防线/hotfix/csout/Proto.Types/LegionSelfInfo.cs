using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class LegionSelfInfo : IMessage<LegionSelfInfo>, IMessage, IEquatable<LegionSelfInfo>, IDeepCloneable<LegionSelfInfo>, IBufferMessage
{
	private static readonly MessageParser<LegionSelfInfo> _parser = new MessageParser<LegionSelfInfo>((Func<LegionSelfInfo>)(() => new LegionSelfInfo()));

	private UnknownFieldSet _unknownFields;

	public const int LegionIdFieldNumber = 1;

	private int legionId_;

	public const int JoinTmFieldNumber = 2;

	private int joinTm_;

	public const int DonateFieldNumber = 3;

	private LegionSelfDonate donate_;

	public const int DinnerListFieldNumber = 4;

	private static readonly FieldCodec<LegionSelfDinner> _repeated_dinnerList_codec = FieldCodec.ForMessage<LegionSelfDinner>(34u, LegionSelfDinner.Parser);

	private readonly RepeatedField<LegionSelfDinner> dinnerList_ = new RepeatedField<LegionSelfDinner>();

	public const int RewardCountFieldNumber = 5;

	private int rewardCount_;

	public const int BossInfoFieldNumber = 6;

	private LegionSelfBoss bossInfo_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<LegionSelfInfo> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeLegionSelfInfoReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int LegionId
	{
		get
		{
			return legionId_;
		}
		set
		{
			legionId_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int JoinTm
	{
		get
		{
			return joinTm_;
		}
		set
		{
			joinTm_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public LegionSelfDonate Donate
	{
		get
		{
			return donate_;
		}
		set
		{
			donate_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<LegionSelfDinner> DinnerList => dinnerList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int RewardCount
	{
		get
		{
			return rewardCount_;
		}
		set
		{
			rewardCount_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public LegionSelfBoss BossInfo
	{
		get
		{
			return bossInfo_;
		}
		set
		{
			bossInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public LegionSelfInfo()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public LegionSelfInfo(LegionSelfInfo other)
		: this()
	{
		legionId_ = other.legionId_;
		joinTm_ = other.joinTm_;
		donate_ = ((other.donate_ != null) ? other.donate_.Clone() : null);
		dinnerList_ = other.dinnerList_.Clone();
		rewardCount_ = other.rewardCount_;
		bossInfo_ = ((other.bossInfo_ != null) ? other.bossInfo_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public LegionSelfInfo Clone()
	{
		return new LegionSelfInfo(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as LegionSelfInfo);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(LegionSelfInfo other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (LegionId != other.LegionId)
		{
			return false;
		}
		if (JoinTm != other.JoinTm)
		{
			return false;
		}
		if (!object.Equals(Donate, other.Donate))
		{
			return false;
		}
		if (!dinnerList_.Equals(other.dinnerList_))
		{
			return false;
		}
		if (RewardCount != other.RewardCount)
		{
			return false;
		}
		if (!object.Equals(BossInfo, other.BossInfo))
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
		if (LegionId != 0)
		{
			num ^= LegionId.GetHashCode();
		}
		if (JoinTm != 0)
		{
			num ^= JoinTm.GetHashCode();
		}
		if (donate_ != null)
		{
			num ^= Donate.GetHashCode();
		}
		num ^= ((object)dinnerList_).GetHashCode();
		if (RewardCount != 0)
		{
			num ^= RewardCount.GetHashCode();
		}
		if (bossInfo_ != null)
		{
			num ^= BossInfo.GetHashCode();
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
		if (LegionId != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteInt32(LegionId);
		}
		if (JoinTm != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteInt32(JoinTm);
		}
		if (donate_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)26);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)Donate);
		}
		dinnerList_.WriteTo(ref output, _repeated_dinnerList_codec);
		if (RewardCount != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)40);
			((WriteContext)(ref output)).WriteInt32(RewardCount);
		}
		if (bossInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)50);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)BossInfo);
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
		if (LegionId != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(LegionId);
		}
		if (JoinTm != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(JoinTm);
		}
		if (donate_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)Donate);
		}
		num += dinnerList_.CalculateSize(_repeated_dinnerList_codec);
		if (RewardCount != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(RewardCount);
		}
		if (bossInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)BossInfo);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(LegionSelfInfo other)
	{
		if (other == null)
		{
			return;
		}
		if (other.LegionId != 0)
		{
			LegionId = other.LegionId;
		}
		if (other.JoinTm != 0)
		{
			JoinTm = other.JoinTm;
		}
		if (other.donate_ != null)
		{
			if (donate_ == null)
			{
				Donate = new LegionSelfDonate();
			}
			Donate.MergeFrom(other.Donate);
		}
		dinnerList_.Add((IEnumerable<LegionSelfDinner>)other.dinnerList_);
		if (other.RewardCount != 0)
		{
			RewardCount = other.RewardCount;
		}
		if (other.bossInfo_ != null)
		{
			if (bossInfo_ == null)
			{
				BossInfo = new LegionSelfBoss();
			}
			BossInfo.MergeFrom(other.BossInfo);
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
				LegionId = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 16u:
				JoinTm = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 26u:
				if (donate_ == null)
				{
					Donate = new LegionSelfDonate();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)Donate);
				break;
			case 34u:
				dinnerList_.AddEntriesFrom(ref input, _repeated_dinnerList_codec);
				break;
			case 40u:
				RewardCount = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 50u:
				if (bossInfo_ == null)
				{
					BossInfo = new LegionSelfBoss();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)BossInfo);
				break;
			}
		}
	}
}
