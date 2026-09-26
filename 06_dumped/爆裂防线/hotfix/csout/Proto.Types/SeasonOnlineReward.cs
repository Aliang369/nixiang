using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class SeasonOnlineReward : IMessage<SeasonOnlineReward>, IMessage, IEquatable<SeasonOnlineReward>, IDeepCloneable<SeasonOnlineReward>, IBufferMessage
{
	private static readonly MessageParser<SeasonOnlineReward> _parser = new MessageParser<SeasonOnlineReward>((Func<SeasonOnlineReward>)(() => new SeasonOnlineReward()));

	private UnknownFieldSet _unknownFields;

	public const int TierFieldNumber = 1;

	private int tier_;

	public const int UnlockedFieldNumber = 2;

	private int unlocked_;

	public const int ClaimedFieldNumber = 3;

	private int claimed_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<SeasonOnlineReward> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeSeasonOnlineRewardReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int Tier
	{
		get
		{
			return tier_;
		}
		set
		{
			tier_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int Unlocked
	{
		get
		{
			return unlocked_;
		}
		set
		{
			unlocked_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int Claimed
	{
		get
		{
			return claimed_;
		}
		set
		{
			claimed_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SeasonOnlineReward()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SeasonOnlineReward(SeasonOnlineReward other)
		: this()
	{
		tier_ = other.tier_;
		unlocked_ = other.unlocked_;
		claimed_ = other.claimed_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SeasonOnlineReward Clone()
	{
		return new SeasonOnlineReward(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as SeasonOnlineReward);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(SeasonOnlineReward other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (Tier != other.Tier)
		{
			return false;
		}
		if (Unlocked != other.Unlocked)
		{
			return false;
		}
		if (Claimed != other.Claimed)
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
		if (Tier != 0)
		{
			num ^= Tier.GetHashCode();
		}
		if (Unlocked != 0)
		{
			num ^= Unlocked.GetHashCode();
		}
		if (Claimed != 0)
		{
			num ^= Claimed.GetHashCode();
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
		if (Tier != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteInt32(Tier);
		}
		if (Unlocked != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteInt32(Unlocked);
		}
		if (Claimed != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)24);
			((WriteContext)(ref output)).WriteInt32(Claimed);
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
		if (Tier != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(Tier);
		}
		if (Unlocked != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(Unlocked);
		}
		if (Claimed != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(Claimed);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(SeasonOnlineReward other)
	{
		if (other != null)
		{
			if (other.Tier != 0)
			{
				Tier = other.Tier;
			}
			if (other.Unlocked != 0)
			{
				Unlocked = other.Unlocked;
			}
			if (other.Claimed != 0)
			{
				Claimed = other.Claimed;
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
				Tier = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 16u:
				Unlocked = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 24u:
				Claimed = ((ParseContext)(ref input)).ReadInt32();
				break;
			}
		}
	}
}
