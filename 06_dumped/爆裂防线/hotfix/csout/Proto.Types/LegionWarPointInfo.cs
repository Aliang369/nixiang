using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class LegionWarPointInfo : IMessage<LegionWarPointInfo>, IMessage, IEquatable<LegionWarPointInfo>, IDeepCloneable<LegionWarPointInfo>, IBufferMessage
{
	private static readonly MessageParser<LegionWarPointInfo> _parser = new MessageParser<LegionWarPointInfo>((Func<LegionWarPointInfo>)(() => new LegionWarPointInfo()));

	private UnknownFieldSet _unknownFields;

	public const int PointStateFieldNumber = 1;

	private LegionWarPointStateDetail pointState_;

	public const int PlayerListFieldNumber = 2;

	private static readonly FieldCodec<LegionWarPointPlayerDetail> _repeated_playerList_codec = FieldCodec.ForMessage<LegionWarPointPlayerDetail>(18u, LegionWarPointPlayerDetail.Parser);

	private readonly RepeatedField<LegionWarPointPlayerDetail> playerList_ = new RepeatedField<LegionWarPointPlayerDetail>();

	public const int FightingListFieldNumber = 3;

	private static readonly FieldCodec<LegionWarFightingDetail> _repeated_fightingList_codec = FieldCodec.ForMessage<LegionWarFightingDetail>(26u, LegionWarFightingDetail.Parser);

	private readonly RepeatedField<LegionWarFightingDetail> fightingList_ = new RepeatedField<LegionWarFightingDetail>();

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<LegionWarPointInfo> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeLegionWarPointInfoReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public LegionWarPointStateDetail PointState
	{
		get
		{
			return pointState_;
		}
		set
		{
			pointState_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<LegionWarPointPlayerDetail> PlayerList => playerList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<LegionWarFightingDetail> FightingList => fightingList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public LegionWarPointInfo()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public LegionWarPointInfo(LegionWarPointInfo other)
		: this()
	{
		pointState_ = ((other.pointState_ != null) ? other.pointState_.Clone() : null);
		playerList_ = other.playerList_.Clone();
		fightingList_ = other.fightingList_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public LegionWarPointInfo Clone()
	{
		return new LegionWarPointInfo(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as LegionWarPointInfo);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(LegionWarPointInfo other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!object.Equals(PointState, other.PointState))
		{
			return false;
		}
		if (!playerList_.Equals(other.playerList_))
		{
			return false;
		}
		if (!fightingList_.Equals(other.fightingList_))
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
		if (pointState_ != null)
		{
			num ^= PointState.GetHashCode();
		}
		num ^= ((object)playerList_).GetHashCode();
		num ^= ((object)fightingList_).GetHashCode();
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
		if (pointState_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)10);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)PointState);
		}
		playerList_.WriteTo(ref output, _repeated_playerList_codec);
		fightingList_.WriteTo(ref output, _repeated_fightingList_codec);
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
		if (pointState_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)PointState);
		}
		num += playerList_.CalculateSize(_repeated_playerList_codec);
		num += fightingList_.CalculateSize(_repeated_fightingList_codec);
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(LegionWarPointInfo other)
	{
		if (other == null)
		{
			return;
		}
		if (other.pointState_ != null)
		{
			if (pointState_ == null)
			{
				PointState = new LegionWarPointStateDetail();
			}
			PointState.MergeFrom(other.PointState);
		}
		playerList_.Add((IEnumerable<LegionWarPointPlayerDetail>)other.playerList_);
		fightingList_.Add((IEnumerable<LegionWarFightingDetail>)other.fightingList_);
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
			case 10u:
				if (pointState_ == null)
				{
					PointState = new LegionWarPointStateDetail();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)PointState);
				break;
			case 18u:
				playerList_.AddEntriesFrom(ref input, _repeated_playerList_codec);
				break;
			case 26u:
				fightingList_.AddEntriesFrom(ref input, _repeated_fightingList_codec);
				break;
			}
		}
	}
}
