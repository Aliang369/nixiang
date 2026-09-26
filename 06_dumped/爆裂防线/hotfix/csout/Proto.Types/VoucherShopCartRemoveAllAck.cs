using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class VoucherShopCartRemoveAllAck : IMessage<VoucherShopCartRemoveAllAck>, IMessage, IEquatable<VoucherShopCartRemoveAllAck>, IDeepCloneable<VoucherShopCartRemoveAllAck>, IBufferMessage
{
	private static readonly MessageParser<VoucherShopCartRemoveAllAck> _parser = new MessageParser<VoucherShopCartRemoveAllAck>((Func<VoucherShopCartRemoveAllAck>)(() => new VoucherShopCartRemoveAllAck()));

	private UnknownFieldSet _unknownFields;

	public const int VoucherShopInfoFieldNumber = 1;

	private VoucherShopInfo voucherShopInfo_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<VoucherShopCartRemoveAllAck> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeVoucherShopCartRemoveAllAckReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public VoucherShopInfo VoucherShopInfo
	{
		get
		{
			return voucherShopInfo_;
		}
		set
		{
			voucherShopInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public VoucherShopCartRemoveAllAck()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public VoucherShopCartRemoveAllAck(VoucherShopCartRemoveAllAck other)
		: this()
	{
		voucherShopInfo_ = ((other.voucherShopInfo_ != null) ? other.voucherShopInfo_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public VoucherShopCartRemoveAllAck Clone()
	{
		return new VoucherShopCartRemoveAllAck(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as VoucherShopCartRemoveAllAck);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(VoucherShopCartRemoveAllAck other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!object.Equals(VoucherShopInfo, other.VoucherShopInfo))
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
		if (voucherShopInfo_ != null)
		{
			num ^= VoucherShopInfo.GetHashCode();
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
		if (voucherShopInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)10);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)VoucherShopInfo);
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
		if (voucherShopInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)VoucherShopInfo);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(VoucherShopCartRemoveAllAck other)
	{
		if (other == null)
		{
			return;
		}
		if (other.voucherShopInfo_ != null)
		{
			if (voucherShopInfo_ == null)
			{
				VoucherShopInfo = new VoucherShopInfo();
			}
			VoucherShopInfo.MergeFrom(other.VoucherShopInfo);
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
			if (voucherShopInfo_ == null)
			{
				VoucherShopInfo = new VoucherShopInfo();
			}
			((ParseContext)(ref input)).ReadMessage((IMessage)(object)VoucherShopInfo);
		}
	}
}
