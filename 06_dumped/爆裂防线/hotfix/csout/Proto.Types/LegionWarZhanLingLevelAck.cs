using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class LegionWarZhanLingLevelAck : IMessage<LegionWarZhanLingLevelAck>, IMessage, IEquatable<LegionWarZhanLingLevelAck>, IDeepCloneable<LegionWarZhanLingLevelAck>, IBufferMessage
{
	private static readonly MessageParser<LegionWarZhanLingLevelAck> _parser = new MessageParser<LegionWarZhanLingLevelAck>((Func<LegionWarZhanLingLevelAck>)(() => new LegionWarZhanLingLevelAck()));

	private UnknownFieldSet _unknownFields;

	public const int LegionWarBaseInfoFieldNumber = 1;

	private LegionWarBaseInfo legionWarBaseInfo_;

	public const int LegionWarSelfTeamInfoFieldNumber = 2;

	private LegionWarSelfTeamInfo legionWarSelfTeamInfo_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<LegionWarZhanLingLevelAck> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeLegionWarZhanLingLevelAckReflection.Descriptor.MessageTypes[0];

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
	public LegionWarSelfTeamInfo LegionWarSelfTeamInfo
	{
		get
		{
			return legionWarSelfTeamInfo_;
		}
		set
		{
			legionWarSelfTeamInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public LegionWarZhanLingLevelAck()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public LegionWarZhanLingLevelAck(LegionWarZhanLingLevelAck other)
		: this()
	{
		legionWarBaseInfo_ = ((other.legionWarBaseInfo_ != null) ? other.legionWarBaseInfo_.Clone() : null);
		legionWarSelfTeamInfo_ = ((other.legionWarSelfTeamInfo_ != null) ? other.legionWarSelfTeamInfo_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public LegionWarZhanLingLevelAck Clone()
	{
		return new LegionWarZhanLingLevelAck(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as LegionWarZhanLingLevelAck);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(LegionWarZhanLingLevelAck other)
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
		if (!object.Equals(LegionWarSelfTeamInfo, other.LegionWarSelfTeamInfo))
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
		if (legionWarSelfTeamInfo_ != null)
		{
			num ^= LegionWarSelfTeamInfo.GetHashCode();
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
		if (legionWarSelfTeamInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)18);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)LegionWarSelfTeamInfo);
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
		if (legionWarSelfTeamInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)LegionWarSelfTeamInfo);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(LegionWarZhanLingLevelAck other)
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
		if (other.legionWarSelfTeamInfo_ != null)
		{
			if (legionWarSelfTeamInfo_ == null)
			{
				LegionWarSelfTeamInfo = new LegionWarSelfTeamInfo();
			}
			LegionWarSelfTeamInfo.MergeFrom(other.LegionWarSelfTeamInfo);
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
				if (legionWarSelfTeamInfo_ == null)
				{
					LegionWarSelfTeamInfo = new LegionWarSelfTeamInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)LegionWarSelfTeamInfo);
				break;
			}
		}
	}
}
