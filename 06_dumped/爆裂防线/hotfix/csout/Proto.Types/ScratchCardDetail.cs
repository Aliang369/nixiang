using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class ScratchCardDetail : IMessage<ScratchCardDetail>, IMessage, IEquatable<ScratchCardDetail>, IDeepCloneable<ScratchCardDetail>, IBufferMessage
{
	private static readonly MessageParser<ScratchCardDetail> _parser = new MessageParser<ScratchCardDetail>((Func<ScratchCardDetail>)(() => new ScratchCardDetail()));

	private UnknownFieldSet _unknownFields;

	public const int BaseInfoFieldNumber = 1;

	private OperateBaseInfo baseInfo_;

	public const int ResetCountFieldNumber = 2;

	private int resetCount_;

	public const int ResetMaxFieldNumber = 3;

	private int resetMax_;

	public const int ResetDiamondFieldNumber = 4;

	private int resetDiamond_;

	public const int PropIdFieldNumber = 5;

	private int propId_;

	public const int ScratchListFieldNumber = 6;

	private static readonly FieldCodec<int> _repeated_scratchList_codec = FieldCodec.ForInt32(50u);

	private readonly RepeatedField<int> scratchList_ = new RepeatedField<int>();

	public const int TargetListFieldNumber = 7;

	private static readonly FieldCodec<OperateScratchCardDetail> _repeated_targetList_codec = FieldCodec.ForMessage<OperateScratchCardDetail>(58u, OperateScratchCardDetail.Parser);

	private readonly RepeatedField<OperateScratchCardDetail> targetList_ = new RepeatedField<OperateScratchCardDetail>();

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<ScratchCardDetail> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeScratchCardDetailReflection.Descriptor.MessageTypes[0];

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
	public int ResetCount
	{
		get
		{
			return resetCount_;
		}
		set
		{
			resetCount_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int ResetMax
	{
		get
		{
			return resetMax_;
		}
		set
		{
			resetMax_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int ResetDiamond
	{
		get
		{
			return resetDiamond_;
		}
		set
		{
			resetDiamond_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int PropId
	{
		get
		{
			return propId_;
		}
		set
		{
			propId_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<int> ScratchList => scratchList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<OperateScratchCardDetail> TargetList => targetList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ScratchCardDetail()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ScratchCardDetail(ScratchCardDetail other)
		: this()
	{
		baseInfo_ = ((other.baseInfo_ != null) ? other.baseInfo_.Clone() : null);
		resetCount_ = other.resetCount_;
		resetMax_ = other.resetMax_;
		resetDiamond_ = other.resetDiamond_;
		propId_ = other.propId_;
		scratchList_ = other.scratchList_.Clone();
		targetList_ = other.targetList_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ScratchCardDetail Clone()
	{
		return new ScratchCardDetail(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as ScratchCardDetail);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(ScratchCardDetail other)
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
		if (ResetCount != other.ResetCount)
		{
			return false;
		}
		if (ResetMax != other.ResetMax)
		{
			return false;
		}
		if (ResetDiamond != other.ResetDiamond)
		{
			return false;
		}
		if (PropId != other.PropId)
		{
			return false;
		}
		if (!scratchList_.Equals(other.scratchList_))
		{
			return false;
		}
		if (!targetList_.Equals(other.targetList_))
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
		if (ResetCount != 0)
		{
			num ^= ResetCount.GetHashCode();
		}
		if (ResetMax != 0)
		{
			num ^= ResetMax.GetHashCode();
		}
		if (ResetDiamond != 0)
		{
			num ^= ResetDiamond.GetHashCode();
		}
		if (PropId != 0)
		{
			num ^= PropId.GetHashCode();
		}
		num ^= ((object)scratchList_).GetHashCode();
		num ^= ((object)targetList_).GetHashCode();
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
		if (ResetCount != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteInt32(ResetCount);
		}
		if (ResetMax != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)24);
			((WriteContext)(ref output)).WriteInt32(ResetMax);
		}
		if (ResetDiamond != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)32);
			((WriteContext)(ref output)).WriteInt32(ResetDiamond);
		}
		if (PropId != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)40);
			((WriteContext)(ref output)).WriteInt32(PropId);
		}
		scratchList_.WriteTo(ref output, _repeated_scratchList_codec);
		targetList_.WriteTo(ref output, _repeated_targetList_codec);
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
		if (ResetCount != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(ResetCount);
		}
		if (ResetMax != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(ResetMax);
		}
		if (ResetDiamond != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(ResetDiamond);
		}
		if (PropId != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(PropId);
		}
		num += scratchList_.CalculateSize(_repeated_scratchList_codec);
		num += targetList_.CalculateSize(_repeated_targetList_codec);
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(ScratchCardDetail other)
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
		if (other.ResetCount != 0)
		{
			ResetCount = other.ResetCount;
		}
		if (other.ResetMax != 0)
		{
			ResetMax = other.ResetMax;
		}
		if (other.ResetDiamond != 0)
		{
			ResetDiamond = other.ResetDiamond;
		}
		if (other.PropId != 0)
		{
			PropId = other.PropId;
		}
		scratchList_.Add((IEnumerable<int>)other.scratchList_);
		targetList_.Add((IEnumerable<OperateScratchCardDetail>)other.targetList_);
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
				ResetCount = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 24u:
				ResetMax = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 32u:
				ResetDiamond = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 40u:
				PropId = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 48u:
			case 50u:
				scratchList_.AddEntriesFrom(ref input, _repeated_scratchList_codec);
				break;
			case 58u:
				targetList_.AddEntriesFrom(ref input, _repeated_targetList_codec);
				break;
			}
		}
	}
}
