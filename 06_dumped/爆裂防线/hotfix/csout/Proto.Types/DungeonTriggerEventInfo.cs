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
public sealed class DungeonTriggerEventInfo : IMessage<DungeonTriggerEventInfo>, IMessage, IEquatable<DungeonTriggerEventInfo>, IDeepCloneable<DungeonTriggerEventInfo>, IBufferMessage
{
	private static readonly MessageParser<DungeonTriggerEventInfo> _parser = new MessageParser<DungeonTriggerEventInfo>((Func<DungeonTriggerEventInfo>)(() => new DungeonTriggerEventInfo()));

	private UnknownFieldSet _unknownFields;

	public const int CellIdFieldNumber = 1;

	private string cellId_ = "";

	public const int GridIdFieldNumber = 2;

	private int gridId_;

	public const int EventTypeFieldNumber = 3;

	private EDungeonEventType eventType_;

	public const int EventIdFieldNumber = 4;

	private int eventId_;

	public const int TriggerFieldNumber = 5;

	private EDungeonEventTrigger trigger_;

	public const int EventModListFieldNumber = 6;

	private static readonly FieldCodec<int> _repeated_eventModList_codec = FieldCodec.ForInt32(50u);

	private readonly RepeatedField<int> eventModList_ = new RepeatedField<int>();

	public const int EventPopupFieldNumber = 7;

	private DungeonEventPopupInfo eventPopup_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<DungeonTriggerEventInfo> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeDungeonTriggerEventInfoReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public string CellId
	{
		get
		{
			return cellId_;
		}
		set
		{
			cellId_ = ProtoPreconditions.CheckNotNull<string>(value, "value");
		}
	}

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
	public RepeatedField<int> EventModList => eventModList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public DungeonEventPopupInfo EventPopup
	{
		get
		{
			return eventPopup_;
		}
		set
		{
			eventPopup_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public DungeonTriggerEventInfo()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public DungeonTriggerEventInfo(DungeonTriggerEventInfo other)
		: this()
	{
		cellId_ = other.cellId_;
		gridId_ = other.gridId_;
		eventType_ = other.eventType_;
		eventId_ = other.eventId_;
		trigger_ = other.trigger_;
		eventModList_ = other.eventModList_.Clone();
		eventPopup_ = ((other.eventPopup_ != null) ? other.eventPopup_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public DungeonTriggerEventInfo Clone()
	{
		return new DungeonTriggerEventInfo(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as DungeonTriggerEventInfo);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(DungeonTriggerEventInfo other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (CellId != other.CellId)
		{
			return false;
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
		if (!eventModList_.Equals(other.eventModList_))
		{
			return false;
		}
		if (!object.Equals(EventPopup, other.EventPopup))
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
		if (CellId.Length != 0)
		{
			num ^= CellId.GetHashCode();
		}
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
		num ^= ((object)eventModList_).GetHashCode();
		if (eventPopup_ != null)
		{
			num ^= EventPopup.GetHashCode();
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
		if (CellId.Length != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)10);
			((WriteContext)(ref output)).WriteString(CellId);
		}
		if (GridId != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteInt32(GridId);
		}
		if (EventType != EDungeonEventType.EmptyValue)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)24);
			((WriteContext)(ref output)).WriteEnum((int)EventType);
		}
		if (EventId != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)32);
			((WriteContext)(ref output)).WriteInt32(EventId);
		}
		if (Trigger != EDungeonEventTrigger.EmptyValue)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)40);
			((WriteContext)(ref output)).WriteEnum((int)Trigger);
		}
		eventModList_.WriteTo(ref output, _repeated_eventModList_codec);
		if (eventPopup_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)58);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)EventPopup);
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
		if (CellId.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(CellId);
		}
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
		num += eventModList_.CalculateSize(_repeated_eventModList_codec);
		if (eventPopup_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)EventPopup);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(DungeonTriggerEventInfo other)
	{
		if (other == null)
		{
			return;
		}
		if (other.CellId.Length != 0)
		{
			CellId = other.CellId;
		}
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
		eventModList_.Add((IEnumerable<int>)other.eventModList_);
		if (other.eventPopup_ != null)
		{
			if (eventPopup_ == null)
			{
				EventPopup = new DungeonEventPopupInfo();
			}
			EventPopup.MergeFrom(other.EventPopup);
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
				CellId = ((ParseContext)(ref input)).ReadString();
				break;
			case 16u:
				GridId = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 24u:
				EventType = (EDungeonEventType)((ParseContext)(ref input)).ReadEnum();
				break;
			case 32u:
				EventId = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 40u:
				Trigger = (EDungeonEventTrigger)((ParseContext)(ref input)).ReadEnum();
				break;
			case 48u:
			case 50u:
				eventModList_.AddEntriesFrom(ref input, _repeated_eventModList_codec);
				break;
			case 58u:
				if (eventPopup_ == null)
				{
					EventPopup = new DungeonEventPopupInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)EventPopup);
				break;
			}
		}
	}
}
