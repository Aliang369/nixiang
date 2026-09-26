using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class SeasonMarchSpeedUpAck : IMessage<SeasonMarchSpeedUpAck>, IMessage, IEquatable<SeasonMarchSpeedUpAck>, IDeepCloneable<SeasonMarchSpeedUpAck>, IBufferMessage
{
	private static readonly MessageParser<SeasonMarchSpeedUpAck> _parser = new MessageParser<SeasonMarchSpeedUpAck>((Func<SeasonMarchSpeedUpAck>)(() => new SeasonMarchSpeedUpAck()));

	private UnknownFieldSet _unknownFields;

	public const int MoveFieldNumber = 1;

	private SeasonMoveDetail move_;

	public const int CostFieldNumber = 2;

	private int cost_;

	public const int DiamondLeftFieldNumber = 3;

	private int diamondLeft_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<SeasonMarchSpeedUpAck> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeSeasonMarchSpeedUpAckReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SeasonMoveDetail Move
	{
		get
		{
			return move_;
		}
		set
		{
			move_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int Cost
	{
		get
		{
			return cost_;
		}
		set
		{
			cost_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int DiamondLeft
	{
		get
		{
			return diamondLeft_;
		}
		set
		{
			diamondLeft_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SeasonMarchSpeedUpAck()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SeasonMarchSpeedUpAck(SeasonMarchSpeedUpAck other)
		: this()
	{
		move_ = ((other.move_ != null) ? other.move_.Clone() : null);
		cost_ = other.cost_;
		diamondLeft_ = other.diamondLeft_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SeasonMarchSpeedUpAck Clone()
	{
		return new SeasonMarchSpeedUpAck(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as SeasonMarchSpeedUpAck);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(SeasonMarchSpeedUpAck other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!object.Equals(Move, other.Move))
		{
			return false;
		}
		if (Cost != other.Cost)
		{
			return false;
		}
		if (DiamondLeft != other.DiamondLeft)
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
		if (move_ != null)
		{
			num ^= Move.GetHashCode();
		}
		if (Cost != 0)
		{
			num ^= Cost.GetHashCode();
		}
		if (DiamondLeft != 0)
		{
			num ^= DiamondLeft.GetHashCode();
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
		if (move_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)10);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)Move);
		}
		if (Cost != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteInt32(Cost);
		}
		if (DiamondLeft != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)24);
			((WriteContext)(ref output)).WriteInt32(DiamondLeft);
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
		if (move_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)Move);
		}
		if (Cost != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(Cost);
		}
		if (DiamondLeft != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(DiamondLeft);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(SeasonMarchSpeedUpAck other)
	{
		if (other == null)
		{
			return;
		}
		if (other.move_ != null)
		{
			if (move_ == null)
			{
				Move = new SeasonMoveDetail();
			}
			Move.MergeFrom(other.Move);
		}
		if (other.Cost != 0)
		{
			Cost = other.Cost;
		}
		if (other.DiamondLeft != 0)
		{
			DiamondLeft = other.DiamondLeft;
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
				if (move_ == null)
				{
					Move = new SeasonMoveDetail();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)Move);
				break;
			case 16u:
				Cost = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 24u:
				DiamondLeft = ((ParseContext)(ref input)).ReadInt32();
				break;
			}
		}
	}
}
