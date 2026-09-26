using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class SeasonEnterAck : IMessage<SeasonEnterAck>, IMessage, IEquatable<SeasonEnterAck>, IDeepCloneable<SeasonEnterAck>, IBufferMessage
{
	private static readonly MessageParser<SeasonEnterAck> _parser = new MessageParser<SeasonEnterAck>((Func<SeasonEnterAck>)(() => new SeasonEnterAck()));

	private UnknownFieldSet _unknownFields;

	public const int SeasonIdFieldNumber = 1;

	private int seasonId_;

	public const int PhaseFieldNumber = 2;

	private ESeasonPhase phase_;

	public const int StartTmFieldNumber = 3;

	private long startTm_;

	public const int EndTmFieldNumber = 4;

	private long endTm_;

	public const int ForceIdFieldNumber = 5;

	private ESeasonForce forceId_;

	public const int ForceNameFieldNumber = 6;

	private string forceName_ = "";

	public const int ForceFlagFieldNumber = 7;

	private string forceFlag_ = "";

	public const int ForceNoticeFieldNumber = 8;

	private string forceNotice_ = "";

	public const int MeritFieldNumber = 9;

	private int merit_;

	public const int DurabilityFieldNumber = 10;

	private int durability_;

	public const int DefDurabilityFieldNumber = 11;

	private int defDurability_;

	public const int GasFieldNumber = 12;

	private int gas_;

	public const int TechPointFieldNumber = 13;

	private int techPoint_;

	public const int NextSeasonTmFieldNumber = 14;

	private long nextSeasonTm_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<SeasonEnterAck> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeSeasonEnterAckReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int SeasonId
	{
		get
		{
			return seasonId_;
		}
		set
		{
			seasonId_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ESeasonPhase Phase
	{
		get
		{
			return phase_;
		}
		set
		{
			phase_ = value;
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
	public long EndTm
	{
		get
		{
			return endTm_;
		}
		set
		{
			endTm_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ESeasonForce ForceId
	{
		get
		{
			return forceId_;
		}
		set
		{
			forceId_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public string ForceName
	{
		get
		{
			return forceName_;
		}
		set
		{
			forceName_ = ProtoPreconditions.CheckNotNull<string>(value, "value");
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public string ForceFlag
	{
		get
		{
			return forceFlag_;
		}
		set
		{
			forceFlag_ = ProtoPreconditions.CheckNotNull<string>(value, "value");
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public string ForceNotice
	{
		get
		{
			return forceNotice_;
		}
		set
		{
			forceNotice_ = ProtoPreconditions.CheckNotNull<string>(value, "value");
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int Merit
	{
		get
		{
			return merit_;
		}
		set
		{
			merit_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int Durability
	{
		get
		{
			return durability_;
		}
		set
		{
			durability_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int DefDurability
	{
		get
		{
			return defDurability_;
		}
		set
		{
			defDurability_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int Gas
	{
		get
		{
			return gas_;
		}
		set
		{
			gas_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int TechPoint
	{
		get
		{
			return techPoint_;
		}
		set
		{
			techPoint_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public long NextSeasonTm
	{
		get
		{
			return nextSeasonTm_;
		}
		set
		{
			nextSeasonTm_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SeasonEnterAck()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SeasonEnterAck(SeasonEnterAck other)
		: this()
	{
		seasonId_ = other.seasonId_;
		phase_ = other.phase_;
		startTm_ = other.startTm_;
		endTm_ = other.endTm_;
		forceId_ = other.forceId_;
		forceName_ = other.forceName_;
		forceFlag_ = other.forceFlag_;
		forceNotice_ = other.forceNotice_;
		merit_ = other.merit_;
		durability_ = other.durability_;
		defDurability_ = other.defDurability_;
		gas_ = other.gas_;
		techPoint_ = other.techPoint_;
		nextSeasonTm_ = other.nextSeasonTm_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SeasonEnterAck Clone()
	{
		return new SeasonEnterAck(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as SeasonEnterAck);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(SeasonEnterAck other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (SeasonId != other.SeasonId)
		{
			return false;
		}
		if (Phase != other.Phase)
		{
			return false;
		}
		if (StartTm != other.StartTm)
		{
			return false;
		}
		if (EndTm != other.EndTm)
		{
			return false;
		}
		if (ForceId != other.ForceId)
		{
			return false;
		}
		if (ForceName != other.ForceName)
		{
			return false;
		}
		if (ForceFlag != other.ForceFlag)
		{
			return false;
		}
		if (ForceNotice != other.ForceNotice)
		{
			return false;
		}
		if (Merit != other.Merit)
		{
			return false;
		}
		if (Durability != other.Durability)
		{
			return false;
		}
		if (DefDurability != other.DefDurability)
		{
			return false;
		}
		if (Gas != other.Gas)
		{
			return false;
		}
		if (TechPoint != other.TechPoint)
		{
			return false;
		}
		if (NextSeasonTm != other.NextSeasonTm)
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
		if (SeasonId != 0)
		{
			num ^= SeasonId.GetHashCode();
		}
		if (Phase != ESeasonPhase.EmptyValue)
		{
			num ^= Phase.GetHashCode();
		}
		if (StartTm != 0L)
		{
			num ^= StartTm.GetHashCode();
		}
		if (EndTm != 0L)
		{
			num ^= EndTm.GetHashCode();
		}
		if (ForceId != ESeasonForce.ForceNeutral)
		{
			num ^= ForceId.GetHashCode();
		}
		if (ForceName.Length != 0)
		{
			num ^= ForceName.GetHashCode();
		}
		if (ForceFlag.Length != 0)
		{
			num ^= ForceFlag.GetHashCode();
		}
		if (ForceNotice.Length != 0)
		{
			num ^= ForceNotice.GetHashCode();
		}
		if (Merit != 0)
		{
			num ^= Merit.GetHashCode();
		}
		if (Durability != 0)
		{
			num ^= Durability.GetHashCode();
		}
		if (DefDurability != 0)
		{
			num ^= DefDurability.GetHashCode();
		}
		if (Gas != 0)
		{
			num ^= Gas.GetHashCode();
		}
		if (TechPoint != 0)
		{
			num ^= TechPoint.GetHashCode();
		}
		if (NextSeasonTm != 0L)
		{
			num ^= NextSeasonTm.GetHashCode();
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
		if (SeasonId != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteInt32(SeasonId);
		}
		if (Phase != ESeasonPhase.EmptyValue)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteEnum((int)Phase);
		}
		if (StartTm != 0L)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)24);
			((WriteContext)(ref output)).WriteInt64(StartTm);
		}
		if (EndTm != 0L)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)32);
			((WriteContext)(ref output)).WriteInt64(EndTm);
		}
		if (ForceId != ESeasonForce.ForceNeutral)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)40);
			((WriteContext)(ref output)).WriteEnum((int)ForceId);
		}
		if (ForceName.Length != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)50);
			((WriteContext)(ref output)).WriteString(ForceName);
		}
		if (ForceFlag.Length != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)58);
			((WriteContext)(ref output)).WriteString(ForceFlag);
		}
		if (ForceNotice.Length != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)66);
			((WriteContext)(ref output)).WriteString(ForceNotice);
		}
		if (Merit != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)72);
			((WriteContext)(ref output)).WriteInt32(Merit);
		}
		if (Durability != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)80);
			((WriteContext)(ref output)).WriteInt32(Durability);
		}
		if (DefDurability != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)88);
			((WriteContext)(ref output)).WriteInt32(DefDurability);
		}
		if (Gas != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)96);
			((WriteContext)(ref output)).WriteInt32(Gas);
		}
		if (TechPoint != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)104);
			((WriteContext)(ref output)).WriteInt32(TechPoint);
		}
		if (NextSeasonTm != 0L)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)112);
			((WriteContext)(ref output)).WriteInt64(NextSeasonTm);
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
		if (SeasonId != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(SeasonId);
		}
		if (Phase != ESeasonPhase.EmptyValue)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)Phase);
		}
		if (StartTm != 0L)
		{
			num += 1 + CodedOutputStream.ComputeInt64Size(StartTm);
		}
		if (EndTm != 0L)
		{
			num += 1 + CodedOutputStream.ComputeInt64Size(EndTm);
		}
		if (ForceId != ESeasonForce.ForceNeutral)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)ForceId);
		}
		if (ForceName.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(ForceName);
		}
		if (ForceFlag.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(ForceFlag);
		}
		if (ForceNotice.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(ForceNotice);
		}
		if (Merit != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(Merit);
		}
		if (Durability != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(Durability);
		}
		if (DefDurability != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(DefDurability);
		}
		if (Gas != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(Gas);
		}
		if (TechPoint != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(TechPoint);
		}
		if (NextSeasonTm != 0L)
		{
			num += 1 + CodedOutputStream.ComputeInt64Size(NextSeasonTm);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(SeasonEnterAck other)
	{
		if (other != null)
		{
			if (other.SeasonId != 0)
			{
				SeasonId = other.SeasonId;
			}
			if (other.Phase != ESeasonPhase.EmptyValue)
			{
				Phase = other.Phase;
			}
			if (other.StartTm != 0L)
			{
				StartTm = other.StartTm;
			}
			if (other.EndTm != 0L)
			{
				EndTm = other.EndTm;
			}
			if (other.ForceId != ESeasonForce.ForceNeutral)
			{
				ForceId = other.ForceId;
			}
			if (other.ForceName.Length != 0)
			{
				ForceName = other.ForceName;
			}
			if (other.ForceFlag.Length != 0)
			{
				ForceFlag = other.ForceFlag;
			}
			if (other.ForceNotice.Length != 0)
			{
				ForceNotice = other.ForceNotice;
			}
			if (other.Merit != 0)
			{
				Merit = other.Merit;
			}
			if (other.Durability != 0)
			{
				Durability = other.Durability;
			}
			if (other.DefDurability != 0)
			{
				DefDurability = other.DefDurability;
			}
			if (other.Gas != 0)
			{
				Gas = other.Gas;
			}
			if (other.TechPoint != 0)
			{
				TechPoint = other.TechPoint;
			}
			if (other.NextSeasonTm != 0L)
			{
				NextSeasonTm = other.NextSeasonTm;
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
				SeasonId = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 16u:
				Phase = (ESeasonPhase)((ParseContext)(ref input)).ReadEnum();
				break;
			case 24u:
				StartTm = ((ParseContext)(ref input)).ReadInt64();
				break;
			case 32u:
				EndTm = ((ParseContext)(ref input)).ReadInt64();
				break;
			case 40u:
				ForceId = (ESeasonForce)((ParseContext)(ref input)).ReadEnum();
				break;
			case 50u:
				ForceName = ((ParseContext)(ref input)).ReadString();
				break;
			case 58u:
				ForceFlag = ((ParseContext)(ref input)).ReadString();
				break;
			case 66u:
				ForceNotice = ((ParseContext)(ref input)).ReadString();
				break;
			case 72u:
				Merit = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 80u:
				Durability = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 88u:
				DefDurability = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 96u:
				Gas = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 104u:
				TechPoint = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 112u:
				NextSeasonTm = ((ParseContext)(ref input)).ReadInt64();
				break;
			}
		}
	}
}
