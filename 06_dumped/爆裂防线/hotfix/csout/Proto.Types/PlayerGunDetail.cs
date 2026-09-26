using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class PlayerGunDetail : IMessage<PlayerGunDetail>, IMessage, IEquatable<PlayerGunDetail>, IDeepCloneable<PlayerGunDetail>, IBufferMessage
{
	private static readonly MessageParser<PlayerGunDetail> _parser = new MessageParser<PlayerGunDetail>((Func<PlayerGunDetail>)(() => new PlayerGunDetail()));

	private UnknownFieldSet _unknownFields;

	public const int GunIdFieldNumber = 1;

	private int gunId_;

	public const int StarFieldNumber = 2;

	private int star_;

	public const int DamagePctFieldNumber = 3;

	private float damagePct_;

	public const int BulletIdFieldNumber = 4;

	private int bulletId_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<PlayerGunDetail> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypePlayerGunDetailReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int GunId
	{
		get
		{
			return gunId_;
		}
		set
		{
			gunId_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int Star
	{
		get
		{
			return star_;
		}
		set
		{
			star_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public float DamagePct
	{
		get
		{
			return damagePct_;
		}
		set
		{
			damagePct_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int BulletId
	{
		get
		{
			return bulletId_;
		}
		set
		{
			bulletId_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public PlayerGunDetail()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public PlayerGunDetail(PlayerGunDetail other)
		: this()
	{
		gunId_ = other.gunId_;
		star_ = other.star_;
		damagePct_ = other.damagePct_;
		bulletId_ = other.bulletId_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public PlayerGunDetail Clone()
	{
		return new PlayerGunDetail(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as PlayerGunDetail);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(PlayerGunDetail other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (GunId != other.GunId)
		{
			return false;
		}
		if (Star != other.Star)
		{
			return false;
		}
		if (!ProtobufEqualityComparers.BitwiseSingleEqualityComparer.Equals(DamagePct, other.DamagePct))
		{
			return false;
		}
		if (BulletId != other.BulletId)
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
		if (GunId != 0)
		{
			num ^= GunId.GetHashCode();
		}
		if (Star != 0)
		{
			num ^= Star.GetHashCode();
		}
		if (DamagePct != 0f)
		{
			num ^= ProtobufEqualityComparers.BitwiseSingleEqualityComparer.GetHashCode(DamagePct);
		}
		if (BulletId != 0)
		{
			num ^= BulletId.GetHashCode();
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
		if (GunId != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteInt32(GunId);
		}
		if (Star != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteInt32(Star);
		}
		if (DamagePct != 0f)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)29);
			((WriteContext)(ref output)).WriteFloat(DamagePct);
		}
		if (BulletId != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)32);
			((WriteContext)(ref output)).WriteInt32(BulletId);
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
		if (GunId != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(GunId);
		}
		if (Star != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(Star);
		}
		if (DamagePct != 0f)
		{
			num += 5;
		}
		if (BulletId != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(BulletId);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(PlayerGunDetail other)
	{
		if (other != null)
		{
			if (other.GunId != 0)
			{
				GunId = other.GunId;
			}
			if (other.Star != 0)
			{
				Star = other.Star;
			}
			if (other.DamagePct != 0f)
			{
				DamagePct = other.DamagePct;
			}
			if (other.BulletId != 0)
			{
				BulletId = other.BulletId;
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
				GunId = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 16u:
				Star = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 29u:
				DamagePct = ((ParseContext)(ref input)).ReadFloat();
				break;
			case 32u:
				BulletId = ((ParseContext)(ref input)).ReadInt32();
				break;
			}
		}
	}
}
