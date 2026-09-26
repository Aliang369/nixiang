using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class SubscribeInfo : IMessage<SubscribeInfo>, IMessage, IEquatable<SubscribeInfo>, IDeepCloneable<SubscribeInfo>, IBufferMessage
{
	private static readonly MessageParser<SubscribeInfo> _parser = new MessageParser<SubscribeInfo>((Func<SubscribeInfo>)(() => new SubscribeInfo()));

	private UnknownFieldSet _unknownFields;

	public const int ActiveInfoFieldNumber = 1;

	private SubscribeActiveInfo activeInfo_;

	public const int GoodsListFieldNumber = 2;

	private static readonly FieldCodec<SubscribeGoodsDetail> _repeated_goodsList_codec = FieldCodec.ForMessage<SubscribeGoodsDetail>(18u, SubscribeGoodsDetail.Parser);

	private readonly RepeatedField<SubscribeGoodsDetail> goodsList_ = new RepeatedField<SubscribeGoodsDetail>();

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<SubscribeInfo> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeSubscribeInfoReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SubscribeActiveInfo ActiveInfo
	{
		get
		{
			return activeInfo_;
		}
		set
		{
			activeInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<SubscribeGoodsDetail> GoodsList => goodsList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SubscribeInfo()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SubscribeInfo(SubscribeInfo other)
		: this()
	{
		activeInfo_ = ((other.activeInfo_ != null) ? other.activeInfo_.Clone() : null);
		goodsList_ = other.goodsList_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SubscribeInfo Clone()
	{
		return new SubscribeInfo(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as SubscribeInfo);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(SubscribeInfo other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!object.Equals(ActiveInfo, other.ActiveInfo))
		{
			return false;
		}
		if (!goodsList_.Equals(other.goodsList_))
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
		if (activeInfo_ != null)
		{
			num ^= ActiveInfo.GetHashCode();
		}
		num ^= ((object)goodsList_).GetHashCode();
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
		if (activeInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)10);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)ActiveInfo);
		}
		goodsList_.WriteTo(ref output, _repeated_goodsList_codec);
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
		if (activeInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)ActiveInfo);
		}
		num += goodsList_.CalculateSize(_repeated_goodsList_codec);
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(SubscribeInfo other)
	{
		if (other == null)
		{
			return;
		}
		if (other.activeInfo_ != null)
		{
			if (activeInfo_ == null)
			{
				ActiveInfo = new SubscribeActiveInfo();
			}
			ActiveInfo.MergeFrom(other.ActiveInfo);
		}
		goodsList_.Add((IEnumerable<SubscribeGoodsDetail>)other.goodsList_);
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
				if (activeInfo_ == null)
				{
					ActiveInfo = new SubscribeActiveInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)ActiveInfo);
				break;
			case 18u:
				goodsList_.AddEntriesFrom(ref input, _repeated_goodsList_codec);
				break;
			}
		}
	}
}
