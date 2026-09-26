using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class OperateBossSubRouletteAck : IMessage<OperateBossSubRouletteAck>, IMessage, IEquatable<OperateBossSubRouletteAck>, IDeepCloneable<OperateBossSubRouletteAck>, IBufferMessage
{
	private static readonly MessageParser<OperateBossSubRouletteAck> _parser = new MessageParser<OperateBossSubRouletteAck>((Func<OperateBossSubRouletteAck>)(() => new OperateBossSubRouletteAck()));

	private UnknownFieldSet _unknownFields;

	public const int BossSubjectListFieldNumber = 1;

	private static readonly FieldCodec<BossSubjectDetail> _repeated_bossSubjectList_codec = FieldCodec.ForMessage<BossSubjectDetail>(10u, BossSubjectDetail.Parser);

	private readonly RepeatedField<BossSubjectDetail> bossSubjectList_ = new RepeatedField<BossSubjectDetail>();

	public const int AwardListFieldNumber = 2;

	private static readonly FieldCodec<AwardDetail> _repeated_awardList_codec = FieldCodec.ForMessage<AwardDetail>(18u, AwardDetail.Parser);

	private readonly RepeatedField<AwardDetail> awardList_ = new RepeatedField<AwardDetail>();

	public const int DataChangeFieldNumber = 3;

	private DataChange dataChange_;

	public const int HitListFieldNumber = 4;

	private static readonly FieldCodec<int> _repeated_hitList_codec = FieldCodec.ForInt32(34u);

	private readonly RepeatedField<int> hitList_ = new RepeatedField<int>();

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<OperateBossSubRouletteAck> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeOperateBossSubRouletteAckReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<BossSubjectDetail> BossSubjectList => bossSubjectList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<AwardDetail> AwardList => awardList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public DataChange DataChange
	{
		get
		{
			return dataChange_;
		}
		set
		{
			dataChange_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<int> HitList => hitList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public OperateBossSubRouletteAck()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public OperateBossSubRouletteAck(OperateBossSubRouletteAck other)
		: this()
	{
		bossSubjectList_ = other.bossSubjectList_.Clone();
		awardList_ = other.awardList_.Clone();
		dataChange_ = ((other.dataChange_ != null) ? other.dataChange_.Clone() : null);
		hitList_ = other.hitList_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public OperateBossSubRouletteAck Clone()
	{
		return new OperateBossSubRouletteAck(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as OperateBossSubRouletteAck);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(OperateBossSubRouletteAck other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!bossSubjectList_.Equals(other.bossSubjectList_))
		{
			return false;
		}
		if (!awardList_.Equals(other.awardList_))
		{
			return false;
		}
		if (!object.Equals(DataChange, other.DataChange))
		{
			return false;
		}
		if (!hitList_.Equals(other.hitList_))
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
		num ^= ((object)bossSubjectList_).GetHashCode();
		num ^= ((object)awardList_).GetHashCode();
		if (dataChange_ != null)
		{
			num ^= DataChange.GetHashCode();
		}
		num ^= ((object)hitList_).GetHashCode();
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
		bossSubjectList_.WriteTo(ref output, _repeated_bossSubjectList_codec);
		awardList_.WriteTo(ref output, _repeated_awardList_codec);
		if (dataChange_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)26);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)DataChange);
		}
		hitList_.WriteTo(ref output, _repeated_hitList_codec);
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
		num += bossSubjectList_.CalculateSize(_repeated_bossSubjectList_codec);
		num += awardList_.CalculateSize(_repeated_awardList_codec);
		if (dataChange_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)DataChange);
		}
		num += hitList_.CalculateSize(_repeated_hitList_codec);
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(OperateBossSubRouletteAck other)
	{
		if (other == null)
		{
			return;
		}
		bossSubjectList_.Add((IEnumerable<BossSubjectDetail>)other.bossSubjectList_);
		awardList_.Add((IEnumerable<AwardDetail>)other.awardList_);
		if (other.dataChange_ != null)
		{
			if (dataChange_ == null)
			{
				DataChange = new DataChange();
			}
			DataChange.MergeFrom(other.DataChange);
		}
		hitList_.Add((IEnumerable<int>)other.hitList_);
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
				bossSubjectList_.AddEntriesFrom(ref input, _repeated_bossSubjectList_codec);
				break;
			case 18u:
				awardList_.AddEntriesFrom(ref input, _repeated_awardList_codec);
				break;
			case 26u:
				if (dataChange_ == null)
				{
					DataChange = new DataChange();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)DataChange);
				break;
			case 32u:
			case 34u:
				hitList_.AddEntriesFrom(ref input, _repeated_hitList_codec);
				break;
			}
		}
	}
}
