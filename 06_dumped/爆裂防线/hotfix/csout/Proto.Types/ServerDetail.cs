using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class ServerDetail : IMessage<ServerDetail>, IMessage, IEquatable<ServerDetail>, IDeepCloneable<ServerDetail>, IBufferMessage
{
	private static readonly MessageParser<ServerDetail> _parser = new MessageParser<ServerDetail>((Func<ServerDetail>)(() => new ServerDetail()));

	private UnknownFieldSet _unknownFields;

	public const int SortIdFieldNumber = 1;

	private int sortId_;

	public const int ServerIdFieldNumber = 2;

	private int serverId_;

	public const int ServerNameFieldNumber = 3;

	private string serverName_ = "";

	public const int StatusFieldNumber = 4;

	private EServerState status_;

	public const int RegisterStateFieldNumber = 5;

	private EBoolState registerState_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<ServerDetail> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeServerDetailReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int SortId
	{
		get
		{
			return sortId_;
		}
		set
		{
			sortId_ = value;
		}
	}

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
	public EServerState Status
	{
		get
		{
			return status_;
		}
		set
		{
			status_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public EBoolState RegisterState
	{
		get
		{
			return registerState_;
		}
		set
		{
			registerState_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ServerDetail()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ServerDetail(ServerDetail other)
		: this()
	{
		sortId_ = other.sortId_;
		serverId_ = other.serverId_;
		serverName_ = other.serverName_;
		status_ = other.status_;
		registerState_ = other.registerState_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ServerDetail Clone()
	{
		return new ServerDetail(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as ServerDetail);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(ServerDetail other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (SortId != other.SortId)
		{
			return false;
		}
		if (ServerId != other.ServerId)
		{
			return false;
		}
		if (ServerName != other.ServerName)
		{
			return false;
		}
		if (Status != other.Status)
		{
			return false;
		}
		if (RegisterState != other.RegisterState)
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
		if (SortId != 0)
		{
			num ^= SortId.GetHashCode();
		}
		if (ServerId != 0)
		{
			num ^= ServerId.GetHashCode();
		}
		if (ServerName.Length != 0)
		{
			num ^= ServerName.GetHashCode();
		}
		if (Status != EServerState.Close)
		{
			num ^= Status.GetHashCode();
		}
		if (RegisterState != EBoolState.False)
		{
			num ^= RegisterState.GetHashCode();
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
		if (SortId != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteInt32(SortId);
		}
		if (ServerId != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteInt32(ServerId);
		}
		if (ServerName.Length != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)26);
			((WriteContext)(ref output)).WriteString(ServerName);
		}
		if (Status != EServerState.Close)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)32);
			((WriteContext)(ref output)).WriteEnum((int)Status);
		}
		if (RegisterState != EBoolState.False)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)40);
			((WriteContext)(ref output)).WriteEnum((int)RegisterState);
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
		if (SortId != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(SortId);
		}
		if (ServerId != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(ServerId);
		}
		if (ServerName.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(ServerName);
		}
		if (Status != EServerState.Close)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)Status);
		}
		if (RegisterState != EBoolState.False)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)RegisterState);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(ServerDetail other)
	{
		if (other != null)
		{
			if (other.SortId != 0)
			{
				SortId = other.SortId;
			}
			if (other.ServerId != 0)
			{
				ServerId = other.ServerId;
			}
			if (other.ServerName.Length != 0)
			{
				ServerName = other.ServerName;
			}
			if (other.Status != EServerState.Close)
			{
				Status = other.Status;
			}
			if (other.RegisterState != EBoolState.False)
			{
				RegisterState = other.RegisterState;
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
				SortId = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 16u:
				ServerId = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 26u:
				ServerName = ((ParseContext)(ref input)).ReadString();
				break;
			case 32u:
				Status = (EServerState)((ParseContext)(ref input)).ReadEnum();
				break;
			case 40u:
				RegisterState = (EBoolState)((ParseContext)(ref input)).ReadEnum();
				break;
			}
		}
	}
}
