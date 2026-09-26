using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class SeasonStageTaskNtf : IMessage<SeasonStageTaskNtf>, IMessage, IEquatable<SeasonStageTaskNtf>, IDeepCloneable<SeasonStageTaskNtf>, IBufferMessage
{
	private static readonly MessageParser<SeasonStageTaskNtf> _parser = new MessageParser<SeasonStageTaskNtf>((Func<SeasonStageTaskNtf>)(() => new SeasonStageTaskNtf()));

	private UnknownFieldSet _unknownFields;

	public const int TidFieldNumber = 1;

	private int tid_;

	public const int TaskTypeFieldNumber = 2;

	private int taskType_;

	public const int ProgressFieldNumber = 3;

	private int progress_;

	public const int TargetFieldNumber = 4;

	private int target_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<SeasonStageTaskNtf> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeSeasonStageTaskNtfReflection.Descriptor.MessageTypes[0];

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
	public SeasonStageTaskNtf()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SeasonStageTaskNtf(SeasonStageTaskNtf other)
		: this()
	{
		tid_ = other.tid_;
		taskType_ = other.taskType_;
		progress_ = other.progress_;
		target_ = other.target_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SeasonStageTaskNtf Clone()
	{
		return new SeasonStageTaskNtf(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as SeasonStageTaskNtf);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(SeasonStageTaskNtf other)
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
		if (Progress != other.Progress)
		{
			return false;
		}
		if (Target != other.Target)
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
		if (Progress != 0)
		{
			num ^= Progress.GetHashCode();
		}
		if (Target != 0)
		{
			num ^= Target.GetHashCode();
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
		if (Progress != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)24);
			((WriteContext)(ref output)).WriteInt32(Progress);
		}
		if (Target != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)32);
			((WriteContext)(ref output)).WriteInt32(Target);
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
		if (Progress != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(Progress);
		}
		if (Target != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(Target);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(SeasonStageTaskNtf other)
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
			if (other.Progress != 0)
			{
				Progress = other.Progress;
			}
			if (other.Target != 0)
			{
				Target = other.Target;
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
				Progress = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 32u:
				Target = ((ParseContext)(ref input)).ReadInt32();
				break;
			}
		}
	}
}
