using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class TreasureInfo : IMessage<TreasureInfo>, IMessage, IEquatable<TreasureInfo>, IDeepCloneable<TreasureInfo>, IBufferMessage
{
	private static readonly MessageParser<TreasureInfo> _parser = new MessageParser<TreasureInfo>((Func<TreasureInfo>)(() => new TreasureInfo()));

	private UnknownFieldSet _unknownFields;

	public const int TreasureListFieldNumber = 1;

	private static readonly FieldCodec<TreasureDetail> _repeated_treasureList_codec = FieldCodec.ForMessage<TreasureDetail>(10u, TreasureDetail.Parser);

	private readonly RepeatedField<TreasureDetail> treasureList_ = new RepeatedField<TreasureDetail>();

	public const int AttrListFieldNumber = 2;

	private static readonly FieldCodec<BattleTeamAttrDetail> _repeated_attrList_codec = FieldCodec.ForMessage<BattleTeamAttrDetail>(18u, BattleTeamAttrDetail.Parser);

	private readonly RepeatedField<BattleTeamAttrDetail> attrList_ = new RepeatedField<BattleTeamAttrDetail>();

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<TreasureInfo> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeTreasureInfoReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<TreasureDetail> TreasureList => treasureList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<BattleTeamAttrDetail> AttrList => attrList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public TreasureInfo()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public TreasureInfo(TreasureInfo other)
		: this()
	{
		treasureList_ = other.treasureList_.Clone();
		attrList_ = other.attrList_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public TreasureInfo Clone()
	{
		return new TreasureInfo(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as TreasureInfo);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(TreasureInfo other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!treasureList_.Equals(other.treasureList_))
		{
			return false;
		}
		if (!attrList_.Equals(other.attrList_))
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
		num ^= ((object)treasureList_).GetHashCode();
		num ^= ((object)attrList_).GetHashCode();
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
		treasureList_.WriteTo(ref output, _repeated_treasureList_codec);
		attrList_.WriteTo(ref output, _repeated_attrList_codec);
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
		num += treasureList_.CalculateSize(_repeated_treasureList_codec);
		num += attrList_.CalculateSize(_repeated_attrList_codec);
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(TreasureInfo other)
	{
		if (other != null)
		{
			treasureList_.Add((IEnumerable<TreasureDetail>)other.treasureList_);
			attrList_.Add((IEnumerable<BattleTeamAttrDetail>)other.attrList_);
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
				treasureList_.AddEntriesFrom(ref input, _repeated_treasureList_codec);
				break;
			case 18u:
				attrList_.AddEntriesFrom(ref input, _repeated_attrList_codec);
				break;
			}
		}
	}
}
