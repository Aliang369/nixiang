using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class WorldResIdleInfo : IMessage<WorldResIdleInfo>, IMessage, IEquatable<WorldResIdleInfo>, IDeepCloneable<WorldResIdleInfo>, IBufferMessage
{
	private static readonly MessageParser<WorldResIdleInfo> _parser = new MessageParser<WorldResIdleInfo>((Func<WorldResIdleInfo>)(() => new WorldResIdleInfo()));

	private UnknownFieldSet _unknownFields;

	public const int SettleMaxFieldNumber = 1;

	private int settleMax_;

	public const int SettleSecondFieldNumber = 2;

	private int settleSecond_;

	public const int GainTmFieldNumber = 3;

	private long gainTm_;

	public const int BagAwardListFieldNumber = 4;

	private static readonly FieldCodec<AwardDetail> _repeated_bagAwardList_codec = FieldCodec.ForMessage<AwardDetail>(34u, AwardDetail.Parser);

	private readonly RepeatedField<AwardDetail> bagAwardList_ = new RepeatedField<AwardDetail>();

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<WorldResIdleInfo> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeWorldResIdleInfoReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int SettleMax
	{
		get
		{
			return settleMax_;
		}
		set
		{
			settleMax_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int SettleSecond
	{
		get
		{
			return settleSecond_;
		}
		set
		{
			settleSecond_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public long GainTm
	{
		get
		{
			return gainTm_;
		}
		set
		{
			gainTm_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<AwardDetail> BagAwardList => bagAwardList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public WorldResIdleInfo()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public WorldResIdleInfo(WorldResIdleInfo other)
		: this()
	{
		settleMax_ = other.settleMax_;
		settleSecond_ = other.settleSecond_;
		gainTm_ = other.gainTm_;
		bagAwardList_ = other.bagAwardList_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public WorldResIdleInfo Clone()
	{
		return new WorldResIdleInfo(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as WorldResIdleInfo);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(WorldResIdleInfo other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (SettleMax != other.SettleMax)
		{
			return false;
		}
		if (SettleSecond != other.SettleSecond)
		{
			return false;
		}
		if (GainTm != other.GainTm)
		{
			return false;
		}
		if (!bagAwardList_.Equals(other.bagAwardList_))
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
		if (SettleMax != 0)
		{
			num ^= SettleMax.GetHashCode();
		}
		if (SettleSecond != 0)
		{
			num ^= SettleSecond.GetHashCode();
		}
		if (GainTm != 0L)
		{
			num ^= GainTm.GetHashCode();
		}
		num ^= ((object)bagAwardList_).GetHashCode();
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
		if (SettleMax != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteInt32(SettleMax);
		}
		if (SettleSecond != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteInt32(SettleSecond);
		}
		if (GainTm != 0L)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)24);
			((WriteContext)(ref output)).WriteInt64(GainTm);
		}
		bagAwardList_.WriteTo(ref output, _repeated_bagAwardList_codec);
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
		if (SettleMax != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(SettleMax);
		}
		if (SettleSecond != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(SettleSecond);
		}
		if (GainTm != 0L)
		{
			num += 1 + CodedOutputStream.ComputeInt64Size(GainTm);
		}
		num += bagAwardList_.CalculateSize(_repeated_bagAwardList_codec);
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(WorldResIdleInfo other)
	{
		if (other != null)
		{
			if (other.SettleMax != 0)
			{
				SettleMax = other.SettleMax;
			}
			if (other.SettleSecond != 0)
			{
				SettleSecond = other.SettleSecond;
			}
			if (other.GainTm != 0L)
			{
				GainTm = other.GainTm;
			}
			bagAwardList_.Add((IEnumerable<AwardDetail>)other.bagAwardList_);
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
				SettleMax = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 16u:
				SettleSecond = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 24u:
				GainTm = ((ParseContext)(ref input)).ReadInt64();
				break;
			case 34u:
				bagAwardList_.AddEntriesFrom(ref input, _repeated_bagAwardList_codec);
				break;
			}
		}
	}
}
