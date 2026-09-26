using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class FeatureEndlessHorde : IMessage<FeatureEndlessHorde>, IMessage, IEquatable<FeatureEndlessHorde>, IDeepCloneable<FeatureEndlessHorde>, IBufferMessage
{
	private static readonly MessageParser<FeatureEndlessHorde> _parser = new MessageParser<FeatureEndlessHorde>((Func<FeatureEndlessHorde>)(() => new FeatureEndlessHorde()));

	private UnknownFieldSet _unknownFields;

	public const int EndlessHordeInfoFieldNumber = 1;

	private EndlessHordeInfo endlessHordeInfo_;

	public const int SelfRankFieldNumber = 2;

	private int selfRank_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<FeatureEndlessHorde> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeFeatureEndlessHordeReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public EndlessHordeInfo EndlessHordeInfo
	{
		get
		{
			return endlessHordeInfo_;
		}
		set
		{
			endlessHordeInfo_ = value;
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
	public FeatureEndlessHorde()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public FeatureEndlessHorde(FeatureEndlessHorde other)
		: this()
	{
		endlessHordeInfo_ = ((other.endlessHordeInfo_ != null) ? other.endlessHordeInfo_.Clone() : null);
		selfRank_ = other.selfRank_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public FeatureEndlessHorde Clone()
	{
		return new FeatureEndlessHorde(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as FeatureEndlessHorde);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(FeatureEndlessHorde other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!object.Equals(EndlessHordeInfo, other.EndlessHordeInfo))
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
		if (endlessHordeInfo_ != null)
		{
			num ^= EndlessHordeInfo.GetHashCode();
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
		if (endlessHordeInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)10);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)EndlessHordeInfo);
		}
		if (SelfRank != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
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
		if (endlessHordeInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)EndlessHordeInfo);
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
	public void MergeFrom(FeatureEndlessHorde other)
	{
		if (other == null)
		{
			return;
		}
		if (other.endlessHordeInfo_ != null)
		{
			if (endlessHordeInfo_ == null)
			{
				EndlessHordeInfo = new EndlessHordeInfo();
			}
			EndlessHordeInfo.MergeFrom(other.EndlessHordeInfo);
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
				if (endlessHordeInfo_ == null)
				{
					EndlessHordeInfo = new EndlessHordeInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)EndlessHordeInfo);
				break;
			case 16u:
				SelfRank = ((ParseContext)(ref input)).ReadInt32();
				break;
			}
		}
	}
}
