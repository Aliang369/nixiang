using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class MineWarSelfInfo : IMessage<MineWarSelfInfo>, IMessage, IEquatable<MineWarSelfInfo>, IDeepCloneable<MineWarSelfInfo>, IBufferMessage
{
	private static readonly MessageParser<MineWarSelfInfo> _parser = new MessageParser<MineWarSelfInfo>((Func<MineWarSelfInfo>)(() => new MineWarSelfInfo()));

	private UnknownFieldSet _unknownFields;

	public const int CombatNumFieldNumber = 1;

	private int combatNum_;

	public const int BuyCombatFieldNumber = 2;

	private int buyCombat_;

	public const int RecoverTmFieldNumber = 3;

	private long recoverTm_;

	public const int NextCombatTmFieldNumber = 4;

	private long nextCombatTm_;

	public const int CollectTmFieldNumber = 5;

	private long collectTm_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<MineWarSelfInfo> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeMineWarSelfInfoReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int CombatNum
	{
		get
		{
			return combatNum_;
		}
		set
		{
			combatNum_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int BuyCombat
	{
		get
		{
			return buyCombat_;
		}
		set
		{
			buyCombat_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public long RecoverTm
	{
		get
		{
			return recoverTm_;
		}
		set
		{
			recoverTm_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public long NextCombatTm
	{
		get
		{
			return nextCombatTm_;
		}
		set
		{
			nextCombatTm_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public long CollectTm
	{
		get
		{
			return collectTm_;
		}
		set
		{
			collectTm_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public MineWarSelfInfo()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public MineWarSelfInfo(MineWarSelfInfo other)
		: this()
	{
		combatNum_ = other.combatNum_;
		buyCombat_ = other.buyCombat_;
		recoverTm_ = other.recoverTm_;
		nextCombatTm_ = other.nextCombatTm_;
		collectTm_ = other.collectTm_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public MineWarSelfInfo Clone()
	{
		return new MineWarSelfInfo(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as MineWarSelfInfo);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(MineWarSelfInfo other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (CombatNum != other.CombatNum)
		{
			return false;
		}
		if (BuyCombat != other.BuyCombat)
		{
			return false;
		}
		if (RecoverTm != other.RecoverTm)
		{
			return false;
		}
		if (NextCombatTm != other.NextCombatTm)
		{
			return false;
		}
		if (CollectTm != other.CollectTm)
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
		if (CombatNum != 0)
		{
			num ^= CombatNum.GetHashCode();
		}
		if (BuyCombat != 0)
		{
			num ^= BuyCombat.GetHashCode();
		}
		if (RecoverTm != 0L)
		{
			num ^= RecoverTm.GetHashCode();
		}
		if (NextCombatTm != 0L)
		{
			num ^= NextCombatTm.GetHashCode();
		}
		if (CollectTm != 0L)
		{
			num ^= CollectTm.GetHashCode();
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
		if (CombatNum != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteInt32(CombatNum);
		}
		if (BuyCombat != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteInt32(BuyCombat);
		}
		if (RecoverTm != 0L)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)24);
			((WriteContext)(ref output)).WriteInt64(RecoverTm);
		}
		if (NextCombatTm != 0L)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)32);
			((WriteContext)(ref output)).WriteInt64(NextCombatTm);
		}
		if (CollectTm != 0L)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)40);
			((WriteContext)(ref output)).WriteInt64(CollectTm);
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
		if (CombatNum != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(CombatNum);
		}
		if (BuyCombat != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(BuyCombat);
		}
		if (RecoverTm != 0L)
		{
			num += 1 + CodedOutputStream.ComputeInt64Size(RecoverTm);
		}
		if (NextCombatTm != 0L)
		{
			num += 1 + CodedOutputStream.ComputeInt64Size(NextCombatTm);
		}
		if (CollectTm != 0L)
		{
			num += 1 + CodedOutputStream.ComputeInt64Size(CollectTm);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(MineWarSelfInfo other)
	{
		if (other != null)
		{
			if (other.CombatNum != 0)
			{
				CombatNum = other.CombatNum;
			}
			if (other.BuyCombat != 0)
			{
				BuyCombat = other.BuyCombat;
			}
			if (other.RecoverTm != 0L)
			{
				RecoverTm = other.RecoverTm;
			}
			if (other.NextCombatTm != 0L)
			{
				NextCombatTm = other.NextCombatTm;
			}
			if (other.CollectTm != 0L)
			{
				CollectTm = other.CollectTm;
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
				CombatNum = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 16u:
				BuyCombat = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 24u:
				RecoverTm = ((ParseContext)(ref input)).ReadInt64();
				break;
			case 32u:
				NextCombatTm = ((ParseContext)(ref input)).ReadInt64();
				break;
			case 40u:
				CollectTm = ((ParseContext)(ref input)).ReadInt64();
				break;
			}
		}
	}
}
