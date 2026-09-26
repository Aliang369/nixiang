using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class WorkerDetail : IMessage<WorkerDetail>, IMessage, IEquatable<WorkerDetail>, IDeepCloneable<WorkerDetail>, IBufferMessage
{
	private static readonly MessageParser<WorkerDetail> _parser = new MessageParser<WorkerDetail>((Func<WorkerDetail>)(() => new WorkerDetail()));

	private UnknownFieldSet _unknownFields;

	public const int WorkerIdFieldNumber = 1;

	private int workerId_;

	public const int StyleInfoFieldNumber = 2;

	private WorkerStyleInfo styleInfo_;

	public const int RoomIdFieldNumber = 3;

	private int roomId_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<WorkerDetail> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeWorkerDetailReflection.Descriptor.MessageTypes[0];

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
	public WorkerStyleInfo StyleInfo
	{
		get
		{
			return styleInfo_;
		}
		set
		{
			styleInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int RoomId
	{
		get
		{
			return roomId_;
		}
		set
		{
			roomId_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public WorkerDetail()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public WorkerDetail(WorkerDetail other)
		: this()
	{
		workerId_ = other.workerId_;
		styleInfo_ = ((other.styleInfo_ != null) ? other.styleInfo_.Clone() : null);
		roomId_ = other.roomId_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public WorkerDetail Clone()
	{
		return new WorkerDetail(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as WorkerDetail);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(WorkerDetail other)
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
		if (!object.Equals(StyleInfo, other.StyleInfo))
		{
			return false;
		}
		if (RoomId != other.RoomId)
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
		if (styleInfo_ != null)
		{
			num ^= StyleInfo.GetHashCode();
		}
		if (RoomId != 0)
		{
			num ^= RoomId.GetHashCode();
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
		if (styleInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)18);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)StyleInfo);
		}
		if (RoomId != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)24);
			((WriteContext)(ref output)).WriteInt32(RoomId);
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
		if (styleInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)StyleInfo);
		}
		if (RoomId != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(RoomId);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(WorkerDetail other)
	{
		if (other == null)
		{
			return;
		}
		if (other.WorkerId != 0)
		{
			WorkerId = other.WorkerId;
		}
		if (other.styleInfo_ != null)
		{
			if (styleInfo_ == null)
			{
				StyleInfo = new WorkerStyleInfo();
			}
			StyleInfo.MergeFrom(other.StyleInfo);
		}
		if (other.RoomId != 0)
		{
			RoomId = other.RoomId;
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
				if (styleInfo_ == null)
				{
					StyleInfo = new WorkerStyleInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)StyleInfo);
				break;
			case 24u:
				RoomId = ((ParseContext)(ref input)).ReadInt32();
				break;
			}
		}
	}
}
