using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class OperateMechaSubjectBarDrawAck : IMessage<OperateMechaSubjectBarDrawAck>, IMessage, IEquatable<OperateMechaSubjectBarDrawAck>, IDeepCloneable<OperateMechaSubjectBarDrawAck>, IBufferMessage
{
	private static readonly MessageParser<OperateMechaSubjectBarDrawAck> _parser = new MessageParser<OperateMechaSubjectBarDrawAck>((Func<OperateMechaSubjectBarDrawAck>)(() => new OperateMechaSubjectBarDrawAck()));

	private UnknownFieldSet _unknownFields;

	public const int MechaSubjectListFieldNumber = 1;

	private static readonly FieldCodec<MechaSubjectDetail> _repeated_mechaSubjectList_codec = FieldCodec.ForMessage<MechaSubjectDetail>(10u, MechaSubjectDetail.Parser);

	private readonly RepeatedField<MechaSubjectDetail> mechaSubjectList_ = new RepeatedField<MechaSubjectDetail>();

	public const int RelicListFieldNumber = 2;

	private static readonly FieldCodec<int> _repeated_relicList_codec = FieldCodec.ForInt32(18u);

	private readonly RepeatedField<int> relicList_ = new RepeatedField<int>();

	public const int AwardListFieldNumber = 3;

	private static readonly FieldCodec<AwardDetail> _repeated_awardList_codec = FieldCodec.ForMessage<AwardDetail>(26u, AwardDetail.Parser);

	private readonly RepeatedField<AwardDetail> awardList_ = new RepeatedField<AwardDetail>();

	public const int DataChangeFieldNumber = 4;

	private DataChange dataChange_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<OperateMechaSubjectBarDrawAck> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeOperateMechaSubjectBarDrawAckReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<MechaSubjectDetail> MechaSubjectList => mechaSubjectList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<int> RelicList => relicList_;

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
	public OperateMechaSubjectBarDrawAck()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public OperateMechaSubjectBarDrawAck(OperateMechaSubjectBarDrawAck other)
		: this()
	{
		mechaSubjectList_ = other.mechaSubjectList_.Clone();
		relicList_ = other.relicList_.Clone();
		awardList_ = other.awardList_.Clone();
		dataChange_ = ((other.dataChange_ != null) ? other.dataChange_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public OperateMechaSubjectBarDrawAck Clone()
	{
		return new OperateMechaSubjectBarDrawAck(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as OperateMechaSubjectBarDrawAck);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(OperateMechaSubjectBarDrawAck other)
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
		if (!relicList_.Equals(other.relicList_))
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
		num ^= ((object)relicList_).GetHashCode();
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
		relicList_.WriteTo(ref output, _repeated_relicList_codec);
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
		num += relicList_.CalculateSize(_repeated_relicList_codec);
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
	public void MergeFrom(OperateMechaSubjectBarDrawAck other)
	{
		if (other == null)
		{
			return;
		}
		mechaSubjectList_.Add((IEnumerable<MechaSubjectDetail>)other.mechaSubjectList_);
		relicList_.Add((IEnumerable<int>)other.relicList_);
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
				relicList_.AddEntriesFrom(ref input, _repeated_relicList_codec);
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
