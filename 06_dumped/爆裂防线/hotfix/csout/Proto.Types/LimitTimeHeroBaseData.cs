using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class LimitTimeHeroBaseData : IMessage<LimitTimeHeroBaseData>, IMessage, IEquatable<LimitTimeHeroBaseData>, IDeepCloneable<LimitTimeHeroBaseData>, IBufferMessage
{
	private static readonly MessageParser<LimitTimeHeroBaseData> _parser = new MessageParser<LimitTimeHeroBaseData>((Func<LimitTimeHeroBaseData>)(() => new LimitTimeHeroBaseData()));

	private UnknownFieldSet _unknownFields;

	public const int HeroTidFieldNumber = 1;

	private int heroTid_;

	public const int OnceCostFieldNumber = 2;

	private int onceCost_;

	public const int OnceScoreFieldNumber = 3;

	private int onceScore_;

	public const int TotalScoreFieldNumber = 4;

	private int totalScore_;

	public const int LeftScoreFieldNumber = 5;

	private int leftScore_;

	public const int NextFreeTmFieldNumber = 6;

	private long nextFreeTm_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<LimitTimeHeroBaseData> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeLimitTimeHeroBaseDataReflection.Descriptor.MessageTypes[0];

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
	public int OnceScore
	{
		get
		{
			return onceScore_;
		}
		set
		{
			onceScore_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int TotalScore
	{
		get
		{
			return totalScore_;
		}
		set
		{
			totalScore_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int LeftScore
	{
		get
		{
			return leftScore_;
		}
		set
		{
			leftScore_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public long NextFreeTm
	{
		get
		{
			return nextFreeTm_;
		}
		set
		{
			nextFreeTm_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public LimitTimeHeroBaseData()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public LimitTimeHeroBaseData(LimitTimeHeroBaseData other)
		: this()
	{
		heroTid_ = other.heroTid_;
		onceCost_ = other.onceCost_;
		onceScore_ = other.onceScore_;
		totalScore_ = other.totalScore_;
		leftScore_ = other.leftScore_;
		nextFreeTm_ = other.nextFreeTm_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public LimitTimeHeroBaseData Clone()
	{
		return new LimitTimeHeroBaseData(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as LimitTimeHeroBaseData);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(LimitTimeHeroBaseData other)
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
		if (OnceCost != other.OnceCost)
		{
			return false;
		}
		if (OnceScore != other.OnceScore)
		{
			return false;
		}
		if (TotalScore != other.TotalScore)
		{
			return false;
		}
		if (LeftScore != other.LeftScore)
		{
			return false;
		}
		if (NextFreeTm != other.NextFreeTm)
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
		if (OnceCost != 0)
		{
			num ^= OnceCost.GetHashCode();
		}
		if (OnceScore != 0)
		{
			num ^= OnceScore.GetHashCode();
		}
		if (TotalScore != 0)
		{
			num ^= TotalScore.GetHashCode();
		}
		if (LeftScore != 0)
		{
			num ^= LeftScore.GetHashCode();
		}
		if (NextFreeTm != 0L)
		{
			num ^= NextFreeTm.GetHashCode();
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
		if (OnceCost != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteInt32(OnceCost);
		}
		if (OnceScore != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)24);
			((WriteContext)(ref output)).WriteInt32(OnceScore);
		}
		if (TotalScore != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)32);
			((WriteContext)(ref output)).WriteInt32(TotalScore);
		}
		if (LeftScore != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)40);
			((WriteContext)(ref output)).WriteInt32(LeftScore);
		}
		if (NextFreeTm != 0L)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)48);
			((WriteContext)(ref output)).WriteInt64(NextFreeTm);
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
		if (OnceCost != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(OnceCost);
		}
		if (OnceScore != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(OnceScore);
		}
		if (TotalScore != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(TotalScore);
		}
		if (LeftScore != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(LeftScore);
		}
		if (NextFreeTm != 0L)
		{
			num += 1 + CodedOutputStream.ComputeInt64Size(NextFreeTm);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(LimitTimeHeroBaseData other)
	{
		if (other != null)
		{
			if (other.HeroTid != 0)
			{
				HeroTid = other.HeroTid;
			}
			if (other.OnceCost != 0)
			{
				OnceCost = other.OnceCost;
			}
			if (other.OnceScore != 0)
			{
				OnceScore = other.OnceScore;
			}
			if (other.TotalScore != 0)
			{
				TotalScore = other.TotalScore;
			}
			if (other.LeftScore != 0)
			{
				LeftScore = other.LeftScore;
			}
			if (other.NextFreeTm != 0L)
			{
				NextFreeTm = other.NextFreeTm;
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
				OnceCost = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 24u:
				OnceScore = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 32u:
				TotalScore = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 40u:
				LeftScore = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 48u:
				NextFreeTm = ((ParseContext)(ref input)).ReadInt64();
				break;
			}
		}
	}
}
