using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class RechargeActiveInfo : IMessage<RechargeActiveInfo>, IMessage, IEquatable<RechargeActiveInfo>, IDeepCloneable<RechargeActiveInfo>, IBufferMessage
{
	private static readonly MessageParser<RechargeActiveInfo> _parser = new MessageParser<RechargeActiveInfo>((Func<RechargeActiveInfo>)(() => new RechargeActiveInfo()));

	private UnknownFieldSet _unknownFields;

	public const int NovicePackInfoFieldNumber = 1;

	private NovicePackInfo novicePackInfo_;

	public const int DailyPackInfoFieldNumber = 2;

	private DailyPackInfo dailyPackInfo_;

	public const int ChargebackInfoFieldNumber = 3;

	private ChargebackInfo chargebackInfo_;

	public const int LevelDiamondInfoFieldNumber = 4;

	private ZhanLingCommonInfo levelDiamondInfo_;

	public const int PassportInfoFieldNumber = 5;

	private PassportInfo passportInfo_;

	public const int NovicePackExtraListFieldNumber = 6;

	private static readonly FieldCodec<NovicePackInfo> _repeated_novicePackExtraList_codec = FieldCodec.ForMessage<NovicePackInfo>(50u, NovicePackInfo.Parser);

	private readonly RepeatedField<NovicePackInfo> novicePackExtraList_ = new RepeatedField<NovicePackInfo>();

	public const int ZhanLingMainInfoFieldNumber = 7;

	private ZhanLingMainInfo zhanLingMainInfo_;

	public const int ZhanLingElementInfoFieldNumber = 8;

	private ZhanLingCommonInfo zhanLingElementInfo_;

	public const int ZhanLingEndlessInfoFieldNumber = 9;

	private ZhanLingCommonInfo zhanLingEndlessInfo_;

	public const int ZhanLingLegendaryInfoFieldNumber = 10;

	private ZhanLingMainInfo zhanLingLegendaryInfo_;

	public const int TreasurePrivilegeInfoFieldNumber = 11;

	private TreasurePrivilegeInfo treasurePrivilegeInfo_;

	public const int ResourceCardDoubleInfoFieldNumber = 12;

	private ResourceCardDoubleInfo resourceCardDoubleInfo_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<RechargeActiveInfo> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeRechargeActiveInfoReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public NovicePackInfo NovicePackInfo
	{
		get
		{
			return novicePackInfo_;
		}
		set
		{
			novicePackInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public DailyPackInfo DailyPackInfo
	{
		get
		{
			return dailyPackInfo_;
		}
		set
		{
			dailyPackInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ChargebackInfo ChargebackInfo
	{
		get
		{
			return chargebackInfo_;
		}
		set
		{
			chargebackInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ZhanLingCommonInfo LevelDiamondInfo
	{
		get
		{
			return levelDiamondInfo_;
		}
		set
		{
			levelDiamondInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public PassportInfo PassportInfo
	{
		get
		{
			return passportInfo_;
		}
		set
		{
			passportInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<NovicePackInfo> NovicePackExtraList => novicePackExtraList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ZhanLingMainInfo ZhanLingMainInfo
	{
		get
		{
			return zhanLingMainInfo_;
		}
		set
		{
			zhanLingMainInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ZhanLingCommonInfo ZhanLingElementInfo
	{
		get
		{
			return zhanLingElementInfo_;
		}
		set
		{
			zhanLingElementInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ZhanLingCommonInfo ZhanLingEndlessInfo
	{
		get
		{
			return zhanLingEndlessInfo_;
		}
		set
		{
			zhanLingEndlessInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ZhanLingMainInfo ZhanLingLegendaryInfo
	{
		get
		{
			return zhanLingLegendaryInfo_;
		}
		set
		{
			zhanLingLegendaryInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public TreasurePrivilegeInfo TreasurePrivilegeInfo
	{
		get
		{
			return treasurePrivilegeInfo_;
		}
		set
		{
			treasurePrivilegeInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ResourceCardDoubleInfo ResourceCardDoubleInfo
	{
		get
		{
			return resourceCardDoubleInfo_;
		}
		set
		{
			resourceCardDoubleInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RechargeActiveInfo()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RechargeActiveInfo(RechargeActiveInfo other)
		: this()
	{
		novicePackInfo_ = ((other.novicePackInfo_ != null) ? other.novicePackInfo_.Clone() : null);
		dailyPackInfo_ = ((other.dailyPackInfo_ != null) ? other.dailyPackInfo_.Clone() : null);
		chargebackInfo_ = ((other.chargebackInfo_ != null) ? other.chargebackInfo_.Clone() : null);
		levelDiamondInfo_ = ((other.levelDiamondInfo_ != null) ? other.levelDiamondInfo_.Clone() : null);
		passportInfo_ = ((other.passportInfo_ != null) ? other.passportInfo_.Clone() : null);
		novicePackExtraList_ = other.novicePackExtraList_.Clone();
		zhanLingMainInfo_ = ((other.zhanLingMainInfo_ != null) ? other.zhanLingMainInfo_.Clone() : null);
		zhanLingElementInfo_ = ((other.zhanLingElementInfo_ != null) ? other.zhanLingElementInfo_.Clone() : null);
		zhanLingEndlessInfo_ = ((other.zhanLingEndlessInfo_ != null) ? other.zhanLingEndlessInfo_.Clone() : null);
		zhanLingLegendaryInfo_ = ((other.zhanLingLegendaryInfo_ != null) ? other.zhanLingLegendaryInfo_.Clone() : null);
		treasurePrivilegeInfo_ = ((other.treasurePrivilegeInfo_ != null) ? other.treasurePrivilegeInfo_.Clone() : null);
		resourceCardDoubleInfo_ = ((other.resourceCardDoubleInfo_ != null) ? other.resourceCardDoubleInfo_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RechargeActiveInfo Clone()
	{
		return new RechargeActiveInfo(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as RechargeActiveInfo);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(RechargeActiveInfo other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!object.Equals(NovicePackInfo, other.NovicePackInfo))
		{
			return false;
		}
		if (!object.Equals(DailyPackInfo, other.DailyPackInfo))
		{
			return false;
		}
		if (!object.Equals(ChargebackInfo, other.ChargebackInfo))
		{
			return false;
		}
		if (!object.Equals(LevelDiamondInfo, other.LevelDiamondInfo))
		{
			return false;
		}
		if (!object.Equals(PassportInfo, other.PassportInfo))
		{
			return false;
		}
		if (!novicePackExtraList_.Equals(other.novicePackExtraList_))
		{
			return false;
		}
		if (!object.Equals(ZhanLingMainInfo, other.ZhanLingMainInfo))
		{
			return false;
		}
		if (!object.Equals(ZhanLingElementInfo, other.ZhanLingElementInfo))
		{
			return false;
		}
		if (!object.Equals(ZhanLingEndlessInfo, other.ZhanLingEndlessInfo))
		{
			return false;
		}
		if (!object.Equals(ZhanLingLegendaryInfo, other.ZhanLingLegendaryInfo))
		{
			return false;
		}
		if (!object.Equals(TreasurePrivilegeInfo, other.TreasurePrivilegeInfo))
		{
			return false;
		}
		if (!object.Equals(ResourceCardDoubleInfo, other.ResourceCardDoubleInfo))
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
		if (novicePackInfo_ != null)
		{
			num ^= NovicePackInfo.GetHashCode();
		}
		if (dailyPackInfo_ != null)
		{
			num ^= DailyPackInfo.GetHashCode();
		}
		if (chargebackInfo_ != null)
		{
			num ^= ChargebackInfo.GetHashCode();
		}
		if (levelDiamondInfo_ != null)
		{
			num ^= LevelDiamondInfo.GetHashCode();
		}
		if (passportInfo_ != null)
		{
			num ^= PassportInfo.GetHashCode();
		}
		num ^= ((object)novicePackExtraList_).GetHashCode();
		if (zhanLingMainInfo_ != null)
		{
			num ^= ZhanLingMainInfo.GetHashCode();
		}
		if (zhanLingElementInfo_ != null)
		{
			num ^= ZhanLingElementInfo.GetHashCode();
		}
		if (zhanLingEndlessInfo_ != null)
		{
			num ^= ZhanLingEndlessInfo.GetHashCode();
		}
		if (zhanLingLegendaryInfo_ != null)
		{
			num ^= ZhanLingLegendaryInfo.GetHashCode();
		}
		if (treasurePrivilegeInfo_ != null)
		{
			num ^= TreasurePrivilegeInfo.GetHashCode();
		}
		if (resourceCardDoubleInfo_ != null)
		{
			num ^= ResourceCardDoubleInfo.GetHashCode();
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
		if (novicePackInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)10);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)NovicePackInfo);
		}
		if (dailyPackInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)18);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)DailyPackInfo);
		}
		if (chargebackInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)26);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)ChargebackInfo);
		}
		if (levelDiamondInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)34);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)LevelDiamondInfo);
		}
		if (passportInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)42);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)PassportInfo);
		}
		novicePackExtraList_.WriteTo(ref output, _repeated_novicePackExtraList_codec);
		if (zhanLingMainInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)58);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)ZhanLingMainInfo);
		}
		if (zhanLingElementInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)66);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)ZhanLingElementInfo);
		}
		if (zhanLingEndlessInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)74);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)ZhanLingEndlessInfo);
		}
		if (zhanLingLegendaryInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)82);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)ZhanLingLegendaryInfo);
		}
		if (treasurePrivilegeInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)90);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)TreasurePrivilegeInfo);
		}
		if (resourceCardDoubleInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)98);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)ResourceCardDoubleInfo);
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
		if (novicePackInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)NovicePackInfo);
		}
		if (dailyPackInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)DailyPackInfo);
		}
		if (chargebackInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)ChargebackInfo);
		}
		if (levelDiamondInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)LevelDiamondInfo);
		}
		if (passportInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)PassportInfo);
		}
		num += novicePackExtraList_.CalculateSize(_repeated_novicePackExtraList_codec);
		if (zhanLingMainInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)ZhanLingMainInfo);
		}
		if (zhanLingElementInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)ZhanLingElementInfo);
		}
		if (zhanLingEndlessInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)ZhanLingEndlessInfo);
		}
		if (zhanLingLegendaryInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)ZhanLingLegendaryInfo);
		}
		if (treasurePrivilegeInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)TreasurePrivilegeInfo);
		}
		if (resourceCardDoubleInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)ResourceCardDoubleInfo);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(RechargeActiveInfo other)
	{
		if (other == null)
		{
			return;
		}
		if (other.novicePackInfo_ != null)
		{
			if (novicePackInfo_ == null)
			{
				NovicePackInfo = new NovicePackInfo();
			}
			NovicePackInfo.MergeFrom(other.NovicePackInfo);
		}
		if (other.dailyPackInfo_ != null)
		{
			if (dailyPackInfo_ == null)
			{
				DailyPackInfo = new DailyPackInfo();
			}
			DailyPackInfo.MergeFrom(other.DailyPackInfo);
		}
		if (other.chargebackInfo_ != null)
		{
			if (chargebackInfo_ == null)
			{
				ChargebackInfo = new ChargebackInfo();
			}
			ChargebackInfo.MergeFrom(other.ChargebackInfo);
		}
		if (other.levelDiamondInfo_ != null)
		{
			if (levelDiamondInfo_ == null)
			{
				LevelDiamondInfo = new ZhanLingCommonInfo();
			}
			LevelDiamondInfo.MergeFrom(other.LevelDiamondInfo);
		}
		if (other.passportInfo_ != null)
		{
			if (passportInfo_ == null)
			{
				PassportInfo = new PassportInfo();
			}
			PassportInfo.MergeFrom(other.PassportInfo);
		}
		novicePackExtraList_.Add((IEnumerable<NovicePackInfo>)other.novicePackExtraList_);
		if (other.zhanLingMainInfo_ != null)
		{
			if (zhanLingMainInfo_ == null)
			{
				ZhanLingMainInfo = new ZhanLingMainInfo();
			}
			ZhanLingMainInfo.MergeFrom(other.ZhanLingMainInfo);
		}
		if (other.zhanLingElementInfo_ != null)
		{
			if (zhanLingElementInfo_ == null)
			{
				ZhanLingElementInfo = new ZhanLingCommonInfo();
			}
			ZhanLingElementInfo.MergeFrom(other.ZhanLingElementInfo);
		}
		if (other.zhanLingEndlessInfo_ != null)
		{
			if (zhanLingEndlessInfo_ == null)
			{
				ZhanLingEndlessInfo = new ZhanLingCommonInfo();
			}
			ZhanLingEndlessInfo.MergeFrom(other.ZhanLingEndlessInfo);
		}
		if (other.zhanLingLegendaryInfo_ != null)
		{
			if (zhanLingLegendaryInfo_ == null)
			{
				ZhanLingLegendaryInfo = new ZhanLingMainInfo();
			}
			ZhanLingLegendaryInfo.MergeFrom(other.ZhanLingLegendaryInfo);
		}
		if (other.treasurePrivilegeInfo_ != null)
		{
			if (treasurePrivilegeInfo_ == null)
			{
				TreasurePrivilegeInfo = new TreasurePrivilegeInfo();
			}
			TreasurePrivilegeInfo.MergeFrom(other.TreasurePrivilegeInfo);
		}
		if (other.resourceCardDoubleInfo_ != null)
		{
			if (resourceCardDoubleInfo_ == null)
			{
				ResourceCardDoubleInfo = new ResourceCardDoubleInfo();
			}
			ResourceCardDoubleInfo.MergeFrom(other.ResourceCardDoubleInfo);
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
				if (novicePackInfo_ == null)
				{
					NovicePackInfo = new NovicePackInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)NovicePackInfo);
				break;
			case 18u:
				if (dailyPackInfo_ == null)
				{
					DailyPackInfo = new DailyPackInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)DailyPackInfo);
				break;
			case 26u:
				if (chargebackInfo_ == null)
				{
					ChargebackInfo = new ChargebackInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)ChargebackInfo);
				break;
			case 34u:
				if (levelDiamondInfo_ == null)
				{
					LevelDiamondInfo = new ZhanLingCommonInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)LevelDiamondInfo);
				break;
			case 42u:
				if (passportInfo_ == null)
				{
					PassportInfo = new PassportInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)PassportInfo);
				break;
			case 50u:
				novicePackExtraList_.AddEntriesFrom(ref input, _repeated_novicePackExtraList_codec);
				break;
			case 58u:
				if (zhanLingMainInfo_ == null)
				{
					ZhanLingMainInfo = new ZhanLingMainInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)ZhanLingMainInfo);
				break;
			case 66u:
				if (zhanLingElementInfo_ == null)
				{
					ZhanLingElementInfo = new ZhanLingCommonInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)ZhanLingElementInfo);
				break;
			case 74u:
				if (zhanLingEndlessInfo_ == null)
				{
					ZhanLingEndlessInfo = new ZhanLingCommonInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)ZhanLingEndlessInfo);
				break;
			case 82u:
				if (zhanLingLegendaryInfo_ == null)
				{
					ZhanLingLegendaryInfo = new ZhanLingMainInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)ZhanLingLegendaryInfo);
				break;
			case 90u:
				if (treasurePrivilegeInfo_ == null)
				{
					TreasurePrivilegeInfo = new TreasurePrivilegeInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)TreasurePrivilegeInfo);
				break;
			case 98u:
				if (resourceCardDoubleInfo_ == null)
				{
					ResourceCardDoubleInfo = new ResourceCardDoubleInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)ResourceCardDoubleInfo);
				break;
			}
		}
	}
}
