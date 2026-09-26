using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class UserLoginReq : IMessage<UserLoginReq>, IMessage, IEquatable<UserLoginReq>, IDeepCloneable<UserLoginReq>, IBufferMessage
{
	private static readonly MessageParser<UserLoginReq> _parser = new MessageParser<UserLoginReq>((Func<UserLoginReq>)(() => new UserLoginReq()));

	private UnknownFieldSet _unknownFields;

	public const int PlatformFieldNumber = 1;

	private EPlatformType platform_;

	public const int AppChannelFieldNumber = 2;

	private EChannelType appChannel_;

	public const int LoginChannelFieldNumber = 3;

	private EChannelType loginChannel_;

	public const int LoginTokenFieldNumber = 4;

	private string loginToken_ = "";

	public const int DeviceIdFieldNumber = 5;

	private string deviceId_ = "";

	public const int LangFieldNumber = 6;

	private ELanguageType lang_;

	public const int RegionFieldNumber = 7;

	private string region_ = "";

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<UserLoginReq> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeUserLoginReqReflection.Descriptor.MessageTypes[0];

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
	public string LoginToken
	{
		get
		{
			return loginToken_;
		}
		set
		{
			loginToken_ = ProtoPreconditions.CheckNotNull<string>(value, "value");
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public string DeviceId
	{
		get
		{
			return deviceId_;
		}
		set
		{
			deviceId_ = ProtoPreconditions.CheckNotNull<string>(value, "value");
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
	public string Region
	{
		get
		{
			return region_;
		}
		set
		{
			region_ = ProtoPreconditions.CheckNotNull<string>(value, "value");
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public UserLoginReq()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public UserLoginReq(UserLoginReq other)
		: this()
	{
		platform_ = other.platform_;
		appChannel_ = other.appChannel_;
		loginChannel_ = other.loginChannel_;
		loginToken_ = other.loginToken_;
		deviceId_ = other.deviceId_;
		lang_ = other.lang_;
		region_ = other.region_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public UserLoginReq Clone()
	{
		return new UserLoginReq(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as UserLoginReq);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(UserLoginReq other)
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
		if (LoginChannel != other.LoginChannel)
		{
			return false;
		}
		if (LoginToken != other.LoginToken)
		{
			return false;
		}
		if (DeviceId != other.DeviceId)
		{
			return false;
		}
		if (Lang != other.Lang)
		{
			return false;
		}
		if (Region != other.Region)
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
		if (LoginChannel != EChannelType.EmptyValue)
		{
			num ^= LoginChannel.GetHashCode();
		}
		if (LoginToken.Length != 0)
		{
			num ^= LoginToken.GetHashCode();
		}
		if (DeviceId.Length != 0)
		{
			num ^= DeviceId.GetHashCode();
		}
		if (Lang != ELanguageType.EmptyValue)
		{
			num ^= Lang.GetHashCode();
		}
		if (Region.Length != 0)
		{
			num ^= Region.GetHashCode();
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
		if (LoginChannel != EChannelType.EmptyValue)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)24);
			((WriteContext)(ref output)).WriteEnum((int)LoginChannel);
		}
		if (LoginToken.Length != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)34);
			((WriteContext)(ref output)).WriteString(LoginToken);
		}
		if (DeviceId.Length != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)42);
			((WriteContext)(ref output)).WriteString(DeviceId);
		}
		if (Lang != ELanguageType.EmptyValue)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)48);
			((WriteContext)(ref output)).WriteEnum((int)Lang);
		}
		if (Region.Length != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)58);
			((WriteContext)(ref output)).WriteString(Region);
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
		if (LoginChannel != EChannelType.EmptyValue)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)LoginChannel);
		}
		if (LoginToken.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(LoginToken);
		}
		if (DeviceId.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(DeviceId);
		}
		if (Lang != ELanguageType.EmptyValue)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)Lang);
		}
		if (Region.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(Region);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(UserLoginReq other)
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
			if (other.LoginChannel != EChannelType.EmptyValue)
			{
				LoginChannel = other.LoginChannel;
			}
			if (other.LoginToken.Length != 0)
			{
				LoginToken = other.LoginToken;
			}
			if (other.DeviceId.Length != 0)
			{
				DeviceId = other.DeviceId;
			}
			if (other.Lang != ELanguageType.EmptyValue)
			{
				Lang = other.Lang;
			}
			if (other.Region.Length != 0)
			{
				Region = other.Region;
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
				LoginChannel = (EChannelType)((ParseContext)(ref input)).ReadEnum();
				break;
			case 34u:
				LoginToken = ((ParseContext)(ref input)).ReadString();
				break;
			case 42u:
				DeviceId = ((ParseContext)(ref input)).ReadString();
				break;
			case 48u:
				Lang = (ELanguageType)((ParseContext)(ref input)).ReadEnum();
				break;
			case 58u:
				Region = ((ParseContext)(ref input)).ReadString();
				break;
			}
		}
	}
}
