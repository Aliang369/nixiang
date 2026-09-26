using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class WorldLairRefreshAck : IMessage<WorldLairRefreshAck>, IMessage, IEquatable<WorldLairRefreshAck>, IDeepCloneable<WorldLairRefreshAck>, IBufferMessage
{
	private static readonly MessageParser<WorldLairRefreshAck> _parser = new MessageParser<WorldLairRefreshAck>((Func<WorldLairRefreshAck>)(() => new WorldLairRefreshAck()));

	private UnknownFieldSet _unknownFields;

	public const int WorldLairInfoFieldNumber = 1;

	private WorldLairInfo worldLairInfo_;

	public const int WorldLairHelpListFieldNumber = 2;

	private static readonly FieldCodec<WorldLairHelpInfo> _repeated_worldLairHelpList_codec = FieldCodec.ForMessage<WorldLairHelpInfo>(18u, WorldLairHelpInfo.Parser);

	private readonly RepeatedField<WorldLairHelpInfo> worldLairHelpList_ = new RepeatedField<WorldLairHelpInfo>();

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<WorldLairRefreshAck> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeWorldLairRefreshAckReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public WorldLairInfo WorldLairInfo
	{
		get
		{
			return worldLairInfo_;
		}
		set
		{
			worldLairInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<WorldLairHelpInfo> WorldLairHelpList => worldLairHelpList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public WorldLairRefreshAck()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public WorldLairRefreshAck(WorldLairRefreshAck other)
		: this()
	{
		worldLairInfo_ = ((other.worldLairInfo_ != null) ? other.worldLairInfo_.Clone() : null);
		worldLairHelpList_ = other.worldLairHelpList_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public WorldLairRefreshAck Clone()
	{
		return new WorldLairRefreshAck(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as WorldLairRefreshAck);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(WorldLairRefreshAck other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!object.Equals(WorldLairInfo, other.WorldLairInfo))
		{
			return false;
		}
		if (!worldLairHelpList_.Equals(other.worldLairHelpList_))
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
		if (worldLairInfo_ != null)
		{
			num ^= WorldLairInfo.GetHashCode();
		}
		num ^= ((object)worldLairHelpList_).GetHashCode();
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
		if (worldLairInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)10);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)WorldLairInfo);
		}
		worldLairHelpList_.WriteTo(ref output, _repeated_worldLairHelpList_codec);
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
		if (worldLairInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)WorldLairInfo);
		}
		num += worldLairHelpList_.CalculateSize(_repeated_worldLairHelpList_codec);
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(WorldLairRefreshAck other)
	{
		if (other == null)
		{
			return;
		}
		if (other.worldLairInfo_ != null)
		{
			if (worldLairInfo_ == null)
			{
				WorldLairInfo = new WorldLairInfo();
			}
			WorldLairInfo.MergeFrom(other.WorldLairInfo);
		}
		worldLairHelpList_.Add((IEnumerable<WorldLairHelpInfo>)other.worldLairHelpList_);
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
				if (worldLairInfo_ == null)
				{
					WorldLairInfo = new WorldLairInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)WorldLairInfo);
				break;
			case 18u:
				worldLairHelpList_.AddEntriesFrom(ref input, _repeated_worldLairHelpList_codec);
				break;
			}
		}
	}
}
