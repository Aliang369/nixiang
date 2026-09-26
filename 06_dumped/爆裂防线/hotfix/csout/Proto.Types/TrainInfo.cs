using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class TrainInfo : IMessage<TrainInfo>, IMessage, IEquatable<TrainInfo>, IDeepCloneable<TrainInfo>, IBufferMessage
{
	private static readonly MessageParser<TrainInfo> _parser = new MessageParser<TrainInfo>((Func<TrainInfo>)(() => new TrainInfo()));

	private UnknownFieldSet _unknownFields;

	public const int UnlockListFieldNumber = 1;

	private static readonly FieldCodec<int> _repeated_unlockList_codec = FieldCodec.ForInt32(10u);

	private readonly RepeatedField<int> unlockList_ = new RepeatedField<int>();

	public const int SelfTrainFieldNumber = 2;

	private TrainDetail selfTrain_;

	public const int WaitTrainFieldNumber = 3;

	private TrainDetail waitTrain_;

	public const int MatchInfoFieldNumber = 4;

	private TrainMatchInfo matchInfo_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<TrainInfo> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeTrainInfoReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<int> UnlockList => unlockList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public TrainDetail SelfTrain
	{
		get
		{
			return selfTrain_;
		}
		set
		{
			selfTrain_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public TrainDetail WaitTrain
	{
		get
		{
			return waitTrain_;
		}
		set
		{
			waitTrain_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public TrainMatchInfo MatchInfo
	{
		get
		{
			return matchInfo_;
		}
		set
		{
			matchInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public TrainInfo()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public TrainInfo(TrainInfo other)
		: this()
	{
		unlockList_ = other.unlockList_.Clone();
		selfTrain_ = ((other.selfTrain_ != null) ? other.selfTrain_.Clone() : null);
		waitTrain_ = ((other.waitTrain_ != null) ? other.waitTrain_.Clone() : null);
		matchInfo_ = ((other.matchInfo_ != null) ? other.matchInfo_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public TrainInfo Clone()
	{
		return new TrainInfo(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as TrainInfo);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(TrainInfo other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!unlockList_.Equals(other.unlockList_))
		{
			return false;
		}
		if (!object.Equals(SelfTrain, other.SelfTrain))
		{
			return false;
		}
		if (!object.Equals(WaitTrain, other.WaitTrain))
		{
			return false;
		}
		if (!object.Equals(MatchInfo, other.MatchInfo))
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
		num ^= ((object)unlockList_).GetHashCode();
		if (selfTrain_ != null)
		{
			num ^= SelfTrain.GetHashCode();
		}
		if (waitTrain_ != null)
		{
			num ^= WaitTrain.GetHashCode();
		}
		if (matchInfo_ != null)
		{
			num ^= MatchInfo.GetHashCode();
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
		unlockList_.WriteTo(ref output, _repeated_unlockList_codec);
		if (selfTrain_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)18);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)SelfTrain);
		}
		if (waitTrain_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)26);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)WaitTrain);
		}
		if (matchInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)34);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)MatchInfo);
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
		num += unlockList_.CalculateSize(_repeated_unlockList_codec);
		if (selfTrain_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)SelfTrain);
		}
		if (waitTrain_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)WaitTrain);
		}
		if (matchInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)MatchInfo);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(TrainInfo other)
	{
		if (other == null)
		{
			return;
		}
		unlockList_.Add((IEnumerable<int>)other.unlockList_);
		if (other.selfTrain_ != null)
		{
			if (selfTrain_ == null)
			{
				SelfTrain = new TrainDetail();
			}
			SelfTrain.MergeFrom(other.SelfTrain);
		}
		if (other.waitTrain_ != null)
		{
			if (waitTrain_ == null)
			{
				WaitTrain = new TrainDetail();
			}
			WaitTrain.MergeFrom(other.WaitTrain);
		}
		if (other.matchInfo_ != null)
		{
			if (matchInfo_ == null)
			{
				MatchInfo = new TrainMatchInfo();
			}
			MatchInfo.MergeFrom(other.MatchInfo);
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
			case 10u:
				unlockList_.AddEntriesFrom(ref input, _repeated_unlockList_codec);
				break;
			case 18u:
				if (selfTrain_ == null)
				{
					SelfTrain = new TrainDetail();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)SelfTrain);
				break;
			case 26u:
				if (waitTrain_ == null)
				{
					WaitTrain = new TrainDetail();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)WaitTrain);
				break;
			case 34u:
				if (matchInfo_ == null)
				{
					MatchInfo = new TrainMatchInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)MatchInfo);
				break;
			}
		}
	}
}
