using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class WorldLairInfo : IMessage<WorldLairInfo>, IMessage, IEquatable<WorldLairInfo>, IDeepCloneable<WorldLairInfo>, IBufferMessage
{
	private static readonly MessageParser<WorldLairInfo> _parser = new MessageParser<WorldLairInfo>((Func<WorldLairInfo>)(() => new WorldLairInfo()));

	private UnknownFieldSet _unknownFields;

	public const int WorldLairBaseInfoFieldNumber = 1;

	private WorldLairBaseInfo worldLairBaseInfo_;

	public const int SelfLairInfoFieldNumber = 2;

	private WorldLairDetail selfLairInfo_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<WorldLairInfo> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeWorldLairInfoReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public WorldLairBaseInfo WorldLairBaseInfo
	{
		get
		{
			return worldLairBaseInfo_;
		}
		set
		{
			worldLairBaseInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public WorldLairDetail SelfLairInfo
	{
		get
		{
			return selfLairInfo_;
		}
		set
		{
			selfLairInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public WorldLairInfo()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public WorldLairInfo(WorldLairInfo other)
		: this()
	{
		worldLairBaseInfo_ = ((other.worldLairBaseInfo_ != null) ? other.worldLairBaseInfo_.Clone() : null);
		selfLairInfo_ = ((other.selfLairInfo_ != null) ? other.selfLairInfo_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public WorldLairInfo Clone()
	{
		return new WorldLairInfo(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as WorldLairInfo);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(WorldLairInfo other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!object.Equals(WorldLairBaseInfo, other.WorldLairBaseInfo))
		{
			return false;
		}
		if (!object.Equals(SelfLairInfo, other.SelfLairInfo))
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
		if (worldLairBaseInfo_ != null)
		{
			num ^= WorldLairBaseInfo.GetHashCode();
		}
		if (selfLairInfo_ != null)
		{
			num ^= SelfLairInfo.GetHashCode();
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
		if (worldLairBaseInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)10);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)WorldLairBaseInfo);
		}
		if (selfLairInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)18);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)SelfLairInfo);
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
		if (worldLairBaseInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)WorldLairBaseInfo);
		}
		if (selfLairInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)SelfLairInfo);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(WorldLairInfo other)
	{
		if (other == null)
		{
			return;
		}
		if (other.worldLairBaseInfo_ != null)
		{
			if (worldLairBaseInfo_ == null)
			{
				WorldLairBaseInfo = new WorldLairBaseInfo();
			}
			WorldLairBaseInfo.MergeFrom(other.WorldLairBaseInfo);
		}
		if (other.selfLairInfo_ != null)
		{
			if (selfLairInfo_ == null)
			{
				SelfLairInfo = new WorldLairDetail();
			}
			SelfLairInfo.MergeFrom(other.SelfLairInfo);
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
				if (worldLairBaseInfo_ == null)
				{
					WorldLairBaseInfo = new WorldLairBaseInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)WorldLairBaseInfo);
				break;
			case 18u:
				if (selfLairInfo_ == null)
				{
					SelfLairInfo = new WorldLairDetail();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)SelfLairInfo);
				break;
			}
		}
	}
}
