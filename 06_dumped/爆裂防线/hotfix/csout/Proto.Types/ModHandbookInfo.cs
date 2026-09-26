using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class ModHandbookInfo : IMessage<ModHandbookInfo>, IMessage, IEquatable<ModHandbookInfo>, IDeepCloneable<ModHandbookInfo>, IBufferMessage
{
	private static readonly MessageParser<ModHandbookInfo> _parser = new MessageParser<ModHandbookInfo>((Func<ModHandbookInfo>)(() => new ModHandbookInfo()));

	private UnknownFieldSet _unknownFields;

	public const int ModListFieldNumber = 1;

	private static readonly FieldCodec<ModHandbookDetail> _repeated_modList_codec = FieldCodec.ForMessage<ModHandbookDetail>(10u, ModHandbookDetail.Parser);

	private readonly RepeatedField<ModHandbookDetail> modList_ = new RepeatedField<ModHandbookDetail>();

	public const int TypeTargetListFieldNumber = 2;

	private static readonly FieldCodec<ModHandbookTypeTarget> _repeated_typeTargetList_codec = FieldCodec.ForMessage<ModHandbookTypeTarget>(18u, ModHandbookTypeTarget.Parser);

	private readonly RepeatedField<ModHandbookTypeTarget> typeTargetList_ = new RepeatedField<ModHandbookTypeTarget>();

	public const int TargetInfoFieldNumber = 3;

	private ModHandbookTarget targetInfo_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<ModHandbookInfo> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeModHandbookInfoReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<ModHandbookDetail> ModList => modList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<ModHandbookTypeTarget> TypeTargetList => typeTargetList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ModHandbookTarget TargetInfo
	{
		get
		{
			return targetInfo_;
		}
		set
		{
			targetInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ModHandbookInfo()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ModHandbookInfo(ModHandbookInfo other)
		: this()
	{
		modList_ = other.modList_.Clone();
		typeTargetList_ = other.typeTargetList_.Clone();
		targetInfo_ = ((other.targetInfo_ != null) ? other.targetInfo_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ModHandbookInfo Clone()
	{
		return new ModHandbookInfo(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as ModHandbookInfo);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(ModHandbookInfo other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!modList_.Equals(other.modList_))
		{
			return false;
		}
		if (!typeTargetList_.Equals(other.typeTargetList_))
		{
			return false;
		}
		if (!object.Equals(TargetInfo, other.TargetInfo))
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
		num ^= ((object)modList_).GetHashCode();
		num ^= ((object)typeTargetList_).GetHashCode();
		if (targetInfo_ != null)
		{
			num ^= TargetInfo.GetHashCode();
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
		modList_.WriteTo(ref output, _repeated_modList_codec);
		typeTargetList_.WriteTo(ref output, _repeated_typeTargetList_codec);
		if (targetInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)26);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)TargetInfo);
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
		num += modList_.CalculateSize(_repeated_modList_codec);
		num += typeTargetList_.CalculateSize(_repeated_typeTargetList_codec);
		if (targetInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)TargetInfo);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(ModHandbookInfo other)
	{
		if (other == null)
		{
			return;
		}
		modList_.Add((IEnumerable<ModHandbookDetail>)other.modList_);
		typeTargetList_.Add((IEnumerable<ModHandbookTypeTarget>)other.typeTargetList_);
		if (other.targetInfo_ != null)
		{
			if (targetInfo_ == null)
			{
				TargetInfo = new ModHandbookTarget();
			}
			TargetInfo.MergeFrom(other.TargetInfo);
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
				modList_.AddEntriesFrom(ref input, _repeated_modList_codec);
				break;
			case 18u:
				typeTargetList_.AddEntriesFrom(ref input, _repeated_typeTargetList_codec);
				break;
			case 26u:
				if (targetInfo_ == null)
				{
					TargetInfo = new ModHandbookTarget();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)TargetInfo);
				break;
			}
		}
	}
}
