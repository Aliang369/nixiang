using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class HelpInfo : IMessage<HelpInfo>, IMessage, IEquatable<HelpInfo>, IDeepCloneable<HelpInfo>, IBufferMessage
{
	private static readonly MessageParser<HelpInfo> _parser = new MessageParser<HelpInfo>((Func<HelpInfo>)(() => new HelpInfo()));

	private UnknownFieldSet _unknownFields;

	public const int HelperListFieldNumber = 1;

	private static readonly FieldCodec<long> _repeated_helperList_codec = FieldCodec.ForInt64(10u);

	private readonly RepeatedField<long> helperList_ = new RepeatedField<long>();

	public const int HelpAwardCountFieldNumber = 2;

	private int helpAwardCount_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<HelpInfo> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeHelpInfoReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<long> HelperList => helperList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int HelpAwardCount
	{
		get
		{
			return helpAwardCount_;
		}
		set
		{
			helpAwardCount_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public HelpInfo()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public HelpInfo(HelpInfo other)
		: this()
	{
		helperList_ = other.helperList_.Clone();
		helpAwardCount_ = other.helpAwardCount_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public HelpInfo Clone()
	{
		return new HelpInfo(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as HelpInfo);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(HelpInfo other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!helperList_.Equals(other.helperList_))
		{
			return false;
		}
		if (HelpAwardCount != other.HelpAwardCount)
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
		num ^= ((object)helperList_).GetHashCode();
		if (HelpAwardCount != 0)
		{
			num ^= HelpAwardCount.GetHashCode();
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
		helperList_.WriteTo(ref output, _repeated_helperList_codec);
		if (HelpAwardCount != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteInt32(HelpAwardCount);
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
		num += helperList_.CalculateSize(_repeated_helperList_codec);
		if (HelpAwardCount != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(HelpAwardCount);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(HelpInfo other)
	{
		if (other != null)
		{
			helperList_.Add((IEnumerable<long>)other.helperList_);
			if (other.HelpAwardCount != 0)
			{
				HelpAwardCount = other.HelpAwardCount;
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
			case 10u:
				helperList_.AddEntriesFrom(ref input, _repeated_helperList_codec);
				break;
			case 16u:
				HelpAwardCount = ((ParseContext)(ref input)).ReadInt32();
				break;
			}
		}
	}
}
