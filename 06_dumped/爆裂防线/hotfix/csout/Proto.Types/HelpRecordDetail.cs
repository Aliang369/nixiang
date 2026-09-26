using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class HelpRecordDetail : IMessage<HelpRecordDetail>, IMessage, IEquatable<HelpRecordDetail>, IDeepCloneable<HelpRecordDetail>, IBufferMessage
{
	private static readonly MessageParser<HelpRecordDetail> _parser = new MessageParser<HelpRecordDetail>((Func<HelpRecordDetail>)(() => new HelpRecordDetail()));

	private UnknownFieldSet _unknownFields;

	public const int RecordIdFieldNumber = 1;

	private int recordId_;

	public const int EliteLevelFieldNumber = 2;

	private int eliteLevel_;

	public const int RecordTmFieldNumber = 3;

	private long recordTm_;

	public const int RoleBaseInfoFieldNumber = 4;

	private RoleBaseInfo roleBaseInfo_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<HelpRecordDetail> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeHelpRecordDetailReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int RecordId
	{
		get
		{
			return recordId_;
		}
		set
		{
			recordId_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int EliteLevel
	{
		get
		{
			return eliteLevel_;
		}
		set
		{
			eliteLevel_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public long RecordTm
	{
		get
		{
			return recordTm_;
		}
		set
		{
			recordTm_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RoleBaseInfo RoleBaseInfo
	{
		get
		{
			return roleBaseInfo_;
		}
		set
		{
			roleBaseInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public HelpRecordDetail()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public HelpRecordDetail(HelpRecordDetail other)
		: this()
	{
		recordId_ = other.recordId_;
		eliteLevel_ = other.eliteLevel_;
		recordTm_ = other.recordTm_;
		roleBaseInfo_ = ((other.roleBaseInfo_ != null) ? other.roleBaseInfo_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public HelpRecordDetail Clone()
	{
		return new HelpRecordDetail(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as HelpRecordDetail);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(HelpRecordDetail other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (RecordId != other.RecordId)
		{
			return false;
		}
		if (EliteLevel != other.EliteLevel)
		{
			return false;
		}
		if (RecordTm != other.RecordTm)
		{
			return false;
		}
		if (!object.Equals(RoleBaseInfo, other.RoleBaseInfo))
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
		if (RecordId != 0)
		{
			num ^= RecordId.GetHashCode();
		}
		if (EliteLevel != 0)
		{
			num ^= EliteLevel.GetHashCode();
		}
		if (RecordTm != 0L)
		{
			num ^= RecordTm.GetHashCode();
		}
		if (roleBaseInfo_ != null)
		{
			num ^= RoleBaseInfo.GetHashCode();
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
		if (RecordId != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteInt32(RecordId);
		}
		if (EliteLevel != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteInt32(EliteLevel);
		}
		if (RecordTm != 0L)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)24);
			((WriteContext)(ref output)).WriteInt64(RecordTm);
		}
		if (roleBaseInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)34);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)RoleBaseInfo);
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
		if (RecordId != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(RecordId);
		}
		if (EliteLevel != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(EliteLevel);
		}
		if (RecordTm != 0L)
		{
			num += 1 + CodedOutputStream.ComputeInt64Size(RecordTm);
		}
		if (roleBaseInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)RoleBaseInfo);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(HelpRecordDetail other)
	{
		if (other == null)
		{
			return;
		}
		if (other.RecordId != 0)
		{
			RecordId = other.RecordId;
		}
		if (other.EliteLevel != 0)
		{
			EliteLevel = other.EliteLevel;
		}
		if (other.RecordTm != 0L)
		{
			RecordTm = other.RecordTm;
		}
		if (other.roleBaseInfo_ != null)
		{
			if (roleBaseInfo_ == null)
			{
				RoleBaseInfo = new RoleBaseInfo();
			}
			RoleBaseInfo.MergeFrom(other.RoleBaseInfo);
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
			case 8u:
				RecordId = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 16u:
				EliteLevel = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 24u:
				RecordTm = ((ParseContext)(ref input)).ReadInt64();
				break;
			case 34u:
				if (roleBaseInfo_ == null)
				{
					RoleBaseInfo = new RoleBaseInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)RoleBaseInfo);
				break;
			}
		}
	}
}
