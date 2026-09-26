using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class MineWarPointDetail : IMessage<MineWarPointDetail>, IMessage, IEquatable<MineWarPointDetail>, IDeepCloneable<MineWarPointDetail>, IBufferMessage
{
	private static readonly MessageParser<MineWarPointDetail> _parser = new MessageParser<MineWarPointDetail>((Func<MineWarPointDetail>)(() => new MineWarPointDetail()));

	private UnknownFieldSet _unknownFields;

	public const int PointIdFieldNumber = 1;

	private int pointId_;

	public const int PlayerIdFieldNumber = 2;

	private int playerId_;

	public const int PlayerNameFieldNumber = 3;

	private string playerName_ = "";

	public const int ProtectTmFieldNumber = 4;

	private long protectTm_;

	public const int LockedTmFieldNumber = 5;

	private long lockedTm_;

	public const int DestroyTmFieldNumber = 6;

	private long destroyTm_;

	public const int StartTmFieldNumber = 7;

	private long startTm_;

	public const int FullTmFieldNumber = 8;

	private long fullTm_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<MineWarPointDetail> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeMineWarPointDetailReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

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
	public string PlayerName
	{
		get
		{
			return playerName_;
		}
		set
		{
			playerName_ = ProtoPreconditions.CheckNotNull<string>(value, "value");
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public long ProtectTm
	{
		get
		{
			return protectTm_;
		}
		set
		{
			protectTm_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public long LockedTm
	{
		get
		{
			return lockedTm_;
		}
		set
		{
			lockedTm_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public long DestroyTm
	{
		get
		{
			return destroyTm_;
		}
		set
		{
			destroyTm_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public long StartTm
	{
		get
		{
			return startTm_;
		}
		set
		{
			startTm_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public long FullTm
	{
		get
		{
			return fullTm_;
		}
		set
		{
			fullTm_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public MineWarPointDetail()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public MineWarPointDetail(MineWarPointDetail other)
		: this()
	{
		pointId_ = other.pointId_;
		playerId_ = other.playerId_;
		playerName_ = other.playerName_;
		protectTm_ = other.protectTm_;
		lockedTm_ = other.lockedTm_;
		destroyTm_ = other.destroyTm_;
		startTm_ = other.startTm_;
		fullTm_ = other.fullTm_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public MineWarPointDetail Clone()
	{
		return new MineWarPointDetail(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as MineWarPointDetail);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(MineWarPointDetail other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (PointId != other.PointId)
		{
			return false;
		}
		if (PlayerId != other.PlayerId)
		{
			return false;
		}
		if (PlayerName != other.PlayerName)
		{
			return false;
		}
		if (ProtectTm != other.ProtectTm)
		{
			return false;
		}
		if (LockedTm != other.LockedTm)
		{
			return false;
		}
		if (DestroyTm != other.DestroyTm)
		{
			return false;
		}
		if (StartTm != other.StartTm)
		{
			return false;
		}
		if (FullTm != other.FullTm)
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
		if (PointId != 0)
		{
			num ^= PointId.GetHashCode();
		}
		if (PlayerId != 0)
		{
			num ^= PlayerId.GetHashCode();
		}
		if (PlayerName.Length != 0)
		{
			num ^= PlayerName.GetHashCode();
		}
		if (ProtectTm != 0L)
		{
			num ^= ProtectTm.GetHashCode();
		}
		if (LockedTm != 0L)
		{
			num ^= LockedTm.GetHashCode();
		}
		if (DestroyTm != 0L)
		{
			num ^= DestroyTm.GetHashCode();
		}
		if (StartTm != 0L)
		{
			num ^= StartTm.GetHashCode();
		}
		if (FullTm != 0L)
		{
			num ^= FullTm.GetHashCode();
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
		if (PointId != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteInt32(PointId);
		}
		if (PlayerId != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteInt32(PlayerId);
		}
		if (PlayerName.Length != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)26);
			((WriteContext)(ref output)).WriteString(PlayerName);
		}
		if (ProtectTm != 0L)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)32);
			((WriteContext)(ref output)).WriteInt64(ProtectTm);
		}
		if (LockedTm != 0L)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)40);
			((WriteContext)(ref output)).WriteInt64(LockedTm);
		}
		if (DestroyTm != 0L)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)48);
			((WriteContext)(ref output)).WriteInt64(DestroyTm);
		}
		if (StartTm != 0L)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)56);
			((WriteContext)(ref output)).WriteInt64(StartTm);
		}
		if (FullTm != 0L)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)64);
			((WriteContext)(ref output)).WriteInt64(FullTm);
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
		if (PointId != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(PointId);
		}
		if (PlayerId != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(PlayerId);
		}
		if (PlayerName.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(PlayerName);
		}
		if (ProtectTm != 0L)
		{
			num += 1 + CodedOutputStream.ComputeInt64Size(ProtectTm);
		}
		if (LockedTm != 0L)
		{
			num += 1 + CodedOutputStream.ComputeInt64Size(LockedTm);
		}
		if (DestroyTm != 0L)
		{
			num += 1 + CodedOutputStream.ComputeInt64Size(DestroyTm);
		}
		if (StartTm != 0L)
		{
			num += 1 + CodedOutputStream.ComputeInt64Size(StartTm);
		}
		if (FullTm != 0L)
		{
			num += 1 + CodedOutputStream.ComputeInt64Size(FullTm);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(MineWarPointDetail other)
	{
		if (other != null)
		{
			if (other.PointId != 0)
			{
				PointId = other.PointId;
			}
			if (other.PlayerId != 0)
			{
				PlayerId = other.PlayerId;
			}
			if (other.PlayerName.Length != 0)
			{
				PlayerName = other.PlayerName;
			}
			if (other.ProtectTm != 0L)
			{
				ProtectTm = other.ProtectTm;
			}
			if (other.LockedTm != 0L)
			{
				LockedTm = other.LockedTm;
			}
			if (other.DestroyTm != 0L)
			{
				DestroyTm = other.DestroyTm;
			}
			if (other.StartTm != 0L)
			{
				StartTm = other.StartTm;
			}
			if (other.FullTm != 0L)
			{
				FullTm = other.FullTm;
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
				PointId = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 16u:
				PlayerId = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 26u:
				PlayerName = ((ParseContext)(ref input)).ReadString();
				break;
			case 32u:
				ProtectTm = ((ParseContext)(ref input)).ReadInt64();
				break;
			case 40u:
				LockedTm = ((ParseContext)(ref input)).ReadInt64();
				break;
			case 48u:
				DestroyTm = ((ParseContext)(ref input)).ReadInt64();
				break;
			case 56u:
				StartTm = ((ParseContext)(ref input)).ReadInt64();
				break;
			case 64u:
				FullTm = ((ParseContext)(ref input)).ReadInt64();
				break;
			}
		}
	}
}
