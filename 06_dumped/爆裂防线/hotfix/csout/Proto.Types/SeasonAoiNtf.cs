using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class SeasonAoiNtf : IMessage<SeasonAoiNtf>, IMessage, IEquatable<SeasonAoiNtf>, IDeepCloneable<SeasonAoiNtf>, IBufferMessage
{
	private static readonly MessageParser<SeasonAoiNtf> _parser = new MessageParser<SeasonAoiNtf>((Func<SeasonAoiNtf>)(() => new SeasonAoiNtf()));

	private UnknownFieldSet _unknownFields;

	public const int AppearListFieldNumber = 1;

	private static readonly FieldCodec<SeasonPlayerPos> _repeated_appearList_codec = FieldCodec.ForMessage<SeasonPlayerPos>(10u, SeasonPlayerPos.Parser);

	private readonly RepeatedField<SeasonPlayerPos> appearList_ = new RepeatedField<SeasonPlayerPos>();

	public const int DisappearListFieldNumber = 2;

	private static readonly FieldCodec<long> _repeated_disappearList_codec = FieldCodec.ForInt64(18u);

	private readonly RepeatedField<long> disappearList_ = new RepeatedField<long>();

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<SeasonAoiNtf> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeSeasonAoiNtfReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<SeasonPlayerPos> AppearList => appearList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<long> DisappearList => disappearList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SeasonAoiNtf()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SeasonAoiNtf(SeasonAoiNtf other)
		: this()
	{
		appearList_ = other.appearList_.Clone();
		disappearList_ = other.disappearList_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SeasonAoiNtf Clone()
	{
		return new SeasonAoiNtf(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as SeasonAoiNtf);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(SeasonAoiNtf other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!appearList_.Equals(other.appearList_))
		{
			return false;
		}
		if (!disappearList_.Equals(other.disappearList_))
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
		num ^= ((object)appearList_).GetHashCode();
		num ^= ((object)disappearList_).GetHashCode();
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
		appearList_.WriteTo(ref output, _repeated_appearList_codec);
		disappearList_.WriteTo(ref output, _repeated_disappearList_codec);
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
		num += appearList_.CalculateSize(_repeated_appearList_codec);
		num += disappearList_.CalculateSize(_repeated_disappearList_codec);
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(SeasonAoiNtf other)
	{
		if (other != null)
		{
			appearList_.Add((IEnumerable<SeasonPlayerPos>)other.appearList_);
			disappearList_.Add((IEnumerable<long>)other.disappearList_);
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
				appearList_.AddEntriesFrom(ref input, _repeated_appearList_codec);
				break;
			case 16u:
			case 18u:
				disappearList_.AddEntriesFrom(ref input, _repeated_disappearList_codec);
				break;
			}
		}
	}
}
