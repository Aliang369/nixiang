using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class LegionCreateAck : IMessage<LegionCreateAck>, IMessage, IEquatable<LegionCreateAck>, IDeepCloneable<LegionCreateAck>, IBufferMessage
{
	private static readonly MessageParser<LegionCreateAck> _parser = new MessageParser<LegionCreateAck>((Func<LegionCreateAck>)(() => new LegionCreateAck()));

	private UnknownFieldSet _unknownFields;

	public const int LegionSelfInfoFieldNumber = 1;

	private LegionSelfInfo legionSelfInfo_;

	public const int LegionInfoFieldNumber = 2;

	private LegionInfo legionInfo_;

	public const int DataChangeFieldNumber = 3;

	private DataChange dataChange_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<LegionCreateAck> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeLegionCreateAckReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public LegionSelfInfo LegionSelfInfo
	{
		get
		{
			return legionSelfInfo_;
		}
		set
		{
			legionSelfInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public LegionInfo LegionInfo
	{
		get
		{
			return legionInfo_;
		}
		set
		{
			legionInfo_ = value;
		}
	}

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
	public LegionCreateAck()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public LegionCreateAck(LegionCreateAck other)
		: this()
	{
		legionSelfInfo_ = ((other.legionSelfInfo_ != null) ? other.legionSelfInfo_.Clone() : null);
		legionInfo_ = ((other.legionInfo_ != null) ? other.legionInfo_.Clone() : null);
		dataChange_ = ((other.dataChange_ != null) ? other.dataChange_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public LegionCreateAck Clone()
	{
		return new LegionCreateAck(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as LegionCreateAck);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(LegionCreateAck other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!object.Equals(LegionSelfInfo, other.LegionSelfInfo))
		{
			return false;
		}
		if (!object.Equals(LegionInfo, other.LegionInfo))
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
		if (legionSelfInfo_ != null)
		{
			num ^= LegionSelfInfo.GetHashCode();
		}
		if (legionInfo_ != null)
		{
			num ^= LegionInfo.GetHashCode();
		}
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
		if (legionSelfInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)10);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)LegionSelfInfo);
		}
		if (legionInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)18);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)LegionInfo);
		}
		if (dataChange_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)26);
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
		if (legionSelfInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)LegionSelfInfo);
		}
		if (legionInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)LegionInfo);
		}
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
	public void MergeFrom(LegionCreateAck other)
	{
		if (other == null)
		{
			return;
		}
		if (other.legionSelfInfo_ != null)
		{
			if (legionSelfInfo_ == null)
			{
				LegionSelfInfo = new LegionSelfInfo();
			}
			LegionSelfInfo.MergeFrom(other.LegionSelfInfo);
		}
		if (other.legionInfo_ != null)
		{
			if (legionInfo_ == null)
			{
				LegionInfo = new LegionInfo();
			}
			LegionInfo.MergeFrom(other.LegionInfo);
		}
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
				if (legionSelfInfo_ == null)
				{
					LegionSelfInfo = new LegionSelfInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)LegionSelfInfo);
				break;
			case 18u:
				if (legionInfo_ == null)
				{
					LegionInfo = new LegionInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)LegionInfo);
				break;
			case 26u:
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
