using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class VipExpInfo : IMessage<VipExpInfo>, IMessage, IEquatable<VipExpInfo>, IDeepCloneable<VipExpInfo>, IBufferMessage
{
	private static readonly MessageParser<VipExpInfo> _parser = new MessageParser<VipExpInfo>((Func<VipExpInfo>)(() => new VipExpInfo()));

	private UnknownFieldSet _unknownFields;

	public const int VipExpFieldNumber = 1;

	private int vipExp_;

	public const int NextExpFieldNumber = 2;

	private int nextExp_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<VipExpInfo> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeVipExpInfoReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int VipExp
	{
		get
		{
			return vipExp_;
		}
		set
		{
			vipExp_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int NextExp
	{
		get
		{
			return nextExp_;
		}
		set
		{
			nextExp_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public VipExpInfo()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public VipExpInfo(VipExpInfo other)
		: this()
	{
		vipExp_ = other.vipExp_;
		nextExp_ = other.nextExp_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public VipExpInfo Clone()
	{
		return new VipExpInfo(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as VipExpInfo);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(VipExpInfo other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (VipExp != other.VipExp)
		{
			return false;
		}
		if (NextExp != other.NextExp)
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
		if (VipExp != 0)
		{
			num ^= VipExp.GetHashCode();
		}
		if (NextExp != 0)
		{
			num ^= NextExp.GetHashCode();
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
		if (VipExp != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteInt32(VipExp);
		}
		if (NextExp != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteInt32(NextExp);
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
		if (VipExp != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(VipExp);
		}
		if (NextExp != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(NextExp);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(VipExpInfo other)
	{
		if (other != null)
		{
			if (other.VipExp != 0)
			{
				VipExp = other.VipExp;
			}
			if (other.NextExp != 0)
			{
				NextExp = other.NextExp;
			}
			_unknownFields = UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
		}
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
				VipExp = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 16u:
				NextExp = ((ParseContext)(ref input)).ReadInt32();
				break;
			}
		}
	}
}
