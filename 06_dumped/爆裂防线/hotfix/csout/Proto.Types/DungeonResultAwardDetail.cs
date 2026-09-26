using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class DungeonResultAwardDetail : IMessage<DungeonResultAwardDetail>, IMessage, IEquatable<DungeonResultAwardDetail>, IDeepCloneable<DungeonResultAwardDetail>, IBufferMessage
{
	private static readonly MessageParser<DungeonResultAwardDetail> _parser = new MessageParser<DungeonResultAwardDetail>((Func<DungeonResultAwardDetail>)(() => new DungeonResultAwardDetail()));

	private UnknownFieldSet _unknownFields;

	public const int StateFieldNumber = 1;

	private EBoolState state_;

	public const int AwardFieldNumber = 2;

	private AwardDetail award_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<DungeonResultAwardDetail> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeDungeonResultAwardDetailReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

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
	public DungeonResultAwardDetail()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public DungeonResultAwardDetail(DungeonResultAwardDetail other)
		: this()
	{
		state_ = other.state_;
		award_ = ((other.award_ != null) ? other.award_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public DungeonResultAwardDetail Clone()
	{
		return new DungeonResultAwardDetail(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as DungeonResultAwardDetail);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(DungeonResultAwardDetail other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
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
		if (State != EBoolState.False)
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
		if (State != EBoolState.False)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteEnum((int)State);
		}
		if (award_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)18);
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
		if (State != EBoolState.False)
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
	public void MergeFrom(DungeonResultAwardDetail other)
	{
		if (other == null)
		{
			return;
		}
		if (other.State != EBoolState.False)
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
			case 8u:
				State = (EBoolState)((ParseContext)(ref input)).ReadEnum();
				break;
			case 18u:
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
