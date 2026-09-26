using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class PlayerSuitInfo : IMessage<PlayerSuitInfo>, IMessage, IEquatable<PlayerSuitInfo>, IDeepCloneable<PlayerSuitInfo>, IBufferMessage
{
	private static readonly MessageParser<PlayerSuitInfo> _parser = new MessageParser<PlayerSuitInfo>((Func<PlayerSuitInfo>)(() => new PlayerSuitInfo()));

	private UnknownFieldSet _unknownFields;

	public const int SuitListFieldNumber = 1;

	private static readonly FieldCodec<PlayerSuitDetail> _repeated_suitList_codec = FieldCodec.ForMessage<PlayerSuitDetail>(10u, PlayerSuitDetail.Parser);

	private readonly RepeatedField<PlayerSuitDetail> suitList_ = new RepeatedField<PlayerSuitDetail>();

	public const int TotalAttackFieldNumber = 2;

	private int totalAttack_;

	public const int UseListFieldNumber = 3;

	private static readonly FieldCodec<int> _repeated_useList_codec = FieldCodec.ForInt32(26u);

	private readonly RepeatedField<int> useList_ = new RepeatedField<int>();

	public const int SuperUseListFieldNumber = 4;

	private static readonly FieldCodec<int> _repeated_superUseList_codec = FieldCodec.ForInt32(34u);

	private readonly RepeatedField<int> superUseList_ = new RepeatedField<int>();

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<PlayerSuitInfo> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypePlayerSuitInfoReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<PlayerSuitDetail> SuitList => suitList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int TotalAttack
	{
		get
		{
			return totalAttack_;
		}
		set
		{
			totalAttack_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<int> UseList => useList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<int> SuperUseList => superUseList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public PlayerSuitInfo()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public PlayerSuitInfo(PlayerSuitInfo other)
		: this()
	{
		suitList_ = other.suitList_.Clone();
		totalAttack_ = other.totalAttack_;
		useList_ = other.useList_.Clone();
		superUseList_ = other.superUseList_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public PlayerSuitInfo Clone()
	{
		return new PlayerSuitInfo(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as PlayerSuitInfo);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(PlayerSuitInfo other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!suitList_.Equals(other.suitList_))
		{
			return false;
		}
		if (TotalAttack != other.TotalAttack)
		{
			return false;
		}
		if (!useList_.Equals(other.useList_))
		{
			return false;
		}
		if (!superUseList_.Equals(other.superUseList_))
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
		num ^= ((object)suitList_).GetHashCode();
		if (TotalAttack != 0)
		{
			num ^= TotalAttack.GetHashCode();
		}
		num ^= ((object)useList_).GetHashCode();
		num ^= ((object)superUseList_).GetHashCode();
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
		suitList_.WriteTo(ref output, _repeated_suitList_codec);
		if (TotalAttack != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteInt32(TotalAttack);
		}
		useList_.WriteTo(ref output, _repeated_useList_codec);
		superUseList_.WriteTo(ref output, _repeated_superUseList_codec);
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
		num += suitList_.CalculateSize(_repeated_suitList_codec);
		if (TotalAttack != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(TotalAttack);
		}
		num += useList_.CalculateSize(_repeated_useList_codec);
		num += superUseList_.CalculateSize(_repeated_superUseList_codec);
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(PlayerSuitInfo other)
	{
		if (other != null)
		{
			suitList_.Add((IEnumerable<PlayerSuitDetail>)other.suitList_);
			if (other.TotalAttack != 0)
			{
				TotalAttack = other.TotalAttack;
			}
			useList_.Add((IEnumerable<int>)other.useList_);
			superUseList_.Add((IEnumerable<int>)other.superUseList_);
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
			case 10u:
				suitList_.AddEntriesFrom(ref input, _repeated_suitList_codec);
				break;
			case 16u:
				TotalAttack = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 24u:
			case 26u:
				useList_.AddEntriesFrom(ref input, _repeated_useList_codec);
				break;
			case 32u:
			case 34u:
				superUseList_.AddEntriesFrom(ref input, _repeated_superUseList_codec);
				break;
			}
		}
	}
}
