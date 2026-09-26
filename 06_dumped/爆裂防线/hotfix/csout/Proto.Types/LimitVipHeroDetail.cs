using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class LimitVipHeroDetail : IMessage<LimitVipHeroDetail>, IMessage, IEquatable<LimitVipHeroDetail>, IDeepCloneable<LimitVipHeroDetail>, IBufferMessage
{
	private static readonly MessageParser<LimitVipHeroDetail> _parser = new MessageParser<LimitVipHeroDetail>((Func<LimitVipHeroDetail>)(() => new LimitVipHeroDetail()));

	private UnknownFieldSet _unknownFields;

	public const int BaseInfoFieldNumber = 1;

	private OperateBaseInfo baseInfo_;

	public const int BaseDataFieldNumber = 2;

	private LimitVipHeroBaseData baseData_;

	public const int RandListFieldNumber = 3;

	private static readonly FieldCodec<LimitVipHeroRandDetail> _repeated_randList_codec = FieldCodec.ForMessage<LimitVipHeroRandDetail>(26u, LimitVipHeroRandDetail.Parser);

	private readonly RepeatedField<LimitVipHeroRandDetail> randList_ = new RepeatedField<LimitVipHeroRandDetail>();

	public const int TargetListFieldNumber = 4;

	private static readonly FieldCodec<OperateTargetDetail> _repeated_targetList_codec = FieldCodec.ForMessage<OperateTargetDetail>(34u, OperateTargetDetail.Parser);

	private readonly RepeatedField<OperateTargetDetail> targetList_ = new RepeatedField<OperateTargetDetail>();

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<LimitVipHeroDetail> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeLimitVipHeroDetailReflection.Descriptor.MessageTypes[0];

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
	public LimitVipHeroBaseData BaseData
	{
		get
		{
			return baseData_;
		}
		set
		{
			baseData_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<LimitVipHeroRandDetail> RandList => randList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<OperateTargetDetail> TargetList => targetList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public LimitVipHeroDetail()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public LimitVipHeroDetail(LimitVipHeroDetail other)
		: this()
	{
		baseInfo_ = ((other.baseInfo_ != null) ? other.baseInfo_.Clone() : null);
		baseData_ = ((other.baseData_ != null) ? other.baseData_.Clone() : null);
		randList_ = other.randList_.Clone();
		targetList_ = other.targetList_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public LimitVipHeroDetail Clone()
	{
		return new LimitVipHeroDetail(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as LimitVipHeroDetail);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(LimitVipHeroDetail other)
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
		if (!object.Equals(BaseData, other.BaseData))
		{
			return false;
		}
		if (!randList_.Equals(other.randList_))
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
		if (baseData_ != null)
		{
			num ^= BaseData.GetHashCode();
		}
		num ^= ((object)randList_).GetHashCode();
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
		if (baseData_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)18);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)BaseData);
		}
		randList_.WriteTo(ref output, _repeated_randList_codec);
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
		if (baseData_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)BaseData);
		}
		num += randList_.CalculateSize(_repeated_randList_codec);
		num += targetList_.CalculateSize(_repeated_targetList_codec);
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(LimitVipHeroDetail other)
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
		if (other.baseData_ != null)
		{
			if (baseData_ == null)
			{
				BaseData = new LimitVipHeroBaseData();
			}
			BaseData.MergeFrom(other.BaseData);
		}
		randList_.Add((IEnumerable<LimitVipHeroRandDetail>)other.randList_);
		targetList_.Add((IEnumerable<OperateTargetDetail>)other.targetList_);
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
			case 18u:
				if (baseData_ == null)
				{
					BaseData = new LimitVipHeroBaseData();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)BaseData);
				break;
			case 26u:
				randList_.AddEntriesFrom(ref input, _repeated_randList_codec);
				break;
			case 34u:
				targetList_.AddEntriesFrom(ref input, _repeated_targetList_codec);
				break;
			}
		}
	}
}
