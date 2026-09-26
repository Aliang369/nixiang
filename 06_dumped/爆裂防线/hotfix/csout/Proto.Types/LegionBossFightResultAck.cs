using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class LegionBossFightResultAck : IMessage<LegionBossFightResultAck>, IMessage, IEquatable<LegionBossFightResultAck>, IDeepCloneable<LegionBossFightResultAck>, IBufferMessage
{
	private static readonly MessageParser<LegionBossFightResultAck> _parser = new MessageParser<LegionBossFightResultAck>((Func<LegionBossFightResultAck>)(() => new LegionBossFightResultAck()));

	private UnknownFieldSet _unknownFields;

	public const int BattleSettleFieldNumber = 1;

	private BattleResultInfo battleSettle_;

	public const int LegionBaseInfoFieldNumber = 2;

	private LegionBaseInfo legionBaseInfo_;

	public const int LegionSelfInfoFieldNumber = 3;

	private LegionSelfInfo legionSelfInfo_;

	public const int LegionBossInfoFieldNumber = 4;

	private LegionBossInfo legionBossInfo_;

	public const int AwardListFieldNumber = 5;

	private static readonly FieldCodec<AwardDetail> _repeated_awardList_codec = FieldCodec.ForMessage<AwardDetail>(42u, AwardDetail.Parser);

	private readonly RepeatedField<AwardDetail> awardList_ = new RepeatedField<AwardDetail>();

	public const int DataChangeFieldNumber = 6;

	private DataChange dataChange_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<LegionBossFightResultAck> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeLegionBossFightResultAckReflection.Descriptor.MessageTypes[0];

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
	public LegionBaseInfo LegionBaseInfo
	{
		get
		{
			return legionBaseInfo_;
		}
		set
		{
			legionBaseInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public LegionSelfInfo LegionSelfInfo
	{
		get
		{
			return legionSelfInfo_;
		}
		set
		{
			legionSelfInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public LegionBossInfo LegionBossInfo
	{
		get
		{
			return legionBossInfo_;
		}
		set
		{
			legionBossInfo_ = value;
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
	public LegionBossFightResultAck()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public LegionBossFightResultAck(LegionBossFightResultAck other)
		: this()
	{
		battleSettle_ = ((other.battleSettle_ != null) ? other.battleSettle_.Clone() : null);
		legionBaseInfo_ = ((other.legionBaseInfo_ != null) ? other.legionBaseInfo_.Clone() : null);
		legionSelfInfo_ = ((other.legionSelfInfo_ != null) ? other.legionSelfInfo_.Clone() : null);
		legionBossInfo_ = ((other.legionBossInfo_ != null) ? other.legionBossInfo_.Clone() : null);
		awardList_ = other.awardList_.Clone();
		dataChange_ = ((other.dataChange_ != null) ? other.dataChange_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public LegionBossFightResultAck Clone()
	{
		return new LegionBossFightResultAck(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as LegionBossFightResultAck);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(LegionBossFightResultAck other)
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
		if (!object.Equals(LegionBaseInfo, other.LegionBaseInfo))
		{
			return false;
		}
		if (!object.Equals(LegionSelfInfo, other.LegionSelfInfo))
		{
			return false;
		}
		if (!object.Equals(LegionBossInfo, other.LegionBossInfo))
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
		if (legionBaseInfo_ != null)
		{
			num ^= LegionBaseInfo.GetHashCode();
		}
		if (legionSelfInfo_ != null)
		{
			num ^= LegionSelfInfo.GetHashCode();
		}
		if (legionBossInfo_ != null)
		{
			num ^= LegionBossInfo.GetHashCode();
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
		if (legionBaseInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)18);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)LegionBaseInfo);
		}
		if (legionSelfInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)26);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)LegionSelfInfo);
		}
		if (legionBossInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)34);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)LegionBossInfo);
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
		if (legionBaseInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)LegionBaseInfo);
		}
		if (legionSelfInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)LegionSelfInfo);
		}
		if (legionBossInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)LegionBossInfo);
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
	public void MergeFrom(LegionBossFightResultAck other)
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
		if (other.legionBaseInfo_ != null)
		{
			if (legionBaseInfo_ == null)
			{
				LegionBaseInfo = new LegionBaseInfo();
			}
			LegionBaseInfo.MergeFrom(other.LegionBaseInfo);
		}
		if (other.legionSelfInfo_ != null)
		{
			if (legionSelfInfo_ == null)
			{
				LegionSelfInfo = new LegionSelfInfo();
			}
			LegionSelfInfo.MergeFrom(other.LegionSelfInfo);
		}
		if (other.legionBossInfo_ != null)
		{
			if (legionBossInfo_ == null)
			{
				LegionBossInfo = new LegionBossInfo();
			}
			LegionBossInfo.MergeFrom(other.LegionBossInfo);
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
				if (legionBaseInfo_ == null)
				{
					LegionBaseInfo = new LegionBaseInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)LegionBaseInfo);
				break;
			case 26u:
				if (legionSelfInfo_ == null)
				{
					LegionSelfInfo = new LegionSelfInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)LegionSelfInfo);
				break;
			case 34u:
				if (legionBossInfo_ == null)
				{
					LegionBossInfo = new LegionBossInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)LegionBossInfo);
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
