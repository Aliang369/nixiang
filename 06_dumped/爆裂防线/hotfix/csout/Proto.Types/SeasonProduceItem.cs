using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class SeasonProduceItem : IMessage<SeasonProduceItem>, IMessage, IEquatable<SeasonProduceItem>, IDeepCloneable<SeasonProduceItem>, IBufferMessage
{
	private static readonly MessageParser<SeasonProduceItem> _parser = new MessageParser<SeasonProduceItem>((Func<SeasonProduceItem>)(() => new SeasonProduceItem()));

	private UnknownFieldSet _unknownFields;

	public const int ResNodeIdFieldNumber = 1;

	private int resNodeId_;

	public const int ResTypeFieldNumber = 2;

	private ESeasonResType resType_;

	public const int GainFieldNumber = 3;

	private int gain_;

	public const int ActiveFieldNumber = 4;

	private int active_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<SeasonProduceItem> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeSeasonProduceItemReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int ResNodeId
	{
		get
		{
			return resNodeId_;
		}
		set
		{
			resNodeId_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ESeasonResType ResType
	{
		get
		{
			return resType_;
		}
		set
		{
			resType_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int Gain
	{
		get
		{
			return gain_;
		}
		set
		{
			gain_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int Active
	{
		get
		{
			return active_;
		}
		set
		{
			active_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SeasonProduceItem()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SeasonProduceItem(SeasonProduceItem other)
		: this()
	{
		resNodeId_ = other.resNodeId_;
		resType_ = other.resType_;
		gain_ = other.gain_;
		active_ = other.active_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SeasonProduceItem Clone()
	{
		return new SeasonProduceItem(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as SeasonProduceItem);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(SeasonProduceItem other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (ResNodeId != other.ResNodeId)
		{
			return false;
		}
		if (ResType != other.ResType)
		{
			return false;
		}
		if (Gain != other.Gain)
		{
			return false;
		}
		if (Active != other.Active)
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
		if (ResNodeId != 0)
		{
			num ^= ResNodeId.GetHashCode();
		}
		if (ResType != ESeasonResType.EmptyValue)
		{
			num ^= ResType.GetHashCode();
		}
		if (Gain != 0)
		{
			num ^= Gain.GetHashCode();
		}
		if (Active != 0)
		{
			num ^= Active.GetHashCode();
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
		if (ResNodeId != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteInt32(ResNodeId);
		}
		if (ResType != ESeasonResType.EmptyValue)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteEnum((int)ResType);
		}
		if (Gain != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)24);
			((WriteContext)(ref output)).WriteInt32(Gain);
		}
		if (Active != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)32);
			((WriteContext)(ref output)).WriteInt32(Active);
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
		if (ResNodeId != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(ResNodeId);
		}
		if (ResType != ESeasonResType.EmptyValue)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)ResType);
		}
		if (Gain != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(Gain);
		}
		if (Active != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(Active);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(SeasonProduceItem other)
	{
		if (other != null)
		{
			if (other.ResNodeId != 0)
			{
				ResNodeId = other.ResNodeId;
			}
			if (other.ResType != ESeasonResType.EmptyValue)
			{
				ResType = other.ResType;
			}
			if (other.Gain != 0)
			{
				Gain = other.Gain;
			}
			if (other.Active != 0)
			{
				Active = other.Active;
			}
			_unknownFields = UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
		}
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
				ResNodeId = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 16u:
				ResType = (ESeasonResType)((ParseContext)(ref input)).ReadEnum();
				break;
			case 24u:
				Gain = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 32u:
				Active = ((ParseContext)(ref input)).ReadInt32();
				break;
			}
		}
	}
}
