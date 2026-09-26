using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class LegionWarOpenInfo : IMessage<LegionWarOpenInfo>, IMessage, IEquatable<LegionWarOpenInfo>, IDeepCloneable<LegionWarOpenInfo>, IBufferMessage
{
	private static readonly MessageParser<LegionWarOpenInfo> _parser = new MessageParser<LegionWarOpenInfo>((Func<LegionWarOpenInfo>)(() => new LegionWarOpenInfo()));

	private UnknownFieldSet _unknownFields;

	public const int PreviewStartTmFieldNumber = 1;

	private long previewStartTm_;

	public const int PreviewEndTmFieldNumber = 2;

	private long previewEndTm_;

	public const int SignUpStartTmFieldNumber = 3;

	private long signUpStartTm_;

	public const int SignUpEndTmFieldNumber = 4;

	private long signUpEndTm_;

	public const int PrepareStartTmFieldNumber = 5;

	private long prepareStartTm_;

	public const int PrepareEndTmFieldNumber = 6;

	private long prepareEndTm_;

	public const int FightingStartTmFieldNumber = 7;

	private long fightingStartTm_;

	public const int FightingEndTmFieldNumber = 8;

	private long fightingEndTm_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<LegionWarOpenInfo> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeLegionWarOpenInfoReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public long PreviewStartTm
	{
		get
		{
			return previewStartTm_;
		}
		set
		{
			previewStartTm_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public long PreviewEndTm
	{
		get
		{
			return previewEndTm_;
		}
		set
		{
			previewEndTm_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public long SignUpStartTm
	{
		get
		{
			return signUpStartTm_;
		}
		set
		{
			signUpStartTm_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public long SignUpEndTm
	{
		get
		{
			return signUpEndTm_;
		}
		set
		{
			signUpEndTm_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public long PrepareStartTm
	{
		get
		{
			return prepareStartTm_;
		}
		set
		{
			prepareStartTm_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public long PrepareEndTm
	{
		get
		{
			return prepareEndTm_;
		}
		set
		{
			prepareEndTm_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public long FightingStartTm
	{
		get
		{
			return fightingStartTm_;
		}
		set
		{
			fightingStartTm_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public long FightingEndTm
	{
		get
		{
			return fightingEndTm_;
		}
		set
		{
			fightingEndTm_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public LegionWarOpenInfo()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public LegionWarOpenInfo(LegionWarOpenInfo other)
		: this()
	{
		previewStartTm_ = other.previewStartTm_;
		previewEndTm_ = other.previewEndTm_;
		signUpStartTm_ = other.signUpStartTm_;
		signUpEndTm_ = other.signUpEndTm_;
		prepareStartTm_ = other.prepareStartTm_;
		prepareEndTm_ = other.prepareEndTm_;
		fightingStartTm_ = other.fightingStartTm_;
		fightingEndTm_ = other.fightingEndTm_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public LegionWarOpenInfo Clone()
	{
		return new LegionWarOpenInfo(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as LegionWarOpenInfo);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(LegionWarOpenInfo other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (PreviewStartTm != other.PreviewStartTm)
		{
			return false;
		}
		if (PreviewEndTm != other.PreviewEndTm)
		{
			return false;
		}
		if (SignUpStartTm != other.SignUpStartTm)
		{
			return false;
		}
		if (SignUpEndTm != other.SignUpEndTm)
		{
			return false;
		}
		if (PrepareStartTm != other.PrepareStartTm)
		{
			return false;
		}
		if (PrepareEndTm != other.PrepareEndTm)
		{
			return false;
		}
		if (FightingStartTm != other.FightingStartTm)
		{
			return false;
		}
		if (FightingEndTm != other.FightingEndTm)
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
		if (PreviewStartTm != 0L)
		{
			num ^= PreviewStartTm.GetHashCode();
		}
		if (PreviewEndTm != 0L)
		{
			num ^= PreviewEndTm.GetHashCode();
		}
		if (SignUpStartTm != 0L)
		{
			num ^= SignUpStartTm.GetHashCode();
		}
		if (SignUpEndTm != 0L)
		{
			num ^= SignUpEndTm.GetHashCode();
		}
		if (PrepareStartTm != 0L)
		{
			num ^= PrepareStartTm.GetHashCode();
		}
		if (PrepareEndTm != 0L)
		{
			num ^= PrepareEndTm.GetHashCode();
		}
		if (FightingStartTm != 0L)
		{
			num ^= FightingStartTm.GetHashCode();
		}
		if (FightingEndTm != 0L)
		{
			num ^= FightingEndTm.GetHashCode();
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
		if (PreviewStartTm != 0L)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteInt64(PreviewStartTm);
		}
		if (PreviewEndTm != 0L)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteInt64(PreviewEndTm);
		}
		if (SignUpStartTm != 0L)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)24);
			((WriteContext)(ref output)).WriteInt64(SignUpStartTm);
		}
		if (SignUpEndTm != 0L)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)32);
			((WriteContext)(ref output)).WriteInt64(SignUpEndTm);
		}
		if (PrepareStartTm != 0L)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)40);
			((WriteContext)(ref output)).WriteInt64(PrepareStartTm);
		}
		if (PrepareEndTm != 0L)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)48);
			((WriteContext)(ref output)).WriteInt64(PrepareEndTm);
		}
		if (FightingStartTm != 0L)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)56);
			((WriteContext)(ref output)).WriteInt64(FightingStartTm);
		}
		if (FightingEndTm != 0L)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)64);
			((WriteContext)(ref output)).WriteInt64(FightingEndTm);
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
		if (PreviewStartTm != 0L)
		{
			num += 1 + CodedOutputStream.ComputeInt64Size(PreviewStartTm);
		}
		if (PreviewEndTm != 0L)
		{
			num += 1 + CodedOutputStream.ComputeInt64Size(PreviewEndTm);
		}
		if (SignUpStartTm != 0L)
		{
			num += 1 + CodedOutputStream.ComputeInt64Size(SignUpStartTm);
		}
		if (SignUpEndTm != 0L)
		{
			num += 1 + CodedOutputStream.ComputeInt64Size(SignUpEndTm);
		}
		if (PrepareStartTm != 0L)
		{
			num += 1 + CodedOutputStream.ComputeInt64Size(PrepareStartTm);
		}
		if (PrepareEndTm != 0L)
		{
			num += 1 + CodedOutputStream.ComputeInt64Size(PrepareEndTm);
		}
		if (FightingStartTm != 0L)
		{
			num += 1 + CodedOutputStream.ComputeInt64Size(FightingStartTm);
		}
		if (FightingEndTm != 0L)
		{
			num += 1 + CodedOutputStream.ComputeInt64Size(FightingEndTm);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(LegionWarOpenInfo other)
	{
		if (other != null)
		{
			if (other.PreviewStartTm != 0L)
			{
				PreviewStartTm = other.PreviewStartTm;
			}
			if (other.PreviewEndTm != 0L)
			{
				PreviewEndTm = other.PreviewEndTm;
			}
			if (other.SignUpStartTm != 0L)
			{
				SignUpStartTm = other.SignUpStartTm;
			}
			if (other.SignUpEndTm != 0L)
			{
				SignUpEndTm = other.SignUpEndTm;
			}
			if (other.PrepareStartTm != 0L)
			{
				PrepareStartTm = other.PrepareStartTm;
			}
			if (other.PrepareEndTm != 0L)
			{
				PrepareEndTm = other.PrepareEndTm;
			}
			if (other.FightingStartTm != 0L)
			{
				FightingStartTm = other.FightingStartTm;
			}
			if (other.FightingEndTm != 0L)
			{
				FightingEndTm = other.FightingEndTm;
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
				PreviewStartTm = ((ParseContext)(ref input)).ReadInt64();
				break;
			case 16u:
				PreviewEndTm = ((ParseContext)(ref input)).ReadInt64();
				break;
			case 24u:
				SignUpStartTm = ((ParseContext)(ref input)).ReadInt64();
				break;
			case 32u:
				SignUpEndTm = ((ParseContext)(ref input)).ReadInt64();
				break;
			case 40u:
				PrepareStartTm = ((ParseContext)(ref input)).ReadInt64();
				break;
			case 48u:
				PrepareEndTm = ((ParseContext)(ref input)).ReadInt64();
				break;
			case 56u:
				FightingStartTm = ((ParseContext)(ref input)).ReadInt64();
				break;
			case 64u:
				FightingEndTm = ((ParseContext)(ref input)).ReadInt64();
				break;
			}
		}
	}
}
