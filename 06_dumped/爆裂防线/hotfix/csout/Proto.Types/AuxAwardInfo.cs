using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class AuxAwardInfo : IMessage<AuxAwardInfo>, IMessage, IEquatable<AuxAwardInfo>, IDeepCloneable<AuxAwardInfo>, IBufferMessage
{
	private static readonly MessageParser<AuxAwardInfo> _parser = new MessageParser<AuxAwardInfo>((Func<AuxAwardInfo>)(() => new AuxAwardInfo()));

	private UnknownFieldSet _unknownFields;

	public const int AccountAwardInfoFieldNumber = 1;

	private AccountAwardInfo accountAwardInfo_;

	public const int FuncListFieldNumber = 2;

	private static readonly FieldCodec<int> _repeated_funcList_codec = FieldCodec.ForInt32(18u);

	private readonly RepeatedField<int> funcList_ = new RepeatedField<int>();

	public const int RankListFieldNumber = 3;

	private static readonly FieldCodec<RankFirstReachAwardDetail> _repeated_rankList_codec = FieldCodec.ForMessage<RankFirstReachAwardDetail>(26u, RankFirstReachAwardDetail.Parser);

	private readonly RepeatedField<RankFirstReachAwardDetail> rankList_ = new RepeatedField<RankFirstReachAwardDetail>();

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<AuxAwardInfo> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeAuxAwardInfoReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public AccountAwardInfo AccountAwardInfo
	{
		get
		{
			return accountAwardInfo_;
		}
		set
		{
			accountAwardInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<int> FuncList => funcList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<RankFirstReachAwardDetail> RankList => rankList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public AuxAwardInfo()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public AuxAwardInfo(AuxAwardInfo other)
		: this()
	{
		accountAwardInfo_ = ((other.accountAwardInfo_ != null) ? other.accountAwardInfo_.Clone() : null);
		funcList_ = other.funcList_.Clone();
		rankList_ = other.rankList_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public AuxAwardInfo Clone()
	{
		return new AuxAwardInfo(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as AuxAwardInfo);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(AuxAwardInfo other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!object.Equals(AccountAwardInfo, other.AccountAwardInfo))
		{
			return false;
		}
		if (!funcList_.Equals(other.funcList_))
		{
			return false;
		}
		if (!rankList_.Equals(other.rankList_))
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
		if (accountAwardInfo_ != null)
		{
			num ^= AccountAwardInfo.GetHashCode();
		}
		num ^= ((object)funcList_).GetHashCode();
		num ^= ((object)rankList_).GetHashCode();
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
		if (accountAwardInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)10);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)AccountAwardInfo);
		}
		funcList_.WriteTo(ref output, _repeated_funcList_codec);
		rankList_.WriteTo(ref output, _repeated_rankList_codec);
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
		if (accountAwardInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)AccountAwardInfo);
		}
		num += funcList_.CalculateSize(_repeated_funcList_codec);
		num += rankList_.CalculateSize(_repeated_rankList_codec);
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(AuxAwardInfo other)
	{
		if (other == null)
		{
			return;
		}
		if (other.accountAwardInfo_ != null)
		{
			if (accountAwardInfo_ == null)
			{
				AccountAwardInfo = new AccountAwardInfo();
			}
			AccountAwardInfo.MergeFrom(other.AccountAwardInfo);
		}
		funcList_.Add((IEnumerable<int>)other.funcList_);
		rankList_.Add((IEnumerable<RankFirstReachAwardDetail>)other.rankList_);
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
				if (accountAwardInfo_ == null)
				{
					AccountAwardInfo = new AccountAwardInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)AccountAwardInfo);
				break;
			case 16u:
			case 18u:
				funcList_.AddEntriesFrom(ref input, _repeated_funcList_codec);
				break;
			case 26u:
				rankList_.AddEntriesFrom(ref input, _repeated_rankList_codec);
				break;
			}
		}
	}
}
