using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class FusionBaseInfo : IMessage<FusionBaseInfo>, IMessage, IEquatable<FusionBaseInfo>, IDeepCloneable<FusionBaseInfo>, IBufferMessage
{
	private static readonly MessageParser<FusionBaseInfo> _parser = new MessageParser<FusionBaseInfo>((Func<FusionBaseInfo>)(() => new FusionBaseInfo()));

	private UnknownFieldSet _unknownFields;

	public const int HistoryScoreFieldNumber = 1;

	private int historyScore_;

	public const int ActScoreFieldNumber = 2;

	private int actScore_;

	public const int BestScoreFieldNumber = 3;

	private int bestScore_;

	public const int ScoreFieldNumber = 4;

	private int score_;

	public const int CleanFieldNumber = 5;

	private int clean_;

	public const int CommitTmFieldNumber = 6;

	private long commitTm_;

	public const int TargetListFieldNumber = 7;

	private static readonly FieldCodec<OperateTargetDetail> _repeated_targetList_codec = FieldCodec.ForMessage<OperateTargetDetail>(58u, OperateTargetDetail.Parser);

	private readonly RepeatedField<OperateTargetDetail> targetList_ = new RepeatedField<OperateTargetDetail>();

	public const int DailyBestScoreFieldNumber = 8;

	private int dailyBestScore_;

	public const int DailyAwardListFieldNumber = 9;

	private static readonly FieldCodec<OperateTargetDetail> _repeated_dailyAwardList_codec = FieldCodec.ForMessage<OperateTargetDetail>(74u, OperateTargetDetail.Parser);

	private readonly RepeatedField<OperateTargetDetail> dailyAwardList_ = new RepeatedField<OperateTargetDetail>();

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<FusionBaseInfo> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeFusionBaseInfoReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int HistoryScore
	{
		get
		{
			return historyScore_;
		}
		set
		{
			historyScore_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int ActScore
	{
		get
		{
			return actScore_;
		}
		set
		{
			actScore_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int BestScore
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
	public int Clean
	{
		get
		{
			return clean_;
		}
		set
		{
			clean_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public long CommitTm
	{
		get
		{
			return commitTm_;
		}
		set
		{
			commitTm_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<OperateTargetDetail> TargetList => targetList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int DailyBestScore
	{
		get
		{
			return dailyBestScore_;
		}
		set
		{
			dailyBestScore_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<OperateTargetDetail> DailyAwardList => dailyAwardList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public FusionBaseInfo()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public FusionBaseInfo(FusionBaseInfo other)
		: this()
	{
		historyScore_ = other.historyScore_;
		actScore_ = other.actScore_;
		bestScore_ = other.bestScore_;
		score_ = other.score_;
		clean_ = other.clean_;
		commitTm_ = other.commitTm_;
		targetList_ = other.targetList_.Clone();
		dailyBestScore_ = other.dailyBestScore_;
		dailyAwardList_ = other.dailyAwardList_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public FusionBaseInfo Clone()
	{
		return new FusionBaseInfo(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as FusionBaseInfo);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(FusionBaseInfo other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (HistoryScore != other.HistoryScore)
		{
			return false;
		}
		if (ActScore != other.ActScore)
		{
			return false;
		}
		if (BestScore != other.BestScore)
		{
			return false;
		}
		if (Score != other.Score)
		{
			return false;
		}
		if (Clean != other.Clean)
		{
			return false;
		}
		if (CommitTm != other.CommitTm)
		{
			return false;
		}
		if (!targetList_.Equals(other.targetList_))
		{
			return false;
		}
		if (DailyBestScore != other.DailyBestScore)
		{
			return false;
		}
		if (!dailyAwardList_.Equals(other.dailyAwardList_))
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
		if (HistoryScore != 0)
		{
			num ^= HistoryScore.GetHashCode();
		}
		if (ActScore != 0)
		{
			num ^= ActScore.GetHashCode();
		}
		if (BestScore != 0)
		{
			num ^= BestScore.GetHashCode();
		}
		if (Score != 0)
		{
			num ^= Score.GetHashCode();
		}
		if (Clean != 0)
		{
			num ^= Clean.GetHashCode();
		}
		if (CommitTm != 0L)
		{
			num ^= CommitTm.GetHashCode();
		}
		num ^= ((object)targetList_).GetHashCode();
		if (DailyBestScore != 0)
		{
			num ^= DailyBestScore.GetHashCode();
		}
		num ^= ((object)dailyAwardList_).GetHashCode();
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
		if (HistoryScore != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteInt32(HistoryScore);
		}
		if (ActScore != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteInt32(ActScore);
		}
		if (BestScore != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)24);
			((WriteContext)(ref output)).WriteInt32(BestScore);
		}
		if (Score != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)32);
			((WriteContext)(ref output)).WriteInt32(Score);
		}
		if (Clean != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)40);
			((WriteContext)(ref output)).WriteInt32(Clean);
		}
		if (CommitTm != 0L)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)48);
			((WriteContext)(ref output)).WriteInt64(CommitTm);
		}
		targetList_.WriteTo(ref output, _repeated_targetList_codec);
		if (DailyBestScore != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)64);
			((WriteContext)(ref output)).WriteInt32(DailyBestScore);
		}
		dailyAwardList_.WriteTo(ref output, _repeated_dailyAwardList_codec);
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
		if (HistoryScore != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(HistoryScore);
		}
		if (ActScore != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(ActScore);
		}
		if (BestScore != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(BestScore);
		}
		if (Score != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(Score);
		}
		if (Clean != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(Clean);
		}
		if (CommitTm != 0L)
		{
			num += 1 + CodedOutputStream.ComputeInt64Size(CommitTm);
		}
		num += targetList_.CalculateSize(_repeated_targetList_codec);
		if (DailyBestScore != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(DailyBestScore);
		}
		num += dailyAwardList_.CalculateSize(_repeated_dailyAwardList_codec);
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(FusionBaseInfo other)
	{
		if (other != null)
		{
			if (other.HistoryScore != 0)
			{
				HistoryScore = other.HistoryScore;
			}
			if (other.ActScore != 0)
			{
				ActScore = other.ActScore;
			}
			if (other.BestScore != 0)
			{
				BestScore = other.BestScore;
			}
			if (other.Score != 0)
			{
				Score = other.Score;
			}
			if (other.Clean != 0)
			{
				Clean = other.Clean;
			}
			if (other.CommitTm != 0L)
			{
				CommitTm = other.CommitTm;
			}
			targetList_.Add((IEnumerable<OperateTargetDetail>)other.targetList_);
			if (other.DailyBestScore != 0)
			{
				DailyBestScore = other.DailyBestScore;
			}
			dailyAwardList_.Add((IEnumerable<OperateTargetDetail>)other.dailyAwardList_);
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
				HistoryScore = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 16u:
				ActScore = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 24u:
				BestScore = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 32u:
				Score = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 40u:
				Clean = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 48u:
				CommitTm = ((ParseContext)(ref input)).ReadInt64();
				break;
			case 58u:
				targetList_.AddEntriesFrom(ref input, _repeated_targetList_codec);
				break;
			case 64u:
				DailyBestScore = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 74u:
				dailyAwardList_.AddEntriesFrom(ref input, _repeated_dailyAwardList_codec);
				break;
			}
		}
	}
}
