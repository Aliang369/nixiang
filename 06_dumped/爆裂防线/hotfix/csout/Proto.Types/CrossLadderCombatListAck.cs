using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class CrossLadderCombatListAck : IMessage<CrossLadderCombatListAck>, IMessage, IEquatable<CrossLadderCombatListAck>, IDeepCloneable<CrossLadderCombatListAck>, IBufferMessage
{
	private static readonly MessageParser<CrossLadderCombatListAck> _parser = new MessageParser<CrossLadderCombatListAck>((Func<CrossLadderCombatListAck>)(() => new CrossLadderCombatListAck()));

	private UnknownFieldSet _unknownFields;

	public const int CrossLadderInfoFieldNumber = 1;

	private CrossLadderInfo crossLadderInfo_;

	public const int CombatListFieldNumber = 2;

	private static readonly FieldCodec<CrossLadderPlayerInfo> _repeated_combatList_codec = FieldCodec.ForMessage<CrossLadderPlayerInfo>(18u, CrossLadderPlayerInfo.Parser);

	private readonly RepeatedField<CrossLadderPlayerInfo> combatList_ = new RepeatedField<CrossLadderPlayerInfo>();

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<CrossLadderCombatListAck> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeCrossLadderCombatListAckReflection.Descriptor.MessageTypes[0];

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
	public RepeatedField<CrossLadderPlayerInfo> CombatList => combatList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public CrossLadderCombatListAck()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public CrossLadderCombatListAck(CrossLadderCombatListAck other)
		: this()
	{
		crossLadderInfo_ = ((other.crossLadderInfo_ != null) ? other.crossLadderInfo_.Clone() : null);
		combatList_ = other.combatList_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public CrossLadderCombatListAck Clone()
	{
		return new CrossLadderCombatListAck(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as CrossLadderCombatListAck);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(CrossLadderCombatListAck other)
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
		if (!combatList_.Equals(other.combatList_))
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
		num ^= ((object)combatList_).GetHashCode();
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
		combatList_.WriteTo(ref output, _repeated_combatList_codec);
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
		num += combatList_.CalculateSize(_repeated_combatList_codec);
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(CrossLadderCombatListAck other)
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
		combatList_.Add((IEnumerable<CrossLadderPlayerInfo>)other.combatList_);
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
				combatList_.AddEntriesFrom(ref input, _repeated_combatList_codec);
				break;
			}
		}
	}
}
