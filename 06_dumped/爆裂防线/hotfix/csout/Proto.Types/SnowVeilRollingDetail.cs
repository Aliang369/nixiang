using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class SnowVeilRollingDetail : IMessage<SnowVeilRollingDetail>, IMessage, IEquatable<SnowVeilRollingDetail>, IDeepCloneable<SnowVeilRollingDetail>, IBufferMessage
{
	private static readonly MessageParser<SnowVeilRollingDetail> _parser = new MessageParser<SnowVeilRollingDetail>((Func<SnowVeilRollingDetail>)(() => new SnowVeilRollingDetail()));

	private UnknownFieldSet _unknownFields;

	public const int SingleAwardFieldNumber = 1;

	private AwardDetail singleAward_;

	public const int RollingCountFieldNumber = 2;

	private int rollingCount_;

	public const int RollingStateFieldNumber = 3;

	private static readonly FieldCodec<int> _repeated_rollingState_codec = FieldCodec.ForInt32(26u);

	private readonly RepeatedField<int> rollingState_ = new RepeatedField<int>();

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<SnowVeilRollingDetail> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeSnowVeilRollingDetailReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public AwardDetail SingleAward
	{
		get
		{
			return singleAward_;
		}
		set
		{
			singleAward_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int RollingCount
	{
		get
		{
			return rollingCount_;
		}
		set
		{
			rollingCount_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<int> RollingState => rollingState_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SnowVeilRollingDetail()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SnowVeilRollingDetail(SnowVeilRollingDetail other)
		: this()
	{
		singleAward_ = ((other.singleAward_ != null) ? other.singleAward_.Clone() : null);
		rollingCount_ = other.rollingCount_;
		rollingState_ = other.rollingState_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SnowVeilRollingDetail Clone()
	{
		return new SnowVeilRollingDetail(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as SnowVeilRollingDetail);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(SnowVeilRollingDetail other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!object.Equals(SingleAward, other.SingleAward))
		{
			return false;
		}
		if (RollingCount != other.RollingCount)
		{
			return false;
		}
		if (!rollingState_.Equals(other.rollingState_))
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
		if (singleAward_ != null)
		{
			num ^= SingleAward.GetHashCode();
		}
		if (RollingCount != 0)
		{
			num ^= RollingCount.GetHashCode();
		}
		num ^= ((object)rollingState_).GetHashCode();
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
		if (singleAward_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)10);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)SingleAward);
		}
		if (RollingCount != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteInt32(RollingCount);
		}
		rollingState_.WriteTo(ref output, _repeated_rollingState_codec);
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
		if (singleAward_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)SingleAward);
		}
		if (RollingCount != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(RollingCount);
		}
		num += rollingState_.CalculateSize(_repeated_rollingState_codec);
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(SnowVeilRollingDetail other)
	{
		if (other == null)
		{
			return;
		}
		if (other.singleAward_ != null)
		{
			if (singleAward_ == null)
			{
				SingleAward = new AwardDetail();
			}
			SingleAward.MergeFrom(other.SingleAward);
		}
		if (other.RollingCount != 0)
		{
			RollingCount = other.RollingCount;
		}
		rollingState_.Add((IEnumerable<int>)other.rollingState_);
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
				if (singleAward_ == null)
				{
					SingleAward = new AwardDetail();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)SingleAward);
				break;
			case 16u:
				RollingCount = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 24u:
			case 26u:
				rollingState_.AddEntriesFrom(ref input, _repeated_rollingState_codec);
				break;
			}
		}
	}
}
