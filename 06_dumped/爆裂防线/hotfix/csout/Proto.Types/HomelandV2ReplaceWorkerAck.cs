using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class HomelandV2ReplaceWorkerAck : IMessage<HomelandV2ReplaceWorkerAck>, IMessage, IEquatable<HomelandV2ReplaceWorkerAck>, IDeepCloneable<HomelandV2ReplaceWorkerAck>, IBufferMessage
{
	private static readonly MessageParser<HomelandV2ReplaceWorkerAck> _parser = new MessageParser<HomelandV2ReplaceWorkerAck>((Func<HomelandV2ReplaceWorkerAck>)(() => new HomelandV2ReplaceWorkerAck()));

	private UnknownFieldSet _unknownFields;

	public const int WorkerIdFieldNumber = 1;

	private int workerId_;

	public const int ReplaceListFieldNumber = 2;

	private static readonly FieldCodec<WorkerDetail> _repeated_replaceList_codec = FieldCodec.ForMessage<WorkerDetail>(18u, WorkerDetail.Parser);

	private readonly RepeatedField<WorkerDetail> replaceList_ = new RepeatedField<WorkerDetail>();

	public const int DataChangeFieldNumber = 3;

	private DataChange dataChange_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<HomelandV2ReplaceWorkerAck> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeHomelandV2ReplaceWorkerAckReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int WorkerId
	{
		get
		{
			return workerId_;
		}
		set
		{
			workerId_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<WorkerDetail> ReplaceList => replaceList_;

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
	public HomelandV2ReplaceWorkerAck()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public HomelandV2ReplaceWorkerAck(HomelandV2ReplaceWorkerAck other)
		: this()
	{
		workerId_ = other.workerId_;
		replaceList_ = other.replaceList_.Clone();
		dataChange_ = ((other.dataChange_ != null) ? other.dataChange_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public HomelandV2ReplaceWorkerAck Clone()
	{
		return new HomelandV2ReplaceWorkerAck(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as HomelandV2ReplaceWorkerAck);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(HomelandV2ReplaceWorkerAck other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (WorkerId != other.WorkerId)
		{
			return false;
		}
		if (!replaceList_.Equals(other.replaceList_))
		{
			return false;
		}
		if (!object.Equals(DataChange, other.DataChange))
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
		if (WorkerId != 0)
		{
			num ^= WorkerId.GetHashCode();
		}
		num ^= ((object)replaceList_).GetHashCode();
		if (dataChange_ != null)
		{
			num ^= DataChange.GetHashCode();
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
		if (WorkerId != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteInt32(WorkerId);
		}
		replaceList_.WriteTo(ref output, _repeated_replaceList_codec);
		if (dataChange_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)26);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)DataChange);
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
		if (WorkerId != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(WorkerId);
		}
		num += replaceList_.CalculateSize(_repeated_replaceList_codec);
		if (dataChange_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)DataChange);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(HomelandV2ReplaceWorkerAck other)
	{
		if (other == null)
		{
			return;
		}
		if (other.WorkerId != 0)
		{
			WorkerId = other.WorkerId;
		}
		replaceList_.Add((IEnumerable<WorkerDetail>)other.replaceList_);
		if (other.dataChange_ != null)
		{
			if (dataChange_ == null)
			{
				DataChange = new DataChange();
			}
			DataChange.MergeFrom(other.DataChange);
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
				WorkerId = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 18u:
				replaceList_.AddEntriesFrom(ref input, _repeated_replaceList_codec);
				break;
			case 26u:
				if (dataChange_ == null)
				{
					DataChange = new DataChange();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)DataChange);
				break;
			}
		}
	}
}
