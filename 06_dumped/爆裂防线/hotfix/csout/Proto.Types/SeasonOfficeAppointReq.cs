using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class SeasonOfficeAppointReq : IMessage<SeasonOfficeAppointReq>, IMessage, IEquatable<SeasonOfficeAppointReq>, IDeepCloneable<SeasonOfficeAppointReq>, IBufferMessage
{
	private static readonly MessageParser<SeasonOfficeAppointReq> _parser = new MessageParser<SeasonOfficeAppointReq>((Func<SeasonOfficeAppointReq>)(() => new SeasonOfficeAppointReq()));

	private UnknownFieldSet _unknownFields;

	public const int TargetRoleIdFieldNumber = 1;

	private long targetRoleId_;

	public const int OfficeFieldNumber = 2;

	private ESeasonOffice office_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<SeasonOfficeAppointReq> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeSeasonOfficeAppointReqReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public long TargetRoleId
	{
		get
		{
			return targetRoleId_;
		}
		set
		{
			targetRoleId_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ESeasonOffice Office
	{
		get
		{
			return office_;
		}
		set
		{
			office_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SeasonOfficeAppointReq()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SeasonOfficeAppointReq(SeasonOfficeAppointReq other)
		: this()
	{
		targetRoleId_ = other.targetRoleId_;
		office_ = other.office_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public SeasonOfficeAppointReq Clone()
	{
		return new SeasonOfficeAppointReq(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as SeasonOfficeAppointReq);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(SeasonOfficeAppointReq other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (TargetRoleId != other.TargetRoleId)
		{
			return false;
		}
		if (Office != other.Office)
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
		if (TargetRoleId != 0L)
		{
			num ^= TargetRoleId.GetHashCode();
		}
		if (Office != ESeasonOffice.EmptyValue)
		{
			num ^= Office.GetHashCode();
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
		if (TargetRoleId != 0L)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteInt64(TargetRoleId);
		}
		if (Office != ESeasonOffice.EmptyValue)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteEnum((int)Office);
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
		if (TargetRoleId != 0L)
		{
			num += 1 + CodedOutputStream.ComputeInt64Size(TargetRoleId);
		}
		if (Office != ESeasonOffice.EmptyValue)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)Office);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(SeasonOfficeAppointReq other)
	{
		if (other != null)
		{
			if (other.TargetRoleId != 0L)
			{
				TargetRoleId = other.TargetRoleId;
			}
			if (other.Office != ESeasonOffice.EmptyValue)
			{
				Office = other.Office;
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
				TargetRoleId = ((ParseContext)(ref input)).ReadInt64();
				break;
			case 16u:
				Office = (ESeasonOffice)((ParseContext)(ref input)).ReadEnum();
				break;
			}
		}
	}
}
