using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class OperateMechaSubjectRouletteAck : IMessage<OperateMechaSubjectRouletteAck>, IMessage, IEquatable<OperateMechaSubjectRouletteAck>, IDeepCloneable<OperateMechaSubjectRouletteAck>, IBufferMessage
{
	private static readonly MessageParser<OperateMechaSubjectRouletteAck> _parser = new MessageParser<OperateMechaSubjectRouletteAck>((Func<OperateMechaSubjectRouletteAck>)(() => new OperateMechaSubjectRouletteAck()));

	private UnknownFieldSet _unknownFields;

	public const int MechaSubjectListFieldNumber = 1;

	private static readonly FieldCodec<MechaSubjectDetail> _repeated_mechaSubjectList_codec = FieldCodec.ForMessage<MechaSubjectDetail>(10u, MechaSubjectDetail.Parser);

	private readonly RepeatedField<MechaSubjectDetail> mechaSubjectList_ = new RepeatedField<MechaSubjectDetail>();

	public const int HitListFieldNumber = 2;

	private static readonly FieldCodec<int> _repeated_hitList_codec = FieldCodec.ForInt32(18u);

	private readonly RepeatedField<int> hitList_ = new RepeatedField<int>();

	public const int AwardListFieldNumber = 3;

	private static readonly FieldCodec<AwardDetail> _repeated_awardList_codec = FieldCodec.ForMessage<AwardDetail>(26u, AwardDetail.Parser);

	private readonly RepeatedField<AwardDetail> awardList_ = new RepeatedField<AwardDetail>();

	public const int DataChangeFieldNumber = 4;

	private DataChange dataChange_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<OperateMechaSubjectRouletteAck> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeOperateMechaSubjectRouletteAckReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<MechaSubjectDetail> MechaSubjectList => mechaSubjectList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<int> HitList => hitList_;

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
	public OperateMechaSubjectRouletteAck()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public OperateMechaSubjectRouletteAck(OperateMechaSubjectRouletteAck other)
		: this()
	{
		mechaSubjectList_ = other.mechaSubjectList_.Clone();
		hitList_ = other.hitList_.Clone();
		awardList_ = other.awardList_.Clone();
		dataChange_ = ((other.dataChange_ != null) ? other.dataChange_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public OperateMechaSubjectRouletteAck Clone()
	{
		return new OperateMechaSubjectRouletteAck(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as OperateMechaSubjectRouletteAck);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(OperateMechaSubjectRouletteAck other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!mechaSubjectList_.Equals(other.mechaSubjectList_))
		{
			return false;
		}
		if (!hitList_.Equals(other.hitList_))
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
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override int GetHashCode()
	{
		int num = 1;
		num ^= ((object)mechaSubjectList_).GetHashCode();
		num ^= ((object)hitList_).GetHashCode();
		num ^= ((object)awardList_).GetHashCode();
		if (dataChange_ != null)
		{
			num ^= DataChange.GetHashCode();
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
		mechaSubjectList_.WriteTo(ref output, _repeated_mechaSubjectList_codec);
		hitList_.WriteTo(ref output, _repeated_hitList_codec);
		awardList_.WriteTo(ref output, _repeated_awardList_codec);
		if (dataChange_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)34);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)DataChange);
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
		num += mechaSubjectList_.CalculateSize(_repeated_mechaSubjectList_codec);
		num += hitList_.CalculateSize(_repeated_hitList_codec);
		num += awardList_.CalculateSize(_repeated_awardList_codec);
		if (dataChange_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)DataChange);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(OperateMechaSubjectRouletteAck other)
	{
		if (other == null)
		{
			return;
		}
		mechaSubjectList_.Add((IEnumerable<MechaSubjectDetail>)other.mechaSubjectList_);
		hitList_.Add((IEnumerable<int>)other.hitList_);
		awardList_.Add((IEnumerable<AwardDetail>)other.awardList_);
		if (other.dataChange_ != null)
		{
			if (dataChange_ == null)
			{
				DataChange = new DataChange();
			}
			DataChange.MergeFrom(other.DataChange);
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
			case 10u:
				mechaSubjectList_.AddEntriesFrom(ref input, _repeated_mechaSubjectList_codec);
				break;
			case 16u:
			case 18u:
				hitList_.AddEntriesFrom(ref input, _repeated_hitList_codec);
				break;
			case 26u:
				awardList_.AddEntriesFrom(ref input, _repeated_awardList_codec);
				break;
			case 34u:
				if (dataChange_ == null)
				{
					DataChange = new DataChange();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)DataChange);
				break;
			}
		}
	}
}
