using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class SeasonAirdropInfo : IMessage<SeasonAirdropInfo>, IMessage, IEquatable<SeasonAirdropInfo>, IDeepCloneable<SeasonAirdropInfo>, IBufferMessage
{
	private static readonly MessageParser<SeasonAirdropInfo> _parser = new MessageParser<SeasonAirdropInfo>((Func<SeasonAirdropInfo>)(() => new SeasonAirdropInfo()));

	private UnknownFieldSet _unknownFields;

	public const int DropIdFieldNumber = 1;

	private int dropId_;

	public const int QualityFieldNumber = 2;

	private ESeasonAirdropQuality quality_;

	public const int PosXFieldNumber = 3;

	private float posX_;

	public const int PosYFieldNumber = 4;

	private float posY_;

	public const int TotalProgFieldNumber = 5;

	private int totalProg_;

	public const int CurProgFieldNumber = 6;

	private int curProg_;

	public const int CapacityFieldNumber = 7;

	private int capacity_;

	public const int ParticipantsFieldNumber = 8;

	private static readonly FieldCodec<SeasonAirdropParticipant> _repeated_participants_codec = FieldCodec.ForMessage<SeasonAirdropParticipant>(66u, SeasonAirdropParticipant.Parser);

	private readonly RepeatedField<SeasonAirdropParticipant> participants_ = new RepeatedField<SeasonAirdropParticipant>();

	public const int DailyWinFieldNumber = 9;

	private int dailyWin_;

	public const int DailyLimitFieldNumber = 10;

	private int dailyLimit_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<SeasonAirdropInfo> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeSeasonAirdropInfoReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int DropId
	{
		get
		{
			return dropId_;
		}
		set
		{
			dropId_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ESeasonAirdropQuality Quality
	{
		get
		{
			return quality_;
		}
		set
		{
			quality_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public float PosX
	{
		get
		{
			return posX_;
		}
		set
		{
			posX_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public float PosY
	{
		get
		{
			return posY_;
		}
		set
		{
			posY_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int TotalProg
	{
		get
		{
			return totalProg_;
		}
		set
		{
			totalProg_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int CurProg
	{
		get
		{
			return curProg_;
		}
		set
		{
			curProg_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int Capacity
	{
		get
		{
			return capacity_;
		}
		set
		{
			capacity_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<SeasonAirdropParticipant> Participants => participants_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int DailyWin
	{
		get
		{
			return dailyWin_;
		}
		set
		{
			dailyWin_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int DailyLimit
	{
		get
		{
			return dailyLimit_;
		}
		set
		{
			dailyLimit_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SeasonAirdropInfo()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SeasonAirdropInfo(SeasonAirdropInfo other)
		: this()
	{
		dropId_ = other.dropId_;
		quality_ = other.quality_;
		posX_ = other.posX_;
		posY_ = other.posY_;
		totalProg_ = other.totalProg_;
		curProg_ = other.curProg_;
		capacity_ = other.capacity_;
		participants_ = other.participants_.Clone();
		dailyWin_ = other.dailyWin_;
		dailyLimit_ = other.dailyLimit_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SeasonAirdropInfo Clone()
	{
		return new SeasonAirdropInfo(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as SeasonAirdropInfo);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(SeasonAirdropInfo other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (DropId != other.DropId)
		{
			return false;
		}
		if (Quality != other.Quality)
		{
			return false;
		}
		if (!ProtobufEqualityComparers.BitwiseSingleEqualityComparer.Equals(PosX, other.PosX))
		{
			return false;
		}
		if (!ProtobufEqualityComparers.BitwiseSingleEqualityComparer.Equals(PosY, other.PosY))
		{
			return false;
		}
		if (TotalProg != other.TotalProg)
		{
			return false;
		}
		if (CurProg != other.CurProg)
		{
			return false;
		}
		if (Capacity != other.Capacity)
		{
			return false;
		}
		if (!participants_.Equals(other.participants_))
		{
			return false;
		}
		if (DailyWin != other.DailyWin)
		{
			return false;
		}
		if (DailyLimit != other.DailyLimit)
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
		if (DropId != 0)
		{
			num ^= DropId.GetHashCode();
		}
		if (Quality != ESeasonAirdropQuality.EmptyValue)
		{
			num ^= Quality.GetHashCode();
		}
		if (PosX != 0f)
		{
			num ^= ProtobufEqualityComparers.BitwiseSingleEqualityComparer.GetHashCode(PosX);
		}
		if (PosY != 0f)
		{
			num ^= ProtobufEqualityComparers.BitwiseSingleEqualityComparer.GetHashCode(PosY);
		}
		if (TotalProg != 0)
		{
			num ^= TotalProg.GetHashCode();
		}
		if (CurProg != 0)
		{
			num ^= CurProg.GetHashCode();
		}
		if (Capacity != 0)
		{
			num ^= Capacity.GetHashCode();
		}
		num ^= ((object)participants_).GetHashCode();
		if (DailyWin != 0)
		{
			num ^= DailyWin.GetHashCode();
		}
		if (DailyLimit != 0)
		{
			num ^= DailyLimit.GetHashCode();
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
		if (DropId != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteInt32(DropId);
		}
		if (Quality != ESeasonAirdropQuality.EmptyValue)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteEnum((int)Quality);
		}
		if (PosX != 0f)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)29);
			((WriteContext)(ref output)).WriteFloat(PosX);
		}
		if (PosY != 0f)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)37);
			((WriteContext)(ref output)).WriteFloat(PosY);
		}
		if (TotalProg != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)40);
			((WriteContext)(ref output)).WriteInt32(TotalProg);
		}
		if (CurProg != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)48);
			((WriteContext)(ref output)).WriteInt32(CurProg);
		}
		if (Capacity != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)56);
			((WriteContext)(ref output)).WriteInt32(Capacity);
		}
		participants_.WriteTo(ref output, _repeated_participants_codec);
		if (DailyWin != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)72);
			((WriteContext)(ref output)).WriteInt32(DailyWin);
		}
		if (DailyLimit != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)80);
			((WriteContext)(ref output)).WriteInt32(DailyLimit);
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
		if (DropId != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(DropId);
		}
		if (Quality != ESeasonAirdropQuality.EmptyValue)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)Quality);
		}
		if (PosX != 0f)
		{
			num += 5;
		}
		if (PosY != 0f)
		{
			num += 5;
		}
		if (TotalProg != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(TotalProg);
		}
		if (CurProg != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(CurProg);
		}
		if (Capacity != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(Capacity);
		}
		num += participants_.CalculateSize(_repeated_participants_codec);
		if (DailyWin != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(DailyWin);
		}
		if (DailyLimit != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(DailyLimit);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(SeasonAirdropInfo other)
	{
		if (other != null)
		{
			if (other.DropId != 0)
			{
				DropId = other.DropId;
			}
			if (other.Quality != ESeasonAirdropQuality.EmptyValue)
			{
				Quality = other.Quality;
			}
			if (other.PosX != 0f)
			{
				PosX = other.PosX;
			}
			if (other.PosY != 0f)
			{
				PosY = other.PosY;
			}
			if (other.TotalProg != 0)
			{
				TotalProg = other.TotalProg;
			}
			if (other.CurProg != 0)
			{
				CurProg = other.CurProg;
			}
			if (other.Capacity != 0)
			{
				Capacity = other.Capacity;
			}
			participants_.Add((IEnumerable<SeasonAirdropParticipant>)other.participants_);
			if (other.DailyWin != 0)
			{
				DailyWin = other.DailyWin;
			}
			if (other.DailyLimit != 0)
			{
				DailyLimit = other.DailyLimit;
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
				DropId = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 16u:
				Quality = (ESeasonAirdropQuality)((ParseContext)(ref input)).ReadEnum();
				break;
			case 29u:
				PosX = ((ParseContext)(ref input)).ReadFloat();
				break;
			case 37u:
				PosY = ((ParseContext)(ref input)).ReadFloat();
				break;
			case 40u:
				TotalProg = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 48u:
				CurProg = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 56u:
				Capacity = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 66u:
				participants_.AddEntriesFrom(ref input, _repeated_participants_codec);
				break;
			case 72u:
				DailyWin = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 80u:
				DailyLimit = ((ParseContext)(ref input)).ReadInt32();
				break;
			}
		}
	}
}
