using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class BagGeneComposeCalcReq : IMessage<BagGeneComposeCalcReq>, IMessage, IEquatable<BagGeneComposeCalcReq>, IDeepCloneable<BagGeneComposeCalcReq>, IBufferMessage
{
	private static readonly MessageParser<BagGeneComposeCalcReq> _parser = new MessageParser<BagGeneComposeCalcReq>((Func<BagGeneComposeCalcReq>)(() => new BagGeneComposeCalcReq()));

	private UnknownFieldSet _unknownFields;

	public const int GeneTypeFieldNumber = 1;

	private EGeneType geneType_;

	public const int GeneShapeFieldNumber = 2;

	private EGeneShape geneShape_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<BagGeneComposeCalcReq> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeBagGeneComposeCalcReqReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public EGeneType GeneType
	{
		get
		{
			return geneType_;
		}
		set
		{
			geneType_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public EGeneShape GeneShape
	{
		get
		{
			return geneShape_;
		}
		set
		{
			geneShape_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public BagGeneComposeCalcReq()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public BagGeneComposeCalcReq(BagGeneComposeCalcReq other)
		: this()
	{
		geneType_ = other.geneType_;
		geneShape_ = other.geneShape_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public BagGeneComposeCalcReq Clone()
	{
		return new BagGeneComposeCalcReq(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as BagGeneComposeCalcReq);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(BagGeneComposeCalcReq other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (GeneType != other.GeneType)
		{
			return false;
		}
		if (GeneShape != other.GeneShape)
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
		if (GeneType != EGeneType.EmptyValue)
		{
			num ^= GeneType.GetHashCode();
		}
		if (GeneShape != EGeneShape.EmptyValue)
		{
			num ^= GeneShape.GetHashCode();
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
		if (GeneType != EGeneType.EmptyValue)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteEnum((int)GeneType);
		}
		if (GeneShape != EGeneShape.EmptyValue)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteEnum((int)GeneShape);
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
		if (GeneType != EGeneType.EmptyValue)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)GeneType);
		}
		if (GeneShape != EGeneShape.EmptyValue)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)GeneShape);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(BagGeneComposeCalcReq other)
	{
		if (other != null)
		{
			if (other.GeneType != EGeneType.EmptyValue)
			{
				GeneType = other.GeneType;
			}
			if (other.GeneShape != EGeneShape.EmptyValue)
			{
				GeneShape = other.GeneShape;
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
				GeneType = (EGeneType)((ParseContext)(ref input)).ReadEnum();
				break;
			case 16u:
				GeneShape = (EGeneShape)((ParseContext)(ref input)).ReadEnum();
				break;
			}
		}
	}
}
