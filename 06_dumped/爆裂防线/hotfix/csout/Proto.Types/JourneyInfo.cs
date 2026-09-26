using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class JourneyInfo : IMessage<JourneyInfo>, IMessage, IEquatable<JourneyInfo>, IDeepCloneable<JourneyInfo>, IBufferMessage
{
	private static readonly MessageParser<JourneyInfo> _parser = new MessageParser<JourneyInfo>((Func<JourneyInfo>)(() => new JourneyInfo()));

	private UnknownFieldSet _unknownFields;

	public const int ChapterOneFieldNumber = 1;

	private JourneyChapterOneInfo chapterOne_;

	public const int ChapterTwoFieldNumber = 2;

	private JourneyChapterTwoInfo chapterTwo_;

	public const int ChapterThreeFieldNumber = 3;

	private JourneyChapterThreeInfo chapterThree_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<JourneyInfo> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeJourneyInfoReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public JourneyChapterOneInfo ChapterOne
	{
		get
		{
			return chapterOne_;
		}
		set
		{
			chapterOne_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public JourneyChapterTwoInfo ChapterTwo
	{
		get
		{
			return chapterTwo_;
		}
		set
		{
			chapterTwo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public JourneyChapterThreeInfo ChapterThree
	{
		get
		{
			return chapterThree_;
		}
		set
		{
			chapterThree_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public JourneyInfo()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public JourneyInfo(JourneyInfo other)
		: this()
	{
		chapterOne_ = ((other.chapterOne_ != null) ? other.chapterOne_.Clone() : null);
		chapterTwo_ = ((other.chapterTwo_ != null) ? other.chapterTwo_.Clone() : null);
		chapterThree_ = ((other.chapterThree_ != null) ? other.chapterThree_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public JourneyInfo Clone()
	{
		return new JourneyInfo(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as JourneyInfo);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(JourneyInfo other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!object.Equals(ChapterOne, other.ChapterOne))
		{
			return false;
		}
		if (!object.Equals(ChapterTwo, other.ChapterTwo))
		{
			return false;
		}
		if (!object.Equals(ChapterThree, other.ChapterThree))
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
		if (chapterOne_ != null)
		{
			num ^= ChapterOne.GetHashCode();
		}
		if (chapterTwo_ != null)
		{
			num ^= ChapterTwo.GetHashCode();
		}
		if (chapterThree_ != null)
		{
			num ^= ChapterThree.GetHashCode();
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
		if (chapterOne_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)10);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)ChapterOne);
		}
		if (chapterTwo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)18);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)ChapterTwo);
		}
		if (chapterThree_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)26);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)ChapterThree);
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
		if (chapterOne_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)ChapterOne);
		}
		if (chapterTwo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)ChapterTwo);
		}
		if (chapterThree_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)ChapterThree);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(JourneyInfo other)
	{
		if (other == null)
		{
			return;
		}
		if (other.chapterOne_ != null)
		{
			if (chapterOne_ == null)
			{
				ChapterOne = new JourneyChapterOneInfo();
			}
			ChapterOne.MergeFrom(other.ChapterOne);
		}
		if (other.chapterTwo_ != null)
		{
			if (chapterTwo_ == null)
			{
				ChapterTwo = new JourneyChapterTwoInfo();
			}
			ChapterTwo.MergeFrom(other.ChapterTwo);
		}
		if (other.chapterThree_ != null)
		{
			if (chapterThree_ == null)
			{
				ChapterThree = new JourneyChapterThreeInfo();
			}
			ChapterThree.MergeFrom(other.ChapterThree);
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
				if (chapterOne_ == null)
				{
					ChapterOne = new JourneyChapterOneInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)ChapterOne);
				break;
			case 18u:
				if (chapterTwo_ == null)
				{
					ChapterTwo = new JourneyChapterTwoInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)ChapterTwo);
				break;
			case 26u:
				if (chapterThree_ == null)
				{
					ChapterThree = new JourneyChapterThreeInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)ChapterThree);
				break;
			}
		}
	}
}
