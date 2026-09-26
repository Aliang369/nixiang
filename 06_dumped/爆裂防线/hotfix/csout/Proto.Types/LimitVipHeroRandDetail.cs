using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class LimitVipHeroRandDetail : IMessage<LimitVipHeroRandDetail>, IMessage, IEquatable<LimitVipHeroRandDetail>, IDeepCloneable<LimitVipHeroRandDetail>, IBufferMessage
{
	private static readonly MessageParser<LimitVipHeroRandDetail> _parser = new MessageParser<LimitVipHeroRandDetail>((Func<LimitVipHeroRandDetail>)(() => new LimitVipHeroRandDetail()));

	private UnknownFieldSet _unknownFields;

	public const int IndexFieldNumber = 1;

	private int index_;

	public const int RareFieldNumber = 2;

	private ECommonState rare_;

	public const int AwardDetailFieldNumber = 3;

	private AwardDetail awardDetail_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<LimitVipHeroRandDetail> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeLimitVipHeroRandDetailReflection.Descriptor.MessageTypes[0];

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
	public ECommonState Rare
	{
		get
		{
			return rare_;
		}
		set
		{
			rare_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public AwardDetail AwardDetail
	{
		get
		{
			return awardDetail_;
		}
		set
		{
			awardDetail_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public LimitVipHeroRandDetail()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public LimitVipHeroRandDetail(LimitVipHeroRandDetail other)
		: this()
	{
		index_ = other.index_;
		rare_ = other.rare_;
		awardDetail_ = ((other.awardDetail_ != null) ? other.awardDetail_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public LimitVipHeroRandDetail Clone()
	{
		return new LimitVipHeroRandDetail(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as LimitVipHeroRandDetail);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(LimitVipHeroRandDetail other)
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
		if (Rare != other.Rare)
		{
			return false;
		}
		if (!object.Equals(AwardDetail, other.AwardDetail))
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
		if (Rare != ECommonState.No)
		{
			num ^= Rare.GetHashCode();
		}
		if (awardDetail_ != null)
		{
			num ^= AwardDetail.GetHashCode();
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
		if (Index != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteInt32(Index);
		}
		if (Rare != ECommonState.No)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteEnum((int)Rare);
		}
		if (awardDetail_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)26);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)AwardDetail);
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
		if (Index != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(Index);
		}
		if (Rare != ECommonState.No)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)Rare);
		}
		if (awardDetail_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)AwardDetail);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(LimitVipHeroRandDetail other)
	{
		if (other == null)
		{
			return;
		}
		if (other.Index != 0)
		{
			Index = other.Index;
		}
		if (other.Rare != ECommonState.No)
		{
			Rare = other.Rare;
		}
		if (other.awardDetail_ != null)
		{
			if (awardDetail_ == null)
			{
				AwardDetail = new AwardDetail();
			}
			AwardDetail.MergeFrom(other.AwardDetail);
		}
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
			case 8u:
				Index = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 16u:
				Rare = (ECommonState)((ParseContext)(ref input)).ReadEnum();
				break;
			case 26u:
				if (awardDetail_ == null)
				{
					AwardDetail = new AwardDetail();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)AwardDetail);
				break;
			}
		}
	}
}
