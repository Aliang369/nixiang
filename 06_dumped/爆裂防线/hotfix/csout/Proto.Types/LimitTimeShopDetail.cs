using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class LimitTimeShopDetail : IMessage<LimitTimeShopDetail>, IMessage, IEquatable<LimitTimeShopDetail>, IDeepCloneable<LimitTimeShopDetail>, IBufferMessage
{
	private static readonly MessageParser<LimitTimeShopDetail> _parser = new MessageParser<LimitTimeShopDetail>((Func<LimitTimeShopDetail>)(() => new LimitTimeShopDetail()));

	private UnknownFieldSet _unknownFields;

	public const int BaseInfoFieldNumber = 1;

	private OperateBaseInfo baseInfo_;

	public const int ItemListFieldNumber = 2;

	private static readonly FieldCodec<LimitTimeShopItemDetail> _repeated_itemList_codec = FieldCodec.ForMessage<LimitTimeShopItemDetail>(18u, LimitTimeShopItemDetail.Parser);

	private readonly RepeatedField<LimitTimeShopItemDetail> itemList_ = new RepeatedField<LimitTimeShopItemDetail>();

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<LimitTimeShopDetail> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeLimitTimeShopDetailReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public OperateBaseInfo BaseInfo
	{
		get
		{
			return baseInfo_;
		}
		set
		{
			baseInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<LimitTimeShopItemDetail> ItemList => itemList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public LimitTimeShopDetail()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public LimitTimeShopDetail(LimitTimeShopDetail other)
		: this()
	{
		baseInfo_ = ((other.baseInfo_ != null) ? other.baseInfo_.Clone() : null);
		itemList_ = other.itemList_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public LimitTimeShopDetail Clone()
	{
		return new LimitTimeShopDetail(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as LimitTimeShopDetail);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(LimitTimeShopDetail other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!object.Equals(BaseInfo, other.BaseInfo))
		{
			return false;
		}
		if (!itemList_.Equals(other.itemList_))
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
		if (baseInfo_ != null)
		{
			num ^= BaseInfo.GetHashCode();
		}
		num ^= ((object)itemList_).GetHashCode();
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
		if (baseInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)10);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)BaseInfo);
		}
		itemList_.WriteTo(ref output, _repeated_itemList_codec);
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
		if (baseInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)BaseInfo);
		}
		num += itemList_.CalculateSize(_repeated_itemList_codec);
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(LimitTimeShopDetail other)
	{
		if (other == null)
		{
			return;
		}
		if (other.baseInfo_ != null)
		{
			if (baseInfo_ == null)
			{
				BaseInfo = new OperateBaseInfo();
			}
			BaseInfo.MergeFrom(other.BaseInfo);
		}
		itemList_.Add((IEnumerable<LimitTimeShopItemDetail>)other.itemList_);
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
				if (baseInfo_ == null)
				{
					BaseInfo = new OperateBaseInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)BaseInfo);
				break;
			case 18u:
				itemList_.AddEntriesFrom(ref input, _repeated_itemList_codec);
				break;
			}
		}
	}
}
