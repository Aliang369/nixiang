using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class SettleRankInfo : IMessage<SettleRankInfo>, IMessage, IEquatable<SettleRankInfo>, IDeepCloneable<SettleRankInfo>, IBufferMessage
{
	private static readonly MessageParser<SettleRankInfo> _parser = new MessageParser<SettleRankInfo>((Func<SettleRankInfo>)(() => new SettleRankInfo()));

	private UnknownFieldSet _unknownFields;

	public const int OldRankFieldNumber = 1;

	private int oldRank_;

	public const int NewRankFieldNumber = 2;

	private int newRank_;

	public const int OldBestRankFieldNumber = 3;

	private int oldBestRank_;

	public const int NewBestRankFieldNumber = 4;

	private int newBestRank_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<SettleRankInfo> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeSettleRankInfoReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int OldRank
	{
		get
		{
			return oldRank_;
		}
		set
		{
			oldRank_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int NewRank
	{
		get
		{
			return newRank_;
		}
		set
		{
			newRank_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int OldBestRank
	{
		get
		{
			return oldBestRank_;
		}
		set
		{
			oldBestRank_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int NewBestRank
	{
		get
		{
			return newBestRank_;
		}
		set
		{
			newBestRank_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SettleRankInfo()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SettleRankInfo(SettleRankInfo other)
		: this()
	{
		oldRank_ = other.oldRank_;
		newRank_ = other.newRank_;
		oldBestRank_ = other.oldBestRank_;
		newBestRank_ = other.newBestRank_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SettleRankInfo Clone()
	{
		return new SettleRankInfo(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as SettleRankInfo);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(SettleRankInfo other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (OldRank != other.OldRank)
		{
			return false;
		}
		if (NewRank != other.NewRank)
		{
			return false;
		}
		if (OldBestRank != other.OldBestRank)
		{
			return false;
		}
		if (NewBestRank != other.NewBestRank)
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
		if (OldRank != 0)
		{
			num ^= OldRank.GetHashCode();
		}
		if (NewRank != 0)
		{
			num ^= NewRank.GetHashCode();
		}
		if (OldBestRank != 0)
		{
			num ^= OldBestRank.GetHashCode();
		}
		if (NewBestRank != 0)
		{
			num ^= NewBestRank.GetHashCode();
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
		if (OldRank != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteInt32(OldRank);
		}
		if (NewRank != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteInt32(NewRank);
		}
		if (OldBestRank != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)24);
			((WriteContext)(ref output)).WriteInt32(OldBestRank);
		}
		if (NewBestRank != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)32);
			((WriteContext)(ref output)).WriteInt32(NewBestRank);
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
		if (OldRank != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(OldRank);
		}
		if (NewRank != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(NewRank);
		}
		if (OldBestRank != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(OldBestRank);
		}
		if (NewBestRank != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(NewBestRank);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(SettleRankInfo other)
	{
		if (other != null)
		{
			if (other.OldRank != 0)
			{
				OldRank = other.OldRank;
			}
			if (other.NewRank != 0)
			{
				NewRank = other.NewRank;
			}
			if (other.OldBestRank != 0)
			{
				OldBestRank = other.OldBestRank;
			}
			if (other.NewBestRank != 0)
			{
				NewBestRank = other.NewBestRank;
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
				OldRank = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 16u:
				NewRank = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 24u:
				OldBestRank = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 32u:
				NewBestRank = ((ParseContext)(ref input)).ReadInt32();
				break;
			}
		}
	}
}
