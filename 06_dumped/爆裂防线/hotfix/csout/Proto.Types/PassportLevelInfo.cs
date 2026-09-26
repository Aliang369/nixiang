using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class PassportLevelInfo : IMessage<PassportLevelInfo>, IMessage, IEquatable<PassportLevelInfo>, IDeepCloneable<PassportLevelInfo>, IBufferMessage
{
	private static readonly MessageParser<PassportLevelInfo> _parser = new MessageParser<PassportLevelInfo>((Func<PassportLevelInfo>)(() => new PassportLevelInfo()));

	private UnknownFieldSet _unknownFields;

	public const int LevelFieldNumber = 1;

	private int level_;

	public const int ExpFieldNumber = 2;

	private int exp_;

	public const int LowAwardFieldNumber = 3;

	private int lowAward_;

	public const int HighAwardFieldNumber = 4;

	private int highAward_;

	public const int BuyLevelListFieldNumber = 5;

	private static readonly FieldCodec<PassportBuyLevelDetail> _repeated_buyLevelList_codec = FieldCodec.ForMessage<PassportBuyLevelDetail>(42u, PassportBuyLevelDetail.Parser);

	private readonly RepeatedField<PassportBuyLevelDetail> buyLevelList_ = new RepeatedField<PassportBuyLevelDetail>();

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<PassportLevelInfo> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypePassportLevelInfoReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int Level
	{
		get
		{
			return level_;
		}
		set
		{
			level_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int Exp
	{
		get
		{
			return exp_;
		}
		set
		{
			exp_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int LowAward
	{
		get
		{
			return lowAward_;
		}
		set
		{
			lowAward_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int HighAward
	{
		get
		{
			return highAward_;
		}
		set
		{
			highAward_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<PassportBuyLevelDetail> BuyLevelList => buyLevelList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public PassportLevelInfo()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public PassportLevelInfo(PassportLevelInfo other)
		: this()
	{
		level_ = other.level_;
		exp_ = other.exp_;
		lowAward_ = other.lowAward_;
		highAward_ = other.highAward_;
		buyLevelList_ = other.buyLevelList_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public PassportLevelInfo Clone()
	{
		return new PassportLevelInfo(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as PassportLevelInfo);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(PassportLevelInfo other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (Level != other.Level)
		{
			return false;
		}
		if (Exp != other.Exp)
		{
			return false;
		}
		if (LowAward != other.LowAward)
		{
			return false;
		}
		if (HighAward != other.HighAward)
		{
			return false;
		}
		if (!buyLevelList_.Equals(other.buyLevelList_))
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
		if (Level != 0)
		{
			num ^= Level.GetHashCode();
		}
		if (Exp != 0)
		{
			num ^= Exp.GetHashCode();
		}
		if (LowAward != 0)
		{
			num ^= LowAward.GetHashCode();
		}
		if (HighAward != 0)
		{
			num ^= HighAward.GetHashCode();
		}
		num ^= ((object)buyLevelList_).GetHashCode();
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
		if (Level != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteInt32(Level);
		}
		if (Exp != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteInt32(Exp);
		}
		if (LowAward != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)24);
			((WriteContext)(ref output)).WriteInt32(LowAward);
		}
		if (HighAward != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)32);
			((WriteContext)(ref output)).WriteInt32(HighAward);
		}
		buyLevelList_.WriteTo(ref output, _repeated_buyLevelList_codec);
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
		if (Level != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(Level);
		}
		if (Exp != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(Exp);
		}
		if (LowAward != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(LowAward);
		}
		if (HighAward != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(HighAward);
		}
		num += buyLevelList_.CalculateSize(_repeated_buyLevelList_codec);
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(PassportLevelInfo other)
	{
		if (other != null)
		{
			if (other.Level != 0)
			{
				Level = other.Level;
			}
			if (other.Exp != 0)
			{
				Exp = other.Exp;
			}
			if (other.LowAward != 0)
			{
				LowAward = other.LowAward;
			}
			if (other.HighAward != 0)
			{
				HighAward = other.HighAward;
			}
			buyLevelList_.Add((IEnumerable<PassportBuyLevelDetail>)other.buyLevelList_);
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
				Level = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 16u:
				Exp = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 24u:
				LowAward = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 32u:
				HighAward = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 42u:
				buyLevelList_.AddEntriesFrom(ref input, _repeated_buyLevelList_codec);
				break;
			}
		}
	}
}
