using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class FeatureListAck : IMessage<FeatureListAck>, IMessage, IEquatable<FeatureListAck>, IDeepCloneable<FeatureListAck>, IBufferMessage
{
	private static readonly MessageParser<FeatureListAck> _parser = new MessageParser<FeatureListAck>((Func<FeatureListAck>)(() => new FeatureListAck()));

	private UnknownFieldSet _unknownFields;

	public const int FeatureWorldLairFieldNumber = 1;

	private FeatureWorldLair featureWorldLair_;

	public const int FeatureLegionBossFieldNumber = 2;

	private FeatureLegionBoss featureLegionBoss_;

	public const int FeatureTrialInfoFieldNumber = 3;

	private HeroTrialInfo featureTrialInfo_;

	public const int FeatureWorldBossFieldNumber = 4;

	private FeatureWorldBoss featureWorldBoss_;

	public const int FeatureEndlessHordeFieldNumber = 5;

	private FeatureEndlessHorde featureEndlessHorde_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<FeatureListAck> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeFeatureListAckReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public FeatureWorldLair FeatureWorldLair
	{
		get
		{
			return featureWorldLair_;
		}
		set
		{
			featureWorldLair_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public FeatureLegionBoss FeatureLegionBoss
	{
		get
		{
			return featureLegionBoss_;
		}
		set
		{
			featureLegionBoss_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public HeroTrialInfo FeatureTrialInfo
	{
		get
		{
			return featureTrialInfo_;
		}
		set
		{
			featureTrialInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public FeatureWorldBoss FeatureWorldBoss
	{
		get
		{
			return featureWorldBoss_;
		}
		set
		{
			featureWorldBoss_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public FeatureEndlessHorde FeatureEndlessHorde
	{
		get
		{
			return featureEndlessHorde_;
		}
		set
		{
			featureEndlessHorde_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public FeatureListAck()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public FeatureListAck(FeatureListAck other)
		: this()
	{
		featureWorldLair_ = ((other.featureWorldLair_ != null) ? other.featureWorldLair_.Clone() : null);
		featureLegionBoss_ = ((other.featureLegionBoss_ != null) ? other.featureLegionBoss_.Clone() : null);
		featureTrialInfo_ = ((other.featureTrialInfo_ != null) ? other.featureTrialInfo_.Clone() : null);
		featureWorldBoss_ = ((other.featureWorldBoss_ != null) ? other.featureWorldBoss_.Clone() : null);
		featureEndlessHorde_ = ((other.featureEndlessHorde_ != null) ? other.featureEndlessHorde_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public FeatureListAck Clone()
	{
		return new FeatureListAck(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as FeatureListAck);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(FeatureListAck other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!object.Equals(FeatureWorldLair, other.FeatureWorldLair))
		{
			return false;
		}
		if (!object.Equals(FeatureLegionBoss, other.FeatureLegionBoss))
		{
			return false;
		}
		if (!object.Equals(FeatureTrialInfo, other.FeatureTrialInfo))
		{
			return false;
		}
		if (!object.Equals(FeatureWorldBoss, other.FeatureWorldBoss))
		{
			return false;
		}
		if (!object.Equals(FeatureEndlessHorde, other.FeatureEndlessHorde))
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
		if (featureWorldLair_ != null)
		{
			num ^= FeatureWorldLair.GetHashCode();
		}
		if (featureLegionBoss_ != null)
		{
			num ^= FeatureLegionBoss.GetHashCode();
		}
		if (featureTrialInfo_ != null)
		{
			num ^= FeatureTrialInfo.GetHashCode();
		}
		if (featureWorldBoss_ != null)
		{
			num ^= FeatureWorldBoss.GetHashCode();
		}
		if (featureEndlessHorde_ != null)
		{
			num ^= FeatureEndlessHorde.GetHashCode();
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
		if (featureWorldLair_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)10);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)FeatureWorldLair);
		}
		if (featureLegionBoss_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)18);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)FeatureLegionBoss);
		}
		if (featureTrialInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)26);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)FeatureTrialInfo);
		}
		if (featureWorldBoss_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)34);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)FeatureWorldBoss);
		}
		if (featureEndlessHorde_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)42);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)FeatureEndlessHorde);
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
		if (featureWorldLair_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)FeatureWorldLair);
		}
		if (featureLegionBoss_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)FeatureLegionBoss);
		}
		if (featureTrialInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)FeatureTrialInfo);
		}
		if (featureWorldBoss_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)FeatureWorldBoss);
		}
		if (featureEndlessHorde_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)FeatureEndlessHorde);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(FeatureListAck other)
	{
		if (other == null)
		{
			return;
		}
		if (other.featureWorldLair_ != null)
		{
			if (featureWorldLair_ == null)
			{
				FeatureWorldLair = new FeatureWorldLair();
			}
			FeatureWorldLair.MergeFrom(other.FeatureWorldLair);
		}
		if (other.featureLegionBoss_ != null)
		{
			if (featureLegionBoss_ == null)
			{
				FeatureLegionBoss = new FeatureLegionBoss();
			}
			FeatureLegionBoss.MergeFrom(other.FeatureLegionBoss);
		}
		if (other.featureTrialInfo_ != null)
		{
			if (featureTrialInfo_ == null)
			{
				FeatureTrialInfo = new HeroTrialInfo();
			}
			FeatureTrialInfo.MergeFrom(other.FeatureTrialInfo);
		}
		if (other.featureWorldBoss_ != null)
		{
			if (featureWorldBoss_ == null)
			{
				FeatureWorldBoss = new FeatureWorldBoss();
			}
			FeatureWorldBoss.MergeFrom(other.FeatureWorldBoss);
		}
		if (other.featureEndlessHorde_ != null)
		{
			if (featureEndlessHorde_ == null)
			{
				FeatureEndlessHorde = new FeatureEndlessHorde();
			}
			FeatureEndlessHorde.MergeFrom(other.FeatureEndlessHorde);
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
				if (featureWorldLair_ == null)
				{
					FeatureWorldLair = new FeatureWorldLair();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)FeatureWorldLair);
				break;
			case 18u:
				if (featureLegionBoss_ == null)
				{
					FeatureLegionBoss = new FeatureLegionBoss();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)FeatureLegionBoss);
				break;
			case 26u:
				if (featureTrialInfo_ == null)
				{
					FeatureTrialInfo = new HeroTrialInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)FeatureTrialInfo);
				break;
			case 34u:
				if (featureWorldBoss_ == null)
				{
					FeatureWorldBoss = new FeatureWorldBoss();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)FeatureWorldBoss);
				break;
			case 42u:
				if (featureEndlessHorde_ == null)
				{
					FeatureEndlessHorde = new FeatureEndlessHorde();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)FeatureEndlessHorde);
				break;
			}
		}
	}
}
