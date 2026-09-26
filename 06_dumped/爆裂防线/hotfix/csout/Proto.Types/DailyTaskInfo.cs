using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class DailyTaskInfo : IMessage<DailyTaskInfo>, IMessage, IEquatable<DailyTaskInfo>, IDeepCloneable<DailyTaskInfo>, IBufferMessage
{
	private static readonly MessageParser<DailyTaskInfo> _parser = new MessageParser<DailyTaskInfo>((Func<DailyTaskInfo>)(() => new DailyTaskInfo()));

	private UnknownFieldSet _unknownFields;

	public const int TaskListFieldNumber = 1;

	private static readonly FieldCodec<TaskDetail> _repeated_taskList_codec = FieldCodec.ForMessage<TaskDetail>(10u, TaskDetail.Parser);

	private readonly RepeatedField<TaskDetail> taskList_ = new RepeatedField<TaskDetail>();

	public const int TaskTargetFieldNumber = 2;

	private TaskTarget taskTarget_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<DailyTaskInfo> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeDailyTaskInfoReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<TaskDetail> TaskList => taskList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public TaskTarget TaskTarget
	{
		get
		{
			return taskTarget_;
		}
		set
		{
			taskTarget_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public DailyTaskInfo()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public DailyTaskInfo(DailyTaskInfo other)
		: this()
	{
		taskList_ = other.taskList_.Clone();
		taskTarget_ = ((other.taskTarget_ != null) ? other.taskTarget_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public DailyTaskInfo Clone()
	{
		return new DailyTaskInfo(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as DailyTaskInfo);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(DailyTaskInfo other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!taskList_.Equals(other.taskList_))
		{
			return false;
		}
		if (!object.Equals(TaskTarget, other.TaskTarget))
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
		num ^= ((object)taskList_).GetHashCode();
		if (taskTarget_ != null)
		{
			num ^= TaskTarget.GetHashCode();
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
		taskList_.WriteTo(ref output, _repeated_taskList_codec);
		if (taskTarget_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)18);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)TaskTarget);
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
		num += taskList_.CalculateSize(_repeated_taskList_codec);
		if (taskTarget_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)TaskTarget);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(DailyTaskInfo other)
	{
		if (other == null)
		{
			return;
		}
		taskList_.Add((IEnumerable<TaskDetail>)other.taskList_);
		if (other.taskTarget_ != null)
		{
			if (taskTarget_ == null)
			{
				TaskTarget = new TaskTarget();
			}
			TaskTarget.MergeFrom(other.TaskTarget);
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
				taskList_.AddEntriesFrom(ref input, _repeated_taskList_codec);
				break;
			case 18u:
				if (taskTarget_ == null)
				{
					TaskTarget = new TaskTarget();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)TaskTarget);
				break;
			}
		}
	}
}
