using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class FeatureWorldLair : IMessage<FeatureWorldLair>, IMessage, IEquatable<FeatureWorldLair>, IDeepCloneable<FeatureWorldLair>, IBufferMessage
{
	private static readonly MessageParser<FeatureWorldLair> _parser = new MessageParser<FeatureWorldLair>((Func<FeatureWorldLair>)(() => new FeatureWorldLair()));

	private UnknownFieldSet _unknownFields;

	public const int WorldLairInfoFieldNumber = 1;

	private WorldLairInfo worldLairInfo_;

	public const int HelpCountFieldNumber = 2;

	private int helpCount_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<FeatureWorldLair> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeFeatureWorldLairReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public WorldLairInfo WorldLairInfo
	{
		get
		{
			return worldLairInfo_;
		}
		set
		{
			worldLairInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int HelpCount
	{
		get
		{
			return helpCount_;
		}
		set
		{
			helpCount_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public FeatureWorldLair()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public FeatureWorldLair(FeatureWorldLair other)
		: this()
	{
		worldLairInfo_ = ((other.worldLairInfo_ != null) ? other.worldLairInfo_.Clone() : null);
		helpCount_ = other.helpCount_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public FeatureWorldLair Clone()
	{
		return new FeatureWorldLair(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as FeatureWorldLair);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(FeatureWorldLair other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!object.Equals(WorldLairInfo, other.WorldLairInfo))
		{
			return false;
		}
		if (HelpCount != other.HelpCount)
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
		if (worldLairInfo_ != null)
		{
			num ^= WorldLairInfo.GetHashCode();
		}
		if (HelpCount != 0)
		{
			num ^= HelpCount.GetHashCode();
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
		if (worldLairInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)10);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)WorldLairInfo);
		}
		if (HelpCount != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteInt32(HelpCount);
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
		if (worldLairInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)WorldLairInfo);
		}
		if (HelpCount != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(HelpCount);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(FeatureWorldLair other)
	{
		if (other == null)
		{
			return;
		}
		if (other.worldLairInfo_ != null)
		{
			if (worldLairInfo_ == null)
			{
				WorldLairInfo = new WorldLairInfo();
			}
			WorldLairInfo.MergeFrom(other.WorldLairInfo);
		}
		if (other.HelpCount != 0)
		{
			HelpCount = other.HelpCount;
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
				if (worldLairInfo_ == null)
				{
					WorldLairInfo = new WorldLairInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)WorldLairInfo);
				break;
			case 16u:
				HelpCount = ((ParseContext)(ref input)).ReadInt32();
				break;
			}
		}
	}
}
