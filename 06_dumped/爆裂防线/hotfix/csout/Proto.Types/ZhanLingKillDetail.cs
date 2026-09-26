using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class ZhanLingKillDetail : IMessage<ZhanLingKillDetail>, IMessage, IEquatable<ZhanLingKillDetail>, IDeepCloneable<ZhanLingKillDetail>, IBufferMessage
{
	private static readonly MessageParser<ZhanLingKillDetail> _parser = new MessageParser<ZhanLingKillDetail>((Func<ZhanLingKillDetail>)(() => new ZhanLingKillDetail()));

	private UnknownFieldSet _unknownFields;

	public const int BaseInfoFieldNumber = 1;

	private OperateBaseInfo baseInfo_;

	public const int ZhanLingInfoFieldNumber = 2;

	private OperateZhanLingInfo zhanLingInfo_;

	public const int GrandExtraListFieldNumber = 3;

	private static readonly FieldCodec<OperateZhanLingExtraDetail> _repeated_grandExtraList_codec = FieldCodec.ForMessage<OperateZhanLingExtraDetail>(26u, OperateZhanLingExtraDetail.Parser);

	private readonly RepeatedField<OperateZhanLingExtraDetail> grandExtraList_ = new RepeatedField<OperateZhanLingExtraDetail>();

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<ZhanLingKillDetail> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeZhanLingKillDetailReflection.Descriptor.MessageTypes[0];

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
	public OperateZhanLingInfo ZhanLingInfo
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
	public RepeatedField<OperateZhanLingExtraDetail> GrandExtraList => grandExtraList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ZhanLingKillDetail()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ZhanLingKillDetail(ZhanLingKillDetail other)
		: this()
	{
		baseInfo_ = ((other.baseInfo_ != null) ? other.baseInfo_.Clone() : null);
		zhanLingInfo_ = ((other.zhanLingInfo_ != null) ? other.zhanLingInfo_.Clone() : null);
		grandExtraList_ = other.grandExtraList_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ZhanLingKillDetail Clone()
	{
		return new ZhanLingKillDetail(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as ZhanLingKillDetail);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(ZhanLingKillDetail other)
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
		if (!object.Equals(ZhanLingInfo, other.ZhanLingInfo))
		{
			return false;
		}
		if (!grandExtraList_.Equals(other.grandExtraList_))
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
		if (zhanLingInfo_ != null)
		{
			num ^= ZhanLingInfo.GetHashCode();
		}
		num ^= ((object)grandExtraList_).GetHashCode();
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
		if (zhanLingInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)18);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)ZhanLingInfo);
		}
		grandExtraList_.WriteTo(ref output, _repeated_grandExtraList_codec);
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
		if (zhanLingInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)ZhanLingInfo);
		}
		num += grandExtraList_.CalculateSize(_repeated_grandExtraList_codec);
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(ZhanLingKillDetail other)
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
		if (other.zhanLingInfo_ != null)
		{
			if (zhanLingInfo_ == null)
			{
				ZhanLingInfo = new OperateZhanLingInfo();
			}
			ZhanLingInfo.MergeFrom(other.ZhanLingInfo);
		}
		grandExtraList_.Add((IEnumerable<OperateZhanLingExtraDetail>)other.grandExtraList_);
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
				if (zhanLingInfo_ == null)
				{
					ZhanLingInfo = new OperateZhanLingInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)ZhanLingInfo);
				break;
			case 26u:
				grandExtraList_.AddEntriesFrom(ref input, _repeated_grandExtraList_codec);
				break;
			}
		}
	}
}
