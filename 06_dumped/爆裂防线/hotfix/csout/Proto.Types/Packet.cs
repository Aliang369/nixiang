using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class Packet : IMessage<Packet>, IMessage, IEquatable<Packet>, IDeepCloneable<Packet>, IBufferMessage
{
	private static readonly MessageParser<Packet> _parser = new MessageParser<Packet>((Func<Packet>)(() => new Packet()));

	private UnknownFieldSet _unknownFields;

	public const int MsgIdFieldNumber = 1;

	private int msgId_;

	public const int VerFieldNumber = 2;

	private int ver_;

	public const int TmFieldNumber = 3;

	private long tm_;

	public const int SeqFieldNumber = 4;

	private int seq_;

	public const int DataFieldNumber = 5;

	private ByteString data_ = ByteString.Empty;

	public const int SignFieldNumber = 6;

	private string sign_ = "";

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<Packet> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypePacketReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int MsgId
	{
		get
		{
			return msgId_;
		}
		set
		{
			msgId_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int Ver
	{
		get
		{
			return ver_;
		}
		set
		{
			ver_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public long Tm
	{
		get
		{
			return tm_;
		}
		set
		{
			tm_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int Seq
	{
		get
		{
			return seq_;
		}
		set
		{
			seq_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ByteString Data
	{
		get
		{
			return data_;
		}
		set
		{
			data_ = ProtoPreconditions.CheckNotNull<ByteString>(value, "value");
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public string Sign
	{
		get
		{
			return sign_;
		}
		set
		{
			sign_ = ProtoPreconditions.CheckNotNull<string>(value, "value");
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public Packet()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public Packet(Packet other)
		: this()
	{
		msgId_ = other.msgId_;
		ver_ = other.ver_;
		tm_ = other.tm_;
		seq_ = other.seq_;
		data_ = other.data_;
		sign_ = other.sign_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public Packet Clone()
	{
		return new Packet(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as Packet);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(Packet other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (MsgId != other.MsgId)
		{
			return false;
		}
		if (Ver != other.Ver)
		{
			return false;
		}
		if (Tm != other.Tm)
		{
			return false;
		}
		if (Seq != other.Seq)
		{
			return false;
		}
		if (Data != other.Data)
		{
			return false;
		}
		if (Sign != other.Sign)
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
		if (MsgId != 0)
		{
			num ^= MsgId.GetHashCode();
		}
		if (Ver != 0)
		{
			num ^= Ver.GetHashCode();
		}
		if (Tm != 0L)
		{
			num ^= Tm.GetHashCode();
		}
		if (Seq != 0)
		{
			num ^= Seq.GetHashCode();
		}
		if (Data.Length != 0)
		{
			num ^= ((object)Data).GetHashCode();
		}
		if (Sign.Length != 0)
		{
			num ^= Sign.GetHashCode();
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
		if (MsgId != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteInt32(MsgId);
		}
		if (Ver != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteInt32(Ver);
		}
		if (Tm != 0L)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)24);
			((WriteContext)(ref output)).WriteInt64(Tm);
		}
		if (Seq != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)32);
			((WriteContext)(ref output)).WriteInt32(Seq);
		}
		if (Data.Length != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)42);
			((WriteContext)(ref output)).WriteBytes(Data);
		}
		if (Sign.Length != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)50);
			((WriteContext)(ref output)).WriteString(Sign);
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
		if (MsgId != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(MsgId);
		}
		if (Ver != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(Ver);
		}
		if (Tm != 0L)
		{
			num += 1 + CodedOutputStream.ComputeInt64Size(Tm);
		}
		if (Seq != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(Seq);
		}
		if (Data.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeBytesSize(Data);
		}
		if (Sign.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(Sign);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(Packet other)
	{
		if (other != null)
		{
			if (other.MsgId != 0)
			{
				MsgId = other.MsgId;
			}
			if (other.Ver != 0)
			{
				Ver = other.Ver;
			}
			if (other.Tm != 0L)
			{
				Tm = other.Tm;
			}
			if (other.Seq != 0)
			{
				Seq = other.Seq;
			}
			if (other.Data.Length != 0)
			{
				Data = other.Data;
			}
			if (other.Sign.Length != 0)
			{
				Sign = other.Sign;
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
				MsgId = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 16u:
				Ver = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 24u:
				Tm = ((ParseContext)(ref input)).ReadInt64();
				break;
			case 32u:
				Seq = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 42u:
				Data = ((ParseContext)(ref input)).ReadBytes();
				break;
			case 50u:
				Sign = ((ParseContext)(ref input)).ReadString();
				break;
			}
		}
	}
}
