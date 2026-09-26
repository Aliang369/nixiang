using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class BossSubjectInfo : IMessage<BossSubjectInfo>, IMessage, IEquatable<BossSubjectInfo>, IDeepCloneable<BossSubjectInfo>, IBufferMessage
{
	private static readonly MessageParser<BossSubjectInfo> _parser = new MessageParser<BossSubjectInfo>((Func<BossSubjectInfo>)(() => new BossSubjectInfo()));

	private UnknownFieldSet _unknownFields;

	public const int SubjectIdFieldNumber = 1;

	private int subjectId_;

	public const int PassLevelFieldNumber = 2;

	private int passLevel_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<BossSubjectInfo> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeBossSubjectInfoReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int SubjectId
	{
		get
		{
			return subjectId_;
		}
		set
		{
			subjectId_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int PassLevel
	{
		get
		{
			return passLevel_;
		}
		set
		{
			passLevel_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public BossSubjectInfo()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public BossSubjectInfo(BossSubjectInfo other)
		: this()
	{
		subjectId_ = other.subjectId_;
		passLevel_ = other.passLevel_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public BossSubjectInfo Clone()
	{
		return new BossSubjectInfo(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as BossSubjectInfo);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(BossSubjectInfo other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (SubjectId != other.SubjectId)
		{
			return false;
		}
		if (PassLevel != other.PassLevel)
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
		if (SubjectId != 0)
		{
			num ^= SubjectId.GetHashCode();
		}
		if (PassLevel != 0)
		{
			num ^= PassLevel.GetHashCode();
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
		if (SubjectId != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteInt32(SubjectId);
		}
		if (PassLevel != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteInt32(PassLevel);
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
		if (SubjectId != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(SubjectId);
		}
		if (PassLevel != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(PassLevel);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(BossSubjectInfo other)
	{
		if (other != null)
		{
			if (other.SubjectId != 0)
			{
				SubjectId = other.SubjectId;
			}
			if (other.PassLevel != 0)
			{
				PassLevel = other.PassLevel;
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
				SubjectId = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 16u:
				PassLevel = ((ParseContext)(ref input)).ReadInt32();
				break;
			}
		}
	}
}
