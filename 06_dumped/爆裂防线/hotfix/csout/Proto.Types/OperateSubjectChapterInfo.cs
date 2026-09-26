using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class OperateSubjectChapterInfo : IMessage<OperateSubjectChapterInfo>, IMessage, IEquatable<OperateSubjectChapterInfo>, IDeepCloneable<OperateSubjectChapterInfo>, IBufferMessage
{
	private static readonly MessageParser<OperateSubjectChapterInfo> _parser = new MessageParser<OperateSubjectChapterInfo>((Func<OperateSubjectChapterInfo>)(() => new OperateSubjectChapterInfo()));

	private UnknownFieldSet _unknownFields;

	public const int PassLevelFieldNumber = 1;

	private int passLevel_;

	public const int ChapterListFieldNumber = 2;

	private static readonly FieldCodec<OperateSubjectChapterDetail> _repeated_chapterList_codec = FieldCodec.ForMessage<OperateSubjectChapterDetail>(18u, OperateSubjectChapterDetail.Parser);

	private readonly RepeatedField<OperateSubjectChapterDetail> chapterList_ = new RepeatedField<OperateSubjectChapterDetail>();

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<OperateSubjectChapterInfo> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeOperateSubjectChapterInfoReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int PassLevel
	{
		get
		{
			return passLevel_;
		}
		set
		{
			passLevel_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<OperateSubjectChapterDetail> ChapterList => chapterList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public OperateSubjectChapterInfo()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public OperateSubjectChapterInfo(OperateSubjectChapterInfo other)
		: this()
	{
		passLevel_ = other.passLevel_;
		chapterList_ = other.chapterList_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public OperateSubjectChapterInfo Clone()
	{
		return new OperateSubjectChapterInfo(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as OperateSubjectChapterInfo);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(OperateSubjectChapterInfo other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (PassLevel != other.PassLevel)
		{
			return false;
		}
		if (!chapterList_.Equals(other.chapterList_))
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
		if (PassLevel != 0)
		{
			num ^= PassLevel.GetHashCode();
		}
		num ^= ((object)chapterList_).GetHashCode();
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
		if (PassLevel != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteInt32(PassLevel);
		}
		chapterList_.WriteTo(ref output, _repeated_chapterList_codec);
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
		if (PassLevel != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(PassLevel);
		}
		num += chapterList_.CalculateSize(_repeated_chapterList_codec);
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(OperateSubjectChapterInfo other)
	{
		if (other != null)
		{
			if (other.PassLevel != 0)
			{
				PassLevel = other.PassLevel;
			}
			chapterList_.Add((IEnumerable<OperateSubjectChapterDetail>)other.chapterList_);
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
			case 8u:
				PassLevel = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 18u:
				chapterList_.AddEntriesFrom(ref input, _repeated_chapterList_codec);
				break;
			}
		}
	}
}
