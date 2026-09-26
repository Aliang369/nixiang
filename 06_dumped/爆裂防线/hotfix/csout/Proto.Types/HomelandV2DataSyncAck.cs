using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class HomelandV2DataSyncAck : IMessage<HomelandV2DataSyncAck>, IMessage, IEquatable<HomelandV2DataSyncAck>, IDeepCloneable<HomelandV2DataSyncAck>, IBufferMessage
{
	private static readonly MessageParser<HomelandV2DataSyncAck> _parser = new MessageParser<HomelandV2DataSyncAck>((Func<HomelandV2DataSyncAck>)(() => new HomelandV2DataSyncAck()));

	private UnknownFieldSet _unknownFields;

	public const int ResIdleInfoFieldNumber = 1;

	private WorldResIdleInfo resIdleInfo_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<HomelandV2DataSyncAck> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeHomelandV2DataSyncAckReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

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
	public HomelandV2DataSyncAck()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public HomelandV2DataSyncAck(HomelandV2DataSyncAck other)
		: this()
	{
		resIdleInfo_ = ((other.resIdleInfo_ != null) ? other.resIdleInfo_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public HomelandV2DataSyncAck Clone()
	{
		return new HomelandV2DataSyncAck(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as HomelandV2DataSyncAck);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(HomelandV2DataSyncAck other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!object.Equals(ResIdleInfo, other.ResIdleInfo))
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
		if (resIdleInfo_ != null)
		{
			num ^= ResIdleInfo.GetHashCode();
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
		if (resIdleInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)10);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)ResIdleInfo);
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
		if (resIdleInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)ResIdleInfo);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(HomelandV2DataSyncAck other)
	{
		if (other == null)
		{
			return;
		}
		if (other.resIdleInfo_ != null)
		{
			if (resIdleInfo_ == null)
			{
				ResIdleInfo = new WorldResIdleInfo();
			}
			ResIdleInfo.MergeFrom(other.ResIdleInfo);
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
			if (num != 10)
			{
				_unknownFields = UnknownFieldSet.MergeFieldFrom(_unknownFields, ref input);
				continue;
			}
			if (resIdleInfo_ == null)
			{
				ResIdleInfo = new WorldResIdleInfo();
			}
			((ParseContext)(ref input)).ReadMessage((IMessage)(object)ResIdleInfo);
		}
	}
}
