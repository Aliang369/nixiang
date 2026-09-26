using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class OperateScratchCardDetail : IMessage<OperateScratchCardDetail>, IMessage, IEquatable<OperateScratchCardDetail>, IDeepCloneable<OperateScratchCardDetail>, IBufferMessage
{
	private static readonly MessageParser<OperateScratchCardDetail> _parser = new MessageParser<OperateScratchCardDetail>((Func<OperateScratchCardDetail>)(() => new OperateScratchCardDetail()));

	private UnknownFieldSet _unknownFields;

	public const int IndexFieldNumber = 1;

	private int index_;

	public const int PrizeFieldNumber = 2;

	private int prize_;

	public const int CardListFieldNumber = 3;

	private static readonly FieldCodec<int> _repeated_cardList_codec = FieldCodec.ForInt32(26u);

	private readonly RepeatedField<int> cardList_ = new RepeatedField<int>();

	public const int StateFieldNumber = 4;

	private ECommonState state_;

	public const int AwardListFieldNumber = 5;

	private static readonly FieldCodec<AwardDetail> _repeated_awardList_codec = FieldCodec.ForMessage<AwardDetail>(42u, AwardDetail.Parser);

	private readonly RepeatedField<AwardDetail> awardList_ = new RepeatedField<AwardDetail>();

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<OperateScratchCardDetail> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeOperateScratchCardDetailReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int Index
	{
		get
		{
			return index_;
		}
		set
		{
			index_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int Prize
	{
		get
		{
			return prize_;
		}
		set
		{
			prize_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<int> CardList => cardList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ECommonState State
	{
		get
		{
			return state_;
		}
		set
		{
			state_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<AwardDetail> AwardList => awardList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public OperateScratchCardDetail()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public OperateScratchCardDetail(OperateScratchCardDetail other)
		: this()
	{
		index_ = other.index_;
		prize_ = other.prize_;
		cardList_ = other.cardList_.Clone();
		state_ = other.state_;
		awardList_ = other.awardList_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public OperateScratchCardDetail Clone()
	{
		return new OperateScratchCardDetail(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as OperateScratchCardDetail);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(OperateScratchCardDetail other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (Index != other.Index)
		{
			return false;
		}
		if (Prize != other.Prize)
		{
			return false;
		}
		if (!cardList_.Equals(other.cardList_))
		{
			return false;
		}
		if (State != other.State)
		{
			return false;
		}
		if (!awardList_.Equals(other.awardList_))
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
		if (Index != 0)
		{
			num ^= Index.GetHashCode();
		}
		if (Prize != 0)
		{
			num ^= Prize.GetHashCode();
		}
		num ^= ((object)cardList_).GetHashCode();
		if (State != ECommonState.No)
		{
			num ^= State.GetHashCode();
		}
		num ^= ((object)awardList_).GetHashCode();
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
		if (Index != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteInt32(Index);
		}
		if (Prize != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteInt32(Prize);
		}
		cardList_.WriteTo(ref output, _repeated_cardList_codec);
		if (State != ECommonState.No)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)32);
			((WriteContext)(ref output)).WriteEnum((int)State);
		}
		awardList_.WriteTo(ref output, _repeated_awardList_codec);
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
		if (Index != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(Index);
		}
		if (Prize != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(Prize);
		}
		num += cardList_.CalculateSize(_repeated_cardList_codec);
		if (State != ECommonState.No)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)State);
		}
		num += awardList_.CalculateSize(_repeated_awardList_codec);
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(OperateScratchCardDetail other)
	{
		if (other != null)
		{
			if (other.Index != 0)
			{
				Index = other.Index;
			}
			if (other.Prize != 0)
			{
				Prize = other.Prize;
			}
			cardList_.Add((IEnumerable<int>)other.cardList_);
			if (other.State != ECommonState.No)
			{
				State = other.State;
			}
			awardList_.Add((IEnumerable<AwardDetail>)other.awardList_);
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
				Index = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 16u:
				Prize = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 24u:
			case 26u:
				cardList_.AddEntriesFrom(ref input, _repeated_cardList_codec);
				break;
			case 32u:
				State = (ECommonState)((ParseContext)(ref input)).ReadEnum();
				break;
			case 42u:
				awardList_.AddEntriesFrom(ref input, _repeated_awardList_codec);
				break;
			}
		}
	}
}
