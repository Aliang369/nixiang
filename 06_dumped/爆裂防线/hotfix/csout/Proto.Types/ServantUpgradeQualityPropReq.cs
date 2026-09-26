using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class ServantUpgradeQualityPropReq : IMessage<ServantUpgradeQualityPropReq>, IMessage, IEquatable<ServantUpgradeQualityPropReq>, IDeepCloneable<ServantUpgradeQualityPropReq>, IBufferMessage
{
	private static readonly MessageParser<ServantUpgradeQualityPropReq> _parser = new MessageParser<ServantUpgradeQualityPropReq>((Func<ServantUpgradeQualityPropReq>)(() => new ServantUpgradeQualityPropReq()));

	private UnknownFieldSet _unknownFields;

	public const int ServantIdFieldNumber = 1;

	private string servantId_ = "";

	public const int PropCountFieldNumber = 2;

	private int propCount_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<ServantUpgradeQualityPropReq> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeServantUpgradeQualityPropReqReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public string ServantId
	{
		get
		{
			return servantId_;
		}
		set
		{
			servantId_ = ProtoPreconditions.CheckNotNull<string>(value, "value");
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int PropCount
	{
		get
		{
			return propCount_;
		}
		set
		{
			propCount_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ServantUpgradeQualityPropReq()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ServantUpgradeQualityPropReq(ServantUpgradeQualityPropReq other)
		: this()
	{
		servantId_ = other.servantId_;
		propCount_ = other.propCount_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ServantUpgradeQualityPropReq Clone()
	{
		return new ServantUpgradeQualityPropReq(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as ServantUpgradeQualityPropReq);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(ServantUpgradeQualityPropReq other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (ServantId != other.ServantId)
		{
			return false;
		}
		if (PropCount != other.PropCount)
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
		if (ServantId.Length != 0)
		{
			num ^= ServantId.GetHashCode();
		}
		if (PropCount != 0)
		{
			num ^= PropCount.GetHashCode();
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
		if (ServantId.Length != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)10);
			((WriteContext)(ref output)).WriteString(ServantId);
		}
		if (PropCount != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteInt32(PropCount);
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
		if (ServantId.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(ServantId);
		}
		if (PropCount != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(PropCount);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(ServantUpgradeQualityPropReq other)
	{
		if (other != null)
		{
			if (other.ServantId.Length != 0)
			{
				ServantId = other.ServantId;
			}
			if (other.PropCount != 0)
			{
				PropCount = other.PropCount;
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
			case 10u:
				ServantId = ((ParseContext)(ref input)).ReadString();
				break;
			case 16u:
				PropCount = ((ParseContext)(ref input)).ReadInt32();
				break;
			}
		}
	}
}
