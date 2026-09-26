using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class PvpModInfo : IMessage<PvpModInfo>, IMessage, IEquatable<PvpModInfo>, IDeepCloneable<PvpModInfo>, IBufferMessage
{
	private static readonly MessageParser<PvpModInfo> _parser = new MessageParser<PvpModInfo>((Func<PvpModInfo>)(() => new PvpModInfo()));

	private UnknownFieldSet _unknownFields;

	public const int SkillListFieldNumber = 1;

	private static readonly FieldCodec<SkillDetail> _repeated_skillList_codec = FieldCodec.ForMessage<SkillDetail>(10u, SkillDetail.Parser);

	private readonly RepeatedField<SkillDetail> skillList_ = new RepeatedField<SkillDetail>();

	public const int ModListFieldNumber = 2;

	private static readonly FieldCodec<int> _repeated_modList_codec = FieldCodec.ForInt32(18u);

	private readonly RepeatedField<int> modList_ = new RepeatedField<int>();

	public const int SelectListFieldNumber = 3;

	private static readonly FieldCodec<int> _repeated_selectList_codec = FieldCodec.ForInt32(26u);

	private readonly RepeatedField<int> selectList_ = new RepeatedField<int>();

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<PvpModInfo> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypePvpModInfoReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<SkillDetail> SkillList => skillList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<int> ModList => modList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<int> SelectList => selectList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public PvpModInfo()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public PvpModInfo(PvpModInfo other)
		: this()
	{
		skillList_ = other.skillList_.Clone();
		modList_ = other.modList_.Clone();
		selectList_ = other.selectList_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public PvpModInfo Clone()
	{
		return new PvpModInfo(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as PvpModInfo);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(PvpModInfo other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!skillList_.Equals(other.skillList_))
		{
			return false;
		}
		if (!modList_.Equals(other.modList_))
		{
			return false;
		}
		if (!selectList_.Equals(other.selectList_))
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
		num ^= ((object)skillList_).GetHashCode();
		num ^= ((object)modList_).GetHashCode();
		num ^= ((object)selectList_).GetHashCode();
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
		skillList_.WriteTo(ref output, _repeated_skillList_codec);
		modList_.WriteTo(ref output, _repeated_modList_codec);
		selectList_.WriteTo(ref output, _repeated_selectList_codec);
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
		num += skillList_.CalculateSize(_repeated_skillList_codec);
		num += modList_.CalculateSize(_repeated_modList_codec);
		num += selectList_.CalculateSize(_repeated_selectList_codec);
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(PvpModInfo other)
	{
		if (other != null)
		{
			skillList_.Add((IEnumerable<SkillDetail>)other.skillList_);
			modList_.Add((IEnumerable<int>)other.modList_);
			selectList_.Add((IEnumerable<int>)other.selectList_);
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
				skillList_.AddEntriesFrom(ref input, _repeated_skillList_codec);
				break;
			case 16u:
			case 18u:
				modList_.AddEntriesFrom(ref input, _repeated_modList_codec);
				break;
			case 24u:
			case 26u:
				selectList_.AddEntriesFrom(ref input, _repeated_selectList_codec);
				break;
			}
		}
	}
}
