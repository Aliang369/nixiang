using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class NoviceLogin : IMessage<NoviceLogin>, IMessage, IEquatable<NoviceLogin>, IDeepCloneable<NoviceLogin>, IBufferMessage
{
	private static readonly MessageParser<NoviceLogin> _parser = new MessageParser<NoviceLogin>((Func<NoviceLogin>)(() => new NoviceLogin()));

	private UnknownFieldSet _unknownFields;

	public const int StartTmFieldNumber = 1;

	private long startTm_;

	public const int DayListFieldNumber = 2;

	private static readonly FieldCodec<NoviceLoginDayDetail> _repeated_dayList_codec = FieldCodec.ForMessage<NoviceLoginDayDetail>(18u, NoviceLoginDayDetail.Parser);

	private readonly RepeatedField<NoviceLoginDayDetail> dayList_ = new RepeatedField<NoviceLoginDayDetail>();

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<NoviceLogin> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeNoviceLoginReflection.Descriptor.MessageTypes[0];

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
	public RepeatedField<NoviceLoginDayDetail> DayList => dayList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public NoviceLogin()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public NoviceLogin(NoviceLogin other)
		: this()
	{
		startTm_ = other.startTm_;
		dayList_ = other.dayList_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public NoviceLogin Clone()
	{
		return new NoviceLogin(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as NoviceLogin);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(NoviceLogin other)
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
		if (!dayList_.Equals(other.dayList_))
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
		num ^= ((object)dayList_).GetHashCode();
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
		dayList_.WriteTo(ref output, _repeated_dayList_codec);
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
		num += dayList_.CalculateSize(_repeated_dayList_codec);
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(NoviceLogin other)
	{
		if (other != null)
		{
			if (other.StartTm != 0L)
			{
				StartTm = other.StartTm;
			}
			dayList_.Add((IEnumerable<NoviceLoginDayDetail>)other.dayList_);
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
			case 18u:
				dayList_.AddEntriesFrom(ref input, _repeated_dayList_codec);
				break;
			}
		}
	}
}
