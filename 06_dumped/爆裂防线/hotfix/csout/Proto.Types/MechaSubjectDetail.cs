using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class MechaSubjectDetail : IMessage<MechaSubjectDetail>, IMessage, IEquatable<MechaSubjectDetail>, IDeepCloneable<MechaSubjectDetail>, IBufferMessage
{
	private static readonly MessageParser<MechaSubjectDetail> _parser = new MessageParser<MechaSubjectDetail>((Func<MechaSubjectDetail>)(() => new MechaSubjectDetail()));

	private UnknownFieldSet _unknownFields;

	public const int BaseInfoFieldNumber = 1;

	private OperateBaseInfo baseInfo_;

	public const int SubjectIdFieldNumber = 2;

	private int subjectId_;

	public const int ChapterInfoFieldNumber = 3;

	private OperateSubjectChapterInfo chapterInfo_;

	public const int BarInfoFieldNumber = 4;

	private OperateSubjectBarInfo barInfo_;

	public const int RouletteInfoFieldNumber = 5;

	private OperateSubjectRouletteInfo rouletteInfo_;

	public const int ZhanLingInfoFieldNumber = 6;

	private ZhanLingCommonInfo zhanLingInfo_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<MechaSubjectDetail> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeMechaSubjectDetailReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public OperateBaseInfo BaseInfo
	{
		get
		{
			return baseInfo_;
		}
		set
		{
			baseInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int SubjectId
	{
		get
		{
			return subjectId_;
		}
		set
		{
			subjectId_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public OperateSubjectChapterInfo ChapterInfo
	{
		get
		{
			return chapterInfo_;
		}
		set
		{
			chapterInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public OperateSubjectBarInfo BarInfo
	{
		get
		{
			return barInfo_;
		}
		set
		{
			barInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public OperateSubjectRouletteInfo RouletteInfo
	{
		get
		{
			return rouletteInfo_;
		}
		set
		{
			rouletteInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ZhanLingCommonInfo ZhanLingInfo
	{
		get
		{
			return zhanLingInfo_;
		}
		set
		{
			zhanLingInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public MechaSubjectDetail()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public MechaSubjectDetail(MechaSubjectDetail other)
		: this()
	{
		baseInfo_ = ((other.baseInfo_ != null) ? other.baseInfo_.Clone() : null);
		subjectId_ = other.subjectId_;
		chapterInfo_ = ((other.chapterInfo_ != null) ? other.chapterInfo_.Clone() : null);
		barInfo_ = ((other.barInfo_ != null) ? other.barInfo_.Clone() : null);
		rouletteInfo_ = ((other.rouletteInfo_ != null) ? other.rouletteInfo_.Clone() : null);
		zhanLingInfo_ = ((other.zhanLingInfo_ != null) ? other.zhanLingInfo_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public MechaSubjectDetail Clone()
	{
		return new MechaSubjectDetail(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as MechaSubjectDetail);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(MechaSubjectDetail other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!object.Equals(BaseInfo, other.BaseInfo))
		{
			return false;
		}
		if (SubjectId != other.SubjectId)
		{
			return false;
		}
		if (!object.Equals(ChapterInfo, other.ChapterInfo))
		{
			return false;
		}
		if (!object.Equals(BarInfo, other.BarInfo))
		{
			return false;
		}
		if (!object.Equals(RouletteInfo, other.RouletteInfo))
		{
			return false;
		}
		if (!object.Equals(ZhanLingInfo, other.ZhanLingInfo))
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
		if (baseInfo_ != null)
		{
			num ^= BaseInfo.GetHashCode();
		}
		if (SubjectId != 0)
		{
			num ^= SubjectId.GetHashCode();
		}
		if (chapterInfo_ != null)
		{
			num ^= ChapterInfo.GetHashCode();
		}
		if (barInfo_ != null)
		{
			num ^= BarInfo.GetHashCode();
		}
		if (rouletteInfo_ != null)
		{
			num ^= RouletteInfo.GetHashCode();
		}
		if (zhanLingInfo_ != null)
		{
			num ^= ZhanLingInfo.GetHashCode();
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
		if (baseInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)10);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)BaseInfo);
		}
		if (SubjectId != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteInt32(SubjectId);
		}
		if (chapterInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)26);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)ChapterInfo);
		}
		if (barInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)34);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)BarInfo);
		}
		if (rouletteInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)42);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)RouletteInfo);
		}
		if (zhanLingInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)50);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)ZhanLingInfo);
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
		if (baseInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)BaseInfo);
		}
		if (SubjectId != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(SubjectId);
		}
		if (chapterInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)ChapterInfo);
		}
		if (barInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)BarInfo);
		}
		if (rouletteInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)RouletteInfo);
		}
		if (zhanLingInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)ZhanLingInfo);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(MechaSubjectDetail other)
	{
		if (other == null)
		{
			return;
		}
		if (other.baseInfo_ != null)
		{
			if (baseInfo_ == null)
			{
				BaseInfo = new OperateBaseInfo();
			}
			BaseInfo.MergeFrom(other.BaseInfo);
		}
		if (other.SubjectId != 0)
		{
			SubjectId = other.SubjectId;
		}
		if (other.chapterInfo_ != null)
		{
			if (chapterInfo_ == null)
			{
				ChapterInfo = new OperateSubjectChapterInfo();
			}
			ChapterInfo.MergeFrom(other.ChapterInfo);
		}
		if (other.barInfo_ != null)
		{
			if (barInfo_ == null)
			{
				BarInfo = new OperateSubjectBarInfo();
			}
			BarInfo.MergeFrom(other.BarInfo);
		}
		if (other.rouletteInfo_ != null)
		{
			if (rouletteInfo_ == null)
			{
				RouletteInfo = new OperateSubjectRouletteInfo();
			}
			RouletteInfo.MergeFrom(other.RouletteInfo);
		}
		if (other.zhanLingInfo_ != null)
		{
			if (zhanLingInfo_ == null)
			{
				ZhanLingInfo = new ZhanLingCommonInfo();
			}
			ZhanLingInfo.MergeFrom(other.ZhanLingInfo);
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
				if (baseInfo_ == null)
				{
					BaseInfo = new OperateBaseInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)BaseInfo);
				break;
			case 16u:
				SubjectId = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 26u:
				if (chapterInfo_ == null)
				{
					ChapterInfo = new OperateSubjectChapterInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)ChapterInfo);
				break;
			case 34u:
				if (barInfo_ == null)
				{
					BarInfo = new OperateSubjectBarInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)BarInfo);
				break;
			case 42u:
				if (rouletteInfo_ == null)
				{
					RouletteInfo = new OperateSubjectRouletteInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)RouletteInfo);
				break;
			case 50u:
				if (zhanLingInfo_ == null)
				{
					ZhanLingInfo = new ZhanLingCommonInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)ZhanLingInfo);
				break;
			}
		}
	}
}
