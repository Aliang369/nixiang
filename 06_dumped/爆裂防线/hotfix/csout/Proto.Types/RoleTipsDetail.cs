using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class RoleTipsDetail : IMessage<RoleTipsDetail>, IMessage, IEquatable<RoleTipsDetail>, IDeepCloneable<RoleTipsDetail>, IBufferMessage
{
	private static readonly MessageParser<RoleTipsDetail> _parser = new MessageParser<RoleTipsDetail>((Func<RoleTipsDetail>)(() => new RoleTipsDetail()));

	private UnknownFieldSet _unknownFields;

	public const int GunListFieldNumber = 1;

	private static readonly FieldCodec<PlayerGunDetail> _repeated_gunList_codec = FieldCodec.ForMessage<PlayerGunDetail>(10u, PlayerGunDetail.Parser);

	private readonly RepeatedField<PlayerGunDetail> gunList_ = new RepeatedField<PlayerGunDetail>();

	public const int SuitListFieldNumber = 2;

	private static readonly FieldCodec<PlayerSuitDetail> _repeated_suitList_codec = FieldCodec.ForMessage<PlayerSuitDetail>(18u, PlayerSuitDetail.Parser);

	private readonly RepeatedField<PlayerSuitDetail> suitList_ = new RepeatedField<PlayerSuitDetail>();

	public const int WallListFieldNumber = 3;

	private static readonly FieldCodec<WallDetail> _repeated_wallList_codec = FieldCodec.ForMessage<WallDetail>(26u, WallDetail.Parser);

	private readonly RepeatedField<WallDetail> wallList_ = new RepeatedField<WallDetail>();

	public const int MechaListFieldNumber = 4;

	private static readonly FieldCodec<MechaDetail> _repeated_mechaList_codec = FieldCodec.ForMessage<MechaDetail>(34u, MechaDetail.Parser);

	private readonly RepeatedField<MechaDetail> mechaList_ = new RepeatedField<MechaDetail>();

	public const int FootholdListFieldNumber = 5;

	private static readonly FieldCodec<FootholdDetail> _repeated_footholdList_codec = FieldCodec.ForMessage<FootholdDetail>(42u, FootholdDetail.Parser);

	private readonly RepeatedField<FootholdDetail> footholdList_ = new RepeatedField<FootholdDetail>();

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<RoleTipsDetail> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeRoleTipsDetailReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<PlayerGunDetail> GunList => gunList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<PlayerSuitDetail> SuitList => suitList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<WallDetail> WallList => wallList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<MechaDetail> MechaList => mechaList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<FootholdDetail> FootholdList => footholdList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RoleTipsDetail()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RoleTipsDetail(RoleTipsDetail other)
		: this()
	{
		gunList_ = other.gunList_.Clone();
		suitList_ = other.suitList_.Clone();
		wallList_ = other.wallList_.Clone();
		mechaList_ = other.mechaList_.Clone();
		footholdList_ = other.footholdList_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RoleTipsDetail Clone()
	{
		return new RoleTipsDetail(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as RoleTipsDetail);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(RoleTipsDetail other)
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
		if (!suitList_.Equals(other.suitList_))
		{
			return false;
		}
		if (!wallList_.Equals(other.wallList_))
		{
			return false;
		}
		if (!mechaList_.Equals(other.mechaList_))
		{
			return false;
		}
		if (!footholdList_.Equals(other.footholdList_))
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
		num ^= ((object)suitList_).GetHashCode();
		num ^= ((object)wallList_).GetHashCode();
		num ^= ((object)mechaList_).GetHashCode();
		num ^= ((object)footholdList_).GetHashCode();
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
		suitList_.WriteTo(ref output, _repeated_suitList_codec);
		wallList_.WriteTo(ref output, _repeated_wallList_codec);
		mechaList_.WriteTo(ref output, _repeated_mechaList_codec);
		footholdList_.WriteTo(ref output, _repeated_footholdList_codec);
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
		num += suitList_.CalculateSize(_repeated_suitList_codec);
		num += wallList_.CalculateSize(_repeated_wallList_codec);
		num += mechaList_.CalculateSize(_repeated_mechaList_codec);
		num += footholdList_.CalculateSize(_repeated_footholdList_codec);
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(RoleTipsDetail other)
	{
		if (other != null)
		{
			gunList_.Add((IEnumerable<PlayerGunDetail>)other.gunList_);
			suitList_.Add((IEnumerable<PlayerSuitDetail>)other.suitList_);
			wallList_.Add((IEnumerable<WallDetail>)other.wallList_);
			mechaList_.Add((IEnumerable<MechaDetail>)other.mechaList_);
			footholdList_.Add((IEnumerable<FootholdDetail>)other.footholdList_);
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
			case 18u:
				suitList_.AddEntriesFrom(ref input, _repeated_suitList_codec);
				break;
			case 26u:
				wallList_.AddEntriesFrom(ref input, _repeated_wallList_codec);
				break;
			case 34u:
				mechaList_.AddEntriesFrom(ref input, _repeated_mechaList_codec);
				break;
			case 42u:
				footholdList_.AddEntriesFrom(ref input, _repeated_footholdList_codec);
				break;
			}
		}
	}
}
