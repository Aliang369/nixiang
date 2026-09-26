using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class UserLoginAck : IMessage<UserLoginAck>, IMessage, IEquatable<UserLoginAck>, IDeepCloneable<UserLoginAck>, IBufferMessage
{
	private static readonly MessageParser<UserLoginAck> _parser = new MessageParser<UserLoginAck>((Func<UserLoginAck>)(() => new UserLoginAck()));

	private UnknownFieldSet _unknownFields;

	public const int LoginChannelFieldNumber = 1;

	private EChannelType loginChannel_;

	public const int GuidFieldNumber = 2;

	private long guid_;

	public const int TicketFieldNumber = 3;

	private string ticket_ = "";

	public const int LoginAccountIDFieldNumber = 4;

	private string loginAccountID_ = "";

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<UserLoginAck> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeUserLoginAckReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public EChannelType LoginChannel
	{
		get
		{
			return loginChannel_;
		}
		set
		{
			loginChannel_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public long Guid
	{
		get
		{
			return guid_;
		}
		set
		{
			guid_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public string Ticket
	{
		get
		{
			return ticket_;
		}
		set
		{
			ticket_ = ProtoPreconditions.CheckNotNull<string>(value, "value");
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public string LoginAccountID
	{
		get
		{
			return loginAccountID_;
		}
		set
		{
			loginAccountID_ = ProtoPreconditions.CheckNotNull<string>(value, "value");
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public UserLoginAck()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public UserLoginAck(UserLoginAck other)
		: this()
	{
		loginChannel_ = other.loginChannel_;
		guid_ = other.guid_;
		ticket_ = other.ticket_;
		loginAccountID_ = other.loginAccountID_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public UserLoginAck Clone()
	{
		return new UserLoginAck(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as UserLoginAck);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(UserLoginAck other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (LoginChannel != other.LoginChannel)
		{
			return false;
		}
		if (Guid != other.Guid)
		{
			return false;
		}
		if (Ticket != other.Ticket)
		{
			return false;
		}
		if (LoginAccountID != other.LoginAccountID)
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
		if (LoginChannel != EChannelType.EmptyValue)
		{
			num ^= LoginChannel.GetHashCode();
		}
		if (Guid != 0L)
		{
			num ^= Guid.GetHashCode();
		}
		if (Ticket.Length != 0)
		{
			num ^= Ticket.GetHashCode();
		}
		if (LoginAccountID.Length != 0)
		{
			num ^= LoginAccountID.GetHashCode();
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
		if (LoginChannel != EChannelType.EmptyValue)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteEnum((int)LoginChannel);
		}
		if (Guid != 0L)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteInt64(Guid);
		}
		if (Ticket.Length != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)26);
			((WriteContext)(ref output)).WriteString(Ticket);
		}
		if (LoginAccountID.Length != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)34);
			((WriteContext)(ref output)).WriteString(LoginAccountID);
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
		if (LoginChannel != EChannelType.EmptyValue)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)LoginChannel);
		}
		if (Guid != 0L)
		{
			num += 1 + CodedOutputStream.ComputeInt64Size(Guid);
		}
		if (Ticket.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(Ticket);
		}
		if (LoginAccountID.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(LoginAccountID);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(UserLoginAck other)
	{
		if (other != null)
		{
			if (other.LoginChannel != EChannelType.EmptyValue)
			{
				LoginChannel = other.LoginChannel;
			}
			if (other.Guid != 0L)
			{
				Guid = other.Guid;
			}
			if (other.Ticket.Length != 0)
			{
				Ticket = other.Ticket;
			}
			if (other.LoginAccountID.Length != 0)
			{
				LoginAccountID = other.LoginAccountID;
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
				LoginChannel = (EChannelType)((ParseContext)(ref input)).ReadEnum();
				break;
			case 16u:
				Guid = ((ParseContext)(ref input)).ReadInt64();
				break;
			case 26u:
				Ticket = ((ParseContext)(ref input)).ReadString();
				break;
			case 34u:
				LoginAccountID = ((ParseContext)(ref input)).ReadString();
				break;
			}
		}
	}
}
