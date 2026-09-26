using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class FacilityBaseInfo : IMessage<FacilityBaseInfo>, IMessage, IEquatable<FacilityBaseInfo>, IDeepCloneable<FacilityBaseInfo>, IBufferMessage
{
	private static readonly MessageParser<FacilityBaseInfo> _parser = new MessageParser<FacilityBaseInfo>((Func<FacilityBaseInfo>)(() => new FacilityBaseInfo()));

	private UnknownFieldSet _unknownFields;

	public const int FacilityIdFieldNumber = 1;

	private int facilityId_;

	public const int FacilityTypeFieldNumber = 2;

	private int facilityType_;

	public const int FacilityQualityFieldNumber = 3;

	private EQuality facilityQuality_;

	public const int FacilityMapFieldNumber = 4;

	private int facilityMap_;

	public const int FacilityCreatorFieldNumber = 5;

	private RoleBaseInfo facilityCreator_;

	public const int ExploreShareFieldNumber = 6;

	private int exploreShare_;

	public const int ExploreTotalFieldNumber = 7;

	private int exploreTotal_;

	public const int ExpireTmFieldNumber = 8;

	private long expireTm_;

	public const int DoubleAwardFieldNumber = 9;

	private EBoolState doubleAward_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<FacilityBaseInfo> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeFacilityBaseInfoReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int FacilityId
	{
		get
		{
			return facilityId_;
		}
		set
		{
			facilityId_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int FacilityType
	{
		get
		{
			return facilityType_;
		}
		set
		{
			facilityType_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public EQuality FacilityQuality
	{
		get
		{
			return facilityQuality_;
		}
		set
		{
			facilityQuality_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int FacilityMap
	{
		get
		{
			return facilityMap_;
		}
		set
		{
			facilityMap_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RoleBaseInfo FacilityCreator
	{
		get
		{
			return facilityCreator_;
		}
		set
		{
			facilityCreator_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int ExploreShare
	{
		get
		{
			return exploreShare_;
		}
		set
		{
			exploreShare_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int ExploreTotal
	{
		get
		{
			return exploreTotal_;
		}
		set
		{
			exploreTotal_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public long ExpireTm
	{
		get
		{
			return expireTm_;
		}
		set
		{
			expireTm_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public EBoolState DoubleAward
	{
		get
		{
			return doubleAward_;
		}
		set
		{
			doubleAward_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public FacilityBaseInfo()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public FacilityBaseInfo(FacilityBaseInfo other)
		: this()
	{
		facilityId_ = other.facilityId_;
		facilityType_ = other.facilityType_;
		facilityQuality_ = other.facilityQuality_;
		facilityMap_ = other.facilityMap_;
		facilityCreator_ = ((other.facilityCreator_ != null) ? other.facilityCreator_.Clone() : null);
		exploreShare_ = other.exploreShare_;
		exploreTotal_ = other.exploreTotal_;
		expireTm_ = other.expireTm_;
		doubleAward_ = other.doubleAward_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public FacilityBaseInfo Clone()
	{
		return new FacilityBaseInfo(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as FacilityBaseInfo);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(FacilityBaseInfo other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (FacilityId != other.FacilityId)
		{
			return false;
		}
		if (FacilityType != other.FacilityType)
		{
			return false;
		}
		if (FacilityQuality != other.FacilityQuality)
		{
			return false;
		}
		if (FacilityMap != other.FacilityMap)
		{
			return false;
		}
		if (!object.Equals(FacilityCreator, other.FacilityCreator))
		{
			return false;
		}
		if (ExploreShare != other.ExploreShare)
		{
			return false;
		}
		if (ExploreTotal != other.ExploreTotal)
		{
			return false;
		}
		if (ExpireTm != other.ExpireTm)
		{
			return false;
		}
		if (DoubleAward != other.DoubleAward)
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
		if (FacilityId != 0)
		{
			num ^= FacilityId.GetHashCode();
		}
		if (FacilityType != 0)
		{
			num ^= FacilityType.GetHashCode();
		}
		if (FacilityQuality != EQuality.EmptyValue)
		{
			num ^= FacilityQuality.GetHashCode();
		}
		if (FacilityMap != 0)
		{
			num ^= FacilityMap.GetHashCode();
		}
		if (facilityCreator_ != null)
		{
			num ^= FacilityCreator.GetHashCode();
		}
		if (ExploreShare != 0)
		{
			num ^= ExploreShare.GetHashCode();
		}
		if (ExploreTotal != 0)
		{
			num ^= ExploreTotal.GetHashCode();
		}
		if (ExpireTm != 0L)
		{
			num ^= ExpireTm.GetHashCode();
		}
		if (DoubleAward != EBoolState.False)
		{
			num ^= DoubleAward.GetHashCode();
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
		if (FacilityId != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteInt32(FacilityId);
		}
		if (FacilityType != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteInt32(FacilityType);
		}
		if (FacilityQuality != EQuality.EmptyValue)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)24);
			((WriteContext)(ref output)).WriteEnum((int)FacilityQuality);
		}
		if (FacilityMap != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)32);
			((WriteContext)(ref output)).WriteInt32(FacilityMap);
		}
		if (facilityCreator_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)42);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)FacilityCreator);
		}
		if (ExploreShare != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)48);
			((WriteContext)(ref output)).WriteInt32(ExploreShare);
		}
		if (ExploreTotal != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)56);
			((WriteContext)(ref output)).WriteInt32(ExploreTotal);
		}
		if (ExpireTm != 0L)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)64);
			((WriteContext)(ref output)).WriteInt64(ExpireTm);
		}
		if (DoubleAward != EBoolState.False)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)72);
			((WriteContext)(ref output)).WriteEnum((int)DoubleAward);
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
		if (FacilityId != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(FacilityId);
		}
		if (FacilityType != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(FacilityType);
		}
		if (FacilityQuality != EQuality.EmptyValue)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)FacilityQuality);
		}
		if (FacilityMap != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(FacilityMap);
		}
		if (facilityCreator_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)FacilityCreator);
		}
		if (ExploreShare != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(ExploreShare);
		}
		if (ExploreTotal != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(ExploreTotal);
		}
		if (ExpireTm != 0L)
		{
			num += 1 + CodedOutputStream.ComputeInt64Size(ExpireTm);
		}
		if (DoubleAward != EBoolState.False)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)DoubleAward);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(FacilityBaseInfo other)
	{
		if (other == null)
		{
			return;
		}
		if (other.FacilityId != 0)
		{
			FacilityId = other.FacilityId;
		}
		if (other.FacilityType != 0)
		{
			FacilityType = other.FacilityType;
		}
		if (other.FacilityQuality != EQuality.EmptyValue)
		{
			FacilityQuality = other.FacilityQuality;
		}
		if (other.FacilityMap != 0)
		{
			FacilityMap = other.FacilityMap;
		}
		if (other.facilityCreator_ != null)
		{
			if (facilityCreator_ == null)
			{
				FacilityCreator = new RoleBaseInfo();
			}
			FacilityCreator.MergeFrom(other.FacilityCreator);
		}
		if (other.ExploreShare != 0)
		{
			ExploreShare = other.ExploreShare;
		}
		if (other.ExploreTotal != 0)
		{
			ExploreTotal = other.ExploreTotal;
		}
		if (other.ExpireTm != 0L)
		{
			ExpireTm = other.ExpireTm;
		}
		if (other.DoubleAward != EBoolState.False)
		{
			DoubleAward = other.DoubleAward;
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
			case 8u:
				FacilityId = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 16u:
				FacilityType = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 24u:
				FacilityQuality = (EQuality)((ParseContext)(ref input)).ReadEnum();
				break;
			case 32u:
				FacilityMap = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 42u:
				if (facilityCreator_ == null)
				{
					FacilityCreator = new RoleBaseInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)FacilityCreator);
				break;
			case 48u:
				ExploreShare = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 56u:
				ExploreTotal = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 64u:
				ExpireTm = ((ParseContext)(ref input)).ReadInt64();
				break;
			case 72u:
				DoubleAward = (EBoolState)((ParseContext)(ref input)).ReadEnum();
				break;
			}
		}
	}
}
