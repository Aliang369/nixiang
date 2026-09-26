using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class RoleTipsSuitInfo : IMessage<RoleTipsSuitInfo>, IMessage, IEquatable<RoleTipsSuitInfo>, IDeepCloneable<RoleTipsSuitInfo>, IBufferMessage
{
	private static readonly MessageParser<RoleTipsSuitInfo> _parser = new MessageParser<RoleTipsSuitInfo>((Func<RoleTipsSuitInfo>)(() => new RoleTipsSuitInfo()));

	private UnknownFieldSet _unknownFields;

	public const int SuitListFieldNumber = 1;

	private static readonly FieldCodec<PlayerSuitDetail> _repeated_suitList_codec = FieldCodec.ForMessage<PlayerSuitDetail>(10u, PlayerSuitDetail.Parser);

	private readonly RepeatedField<PlayerSuitDetail> suitList_ = new RepeatedField<PlayerSuitDetail>();

	public const int SuperSuitListFieldNumber = 2;

	private static readonly FieldCodec<PlayerSuitDetail> _repeated_superSuitList_codec = FieldCodec.ForMessage<PlayerSuitDetail>(18u, PlayerSuitDetail.Parser);

	private readonly RepeatedField<PlayerSuitDetail> superSuitList_ = new RepeatedField<PlayerSuitDetail>();

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<RoleTipsSuitInfo> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeRoleTipsSuitInfoReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<PlayerSuitDetail> SuitList => suitList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<PlayerSuitDetail> SuperSuitList => superSuitList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RoleTipsSuitInfo()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RoleTipsSuitInfo(RoleTipsSuitInfo other)
		: this()
	{
		suitList_ = other.suitList_.Clone();
		superSuitList_ = other.superSuitList_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RoleTipsSuitInfo Clone()
	{
		return new RoleTipsSuitInfo(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as RoleTipsSuitInfo);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(RoleTipsSuitInfo other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!suitList_.Equals(other.suitList_))
		{
			return false;
		}
		if (!superSuitList_.Equals(other.superSuitList_))
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
		num ^= ((object)suitList_).GetHashCode();
		num ^= ((object)superSuitList_).GetHashCode();
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
		suitList_.WriteTo(ref output, _repeated_suitList_codec);
		superSuitList_.WriteTo(ref output, _repeated_superSuitList_codec);
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
		num += suitList_.CalculateSize(_repeated_suitList_codec);
		num += superSuitList_.CalculateSize(_repeated_superSuitList_codec);
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(RoleTipsSuitInfo other)
	{
		if (other != null)
		{
			suitList_.Add((IEnumerable<PlayerSuitDetail>)other.suitList_);
			superSuitList_.Add((IEnumerable<PlayerSuitDetail>)other.superSuitList_);
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
				suitList_.AddEntriesFrom(ref input, _repeated_suitList_codec);
				break;
			case 18u:
				superSuitList_.AddEntriesFrom(ref input, _repeated_superSuitList_codec);
				break;
			}
		}
	}
}
