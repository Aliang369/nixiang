using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class CrossLadderAchieveInfo : IMessage<CrossLadderAchieveInfo>, IMessage, IEquatable<CrossLadderAchieveInfo>, IDeepCloneable<CrossLadderAchieveInfo>, IBufferMessage
{
	private static readonly MessageParser<CrossLadderAchieveInfo> _parser = new MessageParser<CrossLadderAchieveInfo>((Func<CrossLadderAchieveInfo>)(() => new CrossLadderAchieveInfo()));

	private UnknownFieldSet _unknownFields;

	public const int LadderIdFieldNumber = 1;

	private int ladderId_;

	public const int BestLadderIdFieldNumber = 2;

	private int bestLadderId_;

	public const int HistoryBestFieldNumber = 3;

	private int historyBest_;

	public const int ScoreFieldNumber = 4;

	private int score_;

	public const int FightCountFieldNumber = 5;

	private int fightCount_;

	public const int WinCountFieldNumber = 6;

	private int winCount_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<CrossLadderAchieveInfo> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeCrossLadderAchieveInfoReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int LadderId
	{
		get
		{
			return ladderId_;
		}
		set
		{
			ladderId_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int BestLadderId
	{
		get
		{
			return bestLadderId_;
		}
		set
		{
			bestLadderId_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int HistoryBest
	{
		get
		{
			return historyBest_;
		}
		set
		{
			historyBest_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int Score
	{
		get
		{
			return score_;
		}
		set
		{
			score_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int FightCount
	{
		get
		{
			return fightCount_;
		}
		set
		{
			fightCount_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int WinCount
	{
		get
		{
			return winCount_;
		}
		set
		{
			winCount_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public CrossLadderAchieveInfo()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public CrossLadderAchieveInfo(CrossLadderAchieveInfo other)
		: this()
	{
		ladderId_ = other.ladderId_;
		bestLadderId_ = other.bestLadderId_;
		historyBest_ = other.historyBest_;
		score_ = other.score_;
		fightCount_ = other.fightCount_;
		winCount_ = other.winCount_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public CrossLadderAchieveInfo Clone()
	{
		return new CrossLadderAchieveInfo(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as CrossLadderAchieveInfo);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(CrossLadderAchieveInfo other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (LadderId != other.LadderId)
		{
			return false;
		}
		if (BestLadderId != other.BestLadderId)
		{
			return false;
		}
		if (HistoryBest != other.HistoryBest)
		{
			return false;
		}
		if (Score != other.Score)
		{
			return false;
		}
		if (FightCount != other.FightCount)
		{
			return false;
		}
		if (WinCount != other.WinCount)
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
		if (LadderId != 0)
		{
			num ^= LadderId.GetHashCode();
		}
		if (BestLadderId != 0)
		{
			num ^= BestLadderId.GetHashCode();
		}
		if (HistoryBest != 0)
		{
			num ^= HistoryBest.GetHashCode();
		}
		if (Score != 0)
		{
			num ^= Score.GetHashCode();
		}
		if (FightCount != 0)
		{
			num ^= FightCount.GetHashCode();
		}
		if (WinCount != 0)
		{
			num ^= WinCount.GetHashCode();
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
		if (LadderId != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteInt32(LadderId);
		}
		if (BestLadderId != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteInt32(BestLadderId);
		}
		if (HistoryBest != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)24);
			((WriteContext)(ref output)).WriteInt32(HistoryBest);
		}
		if (Score != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)32);
			((WriteContext)(ref output)).WriteInt32(Score);
		}
		if (FightCount != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)40);
			((WriteContext)(ref output)).WriteInt32(FightCount);
		}
		if (WinCount != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)48);
			((WriteContext)(ref output)).WriteInt32(WinCount);
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
		if (LadderId != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(LadderId);
		}
		if (BestLadderId != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(BestLadderId);
		}
		if (HistoryBest != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(HistoryBest);
		}
		if (Score != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(Score);
		}
		if (FightCount != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(FightCount);
		}
		if (WinCount != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(WinCount);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(CrossLadderAchieveInfo other)
	{
		if (other != null)
		{
			if (other.LadderId != 0)
			{
				LadderId = other.LadderId;
			}
			if (other.BestLadderId != 0)
			{
				BestLadderId = other.BestLadderId;
			}
			if (other.HistoryBest != 0)
			{
				HistoryBest = other.HistoryBest;
			}
			if (other.Score != 0)
			{
				Score = other.Score;
			}
			if (other.FightCount != 0)
			{
				FightCount = other.FightCount;
			}
			if (other.WinCount != 0)
			{
				WinCount = other.WinCount;
			}
			_unknownFields = UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
		}
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
				LadderId = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 16u:
				BestLadderId = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 24u:
				HistoryBest = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 32u:
				Score = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 40u:
				FightCount = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 48u:
				WinCount = ((ParseContext)(ref input)).ReadInt32();
				break;
			}
		}
	}
}
