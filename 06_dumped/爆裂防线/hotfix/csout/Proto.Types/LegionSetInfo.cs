using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class LegionSetInfo : IMessage<LegionSetInfo>, IMessage, IEquatable<LegionSetInfo>, IDeepCloneable<LegionSetInfo>, IBufferMessage
{
	private static readonly MessageParser<LegionSetInfo> _parser = new MessageParser<LegionSetInfo>((Func<LegionSetInfo>)(() => new LegionSetInfo()));

	private UnknownFieldSet _unknownFields;

	public const int LeaderIdFieldNumber = 1;

	private long leaderId_;

	public const int AdviserFieldNumber = 2;

	private static readonly FieldCodec<long> _repeated_adviser_codec = FieldCodec.ForInt64(18u);

	private readonly RepeatedField<long> adviser_ = new RepeatedField<long>();

	public const int KickCountFieldNumber = 3;

	private int kickCount_;

	public const int LeaderChangeCDFieldNumber = 4;

	private long leaderChangeCD_;

	public const int LegionSignFieldNumber = 5;

	private string legionSign_ = "";

	public const int LegionExpFieldNumber = 6;

	private int legionExp_;

	public const int DonateCountFieldNumber = 7;

	private int donateCount_;

	public const int RewardCountFieldNumber = 8;

	private int rewardCount_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<LegionSetInfo> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeLegionSetInfoReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public long LeaderId
	{
		get
		{
			return leaderId_;
		}
		set
		{
			leaderId_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<long> Adviser => adviser_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int KickCount
	{
		get
		{
			return kickCount_;
		}
		set
		{
			kickCount_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public long LeaderChangeCD
	{
		get
		{
			return leaderChangeCD_;
		}
		set
		{
			leaderChangeCD_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public string LegionSign
	{
		get
		{
			return legionSign_;
		}
		set
		{
			legionSign_ = ProtoPreconditions.CheckNotNull<string>(value, "value");
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int LegionExp
	{
		get
		{
			return legionExp_;
		}
		set
		{
			legionExp_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int DonateCount
	{
		get
		{
			return donateCount_;
		}
		set
		{
			donateCount_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int RewardCount
	{
		get
		{
			return rewardCount_;
		}
		set
		{
			rewardCount_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public LegionSetInfo()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public LegionSetInfo(LegionSetInfo other)
		: this()
	{
		leaderId_ = other.leaderId_;
		adviser_ = other.adviser_.Clone();
		kickCount_ = other.kickCount_;
		leaderChangeCD_ = other.leaderChangeCD_;
		legionSign_ = other.legionSign_;
		legionExp_ = other.legionExp_;
		donateCount_ = other.donateCount_;
		rewardCount_ = other.rewardCount_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public LegionSetInfo Clone()
	{
		return new LegionSetInfo(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as LegionSetInfo);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(LegionSetInfo other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (LeaderId != other.LeaderId)
		{
			return false;
		}
		if (!adviser_.Equals(other.adviser_))
		{
			return false;
		}
		if (KickCount != other.KickCount)
		{
			return false;
		}
		if (LeaderChangeCD != other.LeaderChangeCD)
		{
			return false;
		}
		if (LegionSign != other.LegionSign)
		{
			return false;
		}
		if (LegionExp != other.LegionExp)
		{
			return false;
		}
		if (DonateCount != other.DonateCount)
		{
			return false;
		}
		if (RewardCount != other.RewardCount)
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
		if (LeaderId != 0L)
		{
			num ^= LeaderId.GetHashCode();
		}
		num ^= ((object)adviser_).GetHashCode();
		if (KickCount != 0)
		{
			num ^= KickCount.GetHashCode();
		}
		if (LeaderChangeCD != 0L)
		{
			num ^= LeaderChangeCD.GetHashCode();
		}
		if (LegionSign.Length != 0)
		{
			num ^= LegionSign.GetHashCode();
		}
		if (LegionExp != 0)
		{
			num ^= LegionExp.GetHashCode();
		}
		if (DonateCount != 0)
		{
			num ^= DonateCount.GetHashCode();
		}
		if (RewardCount != 0)
		{
			num ^= RewardCount.GetHashCode();
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
		if (LeaderId != 0L)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteInt64(LeaderId);
		}
		adviser_.WriteTo(ref output, _repeated_adviser_codec);
		if (KickCount != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)24);
			((WriteContext)(ref output)).WriteInt32(KickCount);
		}
		if (LeaderChangeCD != 0L)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)32);
			((WriteContext)(ref output)).WriteInt64(LeaderChangeCD);
		}
		if (LegionSign.Length != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)42);
			((WriteContext)(ref output)).WriteString(LegionSign);
		}
		if (LegionExp != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)48);
			((WriteContext)(ref output)).WriteInt32(LegionExp);
		}
		if (DonateCount != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)56);
			((WriteContext)(ref output)).WriteInt32(DonateCount);
		}
		if (RewardCount != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)64);
			((WriteContext)(ref output)).WriteInt32(RewardCount);
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
		if (LeaderId != 0L)
		{
			num += 1 + CodedOutputStream.ComputeInt64Size(LeaderId);
		}
		num += adviser_.CalculateSize(_repeated_adviser_codec);
		if (KickCount != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(KickCount);
		}
		if (LeaderChangeCD != 0L)
		{
			num += 1 + CodedOutputStream.ComputeInt64Size(LeaderChangeCD);
		}
		if (LegionSign.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(LegionSign);
		}
		if (LegionExp != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(LegionExp);
		}
		if (DonateCount != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(DonateCount);
		}
		if (RewardCount != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(RewardCount);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(LegionSetInfo other)
	{
		if (other != null)
		{
			if (other.LeaderId != 0L)
			{
				LeaderId = other.LeaderId;
			}
			adviser_.Add((IEnumerable<long>)other.adviser_);
			if (other.KickCount != 0)
			{
				KickCount = other.KickCount;
			}
			if (other.LeaderChangeCD != 0L)
			{
				LeaderChangeCD = other.LeaderChangeCD;
			}
			if (other.LegionSign.Length != 0)
			{
				LegionSign = other.LegionSign;
			}
			if (other.LegionExp != 0)
			{
				LegionExp = other.LegionExp;
			}
			if (other.DonateCount != 0)
			{
				DonateCount = other.DonateCount;
			}
			if (other.RewardCount != 0)
			{
				RewardCount = other.RewardCount;
			}
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
				LeaderId = ((ParseContext)(ref input)).ReadInt64();
				break;
			case 16u:
			case 18u:
				adviser_.AddEntriesFrom(ref input, _repeated_adviser_codec);
				break;
			case 24u:
				KickCount = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 32u:
				LeaderChangeCD = ((ParseContext)(ref input)).ReadInt64();
				break;
			case 42u:
				LegionSign = ((ParseContext)(ref input)).ReadString();
				break;
			case 48u:
				LegionExp = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 56u:
				DonateCount = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 64u:
				RewardCount = ((ParseContext)(ref input)).ReadInt32();
				break;
			}
		}
	}
}
