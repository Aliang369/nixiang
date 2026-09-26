using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class WallSetAck : IMessage<WallSetAck>, IMessage, IEquatable<WallSetAck>, IDeepCloneable<WallSetAck>, IBufferMessage
{
	private static readonly MessageParser<WallSetAck> _parser = new MessageParser<WallSetAck>((Func<WallSetAck>)(() => new WallSetAck()));

	private UnknownFieldSet _unknownFields;

	public const int WallInfoFieldNumber = 1;

	private WallInfo wallInfo_;

	public const int DefenseInfoFieldNumber = 2;

	private DefenseInfo defenseInfo_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<WallSetAck> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeWallSetAckReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public WallInfo WallInfo
	{
		get
		{
			return wallInfo_;
		}
		set
		{
			wallInfo_ = value;
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
	public WallSetAck()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public WallSetAck(WallSetAck other)
		: this()
	{
		wallInfo_ = ((other.wallInfo_ != null) ? other.wallInfo_.Clone() : null);
		defenseInfo_ = ((other.defenseInfo_ != null) ? other.defenseInfo_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public WallSetAck Clone()
	{
		return new WallSetAck(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as WallSetAck);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(WallSetAck other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!object.Equals(WallInfo, other.WallInfo))
		{
			return false;
		}
		if (!object.Equals(DefenseInfo, other.DefenseInfo))
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
		if (wallInfo_ != null)
		{
			num ^= WallInfo.GetHashCode();
		}
		if (defenseInfo_ != null)
		{
			num ^= DefenseInfo.GetHashCode();
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
		if (wallInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)10);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)WallInfo);
		}
		if (defenseInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)18);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)DefenseInfo);
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
		if (wallInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)WallInfo);
		}
		if (defenseInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)DefenseInfo);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(WallSetAck other)
	{
		if (other == null)
		{
			return;
		}
		if (other.wallInfo_ != null)
		{
			if (wallInfo_ == null)
			{
				WallInfo = new WallInfo();
			}
			WallInfo.MergeFrom(other.WallInfo);
		}
		if (other.defenseInfo_ != null)
		{
			if (defenseInfo_ == null)
			{
				DefenseInfo = new DefenseInfo();
			}
			DefenseInfo.MergeFrom(other.DefenseInfo);
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
				if (wallInfo_ == null)
				{
					WallInfo = new WallInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)WallInfo);
				break;
			case 18u:
				if (defenseInfo_ == null)
				{
					DefenseInfo = new DefenseInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)DefenseInfo);
				break;
			}
		}
	}
}
