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
public sealed class SnowVeilDetail : IMessage<SnowVeilDetail>, IMessage, IEquatable<SnowVeilDetail>, IDeepCloneable<SnowVeilDetail>, IBufferMessage
{
	private static readonly MessageParser<SnowVeilDetail> _parser = new MessageParser<SnowVeilDetail>((Func<SnowVeilDetail>)(() => new SnowVeilDetail()));

	private UnknownFieldSet _unknownFields;

	public const int BaseInfoFieldNumber = 1;

	private OperateBaseInfo baseInfo_;

	public const int ActivateAwardFieldNumber = 2;

	private EBoolState activateAward_;

	public const int TotalScoreFieldNumber = 3;

	private int totalScore_;

	public const int LandListFieldNumber = 4;

	private static readonly FieldCodec<SnowVeilLandDetail> _repeated_landList_codec = FieldCodec.ForMessage<SnowVeilLandDetail>(34u, SnowVeilLandDetail.Parser);

	private readonly RepeatedField<SnowVeilLandDetail> landList_ = new RepeatedField<SnowVeilLandDetail>();

	public const int TurntableFieldNumber = 5;

	private SnowVeilTurntableDetail turntable_;

	public const int RollingFieldNumber = 6;

	private SnowVeilRollingDetail rolling_;

	public const int RoleStateFieldNumber = 7;

	private SnowVeilRoleState roleState_;

	public const int PropFieldNumber = 8;

	private SnowVeilPropDetail prop_;

	public const int TargetDetailFieldNumber = 9;

	private OperateSnowVeilTargetDetail targetDetail_;

	public const int TargetNextGrandFieldNumber = 10;

