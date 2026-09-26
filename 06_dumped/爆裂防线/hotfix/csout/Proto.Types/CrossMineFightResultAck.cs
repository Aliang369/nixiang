using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class CrossMineFightResultAck : IMessage<CrossMineFightResultAck>, IMessage, IEquatable<CrossMineFightResultAck>, IDeepCloneable<CrossMineFightResultAck>, IBufferMessage
{
	private static readonly MessageParser<CrossMineFightResultAck> _parser = new MessageParser<CrossMineFightResultAck>((Func<CrossMineFightResultAck>)(() => new CrossMineFightResultAck()));

	private UnknownFieldSet _unknownFields;

	public const int ServerIdFieldNumber = 1;

	private int serverId_;

	public const int MineWarBaseInfoFieldNumber = 2;

	private MineWarBaseInfo mineWarBaseInfo_;

	public const int CrossMineWarMapInfoFieldNumber = 3;

	private MineWarMapInfo crossMineWarMapInfo_;

	public const int BattleSettleFieldNumber = 4;

	private BattleResultInfo battleSettle_;

	public const int AwardListFieldNumber = 5;

	private static readonly FieldCodec<AwardDetail> _repeated_awardList_codec = FieldCodec.ForMessage<AwardDetail>(42u, AwardDetail.Parser);

	private readonly RepeatedField<AwardDetail> awardList_ = new RepeatedField<AwardDetail>();

	public const int DataChangeFieldNumber = 6;

	private DataChange dataChange_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<CrossMineFightResultAck> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeCrossMineFightResultAckReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int ServerId
	{
		get
		{
			return serverId_;
		}
		set
		{
			serverId_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public MineWarBaseInfo MineWarBaseInfo
	{
		get
		{
			return mineWarBaseInfo_;
		}
		set
		{
			mineWarBaseInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public MineWarMapInfo CrossMineWarMapInfo
	{
		get
		{
			return crossMineWarMapInfo_;
		}
		set
		{
			crossMineWarMapInfo_ = value;
		}
	}

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
	public CrossMineFightResultAck()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public CrossMineFightResultAck(CrossMineFightResultAck other)
		: this()
	{
		serverId_ = other.serverId_;
		mineWarBaseInfo_ = ((other.mineWarBaseInfo_ != null) ? other.mineWarBaseInfo_.Clone() : null);
		crossMineWarMapInfo_ = ((other.crossMineWarMapInfo_ != null) ? other.crossMineWarMapInfo_.Clone() : null);
		battleSettle_ = ((other.battleSettle_ != null) ? other.battleSettle_.Clone() : null);
		awardList_ = other.awardList_.Clone();
		dataChange_ = ((other.dataChange_ != null) ? other.dataChange_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public CrossMineFightResultAck Clone()
	{
		return new CrossMineFightResultAck(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as CrossMineFightResultAck);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(CrossMineFightResultAck other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (ServerId != other.ServerId)
		{
			return false;
		}
		if (!object.Equals(MineWarBaseInfo, other.MineWarBaseInfo))
		{
			return false;
		}
		if (!object.Equals(CrossMineWarMapInfo, other.CrossMineWarMapInfo))
		{
			return false;
		}
		if (!object.Equals(BattleSettle, other.BattleSettle))
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
		if (ServerId != 0)
		{
			num ^= ServerId.GetHashCode();
		}
		if (mineWarBaseInfo_ != null)
		{
			num ^= MineWarBaseInfo.GetHashCode();
		}
		if (crossMineWarMapInfo_ != null)
		{
			num ^= CrossMineWarMapInfo.GetHashCode();
		}
		if (battleSettle_ != null)
		{
			num ^= BattleSettle.GetHashCode();
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
		if (ServerId != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteInt32(ServerId);
		}
		if (mineWarBaseInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)18);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)MineWarBaseInfo);
		}
		if (crossMineWarMapInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)26);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)CrossMineWarMapInfo);
		}
		if (battleSettle_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)34);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)BattleSettle);
		}
		awardList_.WriteTo(ref output, _repeated_awardList_codec);
		if (dataChange_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)50);
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
		if (ServerId != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(ServerId);
		}
		if (mineWarBaseInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)MineWarBaseInfo);
		}
		if (crossMineWarMapInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)CrossMineWarMapInfo);
		}
		if (battleSettle_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)BattleSettle);
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
	public void MergeFrom(CrossMineFightResultAck other)
	{
		if (other == null)
		{
			return;
		}
		if (other.ServerId != 0)
		{
			ServerId = other.ServerId;
		}
		if (other.mineWarBaseInfo_ != null)
		{
			if (mineWarBaseInfo_ == null)
			{
				MineWarBaseInfo = new MineWarBaseInfo();
			}
			MineWarBaseInfo.MergeFrom(other.MineWarBaseInfo);
		}
		if (other.crossMineWarMapInfo_ != null)
		{
			if (crossMineWarMapInfo_ == null)
			{
				CrossMineWarMapInfo = new MineWarMapInfo();
			}
			CrossMineWarMapInfo.MergeFrom(other.CrossMineWarMapInfo);
		}
		if (other.battleSettle_ != null)
		{
			if (battleSettle_ == null)
			{
				BattleSettle = new BattleResultInfo();
			}
			BattleSettle.MergeFrom(other.BattleSettle);
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
			case 8u:
				ServerId = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 18u:
				if (mineWarBaseInfo_ == null)
				{
					MineWarBaseInfo = new MineWarBaseInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)MineWarBaseInfo);
				break;
			case 26u:
				if (crossMineWarMapInfo_ == null)
				{
					CrossMineWarMapInfo = new MineWarMapInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)CrossMineWarMapInfo);
				break;
			case 34u:
				if (battleSettle_ == null)
				{
					BattleSettle = new BattleResultInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)BattleSettle);
				break;
			case 42u:
				awardList_.AddEntriesFrom(ref input, _repeated_awardList_codec);
				break;
			case 50u:
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
