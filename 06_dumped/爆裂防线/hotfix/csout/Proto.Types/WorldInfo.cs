using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class WorldInfo : IMessage<WorldInfo>, IMessage, IEquatable<WorldInfo>, IDeepCloneable<WorldInfo>, IBufferMessage
{
	private static readonly MessageParser<WorldInfo> _parser = new MessageParser<WorldInfo>((Func<WorldInfo>)(() => new WorldInfo()));

	private UnknownFieldSet _unknownFields;

	public const int WorldMapInfoFieldNumber = 1;

	private WorldMapInfo worldMapInfo_;

	public const int BuildingResListFieldNumber = 2;

	private static readonly FieldCodec<WorldBuildingResDetail> _repeated_buildingResList_codec = FieldCodec.ForMessage<WorldBuildingResDetail>(18u, WorldBuildingResDetail.Parser);

	private readonly RepeatedField<WorldBuildingResDetail> buildingResList_ = new RepeatedField<WorldBuildingResDetail>();

	public const int ResIdleInfoFieldNumber = 3;

	private WorldResIdleInfo resIdleInfo_;

	public const int ReconListFieldNumber = 4;

	private static readonly FieldCodec<WorldReconDetail> _repeated_reconList_codec = FieldCodec.ForMessage<WorldReconDetail>(34u, WorldReconDetail.Parser);

	private readonly RepeatedField<WorldReconDetail> reconList_ = new RepeatedField<WorldReconDetail>();

	public const int EventInfoFieldNumber = 5;

	private WorldEventInfo eventInfo_;

	public const int DungeonBaseInfoFieldNumber = 6;

	private DungeonBaseInfo dungeonBaseInfo_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<WorldInfo> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeWorldInfoReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public WorldMapInfo WorldMapInfo
	{
		get
		{
			return worldMapInfo_;
		}
		set
		{
			worldMapInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<WorldBuildingResDetail> BuildingResList => buildingResList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public WorldResIdleInfo ResIdleInfo
	{
		get
		{
			return resIdleInfo_;
		}
		set
		{
			resIdleInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<WorldReconDetail> ReconList => reconList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public WorldEventInfo EventInfo
	{
		get
		{
			return eventInfo_;
		}
		set
		{
			eventInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public DungeonBaseInfo DungeonBaseInfo
	{
		get
		{
			return dungeonBaseInfo_;
		}
		set
		{
			dungeonBaseInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public WorldInfo()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public WorldInfo(WorldInfo other)
		: this()
	{
		worldMapInfo_ = ((other.worldMapInfo_ != null) ? other.worldMapInfo_.Clone() : null);
		buildingResList_ = other.buildingResList_.Clone();
		resIdleInfo_ = ((other.resIdleInfo_ != null) ? other.resIdleInfo_.Clone() : null);
		reconList_ = other.reconList_.Clone();
		eventInfo_ = ((other.eventInfo_ != null) ? other.eventInfo_.Clone() : null);
		dungeonBaseInfo_ = ((other.dungeonBaseInfo_ != null) ? other.dungeonBaseInfo_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public WorldInfo Clone()
	{
		return new WorldInfo(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as WorldInfo);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(WorldInfo other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!object.Equals(WorldMapInfo, other.WorldMapInfo))
		{
			return false;
		}
		if (!buildingResList_.Equals(other.buildingResList_))
		{
			return false;
		}
		if (!object.Equals(ResIdleInfo, other.ResIdleInfo))
		{
			return false;
		}
		if (!reconList_.Equals(other.reconList_))
		{
			return false;
		}
		if (!object.Equals(EventInfo, other.EventInfo))
		{
			return false;
		}
		if (!object.Equals(DungeonBaseInfo, other.DungeonBaseInfo))
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
		if (worldMapInfo_ != null)
		{
			num ^= WorldMapInfo.GetHashCode();
		}
		num ^= ((object)buildingResList_).GetHashCode();
		if (resIdleInfo_ != null)
		{
			num ^= ResIdleInfo.GetHashCode();
		}
		num ^= ((object)reconList_).GetHashCode();
		if (eventInfo_ != null)
		{
			num ^= EventInfo.GetHashCode();
		}
		if (dungeonBaseInfo_ != null)
		{
			num ^= DungeonBaseInfo.GetHashCode();
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
		if (worldMapInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)10);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)WorldMapInfo);
		}
		buildingResList_.WriteTo(ref output, _repeated_buildingResList_codec);
		if (resIdleInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)26);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)ResIdleInfo);
		}
		reconList_.WriteTo(ref output, _repeated_reconList_codec);
		if (eventInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)42);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)EventInfo);
		}
		if (dungeonBaseInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)50);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)DungeonBaseInfo);
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
		if (worldMapInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)WorldMapInfo);
		}
		num += buildingResList_.CalculateSize(_repeated_buildingResList_codec);
		if (resIdleInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)ResIdleInfo);
		}
		num += reconList_.CalculateSize(_repeated_reconList_codec);
		if (eventInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)EventInfo);
		}
		if (dungeonBaseInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)DungeonBaseInfo);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(WorldInfo other)
	{
		if (other == null)
		{
			return;
		}
		if (other.worldMapInfo_ != null)
		{
			if (worldMapInfo_ == null)
			{
				WorldMapInfo = new WorldMapInfo();
			}
			WorldMapInfo.MergeFrom(other.WorldMapInfo);
		}
		buildingResList_.Add((IEnumerable<WorldBuildingResDetail>)other.buildingResList_);
		if (other.resIdleInfo_ != null)
		{
			if (resIdleInfo_ == null)
			{
				ResIdleInfo = new WorldResIdleInfo();
			}
			ResIdleInfo.MergeFrom(other.ResIdleInfo);
		}
		reconList_.Add((IEnumerable<WorldReconDetail>)other.reconList_);
		if (other.eventInfo_ != null)
		{
			if (eventInfo_ == null)
			{
				EventInfo = new WorldEventInfo();
			}
			EventInfo.MergeFrom(other.EventInfo);
		}
		if (other.dungeonBaseInfo_ != null)
		{
			if (dungeonBaseInfo_ == null)
			{
				DungeonBaseInfo = new DungeonBaseInfo();
			}
			DungeonBaseInfo.MergeFrom(other.DungeonBaseInfo);
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
				if (worldMapInfo_ == null)
				{
					WorldMapInfo = new WorldMapInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)WorldMapInfo);
				break;
			case 18u:
				buildingResList_.AddEntriesFrom(ref input, _repeated_buildingResList_codec);
				break;
			case 26u:
				if (resIdleInfo_ == null)
				{
					ResIdleInfo = new WorldResIdleInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)ResIdleInfo);
				break;
			case 34u:
				reconList_.AddEntriesFrom(ref input, _repeated_reconList_codec);
				break;
			case 42u:
				if (eventInfo_ == null)
				{
					EventInfo = new WorldEventInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)EventInfo);
				break;
			case 50u:
				if (dungeonBaseInfo_ == null)
				{
					DungeonBaseInfo = new DungeonBaseInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)DungeonBaseInfo);
				break;
			}
		}
	}
}
