using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class MineWarOccupyPointReq : IMessage<MineWarOccupyPointReq>, IMessage, IEquatable<MineWarOccupyPointReq>, IDeepCloneable<MineWarOccupyPointReq>, IBufferMessage
{
	private static readonly MessageParser<MineWarOccupyPointReq> _parser = new MessageParser<MineWarOccupyPointReq>((Func<MineWarOccupyPointReq>)(() => new MineWarOccupyPointReq()));

	private UnknownFieldSet _unknownFields;

	public const int UUIDFieldNumber = 1;

	private string uUID_ = "";

	public const int PointIdFieldNumber = 2;

	private int pointId_;

	public const int PlayerIdFieldNumber = 3;

	private int playerId_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<MineWarOccupyPointReq> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeMineWarOccupyPointReqReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

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
	public int PointId
	{
		get
		{
			return pointId_;
		}
		set
		{
			pointId_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int PlayerId
	{
		get
		{
			return playerId_;
		}
		set
		{
			playerId_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public MineWarOccupyPointReq()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public MineWarOccupyPointReq(MineWarOccupyPointReq other)
		: this()
	{
		uUID_ = other.uUID_;
		pointId_ = other.pointId_;
		playerId_ = other.playerId_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public MineWarOccupyPointReq Clone()
	{
		return new MineWarOccupyPointReq(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as MineWarOccupyPointReq);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(MineWarOccupyPointReq other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (UUID != other.UUID)
		{
			return false;
		}
		if (PointId != other.PointId)
		{
			return false;
		}
		if (PlayerId != other.PlayerId)
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
		if (UUID.Length != 0)
		{
			num ^= UUID.GetHashCode();
		}
		if (PointId != 0)
		{
			num ^= PointId.GetHashCode();
		}
		if (PlayerId != 0)
		{
			num ^= PlayerId.GetHashCode();
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
		if (UUID.Length != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)10);
			((WriteContext)(ref output)).WriteString(UUID);
		}
		if (PointId != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteInt32(PointId);
		}
		if (PlayerId != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)24);
			((WriteContext)(ref output)).WriteInt32(PlayerId);
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
		if (UUID.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(UUID);
		}
		if (PointId != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(PointId);
		}
		if (PlayerId != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(PlayerId);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(MineWarOccupyPointReq other)
	{
		if (other != null)
		{
			if (other.UUID.Length != 0)
			{
				UUID = other.UUID;
			}
			if (other.PointId != 0)
			{
				PointId = other.PointId;
			}
			if (other.PlayerId != 0)
			{
				PlayerId = other.PlayerId;
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
				UUID = ((ParseContext)(ref input)).ReadString();
				break;
			case 16u:
				PointId = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 24u:
				PlayerId = ((ParseContext)(ref input)).ReadInt32();
				break;
			}
		}
	}
}
