using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class OperateSnowVeilEventRollingAck : IMessage<OperateSnowVeilEventRollingAck>, IMessage, IEquatable<OperateSnowVeilEventRollingAck>, IDeepCloneable<OperateSnowVeilEventRollingAck>, IBufferMessage
{
	private static readonly MessageParser<OperateSnowVeilEventRollingAck> _parser = new MessageParser<OperateSnowVeilEventRollingAck>((Func<OperateSnowVeilEventRollingAck>)(() => new OperateSnowVeilEventRollingAck()));

	private UnknownFieldSet _unknownFields;

	public const int UUIDFieldNumber = 1;

	private string uUID_ = "";

	public const int LandListFieldNumber = 2;

	private static readonly FieldCodec<SnowVeilLandDetail> _repeated_landList_codec = FieldCodec.ForMessage<SnowVeilLandDetail>(18u, SnowVeilLandDetail.Parser);

	private readonly RepeatedField<SnowVeilLandDetail> landList_ = new RepeatedField<SnowVeilLandDetail>();

	public const int RollingFieldNumber = 3;

	private SnowVeilRollingDetail rolling_;

	public const int RoleStateFieldNumber = 4;

	private SnowVeilRoleState roleState_;

	public const int DataChangeFieldNumber = 5;

	private DataChange dataChange_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<OperateSnowVeilEventRollingAck> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeOperateSnowVeilEventRollingAckReflection.Descriptor.MessageTypes[0];

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
	public RepeatedField<SnowVeilLandDetail> LandList => landList_;

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
	public OperateSnowVeilEventRollingAck()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public OperateSnowVeilEventRollingAck(OperateSnowVeilEventRollingAck other)
		: this()
	{
		uUID_ = other.uUID_;
		landList_ = other.landList_.Clone();
		rolling_ = ((other.rolling_ != null) ? other.rolling_.Clone() : null);
		roleState_ = ((other.roleState_ != null) ? other.roleState_.Clone() : null);
		dataChange_ = ((other.dataChange_ != null) ? other.dataChange_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public OperateSnowVeilEventRollingAck Clone()
	{
		return new OperateSnowVeilEventRollingAck(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as OperateSnowVeilEventRollingAck);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(OperateSnowVeilEventRollingAck other)
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
		if (!landList_.Equals(other.landList_))
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
		num ^= ((object)landList_).GetHashCode();
		if (rolling_ != null)
		{
			num ^= Rolling.GetHashCode();
		}
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
		landList_.WriteTo(ref output, _repeated_landList_codec);
		if (rolling_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)26);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)Rolling);
		}
		if (roleState_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)34);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)RoleState);
		}
		if (dataChange_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)42);
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
		num += landList_.CalculateSize(_repeated_landList_codec);
		if (rolling_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)Rolling);
		}
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
	public void MergeFrom(OperateSnowVeilEventRollingAck other)
	{
		if (other == null)
		{
			return;
		}
		if (other.UUID.Length != 0)
		{
			UUID = other.UUID;
		}
		landList_.Add((IEnumerable<SnowVeilLandDetail>)other.landList_);
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
			case 18u:
				landList_.AddEntriesFrom(ref input, _repeated_landList_codec);
				break;
			case 26u:
				if (rolling_ == null)
				{
					Rolling = new SnowVeilRollingDetail();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)Rolling);
				break;
			case 34u:
				if (roleState_ == null)
				{
					RoleState = new SnowVeilRoleState();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)RoleState);
				break;
			case 42u:
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
