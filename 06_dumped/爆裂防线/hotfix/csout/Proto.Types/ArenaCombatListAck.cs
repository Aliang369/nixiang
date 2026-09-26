using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class ArenaCombatListAck : IMessage<ArenaCombatListAck>, IMessage, IEquatable<ArenaCombatListAck>, IDeepCloneable<ArenaCombatListAck>, IBufferMessage
{
	private static readonly MessageParser<ArenaCombatListAck> _parser = new MessageParser<ArenaCombatListAck>((Func<ArenaCombatListAck>)(() => new ArenaCombatListAck()));

	private UnknownFieldSet _unknownFields;

	public const int ArenaInfoFieldNumber = 1;

	private ArenaInfo arenaInfo_;

	public const int CombatListFieldNumber = 2;

	private static readonly FieldCodec<ArenaCombatDetail> _repeated_combatList_codec = FieldCodec.ForMessage<ArenaCombatDetail>(18u, ArenaCombatDetail.Parser);

	private readonly RepeatedField<ArenaCombatDetail> combatList_ = new RepeatedField<ArenaCombatDetail>();

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<ArenaCombatListAck> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeArenaCombatListAckReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ArenaInfo ArenaInfo
	{
		get
		{
			return arenaInfo_;
		}
		set
		{
			arenaInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<ArenaCombatDetail> CombatList => combatList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ArenaCombatListAck()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ArenaCombatListAck(ArenaCombatListAck other)
		: this()
	{
		arenaInfo_ = ((other.arenaInfo_ != null) ? other.arenaInfo_.Clone() : null);
		combatList_ = other.combatList_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ArenaCombatListAck Clone()
	{
		return new ArenaCombatListAck(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as ArenaCombatListAck);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(ArenaCombatListAck other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!object.Equals(ArenaInfo, other.ArenaInfo))
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
		if (arenaInfo_ != null)
		{
			num ^= ArenaInfo.GetHashCode();
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
		if (arenaInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)10);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)ArenaInfo);
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
		if (arenaInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)ArenaInfo);
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
	public void MergeFrom(ArenaCombatListAck other)
	{
		if (other == null)
		{
			return;
		}
		if (other.arenaInfo_ != null)
		{
			if (arenaInfo_ == null)
			{
				ArenaInfo = new ArenaInfo();
			}
			ArenaInfo.MergeFrom(other.ArenaInfo);
		}
		combatList_.Add((IEnumerable<ArenaCombatDetail>)other.combatList_);
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
				if (arenaInfo_ == null)
				{
					ArenaInfo = new ArenaInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)ArenaInfo);
				break;
			case 18u:
				combatList_.AddEntriesFrom(ref input, _repeated_combatList_codec);
				break;
			}
		}
	}
}
