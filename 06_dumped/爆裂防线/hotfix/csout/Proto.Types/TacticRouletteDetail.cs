using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class TacticRouletteDetail : IMessage<TacticRouletteDetail>, IMessage, IEquatable<TacticRouletteDetail>, IDeepCloneable<TacticRouletteDetail>, IBufferMessage
{
	private static readonly MessageParser<TacticRouletteDetail> _parser = new MessageParser<TacticRouletteDetail>((Func<TacticRouletteDetail>)(() => new TacticRouletteDetail()));

	private UnknownFieldSet _unknownFields;

	public const int BaseInfoFieldNumber = 1;

	private OperateBaseInfo baseInfo_;

	public const int SubjectIdFieldNumber = 2;

	private int subjectId_;

	public const int RouletteInfoFieldNumber = 3;

	private OperateSubjectRouletteInfo rouletteInfo_;

	public const int UltimateAwardListFieldNumber = 4;

	private static readonly FieldCodec<UltimateAwardDetail> _repeated_ultimateAwardList_codec = FieldCodec.ForMessage<UltimateAwardDetail>(34u, UltimateAwardDetail.Parser);

	private readonly RepeatedField<UltimateAwardDetail> ultimateAwardList_ = new RepeatedField<UltimateAwardDetail>();

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<TacticRouletteDetail> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeTacticRouletteDetailReflection.Descriptor.MessageTypes[0];

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
	public RepeatedField<UltimateAwardDetail> UltimateAwardList => ultimateAwardList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public TacticRouletteDetail()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public TacticRouletteDetail(TacticRouletteDetail other)
		: this()
	{
		baseInfo_ = ((other.baseInfo_ != null) ? other.baseInfo_.Clone() : null);
		subjectId_ = other.subjectId_;
		rouletteInfo_ = ((other.rouletteInfo_ != null) ? other.rouletteInfo_.Clone() : null);
		ultimateAwardList_ = other.ultimateAwardList_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public TacticRouletteDetail Clone()
	{
		return new TacticRouletteDetail(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as TacticRouletteDetail);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(TacticRouletteDetail other)
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
		if (!object.Equals(RouletteInfo, other.RouletteInfo))
		{
			return false;
		}
		if (!ultimateAwardList_.Equals(other.ultimateAwardList_))
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
		if (rouletteInfo_ != null)
		{
			num ^= RouletteInfo.GetHashCode();
		}
		num ^= ((object)ultimateAwardList_).GetHashCode();
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
		if (rouletteInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)26);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)RouletteInfo);
		}
		ultimateAwardList_.WriteTo(ref output, _repeated_ultimateAwardList_codec);
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
		if (rouletteInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)RouletteInfo);
		}
		num += ultimateAwardList_.CalculateSize(_repeated_ultimateAwardList_codec);
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(TacticRouletteDetail other)
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
		if (other.rouletteInfo_ != null)
		{
			if (rouletteInfo_ == null)
			{
				RouletteInfo = new OperateSubjectRouletteInfo();
			}
			RouletteInfo.MergeFrom(other.RouletteInfo);
		}
		ultimateAwardList_.Add((IEnumerable<UltimateAwardDetail>)other.ultimateAwardList_);
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
				if (rouletteInfo_ == null)
				{
					RouletteInfo = new OperateSubjectRouletteInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)RouletteInfo);
				break;
			case 34u:
				ultimateAwardList_.AddEntriesFrom(ref input, _repeated_ultimateAwardList_codec);
				break;
			}
		}
	}
}
