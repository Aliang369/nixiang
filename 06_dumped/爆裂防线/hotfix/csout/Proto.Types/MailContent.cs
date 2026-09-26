using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class MailContent : IMessage<MailContent>, IMessage, IEquatable<MailContent>, IDeepCloneable<MailContent>, IBufferMessage
{
	private static readonly MessageParser<MailContent> _parser = new MessageParser<MailContent>((Func<MailContent>)(() => new MailContent()));

	private UnknownFieldSet _unknownFields;

	public const int MailIdFieldNumber = 1;

	private long mailId_;

	public const int ContentFieldNumber = 2;

	private ContentDetail content_;

	public const int LinkFieldNumber = 3;

	private string link_ = "";

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<MailContent> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeMailContentReflection.Descriptor.MessageTypes[0];

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
	public ContentDetail Content
	{
		get
		{
			return content_;
		}
		set
		{
			content_ = value;
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
	public MailContent()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public MailContent(MailContent other)
		: this()
	{
		mailId_ = other.mailId_;
		content_ = ((other.content_ != null) ? other.content_.Clone() : null);
		link_ = other.link_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public MailContent Clone()
	{
		return new MailContent(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as MailContent);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(MailContent other)
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
		if (!object.Equals(Content, other.Content))
		{
			return false;
		}
		if (Link != other.Link)
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
		if (content_ != null)
		{
			num ^= Content.GetHashCode();
		}
		if (Link.Length != 0)
		{
			num ^= Link.GetHashCode();
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
		if (content_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)18);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)Content);
		}
		if (Link.Length != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)26);
			((WriteContext)(ref output)).WriteString(Link);
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
		if (content_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)Content);
		}
		if (Link.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(Link);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(MailContent other)
	{
		if (other == null)
		{
			return;
		}
		if (other.MailId != 0L)
		{
			MailId = other.MailId;
		}
		if (other.content_ != null)
		{
			if (content_ == null)
			{
				Content = new ContentDetail();
			}
			Content.MergeFrom(other.Content);
		}
		if (other.Link.Length != 0)
		{
			Link = other.Link;
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
			case 8u:
				MailId = ((ParseContext)(ref input)).ReadInt64();
				break;
			case 18u:
				if (content_ == null)
				{
					Content = new ContentDetail();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)Content);
				break;
			case 26u:
				Link = ((ParseContext)(ref input)).ReadString();
				break;
			}
		}
	}
}
