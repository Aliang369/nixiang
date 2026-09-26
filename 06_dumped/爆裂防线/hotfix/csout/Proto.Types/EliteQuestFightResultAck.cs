using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class EliteQuestFightResultAck : IMessage<EliteQuestFightResultAck>, IMessage, IEquatable<EliteQuestFightResultAck>, IDeepCloneable<EliteQuestFightResultAck>, IBufferMessage
{
	private static readonly MessageParser<EliteQuestFightResultAck> _parser = new MessageParser<EliteQuestFightResultAck>((Func<EliteQuestFightResultAck>)(() => new EliteQuestFightResultAck()));

	private UnknownFieldSet _unknownFields;

	public const int BestScoreFieldNumber = 1;

	private EBoolState bestScore_;

	public const int BattleSettleFieldNumber = 2;

	private BattleResultInfo battleSettle_;

	public const int EliteQuestInfoFieldNumber = 3;

	private EliteQuestInfo eliteQuestInfo_;

	public const int AwardListFieldNumber = 4;

	private static readonly FieldCodec<AwardDetail> _repeated_awardList_codec = FieldCodec.ForMessage<AwardDetail>(34u, AwardDetail.Parser);

	private readonly RepeatedField<AwardDetail> awardList_ = new RepeatedField<AwardDetail>();

	public const int DataChangeFieldNumber = 5;

	private DataChange dataChange_;

	public const int HelpInfoFieldNumber = 6;

	private HelpInfo helpInfo_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<EliteQuestFightResultAck> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeEliteQuestFightResultAckReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public EBoolState BestScore
	{
		get
		{
			return bestScore_;
		}
		set
		{
			bestScore_ = value;
		}
	}

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
	public EliteQuestInfo EliteQuestInfo
	{
		get
		{
			return eliteQuestInfo_;
		}
		set
		{
			eliteQuestInfo_ = value;
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
	public HelpInfo HelpInfo
	{
		get
		{
			return helpInfo_;
		}
		set
		{
			helpInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public EliteQuestFightResultAck()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public EliteQuestFightResultAck(EliteQuestFightResultAck other)
		: this()
	{
		bestScore_ = other.bestScore_;
		battleSettle_ = ((other.battleSettle_ != null) ? other.battleSettle_.Clone() : null);
		eliteQuestInfo_ = ((other.eliteQuestInfo_ != null) ? other.eliteQuestInfo_.Clone() : null);
		awardList_ = other.awardList_.Clone();
		dataChange_ = ((other.dataChange_ != null) ? other.dataChange_.Clone() : null);
		helpInfo_ = ((other.helpInfo_ != null) ? other.helpInfo_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public EliteQuestFightResultAck Clone()
	{
		return new EliteQuestFightResultAck(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as EliteQuestFightResultAck);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(EliteQuestFightResultAck other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (BestScore != other.BestScore)
		{
			return false;
		}
		if (!object.Equals(BattleSettle, other.BattleSettle))
		{
			return false;
		}
		if (!object.Equals(EliteQuestInfo, other.EliteQuestInfo))
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
		if (!object.Equals(HelpInfo, other.HelpInfo))
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
		if (BestScore != EBoolState.False)
		{
			num ^= BestScore.GetHashCode();
		}
		if (battleSettle_ != null)
		{
			num ^= BattleSettle.GetHashCode();
		}
		if (eliteQuestInfo_ != null)
		{
			num ^= EliteQuestInfo.GetHashCode();
		}
		num ^= ((object)awardList_).GetHashCode();
		if (dataChange_ != null)
		{
			num ^= DataChange.GetHashCode();
		}
		if (helpInfo_ != null)
		{
			num ^= HelpInfo.GetHashCode();
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
		if (BestScore != EBoolState.False)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteEnum((int)BestScore);
		}
		if (battleSettle_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)18);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)BattleSettle);
		}
		if (eliteQuestInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)26);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)EliteQuestInfo);
		}
		awardList_.WriteTo(ref output, _repeated_awardList_codec);
		if (dataChange_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)42);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)DataChange);
		}
		if (helpInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)50);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)HelpInfo);
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
		if (BestScore != EBoolState.False)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)BestScore);
		}
		if (battleSettle_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)BattleSettle);
		}
		if (eliteQuestInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)EliteQuestInfo);
		}
		num += awardList_.CalculateSize(_repeated_awardList_codec);
		if (dataChange_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)DataChange);
		}
		if (helpInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)HelpInfo);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(EliteQuestFightResultAck other)
	{
		if (other == null)
		{
			return;
		}
		if (other.BestScore != EBoolState.False)
		{
			BestScore = other.BestScore;
		}
		if (other.battleSettle_ != null)
		{
			if (battleSettle_ == null)
			{
				BattleSettle = new BattleResultInfo();
			}
			BattleSettle.MergeFrom(other.BattleSettle);
		}
		if (other.eliteQuestInfo_ != null)
		{
			if (eliteQuestInfo_ == null)
			{
				EliteQuestInfo = new EliteQuestInfo();
			}
			EliteQuestInfo.MergeFrom(other.EliteQuestInfo);
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
		if (other.helpInfo_ != null)
		{
			if (helpInfo_ == null)
			{
				HelpInfo = new HelpInfo();
			}
			HelpInfo.MergeFrom(other.HelpInfo);
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
				BestScore = (EBoolState)((ParseContext)(ref input)).ReadEnum();
				break;
			case 18u:
				if (battleSettle_ == null)
				{
					BattleSettle = new BattleResultInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)BattleSettle);
				break;
			case 26u:
				if (eliteQuestInfo_ == null)
				{
					EliteQuestInfo = new EliteQuestInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)EliteQuestInfo);
				break;
			case 34u:
				awardList_.AddEntriesFrom(ref input, _repeated_awardList_codec);
				break;
			case 42u:
				if (dataChange_ == null)
				{
					DataChange = new DataChange();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)DataChange);
				break;
			case 50u:
				if (helpInfo_ == null)
				{
					HelpInfo = new HelpInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)HelpInfo);
				break;
			}
		}
	}
}
