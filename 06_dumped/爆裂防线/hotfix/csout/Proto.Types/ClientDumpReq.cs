using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class ClientDumpReq : IMessage<ClientDumpReq>, IMessage, IEquatable<ClientDumpReq>, IDeepCloneable<ClientDumpReq>, IBufferMessage
{
	private static readonly MessageParser<ClientDumpReq> _parser = new MessageParser<ClientDumpReq>((Func<ClientDumpReq>)(() => new ClientDumpReq()));

	private UnknownFieldSet _unknownFields;

	public const int AppChannelFieldNumber = 1;

	private EChannelType appChannel_;

	public const int OsFieldNumber = 2;

	private string os_ = "";

	public const int RoleIdFieldNumber = 3;

	private long roleId_;

	public const int ServerIdFieldNumber = 4;

	private int serverId_;

	public const int AppVersionFieldNumber = 5;

	private string appVersion_ = "";

	public const int AppResVersionFieldNumber = 6;

	private string appResVersion_ = "";

	public const int StartTimeFieldNumber = 7;

	private long startTime_;

	public const int CountFieldNumber = 8;

	private int count_;

	public const int ContentFieldNumber = 9;

	private string content_ = "";

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<ClientDumpReq> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeClientDumpReqReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

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
	public string Os
	{
		get
		{
			return os_;
		}
		set
		{
			os_ = ProtoPreconditions.CheckNotNull<string>(value, "value");
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public long RoleId
	{
		get
		{
			return roleId_;
		}
		set
		{
			roleId_ = value;
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
	public string AppVersion
	{
		get
		{
			return appVersion_;
		}
		set
		{
			appVersion_ = ProtoPreconditions.CheckNotNull<string>(value, "value");
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public string AppResVersion
	{
		get
		{
			return appResVersion_;
		}
		set
		{
			appResVersion_ = ProtoPreconditions.CheckNotNull<string>(value, "value");
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public long StartTime
	{
		get
		{
			return startTime_;
		}
		set
		{
			startTime_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int Count
	{
		get
		{
			return count_;
		}
		set
		{
			count_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public string Content
	{
		get
		{
			return content_;
		}
		set
		{
			content_ = ProtoPreconditions.CheckNotNull<string>(value, "value");
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ClientDumpReq()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ClientDumpReq(ClientDumpReq other)
		: this()
	{
		appChannel_ = other.appChannel_;
		os_ = other.os_;
		roleId_ = other.roleId_;
		serverId_ = other.serverId_;
		appVersion_ = other.appVersion_;
		appResVersion_ = other.appResVersion_;
		startTime_ = other.startTime_;
		count_ = other.count_;
		content_ = other.content_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ClientDumpReq Clone()
	{
		return new ClientDumpReq(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as ClientDumpReq);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(ClientDumpReq other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (AppChannel != other.AppChannel)
		{
			return false;
		}
		if (Os != other.Os)
		{
			return false;
		}
		if (RoleId != other.RoleId)
		{
			return false;
		}
		if (ServerId != other.ServerId)
		{
			return false;
		}
		if (AppVersion != other.AppVersion)
		{
			return false;
		}
		if (AppResVersion != other.AppResVersion)
		{
			return false;
		}
		if (StartTime != other.StartTime)
		{
			return false;
		}
		if (Count != other.Count)
		{
			return false;
		}
		if (Content != other.Content)
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
		if (AppChannel != EChannelType.EmptyValue)
		{
			num ^= AppChannel.GetHashCode();
		}
		if (Os.Length != 0)
		{
			num ^= Os.GetHashCode();
		}
		if (RoleId != 0L)
		{
			num ^= RoleId.GetHashCode();
		}
		if (ServerId != 0)
		{
			num ^= ServerId.GetHashCode();
		}
		if (AppVersion.Length != 0)
		{
			num ^= AppVersion.GetHashCode();
		}
		if (AppResVersion.Length != 0)
		{
			num ^= AppResVersion.GetHashCode();
		}
		if (StartTime != 0L)
		{
			num ^= StartTime.GetHashCode();
		}
		if (Count != 0)
		{
			num ^= Count.GetHashCode();
		}
		if (Content.Length != 0)
		{
			num ^= Content.GetHashCode();
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
		if (AppChannel != EChannelType.EmptyValue)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteEnum((int)AppChannel);
		}
		if (Os.Length != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)18);
			((WriteContext)(ref output)).WriteString(Os);
		}
		if (RoleId != 0L)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)24);
			((WriteContext)(ref output)).WriteInt64(RoleId);
		}
		if (ServerId != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)32);
			((WriteContext)(ref output)).WriteInt32(ServerId);
		}
		if (AppVersion.Length != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)42);
			((WriteContext)(ref output)).WriteString(AppVersion);
		}
		if (AppResVersion.Length != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)50);
			((WriteContext)(ref output)).WriteString(AppResVersion);
		}
		if (StartTime != 0L)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)56);
			((WriteContext)(ref output)).WriteInt64(StartTime);
		}
		if (Count != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)64);
			((WriteContext)(ref output)).WriteInt32(Count);
		}
		if (Content.Length != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)74);
			((WriteContext)(ref output)).WriteString(Content);
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
		if (AppChannel != EChannelType.EmptyValue)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)AppChannel);
		}
		if (Os.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(Os);
		}
		if (RoleId != 0L)
		{
			num += 1 + CodedOutputStream.ComputeInt64Size(RoleId);
		}
		if (ServerId != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(ServerId);
		}
		if (AppVersion.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(AppVersion);
		}
		if (AppResVersion.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(AppResVersion);
		}
		if (StartTime != 0L)
		{
			num += 1 + CodedOutputStream.ComputeInt64Size(StartTime);
		}
		if (Count != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(Count);
		}
		if (Content.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(Content);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(ClientDumpReq other)
	{
		if (other != null)
		{
			if (other.AppChannel != EChannelType.EmptyValue)
			{
				AppChannel = other.AppChannel;
			}
			if (other.Os.Length != 0)
			{
				Os = other.Os;
			}
			if (other.RoleId != 0L)
			{
				RoleId = other.RoleId;
			}
			if (other.ServerId != 0)
			{
				ServerId = other.ServerId;
			}
			if (other.AppVersion.Length != 0)
			{
				AppVersion = other.AppVersion;
			}
			if (other.AppResVersion.Length != 0)
			{
				AppResVersion = other.AppResVersion;
			}
			if (other.StartTime != 0L)
			{
				StartTime = other.StartTime;
			}
			if (other.Count != 0)
			{
				Count = other.Count;
			}
			if (other.Content.Length != 0)
			{
				Content = other.Content;
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
				AppChannel = (EChannelType)((ParseContext)(ref input)).ReadEnum();
				break;
			case 18u:
				Os = ((ParseContext)(ref input)).ReadString();
				break;
			case 24u:
				RoleId = ((ParseContext)(ref input)).ReadInt64();
				break;
			case 32u:
				ServerId = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 42u:
				AppVersion = ((ParseContext)(ref input)).ReadString();
				break;
			case 50u:
				AppResVersion = ((ParseContext)(ref input)).ReadString();
				break;
			case 56u:
				StartTime = ((ParseContext)(ref input)).ReadInt64();
				break;
			case 64u:
				Count = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 74u:
				Content = ((ParseContext)(ref input)).ReadString();
				break;
			}
		}
	}
}
