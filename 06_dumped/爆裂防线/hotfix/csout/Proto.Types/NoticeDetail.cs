using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class NoticeDetail : IMessage<NoticeDetail>, IMessage, IEquatable<NoticeDetail>, IDeepCloneable<NoticeDetail>, IBufferMessage
{
	private static readonly MessageParser<NoticeDetail> _parser = new MessageParser<NoticeDetail>((Func<NoticeDetail>)(() => new NoticeDetail()));

	private UnknownFieldSet _unknownFields;

	public const int IdFieldNumber = 1;

	private string id_ = "";

	public const int SortIdFieldNumber = 2;

	private int sortId_;

	public const int TitleFieldNumber = 3;

	private string title_ = "";

	public const int SubTitleFieldNumber = 4;

	private string subTitle_ = "";

	public const int ContentFieldNumber = 5;

	private string content_ = "";

	public const int ExpireTmFieldNumber = 6;

	private long expireTm_;

	public const int LinkFieldNumber = 7;

	private string link_ = "";

	public const int StatusFieldNumber = 8;

	private ENoticeState status_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<NoticeDetail> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeNoticeDetailReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public string Id
	{
		get
		{
			return id_;
		}
		set
		{
			id_ = ProtoPreconditions.CheckNotNull<string>(value, "value");
		}
	}

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
	public string SubTitle
	{
		get
		{
			return subTitle_;
		}
		set
		{
			subTitle_ = ProtoPreconditions.CheckNotNull<string>(value, "value");
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
	public long ExpireTm
	{
		get
		{
			return expireTm_;
		}
		set
		{
			expireTm_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public string Link
	{
		get
		{
			return link_;
		}
		set
		{
			link_ = ProtoPreconditions.CheckNotNull<string>(value, "value");
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ENoticeState Status
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
	public NoticeDetail()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public NoticeDetail(NoticeDetail other)
		: this()
	{
		id_ = other.id_;
		sortId_ = other.sortId_;
		title_ = other.title_;
		subTitle_ = other.subTitle_;
		content_ = other.content_;
		expireTm_ = other.expireTm_;
		link_ = other.link_;
		status_ = other.status_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public NoticeDetail Clone()
	{
		return new NoticeDetail(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as NoticeDetail);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(NoticeDetail other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (Id != other.Id)
		{
			return false;
		}
		if (SortId != other.SortId)
		{
			return false;
		}
		if (Title != other.Title)
		{
			return false;
		}
		if (SubTitle != other.SubTitle)
		{
			return false;
		}
		if (Content != other.Content)
		{
			return false;
		}
		if (ExpireTm != other.ExpireTm)
		{
			return false;
		}
		if (Link != other.Link)
		{
			return false;
		}
		if (Status != other.Status)
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
		if (Id.Length != 0)
		{
			num ^= Id.GetHashCode();
		}
		if (SortId != 0)
		{
			num ^= SortId.GetHashCode();
		}
		if (Title.Length != 0)
		{
			num ^= Title.GetHashCode();
		}
		if (SubTitle.Length != 0)
		{
			num ^= SubTitle.GetHashCode();
		}
		if (Content.Length != 0)
		{
			num ^= Content.GetHashCode();
		}
		if (ExpireTm != 0L)
		{
			num ^= ExpireTm.GetHashCode();
		}
		if (Link.Length != 0)
		{
			num ^= Link.GetHashCode();
		}
		if (Status != ENoticeState.EmptyValue)
		{
			num ^= Status.GetHashCode();
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
		if (Id.Length != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)10);
			((WriteContext)(ref output)).WriteString(Id);
		}
		if (SortId != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteInt32(SortId);
		}
		if (Title.Length != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)26);
			((WriteContext)(ref output)).WriteString(Title);
		}
		if (SubTitle.Length != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)34);
			((WriteContext)(ref output)).WriteString(SubTitle);
		}
		if (Content.Length != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)42);
			((WriteContext)(ref output)).WriteString(Content);
		}
		if (ExpireTm != 0L)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)48);
			((WriteContext)(ref output)).WriteInt64(ExpireTm);
		}
		if (Link.Length != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)58);
			((WriteContext)(ref output)).WriteString(Link);
		}
		if (Status != ENoticeState.EmptyValue)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)64);
			((WriteContext)(ref output)).WriteEnum((int)Status);
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
		if (Id.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(Id);
		}
		if (SortId != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(SortId);
		}
		if (Title.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(Title);
		}
		if (SubTitle.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(SubTitle);
		}
		if (Content.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(Content);
		}
		if (ExpireTm != 0L)
		{
			num += 1 + CodedOutputStream.ComputeInt64Size(ExpireTm);
		}
		if (Link.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(Link);
		}
		if (Status != ENoticeState.EmptyValue)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)Status);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(NoticeDetail other)
	{
		if (other != null)
		{
			if (other.Id.Length != 0)
			{
				Id = other.Id;
			}
			if (other.SortId != 0)
			{
				SortId = other.SortId;
			}
			if (other.Title.Length != 0)
			{
				Title = other.Title;
			}
			if (other.SubTitle.Length != 0)
			{
				SubTitle = other.SubTitle;
			}
			if (other.Content.Length != 0)
			{
				Content = other.Content;
			}
			if (other.ExpireTm != 0L)
			{
				ExpireTm = other.ExpireTm;
			}
			if (other.Link.Length != 0)
			{
				Link = other.Link;
			}
			if (other.Status != ENoticeState.EmptyValue)
			{
				Status = other.Status;
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
			case 10u:
				Id = ((ParseContext)(ref input)).ReadString();
				break;
			case 16u:
				SortId = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 26u:
				Title = ((ParseContext)(ref input)).ReadString();
				break;
			case 34u:
				SubTitle = ((ParseContext)(ref input)).ReadString();
				break;
			case 42u:
				Content = ((ParseContext)(ref input)).ReadString();
				break;
			case 48u:
				ExpireTm = ((ParseContext)(ref input)).ReadInt64();
				break;
			case 58u:
				Link = ((ParseContext)(ref input)).ReadString();
				break;
			case 64u:
				Status = (ENoticeState)((ParseContext)(ref input)).ReadEnum();
				break;
			}
		}
	}
}
