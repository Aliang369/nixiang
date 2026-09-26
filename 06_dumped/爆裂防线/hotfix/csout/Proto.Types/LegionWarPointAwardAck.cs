using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class LegionWarPointAwardAck : IMessage<LegionWarPointAwardAck>, IMessage, IEquatable<LegionWarPointAwardAck>, IDeepCloneable<LegionWarPointAwardAck>, IBufferMessage
{
	private static readonly MessageParser<LegionWarPointAwardAck> _parser = new MessageParser<LegionWarPointAwardAck>((Func<LegionWarPointAwardAck>)(() => new LegionWarPointAwardAck()));

	private UnknownFieldSet _unknownFields;

	public const int LegionWarBaseInfoFieldNumber = 1;

	private LegionWarBaseInfo legionWarBaseInfo_;

	public const int LegionWarLegionInfoFieldNumber = 2;

	private LegionWarLegionInfo legionWarLegionInfo_;

	public const int LegionWarSelfInfoFieldNumber = 3;

	private LegionWarSelfInfo legionWarSelfInfo_;

	public const int LegionWarMapInfoFieldNumber = 4;

	private LegionWarMapInfo legionWarMapInfo_;

	public const int AwardListFieldNumber = 5;

	private static readonly FieldCodec<AwardDetail> _repeated_awardList_codec = FieldCodec.ForMessage<AwardDetail>(42u, AwardDetail.Parser);

	private readonly RepeatedField<AwardDetail> awardList_ = new RepeatedField<AwardDetail>();

	public const int DataChangeFieldNumber = 6;

	private DataChange dataChange_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<LegionWarPointAwardAck> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeLegionWarPointAwardAckReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public LegionWarBaseInfo LegionWarBaseInfo
	{
		get
		{
			return legionWarBaseInfo_;
		}
		set
		{
			legionWarBaseInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public LegionWarLegionInfo LegionWarLegionInfo
	{
		get
		{
			return legionWarLegionInfo_;
		}
		set
		{
			legionWarLegionInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public LegionWarSelfInfo LegionWarSelfInfo
	{
		get
		{
			return legionWarSelfInfo_;
		}
		set
		{
			legionWarSelfInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public LegionWarMapInfo LegionWarMapInfo
	{
		get
		{
			return legionWarMapInfo_;
		}
		set
		{
			legionWarMapInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<AwardDetail> AwardList => awardList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public DataChange DataChange
	{
		get
		{
			return dataChange_;
		}
		set
		{
			dataChange_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public LegionWarPointAwardAck()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public LegionWarPointAwardAck(LegionWarPointAwardAck other)
		: this()
	{
		legionWarBaseInfo_ = ((other.legionWarBaseInfo_ != null) ? other.legionWarBaseInfo_.Clone() : null);
		legionWarLegionInfo_ = ((other.legionWarLegionInfo_ != null) ? other.legionWarLegionInfo_.Clone() : null);
		legionWarSelfInfo_ = ((other.legionWarSelfInfo_ != null) ? other.legionWarSelfInfo_.Clone() : null);
		legionWarMapInfo_ = ((other.legionWarMapInfo_ != null) ? other.legionWarMapInfo_.Clone() : null);
		awardList_ = other.awardList_.Clone();
		dataChange_ = ((other.dataChange_ != null) ? other.dataChange_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public LegionWarPointAwardAck Clone()
	{
		return new LegionWarPointAwardAck(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as LegionWarPointAwardAck);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(LegionWarPointAwardAck other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!object.Equals(LegionWarBaseInfo, other.LegionWarBaseInfo))
		{
			return false;
		}
		if (!object.Equals(LegionWarLegionInfo, other.LegionWarLegionInfo))
		{
			return false;
		}
		if (!object.Equals(LegionWarSelfInfo, other.LegionWarSelfInfo))
		{
			return false;
		}
		if (!object.Equals(LegionWarMapInfo, other.LegionWarMapInfo))
		{
			return false;
		}
		if (!awardList_.Equals(other.awardList_))
		{
			return false;
		}
		if (!object.Equals(DataChange, other.DataChange))
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
		if (legionWarBaseInfo_ != null)
		{
			num ^= LegionWarBaseInfo.GetHashCode();
		}
		if (legionWarLegionInfo_ != null)
		{
			num ^= LegionWarLegionInfo.GetHashCode();
		}
		if (legionWarSelfInfo_ != null)
		{
			num ^= LegionWarSelfInfo.GetHashCode();
		}
		if (legionWarMapInfo_ != null)
		{
			num ^= LegionWarMapInfo.GetHashCode();
		}
		num ^= ((object)awardList_).GetHashCode();
		if (dataChange_ != null)
		{
			num ^= DataChange.GetHashCode();
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
		if (legionWarBaseInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)10);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)LegionWarBaseInfo);
		}
		if (legionWarLegionInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)18);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)LegionWarLegionInfo);
		}
		if (legionWarSelfInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)26);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)LegionWarSelfInfo);
		}
		if (legionWarMapInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)34);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)LegionWarMapInfo);
		}
		awardList_.WriteTo(ref output, _repeated_awardList_codec);
		if (dataChange_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)50);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)DataChange);
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
		if (legionWarBaseInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)LegionWarBaseInfo);
		}
		if (legionWarLegionInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)LegionWarLegionInfo);
		}
		if (legionWarSelfInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)LegionWarSelfInfo);
		}
		if (legionWarMapInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)LegionWarMapInfo);
		}
		num += awardList_.CalculateSize(_repeated_awardList_codec);
		if (dataChange_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)DataChange);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(LegionWarPointAwardAck other)
	{
		if (other == null)
		{
			return;
		}
		if (other.legionWarBaseInfo_ != null)
		{
			if (legionWarBaseInfo_ == null)
			{
				LegionWarBaseInfo = new LegionWarBaseInfo();
			}
			LegionWarBaseInfo.MergeFrom(other.LegionWarBaseInfo);
		}
		if (other.legionWarLegionInfo_ != null)
		{
			if (legionWarLegionInfo_ == null)
			{
				LegionWarLegionInfo = new LegionWarLegionInfo();
			}
			LegionWarLegionInfo.MergeFrom(other.LegionWarLegionInfo);
		}
		if (other.legionWarSelfInfo_ != null)
		{
			if (legionWarSelfInfo_ == null)
			{
				LegionWarSelfInfo = new LegionWarSelfInfo();
			}
			LegionWarSelfInfo.MergeFrom(other.LegionWarSelfInfo);
		}
		if (other.legionWarMapInfo_ != null)
		{
			if (legionWarMapInfo_ == null)
			{
				LegionWarMapInfo = new LegionWarMapInfo();
			}
			LegionWarMapInfo.MergeFrom(other.LegionWarMapInfo);
		}
		awardList_.Add((IEnumerable<AwardDetail>)other.awardList_);
		if (other.dataChange_ != null)
		{
			if (dataChange_ == null)
			{
				DataChange = new DataChange();
			}
			DataChange.MergeFrom(other.DataChange);
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
				if (legionWarBaseInfo_ == null)
				{
					LegionWarBaseInfo = new LegionWarBaseInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)LegionWarBaseInfo);
				break;
			case 18u:
				if (legionWarLegionInfo_ == null)
				{
					LegionWarLegionInfo = new LegionWarLegionInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)LegionWarLegionInfo);
				break;
			case 26u:
				if (legionWarSelfInfo_ == null)
				{
					LegionWarSelfInfo = new LegionWarSelfInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)LegionWarSelfInfo);
				break;
			case 34u:
				if (legionWarMapInfo_ == null)
				{
					LegionWarMapInfo = new LegionWarMapInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)LegionWarMapInfo);
				break;
			case 42u:
				awardList_.AddEntriesFrom(ref input, _repeated_awardList_codec);
				break;
			case 50u:
				if (dataChange_ == null)
				{
					DataChange = new DataChange();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)DataChange);
				break;
			}
		}
	}
}
