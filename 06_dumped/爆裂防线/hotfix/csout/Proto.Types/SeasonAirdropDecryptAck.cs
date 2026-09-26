using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class SeasonAirdropDecryptAck : IMessage<SeasonAirdropDecryptAck>, IMessage, IEquatable<SeasonAirdropDecryptAck>, IDeepCloneable<SeasonAirdropDecryptAck>, IBufferMessage
{
	private static readonly MessageParser<SeasonAirdropDecryptAck> _parser = new MessageParser<SeasonAirdropDecryptAck>((Func<SeasonAirdropDecryptAck>)(() => new SeasonAirdropDecryptAck()));

	private UnknownFieldSet _unknownFields;

	public const int InfoFieldNumber = 1;

	private SeasonAirdropInfo info_;

	public const int ProgFieldNumber = 2;

	private SeasonAirdropProg prog_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<SeasonAirdropDecryptAck> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeSeasonAirdropDecryptAckReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SeasonAirdropInfo Info
	{
		get
		{
			return info_;
		}
		set
		{
			info_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SeasonAirdropProg Prog
	{
		get
		{
			return prog_;
		}
		set
		{
			prog_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SeasonAirdropDecryptAck()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SeasonAirdropDecryptAck(SeasonAirdropDecryptAck other)
		: this()
	{
		info_ = ((other.info_ != null) ? other.info_.Clone() : null);
		prog_ = ((other.prog_ != null) ? other.prog_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SeasonAirdropDecryptAck Clone()
	{
		return new SeasonAirdropDecryptAck(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as SeasonAirdropDecryptAck);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(SeasonAirdropDecryptAck other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!object.Equals(Info, other.Info))
		{
			return false;
		}
		if (!object.Equals(Prog, other.Prog))
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
		if (info_ != null)
		{
			num ^= Info.GetHashCode();
		}
		if (prog_ != null)
		{
			num ^= Prog.GetHashCode();
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
		if (info_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)10);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)Info);
		}
		if (prog_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)18);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)Prog);
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
		if (info_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)Info);
		}
		if (prog_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)Prog);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(SeasonAirdropDecryptAck other)
	{
		if (other == null)
		{
			return;
		}
		if (other.info_ != null)
		{
			if (info_ == null)
			{
				Info = new SeasonAirdropInfo();
			}
			Info.MergeFrom(other.Info);
		}
		if (other.prog_ != null)
		{
			if (prog_ == null)
			{
				Prog = new SeasonAirdropProg();
			}
			Prog.MergeFrom(other.Prog);
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
				if (info_ == null)
				{
					Info = new SeasonAirdropInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)Info);
				break;
			case 18u:
				if (prog_ == null)
				{
					Prog = new SeasonAirdropProg();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)Prog);
				break;
			}
		}
	}
}
