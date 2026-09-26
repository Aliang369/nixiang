using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class RoleTeamInfo : IMessage<RoleTeamInfo>, IMessage, IEquatable<RoleTeamInfo>, IDeepCloneable<RoleTeamInfo>, IBufferMessage
{
	private static readonly MessageParser<RoleTeamInfo> _parser = new MessageParser<RoleTeamInfo>((Func<RoleTeamInfo>)(() => new RoleTeamInfo()));

	private UnknownFieldSet _unknownFields;

	public const int AttrListFieldNumber = 1;

	private static readonly FieldCodec<BattleTeamAttrDetail> _repeated_attrList_codec = FieldCodec.ForMessage<BattleTeamAttrDetail>(10u, BattleTeamAttrDetail.Parser);

	private readonly RepeatedField<BattleTeamAttrDetail> attrList_ = new RepeatedField<BattleTeamAttrDetail>();

	public const int ModListFieldNumber = 2;

	private static readonly FieldCodec<int> _repeated_modList_codec = FieldCodec.ForInt32(18u);

	private readonly RepeatedField<int> modList_ = new RepeatedField<int>();

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<RoleTeamInfo> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeRoleTeamInfoReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<BattleTeamAttrDetail> AttrList => attrList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<int> ModList => modList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RoleTeamInfo()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RoleTeamInfo(RoleTeamInfo other)
		: this()
	{
		attrList_ = other.attrList_.Clone();
		modList_ = other.modList_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RoleTeamInfo Clone()
	{
		return new RoleTeamInfo(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as RoleTeamInfo);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(RoleTeamInfo other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!attrList_.Equals(other.attrList_))
		{
			return false;
		}
		if (!modList_.Equals(other.modList_))
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
		num ^= ((object)attrList_).GetHashCode();
		num ^= ((object)modList_).GetHashCode();
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
		attrList_.WriteTo(ref output, _repeated_attrList_codec);
		modList_.WriteTo(ref output, _repeated_modList_codec);
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
		num += attrList_.CalculateSize(_repeated_attrList_codec);
		num += modList_.CalculateSize(_repeated_modList_codec);
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(RoleTeamInfo other)
	{
		if (other != null)
		{
			attrList_.Add((IEnumerable<BattleTeamAttrDetail>)other.attrList_);
			modList_.Add((IEnumerable<int>)other.modList_);
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
				attrList_.AddEntriesFrom(ref input, _repeated_attrList_codec);
				break;
			case 16u:
			case 18u:
				modList_.AddEntriesFrom(ref input, _repeated_modList_codec);
				break;
			}
		}
	}
}
