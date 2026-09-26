using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class AdsZhanLingDetail : IMessage<AdsZhanLingDetail>, IMessage, IEquatable<AdsZhanLingDetail>, IDeepCloneable<AdsZhanLingDetail>, IBufferMessage
{
	private static readonly MessageParser<AdsZhanLingDetail> _parser = new MessageParser<AdsZhanLingDetail>((Func<AdsZhanLingDetail>)(() => new AdsZhanLingDetail()));

	private UnknownFieldSet _unknownFields;

	public const int BaseInfoFieldNumber = 1;

	private OperateBaseInfo baseInfo_;

	public const int LevelFieldNumber = 2;

	private int level_;

	public const int ReachFieldNumber = 3;

	private int reach_;

	public const int TargetListFieldNumber = 4;

	private static readonly FieldCodec<AdsCommonTargetDetail> _repeated_targetList_codec = FieldCodec.ForMessage<AdsCommonTargetDetail>(34u, AdsCommonTargetDetail.Parser);

	private readonly RepeatedField<AdsCommonTargetDetail> targetList_ = new RepeatedField<AdsCommonTargetDetail>();

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<AdsZhanLingDetail> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeAdsZhanLingDetailReflection.Descriptor.MessageTypes[0];

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
	public int Level
	{
		get
		{
			return level_;
		}
		set
		{
			level_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int Reach
	{
		get
		{
			return reach_;
		}
		set
		{
			reach_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<AdsCommonTargetDetail> TargetList => targetList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public AdsZhanLingDetail()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public AdsZhanLingDetail(AdsZhanLingDetail other)
		: this()
	{
		baseInfo_ = ((other.baseInfo_ != null) ? other.baseInfo_.Clone() : null);
		level_ = other.level_;
		reach_ = other.reach_;
		targetList_ = other.targetList_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public AdsZhanLingDetail Clone()
	{
		return new AdsZhanLingDetail(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as AdsZhanLingDetail);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(AdsZhanLingDetail other)
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
		if (Level != other.Level)
		{
			return false;
		}
		if (Reach != other.Reach)
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
		if (Level != 0)
		{
			num ^= Level.GetHashCode();
		}
		if (Reach != 0)
		{
			num ^= Reach.GetHashCode();
		}
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
		if (Level != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteInt32(Level);
		}
		if (Reach != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)24);
			((WriteContext)(ref output)).WriteInt32(Reach);
		}
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
		if (Level != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(Level);
		}
		if (Reach != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(Reach);
		}
		num += targetList_.CalculateSize(_repeated_targetList_codec);
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(AdsZhanLingDetail other)
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
		if (other.Level != 0)
		{
			Level = other.Level;
		}
		if (other.Reach != 0)
		{
			Reach = other.Reach;
		}
		targetList_.Add((IEnumerable<AdsCommonTargetDetail>)other.targetList_);
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
				Level = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 24u:
				Reach = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 34u:
				targetList_.AddEntriesFrom(ref input, _repeated_targetList_codec);
				break;
			}
		}
	}
}
