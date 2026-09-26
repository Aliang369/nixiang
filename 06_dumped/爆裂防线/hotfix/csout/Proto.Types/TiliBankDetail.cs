using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class TiliBankDetail : IMessage<TiliBankDetail>, IMessage, IEquatable<TiliBankDetail>, IDeepCloneable<TiliBankDetail>, IBufferMessage
{
	private static readonly MessageParser<TiliBankDetail> _parser = new MessageParser<TiliBankDetail>((Func<TiliBankDetail>)(() => new TiliBankDetail()));

	private UnknownFieldSet _unknownFields;

	public const int TiliTmFieldNumber = 1;

	private long tiliTm_;

	public const int TiliCountFieldNumber = 2;

	private int tiliCount_;

	public const int StateFieldNumber = 3;

	private ECommonState state_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<TiliBankDetail> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeTiliBankDetailReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public long TiliTm
	{
		get
		{
			return tiliTm_;
		}
		set
		{
			tiliTm_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int TiliCount
	{
		get
		{
			return tiliCount_;
		}
		set
		{
			tiliCount_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ECommonState State
	{
		get
		{
			return state_;
		}
		set
		{
			state_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public TiliBankDetail()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public TiliBankDetail(TiliBankDetail other)
		: this()
	{
		tiliTm_ = other.tiliTm_;
		tiliCount_ = other.tiliCount_;
		state_ = other.state_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public TiliBankDetail Clone()
	{
		return new TiliBankDetail(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as TiliBankDetail);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(TiliBankDetail other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (TiliTm != other.TiliTm)
		{
			return false;
		}
		if (TiliCount != other.TiliCount)
		{
			return false;
		}
		if (State != other.State)
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
		if (TiliTm != 0L)
		{
			num ^= TiliTm.GetHashCode();
		}
		if (TiliCount != 0)
		{
			num ^= TiliCount.GetHashCode();
		}
		if (State != ECommonState.No)
		{
			num ^= State.GetHashCode();
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
		if (TiliTm != 0L)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteInt64(TiliTm);
		}
		if (TiliCount != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteInt32(TiliCount);
		}
		if (State != ECommonState.No)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)24);
			((WriteContext)(ref output)).WriteEnum((int)State);
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
		if (TiliTm != 0L)
		{
			num += 1 + CodedOutputStream.ComputeInt64Size(TiliTm);
		}
		if (TiliCount != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(TiliCount);
		}
		if (State != ECommonState.No)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)State);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(TiliBankDetail other)
	{
		if (other != null)
		{
			if (other.TiliTm != 0L)
			{
				TiliTm = other.TiliTm;
			}
			if (other.TiliCount != 0)
			{
				TiliCount = other.TiliCount;
			}
			if (other.State != ECommonState.No)
			{
				State = other.State;
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
				TiliTm = ((ParseContext)(ref input)).ReadInt64();
				break;
			case 16u:
				TiliCount = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 24u:
				State = (ECommonState)((ParseContext)(ref input)).ReadEnum();
				break;
			}
		}
	}
}
