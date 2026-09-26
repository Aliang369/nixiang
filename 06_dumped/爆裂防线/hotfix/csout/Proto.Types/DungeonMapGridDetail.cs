using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class DungeonMapGridDetail : IMessage<DungeonMapGridDetail>, IMessage, IEquatable<DungeonMapGridDetail>, IDeepCloneable<DungeonMapGridDetail>, IBufferMessage
{
	private static readonly MessageParser<DungeonMapGridDetail> _parser = new MessageParser<DungeonMapGridDetail>((Func<DungeonMapGridDetail>)(() => new DungeonMapGridDetail()));

	private UnknownFieldSet _unknownFields;

	public const int GridIdFieldNumber = 1;

	private int gridId_;

	public const int EventTypeFieldNumber = 2;

	private EDungeonEventType eventType_;

	public const int EventIdFieldNumber = 3;

	private int eventId_;

	public const int TriggerFieldNumber = 4;

	private EDungeonEventTrigger trigger_;

	public const int StateFieldNumber = 5;

	private EActiveState state_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<DungeonMapGridDetail> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeDungeonMapGridDetailReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int GridId
	{
		get
		{
			return gridId_;
		}
		set
		{
			gridId_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public EDungeonEventType EventType
	{
		get
		{
			return eventType_;
		}
		set
		{
			eventType_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int EventId
	{
		get
		{
			return eventId_;
		}
		set
		{
			eventId_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public EDungeonEventTrigger Trigger
	{
		get
		{
			return trigger_;
		}
		set
		{
			trigger_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public EActiveState State
	{
		get
		{
			return state_;
		}
		set
		{
			state_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public DungeonMapGridDetail()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public DungeonMapGridDetail(DungeonMapGridDetail other)
		: this()
	{
		gridId_ = other.gridId_;
		eventType_ = other.eventType_;
		eventId_ = other.eventId_;
		trigger_ = other.trigger_;
		state_ = other.state_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public DungeonMapGridDetail Clone()
	{
		return new DungeonMapGridDetail(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as DungeonMapGridDetail);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(DungeonMapGridDetail other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (GridId != other.GridId)
		{
			return false;
		}
		if (EventType != other.EventType)
		{
			return false;
		}
		if (EventId != other.EventId)
		{
			return false;
		}
		if (Trigger != other.Trigger)
		{
			return false;
		}
		if (State != other.State)
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
		if (GridId != 0)
		{
			num ^= GridId.GetHashCode();
		}
		if (EventType != EDungeonEventType.EmptyValue)
		{
			num ^= EventType.GetHashCode();
		}
		if (EventId != 0)
		{
			num ^= EventId.GetHashCode();
		}
		if (Trigger != EDungeonEventTrigger.EmptyValue)
		{
			num ^= Trigger.GetHashCode();
		}
		if (State != EActiveState.Pending)
		{
			num ^= State.GetHashCode();
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
		if (GridId != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteInt32(GridId);
		}
		if (EventType != EDungeonEventType.EmptyValue)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteEnum((int)EventType);
		}
		if (EventId != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)24);
			((WriteContext)(ref output)).WriteInt32(EventId);
		}
		if (Trigger != EDungeonEventTrigger.EmptyValue)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)32);
			((WriteContext)(ref output)).WriteEnum((int)Trigger);
		}
		if (State != EActiveState.Pending)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)40);
			((WriteContext)(ref output)).WriteEnum((int)State);
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
		if (GridId != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(GridId);
		}
		if (EventType != EDungeonEventType.EmptyValue)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)EventType);
		}
		if (EventId != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(EventId);
		}
		if (Trigger != EDungeonEventTrigger.EmptyValue)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)Trigger);
		}
		if (State != EActiveState.Pending)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)State);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(DungeonMapGridDetail other)
	{
		if (other != null)
		{
			if (other.GridId != 0)
			{
				GridId = other.GridId;
			}
			if (other.EventType != EDungeonEventType.EmptyValue)
			{
				EventType = other.EventType;
			}
			if (other.EventId != 0)
			{
				EventId = other.EventId;
			}
			if (other.Trigger != EDungeonEventTrigger.EmptyValue)
			{
				Trigger = other.Trigger;
			}
			if (other.State != EActiveState.Pending)
			{
				State = other.State;
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
				GridId = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 16u:
				EventType = (EDungeonEventType)((ParseContext)(ref input)).ReadEnum();
				break;
			case 24u:
				EventId = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 32u:
				Trigger = (EDungeonEventTrigger)((ParseContext)(ref input)).ReadEnum();
				break;
			case 40u:
				State = (EActiveState)((ParseContext)(ref input)).ReadEnum();
				break;
			}
		}
	}
}
