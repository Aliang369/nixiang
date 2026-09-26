using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class SeasonTeamMarchInfo : IMessage<SeasonTeamMarchInfo>, IMessage, IEquatable<SeasonTeamMarchInfo>, IDeepCloneable<SeasonTeamMarchInfo>, IBufferMessage
{
	private static readonly MessageParser<SeasonTeamMarchInfo> _parser = new MessageParser<SeasonTeamMarchInfo>((Func<SeasonTeamMarchInfo>)(() => new SeasonTeamMarchInfo()));

	private UnknownFieldSet _unknownFields;

	public const int ActionFieldNumber = 1;

	private int action_;

	public const int IsReturnFieldNumber = 2;

	private int isReturn_;

	public const int FromXFieldNumber = 3;

	private float fromX_;

	public const int FromYFieldNumber = 4;

	private float fromY_;

	public const int ToXFieldNumber = 5;

	private float toX_;

	public const int ToYFieldNumber = 6;

	private float toY_;

	public const int StartTmFieldNumber = 7;

	private long startTm_;

	public const int ArrivalTmFieldNumber = 8;

	private long arrivalTm_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<SeasonTeamMarchInfo> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeSeasonTeamMarchInfoReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int Action
	{
		get
		{
			return action_;
		}
		set
		{
			action_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int IsReturn
	{
		get
		{
			return isReturn_;
		}
		set
		{
			isReturn_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public float FromX
	{
		get
		{
			return fromX_;
		}
		set
		{
			fromX_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public float FromY
	{
		get
		{
			return fromY_;
		}
		set
		{
			fromY_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public float ToX
	{
		get
		{
			return toX_;
		}
		set
		{
			toX_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public float ToY
	{
		get
		{
			return toY_;
		}
		set
		{
			toY_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public long StartTm
	{
		get
		{
			return startTm_;
		}
		set
		{
			startTm_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public long ArrivalTm
	{
		get
		{
			return arrivalTm_;
		}
		set
		{
			arrivalTm_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SeasonTeamMarchInfo()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SeasonTeamMarchInfo(SeasonTeamMarchInfo other)
		: this()
	{
		action_ = other.action_;
		isReturn_ = other.isReturn_;
		fromX_ = other.fromX_;
		fromY_ = other.fromY_;
		toX_ = other.toX_;
		toY_ = other.toY_;
		startTm_ = other.startTm_;
		arrivalTm_ = other.arrivalTm_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SeasonTeamMarchInfo Clone()
	{
		return new SeasonTeamMarchInfo(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as SeasonTeamMarchInfo);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(SeasonTeamMarchInfo other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (Action != other.Action)
		{
			return false;
		}
		if (IsReturn != other.IsReturn)
		{
			return false;
		}
		if (!ProtobufEqualityComparers.BitwiseSingleEqualityComparer.Equals(FromX, other.FromX))
		{
			return false;
		}
		if (!ProtobufEqualityComparers.BitwiseSingleEqualityComparer.Equals(FromY, other.FromY))
		{
			return false;
		}
		if (!ProtobufEqualityComparers.BitwiseSingleEqualityComparer.Equals(ToX, other.ToX))
		{
			return false;
		}
		if (!ProtobufEqualityComparers.BitwiseSingleEqualityComparer.Equals(ToY, other.ToY))
		{
			return false;
		}
		if (StartTm != other.StartTm)
		{
			return false;
		}
		if (ArrivalTm != other.ArrivalTm)
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
		if (Action != 0)
		{
			num ^= Action.GetHashCode();
		}
		if (IsReturn != 0)
		{
			num ^= IsReturn.GetHashCode();
		}
		if (FromX != 0f)
		{
			num ^= ProtobufEqualityComparers.BitwiseSingleEqualityComparer.GetHashCode(FromX);
		}
		if (FromY != 0f)
		{
			num ^= ProtobufEqualityComparers.BitwiseSingleEqualityComparer.GetHashCode(FromY);
		}
		if (ToX != 0f)
		{
			num ^= ProtobufEqualityComparers.BitwiseSingleEqualityComparer.GetHashCode(ToX);
		}
		if (ToY != 0f)
		{
			num ^= ProtobufEqualityComparers.BitwiseSingleEqualityComparer.GetHashCode(ToY);
		}
		if (StartTm != 0L)
		{
			num ^= StartTm.GetHashCode();
		}
		if (ArrivalTm != 0L)
		{
			num ^= ArrivalTm.GetHashCode();
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
		if (Action != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteInt32(Action);
		}
		if (IsReturn != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteInt32(IsReturn);
		}
		if (FromX != 0f)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)29);
			((WriteContext)(ref output)).WriteFloat(FromX);
		}
		if (FromY != 0f)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)37);
			((WriteContext)(ref output)).WriteFloat(FromY);
		}
		if (ToX != 0f)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)45);
			((WriteContext)(ref output)).WriteFloat(ToX);
		}
		if (ToY != 0f)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)53);
			((WriteContext)(ref output)).WriteFloat(ToY);
		}
		if (StartTm != 0L)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)56);
			((WriteContext)(ref output)).WriteInt64(StartTm);
		}
		if (ArrivalTm != 0L)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)64);
			((WriteContext)(ref output)).WriteInt64(ArrivalTm);
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
		if (Action != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(Action);
		}
		if (IsReturn != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(IsReturn);
		}
		if (FromX != 0f)
		{
			num += 5;
		}
		if (FromY != 0f)
		{
			num += 5;
		}
		if (ToX != 0f)
		{
			num += 5;
		}
		if (ToY != 0f)
		{
			num += 5;
		}
		if (StartTm != 0L)
		{
			num += 1 + CodedOutputStream.ComputeInt64Size(StartTm);
		}
		if (ArrivalTm != 0L)
		{
			num += 1 + CodedOutputStream.ComputeInt64Size(ArrivalTm);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(SeasonTeamMarchInfo other)
	{
		if (other != null)
		{
			if (other.Action != 0)
			{
				Action = other.Action;
			}
			if (other.IsReturn != 0)
			{
				IsReturn = other.IsReturn;
			}
			if (other.FromX != 0f)
			{
				FromX = other.FromX;
			}
			if (other.FromY != 0f)
			{
				FromY = other.FromY;
			}
			if (other.ToX != 0f)
			{
				ToX = other.ToX;
			}
			if (other.ToY != 0f)
			{
				ToY = other.ToY;
			}
			if (other.StartTm != 0L)
			{
				StartTm = other.StartTm;
			}
			if (other.ArrivalTm != 0L)
			{
				ArrivalTm = other.ArrivalTm;
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
				Action = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 16u:
				IsReturn = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 29u:
				FromX = ((ParseContext)(ref input)).ReadFloat();
				break;
			case 37u:
				FromY = ((ParseContext)(ref input)).ReadFloat();
				break;
			case 45u:
				ToX = ((ParseContext)(ref input)).ReadFloat();
				break;
			case 53u:
				ToY = ((ParseContext)(ref input)).ReadFloat();
				break;
			case 56u:
				StartTm = ((ParseContext)(ref input)).ReadInt64();
				break;
			case 64u:
				ArrivalTm = ((ParseContext)(ref input)).ReadInt64();
				break;
			}
		}
	}
}
