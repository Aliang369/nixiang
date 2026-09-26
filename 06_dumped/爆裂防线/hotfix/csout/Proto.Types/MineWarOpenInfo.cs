using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class MineWarOpenInfo : IMessage<MineWarOpenInfo>, IMessage, IEquatable<MineWarOpenInfo>, IDeepCloneable<MineWarOpenInfo>, IBufferMessage
{
	private static readonly MessageParser<MineWarOpenInfo> _parser = new MessageParser<MineWarOpenInfo>((Func<MineWarOpenInfo>)(() => new MineWarOpenInfo()));

	private UnknownFieldSet _unknownFields;

	public const int CrossOpenTmFieldNumber = 1;

	private long crossOpenTm_;

	public const int NextCrossStartFieldNumber = 2;

	private long nextCrossStart_;

	public const int NextCrossEndFieldNumber = 3;

	private long nextCrossEnd_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<MineWarOpenInfo> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeMineWarOpenInfoReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public long CrossOpenTm
	{
		get
		{
			return crossOpenTm_;
		}
		set
		{
			crossOpenTm_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public long NextCrossStart
	{
		get
		{
			return nextCrossStart_;
		}
		set
		{
			nextCrossStart_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public long NextCrossEnd
	{
		get
		{
			return nextCrossEnd_;
		}
		set
		{
			nextCrossEnd_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public MineWarOpenInfo()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public MineWarOpenInfo(MineWarOpenInfo other)
		: this()
	{
		crossOpenTm_ = other.crossOpenTm_;
		nextCrossStart_ = other.nextCrossStart_;
		nextCrossEnd_ = other.nextCrossEnd_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public MineWarOpenInfo Clone()
	{
		return new MineWarOpenInfo(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as MineWarOpenInfo);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(MineWarOpenInfo other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (CrossOpenTm != other.CrossOpenTm)
		{
			return false;
		}
		if (NextCrossStart != other.NextCrossStart)
		{
			return false;
		}
		if (NextCrossEnd != other.NextCrossEnd)
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
		if (CrossOpenTm != 0L)
		{
			num ^= CrossOpenTm.GetHashCode();
		}
		if (NextCrossStart != 0L)
		{
			num ^= NextCrossStart.GetHashCode();
		}
		if (NextCrossEnd != 0L)
		{
			num ^= NextCrossEnd.GetHashCode();
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
		if (CrossOpenTm != 0L)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteInt64(CrossOpenTm);
		}
		if (NextCrossStart != 0L)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteInt64(NextCrossStart);
		}
		if (NextCrossEnd != 0L)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)24);
			((WriteContext)(ref output)).WriteInt64(NextCrossEnd);
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
		if (CrossOpenTm != 0L)
		{
			num += 1 + CodedOutputStream.ComputeInt64Size(CrossOpenTm);
		}
		if (NextCrossStart != 0L)
		{
			num += 1 + CodedOutputStream.ComputeInt64Size(NextCrossStart);
		}
		if (NextCrossEnd != 0L)
		{
			num += 1 + CodedOutputStream.ComputeInt64Size(NextCrossEnd);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(MineWarOpenInfo other)
	{
		if (other != null)
		{
			if (other.CrossOpenTm != 0L)
			{
				CrossOpenTm = other.CrossOpenTm;
			}
			if (other.NextCrossStart != 0L)
			{
				NextCrossStart = other.NextCrossStart;
			}
			if (other.NextCrossEnd != 0L)
			{
				NextCrossEnd = other.NextCrossEnd;
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
				CrossOpenTm = ((ParseContext)(ref input)).ReadInt64();
				break;
			case 16u:
				NextCrossStart = ((ParseContext)(ref input)).ReadInt64();
				break;
			case 24u:
				NextCrossEnd = ((ParseContext)(ref input)).ReadInt64();
				break;
			}
		}
	}
}
