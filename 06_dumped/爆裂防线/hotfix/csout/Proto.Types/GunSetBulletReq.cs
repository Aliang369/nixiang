using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class GunSetBulletReq : IMessage<GunSetBulletReq>, IMessage, IEquatable<GunSetBulletReq>, IDeepCloneable<GunSetBulletReq>, IBufferMessage
{
	private static readonly MessageParser<GunSetBulletReq> _parser = new MessageParser<GunSetBulletReq>((Func<GunSetBulletReq>)(() => new GunSetBulletReq()));

	private UnknownFieldSet _unknownFields;

	public const int GunIdFieldNumber = 1;

	private int gunId_;

	public const int BulletIdFieldNumber = 2;

	private int bulletId_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<GunSetBulletReq> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeGunSetBulletReqReflection.Descriptor.MessageTypes[0];

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
	public GunSetBulletReq()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public GunSetBulletReq(GunSetBulletReq other)
		: this()
	{
		gunId_ = other.gunId_;
		bulletId_ = other.bulletId_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public GunSetBulletReq Clone()
	{
		return new GunSetBulletReq(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as GunSetBulletReq);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(GunSetBulletReq other)
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
		if (BulletId != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
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
	public void MergeFrom(GunSetBulletReq other)
	{
		if (other != null)
		{
			if (other.GunId != 0)
			{
				GunId = other.GunId;
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
				BulletId = ((ParseContext)(ref input)).ReadInt32();
				break;
			}
		}
	}
}
