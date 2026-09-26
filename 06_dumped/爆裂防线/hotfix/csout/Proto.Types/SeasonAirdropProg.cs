using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class SeasonAirdropProg : IMessage<SeasonAirdropProg>, IMessage, IEquatable<SeasonAirdropProg>, IDeepCloneable<SeasonAirdropProg>, IBufferMessage
{
	private static readonly MessageParser<SeasonAirdropProg> _parser = new MessageParser<SeasonAirdropProg>((Func<SeasonAirdropProg>)(() => new SeasonAirdropProg()));

	private UnknownFieldSet _unknownFields;

	public const int PersonalProgFieldNumber = 1;

	private int personalProg_;

	public const int TotalProgFieldNumber = 2;

	private int totalProg_;

	public const int CurProgFieldNumber = 3;

	private int curProg_;

	public const int SpeedFieldNumber = 4;

	private int speed_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<SeasonAirdropProg> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeSeasonAirdropProgReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int PersonalProg
	{
		get
		{
			return personalProg_;
		}
		set
		{
			personalProg_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int TotalProg
	{
		get
		{
			return totalProg_;
		}
		set
		{
			totalProg_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int CurProg
	{
		get
		{
			return curProg_;
		}
		set
		{
			curProg_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int Speed
	{
		get
		{
			return speed_;
		}
		set
		{
			speed_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SeasonAirdropProg()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SeasonAirdropProg(SeasonAirdropProg other)
		: this()
	{
		personalProg_ = other.personalProg_;
		totalProg_ = other.totalProg_;
		curProg_ = other.curProg_;
		speed_ = other.speed_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SeasonAirdropProg Clone()
	{
		return new SeasonAirdropProg(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as SeasonAirdropProg);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(SeasonAirdropProg other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (PersonalProg != other.PersonalProg)
		{
			return false;
		}
		if (TotalProg != other.TotalProg)
		{
			return false;
		}
		if (CurProg != other.CurProg)
		{
			return false;
		}
		if (Speed != other.Speed)
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
		if (PersonalProg != 0)
		{
			num ^= PersonalProg.GetHashCode();
		}
		if (TotalProg != 0)
		{
			num ^= TotalProg.GetHashCode();
		}
		if (CurProg != 0)
		{
			num ^= CurProg.GetHashCode();
		}
		if (Speed != 0)
		{
			num ^= Speed.GetHashCode();
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
		if (PersonalProg != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteInt32(PersonalProg);
		}
		if (TotalProg != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteInt32(TotalProg);
		}
		if (CurProg != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)24);
			((WriteContext)(ref output)).WriteInt32(CurProg);
		}
		if (Speed != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)32);
			((WriteContext)(ref output)).WriteInt32(Speed);
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
		if (PersonalProg != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(PersonalProg);
		}
		if (TotalProg != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(TotalProg);
		}
		if (CurProg != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(CurProg);
		}
		if (Speed != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(Speed);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(SeasonAirdropProg other)
	{
		if (other != null)
		{
			if (other.PersonalProg != 0)
			{
				PersonalProg = other.PersonalProg;
			}
			if (other.TotalProg != 0)
			{
				TotalProg = other.TotalProg;
			}
			if (other.CurProg != 0)
			{
				CurProg = other.CurProg;
			}
			if (other.Speed != 0)
			{
				Speed = other.Speed;
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
				PersonalProg = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 16u:
				TotalProg = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 24u:
				CurProg = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 32u:
				Speed = ((ParseContext)(ref input)).ReadInt32();
				break;
			}
		}
	}
}
