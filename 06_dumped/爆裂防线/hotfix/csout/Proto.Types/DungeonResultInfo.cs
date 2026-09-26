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
public sealed class DungeonResultInfo : IMessage<DungeonResultInfo>, IMessage, IEquatable<DungeonResultInfo>, IDeepCloneable<DungeonResultInfo>, IBufferMessage
{
	private static readonly MessageParser<DungeonResultInfo> _parser = new MessageParser<DungeonResultInfo>((Func<DungeonResultInfo>)(() => new DungeonResultInfo()));

	private UnknownFieldSet _unknownFields;

	public const int DungeonIdFieldNumber = 1;

	private int dungeonId_;

	public const int PassStateFieldNumber = 2;

	private EPassState passState_;

	public const int EventCountFieldNumber = 3;

	private int eventCount_;

	public const int TriggerCountFieldNumber = 4;

	private int triggerCount_;

	public const int KillCountFieldNumber = 5;

	private int killCount_;

	public const int AwardListFieldNumber = 6;

	private static readonly FieldCodec<DungeonResultAwardDetail> _repeated_awardList_codec = FieldCodec.ForMessage<DungeonResultAwardDetail>(50u, DungeonResultAwardDetail.Parser);

	private readonly RepeatedField<DungeonResultAwardDetail> awardList_ = new RepeatedField<DungeonResultAwardDetail>();

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<DungeonResultInfo> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeDungeonResultInfoReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int DungeonId
	{
		get
		{
			return dungeonId_;
		}
		set
		{
			dungeonId_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public EPassState PassState
	{
		get
		{
			return passState_;
		}
		set
		{
			passState_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int EventCount
	{
		get
		{
			return eventCount_;
		}
		set
		{
			eventCount_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int TriggerCount
	{
		get
		{
			return triggerCount_;
		}
		set
		{
			triggerCount_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int KillCount
	{
		get
		{
			return killCount_;
		}
		set
		{
			killCount_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<DungeonResultAwardDetail> AwardList => awardList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public DungeonResultInfo()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public DungeonResultInfo(DungeonResultInfo other)
		: this()
	{
		dungeonId_ = other.dungeonId_;
		passState_ = other.passState_;
		eventCount_ = other.eventCount_;
		triggerCount_ = other.triggerCount_;
		killCount_ = other.killCount_;
		awardList_ = other.awardList_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public DungeonResultInfo Clone()
	{
		return new DungeonResultInfo(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as DungeonResultInfo);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(DungeonResultInfo other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (DungeonId != other.DungeonId)
		{
			return false;
		}
		if (PassState != other.PassState)
		{
			return false;
		}
		if (EventCount != other.EventCount)
		{
			return false;
		}
		if (TriggerCount != other.TriggerCount)
		{
			return false;
		}
		if (KillCount != other.KillCount)
		{
			return false;
		}
		if (!awardList_.Equals(other.awardList_))
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
		if (DungeonId != 0)
		{
			num ^= DungeonId.GetHashCode();
		}
		if (PassState != EPassState.NotStarted)
		{
			num ^= PassState.GetHashCode();
		}
		if (EventCount != 0)
		{
			num ^= EventCount.GetHashCode();
		}
		if (TriggerCount != 0)
		{
			num ^= TriggerCount.GetHashCode();
		}
		if (KillCount != 0)
		{
			num ^= KillCount.GetHashCode();
		}
		num ^= ((object)awardList_).GetHashCode();
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
		if (DungeonId != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteInt32(DungeonId);
		}
		if (PassState != EPassState.NotStarted)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteEnum((int)PassState);
		}
		if (EventCount != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)24);
			((WriteContext)(ref output)).WriteInt32(EventCount);
		}
		if (TriggerCount != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)32);
			((WriteContext)(ref output)).WriteInt32(TriggerCount);
		}
		if (KillCount != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)40);
			((WriteContext)(ref output)).WriteInt32(KillCount);
		}
		awardList_.WriteTo(ref output, _repeated_awardList_codec);
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
		if (DungeonId != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(DungeonId);
		}
		if (PassState != EPassState.NotStarted)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)PassState);
		}
		if (EventCount != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(EventCount);
		}
		if (TriggerCount != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(TriggerCount);
		}
		if (KillCount != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(KillCount);
		}
		num += awardList_.CalculateSize(_repeated_awardList_codec);
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(DungeonResultInfo other)
	{
		if (other != null)
		{
			if (other.DungeonId != 0)
			{
				DungeonId = other.DungeonId;
			}
			if (other.PassState != EPassState.NotStarted)
			{
				PassState = other.PassState;
			}
			if (other.EventCount != 0)
			{
				EventCount = other.EventCount;
			}
			if (other.TriggerCount != 0)
			{
				TriggerCount = other.TriggerCount;
			}
			if (other.KillCount != 0)
			{
				KillCount = other.KillCount;
			}
			awardList_.Add((IEnumerable<DungeonResultAwardDetail>)other.awardList_);
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
				DungeonId = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 16u:
				PassState = (EPassState)((ParseContext)(ref input)).ReadEnum();
				break;
			case 24u:
				EventCount = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 32u:
				TriggerCount = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 40u:
				KillCount = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 50u:
				awardList_.AddEntriesFrom(ref input, _repeated_awardList_codec);
				break;
			}
		}
	}
}
