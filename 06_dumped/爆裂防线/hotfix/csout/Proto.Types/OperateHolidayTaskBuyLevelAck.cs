using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class OperateHolidayTaskBuyLevelAck : IMessage<OperateHolidayTaskBuyLevelAck>, IMessage, IEquatable<OperateHolidayTaskBuyLevelAck>, IDeepCloneable<OperateHolidayTaskBuyLevelAck>, IBufferMessage
{
	private static readonly MessageParser<OperateHolidayTaskBuyLevelAck> _parser = new MessageParser<OperateHolidayTaskBuyLevelAck>((Func<OperateHolidayTaskBuyLevelAck>)(() => new OperateHolidayTaskBuyLevelAck()));

	private UnknownFieldSet _unknownFields;

	public const int HolidayTaskListFieldNumber = 1;

	private static readonly FieldCodec<HolidayTaskDetail> _repeated_holidayTaskList_codec = FieldCodec.ForMessage<HolidayTaskDetail>(10u, HolidayTaskDetail.Parser);

	private readonly RepeatedField<HolidayTaskDetail> holidayTaskList_ = new RepeatedField<HolidayTaskDetail>();

	public const int DataChangeFieldNumber = 2;

	private DataChange dataChange_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<OperateHolidayTaskBuyLevelAck> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeOperateHolidayTaskBuyLevelAckReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<HolidayTaskDetail> HolidayTaskList => holidayTaskList_;

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
	public OperateHolidayTaskBuyLevelAck()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public OperateHolidayTaskBuyLevelAck(OperateHolidayTaskBuyLevelAck other)
		: this()
	{
		holidayTaskList_ = other.holidayTaskList_.Clone();
		dataChange_ = ((other.dataChange_ != null) ? other.dataChange_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public OperateHolidayTaskBuyLevelAck Clone()
	{
		return new OperateHolidayTaskBuyLevelAck(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as OperateHolidayTaskBuyLevelAck);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(OperateHolidayTaskBuyLevelAck other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!holidayTaskList_.Equals(other.holidayTaskList_))
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
		num ^= ((object)holidayTaskList_).GetHashCode();
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
		holidayTaskList_.WriteTo(ref output, _repeated_holidayTaskList_codec);
		if (dataChange_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)18);
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
		num += holidayTaskList_.CalculateSize(_repeated_holidayTaskList_codec);
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
	public void MergeFrom(OperateHolidayTaskBuyLevelAck other)
	{
		if (other == null)
		{
			return;
		}
		holidayTaskList_.Add((IEnumerable<HolidayTaskDetail>)other.holidayTaskList_);
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
			case 10u:
				holidayTaskList_.AddEntriesFrom(ref input, _repeated_holidayTaskList_codec);
				break;
			case 18u:
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
