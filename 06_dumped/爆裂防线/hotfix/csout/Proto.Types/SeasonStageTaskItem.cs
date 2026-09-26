using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class SeasonStageTaskItem : IMessage<SeasonStageTaskItem>, IMessage, IEquatable<SeasonStageTaskItem>, IDeepCloneable<SeasonStageTaskItem>, IBufferMessage
{
	private static readonly MessageParser<SeasonStageTaskItem> _parser = new MessageParser<SeasonStageTaskItem>((Func<SeasonStageTaskItem>)(() => new SeasonStageTaskItem()));

	private UnknownFieldSet _unknownFields;

	public const int TidFieldNumber = 1;

	private int tid_;

	public const int TaskTypeFieldNumber = 2;

	private int taskType_;

	public const int StepIdFieldNumber = 3;

	private int stepId_;

	public const int TargetTypeFieldNumber = 4;

	private int targetType_;

	public const int ProgressFieldNumber = 5;

	private int progress_;

	public const int TargetFieldNumber = 6;

	private int target_;

	public const int ParamListFieldNumber = 7;

	private static readonly FieldCodec<int> _repeated_paramList_codec = FieldCodec.ForInt32(58u);

	private readonly RepeatedField<int> paramList_ = new RepeatedField<int>();

	public const int ClaimedFieldNumber = 8;

	private int claimed_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<SeasonStageTaskItem> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeSeasonStageTaskItemReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int Tid
	{
		get
		{
			return tid_;
		}
		set
		{
			tid_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int TaskType
	{
		get
		{
			return taskType_;
		}
		set
		{
			taskType_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int StepId
	{
		get
		{
			return stepId_;
		}
		set
		{
			stepId_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int TargetType
	{
		get
		{
			return targetType_;
		}
		set
		{
			targetType_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int Progress
	{
		get
		{
			return progress_;
		}
		set
		{
			progress_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int Target
	{
		get
		{
			return target_;
		}
		set
		{
			target_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<int> ParamList => paramList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int Claimed
	{
		get
		{
			return claimed_;
		}
		set
		{
			claimed_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SeasonStageTaskItem()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SeasonStageTaskItem(SeasonStageTaskItem other)
		: this()
	{
		tid_ = other.tid_;
		taskType_ = other.taskType_;
		stepId_ = other.stepId_;
		targetType_ = other.targetType_;
		progress_ = other.progress_;
		target_ = other.target_;
		paramList_ = other.paramList_.Clone();
		claimed_ = other.claimed_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SeasonStageTaskItem Clone()
	{
		return new SeasonStageTaskItem(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as SeasonStageTaskItem);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(SeasonStageTaskItem other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (Tid != other.Tid)
		{
			return false;
		}
		if (TaskType != other.TaskType)
		{
			return false;
		}
		if (StepId != other.StepId)
		{
			return false;
		}
		if (TargetType != other.TargetType)
		{
			return false;
		}
		if (Progress != other.Progress)
		{
			return false;
		}
		if (Target != other.Target)
		{
			return false;
		}
		if (!paramList_.Equals(other.paramList_))
		{
			return false;
		}
		if (Claimed != other.Claimed)
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
		if (Tid != 0)
		{
			num ^= Tid.GetHashCode();
		}
		if (TaskType != 0)
		{
			num ^= TaskType.GetHashCode();
		}
		if (StepId != 0)
		{
			num ^= StepId.GetHashCode();
		}
		if (TargetType != 0)
		{
			num ^= TargetType.GetHashCode();
		}
		if (Progress != 0)
		{
			num ^= Progress.GetHashCode();
		}
		if (Target != 0)
		{
			num ^= Target.GetHashCode();
		}
		num ^= ((object)paramList_).GetHashCode();
		if (Claimed != 0)
		{
			num ^= Claimed.GetHashCode();
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
		if (Tid != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteInt32(Tid);
		}
		if (TaskType != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteInt32(TaskType);
		}
		if (StepId != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)24);
			((WriteContext)(ref output)).WriteInt32(StepId);
		}
		if (TargetType != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)32);
			((WriteContext)(ref output)).WriteInt32(TargetType);
		}
		if (Progress != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)40);
			((WriteContext)(ref output)).WriteInt32(Progress);
		}
		if (Target != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)48);
			((WriteContext)(ref output)).WriteInt32(Target);
		}
		paramList_.WriteTo(ref output, _repeated_paramList_codec);
		if (Claimed != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)64);
			((WriteContext)(ref output)).WriteInt32(Claimed);
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
		if (Tid != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(Tid);
		}
		if (TaskType != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(TaskType);
		}
		if (StepId != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(StepId);
		}
		if (TargetType != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(TargetType);
		}
		if (Progress != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(Progress);
		}
		if (Target != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(Target);
		}
		num += paramList_.CalculateSize(_repeated_paramList_codec);
		if (Claimed != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(Claimed);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(SeasonStageTaskItem other)
	{
		if (other != null)
		{
			if (other.Tid != 0)
			{
				Tid = other.Tid;
			}
			if (other.TaskType != 0)
			{
				TaskType = other.TaskType;
			}
			if (other.StepId != 0)
			{
				StepId = other.StepId;
			}
			if (other.TargetType != 0)
			{
				TargetType = other.TargetType;
			}
			if (other.Progress != 0)
			{
				Progress = other.Progress;
			}
			if (other.Target != 0)
			{
				Target = other.Target;
			}
			paramList_.Add((IEnumerable<int>)other.paramList_);
			if (other.Claimed != 0)
			{
				Claimed = other.Claimed;
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
				Tid = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 16u:
				TaskType = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 24u:
				StepId = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 32u:
				TargetType = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 40u:
				Progress = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 48u:
				Target = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 56u:
			case 58u:
				paramList_.AddEntriesFrom(ref input, _repeated_paramList_codec);
				break;
			case 64u:
				Claimed = ((ParseContext)(ref input)).ReadInt32();
				break;
			}
		}
	}
}
