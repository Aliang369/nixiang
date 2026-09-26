using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class PrayLanternUltimateDetail : IMessage<PrayLanternUltimateDetail>, IMessage, IEquatable<PrayLanternUltimateDetail>, IDeepCloneable<PrayLanternUltimateDetail>, IBufferMessage
{
	private static readonly MessageParser<PrayLanternUltimateDetail> _parser = new MessageParser<PrayLanternUltimateDetail>((Func<PrayLanternUltimateDetail>)(() => new PrayLanternUltimateDetail()));

	private UnknownFieldSet _unknownFields;

	public const int UltimateAwardListFieldNumber = 1;

	private static readonly FieldCodec<UltimateAwardDetail> _repeated_ultimateAwardList_codec = FieldCodec.ForMessage<UltimateAwardDetail>(10u, UltimateAwardDetail.Parser);

	private readonly RepeatedField<UltimateAwardDetail> ultimateAwardList_ = new RepeatedField<UltimateAwardDetail>();

	public const int StateFieldNumber = 2;

	private ECommonState state_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<PrayLanternUltimateDetail> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypePrayLanternUltimateDetailReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<UltimateAwardDetail> UltimateAwardList => ultimateAwardList_;

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
	public PrayLanternUltimateDetail()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public PrayLanternUltimateDetail(PrayLanternUltimateDetail other)
		: this()
	{
		ultimateAwardList_ = other.ultimateAwardList_.Clone();
		state_ = other.state_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public PrayLanternUltimateDetail Clone()
	{
		return new PrayLanternUltimateDetail(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as PrayLanternUltimateDetail);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(PrayLanternUltimateDetail other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!ultimateAwardList_.Equals(other.ultimateAwardList_))
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
		num ^= ((object)ultimateAwardList_).GetHashCode();
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
		ultimateAwardList_.WriteTo(ref output, _repeated_ultimateAwardList_codec);
		if (State != ECommonState.No)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
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
		num += ultimateAwardList_.CalculateSize(_repeated_ultimateAwardList_codec);
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
	public void MergeFrom(PrayLanternUltimateDetail other)
	{
		if (other != null)
		{
			ultimateAwardList_.Add((IEnumerable<UltimateAwardDetail>)other.ultimateAwardList_);
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
			case 10u:
				ultimateAwardList_.AddEntriesFrom(ref input, _repeated_ultimateAwardList_codec);
				break;
			case 16u:
				State = (ECommonState)((ParseContext)(ref input)).ReadEnum();
				break;
			}
		}
	}
}
