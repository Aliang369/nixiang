using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class AccountAwardInfo : IMessage<AccountAwardInfo>, IMessage, IEquatable<AccountAwardInfo>, IDeepCloneable<AccountAwardInfo>, IBufferMessage
{
	private static readonly MessageParser<AccountAwardInfo> _parser = new MessageParser<AccountAwardInfo>((Func<AccountAwardInfo>)(() => new AccountAwardInfo()));

	private UnknownFieldSet _unknownFields;

	public const int BindStateFieldNumber = 1;

	private EBoolState bindState_;

	public const int RechargeRebateFieldNumber = 2;

	private RechargeRebate rechargeRebate_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<AccountAwardInfo> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeAccountAwardInfoReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public EBoolState BindState
	{
		get
		{
			return bindState_;
		}
		set
		{
			bindState_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RechargeRebate RechargeRebate
	{
		get
		{
			return rechargeRebate_;
		}
		set
		{
			rechargeRebate_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public AccountAwardInfo()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public AccountAwardInfo(AccountAwardInfo other)
		: this()
	{
		bindState_ = other.bindState_;
		rechargeRebate_ = ((other.rechargeRebate_ != null) ? other.rechargeRebate_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public AccountAwardInfo Clone()
	{
		return new AccountAwardInfo(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as AccountAwardInfo);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(AccountAwardInfo other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (BindState != other.BindState)
		{
			return false;
		}
		if (!object.Equals(RechargeRebate, other.RechargeRebate))
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
		if (BindState != EBoolState.False)
		{
			num ^= BindState.GetHashCode();
		}
		if (rechargeRebate_ != null)
		{
			num ^= RechargeRebate.GetHashCode();
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
		if (BindState != EBoolState.False)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteEnum((int)BindState);
		}
		if (rechargeRebate_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)18);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)RechargeRebate);
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
		if (BindState != EBoolState.False)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)BindState);
		}
		if (rechargeRebate_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)RechargeRebate);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(AccountAwardInfo other)
	{
		if (other == null)
		{
			return;
		}
		if (other.BindState != EBoolState.False)
		{
			BindState = other.BindState;
		}
		if (other.rechargeRebate_ != null)
		{
			if (rechargeRebate_ == null)
			{
				RechargeRebate = new RechargeRebate();
			}
			RechargeRebate.MergeFrom(other.RechargeRebate);
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
				BindState = (EBoolState)((ParseContext)(ref input)).ReadEnum();
				break;
			case 18u:
				if (rechargeRebate_ == null)
				{
					RechargeRebate = new RechargeRebate();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)RechargeRebate);
				break;
			}
		}
	}
}
