using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class UserReadyReq : IMessage<UserReadyReq>, IMessage, IEquatable<UserReadyReq>, IDeepCloneable<UserReadyReq>, IBufferMessage
{
	private static readonly MessageParser<UserReadyReq> _parser = new MessageParser<UserReadyReq>((Func<UserReadyReq>)(() => new UserReadyReq()));

	private UnknownFieldSet _unknownFields;

	public const int PlatformFieldNumber = 1;

	private EPlatformType platform_;

	public const int AppChannelFieldNumber = 2;

	private EChannelType appChannel_;

	public const int GuidFieldNumber = 3;

	private long guid_;

	public const int LangFieldNumber = 4;

	private ELanguageType lang_;

	public const int TicketFieldNumber = 5;

	private string ticket_ = "";

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<UserReadyReq> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeUserReadyReqReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public EPlatformType Platform
	{
		get
		{
			return platform_;
		}
		set
		{
			platform_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public EChannelType AppChannel
	{
		get
		{
			return appChannel_;
		}
		set
		{
			appChannel_ = value;
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
	public ELanguageType Lang
	{
		get
		{
			return lang_;
		}
		set
		{
			lang_ = value;
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
	public UserReadyReq()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public UserReadyReq(UserReadyReq other)
		: this()
	{
		platform_ = other.platform_;
		appChannel_ = other.appChannel_;
		guid_ = other.guid_;
		lang_ = other.lang_;
		ticket_ = other.ticket_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public UserReadyReq Clone()
	{
		return new UserReadyReq(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as UserReadyReq);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(UserReadyReq other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (Platform != other.Platform)
		{
			return false;
		}
		if (AppChannel != other.AppChannel)
		{
			return false;
		}
		if (Guid != other.Guid)
		{
			return false;
		}
		if (Lang != other.Lang)
		{
			return false;
		}
		if (Ticket != other.Ticket)
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
		if (Platform != EPlatformType.EmptyValue)
		{
			num ^= Platform.GetHashCode();
		}
		if (AppChannel != EChannelType.EmptyValue)
		{
			num ^= AppChannel.GetHashCode();
		}
		if (Guid != 0L)
		{
			num ^= Guid.GetHashCode();
		}
		if (Lang != ELanguageType.EmptyValue)
		{
			num ^= Lang.GetHashCode();
		}
		if (Ticket.Length != 0)
		{
			num ^= Ticket.GetHashCode();
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
		if (Platform != EPlatformType.EmptyValue)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteEnum((int)Platform);
		}
		if (AppChannel != EChannelType.EmptyValue)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteEnum((int)AppChannel);
		}
		if (Guid != 0L)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)24);
			((WriteContext)(ref output)).WriteInt64(Guid);
		}
		if (Lang != ELanguageType.EmptyValue)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)32);
			((WriteContext)(ref output)).WriteEnum((int)Lang);
		}
		if (Ticket.Length != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)42);
			((WriteContext)(ref output)).WriteString(Ticket);
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
		if (Platform != EPlatformType.EmptyValue)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)Platform);
		}
		if (AppChannel != EChannelType.EmptyValue)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)AppChannel);
		}
		if (Guid != 0L)
		{
			num += 1 + CodedOutputStream.ComputeInt64Size(Guid);
		}
		if (Lang != ELanguageType.EmptyValue)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)Lang);
		}
		if (Ticket.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(Ticket);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(UserReadyReq other)
	{
		if (other != null)
		{
			if (other.Platform != EPlatformType.EmptyValue)
			{
				Platform = other.Platform;
			}
			if (other.AppChannel != EChannelType.EmptyValue)
			{
				AppChannel = other.AppChannel;
			}
			if (other.Guid != 0L)
			{
				Guid = other.Guid;
			}
			if (other.Lang != ELanguageType.EmptyValue)
			{
				Lang = other.Lang;
			}
			if (other.Ticket.Length != 0)
			{
				Ticket = other.Ticket;
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
				Platform = (EPlatformType)((ParseContext)(ref input)).ReadEnum();
				break;
			case 16u:
				AppChannel = (EChannelType)((ParseContext)(ref input)).ReadEnum();
				break;
			case 24u:
				Guid = ((ParseContext)(ref input)).ReadInt64();
				break;
			case 32u:
				Lang = (ELanguageType)((ParseContext)(ref input)).ReadEnum();
				break;
			case 42u:
				Ticket = ((ParseContext)(ref input)).ReadString();
				break;
			}
		}
	}
}
