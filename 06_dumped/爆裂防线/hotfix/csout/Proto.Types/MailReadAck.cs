using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class MailReadAck : IMessage<MailReadAck>, IMessage, IEquatable<MailReadAck>, IDeepCloneable<MailReadAck>, IBufferMessage
{
	private static readonly MessageParser<MailReadAck> _parser = new MessageParser<MailReadAck>((Func<MailReadAck>)(() => new MailReadAck()));

	private UnknownFieldSet _unknownFields;

	public const int MailFieldNumber = 1;

	private MailDetail mail_;

	public const int ContentFieldNumber = 2;

	private MailContent content_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<MailReadAck> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeMailReadAckReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public MailDetail Mail
	{
		get
		{
			return mail_;
		}
		set
		{
			mail_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public MailContent Content
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
	public MailReadAck()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public MailReadAck(MailReadAck other)
		: this()
	{
		mail_ = ((other.mail_ != null) ? other.mail_.Clone() : null);
		content_ = ((other.content_ != null) ? other.content_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public MailReadAck Clone()
	{
		return new MailReadAck(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as MailReadAck);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(MailReadAck other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!object.Equals(Mail, other.Mail))
		{
			return false;
		}
		if (!object.Equals(Content, other.Content))
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
		if (mail_ != null)
		{
			num ^= Mail.GetHashCode();
		}
		if (content_ != null)
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
		if (mail_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)10);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)Mail);
		}
		if (content_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)18);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)Content);
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
		if (mail_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)Mail);
		}
		if (content_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)Content);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(MailReadAck other)
	{
		if (other == null)
		{
			return;
		}
		if (other.mail_ != null)
		{
			if (mail_ == null)
			{
				Mail = new MailDetail();
			}
			Mail.MergeFrom(other.Mail);
		}
		if (other.content_ != null)
		{
			if (content_ == null)
			{
				Content = new MailContent();
			}
			Content.MergeFrom(other.Content);
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
				if (mail_ == null)
				{
					Mail = new MailDetail();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)Mail);
				break;
			case 18u:
				if (content_ == null)
				{
					Content = new MailContent();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)Content);
				break;
			}
		}
	}
}
