using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class CrossLadderUpgradeFightResultAck : IMessage<CrossLadderUpgradeFightResultAck>, IMessage, IEquatable<CrossLadderUpgradeFightResultAck>, IDeepCloneable<CrossLadderUpgradeFightResultAck>, IBufferMessage
{
	private static readonly MessageParser<CrossLadderUpgradeFightResultAck> _parser = new MessageParser<CrossLadderUpgradeFightResultAck>((Func<CrossLadderUpgradeFightResultAck>)(() => new CrossLadderUpgradeFightResultAck()));

	private UnknownFieldSet _unknownFields;

	public const int CrossLadderInfoFieldNumber = 1;

	private CrossLadderInfo crossLadderInfo_;

	public const int BattleSettleFieldNumber = 2;

	private BattleResultInfo battleSettle_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<CrossLadderUpgradeFightResultAck> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeCrossLadderUpgradeFightResultAckReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public CrossLadderInfo CrossLadderInfo
	{
		get
		{
			return crossLadderInfo_;
		}
		set
		{
			crossLadderInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public BattleResultInfo BattleSettle
	{
		get
		{
			return battleSettle_;
		}
		set
		{
			battleSettle_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public CrossLadderUpgradeFightResultAck()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public CrossLadderUpgradeFightResultAck(CrossLadderUpgradeFightResultAck other)
		: this()
	{
		crossLadderInfo_ = ((other.crossLadderInfo_ != null) ? other.crossLadderInfo_.Clone() : null);
		battleSettle_ = ((other.battleSettle_ != null) ? other.battleSettle_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public CrossLadderUpgradeFightResultAck Clone()
	{
		return new CrossLadderUpgradeFightResultAck(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as CrossLadderUpgradeFightResultAck);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(CrossLadderUpgradeFightResultAck other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!object.Equals(CrossLadderInfo, other.CrossLadderInfo))
		{
			return false;
		}
		if (!object.Equals(BattleSettle, other.BattleSettle))
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
		if (crossLadderInfo_ != null)
		{
			num ^= CrossLadderInfo.GetHashCode();
		}
		if (battleSettle_ != null)
		{
			num ^= BattleSettle.GetHashCode();
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
		if (crossLadderInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)10);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)CrossLadderInfo);
		}
		if (battleSettle_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)18);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)BattleSettle);
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
		if (crossLadderInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)CrossLadderInfo);
		}
		if (battleSettle_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)BattleSettle);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(CrossLadderUpgradeFightResultAck other)
	{
		if (other == null)
		{
			return;
		}
		if (other.crossLadderInfo_ != null)
		{
			if (crossLadderInfo_ == null)
			{
				CrossLadderInfo = new CrossLadderInfo();
			}
			CrossLadderInfo.MergeFrom(other.CrossLadderInfo);
		}
		if (other.battleSettle_ != null)
		{
			if (battleSettle_ == null)
			{
				BattleSettle = new BattleResultInfo();
			}
			BattleSettle.MergeFrom(other.BattleSettle);
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
				if (crossLadderInfo_ == null)
				{
					CrossLadderInfo = new CrossLadderInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)CrossLadderInfo);
				break;
			case 18u:
				if (battleSettle_ == null)
				{
					BattleSettle = new BattleResultInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)BattleSettle);
				break;
			}
		}
	}
}
