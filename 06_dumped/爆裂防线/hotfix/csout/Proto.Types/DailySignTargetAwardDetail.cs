using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class DailySignTargetAwardDetail : IMessage<DailySignTargetAwardDetail>, IMessage, IEquatable<DailySignTargetAwardDetail>, IDeepCloneable<DailySignTargetAwardDetail>, IBufferMessage
{
	private static readonly MessageParser<DailySignTargetAwardDetail> _parser = new MessageParser<DailySignTargetAwardDetail>((Func<DailySignTargetAwardDetail>)(() => new DailySignTargetAwardDetail()));

	private UnknownFieldSet _unknownFields;

	public const int TargetIndexFieldNumber = 1;

	private int targetIndex_;

	public const int TargetStateFieldNumber = 2;

	private ECommonState targetState_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<DailySignTargetAwardDetail> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeDailySignTargetAwardDetailReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int TargetIndex
	{
		get
		{
			return targetIndex_;
		}
		set
		{
			targetIndex_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ECommonState TargetState
	{
		get
		{
			return targetState_;
		}
		set
		{
			targetState_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public DailySignTargetAwardDetail()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public DailySignTargetAwardDetail(DailySignTargetAwardDetail other)
		: this()
	{
		targetIndex_ = other.targetIndex_;
		targetState_ = other.targetState_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public DailySignTargetAwardDetail Clone()
	{
		return new DailySignTargetAwardDetail(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as DailySignTargetAwardDetail);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(DailySignTargetAwardDetail other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (TargetIndex != other.TargetIndex)
		{
			return false;
		}
		if (TargetState != other.TargetState)
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
		if (TargetIndex != 0)
		{
			num ^= TargetIndex.GetHashCode();
		}
		if (TargetState != ECommonState.No)
		{
			num ^= TargetState.GetHashCode();
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
		if (TargetIndex != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteInt32(TargetIndex);
		}
		if (TargetState != ECommonState.No)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteEnum((int)TargetState);
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
		if (TargetIndex != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(TargetIndex);
		}
		if (TargetState != ECommonState.No)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)TargetState);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(DailySignTargetAwardDetail other)
	{
		if (other != null)
		{
			if (other.TargetIndex != 0)
			{
				TargetIndex = other.TargetIndex;
			}
			if (other.TargetState != ECommonState.No)
			{
				TargetState = other.TargetState;
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
				TargetIndex = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 16u:
				TargetState = (ECommonState)((ParseContext)(ref input)).ReadEnum();
				break;
			}
		}
	}
}
