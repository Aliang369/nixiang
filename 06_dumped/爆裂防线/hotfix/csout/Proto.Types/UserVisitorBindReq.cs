using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class UserVisitorBindReq : IMessage<UserVisitorBindReq>, IMessage, IEquatable<UserVisitorBindReq>, IDeepCloneable<UserVisitorBindReq>, IBufferMessage
{
	private static readonly MessageParser<UserVisitorBindReq> _parser = new MessageParser<UserVisitorBindReq>((Func<UserVisitorBindReq>)(() => new UserVisitorBindReq()));

	private UnknownFieldSet _unknownFields;

	public const int PlatformFieldNumber = 1;

	private EPlatformType platform_;

	public const int AppChannelFieldNumber = 2;

	private EChannelType appChannel_;

	public const int VisitorTokenFieldNumber = 3;

	private string visitorToken_ = "";

	public const int BindChannelFieldNumber = 4;

	private EChannelType bindChannel_;

	public const int BindTokenFieldNumber = 5;

	private string bindToken_ = "";

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<UserVisitorBindReq> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeUserVisitorBindReqReflection.Descriptor.MessageTypes[0];

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
	public string VisitorToken
	{
		get
		{
			return visitorToken_;
		}
		set
		{
			visitorToken_ = ProtoPreconditions.CheckNotNull<string>(value, "value");
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public EChannelType BindChannel
	{
		get
		{
			return bindChannel_;
		}
		set
		{
			bindChannel_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public string BindToken
	{
		get
		{
			return bindToken_;
		}
		set
		{
			bindToken_ = ProtoPreconditions.CheckNotNull<string>(value, "value");
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public UserVisitorBindReq()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public UserVisitorBindReq(UserVisitorBindReq other)
		: this()
	{
		platform_ = other.platform_;
		appChannel_ = other.appChannel_;
		visitorToken_ = other.visitorToken_;
		bindChannel_ = other.bindChannel_;
		bindToken_ = other.bindToken_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public UserVisitorBindReq Clone()
	{
		return new UserVisitorBindReq(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as UserVisitorBindReq);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(UserVisitorBindReq other)
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
		if (VisitorToken != other.VisitorToken)
		{
			return false;
		}
		if (BindChannel != other.BindChannel)
		{
			return false;
		}
		if (BindToken != other.BindToken)
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
		if (VisitorToken.Length != 0)
		{
			num ^= VisitorToken.GetHashCode();
		}
		if (BindChannel != EChannelType.EmptyValue)
		{
			num ^= BindChannel.GetHashCode();
		}
		if (BindToken.Length != 0)
		{
			num ^= BindToken.GetHashCode();
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
		if (VisitorToken.Length != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)26);
			((WriteContext)(ref output)).WriteString(VisitorToken);
		}
		if (BindChannel != EChannelType.EmptyValue)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)32);
			((WriteContext)(ref output)).WriteEnum((int)BindChannel);
		}
		if (BindToken.Length != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)42);
			((WriteContext)(ref output)).WriteString(BindToken);
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
		if (VisitorToken.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(VisitorToken);
		}
		if (BindChannel != EChannelType.EmptyValue)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)BindChannel);
		}
		if (BindToken.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(BindToken);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(UserVisitorBindReq other)
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
			if (other.VisitorToken.Length != 0)
			{
				VisitorToken = other.VisitorToken;
			}
			if (other.BindChannel != EChannelType.EmptyValue)
			{
				BindChannel = other.BindChannel;
			}
			if (other.BindToken.Length != 0)
			{
				BindToken = other.BindToken;
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
			case 26u:
				VisitorToken = ((ParseContext)(ref input)).ReadString();
				break;
			case 32u:
				BindChannel = (EChannelType)((ParseContext)(ref input)).ReadEnum();
				break;
			case 42u:
				BindToken = ((ParseContext)(ref input)).ReadString();
				break;
			}
		}
	}
}
