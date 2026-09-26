using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class BossSubjectDetail : IMessage<BossSubjectDetail>, IMessage, IEquatable<BossSubjectDetail>, IDeepCloneable<BossSubjectDetail>, IBufferMessage
{
	private static readonly MessageParser<BossSubjectDetail> _parser = new MessageParser<BossSubjectDetail>((Func<BossSubjectDetail>)(() => new BossSubjectDetail()));

	private UnknownFieldSet _unknownFields;

	public const int BaseInfoFieldNumber = 1;

	private OperateBaseInfo baseInfo_;

	public const int BossSubjectFieldNumber = 2;

	private BossSubjectInfo bossSubject_;

	public const int ZhanLingInfoFieldNumber = 3;

	private ZhanLingCommonInfo zhanLingInfo_;

	public const int RouletteInfoFieldNumber = 4;

	private BossSubjectRouletteInfo rouletteInfo_;

	public const int TaskListFieldNumber = 5;

	private static readonly FieldCodec<TaskDetail> _repeated_taskList_codec = FieldCodec.ForMessage<TaskDetail>(42u, TaskDetail.Parser);

	private readonly RepeatedField<TaskDetail> taskList_ = new RepeatedField<TaskDetail>();

	public const int TaskCountFieldNumber = 6;

	private int taskCount_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<BossSubjectDetail> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeBossSubjectDetailReflection.Descriptor.MessageTypes[0];

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
	public BossSubjectInfo BossSubject
	{
		get
		{
			return bossSubject_;
		}
		set
		{
			bossSubject_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ZhanLingCommonInfo ZhanLingInfo
	{
		get
		{
			return zhanLingInfo_;
		}
		set
		{
			zhanLingInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public BossSubjectRouletteInfo RouletteInfo
	{
		get
		{
			return rouletteInfo_;
		}
		set
		{
			rouletteInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<TaskDetail> TaskList => taskList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int TaskCount
	{
		get
		{
			return taskCount_;
		}
		set
		{
			taskCount_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public BossSubjectDetail()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public BossSubjectDetail(BossSubjectDetail other)
		: this()
	{
		baseInfo_ = ((other.baseInfo_ != null) ? other.baseInfo_.Clone() : null);
		bossSubject_ = ((other.bossSubject_ != null) ? other.bossSubject_.Clone() : null);
		zhanLingInfo_ = ((other.zhanLingInfo_ != null) ? other.zhanLingInfo_.Clone() : null);
		rouletteInfo_ = ((other.rouletteInfo_ != null) ? other.rouletteInfo_.Clone() : null);
		taskList_ = other.taskList_.Clone();
		taskCount_ = other.taskCount_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public BossSubjectDetail Clone()
	{
		return new BossSubjectDetail(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as BossSubjectDetail);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(BossSubjectDetail other)
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
		if (!object.Equals(BossSubject, other.BossSubject))
		{
			return false;
		}
		if (!object.Equals(ZhanLingInfo, other.ZhanLingInfo))
		{
			return false;
		}
		if (!object.Equals(RouletteInfo, other.RouletteInfo))
		{
			return false;
		}
		if (!taskList_.Equals(other.taskList_))
		{
			return false;
		}
		if (TaskCount != other.TaskCount)
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
		if (bossSubject_ != null)
		{
			num ^= BossSubject.GetHashCode();
		}
		if (zhanLingInfo_ != null)
		{
			num ^= ZhanLingInfo.GetHashCode();
		}
		if (rouletteInfo_ != null)
		{
			num ^= RouletteInfo.GetHashCode();
		}
		num ^= ((object)taskList_).GetHashCode();
		if (TaskCount != 0)
		{
			num ^= TaskCount.GetHashCode();
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
		if (baseInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)10);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)BaseInfo);
		}
		if (bossSubject_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)18);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)BossSubject);
		}
		if (zhanLingInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)26);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)ZhanLingInfo);
		}
		if (rouletteInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)34);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)RouletteInfo);
		}
		taskList_.WriteTo(ref output, _repeated_taskList_codec);
		if (TaskCount != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)48);
			((WriteContext)(ref output)).WriteInt32(TaskCount);
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
		if (baseInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)BaseInfo);
		}
		if (bossSubject_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)BossSubject);
		}
		if (zhanLingInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)ZhanLingInfo);
		}
		if (rouletteInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)RouletteInfo);
		}
		num += taskList_.CalculateSize(_repeated_taskList_codec);
		if (TaskCount != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(TaskCount);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(BossSubjectDetail other)
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
		if (other.bossSubject_ != null)
		{
			if (bossSubject_ == null)
			{
				BossSubject = new BossSubjectInfo();
			}
			BossSubject.MergeFrom(other.BossSubject);
		}
		if (other.zhanLingInfo_ != null)
		{
			if (zhanLingInfo_ == null)
			{
				ZhanLingInfo = new ZhanLingCommonInfo();
			}
			ZhanLingInfo.MergeFrom(other.ZhanLingInfo);
		}
		if (other.rouletteInfo_ != null)
		{
			if (rouletteInfo_ == null)
			{
				RouletteInfo = new BossSubjectRouletteInfo();
			}
			RouletteInfo.MergeFrom(other.RouletteInfo);
		}
		taskList_.Add((IEnumerable<TaskDetail>)other.taskList_);
		if (other.TaskCount != 0)
		{
			TaskCount = other.TaskCount;
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
				if (baseInfo_ == null)
				{
					BaseInfo = new OperateBaseInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)BaseInfo);
				break;
			case 18u:
				if (bossSubject_ == null)
				{
					BossSubject = new BossSubjectInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)BossSubject);
				break;
			case 26u:
				if (zhanLingInfo_ == null)
				{
					ZhanLingInfo = new ZhanLingCommonInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)ZhanLingInfo);
				break;
			case 34u:
				if (rouletteInfo_ == null)
				{
					RouletteInfo = new BossSubjectRouletteInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)RouletteInfo);
				break;
			case 42u:
				taskList_.AddEntriesFrom(ref input, _repeated_taskList_codec);
				break;
			case 48u:
				TaskCount = ((ParseContext)(ref input)).ReadInt32();
				break;
			}
		}
	}
}
