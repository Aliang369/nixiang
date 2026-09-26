using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class AbnormalClientReportReq : IMessage<AbnormalClientReportReq>, IMessage, IEquatable<AbnormalClientReportReq>, IDeepCloneable<AbnormalClientReportReq>, IBufferMessage
{
	private static readonly MessageParser<AbnormalClientReportReq> _parser = new MessageParser<AbnormalClientReportReq>((Func<AbnormalClientReportReq>)(() => new AbnormalClientReportReq()));

	private UnknownFieldSet _unknownFields;

	public const int FuncTypeFieldNumber = 1;

	private EFuncType funcType_;

	public const int OriginValueFieldNumber = 2;

	private float originValue_;

	public const int ModifyValueFieldNumber = 3;

	private float modifyValue_;

	public const int ClientVersionFieldNumber = 4;

	private string clientVersion_ = "";

	public const int ReportDescFieldNumber = 5;

	private string reportDesc_ = "";

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<AbnormalClientReportReq> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeAbnormalClientReportReqReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public EFuncType FuncType
	{
		get
		{
			return funcType_;
		}
		set
		{
			funcType_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public float OriginValue
	{
		get
		{
			return originValue_;
		}
		set
		{
			originValue_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public float ModifyValue
	{
		get
		{
			return modifyValue_;
		}
		set
		{
			modifyValue_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public string ClientVersion
	{
		get
		{
			return clientVersion_;
		}
		set
		{
			clientVersion_ = ProtoPreconditions.CheckNotNull<string>(value, "value");
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public string ReportDesc
	{
		get
		{
			return reportDesc_;
		}
		set
		{
			reportDesc_ = ProtoPreconditions.CheckNotNull<string>(value, "value");
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public AbnormalClientReportReq()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public AbnormalClientReportReq(AbnormalClientReportReq other)
		: this()
	{
		funcType_ = other.funcType_;
		originValue_ = other.originValue_;
		modifyValue_ = other.modifyValue_;
		clientVersion_ = other.clientVersion_;
		reportDesc_ = other.reportDesc_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public AbnormalClientReportReq Clone()
	{
		return new AbnormalClientReportReq(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as AbnormalClientReportReq);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(AbnormalClientReportReq other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (FuncType != other.FuncType)
		{
			return false;
		}
		if (!ProtobufEqualityComparers.BitwiseSingleEqualityComparer.Equals(OriginValue, other.OriginValue))
		{
			return false;
		}
		if (!ProtobufEqualityComparers.BitwiseSingleEqualityComparer.Equals(ModifyValue, other.ModifyValue))
		{
			return false;
		}
		if (ClientVersion != other.ClientVersion)
		{
			return false;
		}
		if (ReportDesc != other.ReportDesc)
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
		if (FuncType != EFuncType.EmptyValue)
		{
			num ^= FuncType.GetHashCode();
		}
		if (OriginValue != 0f)
		{
			num ^= ProtobufEqualityComparers.BitwiseSingleEqualityComparer.GetHashCode(OriginValue);
		}
		if (ModifyValue != 0f)
		{
			num ^= ProtobufEqualityComparers.BitwiseSingleEqualityComparer.GetHashCode(ModifyValue);
		}
		if (ClientVersion.Length != 0)
		{
			num ^= ClientVersion.GetHashCode();
		}
		if (ReportDesc.Length != 0)
		{
			num ^= ReportDesc.GetHashCode();
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
		if (FuncType != EFuncType.EmptyValue)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteEnum((int)FuncType);
		}
		if (OriginValue != 0f)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)21);
			((WriteContext)(ref output)).WriteFloat(OriginValue);
		}
		if (ModifyValue != 0f)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)29);
			((WriteContext)(ref output)).WriteFloat(ModifyValue);
		}
		if (ClientVersion.Length != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)34);
			((WriteContext)(ref output)).WriteString(ClientVersion);
		}
		if (ReportDesc.Length != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)42);
			((WriteContext)(ref output)).WriteString(ReportDesc);
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
		if (FuncType != EFuncType.EmptyValue)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)FuncType);
		}
		if (OriginValue != 0f)
		{
			num += 5;
		}
		if (ModifyValue != 0f)
		{
			num += 5;
		}
		if (ClientVersion.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(ClientVersion);
		}
		if (ReportDesc.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(ReportDesc);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(AbnormalClientReportReq other)
	{
		if (other != null)
		{
			if (other.FuncType != EFuncType.EmptyValue)
			{
				FuncType = other.FuncType;
			}
			if (other.OriginValue != 0f)
			{
				OriginValue = other.OriginValue;
			}
			if (other.ModifyValue != 0f)
			{
				ModifyValue = other.ModifyValue;
			}
			if (other.ClientVersion.Length != 0)
			{
				ClientVersion = other.ClientVersion;
			}
			if (other.ReportDesc.Length != 0)
			{
				ReportDesc = other.ReportDesc;
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
				FuncType = (EFuncType)((ParseContext)(ref input)).ReadEnum();
				break;
			case 21u:
				OriginValue = ((ParseContext)(ref input)).ReadFloat();
				break;
			case 29u:
				ModifyValue = ((ParseContext)(ref input)).ReadFloat();
				break;
			case 34u:
				ClientVersion = ((ParseContext)(ref input)).ReadString();
				break;
			case 42u:
				ReportDesc = ((ParseContext)(ref input)).ReadString();
				break;
			}
		}
	}
}
