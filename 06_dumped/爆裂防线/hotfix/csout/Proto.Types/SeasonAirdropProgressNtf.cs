using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class SeasonAirdropProgressNtf : IMessage<SeasonAirdropProgressNtf>, IMessage, IEquatable<SeasonAirdropProgressNtf>, IDeepCloneable<SeasonAirdropProgressNtf>, IBufferMessage
{
	private static readonly MessageParser<SeasonAirdropProgressNtf> _parser = new MessageParser<SeasonAirdropProgressNtf>((Func<SeasonAirdropProgressNtf>)(() => new SeasonAirdropProgressNtf()));

	private UnknownFieldSet _unknownFields;

	public const int DropIdFieldNumber = 1;

	private int dropId_;

	public const int CurProgFieldNumber = 2;

	private int curProg_;

	public const int PersonalProgFieldNumber = 3;

	private int personalProg_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<SeasonAirdropProgressNtf> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeSeasonAirdropProgressNtfReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int DropId
	{
		get
		{
			return dropId_;
		}
		set
		{
			dropId_ = value;
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
	public SeasonAirdropProgressNtf()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SeasonAirdropProgressNtf(SeasonAirdropProgressNtf other)
		: this()
	{
		dropId_ = other.dropId_;
		curProg_ = other.curProg_;
		personalProg_ = other.personalProg_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SeasonAirdropProgressNtf Clone()
	{
		return new SeasonAirdropProgressNtf(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as SeasonAirdropProgressNtf);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(SeasonAirdropProgressNtf other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (DropId != other.DropId)
		{
			return false;
		}
		if (CurProg != other.CurProg)
		{
			return false;
		}
		if (PersonalProg != other.PersonalProg)
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
		if (DropId != 0)
		{
			num ^= DropId.GetHashCode();
		}
		if (CurProg != 0)
		{
			num ^= CurProg.GetHashCode();
		}
		if (PersonalProg != 0)
		{
			num ^= PersonalProg.GetHashCode();
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
		if (DropId != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteInt32(DropId);
		}
		if (CurProg != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteInt32(CurProg);
		}
		if (PersonalProg != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)24);
			((WriteContext)(ref output)).WriteInt32(PersonalProg);
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
		if (DropId != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(DropId);
		}
		if (CurProg != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(CurProg);
		}
		if (PersonalProg != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(PersonalProg);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(SeasonAirdropProgressNtf other)
	{
		if (other != null)
		{
			if (other.DropId != 0)
			{
				DropId = other.DropId;
			}
			if (other.CurProg != 0)
			{
				CurProg = other.CurProg;
			}
			if (other.PersonalProg != 0)
			{
				PersonalProg = other.PersonalProg;
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
				DropId = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 16u:
				CurProg = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 24u:
				PersonalProg = ((ParseContext)(ref input)).ReadInt32();
				break;
			}
		}
	}
}
