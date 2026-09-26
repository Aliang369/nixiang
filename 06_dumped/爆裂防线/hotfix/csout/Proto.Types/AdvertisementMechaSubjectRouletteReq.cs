using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class AdvertisementMechaSubjectRouletteReq : IMessage<AdvertisementMechaSubjectRouletteReq>, IMessage, IEquatable<AdvertisementMechaSubjectRouletteReq>, IDeepCloneable<AdvertisementMechaSubjectRouletteReq>, IBufferMessage
{
	private static readonly MessageParser<AdvertisementMechaSubjectRouletteReq> _parser = new MessageParser<AdvertisementMechaSubjectRouletteReq>((Func<AdvertisementMechaSubjectRouletteReq>)(() => new AdvertisementMechaSubjectRouletteReq()));

	private UnknownFieldSet _unknownFields;

	public const int AdvIdFieldNumber = 1;

	private int advId_;

	public const int UUIDFieldNumber = 2;

	private string uUID_ = "";

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<AdvertisementMechaSubjectRouletteReq> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeAdvertisementMechaSubjectRouletteReqReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int AdvId
	{
		get
		{
			return advId_;
		}
		set
		{
			advId_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public string UUID
	{
		get
		{
			return uUID_;
		}
		set
		{
			uUID_ = ProtoPreconditions.CheckNotNull<string>(value, "value");
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public AdvertisementMechaSubjectRouletteReq()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public AdvertisementMechaSubjectRouletteReq(AdvertisementMechaSubjectRouletteReq other)
		: this()
	{
		advId_ = other.advId_;
		uUID_ = other.uUID_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public AdvertisementMechaSubjectRouletteReq Clone()
	{
		return new AdvertisementMechaSubjectRouletteReq(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as AdvertisementMechaSubjectRouletteReq);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(AdvertisementMechaSubjectRouletteReq other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (AdvId != other.AdvId)
		{
			return false;
		}
		if (UUID != other.UUID)
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
		if (AdvId != 0)
		{
			num ^= AdvId.GetHashCode();
		}
		if (UUID.Length != 0)
		{
			num ^= UUID.GetHashCode();
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
		if (AdvId != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteInt32(AdvId);
		}
		if (UUID.Length != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)18);
			((WriteContext)(ref output)).WriteString(UUID);
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
		if (AdvId != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(AdvId);
		}
		if (UUID.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(UUID);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(AdvertisementMechaSubjectRouletteReq other)
	{
		if (other != null)
		{
			if (other.AdvId != 0)
			{
				AdvId = other.AdvId;
			}
			if (other.UUID.Length != 0)
			{
				UUID = other.UUID;
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
				AdvId = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 18u:
				UUID = ((ParseContext)(ref input)).ReadString();
				break;
			}
		}
	}
}
