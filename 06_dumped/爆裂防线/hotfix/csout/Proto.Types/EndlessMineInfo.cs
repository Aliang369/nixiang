using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class EndlessMineInfo : IMessage<EndlessMineInfo>, IMessage, IEquatable<EndlessMineInfo>, IDeepCloneable<EndlessMineInfo>, IBufferMessage
{
	private static readonly MessageParser<EndlessMineInfo> _parser = new MessageParser<EndlessMineInfo>((Func<EndlessMineInfo>)(() => new EndlessMineInfo()));

	private UnknownFieldSet _unknownFields;

	public const int EndlessMineBaseInfoFieldNumber = 1;

	private EndlessMineBaseInfo endlessMineBaseInfo_;

	public const int EndlessMineMapFieldNumber = 2;

	private static readonly FieldCodec<EndlessMineMapRowDetail> _repeated_endlessMineMap_codec = FieldCodec.ForMessage<EndlessMineMapRowDetail>(18u, EndlessMineMapRowDetail.Parser);

	private readonly RepeatedField<EndlessMineMapRowDetail> endlessMineMap_ = new RepeatedField<EndlessMineMapRowDetail>();

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<EndlessMineInfo> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeEndlessMineInfoReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public EndlessMineBaseInfo EndlessMineBaseInfo
	{
		get
		{
			return endlessMineBaseInfo_;
		}
		set
		{
			endlessMineBaseInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<EndlessMineMapRowDetail> EndlessMineMap => endlessMineMap_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public EndlessMineInfo()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public EndlessMineInfo(EndlessMineInfo other)
		: this()
	{
		endlessMineBaseInfo_ = ((other.endlessMineBaseInfo_ != null) ? other.endlessMineBaseInfo_.Clone() : null);
		endlessMineMap_ = other.endlessMineMap_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public EndlessMineInfo Clone()
	{
		return new EndlessMineInfo(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as EndlessMineInfo);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(EndlessMineInfo other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!object.Equals(EndlessMineBaseInfo, other.EndlessMineBaseInfo))
		{
			return false;
		}
		if (!endlessMineMap_.Equals(other.endlessMineMap_))
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
		if (endlessMineBaseInfo_ != null)
		{
			num ^= EndlessMineBaseInfo.GetHashCode();
		}
		num ^= ((object)endlessMineMap_).GetHashCode();
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
		if (endlessMineBaseInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)10);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)EndlessMineBaseInfo);
		}
		endlessMineMap_.WriteTo(ref output, _repeated_endlessMineMap_codec);
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
		if (endlessMineBaseInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)EndlessMineBaseInfo);
		}
		num += endlessMineMap_.CalculateSize(_repeated_endlessMineMap_codec);
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(EndlessMineInfo other)
	{
		if (other == null)
		{
			return;
		}
		if (other.endlessMineBaseInfo_ != null)
		{
			if (endlessMineBaseInfo_ == null)
			{
				EndlessMineBaseInfo = new EndlessMineBaseInfo();
			}
			EndlessMineBaseInfo.MergeFrom(other.EndlessMineBaseInfo);
		}
		endlessMineMap_.Add((IEnumerable<EndlessMineMapRowDetail>)other.endlessMineMap_);
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
				if (endlessMineBaseInfo_ == null)
				{
					EndlessMineBaseInfo = new EndlessMineBaseInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)EndlessMineBaseInfo);
				break;
			case 18u:
				endlessMineMap_.AddEntriesFrom(ref input, _repeated_endlessMineMap_codec);
				break;
			}
		}
	}
}
