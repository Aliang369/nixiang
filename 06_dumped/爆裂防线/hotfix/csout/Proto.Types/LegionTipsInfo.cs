using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class LegionTipsInfo : IMessage<LegionTipsInfo>, IMessage, IEquatable<LegionTipsInfo>, IDeepCloneable<LegionTipsInfo>, IBufferMessage
{
	private static readonly MessageParser<LegionTipsInfo> _parser = new MessageParser<LegionTipsInfo>((Func<LegionTipsInfo>)(() => new LegionTipsInfo()));

	private UnknownFieldSet _unknownFields;

	public const int LegionBaseInfoFieldNumber = 1;

	private LegionBaseInfo legionBaseInfo_;

	public const int LegionSetInfoFieldNumber = 2;

	private LegionSetInfo legionSetInfo_;

	public const int LeaderBaseInfoFieldNumber = 3;

	private RoleBaseInfo leaderBaseInfo_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<LegionTipsInfo> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeLegionTipsInfoReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public LegionBaseInfo LegionBaseInfo
	{
		get
		{
			return legionBaseInfo_;
		}
		set
		{
			legionBaseInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public LegionSetInfo LegionSetInfo
	{
		get
		{
			return legionSetInfo_;
		}
		set
		{
			legionSetInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RoleBaseInfo LeaderBaseInfo
	{
		get
		{
			return leaderBaseInfo_;
		}
		set
		{
			leaderBaseInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public LegionTipsInfo()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public LegionTipsInfo(LegionTipsInfo other)
		: this()
	{
		legionBaseInfo_ = ((other.legionBaseInfo_ != null) ? other.legionBaseInfo_.Clone() : null);
		legionSetInfo_ = ((other.legionSetInfo_ != null) ? other.legionSetInfo_.Clone() : null);
		leaderBaseInfo_ = ((other.leaderBaseInfo_ != null) ? other.leaderBaseInfo_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public LegionTipsInfo Clone()
	{
		return new LegionTipsInfo(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as LegionTipsInfo);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(LegionTipsInfo other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!object.Equals(LegionBaseInfo, other.LegionBaseInfo))
		{
			return false;
		}
		if (!object.Equals(LegionSetInfo, other.LegionSetInfo))
		{
			return false;
		}
		if (!object.Equals(LeaderBaseInfo, other.LeaderBaseInfo))
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
		if (legionBaseInfo_ != null)
		{
			num ^= LegionBaseInfo.GetHashCode();
		}
		if (legionSetInfo_ != null)
		{
			num ^= LegionSetInfo.GetHashCode();
		}
		if (leaderBaseInfo_ != null)
		{
			num ^= LeaderBaseInfo.GetHashCode();
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
		if (legionBaseInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)10);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)LegionBaseInfo);
		}
		if (legionSetInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)18);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)LegionSetInfo);
		}
		if (leaderBaseInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)26);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)LeaderBaseInfo);
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
		if (legionBaseInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)LegionBaseInfo);
		}
		if (legionSetInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)LegionSetInfo);
		}
		if (leaderBaseInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)LeaderBaseInfo);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(LegionTipsInfo other)
	{
		if (other == null)
		{
			return;
		}
		if (other.legionBaseInfo_ != null)
		{
			if (legionBaseInfo_ == null)
			{
				LegionBaseInfo = new LegionBaseInfo();
			}
			LegionBaseInfo.MergeFrom(other.LegionBaseInfo);
		}
		if (other.legionSetInfo_ != null)
		{
			if (legionSetInfo_ == null)
			{
				LegionSetInfo = new LegionSetInfo();
			}
			LegionSetInfo.MergeFrom(other.LegionSetInfo);
		}
		if (other.leaderBaseInfo_ != null)
		{
			if (leaderBaseInfo_ == null)
			{
				LeaderBaseInfo = new RoleBaseInfo();
			}
			LeaderBaseInfo.MergeFrom(other.LeaderBaseInfo);
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
				if (legionBaseInfo_ == null)
				{
					LegionBaseInfo = new LegionBaseInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)LegionBaseInfo);
				break;
			case 18u:
				if (legionSetInfo_ == null)
				{
					LegionSetInfo = new LegionSetInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)LegionSetInfo);
				break;
			case 26u:
				if (leaderBaseInfo_ == null)
				{
					LeaderBaseInfo = new RoleBaseInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)LeaderBaseInfo);
				break;
			}
		}
	}
}
