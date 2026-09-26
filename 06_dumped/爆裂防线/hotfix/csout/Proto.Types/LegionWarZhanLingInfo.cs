using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class LegionWarZhanLingInfo : IMessage<LegionWarZhanLingInfo>, IMessage, IEquatable<LegionWarZhanLingInfo>, IDeepCloneable<LegionWarZhanLingInfo>, IBufferMessage
{
	private static readonly MessageParser<LegionWarZhanLingInfo> _parser = new MessageParser<LegionWarZhanLingInfo>((Func<LegionWarZhanLingInfo>)(() => new LegionWarZhanLingInfo()));

	private UnknownFieldSet _unknownFields;

	public const int SeasonFieldNumber = 1;

	private int season_;

	public const int ScoreFieldNumber = 2;

	private int score_;

	public const int LevelFieldNumber = 3;

	private int level_;

	public const int MediumActiveFieldNumber = 4;

	private EBoolState mediumActive_;

	public const int HighActiveFieldNumber = 5;

	private EBoolState highActive_;

	public const int LowAwardFieldNumber = 6;

	private int lowAward_;

	public const int MediumAwardFieldNumber = 7;

	private int mediumAward_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<LegionWarZhanLingInfo> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeLegionWarZhanLingInfoReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int Season
	{
		get
		{
			return season_;
		}
		set
		{
			season_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int Score
	{
		get
		{
			return score_;
		}
		set
		{
			score_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int Level
	{
		get
		{
			return level_;
		}
		set
		{
			level_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public EBoolState MediumActive
	{
		get
		{
			return mediumActive_;
		}
		set
		{
			mediumActive_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public EBoolState HighActive
	{
		get
		{
			return highActive_;
		}
		set
		{
			highActive_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int LowAward
	{
		get
		{
			return lowAward_;
		}
		set
		{
			lowAward_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int MediumAward
	{
		get
		{
			return mediumAward_;
		}
		set
		{
			mediumAward_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public LegionWarZhanLingInfo()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public LegionWarZhanLingInfo(LegionWarZhanLingInfo other)
		: this()
	{
		season_ = other.season_;
		score_ = other.score_;
		level_ = other.level_;
		mediumActive_ = other.mediumActive_;
		highActive_ = other.highActive_;
		lowAward_ = other.lowAward_;
		mediumAward_ = other.mediumAward_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public LegionWarZhanLingInfo Clone()
	{
		return new LegionWarZhanLingInfo(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as LegionWarZhanLingInfo);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(LegionWarZhanLingInfo other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (Season != other.Season)
		{
			return false;
		}
		if (Score != other.Score)
		{
			return false;
		}
		if (Level != other.Level)
		{
			return false;
		}
		if (MediumActive != other.MediumActive)
		{
			return false;
		}
		if (HighActive != other.HighActive)
		{
			return false;
		}
		if (LowAward != other.LowAward)
		{
			return false;
		}
		if (MediumAward != other.MediumAward)
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
		if (Season != 0)
		{
			num ^= Season.GetHashCode();
		}
		if (Score != 0)
		{
			num ^= Score.GetHashCode();
		}
		if (Level != 0)
		{
			num ^= Level.GetHashCode();
		}
		if (MediumActive != EBoolState.False)
		{
			num ^= MediumActive.GetHashCode();
		}
		if (HighActive != EBoolState.False)
		{
			num ^= HighActive.GetHashCode();
		}
		if (LowAward != 0)
		{
			num ^= LowAward.GetHashCode();
		}
		if (MediumAward != 0)
		{
			num ^= MediumAward.GetHashCode();
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
		if (Season != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteInt32(Season);
		}
		if (Score != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteInt32(Score);
		}
		if (Level != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)24);
			((WriteContext)(ref output)).WriteInt32(Level);
		}
		if (MediumActive != EBoolState.False)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)32);
			((WriteContext)(ref output)).WriteEnum((int)MediumActive);
		}
		if (HighActive != EBoolState.False)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)40);
			((WriteContext)(ref output)).WriteEnum((int)HighActive);
		}
		if (LowAward != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)48);
			((WriteContext)(ref output)).WriteInt32(LowAward);
		}
		if (MediumAward != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)56);
			((WriteContext)(ref output)).WriteInt32(MediumAward);
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
		if (Season != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(Season);
		}
		if (Score != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(Score);
		}
		if (Level != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(Level);
		}
		if (MediumActive != EBoolState.False)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)MediumActive);
		}
		if (HighActive != EBoolState.False)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)HighActive);
		}
		if (LowAward != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(LowAward);
		}
		if (MediumAward != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(MediumAward);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(LegionWarZhanLingInfo other)
	{
		if (other != null)
		{
			if (other.Season != 0)
			{
				Season = other.Season;
			}
			if (other.Score != 0)
			{
				Score = other.Score;
			}
			if (other.Level != 0)
			{
				Level = other.Level;
			}
			if (other.MediumActive != EBoolState.False)
			{
				MediumActive = other.MediumActive;
			}
			if (other.HighActive != EBoolState.False)
			{
				HighActive = other.HighActive;
			}
			if (other.LowAward != 0)
			{
				LowAward = other.LowAward;
			}
			if (other.MediumAward != 0)
			{
				MediumAward = other.MediumAward;
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
				Season = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 16u:
				Score = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 24u:
				Level = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 32u:
				MediumActive = (EBoolState)((ParseContext)(ref input)).ReadEnum();
				break;
			case 40u:
				HighActive = (EBoolState)((ParseContext)(ref input)).ReadEnum();
				break;
			case 48u:
				LowAward = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 56u:
				MediumAward = ((ParseContext)(ref input)).ReadInt32();
				break;
			}
		}
	}
}
