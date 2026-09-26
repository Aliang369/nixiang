using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class SubscribeActiveInfo : IMessage<SubscribeActiveInfo>, IMessage, IEquatable<SubscribeActiveInfo>, IDeepCloneable<SubscribeActiveInfo>, IBufferMessage
{
	private static readonly MessageParser<SubscribeActiveInfo> _parser = new MessageParser<SubscribeActiveInfo>((Func<SubscribeActiveInfo>)(() => new SubscribeActiveInfo()));

	private UnknownFieldSet _unknownFields;

	public const int StartTmFieldNumber = 1;

	private long startTm_;

	public const int EndTmFieldNumber = 2;

	private long endTm_;

	public const int NextDailyFieldNumber = 3;

	private long nextDaily_;

	public const int NextWeekFieldNumber = 4;

	private long nextWeek_;

	public const int NextMonthFieldNumber = 5;

	private long nextMonth_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<SubscribeActiveInfo> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeSubscribeActiveInfoReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public long StartTm
	{
		get
		{
			return startTm_;
		}
		set
		{
			startTm_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public long EndTm
	{
		get
		{
			return endTm_;
		}
		set
		{
			endTm_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public long NextDaily
	{
		get
		{
			return nextDaily_;
		}
		set
		{
			nextDaily_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public long NextWeek
	{
		get
		{
			return nextWeek_;
		}
		set
		{
			nextWeek_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public long NextMonth
	{
		get
		{
			return nextMonth_;
		}
		set
		{
			nextMonth_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SubscribeActiveInfo()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SubscribeActiveInfo(SubscribeActiveInfo other)
		: this()
	{
		startTm_ = other.startTm_;
		endTm_ = other.endTm_;
		nextDaily_ = other.nextDaily_;
		nextWeek_ = other.nextWeek_;
		nextMonth_ = other.nextMonth_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SubscribeActiveInfo Clone()
	{
		return new SubscribeActiveInfo(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as SubscribeActiveInfo);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(SubscribeActiveInfo other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (StartTm != other.StartTm)
		{
			return false;
		}
		if (EndTm != other.EndTm)
		{
			return false;
		}
		if (NextDaily != other.NextDaily)
		{
			return false;
		}
		if (NextWeek != other.NextWeek)
		{
			return false;
		}
		if (NextMonth != other.NextMonth)
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
		if (StartTm != 0L)
		{
			num ^= StartTm.GetHashCode();
		}
		if (EndTm != 0L)
		{
			num ^= EndTm.GetHashCode();
		}
		if (NextDaily != 0L)
		{
			num ^= NextDaily.GetHashCode();
		}
		if (NextWeek != 0L)
		{
			num ^= NextWeek.GetHashCode();
		}
		if (NextMonth != 0L)
		{
			num ^= NextMonth.GetHashCode();
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
		if (StartTm != 0L)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteInt64(StartTm);
		}
		if (EndTm != 0L)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteInt64(EndTm);
		}
		if (NextDaily != 0L)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)24);
			((WriteContext)(ref output)).WriteInt64(NextDaily);
		}
		if (NextWeek != 0L)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)32);
			((WriteContext)(ref output)).WriteInt64(NextWeek);
		}
		if (NextMonth != 0L)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)40);
			((WriteContext)(ref output)).WriteInt64(NextMonth);
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
		if (StartTm != 0L)
		{
			num += 1 + CodedOutputStream.ComputeInt64Size(StartTm);
		}
		if (EndTm != 0L)
		{
			num += 1 + CodedOutputStream.ComputeInt64Size(EndTm);
		}
		if (NextDaily != 0L)
		{
			num += 1 + CodedOutputStream.ComputeInt64Size(NextDaily);
		}
		if (NextWeek != 0L)
		{
			num += 1 + CodedOutputStream.ComputeInt64Size(NextWeek);
		}
		if (NextMonth != 0L)
		{
			num += 1 + CodedOutputStream.ComputeInt64Size(NextMonth);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(SubscribeActiveInfo other)
	{
		if (other != null)
		{
			if (other.StartTm != 0L)
			{
				StartTm = other.StartTm;
			}
			if (other.EndTm != 0L)
			{
				EndTm = other.EndTm;
			}
			if (other.NextDaily != 0L)
			{
				NextDaily = other.NextDaily;
			}
			if (other.NextWeek != 0L)
			{
				NextWeek = other.NextWeek;
			}
			if (other.NextMonth != 0L)
			{
				NextMonth = other.NextMonth;
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
				StartTm = ((ParseContext)(ref input)).ReadInt64();
				break;
			case 16u:
				EndTm = ((ParseContext)(ref input)).ReadInt64();
				break;
			case 24u:
				NextDaily = ((ParseContext)(ref input)).ReadInt64();
				break;
			case 32u:
				NextWeek = ((ParseContext)(ref input)).ReadInt64();
				break;
			case 40u:
				NextMonth = ((ParseContext)(ref input)).ReadInt64();
				break;
			}
		}
	}
}
