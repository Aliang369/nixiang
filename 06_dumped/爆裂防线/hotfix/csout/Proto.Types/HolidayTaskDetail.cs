using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class HolidayTaskDetail : IMessage<HolidayTaskDetail>, IMessage, IEquatable<HolidayTaskDetail>, IDeepCloneable<HolidayTaskDetail>, IBufferMessage
{
	private static readonly MessageParser<HolidayTaskDetail> _parser = new MessageParser<HolidayTaskDetail>((Func<HolidayTaskDetail>)(() => new HolidayTaskDetail()));

	private UnknownFieldSet _unknownFields;

	public const int BaseInfoFieldNumber = 1;

	private OperateBaseInfo baseInfo_;

	public const int GoodsIdFieldNumber = 2;

	private int goodsId_;

	public const int LevelFieldNumber = 3;

	private int level_;

	public const int ExpFieldNumber = 4;

	private int exp_;

	public const int BuyLevelCountFieldNumber = 5;

	private int buyLevelCount_;

	public const int ActiveStateFieldNumber = 6;

	private int activeState_;

	public const int LowAwardLevelFieldNumber = 7;

	private int lowAwardLevel_;

	public const int HighAwardLevelFieldNumber = 8;

	private int highAwardLevel_;

	public const int HeroIdFieldNumber = 9;

	private int heroId_;

	public const int TaskListFieldNumber = 10;

	private static readonly FieldCodec<TaskDetail> _repeated_taskList_codec = FieldCodec.ForMessage<TaskDetail>(82u, TaskDetail.Parser);

	private readonly RepeatedField<TaskDetail> taskList_ = new RepeatedField<TaskDetail>();

	public const int TotalScoreFieldNumber = 11;

	private int totalScore_;

	public const int ExchangeScoreFieldNumber = 12;

	private int exchangeScore_;

	public const int ExchangeListFieldNumber = 13;

	private static readonly FieldCodec<HolidayTaskExchangeDetail> _repeated_exchangeList_codec = FieldCodec.ForMessage<HolidayTaskExchangeDetail>(106u, HolidayTaskExchangeDetail.Parser);

	private readonly RepeatedField<HolidayTaskExchangeDetail> exchangeList_ = new RepeatedField<HolidayTaskExchangeDetail>();

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<HolidayTaskDetail> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeHolidayTaskDetailReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public OperateBaseInfo BaseInfo
	{
		get
		{
			return baseInfo_;
		}
		set
		{
			baseInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int GoodsId
	{
		get
		{
			return goodsId_;
		}
		set
		{
			goodsId_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int Level
	{
		get
		{
			return level_;
		}
		set
		{
			level_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int Exp
	{
		get
		{
			return exp_;
		}
		set
		{
			exp_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int BuyLevelCount
	{
		get
		{
			return buyLevelCount_;
		}
		set
		{
			buyLevelCount_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int ActiveState
	{
		get
		{
			return activeState_;
		}
		set
		{
			activeState_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int LowAwardLevel
	{
		get
		{
			return lowAwardLevel_;
		}
		set
		{
			lowAwardLevel_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int HighAwardLevel
	{
		get
		{
			return highAwardLevel_;
		}
		set
		{
			highAwardLevel_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int HeroId
	{
		get
		{
			return heroId_;
		}
		set
		{
			heroId_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<TaskDetail> TaskList => taskList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int TotalScore
	{
		get
		{
			return totalScore_;
		}
		set
		{
			totalScore_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int ExchangeScore
	{
		get
		{
			return exchangeScore_;
		}
		set
		{
			exchangeScore_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<HolidayTaskExchangeDetail> ExchangeList => exchangeList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public HolidayTaskDetail()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public HolidayTaskDetail(HolidayTaskDetail other)
		: this()
	{
		baseInfo_ = ((other.baseInfo_ != null) ? other.baseInfo_.Clone() : null);
		goodsId_ = other.goodsId_;
		level_ = other.level_;
		exp_ = other.exp_;
		buyLevelCount_ = other.buyLevelCount_;
		activeState_ = other.activeState_;
		lowAwardLevel_ = other.lowAwardLevel_;
		highAwardLevel_ = other.highAwardLevel_;
		heroId_ = other.heroId_;
		taskList_ = other.taskList_.Clone();
		totalScore_ = other.totalScore_;
		exchangeScore_ = other.exchangeScore_;
		exchangeList_ = other.exchangeList_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public HolidayTaskDetail Clone()
	{
		return new HolidayTaskDetail(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as HolidayTaskDetail);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(HolidayTaskDetail other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!object.Equals(BaseInfo, other.BaseInfo))
		{
			return false;
		}
		if (GoodsId != other.GoodsId)
		{
			return false;
		}
		if (Level != other.Level)
		{
			return false;
		}
		if (Exp != other.Exp)
		{
			return false;
		}
		if (BuyLevelCount != other.BuyLevelCount)
		{
			return false;
		}
		if (ActiveState != other.ActiveState)
		{
			return false;
		}
		if (LowAwardLevel != other.LowAwardLevel)
		{
			return false;
		}
		if (HighAwardLevel != other.HighAwardLevel)
		{
			return false;
		}
		if (HeroId != other.HeroId)
		{
			return false;
		}
		if (!taskList_.Equals(other.taskList_))
		{
			return false;
		}
		if (TotalScore != other.TotalScore)
		{
			return false;
		}
		if (ExchangeScore != other.ExchangeScore)
		{
			return false;
		}
		if (!exchangeList_.Equals(other.exchangeList_))
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
		if (baseInfo_ != null)
		{
			num ^= BaseInfo.GetHashCode();
		}
		if (GoodsId != 0)
		{
			num ^= GoodsId.GetHashCode();
		}
		if (Level != 0)
		{
			num ^= Level.GetHashCode();
		}
		if (Exp != 0)
		{
			num ^= Exp.GetHashCode();
		}
		if (BuyLevelCount != 0)
		{
			num ^= BuyLevelCount.GetHashCode();
		}
		if (ActiveState != 0)
		{
			num ^= ActiveState.GetHashCode();
		}
		if (LowAwardLevel != 0)
		{
			num ^= LowAwardLevel.GetHashCode();
		}
		if (HighAwardLevel != 0)
		{
			num ^= HighAwardLevel.GetHashCode();
		}
		if (HeroId != 0)
		{
			num ^= HeroId.GetHashCode();
		}
		num ^= ((object)taskList_).GetHashCode();
		if (TotalScore != 0)
		{
			num ^= TotalScore.GetHashCode();
		}
		if (ExchangeScore != 0)
		{
			num ^= ExchangeScore.GetHashCode();
		}
		num ^= ((object)exchangeList_).GetHashCode();
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
		if (baseInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)10);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)BaseInfo);
		}
		if (GoodsId != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteInt32(GoodsId);
		}
		if (Level != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)24);
			((WriteContext)(ref output)).WriteInt32(Level);
		}
		if (Exp != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)32);
			((WriteContext)(ref output)).WriteInt32(Exp);
		}
		if (BuyLevelCount != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)40);
			((WriteContext)(ref output)).WriteInt32(BuyLevelCount);
		}
		if (ActiveState != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)48);
			((WriteContext)(ref output)).WriteInt32(ActiveState);
		}
		if (LowAwardLevel != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)56);
			((WriteContext)(ref output)).WriteInt32(LowAwardLevel);
		}
		if (HighAwardLevel != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)64);
			((WriteContext)(ref output)).WriteInt32(HighAwardLevel);
		}
		if (HeroId != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)72);
			((WriteContext)(ref output)).WriteInt32(HeroId);
		}
		taskList_.WriteTo(ref output, _repeated_taskList_codec);
		if (TotalScore != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)88);
			((WriteContext)(ref output)).WriteInt32(TotalScore);
		}
		if (ExchangeScore != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)96);
			((WriteContext)(ref output)).WriteInt32(ExchangeScore);
		}
		exchangeList_.WriteTo(ref output, _repeated_exchangeList_codec);
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
		if (baseInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)BaseInfo);
		}
		if (GoodsId != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(GoodsId);
		}
		if (Level != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(Level);
		}
		if (Exp != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(Exp);
		}
		if (BuyLevelCount != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(BuyLevelCount);
		}
		if (ActiveState != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(ActiveState);
		}
		if (LowAwardLevel != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(LowAwardLevel);
		}
		if (HighAwardLevel != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(HighAwardLevel);
		}
		if (HeroId != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(HeroId);
		}
		num += taskList_.CalculateSize(_repeated_taskList_codec);
		if (TotalScore != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(TotalScore);
		}
		if (ExchangeScore != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(ExchangeScore);
		}
		num += exchangeList_.CalculateSize(_repeated_exchangeList_codec);
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(HolidayTaskDetail other)
	{
		if (other == null)
		{
			return;
		}
		if (other.baseInfo_ != null)
		{
			if (baseInfo_ == null)
			{
				BaseInfo = new OperateBaseInfo();
			}
			BaseInfo.MergeFrom(other.BaseInfo);
		}
		if (other.GoodsId != 0)
		{
			GoodsId = other.GoodsId;
		}
		if (other.Level != 0)
		{
			Level = other.Level;
		}
		if (other.Exp != 0)
		{
			Exp = other.Exp;
		}
		if (other.BuyLevelCount != 0)
		{
			BuyLevelCount = other.BuyLevelCount;
		}
		if (other.ActiveState != 0)
		{
			ActiveState = other.ActiveState;
		}
		if (other.LowAwardLevel != 0)
		{
			LowAwardLevel = other.LowAwardLevel;
		}
		if (other.HighAwardLevel != 0)
		{
			HighAwardLevel = other.HighAwardLevel;
		}
		if (other.HeroId != 0)
		{
			HeroId = other.HeroId;
		}
		taskList_.Add((IEnumerable<TaskDetail>)other.taskList_);
		if (other.TotalScore != 0)
		{
			TotalScore = other.TotalScore;
		}
		if (other.ExchangeScore != 0)
		{
			ExchangeScore = other.ExchangeScore;
		}
		exchangeList_.Add((IEnumerable<HolidayTaskExchangeDetail>)other.exchangeList_);
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
				if (baseInfo_ == null)
				{
					BaseInfo = new OperateBaseInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)BaseInfo);
				break;
			case 16u:
				GoodsId = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 24u:
				Level = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 32u:
				Exp = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 40u:
				BuyLevelCount = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 48u:
				ActiveState = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 56u:
				LowAwardLevel = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 64u:
				HighAwardLevel = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 72u:
				HeroId = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 82u:
				taskList_.AddEntriesFrom(ref input, _repeated_taskList_codec);
				break;
			case 88u:
				TotalScore = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 96u:
				ExchangeScore = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 106u:
				exchangeList_.AddEntriesFrom(ref input, _repeated_exchangeList_codec);
				break;
			}
		}
	}
}
