using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class FreeGiftDetail : IMessage<FreeGiftDetail>, IMessage, IEquatable<FreeGiftDetail>, IDeepCloneable<FreeGiftDetail>, IBufferMessage
{
	private static readonly MessageParser<FreeGiftDetail> _parser = new MessageParser<FreeGiftDetail>((Func<FreeGiftDetail>)(() => new FreeGiftDetail()));

	private UnknownFieldSet _unknownFields;

	public const int FreeTypeFieldNumber = 1;

	private int freeType_;

	public const int RefreshTypeFieldNumber = 2;

	private ERefreshType refreshType_;

	public const int AwardFieldNumber = 3;

	private AwardDetail award_;

	public const int StateFieldNumber = 4;

	private EBoolState state_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<FreeGiftDetail> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeFreeGiftDetailReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int FreeType
	{
		get
		{
			return freeType_;
		}
		set
		{
			freeType_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ERefreshType RefreshType
	{
		get
		{
			return refreshType_;
		}
		set
		{
			refreshType_ = value;
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
	public EBoolState State
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
	public FreeGiftDetail()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public FreeGiftDetail(FreeGiftDetail other)
		: this()
	{
		freeType_ = other.freeType_;
		refreshType_ = other.refreshType_;
		award_ = ((other.award_ != null) ? other.award_.Clone() : null);
		state_ = other.state_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public FreeGiftDetail Clone()
	{
		return new FreeGiftDetail(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as FreeGiftDetail);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(FreeGiftDetail other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (FreeType != other.FreeType)
		{
			return false;
		}
		if (RefreshType != other.RefreshType)
		{
			return false;
		}
		if (!object.Equals(Award, other.Award))
		{
			return false;
		}
		if (State != other.State)
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
		if (FreeType != 0)
		{
			num ^= FreeType.GetHashCode();
		}
		if (RefreshType != ERefreshType.EmptyValue)
		{
			num ^= RefreshType.GetHashCode();
		}
		if (award_ != null)
		{
			num ^= Award.GetHashCode();
		}
		if (State != EBoolState.False)
		{
			num ^= State.GetHashCode();
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
		if (FreeType != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteInt32(FreeType);
		}
		if (RefreshType != ERefreshType.EmptyValue)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteEnum((int)RefreshType);
		}
		if (award_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)26);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)Award);
		}
		if (State != EBoolState.False)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)32);
			((WriteContext)(ref output)).WriteEnum((int)State);
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
		if (FreeType != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(FreeType);
		}
		if (RefreshType != ERefreshType.EmptyValue)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)RefreshType);
		}
		if (award_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)Award);
		}
		if (State != EBoolState.False)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)State);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(FreeGiftDetail other)
	{
		if (other == null)
		{
			return;
		}
		if (other.FreeType != 0)
		{
			FreeType = other.FreeType;
		}
		if (other.RefreshType != ERefreshType.EmptyValue)
		{
			RefreshType = other.RefreshType;
		}
		if (other.award_ != null)
		{
			if (award_ == null)
			{
				Award = new AwardDetail();
			}
			Award.MergeFrom(other.Award);
		}
		if (other.State != EBoolState.False)
		{
			State = other.State;
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
			case 8u:
				FreeType = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 16u:
				RefreshType = (ERefreshType)((ParseContext)(ref input)).ReadEnum();
				break;
			case 26u:
				if (award_ == null)
				{
					Award = new AwardDetail();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)Award);
				break;
			case 32u:
				State = (EBoolState)((ParseContext)(ref input)).ReadEnum();
				break;
			}
		}
	}
}
