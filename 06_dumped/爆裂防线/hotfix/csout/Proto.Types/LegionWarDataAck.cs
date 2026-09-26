using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class LegionWarDataAck : IMessage<LegionWarDataAck>, IMessage, IEquatable<LegionWarDataAck>, IDeepCloneable<LegionWarDataAck>, IBufferMessage
{
	private static readonly MessageParser<LegionWarDataAck> _parser = new MessageParser<LegionWarDataAck>((Func<LegionWarDataAck>)(() => new LegionWarDataAck()));

	private UnknownFieldSet _unknownFields;

	public const int WarStateFieldNumber = 1;

	private ELegionWarState warState_;

	public const int LegionWarBaseInfoFieldNumber = 2;

	private LegionWarBaseInfo legionWarBaseInfo_;

	public const int LegionWarLegionInfoFieldNumber = 3;

	private LegionWarLegionInfo legionWarLegionInfo_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<LegionWarDataAck> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeLegionWarDataAckReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ELegionWarState WarState
	{
		get
		{
			return warState_;
		}
		set
		{
			warState_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public LegionWarBaseInfo LegionWarBaseInfo
	{
		get
		{
			return legionWarBaseInfo_;
		}
		set
		{
			legionWarBaseInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public LegionWarLegionInfo LegionWarLegionInfo
	{
		get
		{
			return legionWarLegionInfo_;
		}
		set
		{
			legionWarLegionInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public LegionWarDataAck()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public LegionWarDataAck(LegionWarDataAck other)
		: this()
	{
		warState_ = other.warState_;
		legionWarBaseInfo_ = ((other.legionWarBaseInfo_ != null) ? other.legionWarBaseInfo_.Clone() : null);
		legionWarLegionInfo_ = ((other.legionWarLegionInfo_ != null) ? other.legionWarLegionInfo_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public LegionWarDataAck Clone()
	{
		return new LegionWarDataAck(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as LegionWarDataAck);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(LegionWarDataAck other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (WarState != other.WarState)
		{
			return false;
		}
		if (!object.Equals(LegionWarBaseInfo, other.LegionWarBaseInfo))
		{
			return false;
		}
		if (!object.Equals(LegionWarLegionInfo, other.LegionWarLegionInfo))
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
		if (WarState != ELegionWarState.EmptyValue)
		{
			num ^= WarState.GetHashCode();
		}
		if (legionWarBaseInfo_ != null)
		{
			num ^= LegionWarBaseInfo.GetHashCode();
		}
		if (legionWarLegionInfo_ != null)
		{
			num ^= LegionWarLegionInfo.GetHashCode();
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
		if (WarState != ELegionWarState.EmptyValue)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteEnum((int)WarState);
		}
		if (legionWarBaseInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)18);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)LegionWarBaseInfo);
		}
		if (legionWarLegionInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)26);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)LegionWarLegionInfo);
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
		if (WarState != ELegionWarState.EmptyValue)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)WarState);
		}
		if (legionWarBaseInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)LegionWarBaseInfo);
		}
		if (legionWarLegionInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)LegionWarLegionInfo);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(LegionWarDataAck other)
	{
		if (other == null)
		{
			return;
		}
		if (other.WarState != ELegionWarState.EmptyValue)
		{
			WarState = other.WarState;
		}
		if (other.legionWarBaseInfo_ != null)
		{
			if (legionWarBaseInfo_ == null)
			{
				LegionWarBaseInfo = new LegionWarBaseInfo();
			}
			LegionWarBaseInfo.MergeFrom(other.LegionWarBaseInfo);
		}
		if (other.legionWarLegionInfo_ != null)
		{
			if (legionWarLegionInfo_ == null)
			{
				LegionWarLegionInfo = new LegionWarLegionInfo();
			}
			LegionWarLegionInfo.MergeFrom(other.LegionWarLegionInfo);
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
				WarState = (ELegionWarState)((ParseContext)(ref input)).ReadEnum();
				break;
			case 18u:
				if (legionWarBaseInfo_ == null)
				{
					LegionWarBaseInfo = new LegionWarBaseInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)LegionWarBaseInfo);
				break;
			case 26u:
				if (legionWarLegionInfo_ == null)
				{
					LegionWarLegionInfo = new LegionWarLegionInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)LegionWarLegionInfo);
				break;
			}
		}
	}
}
