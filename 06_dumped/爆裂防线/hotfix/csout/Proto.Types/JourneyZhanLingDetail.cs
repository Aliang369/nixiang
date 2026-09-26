using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class JourneyZhanLingDetail : IMessage<JourneyZhanLingDetail>, IMessage, IEquatable<JourneyZhanLingDetail>, IDeepCloneable<JourneyZhanLingDetail>, IBufferMessage
{
	private static readonly MessageParser<JourneyZhanLingDetail> _parser = new MessageParser<JourneyZhanLingDetail>((Func<JourneyZhanLingDetail>)(() => new JourneyZhanLingDetail()));

	private UnknownFieldSet _unknownFields;

	public const int LevelFieldNumber = 1;

	private int level_;

	public const int ReachFieldNumber = 2;

	private int reach_;

	public const int LowAwardFieldNumber = 3;

	private int lowAward_;

	public const int ActiveListFieldNumber = 4;

	private static readonly FieldCodec<JourneyZhanLingActiveDetail> _repeated_activeList_codec = FieldCodec.ForMessage<JourneyZhanLingActiveDetail>(34u, JourneyZhanLingActiveDetail.Parser);

	private readonly RepeatedField<JourneyZhanLingActiveDetail> activeList_ = new RepeatedField<JourneyZhanLingActiveDetail>();

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<JourneyZhanLingDetail> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeJourneyZhanLingDetailReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int Level
	{
		get
		{
			return level_;
		}
		set
		{
			level_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int Reach
	{
		get
		{
			return reach_;
		}
		set
		{
			reach_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int LowAward
	{
		get
		{
			return lowAward_;
		}
		set
		{
			lowAward_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<JourneyZhanLingActiveDetail> ActiveList => activeList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public JourneyZhanLingDetail()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public JourneyZhanLingDetail(JourneyZhanLingDetail other)
		: this()
	{
		level_ = other.level_;
		reach_ = other.reach_;
		lowAward_ = other.lowAward_;
		activeList_ = other.activeList_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public JourneyZhanLingDetail Clone()
	{
		return new JourneyZhanLingDetail(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as JourneyZhanLingDetail);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(JourneyZhanLingDetail other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (Level != other.Level)
		{
			return false;
		}
		if (Reach != other.Reach)
		{
			return false;
		}
		if (LowAward != other.LowAward)
		{
			return false;
		}
		if (!activeList_.Equals(other.activeList_))
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
		if (Level != 0)
		{
			num ^= Level.GetHashCode();
		}
		if (Reach != 0)
		{
			num ^= Reach.GetHashCode();
		}
		if (LowAward != 0)
		{
			num ^= LowAward.GetHashCode();
		}
		num ^= ((object)activeList_).GetHashCode();
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
		if (Level != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteInt32(Level);
		}
		if (Reach != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteInt32(Reach);
		}
		if (LowAward != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)24);
			((WriteContext)(ref output)).WriteInt32(LowAward);
		}
		activeList_.WriteTo(ref output, _repeated_activeList_codec);
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
		if (Level != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(Level);
		}
		if (Reach != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(Reach);
		}
		if (LowAward != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(LowAward);
		}
		num += activeList_.CalculateSize(_repeated_activeList_codec);
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(JourneyZhanLingDetail other)
	{
		if (other != null)
		{
			if (other.Level != 0)
			{
				Level = other.Level;
			}
			if (other.Reach != 0)
			{
				Reach = other.Reach;
			}
			if (other.LowAward != 0)
			{
				LowAward = other.LowAward;
			}
			activeList_.Add((IEnumerable<JourneyZhanLingActiveDetail>)other.activeList_);
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
				Level = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 16u:
				Reach = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 24u:
				LowAward = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 34u:
				activeList_.AddEntriesFrom(ref input, _repeated_activeList_codec);
				break;
			}
		}
	}
}
