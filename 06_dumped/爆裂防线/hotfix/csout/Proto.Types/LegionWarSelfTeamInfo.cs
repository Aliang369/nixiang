using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class LegionWarSelfTeamInfo : IMessage<LegionWarSelfTeamInfo>, IMessage, IEquatable<LegionWarSelfTeamInfo>, IDeepCloneable<LegionWarSelfTeamInfo>, IBufferMessage
{
	private static readonly MessageParser<LegionWarSelfTeamInfo> _parser = new MessageParser<LegionWarSelfTeamInfo>((Func<LegionWarSelfTeamInfo>)(() => new LegionWarSelfTeamInfo()));

	private UnknownFieldSet _unknownFields;

	public const int SkillListFieldNumber = 1;

	private static readonly FieldCodec<SkillDetail> _repeated_skillList_codec = FieldCodec.ForMessage<SkillDetail>(10u, SkillDetail.Parser);

	private readonly RepeatedField<SkillDetail> skillList_ = new RepeatedField<SkillDetail>();

	public const int ModListFieldNumber = 2;

	private static readonly FieldCodec<int> _repeated_modList_codec = FieldCodec.ForInt32(18u);

	private readonly RepeatedField<int> modList_ = new RepeatedField<int>();

	public const int ModSelectListFieldNumber = 3;

	private static readonly FieldCodec<int> _repeated_modSelectList_codec = FieldCodec.ForInt32(26u);

	private readonly RepeatedField<int> modSelectList_ = new RepeatedField<int>();

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<LegionWarSelfTeamInfo> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeLegionWarSelfTeamInfoReflection.Descriptor.MessageTypes[0];

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
	public RepeatedField<int> ModSelectList => modSelectList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public LegionWarSelfTeamInfo()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public LegionWarSelfTeamInfo(LegionWarSelfTeamInfo other)
		: this()
	{
		skillList_ = other.skillList_.Clone();
		modList_ = other.modList_.Clone();
		modSelectList_ = other.modSelectList_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public LegionWarSelfTeamInfo Clone()
	{
		return new LegionWarSelfTeamInfo(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as LegionWarSelfTeamInfo);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(LegionWarSelfTeamInfo other)
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
		if (!modSelectList_.Equals(other.modSelectList_))
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
		num ^= ((object)modSelectList_).GetHashCode();
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
		modSelectList_.WriteTo(ref output, _repeated_modSelectList_codec);
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
		num += modSelectList_.CalculateSize(_repeated_modSelectList_codec);
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(LegionWarSelfTeamInfo other)
	{
		if (other != null)
		{
			skillList_.Add((IEnumerable<SkillDetail>)other.skillList_);
			modList_.Add((IEnumerable<int>)other.modList_);
			modSelectList_.Add((IEnumerable<int>)other.modSelectList_);
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
				modSelectList_.AddEntriesFrom(ref input, _repeated_modSelectList_codec);
				break;
			}
		}
	}
}
