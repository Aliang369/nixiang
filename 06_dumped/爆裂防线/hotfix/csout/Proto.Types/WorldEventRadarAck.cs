using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class WorldEventRadarAck : IMessage<WorldEventRadarAck>, IMessage, IEquatable<WorldEventRadarAck>, IDeepCloneable<WorldEventRadarAck>, IBufferMessage
{
	private static readonly MessageParser<WorldEventRadarAck> _parser = new MessageParser<WorldEventRadarAck>((Func<WorldEventRadarAck>)(() => new WorldEventRadarAck()));

	private UnknownFieldSet _unknownFields;

	public const int WorldInfoFieldNumber = 1;

	private WorldInfo worldInfo_;

	public const int HomelandInfoFieldNumber = 2;

	private HomelandInfo homelandInfo_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<WorldEventRadarAck> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeWorldEventRadarAckReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public WorldInfo WorldInfo
	{
		get
		{
			return worldInfo_;
		}
		set
		{
			worldInfo_ = value;
		}
	}

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
	public WorldEventRadarAck()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public WorldEventRadarAck(WorldEventRadarAck other)
		: this()
	{
		worldInfo_ = ((other.worldInfo_ != null) ? other.worldInfo_.Clone() : null);
		homelandInfo_ = ((other.homelandInfo_ != null) ? other.homelandInfo_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public WorldEventRadarAck Clone()
	{
		return new WorldEventRadarAck(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as WorldEventRadarAck);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(WorldEventRadarAck other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!object.Equals(WorldInfo, other.WorldInfo))
		{
			return false;
		}
		if (!object.Equals(HomelandInfo, other.HomelandInfo))
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
		if (worldInfo_ != null)
		{
			num ^= WorldInfo.GetHashCode();
		}
		if (homelandInfo_ != null)
		{
			num ^= HomelandInfo.GetHashCode();
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
		if (worldInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)10);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)WorldInfo);
		}
		if (homelandInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)18);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)HomelandInfo);
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
		if (worldInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)WorldInfo);
		}
		if (homelandInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)HomelandInfo);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(WorldEventRadarAck other)
	{
		if (other == null)
		{
			return;
		}
		if (other.worldInfo_ != null)
		{
			if (worldInfo_ == null)
			{
				WorldInfo = new WorldInfo();
			}
			WorldInfo.MergeFrom(other.WorldInfo);
		}
		if (other.homelandInfo_ != null)
		{
			if (homelandInfo_ == null)
			{
				HomelandInfo = new HomelandInfo();
			}
			HomelandInfo.MergeFrom(other.HomelandInfo);
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
				if (worldInfo_ == null)
				{
					WorldInfo = new WorldInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)WorldInfo);
				break;
			case 18u:
				if (homelandInfo_ == null)
				{
					HomelandInfo = new HomelandInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)HomelandInfo);
				break;
			}
		}
	}
}
