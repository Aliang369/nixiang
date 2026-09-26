using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class PayCreateOrderReq : IMessage<PayCreateOrderReq>, IMessage, IEquatable<PayCreateOrderReq>, IDeepCloneable<PayCreateOrderReq>, IBufferMessage
{
	private static readonly MessageParser<PayCreateOrderReq> _parser = new MessageParser<PayCreateOrderReq>((Func<PayCreateOrderReq>)(() => new PayCreateOrderReq()));

	private UnknownFieldSet _unknownFields;

	public const int PlatformFieldNumber = 1;

	private EPlatformType platform_;

	public const int AppChannelFieldNumber = 2;

	private EChannelType appChannel_;

	public const int GoodsIdFieldNumber = 3;

	private int goodsId_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<PayCreateOrderReq> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypePayCreateOrderReqReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public EPlatformType Platform
	{
		get
		{
			return platform_;
		}
		set
		{
			platform_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public EChannelType AppChannel
	{
		get
		{
			return appChannel_;
		}
		set
		{
			appChannel_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int GoodsId
	{
		get
		{
			return goodsId_;
		}
		set
		{
			goodsId_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public PayCreateOrderReq()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public PayCreateOrderReq(PayCreateOrderReq other)
		: this()
	{
		platform_ = other.platform_;
		appChannel_ = other.appChannel_;
		goodsId_ = other.goodsId_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public PayCreateOrderReq Clone()
	{
		return new PayCreateOrderReq(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as PayCreateOrderReq);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(PayCreateOrderReq other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (Platform != other.Platform)
		{
			return false;
		}
		if (AppChannel != other.AppChannel)
		{
			return false;
		}
		if (GoodsId != other.GoodsId)
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
		if (Platform != EPlatformType.EmptyValue)
		{
			num ^= Platform.GetHashCode();
		}
		if (AppChannel != EChannelType.EmptyValue)
		{
			num ^= AppChannel.GetHashCode();
		}
		if (GoodsId != 0)
		{
			num ^= GoodsId.GetHashCode();
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
		if (Platform != EPlatformType.EmptyValue)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteEnum((int)Platform);
		}
		if (AppChannel != EChannelType.EmptyValue)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteEnum((int)AppChannel);
		}
		if (GoodsId != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)24);
			((WriteContext)(ref output)).WriteInt32(GoodsId);
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
		if (Platform != EPlatformType.EmptyValue)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)Platform);
		}
		if (AppChannel != EChannelType.EmptyValue)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)AppChannel);
		}
		if (GoodsId != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(GoodsId);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(PayCreateOrderReq other)
	{
		if (other != null)
		{
			if (other.Platform != EPlatformType.EmptyValue)
			{
				Platform = other.Platform;
			}
			if (other.AppChannel != EChannelType.EmptyValue)
			{
				AppChannel = other.AppChannel;
			}
			if (other.GoodsId != 0)
			{
				GoodsId = other.GoodsId;
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
				Platform = (EPlatformType)((ParseContext)(ref input)).ReadEnum();
				break;
			case 16u:
				AppChannel = (EChannelType)((ParseContext)(ref input)).ReadEnum();
				break;
			case 24u:
				GoodsId = ((ParseContext)(ref input)).ReadInt32();
				break;
			}
		}
	}
}
