using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class LegionWarLegionInfo : IMessage<LegionWarLegionInfo>, IMessage, IEquatable<LegionWarLegionInfo>, IDeepCloneable<LegionWarLegionInfo>, IBufferMessage
{
	private static readonly MessageParser<LegionWarLegionInfo> _parser = new MessageParser<LegionWarLegionInfo>((Func<LegionWarLegionInfo>)(() => new LegionWarLegionInfo()));

	private UnknownFieldSet _unknownFields;

	public const int LegionZoneFieldNumber = 1;

	private int legionZone_;

	public const int LegionForceIdFieldNumber = 2;

	private ELegionWarForce legionForceId_;

	public const int MarkPointIdFieldNumber = 3;

	private int markPointId_;

	public const int ScoreCountFieldNumber = 4;

	private int scoreCount_;

	public const int SignupFieldNumber = 5;

	private EBoolState signup_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<LegionWarLegionInfo> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeLegionWarLegionInfoReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int LegionZone
	{
		get
		{
			return legionZone_;
		}
		set
		{
			legionZone_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ELegionWarForce LegionForceId
	{
		get
		{
			return legionForceId_;
		}
		set
		{
			legionForceId_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int MarkPointId
	{
		get
		{
			return markPointId_;
		}
		set
		{
			markPointId_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int ScoreCount
	{
		get
		{
			return scoreCount_;
		}
		set
		{
			scoreCount_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public EBoolState Signup
	{
		get
		{
			return signup_;
		}
		set
		{
			signup_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public LegionWarLegionInfo()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public LegionWarLegionInfo(LegionWarLegionInfo other)
		: this()
	{
		legionZone_ = other.legionZone_;
		legionForceId_ = other.legionForceId_;
		markPointId_ = other.markPointId_;
		scoreCount_ = other.scoreCount_;
		signup_ = other.signup_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public LegionWarLegionInfo Clone()
	{
		return new LegionWarLegionInfo(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as LegionWarLegionInfo);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(LegionWarLegionInfo other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (LegionZone != other.LegionZone)
		{
			return false;
		}
		if (LegionForceId != other.LegionForceId)
		{
			return false;
		}
		if (MarkPointId != other.MarkPointId)
		{
			return false;
		}
		if (ScoreCount != other.ScoreCount)
		{
			return false;
		}
		if (Signup != other.Signup)
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
		if (LegionZone != 0)
		{
			num ^= LegionZone.GetHashCode();
		}
		if (LegionForceId != ELegionWarForce.EmptyValue)
		{
			num ^= LegionForceId.GetHashCode();
		}
		if (MarkPointId != 0)
		{
			num ^= MarkPointId.GetHashCode();
		}
		if (ScoreCount != 0)
		{
			num ^= ScoreCount.GetHashCode();
		}
		if (Signup != EBoolState.False)
		{
			num ^= Signup.GetHashCode();
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
		if (LegionZone != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteInt32(LegionZone);
		}
		if (LegionForceId != ELegionWarForce.EmptyValue)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteEnum((int)LegionForceId);
		}
		if (MarkPointId != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)24);
			((WriteContext)(ref output)).WriteInt32(MarkPointId);
		}
		if (ScoreCount != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)32);
			((WriteContext)(ref output)).WriteInt32(ScoreCount);
		}
		if (Signup != EBoolState.False)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)40);
			((WriteContext)(ref output)).WriteEnum((int)Signup);
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
		if (LegionZone != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(LegionZone);
		}
		if (LegionForceId != ELegionWarForce.EmptyValue)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)LegionForceId);
		}
		if (MarkPointId != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(MarkPointId);
		}
		if (ScoreCount != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(ScoreCount);
		}
		if (Signup != EBoolState.False)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)Signup);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(LegionWarLegionInfo other)
	{
		if (other != null)
		{
			if (other.LegionZone != 0)
			{
				LegionZone = other.LegionZone;
			}
			if (other.LegionForceId != ELegionWarForce.EmptyValue)
			{
				LegionForceId = other.LegionForceId;
			}
			if (other.MarkPointId != 0)
			{
				MarkPointId = other.MarkPointId;
			}
			if (other.ScoreCount != 0)
			{
				ScoreCount = other.ScoreCount;
			}
			if (other.Signup != EBoolState.False)
			{
				Signup = other.Signup;
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
				LegionZone = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 16u:
				LegionForceId = (ELegionWarForce)((ParseContext)(ref input)).ReadEnum();
				break;
			case 24u:
				MarkPointId = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 32u:
				ScoreCount = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 40u:
				Signup = (EBoolState)((ParseContext)(ref input)).ReadEnum();
				break;
			}
		}
	}
}
