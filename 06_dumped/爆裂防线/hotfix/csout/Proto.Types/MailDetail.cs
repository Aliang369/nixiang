using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class MailDetail : IMessage<MailDetail>, IMessage, IEquatable<MailDetail>, IDeepCloneable<MailDetail>, IBufferMessage
{
	private static readonly MessageParser<MailDetail> _parser = new MessageParser<MailDetail>((Func<MailDetail>)(() => new MailDetail()));

	private UnknownFieldSet _unknownFields;

	public const int MailIdFieldNumber = 1;

	private long mailId_;

	public const int MailLevelFieldNumber = 2;

	private EMailLevel mailLevel_;

	public const int UUIDFieldNumber = 3;

	private string uUID_ = "";

	public const int SenderIdFieldNumber = 4;

	private int senderId_;

	public const int SenderNameFieldNumber = 5;

	private string senderName_ = "";

	public const int TitleFieldNumber = 6;

	private string title_ = "";

	public const int ReadStateFieldNumber = 7;

	private EBoolState readState_;

	public const int AwardStateFieldNumber = 8;

	private EBoolState awardState_;

	public const int AwardListFieldNumber = 9;

	private static readonly FieldCodec<AwardDetail> _repeated_awardList_codec = FieldCodec.ForMessage<AwardDetail>(74u, AwardDetail.Parser);

	private readonly RepeatedField<AwardDetail> awardList_ = new RepeatedField<AwardDetail>();

	public const int SendTimeFieldNumber = 10;

	private long sendTime_;

	public const int ExpireTimeFieldNumber = 11;

	private long expireTime_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<MailDetail> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeMailDetailReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public long MailId
	{
		get
		{
			return mailId_;
		}
		set
		{
			mailId_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public EMailLevel MailLevel
	{
		get
		{
			return mailLevel_;
		}
		set
		{
			mailLevel_ = value;
		}
	}

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
	public int SenderId
	{
		get
		{
			return senderId_;
		}
		set
		{
			senderId_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public string SenderName
	{
		get
		{
			return senderName_;
		}
		set
		{
			senderName_ = ProtoPreconditions.CheckNotNull<string>(value, "value");
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public string Title
	{
		get
		{
			return title_;
		}
		set
		{
			title_ = ProtoPreconditions.CheckNotNull<string>(value, "value");
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public EBoolState ReadState
	{
		get
		{
			return readState_;
		}
		set
		{
			readState_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public EBoolState AwardState
	{
		get
		{
			return awardState_;
		}
		set
		{
			awardState_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<AwardDetail> AwardList => awardList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public long SendTime
	{
		get
		{
			return sendTime_;
		}
		set
		{
			sendTime_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public long ExpireTime
	{
		get
		{
			return expireTime_;
		}
		set
		{
			expireTime_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public MailDetail()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public MailDetail(MailDetail other)
		: this()
	{
		mailId_ = other.mailId_;
		mailLevel_ = other.mailLevel_;
		uUID_ = other.uUID_;
		senderId_ = other.senderId_;
		senderName_ = other.senderName_;
		title_ = other.title_;
		readState_ = other.readState_;
		awardState_ = other.awardState_;
		awardList_ = other.awardList_.Clone();
		sendTime_ = other.sendTime_;
		expireTime_ = other.expireTime_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public MailDetail Clone()
	{
		return new MailDetail(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as MailDetail);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(MailDetail other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (MailId != other.MailId)
		{
			return false;
		}
		if (MailLevel != other.MailLevel)
		{
			return false;
		}
		if (UUID != other.UUID)
		{
			return false;
		}
		if (SenderId != other.SenderId)
		{
			return false;
		}
		if (SenderName != other.SenderName)
		{
			return false;
		}
		if (Title != other.Title)
		{
			return false;
		}
		if (ReadState != other.ReadState)
		{
			return false;
		}
		if (AwardState != other.AwardState)
		{
			return false;
		}
		if (!awardList_.Equals(other.awardList_))
		{
			return false;
		}
		if (SendTime != other.SendTime)
		{
			return false;
		}
		if (ExpireTime != other.ExpireTime)
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
		if (MailId != 0L)
		{
			num ^= MailId.GetHashCode();
		}
		if (MailLevel != EMailLevel.EmptyValue)
		{
			num ^= MailLevel.GetHashCode();
		}
		if (UUID.Length != 0)
		{
			num ^= UUID.GetHashCode();
		}
		if (SenderId != 0)
		{
			num ^= SenderId.GetHashCode();
		}
		if (SenderName.Length != 0)
		{
			num ^= SenderName.GetHashCode();
		}
		if (Title.Length != 0)
		{
			num ^= Title.GetHashCode();
		}
		if (ReadState != EBoolState.False)
		{
			num ^= ReadState.GetHashCode();
		}
		if (AwardState != EBoolState.False)
		{
			num ^= AwardState.GetHashCode();
		}
		num ^= ((object)awardList_).GetHashCode();
		if (SendTime != 0L)
		{
			num ^= SendTime.GetHashCode();
		}
		if (ExpireTime != 0L)
		{
			num ^= ExpireTime.GetHashCode();
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
		if (MailId != 0L)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteInt64(MailId);
		}
		if (MailLevel != EMailLevel.EmptyValue)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteEnum((int)MailLevel);
		}
		if (UUID.Length != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)26);
			((WriteContext)(ref output)).WriteString(UUID);
		}
		if (SenderId != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)32);
			((WriteContext)(ref output)).WriteInt32(SenderId);
		}
		if (SenderName.Length != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)42);
			((WriteContext)(ref output)).WriteString(SenderName);
		}
		if (Title.Length != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)50);
			((WriteContext)(ref output)).WriteString(Title);
		}
		if (ReadState != EBoolState.False)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)56);
			((WriteContext)(ref output)).WriteEnum((int)ReadState);
		}
		if (AwardState != EBoolState.False)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)64);
			((WriteContext)(ref output)).WriteEnum((int)AwardState);
		}
		awardList_.WriteTo(ref output, _repeated_awardList_codec);
		if (SendTime != 0L)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)80);
			((WriteContext)(ref output)).WriteInt64(SendTime);
		}
		if (ExpireTime != 0L)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)88);
			((WriteContext)(ref output)).WriteInt64(ExpireTime);
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
		if (MailId != 0L)
		{
			num += 1 + CodedOutputStream.ComputeInt64Size(MailId);
		}
		if (MailLevel != EMailLevel.EmptyValue)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)MailLevel);
		}
		if (UUID.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(UUID);
		}
		if (SenderId != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(SenderId);
		}
		if (SenderName.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(SenderName);
		}
		if (Title.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(Title);
		}
		if (ReadState != EBoolState.False)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)ReadState);
		}
		if (AwardState != EBoolState.False)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)AwardState);
		}
		num += awardList_.CalculateSize(_repeated_awardList_codec);
		if (SendTime != 0L)
		{
			num += 1 + CodedOutputStream.ComputeInt64Size(SendTime);
		}
		if (ExpireTime != 0L)
		{
			num += 1 + CodedOutputStream.ComputeInt64Size(ExpireTime);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(MailDetail other)
	{
		if (other != null)
		{
			if (other.MailId != 0L)
			{
				MailId = other.MailId;
			}
			if (other.MailLevel != EMailLevel.EmptyValue)
			{
				MailLevel = other.MailLevel;
			}
			if (other.UUID.Length != 0)
			{
				UUID = other.UUID;
			}
			if (other.SenderId != 0)
			{
				SenderId = other.SenderId;
			}
			if (other.SenderName.Length != 0)
			{
				SenderName = other.SenderName;
			}
			if (other.Title.Length != 0)
			{
				Title = other.Title;
			}
			if (other.ReadState != EBoolState.False)
			{
				ReadState = other.ReadState;
			}
			if (other.AwardState != EBoolState.False)
			{
				AwardState = other.AwardState;
			}
			awardList_.Add((IEnumerable<AwardDetail>)other.awardList_);
			if (other.SendTime != 0L)
			{
				SendTime = other.SendTime;
			}
			if (other.ExpireTime != 0L)
			{
				ExpireTime = other.ExpireTime;
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
				MailId = ((ParseContext)(ref input)).ReadInt64();
				break;
			case 16u:
				MailLevel = (EMailLevel)((ParseContext)(ref input)).ReadEnum();
				break;
			case 26u:
				UUID = ((ParseContext)(ref input)).ReadString();
				break;
			case 32u:
				SenderId = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 42u:
				SenderName = ((ParseContext)(ref input)).ReadString();
				break;
			case 50u:
				Title = ((ParseContext)(ref input)).ReadString();
				break;
			case 56u:
				ReadState = (EBoolState)((ParseContext)(ref input)).ReadEnum();
				break;
			case 64u:
				AwardState = (EBoolState)((ParseContext)(ref input)).ReadEnum();
				break;
			case 74u:
				awardList_.AddEntriesFrom(ref input, _repeated_awardList_codec);
				break;
			case 80u:
				SendTime = ((ParseContext)(ref input)).ReadInt64();
				break;
			case 88u:
				ExpireTime = ((ParseContext)(ref input)).ReadInt64();
				break;
			}
		}
	}
}
