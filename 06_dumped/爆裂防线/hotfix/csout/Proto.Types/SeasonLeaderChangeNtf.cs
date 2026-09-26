using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class SeasonLeaderChangeNtf : IMessage<SeasonLeaderChangeNtf>, IMessage, IEquatable<SeasonLeaderChangeNtf>, IDeepCloneable<SeasonLeaderChangeNtf>, IBufferMessage
{
	private static readonly MessageParser<SeasonLeaderChangeNtf> _parser = new MessageParser<SeasonLeaderChangeNtf>((Func<SeasonLeaderChangeNtf>)(() => new SeasonLeaderChangeNtf()));

	private UnknownFieldSet _unknownFields;

	public const int ForceIdFieldNumber = 1;

	private ESeasonForce forceId_;

	public const int OldLeaderFieldNumber = 2;

	private long oldLeader_;

	public const int NewLeaderFieldNumber = 3;

	private long newLeader_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<SeasonLeaderChangeNtf> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeSeasonLeaderChangeNtfReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ESeasonForce ForceId
	{
		get
		{
			return forceId_;
		}
		set
		{
			forceId_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public long OldLeader
	{
		get
		{
			return oldLeader_;
		}
		set
		{
			oldLeader_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public long NewLeader
	{
		get
		{
			return newLeader_;
		}
		set
		{
			newLeader_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SeasonLeaderChangeNtf()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SeasonLeaderChangeNtf(SeasonLeaderChangeNtf other)
		: this()
	{
		forceId_ = other.forceId_;
		oldLeader_ = other.oldLeader_;
		newLeader_ = other.newLeader_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SeasonLeaderChangeNtf Clone()
	{
		return new SeasonLeaderChangeNtf(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as SeasonLeaderChangeNtf);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(SeasonLeaderChangeNtf other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (ForceId != other.ForceId)
		{
			return false;
		}
		if (OldLeader != other.OldLeader)
		{
			return false;
		}
		if (NewLeader != other.NewLeader)
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
		if (ForceId != ESeasonForce.ForceNeutral)
		{
			num ^= ForceId.GetHashCode();
		}
		if (OldLeader != 0L)
		{
			num ^= OldLeader.GetHashCode();
		}
		if (NewLeader != 0L)
		{
			num ^= NewLeader.GetHashCode();
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
		if (ForceId != ESeasonForce.ForceNeutral)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteEnum((int)ForceId);
		}
		if (OldLeader != 0L)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteInt64(OldLeader);
		}
		if (NewLeader != 0L)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)24);
			((WriteContext)(ref output)).WriteInt64(NewLeader);
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
		if (ForceId != ESeasonForce.ForceNeutral)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)ForceId);
		}
		if (OldLeader != 0L)
		{
			num += 1 + CodedOutputStream.ComputeInt64Size(OldLeader);
		}
		if (NewLeader != 0L)
		{
			num += 1 + CodedOutputStream.ComputeInt64Size(NewLeader);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(SeasonLeaderChangeNtf other)
	{
		if (other != null)
		{
			if (other.ForceId != ESeasonForce.ForceNeutral)
			{
				ForceId = other.ForceId;
			}
			if (other.OldLeader != 0L)
			{
				OldLeader = other.OldLeader;
			}
			if (other.NewLeader != 0L)
			{
				NewLeader = other.NewLeader;
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
				ForceId = (ESeasonForce)((ParseContext)(ref input)).ReadEnum();
				break;
			case 16u:
				OldLeader = ((ParseContext)(ref input)).ReadInt64();
				break;
			case 24u:
				NewLeader = ((ParseContext)(ref input)).ReadInt64();
				break;
			}
		}
	}
}
