using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class ServantUpgradeQualityReq : IMessage<ServantUpgradeQualityReq>, IMessage, IEquatable<ServantUpgradeQualityReq>, IDeepCloneable<ServantUpgradeQualityReq>, IBufferMessage
{
	private static readonly MessageParser<ServantUpgradeQualityReq> _parser = new MessageParser<ServantUpgradeQualityReq>((Func<ServantUpgradeQualityReq>)(() => new ServantUpgradeQualityReq()));

	private UnknownFieldSet _unknownFields;

	public const int ServantIdFieldNumber = 1;

	private string servantId_ = "";

	public const int CostServantIdFieldNumber = 2;

	private string costServantId_ = "";

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<ServantUpgradeQualityReq> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeServantUpgradeQualityReqReflection.Descriptor.MessageTypes[0];

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
	public string CostServantId
	{
		get
		{
			return costServantId_;
		}
		set
		{
			costServantId_ = ProtoPreconditions.CheckNotNull<string>(value, "value");
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ServantUpgradeQualityReq()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ServantUpgradeQualityReq(ServantUpgradeQualityReq other)
		: this()
	{
		servantId_ = other.servantId_;
		costServantId_ = other.costServantId_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ServantUpgradeQualityReq Clone()
	{
		return new ServantUpgradeQualityReq(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as ServantUpgradeQualityReq);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(ServantUpgradeQualityReq other)
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
		if (CostServantId != other.CostServantId)
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
		if (CostServantId.Length != 0)
		{
			num ^= CostServantId.GetHashCode();
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
		if (CostServantId.Length != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)18);
			((WriteContext)(ref output)).WriteString(CostServantId);
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
		if (CostServantId.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(CostServantId);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(ServantUpgradeQualityReq other)
	{
		if (other != null)
		{
			if (other.ServantId.Length != 0)
			{
				ServantId = other.ServantId;
			}
			if (other.CostServantId.Length != 0)
			{
				CostServantId = other.CostServantId;
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
			case 18u:
				CostServantId = ((ParseContext)(ref input)).ReadString();
				break;
			}
		}
	}
}
