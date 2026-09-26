using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class CipherBoxTargetDetail : IMessage<CipherBoxTargetDetail>, IMessage, IEquatable<CipherBoxTargetDetail>, IDeepCloneable<CipherBoxTargetDetail>, IBufferMessage
{
	private static readonly MessageParser<CipherBoxTargetDetail> _parser = new MessageParser<CipherBoxTargetDetail>((Func<CipherBoxTargetDetail>)(() => new CipherBoxTargetDetail()));

	private UnknownFieldSet _unknownFields;

	public const int CipherFieldNumber = 1;

	private string cipher_ = "";

	public const int StateFieldNumber = 2;

	private ECommonState state_;

	public const int AwardFieldNumber = 3;

	private AwardDetail award_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<CipherBoxTargetDetail> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeCipherBoxTargetDetailReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public string Cipher
	{
		get
		{
			return cipher_;
		}
		set
		{
			cipher_ = ProtoPreconditions.CheckNotNull<string>(value, "value");
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ECommonState State
	{
		get
		{
			return state_;
		}
		set
		{
			state_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public AwardDetail Award
	{
		get
		{
			return award_;
		}
		set
		{
			award_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public CipherBoxTargetDetail()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public CipherBoxTargetDetail(CipherBoxTargetDetail other)
		: this()
	{
		cipher_ = other.cipher_;
		state_ = other.state_;
		award_ = ((other.award_ != null) ? other.award_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public CipherBoxTargetDetail Clone()
	{
		return new CipherBoxTargetDetail(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as CipherBoxTargetDetail);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(CipherBoxTargetDetail other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (Cipher != other.Cipher)
		{
			return false;
		}
		if (State != other.State)
		{
			return false;
		}
		if (!object.Equals(Award, other.Award))
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
		if (Cipher.Length != 0)
		{
			num ^= Cipher.GetHashCode();
		}
		if (State != ECommonState.No)
		{
			num ^= State.GetHashCode();
		}
		if (award_ != null)
		{
			num ^= Award.GetHashCode();
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
		if (Cipher.Length != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)10);
			((WriteContext)(ref output)).WriteString(Cipher);
		}
		if (State != ECommonState.No)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteEnum((int)State);
		}
		if (award_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)26);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)Award);
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
		if (Cipher.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(Cipher);
		}
		if (State != ECommonState.No)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)State);
		}
		if (award_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)Award);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(CipherBoxTargetDetail other)
	{
		if (other == null)
		{
			return;
		}
		if (other.Cipher.Length != 0)
		{
			Cipher = other.Cipher;
		}
		if (other.State != ECommonState.No)
		{
			State = other.State;
		}
		if (other.award_ != null)
		{
			if (award_ == null)
			{
				Award = new AwardDetail();
			}
			Award.MergeFrom(other.Award);
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
				Cipher = ((ParseContext)(ref input)).ReadString();
				break;
			case 16u:
				State = (ECommonState)((ParseContext)(ref input)).ReadEnum();
				break;
			case 26u:
				if (award_ == null)
				{
					Award = new AwardDetail();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)Award);
				break;
			}
		}
	}
}
