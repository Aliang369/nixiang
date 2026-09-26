using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class FriendsDetail : IMessage<FriendsDetail>, IMessage, IEquatable<FriendsDetail>, IDeepCloneable<FriendsDetail>, IBufferMessage
{
	private static readonly MessageParser<FriendsDetail> _parser = new MessageParser<FriendsDetail>((Func<FriendsDetail>)(() => new FriendsDetail()));

	private UnknownFieldSet _unknownFields;

	public const int RoleBaseInfoFieldNumber = 1;

	private RoleBaseInfo roleBaseInfo_;

	public const int RoleLoginInfoFieldNumber = 2;

	private RoleLoginInfo roleLoginInfo_;

	public const int ActionMeFieldNumber = 3;

	private EFriendActionCollectState actionMe_;

	public const int ActionHimFieldNumber = 4;

	private EBoolState actionHim_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<FriendsDetail> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeFriendsDetailReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RoleBaseInfo RoleBaseInfo
	{
		get
		{
			return roleBaseInfo_;
		}
		set
		{
			roleBaseInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RoleLoginInfo RoleLoginInfo
	{
		get
		{
			return roleLoginInfo_;
		}
		set
		{
			roleLoginInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public EFriendActionCollectState ActionMe
	{
		get
		{
			return actionMe_;
		}
		set
		{
			actionMe_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public EBoolState ActionHim
	{
		get
		{
			return actionHim_;
		}
		set
		{
			actionHim_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public FriendsDetail()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public FriendsDetail(FriendsDetail other)
		: this()
	{
		roleBaseInfo_ = ((other.roleBaseInfo_ != null) ? other.roleBaseInfo_.Clone() : null);
		roleLoginInfo_ = ((other.roleLoginInfo_ != null) ? other.roleLoginInfo_.Clone() : null);
		actionMe_ = other.actionMe_;
		actionHim_ = other.actionHim_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public FriendsDetail Clone()
	{
		return new FriendsDetail(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as FriendsDetail);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(FriendsDetail other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!object.Equals(RoleBaseInfo, other.RoleBaseInfo))
		{
			return false;
		}
		if (!object.Equals(RoleLoginInfo, other.RoleLoginInfo))
		{
			return false;
		}
		if (ActionMe != other.ActionMe)
		{
			return false;
		}
		if (ActionHim != other.ActionHim)
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
		if (roleBaseInfo_ != null)
		{
			num ^= RoleBaseInfo.GetHashCode();
		}
		if (roleLoginInfo_ != null)
		{
			num ^= RoleLoginInfo.GetHashCode();
		}
		if (ActionMe != EFriendActionCollectState.NotExist)
		{
			num ^= ActionMe.GetHashCode();
		}
		if (ActionHim != EBoolState.False)
		{
			num ^= ActionHim.GetHashCode();
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
		if (roleBaseInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)10);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)RoleBaseInfo);
		}
		if (roleLoginInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)18);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)RoleLoginInfo);
		}
		if (ActionMe != EFriendActionCollectState.NotExist)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)24);
			((WriteContext)(ref output)).WriteEnum((int)ActionMe);
		}
		if (ActionHim != EBoolState.False)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)32);
			((WriteContext)(ref output)).WriteEnum((int)ActionHim);
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
		if (roleBaseInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)RoleBaseInfo);
		}
		if (roleLoginInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)RoleLoginInfo);
		}
		if (ActionMe != EFriendActionCollectState.NotExist)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)ActionMe);
		}
		if (ActionHim != EBoolState.False)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)ActionHim);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(FriendsDetail other)
	{
		if (other == null)
		{
			return;
		}
		if (other.roleBaseInfo_ != null)
		{
			if (roleBaseInfo_ == null)
			{
				RoleBaseInfo = new RoleBaseInfo();
			}
			RoleBaseInfo.MergeFrom(other.RoleBaseInfo);
		}
		if (other.roleLoginInfo_ != null)
		{
			if (roleLoginInfo_ == null)
			{
				RoleLoginInfo = new RoleLoginInfo();
			}
			RoleLoginInfo.MergeFrom(other.RoleLoginInfo);
		}
		if (other.ActionMe != EFriendActionCollectState.NotExist)
		{
			ActionMe = other.ActionMe;
		}
		if (other.ActionHim != EBoolState.False)
		{
			ActionHim = other.ActionHim;
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
				if (roleBaseInfo_ == null)
				{
					RoleBaseInfo = new RoleBaseInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)RoleBaseInfo);
				break;
			case 18u:
				if (roleLoginInfo_ == null)
				{
					RoleLoginInfo = new RoleLoginInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)RoleLoginInfo);
				break;
			case 24u:
				ActionMe = (EFriendActionCollectState)((ParseContext)(ref input)).ReadEnum();
				break;
			case 32u:
				ActionHim = (EBoolState)((ParseContext)(ref input)).ReadEnum();
				break;
			}
		}
	}
}
