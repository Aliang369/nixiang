using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class ServantSubSkillFlushReq : IMessage<ServantSubSkillFlushReq>, IMessage, IEquatable<ServantSubSkillFlushReq>, IDeepCloneable<ServantSubSkillFlushReq>, IBufferMessage
{
	private static readonly MessageParser<ServantSubSkillFlushReq> _parser = new MessageParser<ServantSubSkillFlushReq>((Func<ServantSubSkillFlushReq>)(() => new ServantSubSkillFlushReq()));

	private UnknownFieldSet _unknownFields;

	public const int ServantIdFieldNumber = 1;

	private string servantId_ = "";

	public const int LockListFieldNumber = 2;

	private static readonly FieldCodec<int> _repeated_lockList_codec = FieldCodec.ForInt32(18u);

	private readonly RepeatedField<int> lockList_ = new RepeatedField<int>();

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<ServantSubSkillFlushReq> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeServantSubSkillFlushReqReflection.Descriptor.MessageTypes[0];

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
	public RepeatedField<int> LockList => lockList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ServantSubSkillFlushReq()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ServantSubSkillFlushReq(ServantSubSkillFlushReq other)
		: this()
	{
		servantId_ = other.servantId_;
		lockList_ = other.lockList_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ServantSubSkillFlushReq Clone()
	{
		return new ServantSubSkillFlushReq(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as ServantSubSkillFlushReq);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(ServantSubSkillFlushReq other)
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
		if (!lockList_.Equals(other.lockList_))
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
		num ^= ((object)lockList_).GetHashCode();
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
		lockList_.WriteTo(ref output, _repeated_lockList_codec);
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
		num += lockList_.CalculateSize(_repeated_lockList_codec);
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(ServantSubSkillFlushReq other)
	{
		if (other != null)
		{
			if (other.ServantId.Length != 0)
			{
				ServantId = other.ServantId;
			}
			lockList_.Add((IEnumerable<int>)other.lockList_);
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
			case 18u:
				lockList_.AddEntriesFrom(ref input, _repeated_lockList_codec);
				break;
			}
		}
	}
}
