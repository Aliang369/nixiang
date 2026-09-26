using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class HomelandV2ReplaceWorkerSelectReq : IMessage<HomelandV2ReplaceWorkerSelectReq>, IMessage, IEquatable<HomelandV2ReplaceWorkerSelectReq>, IDeepCloneable<HomelandV2ReplaceWorkerSelectReq>, IBufferMessage
{
	private static readonly MessageParser<HomelandV2ReplaceWorkerSelectReq> _parser = new MessageParser<HomelandV2ReplaceWorkerSelectReq>((Func<HomelandV2ReplaceWorkerSelectReq>)(() => new HomelandV2ReplaceWorkerSelectReq()));

	private UnknownFieldSet _unknownFields;

	public const int WorkerIdFieldNumber = 1;

	private int workerId_;

	public const int SelectIdFieldNumber = 2;

	private int selectId_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<HomelandV2ReplaceWorkerSelectReq> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeHomelandV2ReplaceWorkerSelectReqReflection.Descriptor.MessageTypes[0];

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
	public int SelectId
	{
		get
		{
			return selectId_;
		}
		set
		{
			selectId_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public HomelandV2ReplaceWorkerSelectReq()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public HomelandV2ReplaceWorkerSelectReq(HomelandV2ReplaceWorkerSelectReq other)
		: this()
	{
		workerId_ = other.workerId_;
		selectId_ = other.selectId_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public HomelandV2ReplaceWorkerSelectReq Clone()
	{
		return new HomelandV2ReplaceWorkerSelectReq(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as HomelandV2ReplaceWorkerSelectReq);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(HomelandV2ReplaceWorkerSelectReq other)
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
		if (SelectId != other.SelectId)
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
		if (SelectId != 0)
		{
			num ^= SelectId.GetHashCode();
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
		if (SelectId != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteInt32(SelectId);
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
		if (SelectId != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(SelectId);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(HomelandV2ReplaceWorkerSelectReq other)
	{
		if (other != null)
		{
			if (other.WorkerId != 0)
			{
				WorkerId = other.WorkerId;
			}
			if (other.SelectId != 0)
			{
				SelectId = other.SelectId;
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
				WorkerId = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 16u:
				SelectId = ((ParseContext)(ref input)).ReadInt32();
				break;
			}
		}
	}
}
