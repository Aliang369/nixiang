using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class PlayerGunInfo : IMessage<PlayerGunInfo>, IMessage, IEquatable<PlayerGunInfo>, IDeepCloneable<PlayerGunInfo>, IBufferMessage
{
	private static readonly MessageParser<PlayerGunInfo> _parser = new MessageParser<PlayerGunInfo>((Func<PlayerGunInfo>)(() => new PlayerGunInfo()));

	private UnknownFieldSet _unknownFields;

	public const int GunListFieldNumber = 1;

	private static readonly FieldCodec<PlayerGunDetail> _repeated_gunList_codec = FieldCodec.ForMessage<PlayerGunDetail>(10u, PlayerGunDetail.Parser);

	private readonly RepeatedField<PlayerGunDetail> gunList_ = new RepeatedField<PlayerGunDetail>();

	public const int TotalDamagePctFieldNumber = 2;

	private float totalDamagePct_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<PlayerGunInfo> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypePlayerGunInfoReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<PlayerGunDetail> GunList => gunList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public float TotalDamagePct
	{
		get
		{
			return totalDamagePct_;
		}
		set
		{
			totalDamagePct_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public PlayerGunInfo()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public PlayerGunInfo(PlayerGunInfo other)
		: this()
	{
		gunList_ = other.gunList_.Clone();
		totalDamagePct_ = other.totalDamagePct_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public PlayerGunInfo Clone()
	{
		return new PlayerGunInfo(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as PlayerGunInfo);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(PlayerGunInfo other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!gunList_.Equals(other.gunList_))
		{
			return false;
		}
		if (!ProtobufEqualityComparers.BitwiseSingleEqualityComparer.Equals(TotalDamagePct, other.TotalDamagePct))
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
		num ^= ((object)gunList_).GetHashCode();
		if (TotalDamagePct != 0f)
		{
			num ^= ProtobufEqualityComparers.BitwiseSingleEqualityComparer.GetHashCode(TotalDamagePct);
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
		gunList_.WriteTo(ref output, _repeated_gunList_codec);
		if (TotalDamagePct != 0f)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)21);
			((WriteContext)(ref output)).WriteFloat(TotalDamagePct);
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
		num += gunList_.CalculateSize(_repeated_gunList_codec);
		if (TotalDamagePct != 0f)
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
	public void MergeFrom(PlayerGunInfo other)
	{
		if (other != null)
		{
			gunList_.Add((IEnumerable<PlayerGunDetail>)other.gunList_);
			if (other.TotalDamagePct != 0f)
			{
				TotalDamagePct = other.TotalDamagePct;
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
			case 10u:
				gunList_.AddEntriesFrom(ref input, _repeated_gunList_codec);
				break;
			case 21u:
				TotalDamagePct = ((ParseContext)(ref input)).ReadFloat();
				break;
			}
		}
	}
}
