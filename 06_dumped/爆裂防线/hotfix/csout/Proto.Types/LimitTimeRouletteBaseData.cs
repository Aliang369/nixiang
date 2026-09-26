using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class LimitTimeRouletteBaseData : IMessage<LimitTimeRouletteBaseData>, IMessage, IEquatable<LimitTimeRouletteBaseData>, IDeepCloneable<LimitTimeRouletteBaseData>, IBufferMessage
{
	private static readonly MessageParser<LimitTimeRouletteBaseData> _parser = new MessageParser<LimitTimeRouletteBaseData>((Func<LimitTimeRouletteBaseData>)(() => new LimitTimeRouletteBaseData()));

	private UnknownFieldSet _unknownFields;

	public const int HeroTidFieldNumber = 1;

	private int heroTid_;

	public const int LimitVipFieldNumber = 2;

	private int limitVip_;

	public const int OnceCostFieldNumber = 3;

	private int onceCost_;

	public const int TenCostFieldNumber = 4;

	private int tenCost_;

	public const int OnceLuckyFieldNumber = 5;

	private int onceLucky_;

	public const int MaxLuckyFieldNumber = 6;

	private int maxLucky_;

	public const int CountFieldNumber = 7;

	private int count_;

	public const int LuckyFieldNumber = 8;

	private int lucky_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<LimitTimeRouletteBaseData> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeLimitTimeRouletteBaseDataReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int HeroTid
	{
		get
		{
			return heroTid_;
		}
		set
		{
			heroTid_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int LimitVip
	{
		get
		{
			return limitVip_;
		}
		set
		{
			limitVip_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int OnceCost
	{
		get
		{
			return onceCost_;
		}
		set
		{
			onceCost_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int TenCost
	{
		get
		{
			return tenCost_;
		}
		set
		{
			tenCost_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int OnceLucky
	{
		get
		{
			return onceLucky_;
		}
		set
		{
			onceLucky_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int MaxLucky
	{
		get
		{
			return maxLucky_;
		}
		set
		{
			maxLucky_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int Count
	{
		get
		{
			return count_;
		}
		set
		{
			count_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int Lucky
	{
		get
		{
			return lucky_;
		}
		set
		{
			lucky_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public LimitTimeRouletteBaseData()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public LimitTimeRouletteBaseData(LimitTimeRouletteBaseData other)
		: this()
	{
		heroTid_ = other.heroTid_;
		limitVip_ = other.limitVip_;
		onceCost_ = other.onceCost_;
		tenCost_ = other.tenCost_;
		onceLucky_ = other.onceLucky_;
		maxLucky_ = other.maxLucky_;
		count_ = other.count_;
		lucky_ = other.lucky_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public LimitTimeRouletteBaseData Clone()
	{
		return new LimitTimeRouletteBaseData(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as LimitTimeRouletteBaseData);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(LimitTimeRouletteBaseData other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (HeroTid != other.HeroTid)
		{
			return false;
		}
		if (LimitVip != other.LimitVip)
		{
			return false;
		}
		if (OnceCost != other.OnceCost)
		{
			return false;
		}
		if (TenCost != other.TenCost)
		{
			return false;
		}
		if (OnceLucky != other.OnceLucky)
		{
			return false;
		}
		if (MaxLucky != other.MaxLucky)
		{
			return false;
		}
		if (Count != other.Count)
		{
			return false;
		}
		if (Lucky != other.Lucky)
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
		if (HeroTid != 0)
		{
			num ^= HeroTid.GetHashCode();
		}
		if (LimitVip != 0)
		{
			num ^= LimitVip.GetHashCode();
		}
		if (OnceCost != 0)
		{
			num ^= OnceCost.GetHashCode();
		}
		if (TenCost != 0)
		{
			num ^= TenCost.GetHashCode();
		}
		if (OnceLucky != 0)
		{
			num ^= OnceLucky.GetHashCode();
		}
		if (MaxLucky != 0)
		{
			num ^= MaxLucky.GetHashCode();
		}
		if (Count != 0)
		{
			num ^= Count.GetHashCode();
		}
		if (Lucky != 0)
		{
			num ^= Lucky.GetHashCode();
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
		if (HeroTid != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteInt32(HeroTid);
		}
		if (LimitVip != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteInt32(LimitVip);
		}
		if (OnceCost != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)24);
			((WriteContext)(ref output)).WriteInt32(OnceCost);
		}
		if (TenCost != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)32);
			((WriteContext)(ref output)).WriteInt32(TenCost);
		}
		if (OnceLucky != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)40);
			((WriteContext)(ref output)).WriteInt32(OnceLucky);
		}
		if (MaxLucky != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)48);
			((WriteContext)(ref output)).WriteInt32(MaxLucky);
		}
		if (Count != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)56);
			((WriteContext)(ref output)).WriteInt32(Count);
		}
		if (Lucky != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)64);
			((WriteContext)(ref output)).WriteInt32(Lucky);
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
		if (HeroTid != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(HeroTid);
		}
		if (LimitVip != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(LimitVip);
		}
		if (OnceCost != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(OnceCost);
		}
		if (TenCost != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(TenCost);
		}
		if (OnceLucky != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(OnceLucky);
		}
		if (MaxLucky != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(MaxLucky);
		}
		if (Count != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(Count);
		}
		if (Lucky != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(Lucky);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(LimitTimeRouletteBaseData other)
	{
		if (other != null)
		{
			if (other.HeroTid != 0)
			{
				HeroTid = other.HeroTid;
			}
			if (other.LimitVip != 0)
			{
				LimitVip = other.LimitVip;
			}
			if (other.OnceCost != 0)
			{
				OnceCost = other.OnceCost;
			}
			if (other.TenCost != 0)
			{
				TenCost = other.TenCost;
			}
			if (other.OnceLucky != 0)
			{
				OnceLucky = other.OnceLucky;
			}
			if (other.MaxLucky != 0)
			{
				MaxLucky = other.MaxLucky;
			}
			if (other.Count != 0)
			{
				Count = other.Count;
			}
			if (other.Lucky != 0)
			{
				Lucky = other.Lucky;
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
				HeroTid = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 16u:
				LimitVip = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 24u:
				OnceCost = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 32u:
				TenCost = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 40u:
				OnceLucky = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 48u:
				MaxLucky = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 56u:
				Count = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 64u:
				Lucky = ((ParseContext)(ref input)).ReadInt32();
				break;
			}
		}
	}
}
