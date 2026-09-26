using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class HomelandV2RoomDestroyAck : IMessage<HomelandV2RoomDestroyAck>, IMessage, IEquatable<HomelandV2RoomDestroyAck>, IDeepCloneable<HomelandV2RoomDestroyAck>, IBufferMessage
{
	private static readonly MessageParser<HomelandV2RoomDestroyAck> _parser = new MessageParser<HomelandV2RoomDestroyAck>((Func<HomelandV2RoomDestroyAck>)(() => new HomelandV2RoomDestroyAck()));

	private UnknownFieldSet _unknownFields;

	public const int HomelandInfoFieldNumber = 1;

	private HomelandInfo homelandInfo_;

	public const int WarRoomInfoFieldNumber = 2;

	private WarRoomInfo warRoomInfo_;

	public const int DefenseInfoFieldNumber = 3;

	private DefenseInfo defenseInfo_;

	public const int AwardListFieldNumber = 4;

	private static readonly FieldCodec<AwardDetail> _repeated_awardList_codec = FieldCodec.ForMessage<AwardDetail>(34u, AwardDetail.Parser);

	private readonly RepeatedField<AwardDetail> awardList_ = new RepeatedField<AwardDetail>();

	public const int DataChangeFieldNumber = 5;

	private DataChange dataChange_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<HomelandV2RoomDestroyAck> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeHomelandV2RoomDestroyAckReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public HomelandInfo HomelandInfo
	{
		get
		{
			return homelandInfo_;
		}
		set
		{
			homelandInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public WarRoomInfo WarRoomInfo
	{
		get
		{
			return warRoomInfo_;
		}
		set
		{
			warRoomInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public DefenseInfo DefenseInfo
	{
		get
		{
			return defenseInfo_;
		}
		set
		{
			defenseInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<AwardDetail> AwardList => awardList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public DataChange DataChange
	{
		get
		{
			return dataChange_;
		}
		set
		{
			dataChange_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public HomelandV2RoomDestroyAck()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public HomelandV2RoomDestroyAck(HomelandV2RoomDestroyAck other)
		: this()
	{
		homelandInfo_ = ((other.homelandInfo_ != null) ? other.homelandInfo_.Clone() : null);
		warRoomInfo_ = ((other.warRoomInfo_ != null) ? other.warRoomInfo_.Clone() : null);
		defenseInfo_ = ((other.defenseInfo_ != null) ? other.defenseInfo_.Clone() : null);
		awardList_ = other.awardList_.Clone();
		dataChange_ = ((other.dataChange_ != null) ? other.dataChange_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public HomelandV2RoomDestroyAck Clone()
	{
		return new HomelandV2RoomDestroyAck(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as HomelandV2RoomDestroyAck);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(HomelandV2RoomDestroyAck other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!object.Equals(HomelandInfo, other.HomelandInfo))
		{
			return false;
		}
		if (!object.Equals(WarRoomInfo, other.WarRoomInfo))
		{
			return false;
		}
		if (!object.Equals(DefenseInfo, other.DefenseInfo))
		{
			return false;
		}
		if (!awardList_.Equals(other.awardList_))
		{
			return false;
		}
		if (!object.Equals(DataChange, other.DataChange))
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
		if (homelandInfo_ != null)
		{
			num ^= HomelandInfo.GetHashCode();
		}
		if (warRoomInfo_ != null)
		{
			num ^= WarRoomInfo.GetHashCode();
		}
		if (defenseInfo_ != null)
		{
			num ^= DefenseInfo.GetHashCode();
		}
		num ^= ((object)awardList_).GetHashCode();
		if (dataChange_ != null)
		{
			num ^= DataChange.GetHashCode();
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
		if (homelandInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)10);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)HomelandInfo);
		}
		if (warRoomInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)18);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)WarRoomInfo);
		}
		if (defenseInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)26);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)DefenseInfo);
		}
		awardList_.WriteTo(ref output, _repeated_awardList_codec);
		if (dataChange_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)42);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)DataChange);
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
		if (homelandInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)HomelandInfo);
		}
		if (warRoomInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)WarRoomInfo);
		}
		if (defenseInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)DefenseInfo);
		}
		num += awardList_.CalculateSize(_repeated_awardList_codec);
		if (dataChange_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)DataChange);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(HomelandV2RoomDestroyAck other)
	{
		if (other == null)
		{
			return;
		}
		if (other.homelandInfo_ != null)
		{
			if (homelandInfo_ == null)
			{
				HomelandInfo = new HomelandInfo();
			}
			HomelandInfo.MergeFrom(other.HomelandInfo);
		}
		if (other.warRoomInfo_ != null)
		{
			if (warRoomInfo_ == null)
			{
				WarRoomInfo = new WarRoomInfo();
			}
			WarRoomInfo.MergeFrom(other.WarRoomInfo);
		}
		if (other.defenseInfo_ != null)
		{
			if (defenseInfo_ == null)
			{
				DefenseInfo = new DefenseInfo();
			}
			DefenseInfo.MergeFrom(other.DefenseInfo);
		}
		awardList_.Add((IEnumerable<AwardDetail>)other.awardList_);
		if (other.dataChange_ != null)
		{
			if (dataChange_ == null)
			{
				DataChange = new DataChange();
			}
			DataChange.MergeFrom(other.DataChange);
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
			case 10u:
				if (homelandInfo_ == null)
				{
					HomelandInfo = new HomelandInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)HomelandInfo);
				break;
			case 18u:
				if (warRoomInfo_ == null)
				{
					WarRoomInfo = new WarRoomInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)WarRoomInfo);
				break;
			case 26u:
				if (defenseInfo_ == null)
				{
					DefenseInfo = new DefenseInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)DefenseInfo);
				break;
			case 34u:
				awardList_.AddEntriesFrom(ref input, _repeated_awardList_codec);
				break;
			case 42u:
				if (dataChange_ == null)
				{
					DataChange = new DataChange();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)DataChange);
				break;
			}
		}
	}
}
