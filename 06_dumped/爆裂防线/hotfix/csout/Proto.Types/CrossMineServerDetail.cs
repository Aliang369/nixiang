using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class CrossMineServerDetail : IMessage<CrossMineServerDetail>, IMessage, IEquatable<CrossMineServerDetail>, IDeepCloneable<CrossMineServerDetail>, IBufferMessage
{
	private static readonly MessageParser<CrossMineServerDetail> _parser = new MessageParser<CrossMineServerDetail>((Func<CrossMineServerDetail>)(() => new CrossMineServerDetail()));

	private UnknownFieldSet _unknownFields;

	public const int ServerIdFieldNumber = 1;

	private int serverId_;

	public const int ServerNameFieldNumber = 2;

	private string serverName_ = "";

	public const int StateFieldNumber = 3;

	private EMineWarServerState state_;

	public const int CostCombatNumFieldNumber = 4;

	private int costCombatNum_;

	public const int CrossOpenTmFieldNumber = 5;

	private long crossOpenTm_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<CrossMineServerDetail> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeCrossMineServerDetailReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int ServerId
	{
		get
		{
			return serverId_;
		}
		set
		{
			serverId_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public string ServerName
	{
		get
		{
			return serverName_;
		}
		set
		{
			serverName_ = ProtoPreconditions.CheckNotNull<string>(value, "value");
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public EMineWarServerState State
	{
		get
		{
			return state_;
		}
		set
		{
			state_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int CostCombatNum
	{
		get
		{
			return costCombatNum_;
		}
		set
		{
			costCombatNum_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public long CrossOpenTm
	{
		get
		{
			return crossOpenTm_;
		}
		set
		{
			crossOpenTm_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public CrossMineServerDetail()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public CrossMineServerDetail(CrossMineServerDetail other)
		: this()
	{
		serverId_ = other.serverId_;
		serverName_ = other.serverName_;
		state_ = other.state_;
		costCombatNum_ = other.costCombatNum_;
		crossOpenTm_ = other.crossOpenTm_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public CrossMineServerDetail Clone()
	{
		return new CrossMineServerDetail(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as CrossMineServerDetail);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(CrossMineServerDetail other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (ServerId != other.ServerId)
		{
			return false;
		}
		if (ServerName != other.ServerName)
		{
			return false;
		}
		if (State != other.State)
		{
			return false;
		}
		if (CostCombatNum != other.CostCombatNum)
		{
			return false;
		}
		if (CrossOpenTm != other.CrossOpenTm)
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
		if (ServerId != 0)
		{
			num ^= ServerId.GetHashCode();
		}
		if (ServerName.Length != 0)
		{
			num ^= ServerName.GetHashCode();
		}
		if (State != EMineWarServerState.EmptyValue)
		{
			num ^= State.GetHashCode();
		}
		if (CostCombatNum != 0)
		{
			num ^= CostCombatNum.GetHashCode();
		}
		if (CrossOpenTm != 0L)
		{
			num ^= CrossOpenTm.GetHashCode();
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
		if (ServerId != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteInt32(ServerId);
		}
		if (ServerName.Length != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)18);
			((WriteContext)(ref output)).WriteString(ServerName);
		}
		if (State != EMineWarServerState.EmptyValue)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)24);
			((WriteContext)(ref output)).WriteEnum((int)State);
		}
		if (CostCombatNum != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)32);
			((WriteContext)(ref output)).WriteInt32(CostCombatNum);
		}
		if (CrossOpenTm != 0L)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)40);
			((WriteContext)(ref output)).WriteInt64(CrossOpenTm);
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
		if (ServerId != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(ServerId);
		}
		if (ServerName.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(ServerName);
		}
		if (State != EMineWarServerState.EmptyValue)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)State);
		}
		if (CostCombatNum != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(CostCombatNum);
		}
		if (CrossOpenTm != 0L)
		{
			num += 1 + CodedOutputStream.ComputeInt64Size(CrossOpenTm);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(CrossMineServerDetail other)
	{
		if (other != null)
		{
			if (other.ServerId != 0)
			{
				ServerId = other.ServerId;
			}
			if (other.ServerName.Length != 0)
			{
				ServerName = other.ServerName;
			}
			if (other.State != EMineWarServerState.EmptyValue)
			{
				State = other.State;
			}
			if (other.CostCombatNum != 0)
			{
				CostCombatNum = other.CostCombatNum;
			}
			if (other.CrossOpenTm != 0L)
			{
				CrossOpenTm = other.CrossOpenTm;
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
				ServerId = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 18u:
				ServerName = ((ParseContext)(ref input)).ReadString();
				break;
			case 24u:
				State = (EMineWarServerState)((ParseContext)(ref input)).ReadEnum();
				break;
			case 32u:
				CostCombatNum = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 40u:
				CrossOpenTm = ((ParseContext)(ref input)).ReadInt64();
				break;
			}
		}
	}
}
