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
public sealed class OperateSubjectChapterDetail : IMessage<OperateSubjectChapterDetail>, IMessage, IEquatable<OperateSubjectChapterDetail>, IDeepCloneable<OperateSubjectChapterDetail>, IBufferMessage
{
	private static readonly MessageParser<OperateSubjectChapterDetail> _parser = new MessageParser<OperateSubjectChapterDetail>((Func<OperateSubjectChapterDetail>)(() => new OperateSubjectChapterDetail()));

	private UnknownFieldSet _unknownFields;

	public const int ChapterIdFieldNumber = 1;

	private int chapterId_;

	public const int LevelListFieldNumber = 2;

	private static readonly FieldCodec<OperateSubjectLevelDetail> _repeated_levelList_codec = FieldCodec.ForMessage<OperateSubjectLevelDetail>(18u, OperateSubjectLevelDetail.Parser);

	private readonly RepeatedField<OperateSubjectLevelDetail> levelList_ = new RepeatedField<OperateSubjectLevelDetail>();

	public const int ChapterAwardFieldNumber = 3;

	private ECommonState chapterAward_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<OperateSubjectChapterDetail> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeOperateSubjectChapterDetailReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int ChapterId
	{
		get
		{
			return chapterId_;
		}
		set
		{
			chapterId_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<OperateSubjectLevelDetail> LevelList => levelList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ECommonState ChapterAward
	{
		get
		{
			return chapterAward_;
		}
		set
		{
			chapterAward_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public OperateSubjectChapterDetail()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public OperateSubjectChapterDetail(OperateSubjectChapterDetail other)
		: this()
	{
		chapterId_ = other.chapterId_;
		levelList_ = other.levelList_.Clone();
		chapterAward_ = other.chapterAward_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public OperateSubjectChapterDetail Clone()
	{
		return new OperateSubjectChapterDetail(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as OperateSubjectChapterDetail);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(OperateSubjectChapterDetail other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (ChapterId != other.ChapterId)
		{
			return false;
		}
		if (!levelList_.Equals(other.levelList_))
		{
			return false;
		}
		if (ChapterAward != other.ChapterAward)
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
		if (ChapterId != 0)
		{
			num ^= ChapterId.GetHashCode();
		}
		num ^= ((object)levelList_).GetHashCode();
		if (ChapterAward != ECommonState.No)
		{
			num ^= ChapterAward.GetHashCode();
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
		if (ChapterId != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteInt32(ChapterId);
		}
		levelList_.WriteTo(ref output, _repeated_levelList_codec);
		if (ChapterAward != ECommonState.No)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)24);
			((WriteContext)(ref output)).WriteEnum((int)ChapterAward);
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
		if (ChapterId != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(ChapterId);
		}
		num += levelList_.CalculateSize(_repeated_levelList_codec);
		if (ChapterAward != ECommonState.No)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)ChapterAward);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(OperateSubjectChapterDetail other)
	{
		if (other != null)
		{
			if (other.ChapterId != 0)
			{
				ChapterId = other.ChapterId;
			}
			levelList_.Add((IEnumerable<OperateSubjectLevelDetail>)other.levelList_);
			if (other.ChapterAward != ECommonState.No)
			{
				ChapterAward = other.ChapterAward;
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
			case 8u:
				ChapterId = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 18u:
				levelList_.AddEntriesFrom(ref input, _repeated_levelList_codec);
				break;
			case 24u:
				ChapterAward = (ECommonState)((ParseContext)(ref input)).ReadEnum();
				break;
			}
		}
	}
}
