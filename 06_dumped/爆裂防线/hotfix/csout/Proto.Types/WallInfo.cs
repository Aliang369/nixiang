using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class WallInfo : IMessage<WallInfo>, IMessage, IEquatable<WallInfo>, IDeepCloneable<WallInfo>, IBufferMessage
{
	private static readonly MessageParser<WallInfo> _parser = new MessageParser<WallInfo>((Func<WallInfo>)(() => new WallInfo()));

	private UnknownFieldSet _unknownFields;

	public const int LevelFieldNumber = 1;

	private int level_;

	public const int WallListFieldNumber = 2;

	private static readonly FieldCodec<WallDetail> _repeated_wallList_codec = FieldCodec.ForMessage<WallDetail>(18u, WallDetail.Parser);

	private readonly RepeatedField<WallDetail> wallList_ = new RepeatedField<WallDetail>();

	public const int WallIdFieldNumber = 3;

	private int wallId_;

	public const int UseListFieldNumber = 4;

	private static readonly FieldCodec<int> _repeated_useList_codec = FieldCodec.ForInt32(34u);

	private readonly RepeatedField<int> useList_ = new RepeatedField<int>();

	public const int TotalHpFieldNumber = 5;

	private int totalHp_;

	public const int TotalImmunityFieldNumber = 6;

	private int totalImmunity_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<WallInfo> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeWallInfoReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int Level
	{
		get
		{
			return level_;
		}
		set
		{
			level_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<WallDetail> WallList => wallList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int WallId
	{
		get
		{
			return wallId_;
		}
		set
		{
			wallId_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<int> UseList => useList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int TotalHp
	{
		get
		{
			return totalHp_;
		}
		set
		{
			totalHp_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int TotalImmunity
	{
		get
		{
			return totalImmunity_;
		}
		set
		{
			totalImmunity_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public WallInfo()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public WallInfo(WallInfo other)
		: this()
	{
		level_ = other.level_;
		wallList_ = other.wallList_.Clone();
		wallId_ = other.wallId_;
		useList_ = other.useList_.Clone();
		totalHp_ = other.totalHp_;
		totalImmunity_ = other.totalImmunity_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public WallInfo Clone()
	{
		return new WallInfo(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as WallInfo);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(WallInfo other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (Level != other.Level)
		{
			return false;
		}
		if (!wallList_.Equals(other.wallList_))
		{
			return false;
		}
		if (WallId != other.WallId)
		{
			return false;
		}
		if (!useList_.Equals(other.useList_))
		{
			return false;
		}
		if (TotalHp != other.TotalHp)
		{
			return false;
		}
		if (TotalImmunity != other.TotalImmunity)
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
		if (Level != 0)
		{
			num ^= Level.GetHashCode();
		}
		num ^= ((object)wallList_).GetHashCode();
		if (WallId != 0)
		{
			num ^= WallId.GetHashCode();
		}
		num ^= ((object)useList_).GetHashCode();
		if (TotalHp != 0)
		{
			num ^= TotalHp.GetHashCode();
		}
		if (TotalImmunity != 0)
		{
			num ^= TotalImmunity.GetHashCode();
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
		if (Level != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteInt32(Level);
		}
		wallList_.WriteTo(ref output, _repeated_wallList_codec);
		if (WallId != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)24);
			((WriteContext)(ref output)).WriteInt32(WallId);
		}
		useList_.WriteTo(ref output, _repeated_useList_codec);
		if (TotalHp != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)40);
			((WriteContext)(ref output)).WriteInt32(TotalHp);
		}
		if (TotalImmunity != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)48);
			((WriteContext)(ref output)).WriteInt32(TotalImmunity);
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
		if (Level != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(Level);
		}
		num += wallList_.CalculateSize(_repeated_wallList_codec);
		if (WallId != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(WallId);
		}
		num += useList_.CalculateSize(_repeated_useList_codec);
		if (TotalHp != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(TotalHp);
		}
		if (TotalImmunity != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(TotalImmunity);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(WallInfo other)
	{
		if (other != null)
		{
			if (other.Level != 0)
			{
				Level = other.Level;
			}
			wallList_.Add((IEnumerable<WallDetail>)other.wallList_);
			if (other.WallId != 0)
			{
				WallId = other.WallId;
			}
			useList_.Add((IEnumerable<int>)other.useList_);
			if (other.TotalHp != 0)
			{
				TotalHp = other.TotalHp;
			}
			if (other.TotalImmunity != 0)
			{
				TotalImmunity = other.TotalImmunity;
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
				Level = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 18u:
				wallList_.AddEntriesFrom(ref input, _repeated_wallList_codec);
				break;
			case 24u:
				WallId = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 32u:
			case 34u:
				useList_.AddEntriesFrom(ref input, _repeated_useList_codec);
				break;
			case 40u:
				TotalHp = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 48u:
				TotalImmunity = ((ParseContext)(ref input)).ReadInt32();
				break;
			}
		}
	}
}
