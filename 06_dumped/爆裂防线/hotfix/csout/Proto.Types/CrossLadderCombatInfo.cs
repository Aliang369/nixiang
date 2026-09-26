using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class CrossLadderCombatInfo : IMessage<CrossLadderCombatInfo>, IMessage, IEquatable<CrossLadderCombatInfo>, IDeepCloneable<CrossLadderCombatInfo>, IBufferMessage
{
	private static readonly MessageParser<CrossLadderCombatInfo> _parser = new MessageParser<CrossLadderCombatInfo>((Func<CrossLadderCombatInfo>)(() => new CrossLadderCombatInfo()));

	private UnknownFieldSet _unknownFields;

	public const int StateFieldNumber = 1;

	private ELadderState state_;

	public const int VictoryCountFieldNumber = 2;

	private int victoryCount_;

	public const int UpgradeStateFieldNumber = 3;

	private static readonly FieldCodec<int> _repeated_upgradeState_codec = FieldCodec.ForInt32(26u);

	private readonly RepeatedField<int> upgradeState_ = new RepeatedField<int>();

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<CrossLadderCombatInfo> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeCrossLadderCombatInfoReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ELadderState State
	{
		get
		{
			return state_;
		}
		set
		{
			state_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int VictoryCount
	{
		get
		{
			return victoryCount_;
		}
		set
		{
			victoryCount_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<int> UpgradeState => upgradeState_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public CrossLadderCombatInfo()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public CrossLadderCombatInfo(CrossLadderCombatInfo other)
		: this()
	{
		state_ = other.state_;
		victoryCount_ = other.victoryCount_;
		upgradeState_ = other.upgradeState_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public CrossLadderCombatInfo Clone()
	{
		return new CrossLadderCombatInfo(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as CrossLadderCombatInfo);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(CrossLadderCombatInfo other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (State != other.State)
		{
			return false;
		}
		if (VictoryCount != other.VictoryCount)
		{
			return false;
		}
		if (!upgradeState_.Equals(other.upgradeState_))
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
		if (State != ELadderState.EmptyValue)
		{
			num ^= State.GetHashCode();
		}
		if (VictoryCount != 0)
		{
			num ^= VictoryCount.GetHashCode();
		}
		num ^= ((object)upgradeState_).GetHashCode();
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
		if (State != ELadderState.EmptyValue)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteEnum((int)State);
		}
		if (VictoryCount != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteInt32(VictoryCount);
		}
		upgradeState_.WriteTo(ref output, _repeated_upgradeState_codec);
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
		if (State != ELadderState.EmptyValue)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)State);
		}
		if (VictoryCount != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(VictoryCount);
		}
		num += upgradeState_.CalculateSize(_repeated_upgradeState_codec);
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(CrossLadderCombatInfo other)
	{
		if (other != null)
		{
			if (other.State != ELadderState.EmptyValue)
			{
				State = other.State;
			}
			if (other.VictoryCount != 0)
			{
				VictoryCount = other.VictoryCount;
			}
			upgradeState_.Add((IEnumerable<int>)other.upgradeState_);
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
				State = (ELadderState)((ParseContext)(ref input)).ReadEnum();
				break;
			case 16u:
				VictoryCount = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 24u:
			case 26u:
				upgradeState_.AddEntriesFrom(ref input, _repeated_upgradeState_codec);
				break;
			}
		}
	}
}
