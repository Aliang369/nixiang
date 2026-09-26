using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class FeatureLegionBoss : IMessage<FeatureLegionBoss>, IMessage, IEquatable<FeatureLegionBoss>, IDeepCloneable<FeatureLegionBoss>, IBufferMessage
{
	private static readonly MessageParser<FeatureLegionBoss> _parser = new MessageParser<FeatureLegionBoss>((Func<FeatureLegionBoss>)(() => new FeatureLegionBoss()));

	private UnknownFieldSet _unknownFields;

	public const int LegionSelfInfoFieldNumber = 1;

	private LegionSelfInfo legionSelfInfo_;

	public const int LegionBossInfoFieldNumber = 2;

	private LegionBossInfo legionBossInfo_;

	public const int SelfRankFieldNumber = 3;

	private int selfRank_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<FeatureLegionBoss> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeFeatureLegionBossReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public LegionSelfInfo LegionSelfInfo
	{
		get
		{
			return legionSelfInfo_;
		}
		set
		{
			legionSelfInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public LegionBossInfo LegionBossInfo
	{
		get
		{
			return legionBossInfo_;
		}
		set
		{
			legionBossInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int SelfRank
	{
		get
		{
			return selfRank_;
		}
		set
		{
			selfRank_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public FeatureLegionBoss()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public FeatureLegionBoss(FeatureLegionBoss other)
		: this()
	{
		legionSelfInfo_ = ((other.legionSelfInfo_ != null) ? other.legionSelfInfo_.Clone() : null);
		legionBossInfo_ = ((other.legionBossInfo_ != null) ? other.legionBossInfo_.Clone() : null);
		selfRank_ = other.selfRank_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public FeatureLegionBoss Clone()
	{
		return new FeatureLegionBoss(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as FeatureLegionBoss);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(FeatureLegionBoss other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!object.Equals(LegionSelfInfo, other.LegionSelfInfo))
		{
			return false;
		}
		if (!object.Equals(LegionBossInfo, other.LegionBossInfo))
		{
			return false;
		}
		if (SelfRank != other.SelfRank)
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
		if (legionSelfInfo_ != null)
		{
			num ^= LegionSelfInfo.GetHashCode();
		}
		if (legionBossInfo_ != null)
		{
			num ^= LegionBossInfo.GetHashCode();
		}
		if (SelfRank != 0)
		{
			num ^= SelfRank.GetHashCode();
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
		if (legionSelfInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)10);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)LegionSelfInfo);
		}
		if (legionBossInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)18);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)LegionBossInfo);
		}
		if (SelfRank != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)24);
			((WriteContext)(ref output)).WriteInt32(SelfRank);
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
		if (legionSelfInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)LegionSelfInfo);
		}
		if (legionBossInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)LegionBossInfo);
		}
		if (SelfRank != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(SelfRank);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(FeatureLegionBoss other)
	{
		if (other == null)
		{
			return;
		}
		if (other.legionSelfInfo_ != null)
		{
			if (legionSelfInfo_ == null)
			{
				LegionSelfInfo = new LegionSelfInfo();
			}
			LegionSelfInfo.MergeFrom(other.LegionSelfInfo);
		}
		if (other.legionBossInfo_ != null)
		{
			if (legionBossInfo_ == null)
			{
				LegionBossInfo = new LegionBossInfo();
			}
			LegionBossInfo.MergeFrom(other.LegionBossInfo);
		}
		if (other.SelfRank != 0)
		{
			SelfRank = other.SelfRank;
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
				if (legionSelfInfo_ == null)
				{
					LegionSelfInfo = new LegionSelfInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)LegionSelfInfo);
				break;
			case 18u:
				if (legionBossInfo_ == null)
				{
					LegionBossInfo = new LegionBossInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)LegionBossInfo);
				break;
			case 24u:
				SelfRank = ((ParseContext)(ref input)).ReadInt32();
				break;
			}
		}
	}
}
