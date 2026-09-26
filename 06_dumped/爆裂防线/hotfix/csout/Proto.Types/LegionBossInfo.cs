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
public sealed class LegionBossInfo : IMessage<LegionBossInfo>, IMessage, IEquatable<LegionBossInfo>, IDeepCloneable<LegionBossInfo>, IBufferMessage
{
	private static readonly MessageParser<LegionBossInfo> _parser = new MessageParser<LegionBossInfo>((Func<LegionBossInfo>)(() => new LegionBossInfo()));

	private UnknownFieldSet _unknownFields;

	public const int BossIdFieldNumber = 1;

	private int bossId_;

	public const int StartTmFieldNumber = 2;

	private long startTm_;

	public const int EndTmFieldNumber = 3;

	private long endTm_;

	public const int KillTotalFieldNumber = 4;

	private int killTotal_;

	public const int TargetListFieldNumber = 5;

	private static readonly FieldCodec<int> _repeated_targetList_codec = FieldCodec.ForInt32(42u);

	private readonly RepeatedField<int> targetList_ = new RepeatedField<int>();

	public const int PassStateFieldNumber = 6;

	private EBoolState passState_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<LegionBossInfo> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeLegionBossInfoReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int BossId
	{
		get
		{
			return bossId_;
		}
		set
		{
			bossId_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public long StartTm
	{
		get
		{
			return startTm_;
		}
		set
		{
			startTm_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public long EndTm
	{
		get
		{
			return endTm_;
		}
		set
		{
			endTm_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int KillTotal
	{
		get
		{
			return killTotal_;
		}
		set
		{
			killTotal_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<int> TargetList => targetList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public EBoolState PassState
	{
		get
		{
			return passState_;
		}
		set
		{
			passState_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public LegionBossInfo()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public LegionBossInfo(LegionBossInfo other)
		: this()
	{
		bossId_ = other.bossId_;
		startTm_ = other.startTm_;
		endTm_ = other.endTm_;
		killTotal_ = other.killTotal_;
		targetList_ = other.targetList_.Clone();
		passState_ = other.passState_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public LegionBossInfo Clone()
	{
		return new LegionBossInfo(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as LegionBossInfo);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(LegionBossInfo other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (BossId != other.BossId)
		{
			return false;
		}
		if (StartTm != other.StartTm)
		{
			return false;
		}
		if (EndTm != other.EndTm)
		{
			return false;
		}
		if (KillTotal != other.KillTotal)
		{
			return false;
		}
		if (!targetList_.Equals(other.targetList_))
		{
			return false;
		}
		if (PassState != other.PassState)
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
		if (BossId != 0)
		{
			num ^= BossId.GetHashCode();
		}
		if (StartTm != 0L)
		{
			num ^= StartTm.GetHashCode();
		}
		if (EndTm != 0L)
		{
			num ^= EndTm.GetHashCode();
		}
		if (KillTotal != 0)
		{
			num ^= KillTotal.GetHashCode();
		}
		num ^= ((object)targetList_).GetHashCode();
		if (PassState != EBoolState.False)
		{
			num ^= PassState.GetHashCode();
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
		if (BossId != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteInt32(BossId);
		}
		if (StartTm != 0L)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteInt64(StartTm);
		}
		if (EndTm != 0L)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)24);
			((WriteContext)(ref output)).WriteInt64(EndTm);
		}
		if (KillTotal != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)32);
			((WriteContext)(ref output)).WriteInt32(KillTotal);
		}
		targetList_.WriteTo(ref output, _repeated_targetList_codec);
		if (PassState != EBoolState.False)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)48);
			((WriteContext)(ref output)).WriteEnum((int)PassState);
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
		if (BossId != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(BossId);
		}
		if (StartTm != 0L)
		{
			num += 1 + CodedOutputStream.ComputeInt64Size(StartTm);
		}
		if (EndTm != 0L)
		{
			num += 1 + CodedOutputStream.ComputeInt64Size(EndTm);
		}
		if (KillTotal != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(KillTotal);
		}
		num += targetList_.CalculateSize(_repeated_targetList_codec);
		if (PassState != EBoolState.False)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)PassState);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(LegionBossInfo other)
	{
		if (other != null)
		{
			if (other.BossId != 0)
			{
				BossId = other.BossId;
			}
			if (other.StartTm != 0L)
			{
				StartTm = other.StartTm;
			}
			if (other.EndTm != 0L)
			{
				EndTm = other.EndTm;
			}
			if (other.KillTotal != 0)
			{
				KillTotal = other.KillTotal;
			}
			targetList_.Add((IEnumerable<int>)other.targetList_);
			if (other.PassState != EBoolState.False)
			{
				PassState = other.PassState;
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
				BossId = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 16u:
				StartTm = ((ParseContext)(ref input)).ReadInt64();
				break;
			case 24u:
				EndTm = ((ParseContext)(ref input)).ReadInt64();
				break;
			case 32u:
				KillTotal = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 40u:
			case 42u:
				targetList_.AddEntriesFrom(ref input, _repeated_targetList_codec);
				break;
			case 48u:
				PassState = (EBoolState)((ParseContext)(ref input)).ReadEnum();
				break;
			}
		}
	}
}
