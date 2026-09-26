using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class MineWarFightStartAck : IMessage<MineWarFightStartAck>, IMessage, IEquatable<MineWarFightStartAck>, IDeepCloneable<MineWarFightStartAck>, IBufferMessage
{
	private static readonly MessageParser<MineWarFightStartAck> _parser = new MessageParser<MineWarFightStartAck>((Func<MineWarFightStartAck>)(() => new MineWarFightStartAck()));

	private UnknownFieldSet _unknownFields;

	public const int PointIdFieldNumber = 1;

	private int pointId_;

	public const int PlayerIdFieldNumber = 2;

	private int playerId_;

	public const int BattleInfoFieldNumber = 3;

	private BattleInfo battleInfo_;

	public const int MineWarBaseInfoFieldNumber = 4;

	private MineWarBaseInfo mineWarBaseInfo_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<MineWarFightStartAck> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeMineWarFightStartAckReflection.Descriptor.MessageTypes[0];

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
	public BattleInfo BattleInfo
	{
		get
		{
			return battleInfo_;
		}
		set
		{
			battleInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public MineWarBaseInfo MineWarBaseInfo
	{
		get
		{
			return mineWarBaseInfo_;
		}
		set
		{
			mineWarBaseInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public MineWarFightStartAck()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public MineWarFightStartAck(MineWarFightStartAck other)
		: this()
	{
		pointId_ = other.pointId_;
		playerId_ = other.playerId_;
		battleInfo_ = ((other.battleInfo_ != null) ? other.battleInfo_.Clone() : null);
		mineWarBaseInfo_ = ((other.mineWarBaseInfo_ != null) ? other.mineWarBaseInfo_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public MineWarFightStartAck Clone()
	{
		return new MineWarFightStartAck(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as MineWarFightStartAck);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(MineWarFightStartAck other)
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
		if (!object.Equals(BattleInfo, other.BattleInfo))
		{
			return false;
		}
		if (!object.Equals(MineWarBaseInfo, other.MineWarBaseInfo))
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
		if (battleInfo_ != null)
		{
			num ^= BattleInfo.GetHashCode();
		}
		if (mineWarBaseInfo_ != null)
		{
			num ^= MineWarBaseInfo.GetHashCode();
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
		if (battleInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)26);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)BattleInfo);
		}
		if (mineWarBaseInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)34);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)MineWarBaseInfo);
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
		if (battleInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)BattleInfo);
		}
		if (mineWarBaseInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)MineWarBaseInfo);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(MineWarFightStartAck other)
	{
		if (other == null)
		{
			return;
		}
		if (other.PointId != 0)
		{
			PointId = other.PointId;
		}
		if (other.PlayerId != 0)
		{
			PlayerId = other.PlayerId;
		}
		if (other.battleInfo_ != null)
		{
			if (battleInfo_ == null)
			{
				BattleInfo = new BattleInfo();
			}
			BattleInfo.MergeFrom(other.BattleInfo);
		}
		if (other.mineWarBaseInfo_ != null)
		{
			if (mineWarBaseInfo_ == null)
			{
				MineWarBaseInfo = new MineWarBaseInfo();
			}
			MineWarBaseInfo.MergeFrom(other.MineWarBaseInfo);
		}
		_unknownFields = UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
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
				if (battleInfo_ == null)
				{
					BattleInfo = new BattleInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)BattleInfo);
				break;
			case 34u:
				if (mineWarBaseInfo_ == null)
				{
					MineWarBaseInfo = new MineWarBaseInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)MineWarBaseInfo);
				break;
			}
		}
	}
}
