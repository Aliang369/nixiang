using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class EndlessHordeFightForwardAck : IMessage<EndlessHordeFightForwardAck>, IMessage, IEquatable<EndlessHordeFightForwardAck>, IDeepCloneable<EndlessHordeFightForwardAck>, IBufferMessage
{
	private static readonly MessageParser<EndlessHordeFightForwardAck> _parser = new MessageParser<EndlessHordeFightForwardAck>((Func<EndlessHordeFightForwardAck>)(() => new EndlessHordeFightForwardAck()));

	private UnknownFieldSet _unknownFields;

	public const int BestDistanceFieldNumber = 1;

	private EBoolState bestDistance_;

	public const int EndlessHordeInfoFieldNumber = 2;

	private EndlessHordeInfo endlessHordeInfo_;

	public const int NextLevelIdFieldNumber = 3;

	private int nextLevelId_;

	public const int NextBattleInfoFieldNumber = 4;

	private BattleInfo nextBattleInfo_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<EndlessHordeFightForwardAck> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeEndlessHordeFightForwardAckReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public EBoolState BestDistance
	{
		get
		{
			return bestDistance_;
		}
		set
		{
			bestDistance_ = value;
		}
	}

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
	public int NextLevelId
	{
		get
		{
			return nextLevelId_;
		}
		set
		{
			nextLevelId_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public BattleInfo NextBattleInfo
	{
		get
		{
			return nextBattleInfo_;
		}
		set
		{
			nextBattleInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public EndlessHordeFightForwardAck()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public EndlessHordeFightForwardAck(EndlessHordeFightForwardAck other)
		: this()
	{
		bestDistance_ = other.bestDistance_;
		endlessHordeInfo_ = ((other.endlessHordeInfo_ != null) ? other.endlessHordeInfo_.Clone() : null);
		nextLevelId_ = other.nextLevelId_;
		nextBattleInfo_ = ((other.nextBattleInfo_ != null) ? other.nextBattleInfo_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public EndlessHordeFightForwardAck Clone()
	{
		return new EndlessHordeFightForwardAck(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as EndlessHordeFightForwardAck);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(EndlessHordeFightForwardAck other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (BestDistance != other.BestDistance)
		{
			return false;
		}
		if (!object.Equals(EndlessHordeInfo, other.EndlessHordeInfo))
		{
			return false;
		}
		if (NextLevelId != other.NextLevelId)
		{
			return false;
		}
		if (!object.Equals(NextBattleInfo, other.NextBattleInfo))
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
		if (BestDistance != EBoolState.False)
		{
			num ^= BestDistance.GetHashCode();
		}
		if (endlessHordeInfo_ != null)
		{
			num ^= EndlessHordeInfo.GetHashCode();
		}
		if (NextLevelId != 0)
		{
			num ^= NextLevelId.GetHashCode();
		}
		if (nextBattleInfo_ != null)
		{
			num ^= NextBattleInfo.GetHashCode();
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
		if (BestDistance != EBoolState.False)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteEnum((int)BestDistance);
		}
		if (endlessHordeInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)18);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)EndlessHordeInfo);
		}
		if (NextLevelId != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)24);
			((WriteContext)(ref output)).WriteInt32(NextLevelId);
		}
		if (nextBattleInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)34);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)NextBattleInfo);
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
		if (BestDistance != EBoolState.False)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)BestDistance);
		}
		if (endlessHordeInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)EndlessHordeInfo);
		}
		if (NextLevelId != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(NextLevelId);
		}
		if (nextBattleInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)NextBattleInfo);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(EndlessHordeFightForwardAck other)
	{
		if (other == null)
		{
			return;
		}
		if (other.BestDistance != EBoolState.False)
		{
			BestDistance = other.BestDistance;
		}
		if (other.endlessHordeInfo_ != null)
		{
			if (endlessHordeInfo_ == null)
			{
				EndlessHordeInfo = new EndlessHordeInfo();
			}
			EndlessHordeInfo.MergeFrom(other.EndlessHordeInfo);
		}
		if (other.NextLevelId != 0)
		{
			NextLevelId = other.NextLevelId;
		}
		if (other.nextBattleInfo_ != null)
		{
			if (nextBattleInfo_ == null)
			{
				NextBattleInfo = new BattleInfo();
			}
			NextBattleInfo.MergeFrom(other.NextBattleInfo);
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
				BestDistance = (EBoolState)((ParseContext)(ref input)).ReadEnum();
				break;
			case 18u:
				if (endlessHordeInfo_ == null)
				{
					EndlessHordeInfo = new EndlessHordeInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)EndlessHordeInfo);
				break;
			case 24u:
				NextLevelId = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 34u:
				if (nextBattleInfo_ == null)
				{
					NextBattleInfo = new BattleInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)NextBattleInfo);
				break;
			}
		}
	}
}
