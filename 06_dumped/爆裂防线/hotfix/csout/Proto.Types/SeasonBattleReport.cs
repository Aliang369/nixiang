using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class SeasonBattleReport : IMessage<SeasonBattleReport>, IMessage, IEquatable<SeasonBattleReport>, IDeepCloneable<SeasonBattleReport>, IBufferMessage
{
	private static readonly MessageParser<SeasonBattleReport> _parser = new MessageParser<SeasonBattleReport>((Func<SeasonBattleReport>)(() => new SeasonBattleReport()));

	private UnknownFieldSet _unknownFields;

	public const int UuidFieldNumber = 1;

	private string uuid_ = "";

	public const int AtkRoleIdFieldNumber = 2;

	private long atkRoleId_;

	public const int DefRoleIdFieldNumber = 3;

	private long defRoleId_;

	public const int CityIdFieldNumber = 4;

	private int cityId_;

	public const int WinnerFieldNumber = 5;

	private int winner_;

	public const int BattleTypeFieldNumber = 6;

	private ESeasonBattleType battleType_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<SeasonBattleReport> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeSeasonBattleReportReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public string Uuid
	{
		get
		{
			return uuid_;
		}
		set
		{
			uuid_ = ProtoPreconditions.CheckNotNull<string>(value, "value");
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public long AtkRoleId
	{
		get
		{
			return atkRoleId_;
		}
		set
		{
			atkRoleId_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public long DefRoleId
	{
		get
		{
			return defRoleId_;
		}
		set
		{
			defRoleId_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int CityId
	{
		get
		{
			return cityId_;
		}
		set
		{
			cityId_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int Winner
	{
		get
		{
			return winner_;
		}
		set
		{
			winner_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ESeasonBattleType BattleType
	{
		get
		{
			return battleType_;
		}
		set
		{
			battleType_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SeasonBattleReport()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SeasonBattleReport(SeasonBattleReport other)
		: this()
	{
		uuid_ = other.uuid_;
		atkRoleId_ = other.atkRoleId_;
		defRoleId_ = other.defRoleId_;
		cityId_ = other.cityId_;
		winner_ = other.winner_;
		battleType_ = other.battleType_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SeasonBattleReport Clone()
	{
		return new SeasonBattleReport(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as SeasonBattleReport);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(SeasonBattleReport other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (Uuid != other.Uuid)
		{
			return false;
		}
		if (AtkRoleId != other.AtkRoleId)
		{
			return false;
		}
		if (DefRoleId != other.DefRoleId)
		{
			return false;
		}
		if (CityId != other.CityId)
		{
			return false;
		}
		if (Winner != other.Winner)
		{
			return false;
		}
		if (BattleType != other.BattleType)
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
		if (Uuid.Length != 0)
		{
			num ^= Uuid.GetHashCode();
		}
		if (AtkRoleId != 0L)
		{
			num ^= AtkRoleId.GetHashCode();
		}
		if (DefRoleId != 0L)
		{
			num ^= DefRoleId.GetHashCode();
		}
		if (CityId != 0)
		{
			num ^= CityId.GetHashCode();
		}
		if (Winner != 0)
		{
			num ^= Winner.GetHashCode();
		}
		if (BattleType != ESeasonBattleType.EmptyValue)
		{
			num ^= BattleType.GetHashCode();
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
		if (Uuid.Length != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)10);
			((WriteContext)(ref output)).WriteString(Uuid);
		}
		if (AtkRoleId != 0L)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteInt64(AtkRoleId);
		}
		if (DefRoleId != 0L)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)24);
			((WriteContext)(ref output)).WriteInt64(DefRoleId);
		}
		if (CityId != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)32);
			((WriteContext)(ref output)).WriteInt32(CityId);
		}
		if (Winner != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)40);
			((WriteContext)(ref output)).WriteInt32(Winner);
		}
		if (BattleType != ESeasonBattleType.EmptyValue)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)48);
			((WriteContext)(ref output)).WriteEnum((int)BattleType);
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
		if (Uuid.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(Uuid);
		}
		if (AtkRoleId != 0L)
		{
			num += 1 + CodedOutputStream.ComputeInt64Size(AtkRoleId);
		}
		if (DefRoleId != 0L)
		{
			num += 1 + CodedOutputStream.ComputeInt64Size(DefRoleId);
		}
		if (CityId != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(CityId);
		}
		if (Winner != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(Winner);
		}
		if (BattleType != ESeasonBattleType.EmptyValue)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)BattleType);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(SeasonBattleReport other)
	{
		if (other != null)
		{
			if (other.Uuid.Length != 0)
			{
				Uuid = other.Uuid;
			}
			if (other.AtkRoleId != 0L)
			{
				AtkRoleId = other.AtkRoleId;
			}
			if (other.DefRoleId != 0L)
			{
				DefRoleId = other.DefRoleId;
			}
			if (other.CityId != 0)
			{
				CityId = other.CityId;
			}
			if (other.Winner != 0)
			{
				Winner = other.Winner;
			}
			if (other.BattleType != ESeasonBattleType.EmptyValue)
			{
				BattleType = other.BattleType;
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
			case 10u:
				Uuid = ((ParseContext)(ref input)).ReadString();
				break;
			case 16u:
				AtkRoleId = ((ParseContext)(ref input)).ReadInt64();
				break;
			case 24u:
				DefRoleId = ((ParseContext)(ref input)).ReadInt64();
				break;
			case 32u:
				CityId = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 40u:
				Winner = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 48u:
				BattleType = (ESeasonBattleType)((ParseContext)(ref input)).ReadEnum();
				break;
			}
		}
	}
}
