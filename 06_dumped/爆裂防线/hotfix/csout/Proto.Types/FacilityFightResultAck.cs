using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class FacilityFightResultAck : IMessage<FacilityFightResultAck>, IMessage, IEquatable<FacilityFightResultAck>, IDeepCloneable<FacilityFightResultAck>, IBufferMessage
{
	private static readonly MessageParser<FacilityFightResultAck> _parser = new MessageParser<FacilityFightResultAck>((Func<FacilityFightResultAck>)(() => new FacilityFightResultAck()));

	private UnknownFieldSet _unknownFields;

	public const int BattleSettleFieldNumber = 1;

	private BattleResultInfo battleSettle_;

	public const int FacilityMapInfoFieldNumber = 2;

	private FacilityMapInfo facilityMapInfo_;

	public const int AwardListFieldNumber = 3;

	private static readonly FieldCodec<AwardDetail> _repeated_awardList_codec = FieldCodec.ForMessage<AwardDetail>(26u, AwardDetail.Parser);

	private readonly RepeatedField<AwardDetail> awardList_ = new RepeatedField<AwardDetail>();

	public const int DataChangeFieldNumber = 4;

	private DataChange dataChange_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<FacilityFightResultAck> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeFacilityFightResultAckReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public BattleResultInfo BattleSettle
	{
		get
		{
			return battleSettle_;
		}
		set
		{
			battleSettle_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public FacilityMapInfo FacilityMapInfo
	{
		get
		{
			return facilityMapInfo_;
		}
		set
		{
			facilityMapInfo_ = value;
		}
	}

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
	public FacilityFightResultAck()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public FacilityFightResultAck(FacilityFightResultAck other)
		: this()
	{
		battleSettle_ = ((other.battleSettle_ != null) ? other.battleSettle_.Clone() : null);
		facilityMapInfo_ = ((other.facilityMapInfo_ != null) ? other.facilityMapInfo_.Clone() : null);
		awardList_ = other.awardList_.Clone();
		dataChange_ = ((other.dataChange_ != null) ? other.dataChange_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public FacilityFightResultAck Clone()
	{
		return new FacilityFightResultAck(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as FacilityFightResultAck);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(FacilityFightResultAck other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!object.Equals(BattleSettle, other.BattleSettle))
		{
			return false;
		}
		if (!object.Equals(FacilityMapInfo, other.FacilityMapInfo))
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
		if (battleSettle_ != null)
		{
			num ^= BattleSettle.GetHashCode();
		}
		if (facilityMapInfo_ != null)
		{
			num ^= FacilityMapInfo.GetHashCode();
		}
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
		if (battleSettle_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)10);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)BattleSettle);
		}
		if (facilityMapInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)18);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)FacilityMapInfo);
		}
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
		if (battleSettle_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)BattleSettle);
		}
		if (facilityMapInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)FacilityMapInfo);
		}
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
	public void MergeFrom(FacilityFightResultAck other)
	{
		if (other == null)
		{
			return;
		}
		if (other.battleSettle_ != null)
		{
			if (battleSettle_ == null)
			{
				BattleSettle = new BattleResultInfo();
			}
			BattleSettle.MergeFrom(other.BattleSettle);
		}
		if (other.facilityMapInfo_ != null)
		{
			if (facilityMapInfo_ == null)
			{
				FacilityMapInfo = new FacilityMapInfo();
			}
			FacilityMapInfo.MergeFrom(other.FacilityMapInfo);
		}
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
				if (battleSettle_ == null)
				{
					BattleSettle = new BattleResultInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)BattleSettle);
				break;
			case 18u:
				if (facilityMapInfo_ == null)
				{
					FacilityMapInfo = new FacilityMapInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)FacilityMapInfo);
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
