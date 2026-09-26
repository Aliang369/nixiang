using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class HandbookInfo : IMessage<HandbookInfo>, IMessage, IEquatable<HandbookInfo>, IDeepCloneable<HandbookInfo>, IBufferMessage
{
	private static readonly MessageParser<HandbookInfo> _parser = new MessageParser<HandbookInfo>((Func<HandbookInfo>)(() => new HandbookInfo()));

	private UnknownFieldSet _unknownFields;

	public const int MonsterListFieldNumber = 1;

	private static readonly FieldCodec<MonsterHandbookDetail> _repeated_monsterList_codec = FieldCodec.ForMessage<MonsterHandbookDetail>(10u, MonsterHandbookDetail.Parser);

	private readonly RepeatedField<MonsterHandbookDetail> monsterList_ = new RepeatedField<MonsterHandbookDetail>();

	public const int ModHandbookInfoFieldNumber = 2;

	private ModHandbookInfo modHandbookInfo_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<HandbookInfo> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeHandbookInfoReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<MonsterHandbookDetail> MonsterList => monsterList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ModHandbookInfo ModHandbookInfo
	{
		get
		{
			return modHandbookInfo_;
		}
		set
		{
			modHandbookInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public HandbookInfo()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public HandbookInfo(HandbookInfo other)
		: this()
	{
		monsterList_ = other.monsterList_.Clone();
		modHandbookInfo_ = ((other.modHandbookInfo_ != null) ? other.modHandbookInfo_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public HandbookInfo Clone()
	{
		return new HandbookInfo(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as HandbookInfo);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(HandbookInfo other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!monsterList_.Equals(other.monsterList_))
		{
			return false;
		}
		if (!object.Equals(ModHandbookInfo, other.ModHandbookInfo))
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
		num ^= ((object)monsterList_).GetHashCode();
		if (modHandbookInfo_ != null)
		{
			num ^= ModHandbookInfo.GetHashCode();
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
		monsterList_.WriteTo(ref output, _repeated_monsterList_codec);
		if (modHandbookInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)18);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)ModHandbookInfo);
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
		num += monsterList_.CalculateSize(_repeated_monsterList_codec);
		if (modHandbookInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)ModHandbookInfo);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(HandbookInfo other)
	{
		if (other == null)
		{
			return;
		}
		monsterList_.Add((IEnumerable<MonsterHandbookDetail>)other.monsterList_);
		if (other.modHandbookInfo_ != null)
		{
			if (modHandbookInfo_ == null)
			{
				ModHandbookInfo = new ModHandbookInfo();
			}
			ModHandbookInfo.MergeFrom(other.ModHandbookInfo);
		}
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
				monsterList_.AddEntriesFrom(ref input, _repeated_monsterList_codec);
				break;
			case 18u:
				if (modHandbookInfo_ == null)
				{
					ModHandbookInfo = new ModHandbookInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)ModHandbookInfo);
				break;
			}
		}
	}
}
