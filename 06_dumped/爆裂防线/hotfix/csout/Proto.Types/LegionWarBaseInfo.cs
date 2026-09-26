using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class LegionWarBaseInfo : IMessage<LegionWarBaseInfo>, IMessage, IEquatable<LegionWarBaseInfo>, IDeepCloneable<LegionWarBaseInfo>, IBufferMessage
{
	private static readonly MessageParser<LegionWarBaseInfo> _parser = new MessageParser<LegionWarBaseInfo>((Func<LegionWarBaseInfo>)(() => new LegionWarBaseInfo()));

	private UnknownFieldSet _unknownFields;

	public const int SeasonFieldNumber = 1;

	private int season_;

	public const int LegionWarOpenInfoFieldNumber = 2;

	private LegionWarOpenInfo legionWarOpenInfo_;

	public const int LegionWarZhanLingInfoFieldNumber = 3;

	private LegionWarZhanLingInfo legionWarZhanLingInfo_;

	public const int LegionWarTaskListFieldNumber = 4;

	private static readonly FieldCodec<TaskDetail> _repeated_legionWarTaskList_codec = FieldCodec.ForMessage<TaskDetail>(34u, TaskDetail.Parser);

	private readonly RepeatedField<TaskDetail> legionWarTaskList_ = new RepeatedField<TaskDetail>();

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<LegionWarBaseInfo> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeLegionWarBaseInfoReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int Season
	{
		get
		{
			return season_;
		}
		set
		{
			season_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public LegionWarOpenInfo LegionWarOpenInfo
	{
		get
		{
			return legionWarOpenInfo_;
		}
		set
		{
			legionWarOpenInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public LegionWarZhanLingInfo LegionWarZhanLingInfo
	{
		get
		{
			return legionWarZhanLingInfo_;
		}
		set
		{
			legionWarZhanLingInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<TaskDetail> LegionWarTaskList => legionWarTaskList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public LegionWarBaseInfo()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public LegionWarBaseInfo(LegionWarBaseInfo other)
		: this()
	{
		season_ = other.season_;
		legionWarOpenInfo_ = ((other.legionWarOpenInfo_ != null) ? other.legionWarOpenInfo_.Clone() : null);
		legionWarZhanLingInfo_ = ((other.legionWarZhanLingInfo_ != null) ? other.legionWarZhanLingInfo_.Clone() : null);
		legionWarTaskList_ = other.legionWarTaskList_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public LegionWarBaseInfo Clone()
	{
		return new LegionWarBaseInfo(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as LegionWarBaseInfo);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(LegionWarBaseInfo other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (Season != other.Season)
		{
			return false;
		}
		if (!object.Equals(LegionWarOpenInfo, other.LegionWarOpenInfo))
		{
			return false;
		}
		if (!object.Equals(LegionWarZhanLingInfo, other.LegionWarZhanLingInfo))
		{
			return false;
		}
		if (!legionWarTaskList_.Equals(other.legionWarTaskList_))
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
		if (Season != 0)
		{
			num ^= Season.GetHashCode();
		}
		if (legionWarOpenInfo_ != null)
		{
			num ^= LegionWarOpenInfo.GetHashCode();
		}
		if (legionWarZhanLingInfo_ != null)
		{
			num ^= LegionWarZhanLingInfo.GetHashCode();
		}
		num ^= ((object)legionWarTaskList_).GetHashCode();
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
		if (Season != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteInt32(Season);
		}
		if (legionWarOpenInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)18);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)LegionWarOpenInfo);
		}
		if (legionWarZhanLingInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)26);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)LegionWarZhanLingInfo);
		}
		legionWarTaskList_.WriteTo(ref output, _repeated_legionWarTaskList_codec);
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
		if (Season != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(Season);
		}
		if (legionWarOpenInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)LegionWarOpenInfo);
		}
		if (legionWarZhanLingInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)LegionWarZhanLingInfo);
		}
		num += legionWarTaskList_.CalculateSize(_repeated_legionWarTaskList_codec);
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(LegionWarBaseInfo other)
	{
		if (other == null)
		{
			return;
		}
		if (other.Season != 0)
		{
			Season = other.Season;
		}
		if (other.legionWarOpenInfo_ != null)
		{
			if (legionWarOpenInfo_ == null)
			{
				LegionWarOpenInfo = new LegionWarOpenInfo();
			}
			LegionWarOpenInfo.MergeFrom(other.LegionWarOpenInfo);
		}
		if (other.legionWarZhanLingInfo_ != null)
		{
			if (legionWarZhanLingInfo_ == null)
			{
				LegionWarZhanLingInfo = new LegionWarZhanLingInfo();
			}
			LegionWarZhanLingInfo.MergeFrom(other.LegionWarZhanLingInfo);
		}
		legionWarTaskList_.Add((IEnumerable<TaskDetail>)other.legionWarTaskList_);
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
			case 8u:
				Season = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 18u:
				if (legionWarOpenInfo_ == null)
				{
					LegionWarOpenInfo = new LegionWarOpenInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)LegionWarOpenInfo);
				break;
			case 26u:
				if (legionWarZhanLingInfo_ == null)
				{
					LegionWarZhanLingInfo = new LegionWarZhanLingInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)LegionWarZhanLingInfo);
				break;
			case 34u:
				legionWarTaskList_.AddEntriesFrom(ref input, _repeated_legionWarTaskList_codec);
				break;
			}
		}
	}
}
