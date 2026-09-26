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
public sealed class PrayLanternDetail : IMessage<PrayLanternDetail>, IMessage, IEquatable<PrayLanternDetail>, IDeepCloneable<PrayLanternDetail>, IBufferMessage
{
	private static readonly MessageParser<PrayLanternDetail> _parser = new MessageParser<PrayLanternDetail>((Func<PrayLanternDetail>)(() => new PrayLanternDetail()));

	private UnknownFieldSet _unknownFields;

	public const int BaseInfoFieldNumber = 1;

	private OperateBaseInfo baseInfo_;

	public const int DailyFreeFieldNumber = 2;

	private EBoolState dailyFree_;

	public const int DrawCountFieldNumber = 3;

	private int drawCount_;

	public const int UnlockListFieldNumber = 4;

	private static readonly FieldCodec<int> _repeated_unlockList_codec = FieldCodec.ForInt32(34u);

	private readonly RepeatedField<int> unlockList_ = new RepeatedField<int>();

	public const int TargetListFieldNumber = 5;

	private static readonly FieldCodec<OperateTargetDetail> _repeated_targetList_codec = FieldCodec.ForMessage<OperateTargetDetail>(42u, OperateTargetDetail.Parser);

	private readonly RepeatedField<OperateTargetDetail> targetList_ = new RepeatedField<OperateTargetDetail>();

	public const int UltimateFieldNumber = 6;

	private PrayLanternUltimateDetail ultimate_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<PrayLanternDetail> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypePrayLanternDetailReflection.Descriptor.MessageTypes[0];

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
	public EBoolState DailyFree
	{
		get
		{
			return dailyFree_;
		}
		set
		{
			dailyFree_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int DrawCount
	{
		get
		{
			return drawCount_;
		}
		set
		{
			drawCount_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<int> UnlockList => unlockList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<OperateTargetDetail> TargetList => targetList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public PrayLanternUltimateDetail Ultimate
	{
		get
		{
			return ultimate_;
		}
		set
		{
			ultimate_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public PrayLanternDetail()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public PrayLanternDetail(PrayLanternDetail other)
		: this()
	{
		baseInfo_ = ((other.baseInfo_ != null) ? other.baseInfo_.Clone() : null);
		dailyFree_ = other.dailyFree_;
		drawCount_ = other.drawCount_;
		unlockList_ = other.unlockList_.Clone();
		targetList_ = other.targetList_.Clone();
		ultimate_ = ((other.ultimate_ != null) ? other.ultimate_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public PrayLanternDetail Clone()
	{
		return new PrayLanternDetail(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as PrayLanternDetail);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(PrayLanternDetail other)
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
		if (DailyFree != other.DailyFree)
		{
			return false;
		}
		if (DrawCount != other.DrawCount)
		{
			return false;
		}
		if (!unlockList_.Equals(other.unlockList_))
		{
			return false;
		}
		if (!targetList_.Equals(other.targetList_))
		{
			return false;
		}
		if (!object.Equals(Ultimate, other.Ultimate))
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
		if (DailyFree != EBoolState.False)
		{
			num ^= DailyFree.GetHashCode();
		}
		if (DrawCount != 0)
		{
			num ^= DrawCount.GetHashCode();
		}
		num ^= ((object)unlockList_).GetHashCode();
		num ^= ((object)targetList_).GetHashCode();
		if (ultimate_ != null)
		{
			num ^= Ultimate.GetHashCode();
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
		if (DailyFree != EBoolState.False)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteEnum((int)DailyFree);
		}
		if (DrawCount != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)24);
			((WriteContext)(ref output)).WriteInt32(DrawCount);
		}
		unlockList_.WriteTo(ref output, _repeated_unlockList_codec);
		targetList_.WriteTo(ref output, _repeated_targetList_codec);
		if (ultimate_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)50);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)Ultimate);
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
		if (DailyFree != EBoolState.False)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)DailyFree);
		}
		if (DrawCount != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(DrawCount);
		}
		num += unlockList_.CalculateSize(_repeated_unlockList_codec);
		num += targetList_.CalculateSize(_repeated_targetList_codec);
		if (ultimate_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)Ultimate);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(PrayLanternDetail other)
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
		if (other.DailyFree != EBoolState.False)
		{
			DailyFree = other.DailyFree;
		}
		if (other.DrawCount != 0)
		{
			DrawCount = other.DrawCount;
		}
		unlockList_.Add((IEnumerable<int>)other.unlockList_);
		targetList_.Add((IEnumerable<OperateTargetDetail>)other.targetList_);
		if (other.ultimate_ != null)
		{
			if (ultimate_ == null)
			{
				Ultimate = new PrayLanternUltimateDetail();
			}
			Ultimate.MergeFrom(other.Ultimate);
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
				DailyFree = (EBoolState)((ParseContext)(ref input)).ReadEnum();
				break;
			case 24u:
				DrawCount = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 32u:
			case 34u:
				unlockList_.AddEntriesFrom(ref input, _repeated_unlockList_codec);
				break;
			case 42u:
				targetList_.AddEntriesFrom(ref input, _repeated_targetList_codec);
				break;
			case 50u:
				if (ultimate_ == null)
				{
					Ultimate = new PrayLanternUltimateDetail();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)Ultimate);
				break;
			}
		}
	}
}