	private OperateTargetNextGrandDetail targetNextGrand_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<SnowVeilDetail> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeSnowVeilDetailReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public OperateBaseInfo BaseInfo
	{
		get
		{
			return baseInfo_;
		}
		set
		{
			baseInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public EBoolState ActivateAward
	{
		get
		{
			return activateAward_;
		}
		set
		{
			activateAward_ = value;
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
	public RepeatedField<SnowVeilLandDetail> LandList => landList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SnowVeilTurntableDetail Turntable
	{
		get
		{
			return turntable_;
		}
		set
		{
			turntable_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SnowVeilRollingDetail Rolling
	{
		get
		{
			return rolling_;
		}
		set
		{
			rolling_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SnowVeilRoleState RoleState
	{
		get
		{
			return roleState_;
		}
		set
		{
			roleState_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SnowVeilPropDetail Prop
	{
		get
		{
			return prop_;
		}
		set
		{
			prop_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public OperateSnowVeilTargetDetail TargetDetail
	{
		get
		{
			return targetDetail_;
		}
		set
		{
			targetDetail_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public OperateTargetNextGrandDetail TargetNextGrand
	{
		get
		{
			return targetNextGrand_;
		}
		set
		{
			targetNextGrand_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SnowVeilDetail()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SnowVeilDetail(SnowVeilDetail other)
		: this()
	{
		baseInfo_ = ((other.baseInfo_ != null) ? other.baseInfo_.Clone() : null);
		activateAward_ = other.activateAward_;
		totalScore_ = other.totalScore_;
		landList_ = other.landList_.Clone();
		turntable_ = ((other.turntable_ != null) ? other.turntable_.Clone() : null);
		rolling_ = ((other.rolling_ != null) ? other.rolling_.Clone() : null);
		roleState_ = ((other.roleState_ != null) ? other.roleState_.Clone() : null);
		prop_ = ((other.prop_ != null) ? other.prop_.Clone() : null);
		targetDetail_ = ((other.targetDetail_ != null) ? other.targetDetail_.Clone() : null);
		targetNextGrand_ = ((other.targetNextGrand_ != null) ? other.targetNextGrand_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SnowVeilDetail Clone()
	{
		return new SnowVeilDetail(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as SnowVeilDetail);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(SnowVeilDetail other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!object.Equals(BaseInfo, other.BaseInfo))
		{
			return false;
		}
		if (ActivateAward != other.ActivateAward)
		{
			return false;
		}
		if (TotalScore != other.TotalScore)
		{
			return false;
		}
		if (!landList_.Equals(other.landList_))
		{
			return false;
		}
		if (!object.Equals(Turntable, other.Turntable))
		{
			return false;
		}
		if (!object.Equals(Rolling, other.Rolling))
		{
			return false;
		}
		if (!object.Equals(RoleState, other.RoleState))
		{
			return false;
		}
		if (!object.Equals(Prop, other.Prop))
		{
			return false;
		}
		if (!object.Equals(TargetDetail, other.TargetDetail))
		{
			return false;
		}
		if (!object.Equals(TargetNextGrand, other.TargetNextGrand))
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
		if (baseInfo_ != null)
		{
			num ^= BaseInfo.GetHashCode();
		}
		if (ActivateAward != EBoolState.False)
		{
			num ^= ActivateAward.GetHashCode();
		}
		if (TotalScore != 0)
		{
			num ^= TotalScore.GetHashCode();
		}
		num ^= ((object)landList_).GetHashCode();
		if (turntable_ != null)
		{
			num ^= Turntable.GetHashCode();
		}
		if (rolling_ != null)
		{
			num ^= Rolling.GetHashCode();
		}
		if (roleState_ != null)
		{
			num ^= RoleState.GetHashCode();
		}
		if (prop_ != null)
		{
			num ^= Prop.GetHashCode();
		}
		if (targetDetail_ != null)
		{
			num ^= TargetDetail.GetHashCode();
		}
		if (targetNextGrand_ != null)
		{
			num ^= TargetNextGrand.GetHashCode();
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
		if (baseInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)10);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)BaseInfo);
		}
		if (ActivateAward != EBoolState.False)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteEnum((int)ActivateAward);
		}
		if (TotalScore != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)24);
			((WriteContext)(ref output)).WriteInt32(TotalScore);
		}
		landList_.WriteTo(ref output, _repeated_landList_codec);
		if (turntable_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)42);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)Turntable);
		}
		if (rolling_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)50);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)Rolling);
		}
		if (roleState_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)58);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)RoleState);
		}
		if (prop_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)66);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)Prop);
		}
		if (targetDetail_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)74);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)TargetDetail);
		}
		if (targetNextGrand_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)82);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)TargetNextGrand);
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
		if (baseInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)BaseInfo);
		}
		if (ActivateAward != EBoolState.False)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)ActivateAward);
		}
		if (TotalScore != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(TotalScore);
		}
		num += landList_.CalculateSize(_repeated_landList_codec);
		if (turntable_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)Turntable);
		}
		if (rolling_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)Rolling);
		}
		if (roleState_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)RoleState);
		}
		if (prop_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)Prop);
		}
		if (targetDetail_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)TargetDetail);
		}
		if (targetNextGrand_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)TargetNextGrand);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(SnowVeilDetail other)
	{
		if (other == null)
		{
			return;
		}
		if (other.baseInfo_ != null)
		{
			if (baseInfo_ == null)
			{
				BaseInfo = new OperateBaseInfo();
			}
			BaseInfo.MergeFrom(other.BaseInfo);
		}
		if (other.ActivateAward != EBoolState.False)
		{
			ActivateAward = other.ActivateAward;
		}
		if (other.TotalScore != 0)
		{
			TotalScore = other.TotalScore;
		}
		landList_.Add((IEnumerable<SnowVeilLandDetail>)other.landList_);
		if (other.turntable_ != null)
		{
			if (turntable_ == null)
			{
				Turntable = new SnowVeilTurntableDetail();
			}
			Turntable.MergeFrom(other.Turntable);
		}
		if (other.rolling_ != null)
		{
			if (rolling_ == null)
			{
				Rolling = new SnowVeilRollingDetail();
			}
			Rolling.MergeFrom(other.Rolling);
		}
		if (other.roleState_ != null)
		{
			if (roleState_ == null)
			{
				RoleState = new SnowVeilRoleState();
			}
			RoleState.MergeFrom(other.RoleState);
		}
		if (other.prop_ != null)
		{
			if (prop_ == null)
			{
				Prop = new SnowVeilPropDetail();
			}
			Prop.MergeFrom(other.Prop);
		}
		if (other.targetDetail_ != null)
		{
			if (targetDetail_ == null)
			{
				TargetDetail = new OperateSnowVeilTargetDetail();
			}
			TargetDetail.MergeFrom(other.TargetDetail);
		}
		if (other.targetNextGrand_ != null)
		{
			if (targetNextGrand_ == null)
			{
				TargetNextGrand = new OperateTargetNextGrandDetail();
			}
			TargetNextGrand.MergeFrom(other.TargetNextGrand);
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
				if (baseInfo_ == null)
				{
					BaseInfo = new OperateBaseInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)BaseInfo);
				break;
			case 16u:
				ActivateAward = (EBoolState)((ParseContext)(ref input)).ReadEnum();
				break;
			case 24u:
				TotalScore = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 34u:
				landList_.AddEntriesFrom(ref input, _repeated_landList_codec);
				break;
			case 42u:
				if (turntable_ == null)
				{
					Turntable = new SnowVeilTurntableDetail();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)Turntable);
				break;
			case 50u:
				if (rolling_ == null)
				{
					Rolling = new SnowVeilRollingDetail();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)Rolling);
				break;
			case 58u:
				if (roleState_ == null)
				{
					RoleState = new SnowVeilRoleState();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)RoleState);
				break;
			case 66u:
				if (prop_ == null)
				{
					Prop = new SnowVeilPropDetail();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)Prop);
				break;
			case 74u:
				if (targetDetail_ == null)
				{
					TargetDetail = new OperateSnowVeilTargetDetail();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)TargetDetail);
				break;
			case 82u:
				if (targetNextGrand_ == null)
				{
					TargetNextGrand = new OperateTargetNextGrandDetail();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)TargetNextGrand);
				break;
			}
		}
	}
}
