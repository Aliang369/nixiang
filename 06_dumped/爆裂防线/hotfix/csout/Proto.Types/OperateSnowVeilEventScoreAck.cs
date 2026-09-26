using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class OperateSnowVeilEventScoreAck : IMessage<OperateSnowVeilEventScoreAck>, IMessage, IEquatable<OperateSnowVeilEventScoreAck>, IDeepCloneable<OperateSnowVeilEventScoreAck>, IBufferMessage
{
	private static readonly MessageParser<OperateSnowVeilEventScoreAck> _parser = new MessageParser<OperateSnowVeilEventScoreAck>((Func<OperateSnowVeilEventScoreAck>)(() => new OperateSnowVeilEventScoreAck()));

	private UnknownFieldSet _unknownFields;

	public const int UUIDFieldNumber = 1;

	private string uUID_ = "";

	public const int TotalScoreFieldNumber = 2;

	private int totalScore_;

	public const int TargetDetailFieldNumber = 3;

	private OperateSnowVeilTargetDetail targetDetail_;

	public const int TargetNextGrandFieldNumber = 4;

	private OperateTargetNextGrandDetail targetNextGrand_;

	public const int LandListFieldNumber = 5;

	private static readonly FieldCodec<SnowVeilLandDetail> _repeated_landList_codec = FieldCodec.ForMessage<SnowVeilLandDetail>(42u, SnowVeilLandDetail.Parser);

	private readonly RepeatedField<SnowVeilLandDetail> landList_ = new RepeatedField<SnowVeilLandDetail>();

	public const int RoleStateFieldNumber = 6;

	private SnowVeilRoleState roleState_;

	public const int DataChangeFieldNumber = 7;

	private DataChange dataChange_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<OperateSnowVeilEventScoreAck> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeOperateSnowVeilEventScoreAckReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public string UUID
	{
		get
		{
			return uUID_;
		}
		set
		{
			uUID_ = ProtoPreconditions.CheckNotNull<string>(value, "value");
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
	public RepeatedField<SnowVeilLandDetail> LandList => landList_;

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
	public DataChange DataChange
	{
		get
		{
			return dataChange_;
		}
		set
		{
			dataChange_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public OperateSnowVeilEventScoreAck()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public OperateSnowVeilEventScoreAck(OperateSnowVeilEventScoreAck other)
		: this()
	{
		uUID_ = other.uUID_;
		totalScore_ = other.totalScore_;
		targetDetail_ = ((other.targetDetail_ != null) ? other.targetDetail_.Clone() : null);
		targetNextGrand_ = ((other.targetNextGrand_ != null) ? other.targetNextGrand_.Clone() : null);
		landList_ = other.landList_.Clone();
		roleState_ = ((other.roleState_ != null) ? other.roleState_.Clone() : null);
		dataChange_ = ((other.dataChange_ != null) ? other.dataChange_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public OperateSnowVeilEventScoreAck Clone()
	{
		return new OperateSnowVeilEventScoreAck(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as OperateSnowVeilEventScoreAck);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(OperateSnowVeilEventScoreAck other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (UUID != other.UUID)
		{
			return false;
		}
		if (TotalScore != other.TotalScore)
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
		if (!landList_.Equals(other.landList_))
		{
			return false;
		}
		if (!object.Equals(RoleState, other.RoleState))
		{
			return false;
		}
		if (!object.Equals(DataChange, other.DataChange))
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
		if (UUID.Length != 0)
		{
			num ^= UUID.GetHashCode();
		}
		if (TotalScore != 0)
		{
			num ^= TotalScore.GetHashCode();
		}
		if (targetDetail_ != null)
		{
			num ^= TargetDetail.GetHashCode();
		}
		if (targetNextGrand_ != null)
		{
			num ^= TargetNextGrand.GetHashCode();
		}
		num ^= ((object)landList_).GetHashCode();
		if (roleState_ != null)
		{
			num ^= RoleState.GetHashCode();
		}
		if (dataChange_ != null)
		{
			num ^= DataChange.GetHashCode();
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
		if (UUID.Length != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)10);
			((WriteContext)(ref output)).WriteString(UUID);
		}
		if (TotalScore != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteInt32(TotalScore);
		}
		if (targetDetail_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)26);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)TargetDetail);
		}
		if (targetNextGrand_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)34);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)TargetNextGrand);
		}
		landList_.WriteTo(ref output, _repeated_landList_codec);
		if (roleState_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)50);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)RoleState);
		}
		if (dataChange_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)58);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)DataChange);
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
		if (UUID.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(UUID);
		}
		if (TotalScore != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(TotalScore);
		}
		if (targetDetail_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)TargetDetail);
		}
		if (targetNextGrand_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)TargetNextGrand);
		}
		num += landList_.CalculateSize(_repeated_landList_codec);
		if (roleState_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)RoleState);
		}
		if (dataChange_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)DataChange);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(OperateSnowVeilEventScoreAck other)
	{
		if (other == null)
		{
			return;
		}
		if (other.UUID.Length != 0)
		{
			UUID = other.UUID;
		}
		if (other.TotalScore != 0)
		{
			TotalScore = other.TotalScore;
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
		landList_.Add((IEnumerable<SnowVeilLandDetail>)other.landList_);
		if (other.roleState_ != null)
		{
			if (roleState_ == null)
			{
				RoleState = new SnowVeilRoleState();
			}
			RoleState.MergeFrom(other.RoleState);
		}
		if (other.dataChange_ != null)
		{
			if (dataChange_ == null)
			{
				DataChange = new DataChange();
			}
			DataChange.MergeFrom(other.DataChange);
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
				UUID = ((ParseContext)(ref input)).ReadString();
				break;
			case 16u:
				TotalScore = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 26u:
				if (targetDetail_ == null)
				{
					TargetDetail = new OperateSnowVeilTargetDetail();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)TargetDetail);
				break;
			case 34u:
				if (targetNextGrand_ == null)
				{
					TargetNextGrand = new OperateTargetNextGrandDetail();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)TargetNextGrand);
				break;
			case 42u:
				landList_.AddEntriesFrom(ref input, _repeated_landList_codec);
				break;
			case 50u:
				if (roleState_ == null)
				{
					RoleState = new SnowVeilRoleState();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)RoleState);
				break;
			case 58u:
				if (dataChange_ == null)
				{
					DataChange = new DataChange();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)DataChange);
				break;
			}
		}
	}
}
