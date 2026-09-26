using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class BattleMonsterInfo : IMessage<BattleMonsterInfo>, IMessage, IEquatable<BattleMonsterInfo>, IDeepCloneable<BattleMonsterInfo>, IBufferMessage
{
	private static readonly MessageParser<BattleMonsterInfo> _parser = new MessageParser<BattleMonsterInfo>((Func<BattleMonsterInfo>)(() => new BattleMonsterInfo()));

	private UnknownFieldSet _unknownFields;

	public const int RoundListFieldNumber = 1;

	private static readonly FieldCodec<BattleMonsterRoundDetail> _repeated_roundList_codec = FieldCodec.ForMessage<BattleMonsterRoundDetail>(10u, BattleMonsterRoundDetail.Parser);

	private readonly RepeatedField<BattleMonsterRoundDetail> roundList_ = new RepeatedField<BattleMonsterRoundDetail>();

	public const int MonsterListFieldNumber = 2;

	private static readonly FieldCodec<BattleMonsterDetail> _repeated_monsterList_codec = FieldCodec.ForMessage<BattleMonsterDetail>(18u, BattleMonsterDetail.Parser);

	private readonly RepeatedField<BattleMonsterDetail> monsterList_ = new RepeatedField<BattleMonsterDetail>();

	public const int MonsterFactorFieldNumber = 3;

	private BattleMonsterFactor monsterFactor_;

	public const int ExtraInfoFieldNumber = 4;

	private BattleMonsterExtraInfo extraInfo_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<BattleMonsterInfo> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeBattleMonsterInfoReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<BattleMonsterRoundDetail> RoundList => roundList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<BattleMonsterDetail> MonsterList => monsterList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public BattleMonsterFactor MonsterFactor
	{
		get
		{
			return monsterFactor_;
		}
		set
		{
			monsterFactor_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public BattleMonsterExtraInfo ExtraInfo
	{
		get
		{
			return extraInfo_;
		}
		set
		{
			extraInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public BattleMonsterInfo()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public BattleMonsterInfo(BattleMonsterInfo other)
		: this()
	{
		roundList_ = other.roundList_.Clone();
		monsterList_ = other.monsterList_.Clone();
		monsterFactor_ = ((other.monsterFactor_ != null) ? other.monsterFactor_.Clone() : null);
		extraInfo_ = ((other.extraInfo_ != null) ? other.extraInfo_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public BattleMonsterInfo Clone()
	{
		return new BattleMonsterInfo(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as BattleMonsterInfo);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(BattleMonsterInfo other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!roundList_.Equals(other.roundList_))
		{
			return false;
		}
		if (!monsterList_.Equals(other.monsterList_))
		{
			return false;
		}
		if (!object.Equals(MonsterFactor, other.MonsterFactor))
		{
			return false;
		}
		if (!object.Equals(ExtraInfo, other.ExtraInfo))
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
		num ^= ((object)roundList_).GetHashCode();
		num ^= ((object)monsterList_).GetHashCode();
		if (monsterFactor_ != null)
		{
			num ^= MonsterFactor.GetHashCode();
		}
		if (extraInfo_ != null)
		{
			num ^= ExtraInfo.GetHashCode();
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
		roundList_.WriteTo(ref output, _repeated_roundList_codec);
		monsterList_.WriteTo(ref output, _repeated_monsterList_codec);
		if (monsterFactor_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)26);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)MonsterFactor);
		}
		if (extraInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)34);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)ExtraInfo);
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
		num += roundList_.CalculateSize(_repeated_roundList_codec);
		num += monsterList_.CalculateSize(_repeated_monsterList_codec);
		if (monsterFactor_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)MonsterFactor);
		}
		if (extraInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)ExtraInfo);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(BattleMonsterInfo other)
	{
		if (other == null)
		{
			return;
		}
		roundList_.Add((IEnumerable<BattleMonsterRoundDetail>)other.roundList_);
		monsterList_.Add((IEnumerable<BattleMonsterDetail>)other.monsterList_);
		if (other.monsterFactor_ != null)
		{
			if (monsterFactor_ == null)
			{
				MonsterFactor = new BattleMonsterFactor();
			}
			MonsterFactor.MergeFrom(other.MonsterFactor);
		}
		if (other.extraInfo_ != null)
		{
			if (extraInfo_ == null)
			{
				ExtraInfo = new BattleMonsterExtraInfo();
			}
			ExtraInfo.MergeFrom(other.ExtraInfo);
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
				roundList_.AddEntriesFrom(ref input, _repeated_roundList_codec);
				break;
			case 18u:
				monsterList_.AddEntriesFrom(ref input, _repeated_monsterList_codec);
				break;
			case 26u:
				if (monsterFactor_ == null)
				{
					MonsterFactor = new BattleMonsterFactor();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)MonsterFactor);
				break;
			case 34u:
				if (extraInfo_ == null)
				{
					ExtraInfo = new BattleMonsterExtraInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)ExtraInfo);
				break;
			}
		}
	}
}
