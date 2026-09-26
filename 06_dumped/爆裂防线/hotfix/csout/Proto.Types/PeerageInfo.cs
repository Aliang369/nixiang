using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class PeerageInfo : IMessage<PeerageInfo>, IMessage, IEquatable<PeerageInfo>, IDeepCloneable<PeerageInfo>, IBufferMessage
{
	private static readonly MessageParser<PeerageInfo> _parser = new MessageParser<PeerageInfo>((Func<PeerageInfo>)(() => new PeerageInfo()));

	private UnknownFieldSet _unknownFields;

	public const int PeerageIdFieldNumber = 1;

	private int peerageId_;

	public const int TaskListFieldNumber = 2;

	private static readonly FieldCodec<TaskDetail> _repeated_taskList_codec = FieldCodec.ForMessage<TaskDetail>(18u, TaskDetail.Parser);

	private readonly RepeatedField<TaskDetail> taskList_ = new RepeatedField<TaskDetail>();

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<PeerageInfo> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypePeerageInfoReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int PeerageId
	{
		get
		{
			return peerageId_;
		}
		set
		{
			peerageId_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<TaskDetail> TaskList => taskList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public PeerageInfo()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public PeerageInfo(PeerageInfo other)
		: this()
	{
		peerageId_ = other.peerageId_;
		taskList_ = other.taskList_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public PeerageInfo Clone()
	{
		return new PeerageInfo(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as PeerageInfo);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(PeerageInfo other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (PeerageId != other.PeerageId)
		{
			return false;
		}
		if (!taskList_.Equals(other.taskList_))
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
		if (PeerageId != 0)
		{
			num ^= PeerageId.GetHashCode();
		}
		num ^= ((object)taskList_).GetHashCode();
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
		if (PeerageId != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteInt32(PeerageId);
		}
		taskList_.WriteTo(ref output, _repeated_taskList_codec);
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
		if (PeerageId != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(PeerageId);
		}
		num += taskList_.CalculateSize(_repeated_taskList_codec);
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(PeerageInfo other)
	{
		if (other != null)
		{
			if (other.PeerageId != 0)
			{
				PeerageId = other.PeerageId;
			}
			taskList_.Add((IEnumerable<TaskDetail>)other.taskList_);
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
				PeerageId = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 18u:
				taskList_.AddEntriesFrom(ref input, _repeated_taskList_codec);
				break;
			}
		}
	}
}
