using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class RoleCommonInfo : IMessage<RoleCommonInfo>, IMessage, IEquatable<RoleCommonInfo>, IDeepCloneable<RoleCommonInfo>, IBufferMessage
{
	private static readonly MessageParser<RoleCommonInfo> _parser = new MessageParser<RoleCommonInfo>((Func<RoleCommonInfo>)(() => new RoleCommonInfo()));

	private UnknownFieldSet _unknownFields;

	public const int CreateTmFieldNumber = 1;

	private long createTm_;

	public const int LastRefreshFieldNumber = 2;

	private long lastRefresh_;

	public const int NextRefreshFieldNumber = 3;

	private long nextRefresh_;

	public const int ServerDaysFieldNumber = 4;

	private int serverDays_;

	public const int RoleExpFieldNumber = 5;

	private int roleExp_;

	public const int RoleNameChangeFieldNumber = 6;

	private int roleNameChange_;

	public const int RechargeAmountFieldNumber = 7;

	private float rechargeAmount_;

	public const int ServerOpenTimeFieldNumber = 8;

	private long serverOpenTime_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<RoleCommonInfo> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeRoleCommonInfoReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public long CreateTm
	{
		get
		{
			return createTm_;
		}
		set
		{
			createTm_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public long LastRefresh
	{
		get
		{
			return lastRefresh_;
		}
		set
		{
			lastRefresh_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public long NextRefresh
	{
		get
		{
			return nextRefresh_;
		}
		set
		{
			nextRefresh_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int ServerDays
	{
		get
		{
			return serverDays_;
		}
		set
		{
			serverDays_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int RoleExp
	{
		get
		{
			return roleExp_;
		}
		set
		{
			roleExp_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int RoleNameChange
	{
		get
		{
			return roleNameChange_;
		}
		set
		{
			roleNameChange_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public float RechargeAmount
	{
		get
		{
			return rechargeAmount_;
		}
		set
		{
			rechargeAmount_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public long ServerOpenTime
	{
		get
		{
			return serverOpenTime_;
		}
		set
		{
			serverOpenTime_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RoleCommonInfo()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RoleCommonInfo(RoleCommonInfo other)
		: this()
	{
		createTm_ = other.createTm_;
		lastRefresh_ = other.lastRefresh_;
		nextRefresh_ = other.nextRefresh_;
		serverDays_ = other.serverDays_;
		roleExp_ = other.roleExp_;
		roleNameChange_ = other.roleNameChange_;
		rechargeAmount_ = other.rechargeAmount_;
		serverOpenTime_ = other.serverOpenTime_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RoleCommonInfo Clone()
	{
		return new RoleCommonInfo(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as RoleCommonInfo);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(RoleCommonInfo other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (CreateTm != other.CreateTm)
		{
			return false;
		}
		if (LastRefresh != other.LastRefresh)
		{
			return false;
		}
		if (NextRefresh != other.NextRefresh)
		{
			return false;
		}
		if (ServerDays != other.ServerDays)
		{
			return false;
		}
		if (RoleExp != other.RoleExp)
		{
			return false;
		}
		if (RoleNameChange != other.RoleNameChange)
		{
			return false;
		}
		if (!ProtobufEqualityComparers.BitwiseSingleEqualityComparer.Equals(RechargeAmount, other.RechargeAmount))
		{
			return false;
		}
		if (ServerOpenTime != other.ServerOpenTime)
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
		if (CreateTm != 0L)
		{
			num ^= CreateTm.GetHashCode();
		}
		if (LastRefresh != 0L)
		{
			num ^= LastRefresh.GetHashCode();
		}
		if (NextRefresh != 0L)
		{
			num ^= NextRefresh.GetHashCode();
		}
		if (ServerDays != 0)
		{
			num ^= ServerDays.GetHashCode();
		}
		if (RoleExp != 0)
		{
			num ^= RoleExp.GetHashCode();
		}
		if (RoleNameChange != 0)
		{
			num ^= RoleNameChange.GetHashCode();
		}
		if (RechargeAmount != 0f)
		{
			num ^= ProtobufEqualityComparers.BitwiseSingleEqualityComparer.GetHashCode(RechargeAmount);
		}
		if (ServerOpenTime != 0L)
		{
			num ^= ServerOpenTime.GetHashCode();
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
		if (CreateTm != 0L)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteInt64(CreateTm);
		}
		if (LastRefresh != 0L)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteInt64(LastRefresh);
		}
		if (NextRefresh != 0L)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)24);
			((WriteContext)(ref output)).WriteInt64(NextRefresh);
		}
		if (ServerDays != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)32);
			((WriteContext)(ref output)).WriteInt32(ServerDays);
		}
		if (RoleExp != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)40);
			((WriteContext)(ref output)).WriteInt32(RoleExp);
		}
		if (RoleNameChange != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)48);
			((WriteContext)(ref output)).WriteInt32(RoleNameChange);
		}
		if (RechargeAmount != 0f)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)61);
			((WriteContext)(ref output)).WriteFloat(RechargeAmount);
		}
		if (ServerOpenTime != 0L)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)64);
			((WriteContext)(ref output)).WriteInt64(ServerOpenTime);
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
		if (CreateTm != 0L)
		{
			num += 1 + CodedOutputStream.ComputeInt64Size(CreateTm);
		}
		if (LastRefresh != 0L)
		{
			num += 1 + CodedOutputStream.ComputeInt64Size(LastRefresh);
		}
		if (NextRefresh != 0L)
		{
			num += 1 + CodedOutputStream.ComputeInt64Size(NextRefresh);
		}
		if (ServerDays != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(ServerDays);
		}
		if (RoleExp != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(RoleExp);
		}
		if (RoleNameChange != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(RoleNameChange);
		}
		if (RechargeAmount != 0f)
		{
			num += 5;
		}
		if (ServerOpenTime != 0L)
		{
			num += 1 + CodedOutputStream.ComputeInt64Size(ServerOpenTime);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(RoleCommonInfo other)
	{
		if (other != null)
		{
			if (other.CreateTm != 0L)
			{
				CreateTm = other.CreateTm;
			}
			if (other.LastRefresh != 0L)
			{
				LastRefresh = other.LastRefresh;
			}
			if (other.NextRefresh != 0L)
			{
				NextRefresh = other.NextRefresh;
			}
			if (other.ServerDays != 0)
			{
				ServerDays = other.ServerDays;
			}
			if (other.RoleExp != 0)
			{
				RoleExp = other.RoleExp;
			}
			if (other.RoleNameChange != 0)
			{
				RoleNameChange = other.RoleNameChange;
			}
			if (other.RechargeAmount != 0f)
			{
				RechargeAmount = other.RechargeAmount;
			}
			if (other.ServerOpenTime != 0L)
			{
				ServerOpenTime = other.ServerOpenTime;
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
				CreateTm = ((ParseContext)(ref input)).ReadInt64();
				break;
			case 16u:
				LastRefresh = ((ParseContext)(ref input)).ReadInt64();
				break;
			case 24u:
				NextRefresh = ((ParseContext)(ref input)).ReadInt64();
				break;
			case 32u:
				ServerDays = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 40u:
				RoleExp = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 48u:
				RoleNameChange = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 61u:
				RechargeAmount = ((ParseContext)(ref input)).ReadFloat();
				break;
			case 64u:
				ServerOpenTime = ((ParseContext)(ref input)).ReadInt64();
				break;
			}
		}
	}
}
