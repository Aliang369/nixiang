using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class LegionWarFightStartAck : IMessage<LegionWarFightStartAck>, IMessage, IEquatable<LegionWarFightStartAck>, IDeepCloneable<LegionWarFightStartAck>, IBufferMessage
{
	private static readonly MessageParser<LegionWarFightStartAck> _parser = new MessageParser<LegionWarFightStartAck>((Func<LegionWarFightStartAck>)(() => new LegionWarFightStartAck()));

	private UnknownFieldSet _unknownFields;

	public const int PlayerIdFieldNumber = 1;

	private int playerId_;

	public const int ActionPointsFieldNumber = 2;

	private int actionPoints_;

	public const int BattleInfoFieldNumber = 3;

	private BattleInfo battleInfo_;

	public const int LegionWarSelfInfoFieldNumber = 4;

	private LegionWarSelfInfo legionWarSelfInfo_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<LegionWarFightStartAck> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeLegionWarFightStartAckReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

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
	public int ActionPoints
	{
		get
		{
			return actionPoints_;
		}
		set
		{
			actionPoints_ = value;
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
	public LegionWarSelfInfo LegionWarSelfInfo
	{
		get
		{
			return legionWarSelfInfo_;
		}
		set
		{
			legionWarSelfInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public LegionWarFightStartAck()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public LegionWarFightStartAck(LegionWarFightStartAck other)
		: this()
	{
		playerId_ = other.playerId_;
		actionPoints_ = other.actionPoints_;
		battleInfo_ = ((other.battleInfo_ != null) ? other.battleInfo_.Clone() : null);
		legionWarSelfInfo_ = ((other.legionWarSelfInfo_ != null) ? other.legionWarSelfInfo_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public LegionWarFightStartAck Clone()
	{
		return new LegionWarFightStartAck(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as LegionWarFightStartAck);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(LegionWarFightStartAck other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (PlayerId != other.PlayerId)
		{
			return false;
		}
		if (ActionPoints != other.ActionPoints)
		{
			return false;
		}
		if (!object.Equals(BattleInfo, other.BattleInfo))
		{
			return false;
		}
		if (!object.Equals(LegionWarSelfInfo, other.LegionWarSelfInfo))
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
		if (PlayerId != 0)
		{
			num ^= PlayerId.GetHashCode();
		}
		if (ActionPoints != 0)
		{
			num ^= ActionPoints.GetHashCode();
		}
		if (battleInfo_ != null)
		{
			num ^= BattleInfo.GetHashCode();
		}
		if (legionWarSelfInfo_ != null)
		{
			num ^= LegionWarSelfInfo.GetHashCode();
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
		if (PlayerId != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteInt32(PlayerId);
		}
		if (ActionPoints != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteInt32(ActionPoints);
		}
		if (battleInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)26);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)BattleInfo);
		}
		if (legionWarSelfInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)34);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)LegionWarSelfInfo);
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
		if (PlayerId != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(PlayerId);
		}
		if (ActionPoints != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(ActionPoints);
		}
		if (battleInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)BattleInfo);
		}
		if (legionWarSelfInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)LegionWarSelfInfo);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(LegionWarFightStartAck other)
	{
		if (other == null)
		{
			return;
		}
		if (other.PlayerId != 0)
		{
			PlayerId = other.PlayerId;
		}
		if (other.ActionPoints != 0)
		{
			ActionPoints = other.ActionPoints;
		}
		if (other.battleInfo_ != null)
		{
			if (battleInfo_ == null)
			{
				BattleInfo = new BattleInfo();
			}
			BattleInfo.MergeFrom(other.BattleInfo);
		}
		if (other.legionWarSelfInfo_ != null)
		{
			if (legionWarSelfInfo_ == null)
			{
				LegionWarSelfInfo = new LegionWarSelfInfo();
			}
			LegionWarSelfInfo.MergeFrom(other.LegionWarSelfInfo);
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
				PlayerId = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 16u:
				ActionPoints = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 26u:
				if (battleInfo_ == null)
				{
					BattleInfo = new BattleInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)BattleInfo);
				break;
			case 34u:
				if (legionWarSelfInfo_ == null)
				{
					LegionWarSelfInfo = new LegionWarSelfInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)LegionWarSelfInfo);
				break;
			}
		}
	}
}
