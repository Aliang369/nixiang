using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class RolePvpTipsInfoAck : IMessage<RolePvpTipsInfoAck>, IMessage, IEquatable<RolePvpTipsInfoAck>, IDeepCloneable<RolePvpTipsInfoAck>, IBufferMessage
{
	private static readonly MessageParser<RolePvpTipsInfoAck> _parser = new MessageParser<RolePvpTipsInfoAck>((Func<RolePvpTipsInfoAck>)(() => new RolePvpTipsInfoAck()));

	private UnknownFieldSet _unknownFields;

	public const int PvpTipsInfoFieldNumber = 1;

	private RolePvpTipsInfo pvpTipsInfo_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<RolePvpTipsInfoAck> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeRolePvpTipsInfoAckReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RolePvpTipsInfo PvpTipsInfo
	{
		get
		{
			return pvpTipsInfo_;
		}
		set
		{
			pvpTipsInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RolePvpTipsInfoAck()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RolePvpTipsInfoAck(RolePvpTipsInfoAck other)
		: this()
	{
		pvpTipsInfo_ = ((other.pvpTipsInfo_ != null) ? other.pvpTipsInfo_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RolePvpTipsInfoAck Clone()
	{
		return new RolePvpTipsInfoAck(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as RolePvpTipsInfoAck);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(RolePvpTipsInfoAck other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!object.Equals(PvpTipsInfo, other.PvpTipsInfo))
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
		if (pvpTipsInfo_ != null)
		{
			num ^= PvpTipsInfo.GetHashCode();
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
		if (pvpTipsInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)10);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)PvpTipsInfo);
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
		if (pvpTipsInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)PvpTipsInfo);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(RolePvpTipsInfoAck other)
	{
		if (other == null)
		{
			return;
		}
		if (other.pvpTipsInfo_ != null)
		{
			if (pvpTipsInfo_ == null)
			{
				PvpTipsInfo = new RolePvpTipsInfo();
			}
			PvpTipsInfo.MergeFrom(other.PvpTipsInfo);
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
			if (pvpTipsInfo_ == null)
			{
				PvpTipsInfo = new RolePvpTipsInfo();
			}
			((ParseContext)(ref input)).ReadMessage((IMessage)(object)PvpTipsInfo);
		}
	}
}
