using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class BattleTacticDetail : IMessage<BattleTacticDetail>, IMessage, IEquatable<BattleTacticDetail>, IDeepCloneable<BattleTacticDetail>, IBufferMessage
{
	private static readonly MessageParser<BattleTacticDetail> _parser = new MessageParser<BattleTacticDetail>((Func<BattleTacticDetail>)(() => new BattleTacticDetail()));

	private UnknownFieldSet _unknownFields;

	public const int TacticTidFieldNumber = 1;

	private int tacticTid_;

	public const int TacticModIdFieldNumber = 2;

	private int tacticModId_;

	public const int ModIdFieldNumber = 3;

	private int modId_;

	public const int PctFieldNumber = 4;

	private float pct_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<BattleTacticDetail> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeBattleTacticDetailReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int TacticTid
	{
		get
		{
			return tacticTid_;
		}
		set
		{
			tacticTid_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int TacticModId
	{
		get
		{
			return tacticModId_;
		}
		set
		{
			tacticModId_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int ModId
	{
		get
		{
			return modId_;
		}
		set
		{
			modId_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public float Pct
	{
		get
		{
			return pct_;
		}
		set
		{
			pct_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public BattleTacticDetail()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public BattleTacticDetail(BattleTacticDetail other)
		: this()
	{
		tacticTid_ = other.tacticTid_;
		tacticModId_ = other.tacticModId_;
		modId_ = other.modId_;
		pct_ = other.pct_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public BattleTacticDetail Clone()
	{
		return new BattleTacticDetail(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as BattleTacticDetail);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(BattleTacticDetail other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (TacticTid != other.TacticTid)
		{
			return false;
		}
		if (TacticModId != other.TacticModId)
		{
			return false;
		}
		if (ModId != other.ModId)
		{
			return false;
		}
		if (!ProtobufEqualityComparers.BitwiseSingleEqualityComparer.Equals(Pct, other.Pct))
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
		if (TacticTid != 0)
		{
			num ^= TacticTid.GetHashCode();
		}
		if (TacticModId != 0)
		{
			num ^= TacticModId.GetHashCode();
		}
		if (ModId != 0)
		{
			num ^= ModId.GetHashCode();
		}
		if (Pct != 0f)
		{
			num ^= ProtobufEqualityComparers.BitwiseSingleEqualityComparer.GetHashCode(Pct);
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
		if (TacticTid != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteInt32(TacticTid);
		}
		if (TacticModId != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteInt32(TacticModId);
		}
		if (ModId != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)24);
			((WriteContext)(ref output)).WriteInt32(ModId);
		}
		if (Pct != 0f)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)37);
			((WriteContext)(ref output)).WriteFloat(Pct);
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
		if (TacticTid != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(TacticTid);
		}
		if (TacticModId != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(TacticModId);
		}
		if (ModId != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(ModId);
		}
		if (Pct != 0f)
		{
			num += 5;
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(BattleTacticDetail other)
	{
		if (other != null)
		{
			if (other.TacticTid != 0)
			{
				TacticTid = other.TacticTid;
			}
			if (other.TacticModId != 0)
			{
				TacticModId = other.TacticModId;
			}
			if (other.ModId != 0)
			{
				ModId = other.ModId;
			}
			if (other.Pct != 0f)
			{
				Pct = other.Pct;
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
				TacticTid = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 16u:
				TacticModId = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 24u:
				ModId = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 37u:
				Pct = ((ParseContext)(ref input)).ReadFloat();
				break;
			}
		}
	}
}
