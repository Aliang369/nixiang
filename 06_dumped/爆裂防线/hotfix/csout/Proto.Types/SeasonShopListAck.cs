using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class SeasonShopListAck : IMessage<SeasonShopListAck>, IMessage, IEquatable<SeasonShopListAck>, IDeepCloneable<SeasonShopListAck>, IBufferMessage
{
	private static readonly MessageParser<SeasonShopListAck> _parser = new MessageParser<SeasonShopListAck>((Func<SeasonShopListAck>)(() => new SeasonShopListAck()));

	private UnknownFieldSet _unknownFields;

	public const int MeritFieldNumber = 1;

	private int merit_;

	public const int SeasonCoinFieldNumber = 2;

	private int seasonCoin_;

	public const int GoodsListFieldNumber = 3;

	private static readonly FieldCodec<SeasonShopGoods> _repeated_goodsList_codec = FieldCodec.ForMessage<SeasonShopGoods>(26u, SeasonShopGoods.Parser);

	private readonly RepeatedField<SeasonShopGoods> goodsList_ = new RepeatedField<SeasonShopGoods>();

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<SeasonShopListAck> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeSeasonShopListAckReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int Merit
	{
		get
		{
			return merit_;
		}
		set
		{
			merit_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int SeasonCoin
	{
		get
		{
			return seasonCoin_;
		}
		set
		{
			seasonCoin_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<SeasonShopGoods> GoodsList => goodsList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SeasonShopListAck()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SeasonShopListAck(SeasonShopListAck other)
		: this()
	{
		merit_ = other.merit_;
		seasonCoin_ = other.seasonCoin_;
		goodsList_ = other.goodsList_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SeasonShopListAck Clone()
	{
		return new SeasonShopListAck(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as SeasonShopListAck);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(SeasonShopListAck other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (Merit != other.Merit)
		{
			return false;
		}
		if (SeasonCoin != other.SeasonCoin)
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
		if (Merit != 0)
		{
			num ^= Merit.GetHashCode();
		}
		if (SeasonCoin != 0)
		{
			num ^= SeasonCoin.GetHashCode();
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
		if (Merit != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteInt32(Merit);
		}
		if (SeasonCoin != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteInt32(SeasonCoin);
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
		if (Merit != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(Merit);
		}
		if (SeasonCoin != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(SeasonCoin);
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
	public void MergeFrom(SeasonShopListAck other)
	{
		if (other != null)
		{
			if (other.Merit != 0)
			{
				Merit = other.Merit;
			}
			if (other.SeasonCoin != 0)
			{
				SeasonCoin = other.SeasonCoin;
			}
			goodsList_.Add((IEnumerable<SeasonShopGoods>)other.goodsList_);
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
				Merit = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 16u:
				SeasonCoin = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 26u:
				goodsList_.AddEntriesFrom(ref input, _repeated_goodsList_codec);
				break;
			}
		}
	}
}
