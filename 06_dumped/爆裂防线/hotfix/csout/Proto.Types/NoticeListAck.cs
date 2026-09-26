using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class NoticeListAck : IMessage<NoticeListAck>, IMessage, IEquatable<NoticeListAck>, IDeepCloneable<NoticeListAck>, IBufferMessage
{
	private static readonly MessageParser<NoticeListAck> _parser = new MessageParser<NoticeListAck>((Func<NoticeListAck>)(() => new NoticeListAck()));

	private UnknownFieldSet _unknownFields;

	public const int GameEnvFieldNumber = 1;

	private GameEnv gameEnv_;

	public const int NoticeListFieldNumber = 2;

	private static readonly FieldCodec<NoticeDetail> _repeated_noticeList_codec = FieldCodec.ForMessage<NoticeDetail>(18u, NoticeDetail.Parser);

	private readonly RepeatedField<NoticeDetail> noticeList_ = new RepeatedField<NoticeDetail>();

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<NoticeListAck> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeNoticeListAckReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public GameEnv GameEnv
	{
		get
		{
			return gameEnv_;
		}
		set
		{
			gameEnv_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<NoticeDetail> NoticeList => noticeList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public NoticeListAck()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public NoticeListAck(NoticeListAck other)
		: this()
	{
		gameEnv_ = ((other.gameEnv_ != null) ? other.gameEnv_.Clone() : null);
		noticeList_ = other.noticeList_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public NoticeListAck Clone()
	{
		return new NoticeListAck(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as NoticeListAck);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(NoticeListAck other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!object.Equals(GameEnv, other.GameEnv))
		{
			return false;
		}
		if (!noticeList_.Equals(other.noticeList_))
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
		if (gameEnv_ != null)
		{
			num ^= GameEnv.GetHashCode();
		}
		num ^= ((object)noticeList_).GetHashCode();
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
		if (gameEnv_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)10);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)GameEnv);
		}
		noticeList_.WriteTo(ref output, _repeated_noticeList_codec);
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
		if (gameEnv_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)GameEnv);
		}
		num += noticeList_.CalculateSize(_repeated_noticeList_codec);
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(NoticeListAck other)
	{
		if (other == null)
		{
			return;
		}
		if (other.gameEnv_ != null)
		{
			if (gameEnv_ == null)
			{
				GameEnv = new GameEnv();
			}
			GameEnv.MergeFrom(other.GameEnv);
		}
		noticeList_.Add((IEnumerable<NoticeDetail>)other.noticeList_);
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
				if (gameEnv_ == null)
				{
					GameEnv = new GameEnv();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)GameEnv);
				break;
			case 18u:
				noticeList_.AddEntriesFrom(ref input, _repeated_noticeList_codec);
				break;
			}
		}
	}
}
